using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace MotionChallenge
{
    class Game
    {
        private Level level;
        private Thread gameThread;
        
        private const int THREAD_FREQ = 35;

        public Game()
        {
            level = new Level(/*playerCount*/1);

            // create game thread
            gameThread = new Thread(new ThreadStart(threadRoutine));
            gameThread.Name = "MotionChallenge.GameThread";
            gameThread.Start();
        }

        public void stopGame()
        {
            // kill game thread
            if (gameThread != null)
            {
                gameThread.Abort();
                gameThread.Join();
                Console.WriteLine("Game thread killed");
            }
        }

        private void threadRoutine()
        {
            Console.WriteLine("Game thread started");
            DateTime timeRef = DateTime.Now;

            while (true)
            {
                DateTime now = DateTime.Now;

                // Update level and OpenGL
                if (level != null)
                    level.update((int)now.Subtract(timeRef).TotalMilliseconds);
                MainWindow.getInstance().getGLControl().Invalidate();

                timeRef = now;
                Thread.Sleep(1000 / THREAD_FREQ);
            }
        }

        public Level getLevel()
        {
            return level;
        }
    }
}
