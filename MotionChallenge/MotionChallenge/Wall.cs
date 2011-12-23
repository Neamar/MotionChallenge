using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK.Graphics.OpenGL;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace MotionChallenge
{
    class Wall
    {
        private int[] textureId;
        private string[] wallsPath;

        // Position relative du mur pendant son parcours (de 0 a 1000)
        private int position = 0;

        private const int NORMAL_SPEED = 10;
        private const int HARD_SPEED = 8;
        private int wallSpeed;
        private int wallCount;
        private int currentWallId = 0;

        public static double wallWidth = 180;
        public static double wallHeight = 240;
        public static double wallDepth = 10;
        public static double initialWallY = -200;
        public static double wallSpeedFix = 10;

        Random randomizer = new Random();
        Color[] colors = new Color[4];

        public Wall(int playerCount)
        {
            wallsPath = Directory.GetFiles(@"..\..\..\..\Walls\" + playerCount + @"j\", "*.png");
            wallCount = wallsPath.Length;

            textureId = new int[wallCount];

            wallSpeed = (playerCount > 3) ? HARD_SPEED : NORMAL_SPEED;

            // Texture loading
            for (int i = 0; i < wallCount; i++)
            {
                textureId[i] = TexLib.TexUtil.CreateTextureFromFile(wallsPath[i]);
                GL.BindTexture(TextureTarget.Texture2D, textureId[i]);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            }

            // generate colors for first wall
            generateNewColors();
        }

        public void reset()
        {
            for (int i = 0; i < wallCount; i++)
            {
                GL.DeleteTexture(textureId[i]);
            }
        }

        public int getNumberOfWalls()
        {
            return wallCount;
        }

        public bool atEndOfLine()
        {
            return (position >= 1000);
        }

        public int getPosition()
        {
            return position;
        }

        public void update(int elapsed)
        {
            if (!atEndOfLine())
            {
                // update wall position
                position += wallSpeed;
            }
            else
            {
                position = 0;
                currentWallId++;
                generateNewColors();
                if (currentWallId >= wallCount)
                {
                    currentWallId = 0;
                }
                Console.WriteLine("New wall: " + currentWallId);
            }
        }

        public byte[] getCurrentWallByteArray()
        {
            Bitmap bmp = new Bitmap(wallsPath[currentWallId]);
            BitmapData bmpd = bmp.LockBits(
                new Rectangle(0, 0, bmp.Width, bmp.Height),
                ImageLockMode.ReadOnly,
                bmp.PixelFormat
            );

            //Copier les données basiques
            IntPtr ptr = bmpd.Scan0;

            int bytes = Math.Abs(bmpd.Stride) * bmp.Height;
            byte[] values = new byte[bytes];
            System.Runtime.InteropServices.Marshal.Copy(ptr, values, 0, bytes);

            bmp.UnlockBits(bmpd);

            return values;
        }

        public double getY()
        {
            return initialWallY * getPosition() / 1000;
        }

        public void draw()
        {
            double wallY = getY();

            // Moving wall
            // The moving wall is composed of 4 layers (+ left, right and rear sides)
            // These layers are textured with the posture that the player have to do
            // This succession of layers gives an illusion of a 3D hole in the wall
            GL.BindTexture(TextureTarget.Texture2D, textureId[currentWallId]);
            GL.Begin(BeginMode.Quads);
                // Rear side
                GL.TexCoord2(0, 0); GL.Vertex3(-wallWidth, wallY + wallDepth, wallHeight);
                GL.TexCoord2(1, 0); GL.Vertex3(wallWidth, wallY + wallDepth, wallHeight);
                GL.TexCoord2(1, 1); GL.Vertex3(wallWidth, wallY + wallDepth, 0);
                GL.TexCoord2(0, 1); GL.Vertex3(-wallWidth, wallY + wallDepth, 0);

                // Left side
                GL.TexCoord2(0, 0); GL.Vertex3(-wallWidth, wallY, wallHeight);
                GL.TexCoord2(1, 0); GL.Vertex3(-wallWidth, wallY + wallDepth, wallHeight);
                GL.TexCoord2(1, 1); GL.Vertex3(-wallWidth, wallY + wallDepth, 0);
                GL.TexCoord2(0, 1); GL.Vertex3(-wallWidth, wallY, 0);

                // Right side
                GL.TexCoord2(0, 0); GL.Vertex3(wallWidth, wallY, wallHeight);
                GL.TexCoord2(1, 0); GL.Vertex3(wallWidth, wallY + wallDepth, wallHeight);
                GL.TexCoord2(1, 1); GL.Vertex3(wallWidth, wallY + wallDepth, 0);
                GL.TexCoord2(0, 1); GL.Vertex3(wallWidth, wallY, 0);

                // Inner wall 1
                GL.TexCoord2(0, 0); GL.Vertex3(-wallWidth, wallY + wallDepth * 1 / 4, wallHeight);
                GL.TexCoord2(1, 0); GL.Vertex3(wallWidth, wallY + wallDepth * 1 / 4, wallHeight);
                GL.TexCoord2(1, 1); GL.Vertex3(wallWidth, wallY + wallDepth * 1 / 4, 0);
                GL.TexCoord2(0, 1); GL.Vertex3(-wallWidth, wallY + wallDepth * 1 / 4, 0);

                // Inner wall 2
                GL.TexCoord2(0, 0); GL.Vertex3(-wallWidth, wallY + wallDepth * 2 / 4, wallHeight);
                GL.TexCoord2(1, 0); GL.Vertex3(wallWidth, wallY + wallDepth * 2 / 4, wallHeight);
                GL.TexCoord2(1, 1); GL.Vertex3(wallWidth, wallY + wallDepth * 2 / 4, 0);
                GL.TexCoord2(0, 1); GL.Vertex3(-wallWidth, wallY + wallDepth * 2 / 4, 0);

                // Inner wall 3
                GL.TexCoord2(0, 0); GL.Vertex3(-wallWidth, wallY + wallDepth * 3 / 4, wallHeight);
                GL.TexCoord2(1, 0); GL.Vertex3(wallWidth, wallY + wallDepth * 3 / 4, wallHeight);
                GL.TexCoord2(1, 1); GL.Vertex3(wallWidth, wallY + wallDepth * 3 / 4, 0);
                GL.TexCoord2(0, 1); GL.Vertex3(-wallWidth, wallY + wallDepth * 3 / 4, 0);

                // Front side
                GL.Color3(colors[0]); GL.TexCoord2(0, 0); GL.Vertex3(-wallWidth, wallY, wallHeight);
                GL.Color3(colors[1]); GL.TexCoord2(1, 0); GL.Vertex3(wallWidth, wallY, wallHeight);
                GL.Color3(colors[2]); GL.TexCoord2(1, 1); GL.Vertex3(wallWidth, wallY, 0);
                GL.Color3(colors[3]); GL.TexCoord2(0, 1); GL.Vertex3(-wallWidth, wallY, 0);

                GL.Color3(Color.White);
            GL.End();
        }

        private void generateNewColors()
        {
            for (int i = 0; i < colors.Length; i++)
            {
                colors[i] = getRandomColor();
            }
        }

        private Color getRandomColor()
        {
            return Color.FromArgb(
                    randomizer.Next(256),
                    randomizer.Next(256),
                    randomizer.Next(256));
        }
    }
}
