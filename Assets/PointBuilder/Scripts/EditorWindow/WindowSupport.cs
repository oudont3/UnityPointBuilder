using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

namespace EvenIntervalPointBuilder.Editor
{
    public class WindowSupport
    {
        public static Mesh CreateUVMesh(Mesh baseMesh, PointBuilder.UVType uvType)
        {
            Mesh m = new Mesh
            {
                vertices = baseMesh.vertices,
                triangles = baseMesh.triangles,
                uv = baseMesh.uv,
                uv2 = baseMesh.uv2,
                uv3 = baseMesh.uv3,
                uv4 = baseMesh.uv4,
                uv5 = baseMesh.uv5,
                uv6 = baseMesh.uv6,
                uv7 = baseMesh.uv7,
                uv8 = baseMesh.uv8,
                normals = baseMesh.normals,
                colors = baseMesh.colors,
                tangents = baseMesh.tangents
            };

            Vector2[] uvs;
            if (uvType == PointBuilder.UVType.UV)
            {
                uvs = m.uv;
            }
            else
            {
                uvs = m.uv2;
            }

            var newv = new Vector3[m.vertices.Length];
            for (int i = 0; i < m.vertices.Length; i++)
            {
                newv[i] = new Vector3(uvs[i].x, uvs[i].y, 0);
            }
            m.vertices = newv;

            return m;
        }

        public static Mesh CreateMeshFromPointOnMesh(EvenIntervalPointBuilder.Data.PointData pData)
        {
            Mesh m = new Mesh();

            var vertexes = new Vector3[pData.points.Count];
            var indexes = new int[pData.points.Count];
            for (int i = 0; i < pData.points.Count; i++)
            {
                vertexes[i] = pData.points[i];
                indexes[i] = i;
            }
            m.vertices = vertexes;
            m.SetIndices(indexes, MeshTopology.Points, 0);

            return m;
        }

        public static Mesh CreateMeshFromPointOnUV(List<Vector2> uvPoints)
        {
            Mesh m = new Mesh();

            var vertexes = new Vector3[uvPoints.Count];
            var indexes = new int[uvPoints.Count];
            for (int i = 0; i < uvPoints.Count; i++)
            {
                vertexes[i] = new Vector3(uvPoints[i].x, uvPoints[i].y, 0);
                indexes[i] = i;
            }
            m.vertices = vertexes;
            m.SetIndices(indexes, MeshTopology.Points, 0);

            return m;
        }

        public static string FindExportNameTake(string dir, string prefix)
        {
            string[] files = System.IO.Directory.GetFiles(
                dir, "*.asset", System.IO.SearchOption.TopDirectoryOnly);

            int max = 0;

            foreach (var f in files)
            {
                Match m = Regex.Match(f, prefix + @"(\d+)\.asset$");
                if (m.Success && m.Groups.Count <= 1) continue;

                int v = 0;
                int.TryParse(m.Groups[1].Value, out v);

                if (v > max) max = v;
            }
            return prefix + (max + 1).ToString();
        }
    }

}