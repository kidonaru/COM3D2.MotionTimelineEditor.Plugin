using UnityEngine;
using UnityEngine.Rendering;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace COM3D2.MotionTimelineEditor.Plugin
{
    [ExecuteInEditMode]
    public class StageLaser : MonoBehaviour
    {
        [SerializeField]
        public StageLaserController _controller;
        public StageLaserController controller
        {
            get
            {
                return _controller;
            }
            set
            {
                if (_controller == value) return;
                _controller = value;
                UpdateName();
            }
        }

        [SerializeField]
        private int _index = 0;
        public int index
        {
            get
            {
                return _index;
            }
            set
            {
                if (_index == value) return;
                _index = value;
                UpdateName();
            }
        }

        public string displayName;

        [SerializeField]
        private Vector3 _eulerAngles = DefaultEulerAngles;
        public Vector3 eulerAngles
        {
            get
            {
                return _eulerAngles;
            }
            set
            {
                if (_eulerAngles == value) return;
                _eulerAngles = value;
                transform.localEulerAngles = value;
            }
        }

        [SerializeField]
        private Color _color1 = DefaultColor1;
        public Color color1
        {
            get
            {
                return _color1;
            }
            set
            {
                if (_color1 == value) return;
                _color1 = value;
                _requestedMeshUpdate = true;
            }
        }

        [SerializeField]
        private Color _color2 = DefaultColor2;
        public Color color2
        {
            get
            {
                return _color2;
            }
            set
            {
                if (_color2 == value) return;
                _color2 = value;
                _requestedMeshUpdate = true;
            }
        }

        [SerializeField]
        private float _intensity = 1f;
        public float intensity
        {
            get
            {
                return _intensity;
            }
            set
            {
                if (_intensity == value) return;
                _intensity = value;
                _requestedMeshUpdate = true;
            }
        }

        [SerializeField]
        private float _laserRange = 13.0f;
        public float laserRange
        {
            get
            {
                return _laserRange;
            }
            set
            {
                if (_laserRange == value) return;
                _laserRange = value;
                _requestedMeshUpdate = true;
            }
        }

        [SerializeField]
        private float _laserWidth = 0.05f;
        public float laserWidth
        {
            get
            {
                return _laserWidth;
            }
            set
            {
                if (_laserWidth == value) return;
                _laserWidth = value;
                _requestedMeshUpdate = true;
            }
        }

        [SerializeField]
        [Range(0.1f, 1f)]
        private float _falloffExp = 0.2f;
        public float falloffExp
        {
            get
            {
                return _falloffExp;
            }
            set
            {
                if (_falloffExp == value) return;
                _falloffExp = value;
                _requestedMeshUpdate = true;
            }
        }

        [SerializeField]
        [Range(0f, 1f)]
        private float _noiseStrength = 0.2f;
        public float noiseStrength
        {
            get
            {
                return _noiseStrength;
            }
            set
            {
                if (_noiseStrength == value) return;
                _noiseStrength = value;
                _requestedMaterialUpdate = true;
            }
        }

        [SerializeField]
        [Range(1f, 10f)]
        private float _noiseScale = 5f;
        public float noiseScale
        {
            get
            {
                return _noiseScale;
            }
            set
            {
                if (_noiseScale == value) return;
                _noiseScale = value;
                _requestedMaterialUpdate = true;
            }
        }

        [SerializeField]
        [Range(0f, 1f)]
        private float _coreRadius = 0f;
        public float coreRadius
        {
            get
            {
                return _coreRadius;
            }
            set
            {
                if (_coreRadius == value) return;
                _coreRadius = value;
                _requestedMeshUpdate = true;
            }
        }

        [SerializeField]
        private float _offsetRange = 0f;
        public float offsetRange
        {
            get
            {
                return _offsetRange;
            }
            set
            {
                if (_offsetRange == value) return;
                _offsetRange = value;
                _requestedMeshUpdate = true;
            }
        }

        [SerializeField]
        private float _glowWidth = 0.1f;
        public float glowWidth
        {
            get
            {
                return _glowWidth;
            }
            set
            {
                if (_glowWidth == value) return;
                _glowWidth = value;
                _requestedMeshUpdate = true;
            }
        }

        [SerializeField]
        [Range(1, 64)]
        private int _segmentRange = 10;
        public int segmentRange
        {
            get
            {
                return _segmentRange;
            }
            set
            {
                if (_segmentRange == value) return;
                _segmentRange = value;
                _requestedMeshUpdate = true;
            }
        }

        [SerializeField]
        private bool _zTest = true;
        public bool zTest
        {
            get
            {
                return _zTest;
            }
            set
            {
                if (_zTest == value) return;
                _zTest = value;
                _requestedMaterialUpdate = true;
            }
        }

        public bool visible
        {
            get
            {
                return _meshObject != null && _meshObject.activeSelf;
            }
            set
            {
                if (_meshObject != null && _meshObject.activeSelf != value)
                {
                    _meshObject.SetActive(value);
                }
            }
        }

        public int groupIndex
        {
            get
            {
                if (_controller != null)
                {
                    return _controller.groupIndex;
                }
                return 0;
            }
        }

        public static Color DefaultColor1 = new Color(1f, 1f, 1f, 0.5f);
        public static Color DefaultColor2 = new Color(0, 0.8f, 1f, 0.1f);
        public static Vector3 DefaultPosition = new Vector3(0f, 0f, 0f);
        public static Vector3 DefaultEulerAngles = new Vector3(0f, 0f, 0f);

        private bool _requestedMeshUpdate = false;
        private bool _requestedMaterialUpdate = false;

        private GameObject _meshObject;
        private MeshFilter _meshFilter;
        private MeshRenderer _meshRenderer;

#if COM3D2
        private static TimelineBundleManager bundleManager => TimelineBundleManager.instance;
#endif

        void OnEnable()
        {
            Initialize();
#if UNITY_EDITOR
            EditorApplication.update += OnEditorUpdate;
#endif
        }

        void OnDisable()
        {
#if UNITY_EDITOR
            EditorApplication.update -= OnEditorUpdate;
#endif
        }

        void Reset()
        {
            Initialize();
        }

        void OnValidate()
        {
            if (_meshFilter != null)
            {
                UpdateMesh();
                UpdateMaterial();
            }
        }

        void OnEditorUpdate()
        {
            if (!Application.isPlaying)
            {
                LateUpdate();
            }
        }

        void LateUpdate()
        {
            if (!visible)
            {
                return;
            }

            if (_requestedMeshUpdate)
            {
                UpdateMesh();
                _requestedMeshUpdate = false;
            }

            if (_requestedMaterialUpdate)
            {
                UpdateMaterial();
                _requestedMaterialUpdate = false;
            }

            UpdateTransform();
        }

        public void CopyFrom(StageLaser other)
        {
            if (other == null) return;
            eulerAngles = other.eulerAngles;
            color1 = other.color1;
            color2 = other.color2;
            intensity = other.intensity;
            laserRange = other.laserRange;
            laserWidth = other.laserWidth;
            falloffExp = other.falloffExp;
            noiseStrength = other.noiseStrength;
            noiseScale = other.noiseScale;
            coreRadius = other.coreRadius;
            offsetRange = other.offsetRange;
            glowWidth = other.glowWidth;
            segmentRange = other.segmentRange;
            zTest = other.zTest;
        }

        public void Initialize()
        {
            var meshTransform = transform.Find("Mesh");
            _meshObject = meshTransform != null ? meshTransform.gameObject : null;
            if (_meshObject == null)
            {
                _meshObject = new GameObject("Mesh");
                _meshObject.transform.parent = transform;
                _meshObject.transform.localPosition = Vector3.zero;
                _meshObject.transform.localRotation = Quaternion.identity;
            }

            _meshFilter = _meshObject.GetComponent<MeshFilter>();
            if (_meshFilter == null)
            {
                _meshFilter = _meshObject.AddComponent<MeshFilter>();
            }

            _meshRenderer = _meshObject.GetComponent<MeshRenderer>();
            if (_meshRenderer == null)
            {
                _meshRenderer = _meshObject.AddComponent<MeshRenderer>();
                _meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

#if COM3D2
                var material = bundleManager.LoadMaterial("StageLaser");
#else
                var material = new Material(Shader.Find("MTE/StageLaser"));
                material.SetTexture("_MainTex", Resources.Load<Texture2D>("noise_texture"));
#endif
                _meshRenderer.material = material;
            }

            transform.localPosition = DefaultPosition;
            transform.localEulerAngles = _eulerAngles;

            UpdateName();
            UpdateMesh();
            UpdateMaterial();
            UpdateTransform();
        }

        private Vector3[] _vertices = null;
        private Color[] _colors = null;
        private int[] _triangles = null;

        void UpdateMesh()
        {
            var mesh = _meshFilter.mesh;
            mesh.Clear();

            float range = laserRange;
            float width = laserWidth * 0.5f;
            
            // 範囲方向の分割
            float rangeStep = (range - offsetRange) / segmentRange;

            // 頂点の計算
            int verticesCount = 6 * (segmentRange + 1);
            int vertexIndex = 0;

            if (_vertices == null || _vertices.Length != verticesCount)
            {
                _vertices = new Vector3[verticesCount];
            }

            if (_colors == null || _colors.Length != verticesCount)
            {
                _colors = new Color[verticesCount];
            }

            float coreWidth = width * coreRadius;
            var zeroColor = new Color(0, 0, 0, 0);

            for (int r = 0; r <= segmentRange; r++)
            {
                float z = offsetRange + rangeStep * r;

                float distanceFalloff = Mathf.Pow(1 - Mathf.Clamp01(z / laserRange), falloffExp);
                distanceFalloff = Smoothstep(0, 1, distanceFalloff);
                distanceFalloff *= intensity;

                float currentGlowWidth = Mathf.Lerp(0, glowWidth, distanceFalloff);

                var currentColor2 = Color.Lerp(zeroColor, color2, distanceFalloff);

                // 散乱光の左端（完全に透明）
                _vertices[vertexIndex] = new Vector3(-(currentGlowWidth + width), 0, z);
                _colors[vertexIndex++] = zeroColor;

                // レーザーの左端（color2）
                _vertices[vertexIndex] = new Vector3(-width, 0, z);
                _colors[vertexIndex++] = currentColor2;

                // レーザーのコア部分の左端（color1）
                _vertices[vertexIndex] = new Vector3(-coreWidth, 0, z);
                _colors[vertexIndex++] = Color.Lerp(zeroColor, color1, distanceFalloff);

                // レーザーのコア部分の右端（color1）
                _vertices[vertexIndex] = new Vector3(coreWidth, 0, z);
                _colors[vertexIndex++] = Color.Lerp(zeroColor, color1, distanceFalloff);

                // レーザーの右端（color2）
                _vertices[vertexIndex] = new Vector3(width, 0, z);
                _colors[vertexIndex++] = currentColor2;

                // 散乱光の右端（完全に透明）
                _vertices[vertexIndex] = new Vector3(currentGlowWidth + width, 0, z);
                _colors[vertexIndex++] = zeroColor;
            }

            // インデックスの計算
            int triangleCount = segmentRange * 30; // 5つの領域×2三角形×3頂点
            int triangleIndex = 0;

            if (_triangles == null || _triangles.Length != triangleCount)
            {
                _triangles = new int[triangleCount];
            }

            for (int r = 0; r < segmentRange; r++)
            {
                int baseIndex = r * 6; // 断面は6つの頂点で構成される

                // 散乱光の左側
                _triangles[triangleIndex++] = baseIndex;
                _triangles[triangleIndex++] = baseIndex + 6;
                _triangles[triangleIndex++] = baseIndex + 1;
                
                _triangles[triangleIndex++] = baseIndex + 1;
                _triangles[triangleIndex++] = baseIndex + 6;
                _triangles[triangleIndex++] = baseIndex + 7;

                // レーザーの外側左
                _triangles[triangleIndex++] = baseIndex + 1;
                _triangles[triangleIndex++] = baseIndex + 7;
                _triangles[triangleIndex++] = baseIndex + 2;
                
                _triangles[triangleIndex++] = baseIndex + 2;
                _triangles[triangleIndex++] = baseIndex + 7;
                _triangles[triangleIndex++] = baseIndex + 8;

                // レーザーのコア部分
                _triangles[triangleIndex++] = baseIndex + 2;
                _triangles[triangleIndex++] = baseIndex + 8;
                _triangles[triangleIndex++] = baseIndex + 3;
                
                _triangles[triangleIndex++] = baseIndex + 3;
                _triangles[triangleIndex++] = baseIndex + 8;
                _triangles[triangleIndex++] = baseIndex + 9;

                // レーザーの外側右
                _triangles[triangleIndex++] = baseIndex + 3;
                _triangles[triangleIndex++] = baseIndex + 9;
                _triangles[triangleIndex++] = baseIndex + 4;
                
                _triangles[triangleIndex++] = baseIndex + 4;
                _triangles[triangleIndex++] = baseIndex + 9;
                _triangles[triangleIndex++] = baseIndex + 10;

                // 散乱光の右側
                _triangles[triangleIndex++] = baseIndex + 4;
                _triangles[triangleIndex++] = baseIndex + 10;
                _triangles[triangleIndex++] = baseIndex + 5;
                
                _triangles[triangleIndex++] = baseIndex + 5;
                _triangles[triangleIndex++] = baseIndex + 10;
                _triangles[triangleIndex++] = baseIndex + 11;
            }

            mesh.vertices = _vertices;
            mesh.colors = _colors;
            mesh.triangles = _triangles;
            mesh.RecalculateBounds();
        }

        private float Smoothstep(float edge0, float edge1, float x)
        {
            x = Mathf.Clamp01((x - edge0) / (edge1 - edge0));
            return x * x * (3 - 2 * x);
        }

        void UpdateTransform()
        {
            Camera camera = GetCurrentCamera();
            if (camera == null)
            {
                Debug.LogWarning("カメラが見つかりません");
            }

            if (_meshFilter != null && camera != null)
            {
                _meshFilter.transform.LookAt(camera.transform, transform.forward);
                
                var localRotation = _meshFilter.transform.localRotation;
                localRotation.x = 0;
                localRotation.y = 0;
                _meshFilter.transform.localRotation = localRotation;
            }
        }

        private void UpdateMaterial()
        {
            if (_meshRenderer != null && _meshRenderer.material != null)
            {
                var material = _meshRenderer.material;
                material.SetFloat(Uniforms._NoiseStrength, noiseStrength);
                material.SetFloat(Uniforms._NoiseScaleInv, 1f / noiseScale);
                material.SetInt(Uniforms._zTest, (int) (zTest ? CompareFunction.LessEqual : CompareFunction.Always));
            }
        }

        private static class Uniforms
        {
            internal static readonly int _NoiseStrength = Shader.PropertyToID("_NoiseStrength");
            internal static readonly int _NoiseScaleInv = Shader.PropertyToID("_NoiseScaleInv");
            internal static readonly int _zTest = Shader.PropertyToID("_ZTest");
        }

        private void UpdateName()
        {
            var suffix = " (" + groupIndex + ", " + index + ")";
            name = "StageLaser" + suffix;
            displayName = "ステージレーザー" + suffix;
        }

        private Camera GetCurrentCamera()
        {
#if COM3D2
            return PluginUtils.MainCamera;
#else
            // EditMode時はSceneViewのカメラを使用
            if (!Application.isPlaying)
            {
                SceneView sceneView = SceneView.lastActiveSceneView;
                return sceneView != null ? sceneView.camera : null;
            }
            return Camera.main;
#endif
        }
    }
}