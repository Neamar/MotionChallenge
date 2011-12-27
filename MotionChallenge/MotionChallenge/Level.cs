using OpenTK;
using OpenTK.Graphics.OpenGL;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace MotionChallenge
{
    /**
	 * Un Level contient des murs, une représentation OpenGl et une "poignée" Kinect.
	 *
	 * C'est la classe la plus importante du jeu.
	 */
    class Level
    {
		/*
		 * Ce string sert de pattern pour l'affichage des informations sur le HUD (Head Up Display)
		 */
        private const string HUD_INFO = "Nombre de joueur(s) : %pc\r\nNombre de murs : %w/%tw\r\nDernier score : %ls";

		/*
		 * Cet objet comprend les données Kinect du joueur.
		 * @note le terme est ambigu, car pplayer peut aussi contenir *plusieurs* joueurs.
		 */
        private Player player;

		/*
		 * Tout comme player, le terme wall est ambigu. Il correspond à l'objet encapsulant tous les murs (dont le mur actuel).
		 */
        private Wall wall;

		/*
		 * La tolérance d'erreur
		 * TODO : supprimer ?
		 */
        private const int HOLE_THRESHOLD = 10;

		/*
		 * Définit si le HUD doit être rafraîchi à la prochaine itération du thread principal.
		 */
        private bool hudIsDirty = true;

		/*
		 * Le score total, tous murs confondus depuis le lancement du niveau
		 */
        private int totalScore = 0;

		/*
		 * Le score effectué sur le dernier mur affiché.
		 * 0 si aucun mur n'a encore été réussi.
		 */
        private int lastScore = 0;

		/*
		 * Le nombre total de murs à valider
		 */
        private int totalWall = 0;

		/*
		 * Le mur actuellement affiché
		 */
        private int currentWall = 1;

        /*
		 * Le contrôle permettant d'afficher de l'OpenGL.
		 */
        private GLControl glControl;

		/**
		 * Une liste de paramètres utilisé pour le dessin OpenGL.
		 * @note l'axe vertical est l'axe Z
		 * @note le joueur se trouve sur l'axe Y et se déplace sur l'axe X
		 * @note la caméra correspond globalement aux yeux du joueur (à 1m70 du sol)
		 * @note le mur débute en (0,0,0) puis se déplace sur l'axe Y
		 */
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

		/**
		 * Initialise le Level.
		 */
        public Level(GLControl _glControl, int playerCount)
        {
			//Enregistrer le composant OpenGl à utiliser pour le dessin 3D
            glControl = _glControl;

			//Initialise le joueur contneant les données Kinect
            player = new Player(playerCount);

			//Crée l'objet mur, en lui indiquant le nombre de joueurs afin de pouvoir charger les images appropriées.
            wall = new Wall(playerCount);

			//Définit le nombre total de murs, qui sera affiché sur le HUD.
            totalWall = wall.getNumberOfWalls();

			//Initialise OpenGL.
            initStage();
        }

        /**
		 * Fait avancer la scène de `elapsed` millisecondes
		 */
        public void update(int elapsed)
        {
            //Met à jour la position du mur
            wall.update(elapsed);

			//Théoriquement, on devrait ici appeler une méthode update() sur le joueur.
			//Cependant, Kinect s'occupe de mettre à jour automatiquement les données via l'évènement DepthFrameReady et ce n'est donc pas nécessaire
            //player.update(elapsed);

            //Forcer une mise à jour OpenGl en invalidant les données précédemment dessinnées
            glControl.Invalidate();

			//Le mur est-il en bout de rail ?
            if (wall.atEndOfLine())
            {
                //Déterminer le pourcentage du joueur dans le mur et hors du mur :
                int[] percent = player.percentValues(wall);

				//TODO : supprimer ?
                Console.WriteLine("In " + percent[0] + "        | Out " + percent[1]);

				//Enregistrer le dernier score pour l'afficher dans le HUD
                lastScore = Math.Max(0, percent[0] - (percent[1] - 20) / 2);

                totalScore += lastScore;
                currentWall++;

				//Lever le flag demandant une mise à jour du HUD.
                hudIsDirty = true;
            }
        }

        public void reset()
        {
            wall.reset();
            player.reset();
        }


        ////////////////////// --- UI methods below  --- //////////////////////

        /**
		 * Initialise la scène en indiquant à OpenGl la façon de dessiner.
		 */
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

        /**
		 * Redessine la scène
		 */
        private void drawAll(object sender, PaintEventArgs e)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            //Initialisation des matrices d'OpenGL :
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
            GL.Frustum(-1, 1, -1, 0.5, 1, 1000);
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity();

            //Configurer la caméra
            cameraLookAt = Matrix4.LookAt(cameraX, cameraY, cameraZ, cameraDirectionX, cameraDirectionY, cameraDirectionZ, 0.0f, 0.0f, 1.0f);
            GL.LoadMatrix(ref cameraLookAt);

            //Dessin de la scene alentour
            this.draw();

            //Dessin du mur
            wall.draw();
            player.draw(wall.getY());

			//Envoyer les données et mettre à jour l'affichage :
            GL.Flush();
            glControl.SwapBuffers();
        }

        private void draw()
        {
            /**
			 * GRAPHIQUES 2D : HUD
			 */
            if (hudIsDirty)
            {
				//Mettre à jour le HUD à partir du pattern et des informations stockées sur la classe.
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
                    //Fin du jeu
                    MainWindow.getInstance().Close();
                }
            }


            /**
			 * Graphiques 3D
			 */
            GL.BindTexture(TextureTarget.Texture2D, 0);
            GL.Begin(BeginMode.Quads);
                GL.Color3(Color.LightGray);

                // Côté gauche
                GL.Vertex3(-1000, 1, Wall.wallHeight * 4);
                GL.Vertex3(-Wall.wallWidth, 1, Wall.wallHeight * 4);
                GL.Vertex3(-Wall.wallWidth, 1, 0);
                GL.Vertex3(-1000, 1, 0);

                // Côté face
                GL.Vertex3(-Wall.wallWidth, 1, Wall.wallHeight * 4);
                GL.Vertex3(Wall.wallWidth, 1, Wall.wallHeight * 4);
                GL.Vertex3(Wall.wallWidth, 1, Wall.wallHeight);
                GL.Vertex3(-Wall.wallWidth, 1, Wall.wallHeight);

                // Côté droit
                GL.Vertex3(Wall.wallWidth, 1, Wall.wallHeight * 4);
                GL.Vertex3(1000, 1, Wall.wallHeight * 4);
                GL.Vertex3(1000, 1, 0);
                GL.Vertex3(Wall.wallWidth, 1, 0);

                GL.Color3(Color.White);
            GL.End();

            GL.BindTexture(TextureTarget.Texture2D, 0);
            GL.Begin(BeginMode.Quads);
                GL.Color3(Color.DarkGray);

                // Arrière plan du monde
                GL.Vertex3(-2 * Wall.wallWidth, 100, 2 * Wall.wallHeight);
                GL.Vertex3(2 * Wall.wallWidth, 100, 2 * Wall.wallHeight);
                GL.Vertex3(2 * Wall.wallWidth, 100, 0);
                GL.Vertex3(-2 * Wall.wallWidth, 100, 0);

                GL.Color3(Color.White);
            GL.End();

            // Zone de jeu (la ligne)
            GL.BindTexture(TextureTarget.Texture2D, 0);
            GL.Begin(BeginMode.Quads);
                GL.Color3(Color.Red);

                GL.Vertex3(-groundWidth, Wall.initialWallY * (1 - Wall.wallSpeedFix/1000) + Wall.wallDepth, 0);
                GL.Vertex3(groundWidth, Wall.initialWallY * (1 - Wall.wallSpeedFix/1000) + Wall.wallDepth, 0);
                GL.Vertex3(groundWidth, Wall.initialWallY * (1 - Wall.wallSpeedFix/1000), 0);
                GL.Vertex3(-groundWidth, Wall.initialWallY * (1 - Wall.wallSpeedFix/1000), 0);

                GL.Color3(Color.White);
            GL.End();

            // Zone de jeu (le rail)
            GL.BindTexture(TextureTarget.Texture2D, 0);
            GL.Begin(BeginMode.Quads);
                GL.Color3(Color.Yellow);

                GL.Vertex3(-groundWidth, groundMax, 0);
                GL.Vertex3(groundWidth, groundMax, 0);
                GL.Vertex3(groundWidth, groundMin, 0);
                GL.Vertex3(-groundWidth, groundMin, 0);

                GL.Color3(Color.White);
            GL.End();

            // Le sol (le reste)
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
