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
    /*
     * PLAYER
     * 
     * 
     */
    class Player
    {
        private int count;
        private Runtime nui;
        private BitmapSource bitmapSource;
        private int playerTextureId;

        public Player(int playerCount)
        {
            count = playerCount;

            if (Runtime.Kinects.Count > 0)
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
            }
            else
            {
                Console.WriteLine("WARNING: No Kinect connected!");
            }
        }

        ~Player()
        {
            if(nui != null)
                nui.Uninitialize();
        }
        
        public int percentOut(Wall wall)
        {
            // compare player's body and hole in the wall
            // return value between 0 and 100

            // TODO check if player is in the hole

            return 0;
        }

        void nui_DepthFrameReady(object sender, ImageFrameReadyEventArgs e)
        {
            //Convert depth information for a pixel into color information
            byte[] ColoredBytes = GenerateColoredBytes(e.ImageFrame);

            //create an image based on returned colors

            PlanarImage image = e.ImageFrame.Image;
            bitmapSource = BitmapSource.Create(image.Width, image.Height, 96, 96, PixelFormats.Bgra32, null,
                ColoredBytes, image.Width * PixelFormats.Bgra32.BitsPerPixel / 8);
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


            var depthIndex = 0;
            for (var y = 0; y < height; y++)
            {

                var heightOffset = y * width;

                for (var x = 0; x < width; x++)
                {

                    var index = ((x + 0) + heightOffset) * 4;

                    //Pas de joueur
                    colorFrame[index + BlueIndex] = 255;
                    colorFrame[index + GreenIndex] = 255;
                    colorFrame[index + RedIndex] = 255;
                    colorFrame[index + AlphaIndex] = 0;

                    ////Color a player
                    if (GetPlayerIndex(depthData[depthIndex]) > 0)
                    {
                        colorFrame[index + BlueIndex] = 0;
                        colorFrame[index + GreenIndex] = 255;
                        colorFrame[index + RedIndex] = 0;
                        colorFrame[index + AlphaIndex] = 50;
                    }
                    //jump two bytes at a time
                    depthIndex += 2;
                }
            }

            return colorFrame;
        }

        private int GetPlayerIndex(byte firstFrame)
        {
            //returns 0 = no player, 1 = 1st player, 2 = 2nd player...
            //bitwise & on firstFrame
            return (int)firstFrame & 7;
        }

        private Bitmap GetBitmap(BitmapSource source)
        {
            Bitmap bmp = new Bitmap(source.PixelWidth, source.PixelHeight, System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
            BitmapData data = bmp.LockBits(
              new Rectangle(System.Drawing.Point.Empty, bmp.Size),
              ImageLockMode.WriteOnly,
              System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
            source.CopyPixels(Int32Rect.Empty, data.Scan0, data.Height * data.Stride, data.Stride);
            bmp.UnlockBits(data);
            bmp.RotateFlip(RotateFlipType.RotateNoneFlipY);
            return bmp;
        }

        public void draw()
        {
            // Chargement de la texture du joueur
            if (bitmapSource != null)
            {
                playerTextureId = TexUtil.CreateTextureFromBitmap(GetBitmap(bitmapSource));
                GL.BindTexture(TextureTarget.Texture2D, playerTextureId);
            }
            
            // Silhouette du joueur
            GL.BindTexture(TextureTarget.Texture2D, playerTextureId);
            GL.Begin(BeginMode.Quads);
            GL.TexCoord2(0, 1); GL.Vertex3(-Wall.wallWidth, -200, Wall.wallHeight);
            GL.TexCoord2(1, 1); GL.Vertex3(Wall.wallWidth, -200, Wall.wallHeight);
            GL.TexCoord2(1, 0); GL.Vertex3(Wall.wallWidth, -200, 0);
            GL.TexCoord2(0, 0); GL.Vertex3(-Wall.wallWidth, -200, 0);

            GL.Color3(System.Drawing.Color.White);
            GL.End();
        }
    }
}
