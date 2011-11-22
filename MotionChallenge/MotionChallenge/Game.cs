using OpenTK;
using OpenTK.Graphics.OpenGL;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;

namespace MotionChallenge
{
    class Game
    {
        private Level level;
        private System.Timers.Timer timer;

        DateTime timeRef = DateTime.Now;
        
        private const int THREAD_FREQ = 30;

        public Game(GLControl glControl)
        {
            level = new Level(glControl, /*playerCount*/1);

            // create game loop routine
            timer = new System.Timers.Timer();
            timer.Interval = 1000 / THREAD_FREQ;
            timer.Elapsed += new ElapsedEventHandler(timerRoutine);
            timer.Start();
        }

        public void stopGame()
        {
            // kill game timer
            if (timer != null)
            {
                timer.Stop();
                timer.Close();
            }
        }

        private void timerRoutine(object sender, ElapsedEventArgs e)
        {
            DateTime now = DateTime.Now;
            level.update((int)now.Subtract(timeRef).TotalMilliseconds);
            timeRef = now;
        }
    }
}
