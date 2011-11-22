using OpenTK;
using OpenTK.Graphics.OpenGL;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace MotionChallenge
{
    /*
     * LEVEL
     * 
     *  Manage walls.
     * 
     */
    class Level
    {
        private Player player;
        private Wall wall;
        private const int HOLE_THRESHOLD = 10;

        private GLControl glControl;

        // Initialisation des variables liees aux traces OpenGL
        float cameraX = 0;
        float cameraY = -400;
        float cameraZ = 170;
        float cameraDirectionX = 0;
        float cameraDirectionY = 0;
        float cameraDirectionZ = 170;
        float cameraSpeed = 10;
        Matrix4 cameraLookAt;
        double wallWidth = 180;
        double wallHeight = 240;
        double wallDepth = 10;
        double initialWallY = -200;
        double groundMin = -400;
        double groundMax = 100;
        double groundWidth = 180;
        int textureId;

        public Level(GLControl _glControl, int playerCount)
        {
           glControl = _glControl;
           player = new Player(playerCount);
           wall = new Wall();

           initStage(); 
        }

        public void update(int elapsed)
        {
            // update wall position
            wall.update(elapsed);
            //player.update(elapsed);

            // force OpenGL update in main thread
            glControl.Invalidate();          
          
            if (wall.atEndOfLine())
            {
                // check player
                if (player.percentOut(wall) >= HOLE_THRESHOLD)
                {
                    // Not Ok: Game Over
                }
                else
                {
                    // Ok: increase score, new wall, etc
                }
            }
        }

////////////////////// --- UI methods below  --- //////////////////////
         
        private void initStage()
        {
            // Add draw routine
            glControl.Paint += new PaintEventHandler(this.drawAll);

            // Initialize OpenGL
            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.Texture2D);
            GL.Enable(EnableCap.AlphaTest);
            GL.AlphaFunc(AlphaFunction.Greater, 0.1f);

            // Texture loading
            textureId = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, textureId);
            Bitmap bitmap = new Bitmap(@"..\..\Texture.png");
            bitmap.RotateFlip(RotateFlipType.RotateNoneFlipY);
            BitmapData bitmapData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, bitmapData.Width, bitmapData.Height, 0, OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, bitmapData.Scan0);
            bitmap.UnlockBits(bitmapData);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
        }

        private void drawAll(object sender, PaintEventArgs e)
        {
            this.draw(glControl);
            // TODO wall.draw(glControl);
            // TODO player.draw(glControl);
        }

        private void draw(GLControl glControl)
        {
            double wallY = initialWallY * wall.getPosition() / 1000;
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            // Initialize OpenGL matrices
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
            GL.Frustum(-1, 1, -1, 0.5, 1, 1000);
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity();

            // Camera configuration
            cameraLookAt = Matrix4.LookAt(cameraX, cameraY, cameraZ, cameraDirectionX, cameraDirectionY, cameraDirectionZ, 0.0f, 0.0f, 1.0f);
            GL.LoadMatrix(ref cameraLookAt);

            // Univers
            GL.BindTexture(TextureTarget.Texture2D, 0);
            GL.Begin(BeginMode.Quads);
            GL.Color3(Color.LightGray);

            // Face arriere gauche
            GL.Vertex3(-1000, 1, wallHeight * 4);
            GL.Vertex3(-wallWidth, 1, wallHeight * 4);
            GL.Vertex3(-wallWidth, 1, 0);
            GL.Vertex3(-1000, 1, 0);

            // Face arriere centrale (en avant)
            GL.Vertex3(-wallWidth, 1, wallHeight * 4);
            GL.Vertex3(wallWidth, 1, wallHeight * 4);
            GL.Vertex3(wallWidth, 1, wallHeight);
            GL.Vertex3(-wallWidth, 1, wallHeight);

            // Face arriere droite
            GL.Vertex3(wallWidth, 1, wallHeight * 4);
            GL.Vertex3(1000, 1, wallHeight * 4);
            GL.Vertex3(1000, 1, 0);
            GL.Vertex3(wallWidth, 1, 0);

            GL.Color3(Color.White);
            GL.End();
            GL.BindTexture(TextureTarget.Texture2D, 0);
            GL.Begin(BeginMode.Quads);
            GL.Color3(Color.DarkGray);

            // Face arriere centrale (en arriere)
            GL.Vertex3(-wallWidth, 100, wallHeight);
            GL.Vertex3(wallWidth, 100, wallHeight);
            GL.Vertex3(wallWidth, 100, 0);
            GL.Vertex3(-wallWidth, 100, 0);

            GL.Color3(Color.White);
            GL.End();

            // Mur qui avance
            GL.BindTexture(TextureTarget.Texture2D, textureId);
            GL.Begin(BeginMode.Quads);
            // Face arriere
            GL.TexCoord2(0, 1); GL.Vertex3(-wallWidth, wallY + wallDepth, wallHeight);
            GL.TexCoord2(1, 1); GL.Vertex3(wallWidth, wallY + wallDepth, wallHeight);
            GL.TexCoord2(1, 0); GL.Vertex3(wallWidth, wallY + wallDepth, 0);
            GL.TexCoord2(0, 0); GL.Vertex3(-wallWidth, wallY + wallDepth, 0);

            // Face laterale gauche
            GL.TexCoord2(0, 1); GL.Vertex3(-wallWidth, wallY, wallHeight);
            GL.TexCoord2(1, 1); GL.Vertex3(-wallWidth, wallY + wallDepth, wallHeight);
            GL.TexCoord2(1, 0); GL.Vertex3(-wallWidth, wallY + wallDepth, 0);
            GL.TexCoord2(0, 0); GL.Vertex3(-wallWidth, wallY, 0);

            // Face laterale droite
            GL.TexCoord2(0, 1); GL.Vertex3(wallWidth, wallY, wallHeight);
            GL.TexCoord2(1, 1); GL.Vertex3(wallWidth, wallY + wallDepth, wallHeight);
            GL.TexCoord2(1, 0); GL.Vertex3(wallWidth, wallY + wallDepth, 0);
            GL.TexCoord2(0, 0); GL.Vertex3(wallWidth, wallY, 0);

            // Face interieure 1
            GL.TexCoord2(0, 1); GL.Vertex3(-wallWidth, wallY + wallDepth * 1 / 4, wallHeight);
            GL.TexCoord2(1, 1); GL.Vertex3(wallWidth, wallY + wallDepth * 1 / 4, wallHeight);
            GL.TexCoord2(1, 0); GL.Vertex3(wallWidth, wallY + wallDepth * 1 / 4, 0);
            GL.TexCoord2(0, 0); GL.Vertex3(-wallWidth, wallY + wallDepth * 1 / 4, 0);

            // Face interieure 2
            GL.TexCoord2(0, 1); GL.Vertex3(-wallWidth, wallY + wallDepth / 2, wallHeight);
            GL.TexCoord2(1, 1); GL.Vertex3(wallWidth, wallY + wallDepth / 2, wallHeight);
            GL.TexCoord2(1, 0); GL.Vertex3(wallWidth, wallY + wallDepth / 2, 0);
            GL.TexCoord2(0, 0); GL.Vertex3(-wallWidth, wallY + wallDepth / 2, 0);

            // Face interieure 2
            GL.TexCoord2(0, 1); GL.Vertex3(-wallWidth, wallY + wallDepth * 3 / 4, wallHeight);
            GL.TexCoord2(1, 1); GL.Vertex3(wallWidth, wallY + wallDepth * 3 / 4, wallHeight);
            GL.TexCoord2(1, 0); GL.Vertex3(wallWidth, wallY + wallDepth * 3 / 4, 0);
            GL.TexCoord2(0, 0); GL.Vertex3(-wallWidth, wallY + wallDepth * 3 / 4, 0);

            // Face frontale
            GL.Color3(Color.Green); GL.TexCoord2(0, 1); GL.Vertex3(-wallWidth, wallY, wallHeight);
            GL.Color3(Color.Green); GL.TexCoord2(1, 1); GL.Vertex3(wallWidth, wallY, wallHeight);
            GL.Color3(Color.Green); GL.TexCoord2(1, 0); GL.Vertex3(wallWidth, wallY, 0);
            GL.Color3(Color.Green); GL.TexCoord2(0, 0); GL.Vertex3(-wallWidth, wallY, 0);

            GL.Color3(Color.White);
            GL.End();

            // Sol (trajectoire du mur)
            GL.BindTexture(TextureTarget.Texture2D, 0);
            GL.Begin(BeginMode.Quads);
            GL.Color3(Color.Yellow);

            GL.Vertex3(-groundWidth, groundMax, 0);
            GL.Vertex3(groundWidth, groundMax, 0);
            GL.Vertex3(groundWidth, groundMin, 0);
            GL.Vertex3(-groundWidth, groundMin, 0);

            GL.Color3(Color.White);
            GL.End();

            // Sol (global)
            GL.BindTexture(TextureTarget.Texture2D, 0);
            GL.Begin(BeginMode.Quads);
            GL.Vertex3(-1000, groundMax, 0);
            GL.Vertex3(1000, groundMax, 0);
            GL.Vertex3(1000, groundMin, 0);
            GL.Vertex3(-1000, groundMin, 0);
            GL.End();

            GL.Flush();
            glControl.SwapBuffers();
        }
    }
}
