using OpenTK;
using OpenTK.Graphics.OpenGL;
using System;
using System.Drawing;
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
        private const string HUD_INFO = "Nombre de joueur(s) : %pc\r\nNombre de murs : %w/%tw\r\nDernier score : %ls";

        private Player player;
        private Wall wall;
        private const int HOLE_THRESHOLD = 10;

        private bool hudIsDirty = true;
        private int totalScore = 0;
        private int lastScore = 0;
        private int totalWall = 0;
        private int currentWall = 1;

        // The GLControl used for displaying OpenGL graphics
        private GLControl glControl;

        // Parameters used for drawing with OpenGL
        // The vertical axis is Z
        // The player stand on the Y axis, his eyes are the camera (average height is about 170 cm)
        // The wall is at (0, 0, 0) at the beginning of the game and is moving to the player (wallY is decreasing)
        // The player is facing the wall, looking at a fixed vertex (0, 0, 170)
        // groundMin, groundMax, groundWidth define the size of the rail where the wall is moving
        float cameraX = 0;
        float cameraY = -400;
        float cameraZ = 170;
        float cameraDirectionX = 0;
        float cameraDirectionY = 0;
        float cameraDirectionZ = 170;
        Matrix4 cameraLookAt;
        double groundMin = -400;
        double groundMax = 100;
        double groundWidth = 180;
        
        public Level(GLControl _glControl, int playerCount)
        {
            glControl = _glControl;
            player = new Player(playerCount);
            wall = new Wall(playerCount);
            totalWall = wall.getNumberOfWalls();

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
                int[] percent = player.percentValues(wall);
                Console.WriteLine("In " + percent[0] + "        | Out " + percent[1]);
                lastScore = Math.Max(0, percent[0] - (percent[1] - 20) / 2);

                totalScore += lastScore;
                currentWall++;
                hudIsDirty = true;
            }
        }

        public void reset()
        {
            wall.reset();
            player.reset();
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
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
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

            //Dessin de la scene alentour
            this.draw();

            //Dessin du mur
            wall.draw();
            player.draw(wall.getY());

            GL.Flush();
            glControl.SwapBuffers();
        }

        private void draw()
        {
            //////////////////// 2D graphics ////////////////////
            if (hudIsDirty)
            {
                MainWindow.getInstance().scoreLabel.Content = totalScore.ToString();
                string infos = HUD_INFO;
                infos = infos.Replace("%pc", player.getPlayerCount().ToString());
                infos = infos.Replace("%w", currentWall.ToString());
                infos = infos.Replace("%tw", totalWall.ToString());
                infos = infos.Replace("%ls", lastScore.ToString());
                MainWindow.getInstance().infosLabel.Text = infos;
                MainWindow.getInstance().wallProgress.Value = currentWall - 1;
                MainWindow.getInstance().wallProgress.Maximum = totalWall;

                hudIsDirty = true;

                if (currentWall > totalWall)
                {
                    //End of the game
                    MainWindow.getInstance().Close();
                }
            }
            /////////////////////////////////////////////////////

            //////////////////// 3D graphics ////////////////////
            // Universe
            GL.BindTexture(TextureTarget.Texture2D, 0);
            GL.Begin(BeginMode.Quads);
                GL.Color3(Color.LightGray);

                // Left side of the universe
                GL.Vertex3(-1000, 1, Wall.wallHeight * 4);
                GL.Vertex3(-Wall.wallWidth, 1, Wall.wallHeight * 4);
                GL.Vertex3(-Wall.wallWidth, 1, 0);
                GL.Vertex3(-1000, 1, 0);

                // Front side of the universe
                GL.Vertex3(-Wall.wallWidth, 1, Wall.wallHeight * 4);
                GL.Vertex3(Wall.wallWidth, 1, Wall.wallHeight * 4);
                GL.Vertex3(Wall.wallWidth, 1, Wall.wallHeight);
                GL.Vertex3(-Wall.wallWidth, 1, Wall.wallHeight);

                // Right side of the universe
                GL.Vertex3(Wall.wallWidth, 1, Wall.wallHeight * 4);
                GL.Vertex3(1000, 1, Wall.wallHeight * 4);
                GL.Vertex3(1000, 1, 0);
                GL.Vertex3(Wall.wallWidth, 1, 0);

                GL.Color3(Color.White);
            GL.End();

            // Universe 2
            GL.BindTexture(TextureTarget.Texture2D, 0);
            GL.Begin(BeginMode.Quads);
                GL.Color3(Color.DarkGray);

                // Background of the universe
                GL.Vertex3(-2 * Wall.wallWidth, 100, 2 * Wall.wallHeight);
                GL.Vertex3(2 * Wall.wallWidth, 100, 2 * Wall.wallHeight);
                GL.Vertex3(2 * Wall.wallWidth, 100, 0);
                GL.Vertex3(-2 * Wall.wallWidth, 100, 0);

                GL.Color3(Color.White);
            GL.End();

            // Ground (play area line)
            GL.BindTexture(TextureTarget.Texture2D, 0);
            GL.Begin(BeginMode.Quads);
                GL.Color3(Color.Red);

                GL.Vertex3(-groundWidth, Wall.initialWallY * (1 - Wall.wallSpeedFix/1000) + Wall.wallDepth, 0);
                GL.Vertex3(groundWidth, Wall.initialWallY * (1 - Wall.wallSpeedFix/1000) + Wall.wallDepth, 0);
                GL.Vertex3(groundWidth, Wall.initialWallY * (1 - Wall.wallSpeedFix/1000), 0);
                GL.Vertex3(-groundWidth, Wall.initialWallY * (1 - Wall.wallSpeedFix/1000), 0);

                GL.Color3(Color.White);
            GL.End();

            // Ground (rail)
            GL.BindTexture(TextureTarget.Texture2D, 0);
            GL.Begin(BeginMode.Quads);
                GL.Color3(Color.Yellow);

                GL.Vertex3(-groundWidth, groundMax, 0);
                GL.Vertex3(groundWidth, groundMax, 0);
                GL.Vertex3(groundWidth, groundMin, 0);
                GL.Vertex3(-groundWidth, groundMin, 0);

                GL.Color3(Color.White);
            GL.End();

            // Ground (global)
            GL.BindTexture(TextureTarget.Texture2D, 0);
            GL.Begin(BeginMode.Quads);
                GL.Vertex3(-1000, groundMax, 0);
                GL.Vertex3(1000, groundMax, 0);
                GL.Vertex3(1000, groundMin, 0);
                GL.Vertex3(-1000, groundMin, 0);
            GL.End();
            //////////////////////////////////////////////////////
        }
    }
}
