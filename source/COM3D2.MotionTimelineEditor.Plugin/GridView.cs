using UnityEngine;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    public class GridView : MonoBehaviour
    {
        public float distanceFromNearPlane = 0.01f;

        Material _gridMaterial;
        private LineRenderer[] _gridLinesInDisplay;
        private LineRenderer[] _gridLinesInWorld;
        private LineRenderer _yAxisLine;

        private static Config config
        {
            get
            {
                return ConfigManager.config;
            }
        }

        private static StudioHackBase studioHack
        {
            get
            {
                return StudioHackManager.studioHack;
            }
        }

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

            _yAxisLine = CreateLine(0, true);
        }

        private LineRenderer CreateLine(int index, bool isWorld)
        {
            GameObject lineObj = new GameObject("GridLine_" + index);
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

            if (_yAxisLine != null)
            {
                Destroy(_yAxisLine.gameObject);
                _yAxisLine = null;
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
            if (config.isGridVisibleOnlyEdit && !studioHack.isPoseEditing)
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
                _yAxisLine.enabled = false;
                return;
            }

            SetGridLinesVisibility(_gridLinesInDisplay, config.isGridVisibleInDisplay);
            SetGridLinesVisibility(_gridLinesInWorld, config.isGridVisibleInWorld);
            _yAxisLine.enabled = config.isGridVisibleInWorld;

            if (config.isGridVisibleInDisplay)
            {
                UpdateDisplayGrid();
            }

            if (config.isGridVisibleInWorld)
            {
                UpdateWorldGrid();
            }
        }

        private void UpdateDisplayGrid()
        {
            var cam = Camera.main;
            float distance = cam.nearClipPlane + distanceFromNearPlane;
            Vector3 center = cam.transform.position + cam.transform.forward * distance;
            
            float height = 2.0f * distance * Mathf.Tan(cam.fieldOfView * 0.5f * Mathf.Deg2Rad);
            float width = height * cam.aspect;
            
            var gridColor = config.gridColorInDisplay;
            gridColor.a = config.gridAlpha;

            var centerColor = config.gridCenterColorInDisplay;
            centerColor.a = config.gridAlpha;

            var gridCount = config.gridCount;
            var cellSize = 1.0f / gridCount;

            int lineIndex = 0;
            var centerIndex = (gridCount % 2 == 0) ? gridCount / 2 : -1;

            // カメラからの距離に基づいてwidthMultiplierを計算
            float distanceToCamera = Vector3.Distance(center, Camera.main.transform.position);
            float widthMultiplier = distanceToCamera * 0.001f;

            var right = cam.transform.right;
            var up = cam.transform.up;

            // 縦線を更新
            for (int i = 0; i <= gridCount; i++)
            {
                float x = (i * cellSize - 0.5f) * width;
                Vector3 start = center + right * x - up * height * 0.5f;
                Vector3 end = center + right * x + up * height * 0.5f;
                var color = i == centerIndex ? centerColor : gridColor;

                UpdateLineProp(_gridLinesInDisplay[lineIndex], start, end, color, widthMultiplier);
                lineIndex++;
            }

            // 横線を更新
            for (int j = 0; j <= gridCount; j++)
            {
                float y = (j * cellSize - 0.5f) * height;
                Vector3 start = center - right * width * 0.5f + up * y;
                Vector3 end = center + right * width * 0.5f + up * y;
                var color = j == centerIndex ? centerColor : gridColor;

                UpdateLineProp(_gridLinesInDisplay[lineIndex], start, end, color, widthMultiplier);
                lineIndex++;
            }
        }

        private void UpdateWorldGrid()
        {
            var gridColor = config.gridColorInWorld;
            gridColor.a = config.gridAlphaInWorld;

            var centerColor = config.gridCenterColorInWorld;
            centerColor.a = config.gridAlphaInWorld;

            var cellSize = config.gridCellSize;
            var gridCount = config.gridCountInWorld;
            var halfSize = gridCount * cellSize * 0.5f;

            int lineIndex = 0;
            var centerIndex = (gridCount % 2 == 0) ? gridCount / 2 : -1;

            // カメラからの距離に基づいてwidthMultiplierを計算
            float distanceToCamera = Vector3.Distance(transform.position, Camera.main.transform.position);
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

            // Y軸のラインを更新
            if (_yAxisLine != null)
            {
                Vector3 start = new Vector3(0, 0, 0);
                Vector3 end = new Vector3(0, halfSize, 0);
                UpdateLineProp(_yAxisLine, start, end, centerColor, widthMultiplier);
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