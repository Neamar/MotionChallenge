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
	/**
	 * Définit un ensemble de murs, y compris le mur actuellement affiché
	 */
    class Wall
    {
		/*
		 * Un tableau contenant l'identifiant de toutes les textures mur.
		 */
        private int[] textureId;

		/*
		 * Un tableau contenant l'URI des différentes images sur le disque.
		 */
        private string[] wallsPath;

        /*
		 * Position relative du mur pendant son parcours (de 0 a 1000)
		 */
        private int position = 0;

		/**
		 * Quelques valeurs pour le mur :
		 */
        private const int NORMAL_SPEED = 10;
        private const int HARD_SPEED = 8;
        private int wallSpeed;
        private int wallCount;
        private int currentWallId = 0;

		/**
		 * Géométrie du mur
		 */
        public static double wallWidth = 180;
        public static double wallHeight = 240;
        public static double wallDepth = 10;
        public static double initialWallY = -200;
        public static double wallSpeedFix = 10;

        Random randomizer = new Random();
        Color[] colors = new Color[4];

		/**
		 * À l'initialisation, charger tous les murs du mode sélectionné en mémoire
		 */
        public Wall(int playerCount)
        {
			//On ne peut pas utiliser de chemin absolu, il faut tricher en relatif :
            wallsPath = Directory.GetFiles(@"..\..\..\..\Walls\" + playerCount + @"j\", "*.png");
            wallCount = wallsPath.Length;

            textureId = new int[wallCount];

			//Définir la vitesse du mur en fonction du mode choisi. Le mode HARD est plus lent car les images sont plus dures.
            wallSpeed = (playerCount > 3) ? HARD_SPEED : NORMAL_SPEED;

            //Chargement des textures
            for (int i = 0; i < wallCount; i++)
            {
                textureId[i] = TexLib.TexUtil.CreateTextureFromFile(wallsPath[i]);
                GL.BindTexture(TextureTarget.Texture2D, textureId[i]);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            }

            //Générer les couleurs pour le mur (elles sont aléatoires pour chaque mur)
            generateNewColors();
        }

        /**
		 * Nettoyer la mémoire pour éviter les fuites.
		 */
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

        /**
		 * Renvoie true si le mur est en fin de course
		 */
        public bool atEndOfLine()
        {
            return (position >= 1000);
        }

        public int getPosition()
        {
            return position;
        }

        /**
		 * Met à jour la position du mur
		 */
        public void update(int elapsed)
        {
            if (!atEndOfLine())
            {
                // update wall position
                position += wallSpeed;
            }
            else
            {
				//Passer au mur suivant
                position = 0;
                currentWallId++;

				//Définir les couleurs du nouveau mur
                generateNewColors();
                if (currentWallId >= wallCount)
                {
                    currentWallId = 0;
                }
            }
        }

        /**
		 * Récupérer un tableau de byte représentant le mur actuel.
		 * Cette fonction permettra de déterminer la précision du joueur.
		 */
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

        /**
		 * Détermine la position sur l'axe Y du mur en fonction de son avancement
		 */
        public double getY()
        {
            return initialWallY * getPosition() / 1000;
        }

        /**
		 * Dessiner le mur à l'écran.
		 */
        public void draw()
        {
            double wallY = getY();

            // Déplacer le mur.
			// Un mur est constitué de 4 couches afin de lui donner une illusion de profondeur.
            GL.BindTexture(TextureTarget.Texture2D, textureId[currentWallId]);
            GL.Begin(BeginMode.Quads);
                // Arrière
                GL.TexCoord2(0, 0); GL.Vertex3(-wallWidth, wallY + wallDepth, wallHeight);
                GL.TexCoord2(1, 0); GL.Vertex3(wallWidth, wallY + wallDepth, wallHeight);
                GL.TexCoord2(1, 1); GL.Vertex3(wallWidth, wallY + wallDepth, 0);
                GL.TexCoord2(0, 1); GL.Vertex3(-wallWidth, wallY + wallDepth, 0);

                // Côté gauche
                GL.TexCoord2(0, 0); GL.Vertex3(-wallWidth, wallY, wallHeight);
                GL.TexCoord2(1, 0); GL.Vertex3(-wallWidth, wallY + wallDepth, wallHeight);
                GL.TexCoord2(1, 1); GL.Vertex3(-wallWidth, wallY + wallDepth, 0);
                GL.TexCoord2(0, 1); GL.Vertex3(-wallWidth, wallY, 0);

                // Côté droit
                GL.TexCoord2(0, 0); GL.Vertex3(wallWidth, wallY, wallHeight);
                GL.TexCoord2(1, 0); GL.Vertex3(wallWidth, wallY + wallDepth, wallHeight);
                GL.TexCoord2(1, 1); GL.Vertex3(wallWidth, wallY + wallDepth, 0);
                GL.TexCoord2(0, 1); GL.Vertex3(wallWidth, wallY, 0);

                // Mur intérieur n°1
                GL.TexCoord2(0, 0); GL.Vertex3(-wallWidth, wallY + wallDepth * 1 / 4, wallHeight);
                GL.TexCoord2(1, 0); GL.Vertex3(wallWidth, wallY + wallDepth * 1 / 4, wallHeight);
                GL.TexCoord2(1, 1); GL.Vertex3(wallWidth, wallY + wallDepth * 1 / 4, 0);
                GL.TexCoord2(0, 1); GL.Vertex3(-wallWidth, wallY + wallDepth * 1 / 4, 0);

				// Mur intérieur n°2
                GL.TexCoord2(0, 0); GL.Vertex3(-wallWidth, wallY + wallDepth * 2 / 4, wallHeight);
                GL.TexCoord2(1, 0); GL.Vertex3(wallWidth, wallY + wallDepth * 2 / 4, wallHeight);
                GL.TexCoord2(1, 1); GL.Vertex3(wallWidth, wallY + wallDepth * 2 / 4, 0);
                GL.TexCoord2(0, 1); GL.Vertex3(-wallWidth, wallY + wallDepth * 2 / 4, 0);

				// Mur intérieur n°3
                GL.TexCoord2(0, 0); GL.Vertex3(-wallWidth, wallY + wallDepth * 3 / 4, wallHeight);
                GL.TexCoord2(1, 0); GL.Vertex3(wallWidth, wallY + wallDepth * 3 / 4, wallHeight);
                GL.TexCoord2(1, 1); GL.Vertex3(wallWidth, wallY + wallDepth * 3 / 4, 0);
                GL.TexCoord2(0, 1); GL.Vertex3(-wallWidth, wallY + wallDepth * 3 / 4, 0);

                // Mur de face
                GL.Color3(colors[0]); GL.TexCoord2(0, 0); GL.Vertex3(-wallWidth, wallY, wallHeight);
                GL.Color3(colors[1]); GL.TexCoord2(1, 0); GL.Vertex3(wallWidth, wallY, wallHeight);
                GL.Color3(colors[2]); GL.TexCoord2(1, 1); GL.Vertex3(wallWidth, wallY, 0);
                GL.Color3(colors[3]); GL.TexCoord2(0, 1); GL.Vertex3(-wallWidth, wallY, 0);

                GL.Color3(Color.White);
            GL.End();
        }

        /**
		 * Génère aléatoirement un tableau de couleur qui sera utilisé pour colorier le mur.
		 */
        private void generateNewColors()
        {
            for (int i = 0; i < colors.Length; i++)
            {
                colors[i] = getRandomColor();
            }
        }

        /**
		 * Renvoie une couleur déterminée aléatoirement
		 */
        private Color getRandomColor()
        {
            return Color.FromArgb(
                    randomizer.Next(256),
                    randomizer.Next(256),
                    randomizer.Next(256));
        }
    }
}
