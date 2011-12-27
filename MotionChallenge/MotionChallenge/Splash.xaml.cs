using System;

using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

using Microsoft.VisualBasic;

namespace MotionChallenge
{
    /// <summary>
    /// Logique d'interaction pour Splash.xaml
    /// </summary>
    public partial class Splash : Window
    {
		/*
		 * Le type de jeu en cours
		 */
        private int gameMode;

        public MainWindow mainWindow;

        public Splash()
        {
            InitializeComponent();
        }

        /**
		 * Lors d'un clic sur un bouton, lancer le jeu en lui passant en paramètre le mode choisi.
		 */
        private void nbPlayer_Click(object sender, RoutedEventArgs e)
        {
			//Récupérer le nombre de joueurs à partir du nom du bouton.
			//À noter : le mode MEGA HARD correspond à un hypothétique mode 4 joueurs.
            int playerCount = int.Parse((sender as Button).Name.Replace("nbPlayer", ""));

            //Mémorise les informations qui serviront pour le scoring
            gameMode = playerCount;

			//Initialiser la fenêtre de jeu et s'abonner à l'évènement de fermeture pour reprendre la main en fin de jeu
            mainWindow = new MainWindow(playerCount);
            mainWindow.Show();
            mainWindow.Closing += new System.ComponentModel.CancelEventHandler(mainWindow_Closing);

            this.Hide();
        }

        /**
		 * Une partie se termine.
		 * Récupérer le score et se réafficher
		 */
        void mainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            scoreLabel.Content = (sender as MainWindow).scoreLabel.Content;

            // Mettre à jour le tableau des scores
            listBox.Items.Insert(0, getModeName() + " \t" + (sender as MainWindow).scoreLabel.Content);

            this.Show();
        }

        string getModeName()
        {
            if (gameMode > 0)
            {
                if (gameMode < 4)
                {
                    return gameMode + " j";
                }
                else
                {
                    return "Hard";
                }
            }
            else
            {
                return "";
            }
        }
    }
}
