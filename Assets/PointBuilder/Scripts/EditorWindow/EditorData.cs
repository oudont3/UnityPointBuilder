using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EvenIntervalPointBuilder.Editor.Data
{
    public class EditorData : ScriptableObject
    {
        public Mesh baseMesh;
        public Vector2Int uvDivision = new Vector2Int(50, 50);
        public Vector2 uvOffset = Vector2.zero;
        public PointBuilder.UVType uvType = PointBuilder.UVType.UV2;
        public string exportDir = "Assets/PointBuilder/Data/Build";
        public string exportNamePrefix = "";

        public float wireSize = 0.05f;
        public Color wireColor = Color.white;
        public Color pointColor = new Color(0, 1, 1);
    }
}
