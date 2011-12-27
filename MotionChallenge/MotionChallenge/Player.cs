using System;
using System.Collections.Generic;
using Microsoft.Research.Kinect.Nui;
using OpenTK.Graphics.OpenGL;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using TexLib;

namespace MotionChallenge
{
	/**
	 * La classe Player représente les pixels de profondeur détectés par Kinect comme étant un joueur.
	 * Autrement dit, elle peut représenter un ou plusieurs humains.
	 *
	 * La logique de la classe est très proche de l'application MotionCapture, s'y reporter pour les détails.
	 */
    class Player
    {
        private int count;
        private Runtime nui;
        private BitmapSource bitmapSource;
        private Bitmap lastBitmap;

		/*
		 * OpenGl stocke toutes ses textures dans un tableau.
		 * Il faut conserver une référence afin de pouvoir libérer l'espace au fur et à mesure, une nouvelle texture étant allouée pour chaque frame.
		 */
        private int playerTextureId;

		/*
		 * Le joueur est affichée avec une certaine transparence par dessus la zone de jeu.
		 */
        private const int playerAlpha = 200;

        public Player(int playerCount)
        {
            count = playerCount;

			//Vérifier qu'un Kinect est connecté.
			if (Runtime.Kinects.Count == 0)
			{
				Console.WriteLine("WARNING: No Kinect connected!");
			}
			else
            {
				//Cette partie de code reprend exactement les fonctionnalités de MotionCapture.

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
		 * Nettoyer proprement en cas de RàZ.
		 */
        public void reset()
        {
            if (nui != null)
                nui.Uninitialize();
        }

        public int getPlayerCount()
        {
            return count;
        }

        /**
		 * Détermine le pourcentage de réussite du mur.
		 * Pour cela, deux valeurs sont retournées :
		 * - la première correspond au pourcentage du joueur à l'intérieur du trou : le plus est le mieux
		 * - la seconde correspond au pourcentage du joueur en dehors du trou : le moins est le mieux
		 * Le score idéal est donc de 100 / 0.
		 * Une unique valeur ne permet pas de juger de la précision. Par exemple, on peut avoir 100 sur la valeur intérieure en occupant tous les pixels de l'image.
		 *
		 * @note les deux pourcentages n'étant pas définis de la même façon, il est tout à fait normal que leur somme ne donne pas 100
		 */
        public int[] percentValues(Wall wall)
        {
            //Pourcentage in et pourcentage out
            int[] percent = new int[2];

            Bitmap bmp = lastBitmap;

            //Si le Kinect n'est pas connecté (ou n'a pas encore envoyé de données), il faut renvoyer 0 / 100 pour éviter le crash.
            if (bmp == null)
            {
                percent[0] = 0;
                percent[1] = 100;
                return percent;
            }

            //Récupérer les données de pixel afin de pouvoir les exploiter rapidement.
            BitmapData bmpd = bmp.LockBits(
                new Rectangle(0, 0, bmp.Width, bmp.Height),
                ImageLockMode.ReadOnly,
                bmp.PixelFormat
            );

            //Copier les données basiques et traduire / convertir pour un accès facile aux données de l'image
            IntPtr ptr = bmpd.Scan0;
            int bytes = Math.Abs(bmpd.Stride) * bmp.Height;
            byte[] playerValues = new byte[bytes];
            System.Runtime.InteropServices.Marshal.Copy(ptr, playerValues, 0, bytes);

            byte[] wallValues = wall.getCurrentWallByteArray();

			//Nombre de pixels correspondant au joueur à l'intérieur du trou
            int playerPixelsInTheWall = 0;

			//Nombre de pixels correspondant au joueur à l'extérieur du troi
            int playerPixelsOutTheWall = 1;

			//Nombre de pixels de trou dans le mur
            int emptyPixelsInTheWall = 0;

			//Parcourir chacun des pixels de l'image et remplir les variables nécessaires
            for (int counter = 3; counter < playerValues.Length; counter += 4)
            {
                byte wallValue = wallValues[counter];
                byte playerValue = playerValues[counter];

                if (wallValue == 0)
                {
                    emptyPixelsInTheWall++;

                    if (playerValue == playerAlpha)
                        playerPixelsInTheWall++;
                }
                else
                {
                    if (playerValue == playerAlpha)
                        playerPixelsOutTheWall++;
                }
            }

            //Pourcentage à l'intérieur du mur : nombre de pixels du joueur dans le trou / nombre de pixels de trou
            percent[0] = 100 * playerPixelsInTheWall / emptyPixelsInTheWall;
            //Pourcentage à l'extérieur du mur : nombre de pixels du joueurs hors du trou / (nombre total de pixels du joueur)
            percent[1] = 100 * playerPixelsOutTheWall / (playerPixelsOutTheWall + playerPixelsInTheWall);

            bmp.UnlockBits(bmpd);

            return percent;
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
            bitmapSource = BitmapSource.Create(image.Width, image.Height, 96, 96, PixelFormats.Bgra32, null,
                ColoredBytes, image.Width * PixelFormats.Bgra32.BitsPerPixel / 8);

            lastBitmap = Util.GetBitmap(bitmapSource);
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

            var depthIndex = 0;
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
                    colorFrame[index + AlphaIndex] = 0;

					//Si le pixel contient un joueur
                    if (GetPlayerIndex(depthData[depthIndex]) > 0)
                    {
                        colorFrame[index + BlueIndex] = 255;
                        colorFrame[index + GreenIndex] = 255;
                        colorFrame[index + RedIndex] = 255;
                        colorFrame[index + AlphaIndex] = playerAlpha;
                    }
                    //jump two bytes at a time
                    depthIndex += 2;
                }
            }

            return colorFrame;
        }

        //TODO : à mettre en inline ? Pas besoin d'en faire une fonction.
        private int GetPlayerIndex(byte firstFrame)
        {
            //returns 0 = no player, 1 = 1st player, 2 = 2nd player...
            //bitwise & on firstFrame
            return (int)firstFrame & 7;
        }

        /**
		 * Dessine le joueur en OpenGl.
		 * La position du mur est nécessaire afin de positionner correctement le joueur, qui suit le mur.
		 */
        public void draw(double wallPosition)
        {
            wallPosition -= 5;

            if (bitmapSource != null)
            {
                playerTextureId = TexUtil.CreateTextureFromBitmap(Util.GetBitmap(bitmapSource));

                //La silhouette du joueur
                GL.BindTexture(TextureTarget.Texture2D, playerTextureId);
                GL.Begin(BeginMode.Quads);
                    GL.TexCoord2(0, 0); GL.Vertex3(-Wall.wallWidth, wallPosition, Wall.wallHeight);
                    GL.TexCoord2(1, 0); GL.Vertex3(Wall.wallWidth, wallPosition, Wall.wallHeight);
                    GL.TexCoord2(1, 1); GL.Vertex3(Wall.wallWidth, wallPosition, 0);
                    GL.TexCoord2(0, 1); GL.Vertex3(-Wall.wallWidth, wallPosition, 0);

                    GL.Color3(System.Drawing.Color.White);
                GL.End();

                //Nettoyer la texture du joueur (pour éviter une fuite mémoire)
                GL.DeleteTexture(playerTextureId);
            }
        }
    }
}
