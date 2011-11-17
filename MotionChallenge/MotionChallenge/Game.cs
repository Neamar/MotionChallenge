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
        
        private const int THREAD_FREQ = 10;

        public Game()
        {
            level = new Level(/*playerCount*/1);

            gameThread = new Thread(new ThreadStart(threadRoutine));
            gameThread.Name = "MotionChallenge.GameThread";
            gameThread.Start();
            Console.WriteLine("Game thread started");
        }

        public void stopGame()
        {
            if (gameThread != null)
            {
                gameThread.Abort();
                Console.WriteLine("Game thread killed");
                gameThread = null;
            }
        }

        private void threadRoutine()
        {
            while (true)
            {
                if (level != null)
                    level.update();

                Thread.Sleep(1000 / THREAD_FREQ);
            }
        }
    }
}
