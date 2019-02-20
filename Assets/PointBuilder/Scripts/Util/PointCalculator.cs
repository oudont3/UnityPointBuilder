using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EvenIntervalPointBuilder.Util
{
    public class PointCalculator
    {

        /// <summary>
        /// Mesh上のpointを計算
        /// </summary>
        /// <param name="intermediatePoints"></param>
        /// <param name="i"></param>
        /// <param name="meshVertices"></param>
        /// <param name="triangles"></param>
        /// <returns></returns>
        public static List<Vector3> GetPointsOnMeshSurface(List<Vector2> intermediatePoints, int i, Vector3[] meshVertices, int[] triangles)
        {
            var mv1 = meshVertices[triangles[i]];
            var mv2 = meshVertices[triangles[i + 1]];
            var mv3 = meshVertices[triangles[i + 2]];

            var mvP = GetCrossPoint(mv2, mv1, mv3);
            var result = new List<Vector3>();

            foreach (var inP in intermediatePoints)
            {
                // v1-v3上の点
                var meU = Vector3.Lerp(mv1, mv3, Mathf.Abs(inP.x));

                var meV = Vector3.Lerp(mvP, mv2, Mathf.Abs(inP.y)) - mvP;

                result.Add(meV + meU);
            }

            return result;
        }

        /// <summary>
        /// 面の法線計算
        /// </summary>
        /// <param name="i"></param>
        /// <param name="meshVertices"></param>
        /// <param name="triangles"></param>
        /// <returns></returns>
        public static Vector3 GetSurfaceNormal(int i, Vector3[] meshVertices, int[] triangles)
        {
            var mv1 = meshVertices[triangles[i]];
            var mv2 = meshVertices[triangles[i + 1]];
            var mv3 = meshVertices[triangles[i + 2]];

            var v1 = mv2 - mv1;
            var v2 = mv3 - mv1;
            return Vector3.Cross(v1, v2).normalized;
        }

        /// <summary>
        /// 点がuv三点内のどの位置に存在するか、u, vのそれぞれの比率を計算
        /// </summary>
        /// <param name="uvPoints"></param>
        /// <param name="triangleIdx"></param>
        /// <param name="uvs"></param>
        /// <param name="triangles"></param>
        /// <returns></returns>
        public static List<Vector2> GetIntermediatePoint(List<Vector2> uvPoints, int triangleIdx, Vector2[] uvs, int[] triangles)
        {
            Vector3 v1 = uvs[triangles[triangleIdx]];
            Vector3 v2 = uvs[triangles[triangleIdx + 1]];
            Vector3 v3 = uvs[triangles[triangleIdx + 2]];

            var result = new List<Vector2>();

            // 全体面積
            var s = 0.5f * ((v2.x - v1.x) * (v3.y - v1.y) - (v2.y - v1.y) * (v3.x - v1.x));

            //3点の中心と距離
            Vector2 center = Vector2.zero;
            float distance = 0;
            CalcCircleOf2Radius(ref distance, ref center, v1, v2, v3);
            float powDistance = Mathf.Pow(distance, 2);

            foreach (var p in uvPoints)
            {
                // 3点の円内かどうか
                if ((center - p).sqrMagnitude > powDistance)
                    continue;

                // 三角形の中 & 線上
                if (!IsInnerTriangle(p, v1, v2, v3) && !ExistPointOnTriangleEdge(p, v1, v2, v3))
                    continue;

                // vの比(面積比)
                var s1 = 0.5f * ((v3.x - p.x) * (v1.y - p.y) - (v3.y - p.y) * (v1.x - p.x));
                var v = s1 / s;

                // uの比(面積比)
                // 点p とv1-v3上の垂直交点
                var uvP = GetCrossPoint(p, v1, v3); 

                var s2 = 0.5f * ((v1.x - p.x) * (uvP.y - p.y) - (v1.y - p.y) * (uvP.x - p.x));
                var u = s2 / s1;

                result.Add(new Vector2(u, v));
            }

            return result;
        }

        /// <summary>
        /// 3点から円を作成する
        /// http://www.iot-kyoto.com/satoh/2016/01/29/tangent-003/
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <param name="v3"></param>
        /// <param name="distance"></param>
        /// <param name="center"></param>
        public static void CalcCircleOf2Radius(ref float distance, ref Vector2 center, Vector2 v1, Vector2 v2, Vector2 v3)
        {
            float x1 = v1.x;
            float y1 = v1.y;
            float x2 = v2.x;
            float y2 = v2.y;
            float x3 = v3.x;
            float y3 = v3.y;
            //float cx, cy;
            float r1, r2, r3;

            var a = x2 - x1;
            var b = y2 - y1;
            var c = x3 - x1;
            var d = y3 - y1;

            if ((a != 0 && d != 0) || (b != 0 && c != 0))
            {
                var ox = x1 + (d * (a * a + b * b) - b * (c * c + d * d)) / (a * d - b * c) / 2;
                float oy = 0;
                if (b != 0)
                {
                    oy = (a * (x1 + x2 - ox - ox) + b * (y1 + y2)) / b / 2;
                }
                else
                {
                    oy = (c * (x1 + x3 - ox - ox) + d * (y1 + y3)) / d / 2;
                }
                r1 = Mathf.Sqrt((ox - x1) * (ox - x1) + (oy - y1) * (oy - y1));
                r2 = Mathf.Sqrt((ox - x2) * (ox - x2) + (oy - y2) * (oy - y2));
                r3 = Mathf.Sqrt((ox - x3) * (ox - x3) + (oy - y3) * (oy - y3));

                center.x = ox;
                center.y = oy;
                distance = (r1 + r2 + r3) / 3;
            }
        }


        /// <summary>
        /// 三角形内部チェック
        /// http://esprog.hatenablog.com/entry/2016/05/08/165445
        /// </summary>
        /// <param name="p"></param>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <param name="v3"></param>
        /// <returns></returns>
        public static bool IsInnerTriangle(Vector3 p, Vector3 v1, Vector3 v2, Vector3 v3)
        {
            var n1 = Vector3.Cross(v1 - v3, p - v1).normalized;
            var n2 = Vector3.Cross(v2 - v1, p - v2).normalized;
            var n3 = Vector3.Cross(v3 - v2, p - v3).normalized;

            return 0.999f < Vector3.Dot(n1, n2) && 0.999f < Vector3.Dot(n2, n3);
        }

        //  
        /// <summary>
        /// 点pと直線上の垂直交点
        /// http://sampo.hatenadiary.jp/entry/20070626/p1
        /// </summary>
        /// <param name="p"></param>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns></returns>
        public static Vector3 GetCrossPoint(Vector3 p, Vector3 v1, Vector3 v2)
        {
            var a = v2 - v1;
            var b = p - v1;
            var dot = Vector3.Dot(a, b);
            var adist = dot / Mathf.Pow(a.magnitude, 2);
            return v1 + adist * a;
        }

        /// <summary>
        /// 点pが辺(v1,v2)上に存在するかどうかを調査する
        /// http://esprog.hatenablog.com/entry/2016/10/10/002656
        /// </summary>
        /// <param name="p">調査点</param>
        /// <param name="v1">辺をなす頂点</param>
        /// <param name="v2">辺をなす頂点</param>
        /// <returns>点pが辺上に存在しているかどうか</returns>
        public static bool ExistPointOnEdge(Vector3 p, Vector3 v1, Vector3 v2, float tolerance = 0.001f)
        {
            return 1 - tolerance < Vector3.Dot(v2 - p, v2 - v1);
        }

        /// <summary>
        /// 点pが与えられた3点がなす三角形の辺上に存在するかを調査する
        /// http://esprog.hatenablog.com/entry/2016/10/10/002656
        /// </summary>
        /// <param name="p">調査点p</param>
        /// <param name="t1">三角形をなす頂点</param>
        /// <param name="t2">三角形をなす頂点</param>
        /// <param name="t3">三角形をなす頂点</param>
        /// <returns>点pが三角形の辺城に存在するかどうか</returns>
        public static bool ExistPointOnTriangleEdge(Vector3 p, Vector3 t1, Vector3 t2, Vector3 t3)
        {
            if (ExistPointOnEdge(p, t1, t2) || ExistPointOnEdge(p, t2, t3) || ExistPointOnEdge(p, t3, t1))
                return true;
            return false;
        }
    }

}
