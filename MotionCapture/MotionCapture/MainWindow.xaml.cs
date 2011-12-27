/////////////////////////////////////////////////////////////////////////
//
// Copyright (c) Microsoft Corporation. All rights reserved.
// This code is licensed under the terms of the Microsoft Kinect for
// Windows SDK (Beta) License Agreement:
// http://kinectforwindows.org/KinectSDK-ToU
//
/////////////////////////////////////////////////////////////////////////

using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Coding4Fun.Kinect.Wpf;
using Microsoft.Research.Kinect.Nui;
using System.Drawing;
using System.Drawing.Imaging;
using MotionChallenge;

namespace WorkingWithDepthData
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        //Kinect Runtime
        Runtime nui;

        [System.Runtime.InteropServices.DllImport("gdi32.dll")]
        public static extern bool DeleteObject(IntPtr hObject);

		/**
		 * Fenêtre chargée : initialiser l'application.
		 */
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            SetupKinect();
        }

        /**
		 * Initialiser le Kinect et son orientation
		 */
        private void SetupKinect()
        {
			//Vérifier qu'un Kinect est connecté.
            if (Runtime.Kinects.Count == 0)
            {
                this.Title = "No Kinect connected";
            }
            else
            {
                //Récupérer le premier Kinect (en théorie, le SDK permet d'en connecter plusieurs. Ici, on n'utilise pas cette fonctionnalité)
                nui = Runtime.Kinects[0];

                //Indiquer les options qui nous seront utiles. Dans notre cas, la profondeur
				//TODO : virer skeletalTracking ?
                nui.Initialize(RuntimeOptions.UseDepthAndPlayerIndex | RuntimeOptions.UseSkeletalTracking);

                //Enregistrer une fonction écoutante pour recevoir les données de Kinect lorsqu'une nouvelle DepthFrame est prête.
                nui.DepthFrameReady += new EventHandler<ImageFrameReadyEventArgs>(nui_DepthFrameReady);

                //Ouvrir le flux Depth à la résolution maximale (320*240)
                nui.DepthStream.Open(ImageStreamType.Depth, 2, ImageResolution.Resolution320x240,
                    ImageType.DepthAndPlayerIndex);

				//Placer le Kinect à un angle constant quel que soit le support ou l'angle de départ, afin de toujours avoir la même fenêtre de jeu.
                try
                {
                    nui.NuiCamera.ElevationAngle = 6;
                }
                catch (InvalidOperationException ex)
                {
					//Il arrive régulièrement que Kinect soit incapable d'actionner les moteurs angulaires.
                    MessageBox.Show("Impossible de régler l'orientation du Kinect. Erreur : " + ex.Message);
                }
            }
        }

        /**
		 * Appelé lorsque des nouvelles données de profondeur sont disponibles
		 */
        void nui_DepthFrameReady(object sender, ImageFrameReadyEventArgs e)
        {
            //Le tableau de base (dans e.ImageFrame) contient des données encodées selon un format spécial.
            //La fonction GenerateColoredBytes s'occupe de créer un tableau de pixels directement accessibles.
            byte[] ColoredBytes = GenerateColoredBytes(e.ImageFrame);

            //Convertir les données dans un format affichable à l'écran par C#.
            PlanarImage image = e.ImageFrame.Image;
            BitmapSource bmps = BitmapSource.Create(image.Width, image.Height, 96, 96, PixelFormats.Bgra32, null,
                ColoredBytes, image.Width * PixelFormats.Bgra32.BitsPerPixel / 8);

			//Afficher les données récupérées sur le composant image preview.
			//À noter : l'image contient des pixels blancs et des pixels transparents. On ne devrait normalement rien voir...
			//pour pallier à ce défaut, une toile noire est affichée sous le composant image, et visible à travers les pixels transparents.
            preview.Source = bmps;
        }

        //TODO : à supprimer ?
        void quickSetPixel(byte[] datas, int counter, byte value = 150)
        {
            if (counter < 0 || counter + 3 > datas.Length)
                return;

            datas[counter] = datas[counter + 1] = datas[counter + 2] = datas[counter + 3] = value;
        }

       /**
		* Génère, à partir des données brutes fournies par Kinect, un tableau de bit utilisables.
		* Le format de donnée originale de Kinect est complexe et dépend de nombreux paramètres.
		* Cette fonction encapsule et rend abstrait la conversion.
		*
		* @see http://channel9.msdn.com/Series/KinectSDKQuickstarts/Working-with-Depth-Data
		*/
        private byte[] GenerateColoredBytes(ImageFrame imageFrame)
        {
            int height = imageFrame.Image.Height;
            int width = imageFrame.Image.Width;

            //Depth data for each pixel
            Byte[] depthData = imageFrame.Image.Bits;

            //colorFrame contains color information for all pixels in image
            //Height x Width x 4 (Red, Green, Blue, empty byte)
            Byte[] colorFrame = new byte[imageFrame.Image.Height * imageFrame.Image.Width * 4];

            //Bgr32  - Blue, Green, Red, empty byte
            //Bgra32 - Blue, Green, Red, transparency
            //You must set transparency for Bgra as .NET defaults a byte to 0 = fully transparent

            //hardcoded locations to Blue, Green, Red (BGR) index positions
            const int BlueIndex = 0;
            const int GreenIndex = 1;
            const int RedIndex = 2;
            const int AlphaIndex = 3;

            int depthIndex = 0;
            int distance = 0;
            int nbDistance = 1;

            for (var y = 0; y < height; y++)
            {
                var heightOffset = y * width;

                for (var x = 0; x < width; x++)
                {
                    var index = ((x + 0) + heightOffset) * 4;

                    //Par défaut, le pixel est blanc.
					//Pour rappel, le format d'image étant RGBA, chaque pixel contient quatre composantes qui doivent être définies.
                    colorFrame[index + BlueIndex] = 255;
                    colorFrame[index + GreenIndex] = 255;
                    colorFrame[index + RedIndex] = 255;
                    colorFrame[index + AlphaIndex] = 255;

                    //Si le pixel contient un joueur
                    if (GetPlayerIndex(depthData[depthIndex]) > 0)
                    {
						//L'afficher en blanc transparent.
                        colorFrame[index + BlueIndex] = 0;
                        colorFrame[index + GreenIndex] = 0;
                        colorFrame[index + RedIndex] = 0;
                        colorFrame[index + AlphaIndex] = 0;
                        distance += (int)(depthData[depthIndex] >> 3 | depthData[depthIndex + 1] << 5);
                        nbDistance++;
                    }
                    //jump two bytes at a time
                    depthIndex += 2;
                }
            }

            //On souhaite afficher l'écart par rapport à la distance idéale, afin que toutes les images soient prises à la même profondeur
            //(les deux progressbar en haut de l'application).
            //On va donc calculer la véritable distance en moyennant chacun des pixels
            distance = distance / nbDistance;
            int distanceTropLoin = Math.Max(0, Math.Min(400, distance - 2400));
            int distanceTropProche = Math.Max(0, Math.Min(400, 2400 - distance));

			//On règle la valeur des deux barres de progression par rapport à la distance idéale souhaitée
            if (distance != 0)
                distanceLoinProgress.Value = distanceTropLoin;
            else
                distanceLoinProgress.Value = distanceLoinProgress.Maximum;

            distanceProcheProgress.Value = distanceTropProche;

			//On définit par une quasi-règle de trois la couleur à utiliser
            int distanceCouleur = Math.Max(distanceTropProche, distanceTropLoin);
            System.Windows.Media.Color couleurProgress = new System.Windows.Media.Color();
            couleurProgress.ScR = (float)(distanceCouleur) / 100;
            couleurProgress.ScG = (float)(400 - distanceCouleur) / 800;
            couleurProgress.ScA = 1;
            distanceLoinProgress.Foreground = new SolidColorBrush(couleurProgress);
            distanceProcheProgress.Foreground = new SolidColorBrush(couleurProgress);

            return colorFrame;
        }

        //TODO : à mettre en inline ? Pas besoin d'en faire une fonction.
        private static int GetPlayerIndex(byte firstFrame)
        {
            //returns 0 = no player, 1 = 1st player, 2 = 2nd player...
            //bitwise & on firstFrame
            return (int)firstFrame & 7;
        }

        /**
		 * L'application ou l'utilisateur demande à fermer la fenêtre.
		 */
        private void Window_Unloaded(object sender, RoutedEventArgs e)
        {
            //Libérer les ressources utilisées
            nui.Uninitialize();
        }

        /**
		 * L'utilisateur clique sur le bouton permettant d'enregistrer l'image actuellement affichée sur le disque.
		 */
        private void saveImage_Click(object sender, RoutedEventArgs e)
        {
            //Définir un nom de fichier unique :
            string now = DateTime.Now.ToString().Replace('/', '-').Replace(':', ' ');

			//Appliquer un algorithme d'antialiasing pour lisser les imperfections des données fournies par Kinect.
			//Cet algorithme est long et ne peut donc malheureusement pas être affiché sur la sortie "temps réel".
            Bitmap antiAliasBmp = Util.AntiAliasing(Util.GetBitmap(preview.Source as BitmapSource));

			//Enregistrer l'image.
			//On ne peut pas utiliser de chemin absolu, il faut donc faire un hack tout moche à base de "..\.." pour placer l'image au bon endroit.
            String path = (nbPlayers.SelectedIndex + 1) + "j\\" + now + ".png";
            Util.GetBitmapSource(antiAliasBmp).Save("..\\..\\..\\..\\Walls\\" + path, Coding4Fun.Kinect.Wpf.ImageFormat.Png);

            pathLabel.Content = path;
        }
    }
}

