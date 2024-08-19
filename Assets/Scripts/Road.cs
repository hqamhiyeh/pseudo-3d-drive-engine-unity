using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts
{
    internal class Road
    {
        private List<Point> _points;
        private int _segmentLength;
        private int _segmentCount;
        private int _width;
        private int _length;

        public Road(int segmentCount, int segmentLength, int roadWidth) 
        {
            _points = new List<Point>();
            _segmentCount = segmentCount;
            _segmentLength = segmentLength;
            _width = roadWidth;
            Generate();
        }

        private void Generate()
        {
            for (int i = 0; i < _segmentCount; i++)
                _points.Add(new Point(new Vector3(0.0f, 0.0f, (i + 1) * _segmentLength)));
        }


    }
}
