using UnityEngine;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    public class LetterBoxView : MonoBehaviour
    {
        private Material _letterBoxMaterial;
        private LineRenderer[] _letterBoxLines;

        private float _currentAspect = -1f;
        private float _targetAspect = -1f;

        private static CameraManager cameraManager
        {
            get
            {
                return CameraManager.instance;
            }
        }

        private static Camera frontCamera
        {
            get
            {
                return cameraManager.frontCamera;
            }
        }

        private static TimelineManager timelineManager
        {
            get
            {
                return TimelineManager.instance;
            }
        }

        private static TimelineData timeline
        {
            get
            {
                return timelineManager.timeline;
            }
        }

        public void Awake()
        {
            CreateLetterBoxMaterial();
            CreateLetterBoxLines();
        }

        void CreateLetterBoxMaterial()
        {
            if (!_letterBoxMaterial)
            {
                Shader shader = Shader.Find("Hidden/Internal-Colored");
                _letterBoxMaterial = new Material(shader);
                _letterBoxMaterial.hideFlags = HideFlags.HideAndDontSave;
                _letterBoxMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                _letterBoxMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                _letterBoxMaterial.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
                _letterBoxMaterial.SetInt("_ZWrite", 0);
            }
        }

        void CreateLetterBoxLines()
        {
            RemoveLetterBoxLines();

            // 上下または左右の黒帯用に2本のラインを用意
            _letterBoxLines = new LineRenderer[2];
            for (int i = 0; i < 2; i++)
            {
                _letterBoxLines[i] = CreateLine(i);
            }
        }

        private LineRenderer CreateLine(int index)
        {
            GameObject lineObj = new GameObject("LetterBoxLine_" + index);
            lineObj.layer = LayerMask.NameToLayer("NGUI");
            lineObj.transform.SetParent(this.transform, false);

            LineRenderer line = lineObj.AddComponent<LineRenderer>();
            line.material = _letterBoxMaterial;
            line.positionCount = 2;
            line.useWorldSpace = true;
            line.sortingOrder = 8000;

            return line;
        }

        void RemoveLetterBoxLines()
        {
            if (_letterBoxLines != null)
            {
                foreach (var line in _letterBoxLines)
                {
                    Destroy(line.gameObject);
                }
                _letterBoxLines = null;
            }
        }

        public void OnDestroy()
        {
            if (_letterBoxMaterial != null)
            {
                Destroy(_letterBoxMaterial);
                _letterBoxMaterial = null;
            }
            RemoveLetterBoxLines();
        }

        public void LateUpdate()
        {
            if (timeline.aspectRatio == 0f)
            {
                SetLetterBoxVisibility(false);
                return;
            }

            SetLetterBoxVisibility(true);
            UpdateLetterBox();
        }

        private void SetLetterBoxVisibility(bool isVisible)
        {
            foreach (var line in _letterBoxLines)
            {
                if (line != null && line.enabled != isVisible)
                {
                    line.enabled = isVisible;
                }
            }
        }

        private void UpdateLetterBox()
        {
            var cam = frontCamera;

            float currentAspect = cam.aspect;
            float targetAspect = timeline.aspectRatio;

            if (currentAspect == _currentAspect && targetAspect == _targetAspect)
            {
                return;
            }

            _currentAspect = currentAspect;
            _targetAspect = targetAspect;

            float distance = cam.nearClipPlane + 0.01f;
            Vector3 center = cam.transform.position + cam.transform.forward * distance;

            float height = cam.orthographicSize * 2f;
            float width = height * cam.aspect;

            float widthMultiplier = 1f;
            var letterBoxColor = Color.black;

            if (currentAspect > targetAspect)
            {
                // 左右に黒帯
                float targetWidth = height * targetAspect;
                float barWidth = (width - targetWidth) * 0.5f;

                Vector3 leftStart = center - cam.transform.right * width * 0.5f;
                Vector3 leftEnd = leftStart + cam.transform.right * barWidth;
                UpdateLetterBoxLine(_letterBoxLines[0], leftStart, leftEnd, height, letterBoxColor, widthMultiplier);

                Vector3 rightStart = center + cam.transform.right * width * 0.5f;
                Vector3 rightEnd = rightStart - cam.transform.right * barWidth;
                UpdateLetterBoxLine(_letterBoxLines[1], rightStart, rightEnd, height, letterBoxColor, widthMultiplier);
            }
            else
            {
                // 上下に黒帯
                float targetHeight = width / targetAspect;
                float barHeight = (height - targetHeight) * 0.5f;

                Vector3 topStart = center + cam.transform.up * height * 0.5f;
                Vector3 topEnd = topStart - cam.transform.up * barHeight;
                UpdateLetterBoxLine(_letterBoxLines[0], topStart, topEnd, width, letterBoxColor, widthMultiplier);

                Vector3 bottomStart = center - cam.transform.up * height * 0.5f;
                Vector3 bottomEnd = bottomStart + cam.transform.up * barHeight;
                UpdateLetterBoxLine(_letterBoxLines[1], bottomStart, bottomEnd, width, letterBoxColor, widthMultiplier);
            }
        }

        private void UpdateLetterBoxLine(
            LineRenderer line,
            Vector3 start,
            Vector3 end,
            float length,
            Color color,
            float widthMultiplier)
        {
            line.SetPosition(0, start);
            line.SetPosition(1, end);
            line.startColor = color;
            line.endColor = color;
            line.startWidth = length;
            line.endWidth = length;
            line.widthMultiplier = widthMultiplier;
        }
    }
    
}