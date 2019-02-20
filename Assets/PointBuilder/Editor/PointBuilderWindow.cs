using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;

/// <summary>
/// memo sphere 26 * 26 0.02 * 0.01
/// </summary>

namespace EvenIntervalPointBuilder.Editor
{
    public class PointBuilderWindow : EditorWindow
    {
        private const float ITEM_WIDTH = 350;

        // Data
        private Data.EditorData _property;

        // UV Preview
        private Mesh _uvWireMesh;
        private Mesh _uvPointMesh;
        private Material _uvWireMat;
        private Material _uvPointMat;
        public Vector3 _uvPointPosition = new Vector3(-0.5f, 0.51f, 4);
        private Vector3 _defaultUvPreviewPosition = new Vector3(-0.5f, 0.51f, 4);

        // Mesh Preview
        private Material _meshPointMat;
        private Mesh _meshPointMesh;
        public Vector3 _meshPointPosition = new Vector3(0, 1.75f, 10);
        public Vector3 _meshPointMeshPreviewEuler = new Vector3(-15, 15, -2.5f);
        private Vector3 _defaultPointMeshPreviewPosition = new Vector3(0, 1.75f, 10);
        private Vector3 _defaultPointMeshPreviewEuler = new Vector3(-15, 15, -2.5f);

        // Instances
        private PreviewRenderer _uvRenderer;
        private PreviewRenderer _buildRenderer;
        private ResizeView _resizeView;
        private PointBuilder _pointBuilder;

        // Intermediate data
        private List<Vector2> _evenIntervalPoints;

        // Inner Values
        private bool _repaint = false;


        [MenuItem("PointBuilder/Open Editor")]
        public static void Open()
        {
            var w = EditorWindow.GetWindow<PointBuilderWindow>("PointBuilder");
            w.minSize = new Vector2(640, 640);
            w.wantsMouseMove = true;
        }

