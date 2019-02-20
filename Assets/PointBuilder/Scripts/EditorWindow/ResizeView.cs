using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace EvenIntervalPointBuilder.Editor
{
    /// <summary>
    /// https://answers.unity.com/questions/546686/editorguilayout-split-view-resizable-scroll-view.html
    /// </summary>
    public class ResizeView
    {
        private EditorWindow _window;

        private bool resize = false;

        private float _y;
        public float Y
        {
            get { return _y; }
            set { _y = value; }
        }

        public ResizeView(EditorWindow window, float y)
        {
            _window = window;
            _y = y;
        }

        public void ShowGUI(Rect rect)
        {
            GUI.DrawTexture(rect, EditorGUIUtility.whiteTexture);
            EditorGUIUtility.AddCursorRect(rect, MouseCursor.ResizeVertical);

            if (Event.current.type == EventType.MouseDown && rect.Contains(Event.current.mousePosition))
                resize = true;

            if (resize)
            {
                _y = Event.current.mousePosition.y;
                _window.Repaint();
            }

            if (Event.current.type == EventType.MouseUp)
                resize = false;
        }

    }
}