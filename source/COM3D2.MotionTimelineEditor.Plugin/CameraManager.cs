using UnityEngine;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    public class CameraManager
    {
        private static CameraManager _instance;
        public static CameraManager instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new CameraManager();
                }

                return _instance;
            }
        }

        private Camera _frontCamera = null;
        private LetterBoxView _letterBoxView = null;

        public Camera frontCamera
        {
            get
            {
                CreateCamera();
                return _frontCamera;
            }
        }

        private CameraManager()
        {
        }

        public void Init()
        {
            TimelineManager.onRefresh += CreateCamera;
        }

        public void OnPluginDisable()
        {
            DestroyCamera();
        }

        public void OnPluginEnable()
        {
            CreateCamera();
        }

        public void Update()
        {
        }

        public void ResetCache()
        {
            if (_letterBoxView != null)
            {
                _letterBoxView.ResetCache();
            }
        }

        private void CreateCamera()
        {
            if (_frontCamera == null)
            {
                GameObject go = new GameObject("MTEFrontCamera");
                _frontCamera = go.AddComponent<Camera>();

                _frontCamera.enabled = true;
                _frontCamera.orthographic = true;
                _frontCamera.orthographicSize = 1.0f;
                _frontCamera.transform.position = new Vector3(0.0f, -6601.0f, -0.4f);
                _frontCamera.transform.rotation = Quaternion.Euler(0.0f, 0.0f, 0.0f);
                _frontCamera.fieldOfView = 60f;
                _frontCamera.nearClipPlane = -10f;
                _frontCamera.farClipPlane = 10f;
                _frontCamera.depth = 5f;
                _frontCamera.cullingMask = 256;
                _frontCamera.renderingPath = RenderingPath.Forward;
                _frontCamera.clearFlags = CameraClearFlags.Depth;
                _frontCamera.allowHDR = false;
                _frontCamera.allowMSAA = false;
            }

            if (_letterBoxView == null)
            {
                GameObject go = new GameObject("LetterBoxView");
                _letterBoxView = go.AddComponent<LetterBoxView>();
            }
        }

        private void DestroyCamera()
        {
            if (_frontCamera != null)
            {
                GameObject.Destroy(_frontCamera.gameObject);
                _frontCamera = null;
            }

            if (_letterBoxView != null)
            {
                GameObject.Destroy(_letterBoxView.gameObject);
                _letterBoxView = null;
            }
        }
    }
}