using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MotionChallenge
{
    class Wall
    {
        public Wall()
        {
            //TODO Load random wall
        }

        public bool atEndOfLine()
        {
            //TODO

            return false; 
        }

        public void update(int elapsed)
        {
            if (!atEndOfLine())
            {
                // update wall position
                // update 3D stage
            }
        }
    }
}
