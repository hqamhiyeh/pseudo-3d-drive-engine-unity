using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts
{
    internal class Segment
    {
        public int index;
        public Point p1;
        public Point p2;
        public Color color;

        public Segment(int index)
        {
            this.index = index;
        }

        public Segment(int index, Point p1, Point p2, Color color)
        {
            this.index = index;
            this.p1 = p1;
            this.p2 = p2;
            this.color = color;
        }
    }
}
