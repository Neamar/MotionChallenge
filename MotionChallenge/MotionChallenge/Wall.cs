using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MotionChallenge
{
    class Wall
    {
        private int position = 0;
        private int addPerUpdate = 5;

        public Wall()
        {
            //TODO Load random wall
        }

        public bool atEndOfLine()
        {
            return (position >= 1000); 
        }

        public int getPosition()
        {
            return position;
        }

        public void update(int elapsed)
        {
            if (!atEndOfLine())
            {
                // update wall position
                position += addPerUpdate;
            }
            else
            {
                position = 0;
            }
        }
    }
}
