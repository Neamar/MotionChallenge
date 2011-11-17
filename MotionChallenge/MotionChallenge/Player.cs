using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MotionChallenge
{
    /*
     * PLAYER
     * 
     * 
     */
    class Player
    {
        private int count;

        public Player(int playerCount)
        {
            count = playerCount;
        }

        public void update()
        {
            // draw player(s) position
        }
        
        public int percentOut(Wall wall)
        {
            // comparse player's body and hole in the wall
            // return value between 0 and 100

            return 0;
        }

    }
}
