﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;

namespace EvenIntervalPointBuilder.Sample
{
    public class PointBuilderSimpleRender : MonoBehaviour
    {
        private struct DrawPointData
        {
            public Vector3 Position;
            public Vector3 Normals;
            public Vector3 EulerAngles;
        }

        public Data.PointData _data;
        public Transform _parent;

        public Bounds _drawMeshBounds = new Bounds(Vector3.zero, Vector3.one);
        public Mesh _drawMesh;
        public Material _drawMaterial;
        public Vector3 _drawMeshEuler = new Vector3(0, 0, 0);

        private ComputeBuffer _pointDataBuffer;
        private ComputeBuffer _argsBuffer;

        private uint[] _GPUInstancingArgs = new uint[5] { 0, 0, 0, 0, 0 };

        private int PROPERTY_ID_POINT_DATA_BUFFER = Shader.PropertyToID("_PointDataBuffer");
        private int PROPERTY_ID_LOCAL_TO_WORLD_MAT = Shader.PropertyToID("_LocalToWorldMat");
        private int PROPERTY_ID_PARENT_EULER = Shader.PropertyToID("_ParentEuler");
        private int PROPERTY_ID_EULER = Shader.PropertyToID("_Euler");

        // Start is called before the first frame update
        void Start()
        {
            InitBuffer();
        }

        // Update is called once per frame
        void LateUpdate()
        {
            UpdateBuffer();
        }

        private void OnDestroy()
        {
            if (_pointDataBuffer != null) _pointDataBuffer.Release();
            if (_argsBuffer != null) _argsBuffer.Release();
        }

        private void InitBuffer()
        {
            if (_data == null || _data.points.Count <= 0) return;
            int size = _data.points.Count;

            // バッファ生成
            _pointDataBuffer = new ComputeBuffer(size, Marshal.SizeOf(typeof(DrawPointData)));
            _argsBuffer = new ComputeBuffer(1, _GPUInstancingArgs.Length * sizeof(uint), ComputeBufferType.IndirectArguments);

            var dataArr = new DrawPointData[size];
            for (int i = 0; i < dataArr.Length; ++i)
            {
                dataArr[i].Position = _data.points[i];
                dataArr[i].Normals = _data.normals[i];

                var q = Quaternion.LookRotation(dataArr[i].Normals);
                dataArr[i].EulerAngles = q.eulerAngles * Mathf.Deg2Rad;

            }
            _pointDataBuffer.SetData(dataArr);

        }

        private void UpdateBuffer()
        {
            if (_data == null || _data.points.Count <= 0) return;

            if (_parent == null) _parent = transform;

            _GPUInstancingArgs[0] = (uint)_drawMesh.GetIndexCount(0);
            _GPUInstancingArgs[1] = (uint)_data.points.Count;
            _GPUInstancingArgs[2] = (uint)_drawMesh.GetIndexStart(0);
            _GPUInstancingArgs[3] = (uint)_drawMesh.GetBaseVertex(0);
            _argsBuffer.SetData(_GPUInstancingArgs);

            _drawMaterial.SetBuffer(PROPERTY_ID_POINT_DATA_BUFFER, _pointDataBuffer);
            _drawMaterial.SetMatrix(PROPERTY_ID_LOCAL_TO_WORLD_MAT, _parent.localToWorldMatrix);

            _drawMaterial.SetVector(PROPERTY_ID_PARENT_EULER, _parent.rotation.eulerAngles * Mathf.Deg2Rad);
            _drawMaterial.SetVector(PROPERTY_ID_EULER, _drawMeshEuler * Mathf.Deg2Rad);


            Graphics.DrawMeshInstancedIndirect(_drawMesh, 0, _drawMaterial, _drawMeshBounds, _argsBuffer);
        }
    }
}

