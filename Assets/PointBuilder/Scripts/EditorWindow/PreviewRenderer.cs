using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace EvenIntervalPointBuilder.Editor
{
    public class PreviewRenderer
    {
        private PreviewRenderUtility _renderer;
        public Camera Camera
        {
            get { return _renderer.camera; }
        }

        public PreviewRenderer()
        {
        }

        void InitRenderer()
        {
            _renderer = new PreviewRenderUtility();

            _renderer.camera.nearClipPlane = 0.3f;
            _renderer.camera.farClipPlane = 1000;
            _renderer.camera.transform.position = new Vector3(0, 1, -1);
            _renderer.camera.transform.rotation = Quaternion.Euler(0, 0, 0);
            _renderer.camera.clearFlags = CameraClearFlags.SolidColor;
            _renderer.camera.orthographic = false;
        }

        public void Draw(Rect rect, Action<PreviewRenderUtility> draw)
        {
            if (_renderer == null) InitRenderer();

            _renderer.BeginPreview(rect, GUIStyle.none);

            draw?.Invoke(_renderer);

            _renderer.camera.Render();
            _renderer.EndAndDrawPreview(rect);

        }

        public void Release()
        {
            _renderer?.Cleanup();
        }

        ~PreviewRenderer()
        {
            Release();
        }
    }

}