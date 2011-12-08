﻿using OpenTK;
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

        private GLControl glControl;

        // Initialisation des variables liees aux traces OpenGL
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

            // Initalize OpenGL font rendering
            //fontTextureId = TexUtil.CreateTextureFromFile(@"..\..\Font.png");
            //textureFont = new TextureFont(fontTextureId);
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
            player.draw(wall.getY());

            // Indicateurs textuels
            //textureFont.WriteStringAt("Hole in the Wall", 4, 8, 96, 0);

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

            // Univers 2
            GL.BindTexture(TextureTarget.Texture2D, 0);
            GL.Begin(BeginMode.Quads);
                GL.Color3(Color.DarkGray);

                // Face arriere centrale (en arriere)
                GL.Vertex3(-2 * Wall.wallWidth, 100, 2 * Wall.wallHeight);
                GL.Vertex3(2 * Wall.wallWidth, 100, 2 * Wall.wallHeight);
                GL.Vertex3(2 * Wall.wallWidth, 100, 0);
                GL.Vertex3(-2 * Wall.wallWidth, 100, 0);

                GL.Color3(Color.White);
            GL.End();

            // Sol (dégradé vers la zone de jeu)
            /*GL.BindTexture(TextureTarget.Texture2D, 0);
            GL.Begin(BeginMode.Quads);
                GL.Color3(Color.Yellow);

                GL.Vertex3(-groundWidth, -200 + Wall.wallDepth * 2, 0);
                GL.Vertex3(groundWidth, -200 + Wall.wallDepth * 2, 0);

                GL.Color3(Color.Red);
                GL.Vertex3(groundWidth, -200 + Wall.wallDepth, 0);
                GL.Vertex3(-groundWidth, -200 + Wall.wallDepth, 0);

                GL.Color3(Color.White);
            GL.End();*/

            // Sol (zone de jeu)
            GL.BindTexture(TextureTarget.Texture2D, 0);
            GL.Begin(BeginMode.Quads);
                GL.Color3(Color.Red);

                GL.Vertex3(-groundWidth, Wall.initialWallY * (1 - Wall.wallSpeedFix/1000) + Wall.wallDepth, 0);
                GL.Vertex3(groundWidth, Wall.initialWallY * (1 - Wall.wallSpeedFix/1000) + Wall.wallDepth, 0);
                GL.Vertex3(groundWidth, Wall.initialWallY * (1 - Wall.wallSpeedFix/1000), 0);
                GL.Vertex3(-groundWidth, Wall.initialWallY * (1 - Wall.wallSpeedFix/1000), 0);

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

            //////////////////////////////////////////////////////
        }
    }
}
