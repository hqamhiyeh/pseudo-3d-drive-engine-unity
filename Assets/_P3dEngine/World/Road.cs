using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;

namespace Assets._P3dEngine
{
    [System.Serializable]
    internal class Road
    {
        private bool IsInitialized = false;
        private List<RoadPoint> _points;
        [field: SerializeField] public int SegmentLength { get; private set; }  // Number of lines per segment
        [field: SerializeField] public int SegmentCount { get; private set; }   // Number of segments
        [field: SerializeField] public int Width { get; private set; }          // Width in pixels
        [field: SerializeField] public List<Material> Materials { get; private set; }

        public Road(int segmentLength, int segmentCount, int width)
        {
            _points = new List<RoadPoint>();
            SegmentLength = segmentLength;
            SegmentCount = segmentCount;
            Width = width;
            GenerateRoad();
            Materials = new List<Material>();
        }

        public IReadOnlyList<RoadPoint> Points { get => _points; }

        public RoadPoint this[int i] { get => _points[i]; }

        public void Initialize()
        {
            if (IsInitialized)
                return;

            _points = new List<RoadPoint>();
            GenerateRoad();
            IsInitialized = true;
        }

        private void GenerateRoad()
        {
            for (int i = 0; i < (SegmentCount + 1); i++)
                _points.Add(new RoadPoint(new Vector3(0.0f, 0.0f, (i + 1) * SegmentLength)));
        }

        public void AddMaterial(Material material)
        {
            Materials.Add(material);
        }
    }
}
