using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets._P3dEngine
{
    internal static class UnityManager
    {
        public static GameObject CreateGameObject(string name, GameObject parent)
        {
            GameObject newGameObject = new(name);
            newGameObject.transform.SetParent(parent.transform);
            return newGameObject;
        }

        public static GameObject FindChildGameObject(string childName, GameObject parent) 
        {
            if (parent == null)
                return null;

            Transform childTransform = parent.transform.Find(childName);
            return childTransform != null ? childTransform.gameObject : null;
        }
    }
}
