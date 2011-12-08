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
        private int gameMode;

        public MainWindow mainWindow;

        public Splash()
        {
            InitializeComponent();
        }

        private void nbPlayer_Click(object sender, RoutedEventArgs e)
        {
            int playerCount = int.Parse((sender as Button).Name.Replace("nbPlayer", ""));

            // Memorise les informations qui serviront pour le scoring
            gameMode = playerCount;
            // Demande le nom des joueurs
            //Microsoft.VisualBasic.Interaction.InputBox("Noms : ", "Nom des joueurs pour le mode " + (sender as Button).Name, "");
            
            mainWindow = new MainWindow(playerCount);
            mainWindow.Show();
            mainWindow.Closing += new System.ComponentModel.CancelEventHandler(mainWindow_Closing);

            this.Hide();
        }

        void mainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            scoreLabel.Content = (sender as MainWindow).scoreLabel.Content;

            // Met a jour le tableau des scores
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
