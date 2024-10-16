using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;

namespace Assets.Scripts
{
    internal class Road
    {
        private readonly List<RoadPoint> _points;
        public int SegmentLength { get; private set; }  // Number of lines per segment
        public int SegmentCount { get; private set; }   // Number of segments
        public int Width { get; private set; }          // Width in pixels

        public Road(int segmentLength, int segmentCount, int width)
        {
            _points = new List<RoadPoint>();
            SegmentLength = segmentLength;
            SegmentCount = segmentCount;
            Width = width;
            GenerateRoad();
        }

        public IReadOnlyList<RoadPoint> Points { get => _points; }

        public RoadPoint this[int i] { get => _points[i]; }

        private void GenerateRoad()
        {
            for (int i = 0; i < (SegmentCount + 1); i++)
                _points.Add(new RoadPoint(new Vector3(0.0f, 0.0f, (i + 1) * SegmentLength)));
        }
    }
}
