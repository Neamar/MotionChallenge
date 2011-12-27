using OpenTK;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Navigation;
using System.Timers;

namespace MotionChallenge
{
    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        /*
		 * Instance singleton pour récupérer la fenêtre principale
		 */
        static MainWindow instance;

        /*
		 * Composant permettant de dessiner de l'OpenGl avec WindowsForms.
		 */
        GLControl glControl;

        /*
		 * Le jeu en lui-même
		 */
        Game game;

		/*
		 * Le nombre de joueurs pour la partie en cours
		 */
        int playerCount;

        public MainWindow(int playerCount)
        {
            InitializeComponent();
            instance = this;
            this.playerCount = playerCount;
        }

        /**
		 * Méthode statique pour récupérer le singleton.
		 */
        public static MainWindow getInstance()
        {
            return instance;
        }

        /**
		 * La fenêtre vient de s'ouvrir.
		 */
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //Créer le composant OpenGL et définir ses limites
            glControl = new OpenTK.GLControl();
            glControl.SetBounds(0, 0, 640, 480);

            //L'ajouter à l'intérieur de la fenêtre
            windowsFormsHost.Child = glControl;

            //Créer un nouveau jeu
            game = new Game(glControl, playerCount);
        }

        /**
		 * La fenêtre va se fermer
		 */
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //Arrêter le jeu
            game.stopGame();
        }

        /**
		 * On a appuyé sur une touche, traiter l'évènement.
		 * @note espace = pause.
		 */
        private void Window_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Space:
                    {
                        game.togglePause();
                        break;
                    }
                default:
                    break;
            }
        }
    }
}
