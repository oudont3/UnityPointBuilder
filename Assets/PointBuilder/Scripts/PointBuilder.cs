using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;


namespace EvenIntervalPointBuilder
{
    using Util;

    public class PointBuilder
    {
        public enum UVType
        {
            UV,
            UV2
        }

        /// <summary>
        /// UV情報を利用しMesh上に等間隔に点を配置
        /// </summary>
        /// <param name="data"></param>
        /// <param name="baseMesh"></param>
        /// <param name="evenIntervalOnUV"></param>
        /// <param name="uvType"></param>
        public Data.PointData BuildPointOnMesh(Mesh baseMesh, List<Vector2> evenIntervalOnUV, UVType uvType)
        {
            var data = ScriptableObject.CreateInstance<Data.PointData>();

            var result = new List<Vector3>();
            var normals = new List<Vector3>();

            Vector2[] uvs;
            if(uvType == UVType.UV)
            {
                uvs = baseMesh.uv;
            }
            else
            {
                uvs = baseMesh.uv2;
            }

            Vector3[] meshVertices = baseMesh.vertices;
            for (int subMesh = 0; subMesh < baseMesh.subMeshCount; subMesh++)
            {
                int[] submeshTriangles = baseMesh.GetTriangles(subMesh);
                for (int i = 0; i < submeshTriangles.Length; i += 3)
                {
                    var calcedUvs = PointCalculator.GetIntermediatePoint(evenIntervalOnUV, i, uvs, submeshTriangles);

                    var meshed = PointCalculator.GetPointsOnMeshSurface(calcedUvs, i, meshVertices, submeshTriangles);

                    var surfNormal = PointCalculator.GetSurfaceNormal(i, meshVertices, submeshTriangles);
                    foreach (var m in meshed)
                    {
                        normals.Add(surfNormal);
                    }

                    result.AddRange(meshed);
                }
            }

            data.points = result;
            data.normals = normals;
            return data;
        }

        /// <summary>
        /// 2次元の0～1間の等間隔の点生成
        /// </summary>
        /// <param name="division"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public List<Vector2> CreateEvenIntervalPoints(Vector2Int division, Vector2 offset)
        {
            if (division.x <= 0 || division.y <= 0)
            {
                return new List<Vector2>();
            }

            var result = new List<Vector2>();
            float uStep = 1 / (float)division.x;
            float vStep = 1 / (float)division.y;
            for (float u = offset.x; u <= 1; u += uStep)
            {
                for (float v = offset.y; v <= 1; v += vStep)
                {
                    result.Add(new Vector2(u, v));
                }
            }
            return result;
        }

        /*
        private void BuildIntermediatePoint(SkinnedMeshInCoordinateData data, Mesh targetMesh, List<Vector2> addUvs,  UVType uvType)
        {
            var result = new List<Vector2>();
            var meshIndex1 = new List<int>();
            var meshIndex2 = new List<int>();
            var meshIndex3 = new List<int>();

            Vector2[] uvs;
            if (uvType == UVType.UV)
            {
                uvs = targetMesh.uv;
            }
            else
            {
                uvs = targetMesh.uv2;
            }
            Vector3[] meshVertices = targetMesh.vertices;
            for (int subMesh = 0; subMesh < targetMesh.subMeshCount; subMesh++)
            {
                int[] submeshTriangles = targetMesh.GetTriangles(subMesh);
                for (int i = 0; i < submeshTriangles.Length; i += 3)
                {
                    var calcedUvs = MeshCalculator.GetIntermediatePoint(addUvs, i, uvs, submeshTriangles);

                    result.AddRange(calcedUvs);

                    foreach (var cuv in calcedUvs)
                    {
                        meshIndex1.Add(submeshTriangles[i]);
                        meshIndex2.Add(submeshTriangles[i + 1]);
                        meshIndex3.Add(submeshTriangles[i + 2]);
                    }
                }
            }

            data.coordinate = result;
            data.meshIndex1 = meshIndex1;
            data.meshIndex2 = meshIndex2;
            data.meshIndex3 = meshIndex3;
        }
        */

    }
}

