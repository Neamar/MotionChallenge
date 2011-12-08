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

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            SetupKinect();
        }

        private void SetupKinect()
        {
            if (Runtime.Kinects.Count == 0)
            {
                this.Title = "No Kinect connected";
            }
            else
            {
                //use first Kinect
                nui = Runtime.Kinects[0];

                //UseDepthAndPlayerIndex and UseSkeletalTracking
                nui.Initialize(RuntimeOptions.UseDepthAndPlayerIndex | RuntimeOptions.UseSkeletalTracking);

                //register for event
                nui.DepthFrameReady += new EventHandler<ImageFrameReadyEventArgs>(nui_DepthFrameReady);

                //DepthAndPlayerIndex ImageType
                nui.DepthStream.Open(ImageStreamType.Depth, 2, ImageResolution.Resolution320x240,
                    ImageType.DepthAndPlayerIndex);

                try
                {
                    nui.NuiCamera.ElevationAngle = 6;
                }
                catch (InvalidOperationException ex)
                {
                    MessageBox.Show("Impossible de régler l'orientation du Kinect. Erreur : " + ex.Message);
                }
            }
        }

        void nui_DepthFrameReady(object sender, ImageFrameReadyEventArgs e)
        {
            //Convert depth information for a pixel into color information
            byte[] ColoredBytes = GenerateColoredBytes(e.ImageFrame);

            //create an image based on returned colors
            PlanarImage image = e.ImageFrame.Image;
            BitmapSource bmps = BitmapSource.Create(image.Width, image.Height, 96, 96, PixelFormats.Bgra32, null,
                ColoredBytes, image.Width * PixelFormats.Bgra32.BitsPerPixel / 8);

            preview.Source = bmps;
        }

        void quickSetPixel(byte[] datas, int counter, byte value = 150)
        {
            if (counter < 0 || counter + 3 > datas.Length)
                return;

            datas[counter] = datas[counter + 1] = datas[counter + 2] = datas[counter + 3] = value;
        }

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

                    //we are very close
                    colorFrame[index + BlueIndex] = 255;
                    colorFrame[index + GreenIndex] = 255;
                    colorFrame[index + RedIndex] = 255;
                    colorFrame[index + AlphaIndex] = 255;

                    ////Color a player
                    if (GetPlayerIndex(depthData[depthIndex]) > 0)
                    {
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

            //Calculer la véritable distance
            distance = distance / nbDistance;
            int distanceTropLoin = Math.Max(0, Math.Min(400, distance - 2400));
            int distanceTropProche = Math.Max(0, Math.Min(400, 2400 - distance));
            distanceLoinProgress.Value = distanceTropLoin;
            distanceProcheProgress.Value = distanceTropProche;

            int distanceCouleur = Math.Max(distanceTropProche, distanceTropLoin);
            System.Windows.Media.Color couleurProgress = new System.Windows.Media.Color();
            couleurProgress.ScR = (float)(distanceCouleur) / 100;
            couleurProgress.ScG = (float)(400 - distanceCouleur) / 800;
            couleurProgress.ScA = 1;
            distanceLoinProgress.Foreground = new SolidColorBrush(couleurProgress);
            distanceProcheProgress.Foreground = new SolidColorBrush(couleurProgress);

            return colorFrame;
        }

        private static int GetPlayerIndex(byte firstFrame)
        {
            //returns 0 = no player, 1 = 1st player, 2 = 2nd player...
            //bitwise & on firstFrame
            return (int)firstFrame & 7;
        }

        private void Window_Unloaded(object sender, RoutedEventArgs e)
        {
            //cleanup
            nui.Uninitialize();
        }

        private void saveImage_Click(object sender, RoutedEventArgs e)
        {
            //Enregistrer l'image actuellement affichée
            string now = DateTime.Now.ToString().Replace('/', '-').Replace(':', ' ');

            //Image processing
            Bitmap antiAliasBmp = Util.AntiAliasing(Util.GetBitmap(preview.Source as BitmapSource));
            String path = (nbPlayers.SelectedIndex + 1) + "j\\" + now + ".png";
            Util.GetBitmapSource(antiAliasBmp).Save("..\\..\\..\\..\\Walls\\" + path, Coding4Fun.Kinect.Wpf.ImageFormat.Png);

            pathLabel.Content = path;
        }
    }
}

