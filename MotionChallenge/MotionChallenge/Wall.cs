using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MotionChallenge
{
    class Wall
    {
        int position = 0;

        public Wall()
        {
            //TODO Load random wall
        }

        public bool atEndOfLine()
        {
            //TODO

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
                position += 5;
            }
            else
            {
                position = 0;
            }
        }
    }
}
