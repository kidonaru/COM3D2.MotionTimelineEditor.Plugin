using UnityEngine;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    public class GridView : MonoBehaviour
    {
        public float distanceFromNearPlane = 0.01f;

        Material _gridMaterial;
        private LineRenderer[] _gridLinesInDisplay;
        private LineRenderer[] _gridLinesInWorld;
        private LineRenderer[] _axisLines;

        private static Config config => ConfigManager.instance.config;
        private static StudioHackManager studioHackManager => StudioHackManager.instance;

        public void Awake()
        {
            CreateLineMaterial();
            CreateGridLines();
        }

        void CreateLineMaterial()
        {
            if (!_gridMaterial)
            {
                Shader shader = Shader.Find("Hidden/Internal-Colored");
                _gridMaterial = new Material(shader);
                _gridMaterial.hideFlags = HideFlags.HideAndDontSave;
                _gridMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                _gridMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                _gridMaterial.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
                _gridMaterial.SetInt("_ZWrite", 0);
            }
        }

        public void CreateGridLines()
        {
            RemoveGridLines();

            {
                int totalLines = (config.gridCount + 1) * 2;
                _gridLinesInDisplay = new LineRenderer[totalLines];
                for (int i = 0; i < totalLines; i++)
                {
                    _gridLinesInDisplay[i] = CreateLine(i, false);
                }
            }

            {
                int totalLines = (config.gridCountInWorld + 1) * 2;
                _gridLinesInWorld = new LineRenderer[totalLines];
                for (int i = 0; i < totalLines; i++)
                {
                    _gridLinesInWorld[i] = CreateLine(i, true);
                }
            }

            {
                int totalLines = 3;
                _axisLines = new LineRenderer[totalLines];
                for (int i = 0; i < totalLines; i++)
                {
                    _axisLines[i] = CreateLine(i, true);
                }
            }
        }

        private LineRenderer CreateLine(int index, bool isWorld)
        {
            GameObject lineObj = new GameObject("GridLine_" + index);
            lineObj.layer = isWorld ? LayerMask.NameToLayer("Default") : LayerMask.NameToLayer("NGUI");
            lineObj.transform.SetParent(this.transform, false);
            LineRenderer line = lineObj.AddComponent<LineRenderer>();

            line.material = _gridMaterial;
            line.startWidth = isWorld ? config.gridLineWidthInWorld : config.gridLineWidth;
            line.endWidth = isWorld ? config.gridLineWidthInWorld : config.gridLineWidth;
            line.positionCount = 2;
            line.useWorldSpace = true;
            line.sortingOrder = isWorld ? 3000 : 9000;

            return line;
        }

        public void RemoveGridLines()
        {
            if (_gridLinesInDisplay != null)
            {
                foreach (var line in _gridLinesInDisplay)
                {
                    Destroy(line.gameObject);
                }

                _gridLinesInDisplay = null;
            }

            if (_gridLinesInWorld != null)
            {
                foreach (var line in _gridLinesInWorld)
                {
                    Destroy(line.gameObject);
                }

                _gridLinesInWorld = null;
            }

            if (_axisLines != null)
            {
                foreach (var line in _axisLines)
                {
                    Destroy(line.gameObject);
                }

                _axisLines = null;
            }
        }

        public void OnDestroy()
        {
            if (_gridMaterial != null)
            {
                Destroy(_gridMaterial);
                _gridMaterial = null;
            }

            RemoveGridLines();
        }

        private bool IsGridVisible()
        {
            if (!config.isGridVisible)
            {
                return false;
            }
            if (config.isGridVisibleOnlyEdit && !studioHackManager.isPoseEditing)
            {
                return false;
            }

            return true;
        }

        public void LateUpdate()
        {
            if (!IsGridVisible())
            {
                SetGridLinesVisibility(_gridLinesInDisplay, false);
                SetGridLinesVisibility(_gridLinesInWorld, false);
                SetGridLinesVisibility(_axisLines, false);
                return;
            }

            SetGridLinesVisibility(_gridLinesInDisplay, config.isGridVisibleInDisplay);
            SetGridLinesVisibility(_gridLinesInWorld, config.isGridVisibleInWorld);
            SetGridLinesVisibility(_axisLines, config.isGridVisibleInWorld);

            if (config.isGridVisibleInDisplay)
            {
                UpdateDisplayGrid();
            }

            if (config.isGridVisibleInWorld)
            {
                UpdateWorldGrid();
            }
        }

        private static Camera frontCamera
        {
            get
            {
                return CameraManager.instance.frontCamera;
            }
        }

        private void UpdateDisplayGrid()
        {
            var cam = frontCamera;

            float distance = cam.nearClipPlane + distanceFromNearPlane;
            Vector3 center = cam.transform.position + cam.transform.forward * distance;

            float width, height, widthMultiplier;
            if (cam.orthographic)
            {
                height = cam.orthographicSize * 2f;
                width = height * cam.aspect;

                widthMultiplier = 0.005f;
            }
            else
            {
                height = 2.0f * distance * Mathf.Tan(cam.fieldOfView * 0.5f * Mathf.Deg2Rad);
                width = height * cam.aspect;

                // カメラからの距離に基づいてwidthMultiplierを計算
                float distanceToCamera = Vector3.Distance(center, PluginUtils.MainCamera.transform.position);
                widthMultiplier = distanceToCamera * 0.001f;
            }

            var gridColor = config.gridColorInDisplay;
            gridColor.a = config.gridAlpha;

            var gridCount = config.gridCount;
            var cellSize = 1.0f / gridCount;

            int lineIndex = 0;
            var centerIndex = (gridCount % 2 == 0) ? gridCount / 2 : -1;

            var right = cam.transform.right;
            var up = cam.transform.up;

            // 縦線を更新
            for (int i = 0; i <= gridCount; i++)
            {
                float x = (i * cellSize - 0.5f) * width;
                Vector3 start = center + right * x - up * height * 0.5f;
                Vector3 end = center + right * x + up * height * 0.5f;

                UpdateLineProp(_gridLinesInDisplay[lineIndex], start, end, gridColor, widthMultiplier);
                lineIndex++;
            }

            // 横線を更新
            for (int j = 0; j <= gridCount; j++)
            {
                float y = (j * cellSize - 0.5f) * height;
                Vector3 start = center - right * width * 0.5f + up * y;
                Vector3 end = center + right * width * 0.5f + up * y;

                UpdateLineProp(_gridLinesInDisplay[lineIndex], start, end, gridColor, widthMultiplier);
                lineIndex++;
            }
        }

        private void UpdateWorldGrid()
        {
            var gridColor = config.gridColorInWorld;
            gridColor.a = config.gridAlphaInWorld;
            var centerColor = new Color(0, 0, 0, 0);

            var cellSize = config.gridCellSize;
            var gridCount = config.gridCountInWorld;
            var halfSize = gridCount * cellSize * 0.5f;

            int lineIndex = 0;
            var centerIndex = (gridCount % 2 == 0) ? gridCount / 2 : -1;

            // カメラからの距離に基づいてwidthMultiplierを計算
            float distanceToCamera = Vector3.Distance(transform.position, PluginUtils.MainCamera.transform.position);
            float widthMultiplier = distanceToCamera * 0.001f;

            // 縦線を更新
            for (int i = 0; i <= gridCount; i++)
            {
                float x = i * cellSize - halfSize;
                Vector3 start = new Vector3(x, 0, -halfSize);
                Vector3 end = new Vector3(x, 0, halfSize);
                var color = i == centerIndex ? centerColor : gridColor;

                UpdateLineProp(_gridLinesInWorld[lineIndex], start, end, color, widthMultiplier);
                lineIndex++;
            }

            // 横線を更新
            for (int j = 0; j <= gridCount; j++)
            {
                float z = j * cellSize - halfSize;
                Vector3 start = new Vector3(-halfSize, 0, z);
                Vector3 end = new Vector3(halfSize, 0, z);
                var color = j == centerIndex ? centerColor : gridColor;

                UpdateLineProp(_gridLinesInWorld[lineIndex], start, end, color, widthMultiplier);
                lineIndex++;
            }

            // 軸のラインを更新
            {
                var red = Color.red;
                red.a = config.gridAlphaInWorld;
                var blue = Color.blue;
                blue.a = config.gridAlphaInWorld;
                var green = Color.green;
                green.a = config.gridAlphaInWorld;

                UpdateLineProp(_axisLines[0], new Vector3(-halfSize, 0, 0), new Vector3(halfSize, 0, 0), red, widthMultiplier);
                UpdateLineProp(_axisLines[1], new Vector3(0, -halfSize, 0), new Vector3(0, halfSize, 0), green, widthMultiplier);
                UpdateLineProp(_axisLines[2], new Vector3(0, 0, -halfSize), new Vector3(0, 0, halfSize), blue, widthMultiplier);
            }
        }

        private void SetGridLinesVisibility(LineRenderer[] lineRenderers, bool isVisible)
        {
            foreach (var line in lineRenderers)
            {
                if (line != null && line.enabled != isVisible)
                {
                    line.enabled = isVisible;
                }
            }
        }

        private void UpdateLineProp(
            LineRenderer line,
            Vector3 start,
            Vector3 end,
            Color color,
            float widthMultiplier)
        {
            line.SetPosition(0, start);
            line.SetPosition(1, end);
            line.startColor = color;
            line.endColor = color;
            line.widthMultiplier = widthMultiplier;
        }
    }
    
}