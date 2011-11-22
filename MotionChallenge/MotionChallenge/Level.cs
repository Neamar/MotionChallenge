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
        double groundMin = -400;
        double groundMax = 100;
        double groundWidth = 180;
        int textureId;

        public Level(GLControl _glControl, int playerCount)
        {
           glControl = _glControl;
           player = new Player(playerCount);
           wall = new Wall(playerCount);

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

            //GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            //GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
        }

        private void drawAll(object sender, PaintEventArgs e)
        {
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

            //Dessin de la scène alentour
            this.draw();
            
            //Dessin du mur
            wall.draw();
            // TODO player.draw();

            GL.Flush();
            glControl.SwapBuffers();
        }

        private void draw()
        {
            // Univers
            GL.BindTexture(TextureTarget.Texture2D, 0);
            GL.Begin(BeginMode.Quads);
            GL.Color3(Color.LightGray);

            // Face arriere gauche
            GL.Vertex3(-1000, 1, Wall.wallHeight * 4);
            GL.Vertex3(-Wall.wallWidth, 1, Wall.wallHeight * 4);
            GL.Vertex3(-Wall.wallWidth, 1, 0);
            GL.Vertex3(-1000, 1, 0);

            // Face arriere centrale (en avant)
            GL.Vertex3(-Wall.wallWidth, 1, Wall.wallHeight * 4);
            GL.Vertex3(Wall.wallWidth, 1, Wall.wallHeight * 4);
            GL.Vertex3(Wall.wallWidth, 1, Wall.wallHeight);
            GL.Vertex3(-Wall.wallWidth, 1, Wall.wallHeight);

            // Face arriere droite
            GL.Vertex3(Wall.wallWidth, 1, Wall.wallHeight * 4);
            GL.Vertex3(1000, 1, Wall.wallHeight * 4);
            GL.Vertex3(1000, 1, 0);
            GL.Vertex3(Wall.wallWidth, 1, 0);

            GL.Color3(Color.White);
            GL.End();
            GL.BindTexture(TextureTarget.Texture2D, 0);
            GL.Begin(BeginMode.Quads);
            GL.Color3(Color.DarkGray);

            // Face arriere centrale (en arriere)
            GL.Vertex3(-Wall.wallWidth, 100, Wall.wallHeight);
            GL.Vertex3(Wall.wallWidth, 100, Wall.wallHeight);
            GL.Vertex3(Wall.wallWidth, 100, 0);
            GL.Vertex3(-Wall.wallWidth, 100, 0);

            GL.Color3(Color.White);
            GL.End();

            //wall.draw

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
        }
    }
}
