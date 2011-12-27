using OpenTK;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;

namespace MotionChallenge
{
	/**
	 * Cette classe représente une partie de MotionChallenge.
	 * Elle contient un Niveau, ainsi qu'un système de "tick" pour faire évoluer le monde.
	 */
    class Game
    {
        private Level level;
        private System.Timers.Timer timer;
        private System.Boolean isRunning;

        DateTime timeRef = DateTime.Now;

		/*
		 * À quelle fréquence (en Hz) le jeu doit évoluer
		 */
        private const int THREAD_FREQ = 30;

		/**
		 * Initialise un jeu.
		 */
        public Game(GLControl glControl, int playerCount)
        {
            level = new Level(glControl, playerCount);

            //Mettre en place le timer pour faire évoluer le monde
            timer = new System.Timers.Timer();
            timer.Interval = 1000 / THREAD_FREQ;
            timer.Elapsed += new ElapsedEventHandler(timerRoutine);
            timer.Start();
            isRunning = true;
        }

        /**
		 * Arrêter le jeu
		 */
        public void stopGame()
        {
            // Terminer le timer proprement
            if (timer != null)
            {
                timer.Stop();
                timer.Close();
            }

            level.reset();
        }

        /**
		 * Alterner entre la pause et le jeu selon l'état actuel.
		 */
        public void togglePause()
        {
            if (isRunning)
            {
                timer.Stop();
            }
            else
            {
                timer.Start();
            }
            isRunning = !isRunning;
        }

        /**
		 * Faire évoluer le monde d'une itération en lui indiquant combien de temps s'est écoulé depuis le dernier appel.
		 */
        private void timerRoutine(object sender, ElapsedEventArgs e)
        {
            DateTime now = DateTime.Now;
            level.update((int)now.Subtract(timeRef).TotalMilliseconds);
            timeRef = now;
        }
    }
}
