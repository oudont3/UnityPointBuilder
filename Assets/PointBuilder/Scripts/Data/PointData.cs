using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace EvenIntervalPointBuilder.Data
{
    public class PointData : ScriptableObject
    {
        public List<Vector3> points;
        public List<Vector3> normals;
        public Vector2 uvDivision;
        public Vector2 uvOffset;
    }

}