        private void OnEnable()
        {
            var path = "Assets/PointBuilder/Data/Editor/PrevData.asset";
            var asset = AssetDatabase.LoadAssetAtPath<Data.EditorData>(path);
            if (asset == null)
            {
                asset = CreateInstance<Data.EditorData>();
                AssetDatabase.CreateAsset(asset, path);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
            _property = asset;

            _pointBuilder = new PointBuilder();

            _uvRenderer = new PreviewRenderer();
            _buildRenderer = new PreviewRenderer();

            _uvWireMat = new Material(Shader.Find("PointBuilder/Editor/Wireframe"));
            _uvWireMat.color = _property.wireColor;
            _uvPointMat = new Material(Shader.Find("PointBuilder/Editor/SimplePoint"));
            _uvPointMat.color = _property.pointColor;
            _meshPointMat = new Material(Shader.Find("PointBuilder/Editor/SimplePoint"));
            _meshPointMat.color = _property.pointColor;

            _repaint = false;

            // load preview
            OnChangeMeshUV();
            OnChangePointBuilderSettings();
            OnChangePreviewMaterial();

        }

        public void OnGUI()
        {
            OnGUIEvent();

            OnGUIRightSide();

            OnGUILeftSide();

            OnGUILast();
        }

        private void OnDisable()
        {
            EditorUtility.SetDirty(_property);
            _uvRenderer?.Release();
            _buildRenderer?.Release();
        }

        private void OnGUILast()
        {
            if(_repaint)
            {
                // eventからの操作の場合描画タイミングがずれるので強制描画
                Repaint();
                _repaint = false;
            }
        }

        private void OnGUILeftSide()
        {
            var defaultRect = EditorGUILayout.GetControlRect(true);
            var width = position.width - ITEM_WIDTH;
            var rect = new Rect(0, 0, width, defaultRect.height);
            var resizeHeight = 5;

            if (_resizeView == null)
                _resizeView = new ResizeView(this, position.height / 2f + 15);

            // Title
            EditorGUI.LabelField(rect, "Preview", EditorStyles.boldLabel);

            // UV Preview
            rect.y += defaultRect.height;
            rect.height = _resizeView.Y - defaultRect.height;

            OnGUIDrawUVPreview(rect);

            // Resize
            rect.y = _resizeView.Y;
            rect.height = resizeHeight;
            _resizeView.ShowGUI(rect);

            // Mesh Point Preview
            rect.y = _resizeView.Y + resizeHeight;
            rect.height = position.height;

            OnGUIDrawMeshPreview(rect);
        }

        private void OnGUIRightSide()
        {
            var defaultRect = EditorGUILayout.GetControlRect(true);
            var itemX = position.width - ITEM_WIDTH + 2;
            var itemWidth = ITEM_WIDTH - 4;
            var itemHeight = defaultRect.height;

            var rect = new Rect(itemX, 2, itemWidth, itemHeight);

            // Parameter Draw
            OnGUIDrawItems(rect, () =>
            {
                rect.y += itemHeight + 2;
                return rect;
            });
        }

        private void OnGUIEvent()
        {
            // Preview View Mouse Event
            if (Event.current.mousePosition.x > position.width - ITEM_WIDTH)
            {
                return;
            }

            if (Event.current.type == EventType.ScrollWheel)
            {
                //Debug.Log(Event.current.delta);
                if(Event.current.mousePosition.y < _resizeView.Y)
                {
                    _uvPointPosition.z += Event.current.delta.y * 0.1f;
                }
                else
                {
                    _meshPointPosition.z += Event.current.delta.y * 0.1f;
                }

                _repaint = true;
            }
            else if (Event.current.type == EventType.MouseDrag)
            {
                if (Event.current.delta.x > 100 || Event.current.delta.y > 100) return;

                if(Event.current.button == 2)
                {
                    if (Event.current.mousePosition.y < _resizeView.Y)
                    {
                        _uvPointPosition.x += Event.current.delta.x * 0.002f;
                        _uvPointPosition.y -= Event.current.delta.y * 0.002f;
                    }
                    else
                    {
                        _meshPointPosition.x += Event.current.delta.x * 0.002f;
                        _meshPointPosition.y -= Event.current.delta.y * 0.002f;
                    }
                }
                else if(Event.current.button == 1)
                {
                    if (Event.current.mousePosition.y >= _resizeView.Y)
                    {
                        _meshPointMeshPreviewEuler.y -= Event.current.delta.x * 0.1f;
                        _meshPointMeshPreviewEuler.x -= Event.current.delta.y * 0.1f;
                    }
                }

                _repaint = true;
            }
        }

        private void OnGUIDrawItems(Rect rect, Func<Rect> next)
        {

            EditorGUI.LabelField(rect, "Point Settings", EditorStyles.boldLabel);

            EditorGUIChangeCheck(() =>
            {
                // Mesh
                rect = next();
                _property.baseMesh = (Mesh)EditorGUI.ObjectField(rect, "Target Mesh", _property.baseMesh, typeof(Mesh), true);

                // UV Type
                rect = next();
                _property.uvType = (PointBuilder.UVType)EditorGUI.EnumPopup(rect, "UV Type", _property.uvType);

            }, OnChangeMeshUV);

            EditorGUIChangeCheck(() =>
            {
                // uv interval
                rect = next();
                _property.uvDivision = EditorGUI.Vector2IntField(rect, "UV Division", _property.uvDivision);

                rect = next();
                rect = next();
                _property.uvOffset = EditorGUI.Vector2Field(rect, "UV Offset", _property.uvOffset);

            }, OnChangePointBuilderSettings);

            // preview

            rect = next();
            rect = next();
            rect = next();
            EditorGUI.LabelField(rect, "Preview", EditorStyles.boldLabel);

            /*
            rect.y += itemHeight + 2;
            _uvPreviewPosition = EditorGUI.Vector3Field(rect, "UV Position", _uvPreviewPosition);

            rect.y += itemHeight * 2 + 2;
            _pointMeshPreviewPosition = EditorGUI.Vector3Field(rect, "Point Position", _pointMeshPreviewPosition);

            rect.y += itemHeight * 2 + 2;
            _pointMeshPreviewEuler = EditorGUI.Vector3Field(rect, "Point Euler", _pointMeshPreviewEuler);
            */

            EditorGUIChangeCheck(() =>
            {
                rect = next();
                _property.wireSize = EditorGUI.Slider(rect, "Wire Size", _property.wireSize, 0, 0.34f);

                //_pointSize = EditorGUI.IntSlider(rect, "Point Size", _pointSize, 1, 10);

                rect = next();
                _property.wireColor = EditorGUI.ColorField(rect, "Wire Color", _property.wireColor);

                rect = next();
                _property.pointColor = EditorGUI.ColorField(rect, "Point Color", _property.pointColor);

            }, OnChangePreviewMaterial);

            rect = next();
            if (GUI.Button(rect, "Preview Camera Reset"))
            {
                OnClickPreviewResetButton();
            }

            rect = next();
            if (GUI.Button(rect, "Build Preview"))
            {
                OnClickPreviewButton();
            }

            // Export
            EditorGUIChangeCheck(() =>
            {
                rect = next();
                rect = next();
                EditorGUI.LabelField(rect, "Export", EditorStyles.boldLabel);

                rect = next();
                _property.exportDir = EditorGUI.TextField(rect, "Export Dir", _property.exportDir);

                rect = next();
                _property.exportNamePrefix = EditorGUI.TextField(rect, "Export Name Prefix", _property.exportNamePrefix);

            }, OnChangeProperty);

            rect = next();
            if (GUI.Button(rect, "Export Point Mesh"))
            {
                OnClickExportPointMesh();
            }
        }

        private void EditorGUIChangeCheck(Action main, Action changed)
        {
            EditorGUI.BeginChangeCheck();

            main();

            if (EditorGUI.EndChangeCheck())
                changed();
        }

        private void OnGUIDrawUVPreview(Rect rect)
        {
            _uvRenderer.Draw(rect, (renderer) =>
            {
                if (_uvWireMesh != null)
                {
                    renderer.DrawMesh(
                        _uvWireMesh, 
                        _uvPointPosition + new Vector3(0, 0, 0.01f), 
                        Quaternion.Euler(0, 0, 0), 
                        _uvWireMat, 
                        0);
                }
                if (_uvPointMesh != null)
                {
                    renderer.DrawMesh(
                        _uvPointMesh, 
                        _uvPointPosition, 
                        Quaternion.Euler(0, 0, 0), 
                        _uvPointMat, 
                        0);
                }
            });
        }

        private void OnGUIDrawMeshPreview(Rect rect)
        {
            _buildRenderer.Draw(rect, (renderer) =>
            {
                if (_meshPointMesh != null)
                {
                    renderer.DrawMesh(
                        _meshPointMesh, 
                        _meshPointPosition, 
                        Quaternion.Euler(_meshPointMeshPreviewEuler),
                        _meshPointMat, 
                        0);
                }
            });
        }

        private void OnClickPreviewButton()
        {
            var pointData = _pointBuilder.BuildPointOnMesh(_property.baseMesh, _evenIntervalPoints, _property.uvType);
            _meshPointMesh = WindowSupport.CreateMeshFromPointOnMesh(pointData);
        }

        private void OnClickExportPointMesh()
        {
            if (_property.baseMesh == null) return;

            var pointData = _pointBuilder.BuildPointOnMesh(_property.baseMesh, _evenIntervalPoints, _property.uvType);

            pointData.uvDivision = _property.uvDivision;
            pointData.uvOffset = _property.uvOffset;

            // TODO directory auto make
            if (!_property.exportDir.EndsWith("/")) _property.exportDir += "/";
            var exportName = WindowSupport.FindExportNameTake(_property.exportDir, _property.exportNamePrefix);

            var path = string.Format("{0}{1}.asset", _property.exportDir, exportName);
            AssetDatabase.CreateAsset(pointData, path);
            AssetDatabase.SaveAssets();

            EditorUtility.DisplayDialog("PointBuild", "Export To: \n  " + path, "ok");

        }

        private void OnClickPreviewResetButton()
        {
            _uvPointPosition = _defaultUvPreviewPosition;
            _meshPointPosition = _defaultPointMeshPreviewPosition;
            _meshPointMeshPreviewEuler = _defaultPointMeshPreviewEuler;
        }

        private void OnChangePreviewMaterial()
        {
            _uvWireMat.SetFloat("_WireframeVal", _property.wireSize);
            //_pointMat.SetFloat("_Size", _pointSize);
            _uvWireMat.SetColor("_FrontColor", _property.wireColor);
            _uvPointMat.SetColor("_Color", _property.pointColor);

            OnChangeProperty();
        }

        private void OnChangeMeshUV()
        {
            if (_property.baseMesh != null)
            {
                _uvWireMesh = WindowSupport.CreateUVMesh(_property.baseMesh, _property.uvType);

                if (string.IsNullOrEmpty(_property.exportNamePrefix))
                    _property.exportNamePrefix = _property.baseMesh.name + "_";
            }

            _meshPointMesh = null;

            OnChangeProperty();
        }

        private void OnChangePointBuilderSettings()
        {
            _evenIntervalPoints = _pointBuilder.CreateEvenIntervalPoints(_property.uvDivision, _property.uvOffset);
            _uvPointMesh = WindowSupport.CreateMeshFromPointOnUV(_evenIntervalPoints);

            _meshPointMesh = null;

            OnChangeProperty();
        }

        private void OnChangeProperty()
        {
            EditorUtility.SetDirty(_property);
        }

    }
}