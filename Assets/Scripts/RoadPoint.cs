using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts
{
    internal class RoadPoint
    {
        private Vector3 _world;
        private Vector3 _view;
        private Vector4 _project;        // w component utilized for width of road line
        private Vector4 _transform;   // w component utilized for width of road line

        public RoadPoint(Vector3 world) { _world = world; }

        public ref Vector3 World
        {
            get { return ref _world; }
        }

        public ref Vector3 View
        {
            get { return ref _view; }
        }

        public ref Vector4 Project
        {
            get { return ref _project; }
        }

        public ref Vector4 Transform
        {
            get { return ref _transform; }
        }
    }
}
