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
            while (true)
            {
                if (level != null)
                    level.update();
                Thread.Sleep(1000 / THREAD_FREQ);
            }
        }
    }
}
