using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MotionChallenge
{
    /*
     * LEVEL
     * 
     *  Manage walls.
     * 
     */
    class Level
    {
        private Player player;
        private Wall wall;
        private const int HOLE_THRESHOLD = 10;

        public Level(int playerCount)
        {
           player = new Player(playerCount);
        }

        public void update(int elapsed)
        {
            if (wall == null)
                wall = new Wall();

            // update wall position
            wall.update(elapsed);

            if (wall.atEndOfLine())
            {
                // check player
                if (player.percentOut(wall) >= HOLE_THRESHOLD)
                {
                    // Not Ok: Game Over
                }
                else
                {
                    // Ok: increase score, new wall, etc
                }
            }
        }

        public Wall getWall()
        {
            return wall;
        }
    }
}
