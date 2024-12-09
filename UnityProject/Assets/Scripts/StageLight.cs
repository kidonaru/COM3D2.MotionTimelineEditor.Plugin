using UnityEngine;
using UnityEngine.Rendering;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace COM3D2.MotionTimelineEditor.Plugin
{
    [ExecuteInEditMode]
    public class StageLight : MonoBehaviour
    {
        [SerializeField]
        public StageLightController _controller;
        public StageLightController controller
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

        public Light spotLight;

        [SerializeField]
        private Vector3 _position = DefaultPosition;
        public Vector3 position
        {
            get
            {
                return _position;
            }
            set
            {
                if (_position == value) return;
                _position = value;
                transform.localPosition = value;
            }
        }

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
        private Color _color = Color.white;
        public Color color
        {
            get
            {
                return _color;
            }
            set
            {
                if (_color == value) return;
                _color = value;
                UpdateMaterial();
            }
        }

        [SerializeField]
        [Range(1f, 179f)]
        private float _spotAngle = 10.0f;
        public float spotAngle
        {
            get
            {
                return _spotAngle;
            }
            set
            {
                if (_spotAngle == value) return;
                _spotAngle = value;
                UpdateMesh();
                UpdateMaterial();
            }
        }

        [SerializeField]
        private float _spotRange = 10.0f;
        public float spotRange
        {
            get
            {
                return _spotRange;
            }
            set
            {
                if (_spotRange == value) return;
                _spotRange = value;
                UpdateMesh();
                UpdateMaterial();
            }
        }

        [SerializeField]
        [Range(0.1f, 1.0f)]
        private float _rangeMultiplier = 0.8f;
        public float rangeMultiplier
        {
            get
            {
                return _rangeMultiplier;
            }
            set
            {
                if (_rangeMultiplier == value) return;
                _rangeMultiplier = value;
                UpdateMesh();
                UpdateMaterial();
            }
        }

        [SerializeField]
        [Range(0.1f, 1f)]
        private float _falloffExp = 0.5f;
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
                UpdateMaterial();
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
                UpdateMaterial();
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
                UpdateMaterial();
            }
        }

        [SerializeField]
        [Range(0f, 1f)]
        private float _coreRadius = 0.2f;
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
                UpdateMaterial();
            }
        }

        [SerializeField]
        private float _offsetRange = 0.5f;
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
                UpdateMesh();
            }
        }

        [SerializeField]
        [Range(1f, 64)]
        private int _segmentAngle = 10;
        public int segmentAngle
        {
            get
            {
                return _segmentAngle;
            }
            set
            {
                if (_segmentAngle == value) return;
                _segmentAngle = value;
                UpdateMesh();
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
                UpdateMesh();
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
                UpdateMaterial();
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

        public static Vector3 DefaultPosition = new Vector3(0f, 10f, 0f);

        public static Vector3 DefaultEulerAngles = new Vector3(90f, 0f, 0f);

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

        void OnDestroy()
        {
            if (_meshRenderer.sharedMaterial != null)
            {
                DestroyImmediate(_meshRenderer.sharedMaterial);
            }
            if (_meshFilter != null && _meshFilter.sharedMesh != null)
            {
                DestroyImmediate(_meshFilter.sharedMesh);
            }
            if (_meshObject != null)
            {
                DestroyImmediate(_meshObject);
            }
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
                Update();
            }
        }

        void Update()
        {
            if (spotLight != null)
            {
                if (spotLight.color != color)
                {
                    spotLight.color = color;
                }
                if (spotLight.spotAngle != spotAngle)
                {
                    spotLight.spotAngle = spotAngle;
                }
                if (spotLight.range != spotRange)
                {
                    spotLight.range = spotRange;
                }
            }

            UpdateTransform();
        }

        public void CopyFrom(StageLight other)
        {
            if (other == null) return;
            position = other.position;
            eulerAngles = other.eulerAngles;
            color = other.color;
            spotAngle = other.spotAngle;
            spotRange = other.spotRange;
            rangeMultiplier = other.rangeMultiplier;
            falloffExp = other.falloffExp;
            noiseStrength = other.noiseStrength;
            noiseScale = other.noiseScale;
            coreRadius = other.coreRadius;
            offsetRange = other.offsetRange;
            segmentAngle = other.segmentAngle;
            segmentRange = other.segmentRange;
        }

        public void Initialize()
        {
            if (spotLight != null && spotLight.type != LightType.Spot)
            {
                Debug.LogError("このコンポーネントはスポットライトにのみ使用できます");
            }

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

            if (_meshFilter.sharedMesh != null)
            {
                DestroyImmediate(_meshFilter.sharedMesh);
            }

            _meshRenderer = _meshObject.GetComponent<MeshRenderer>();
            if (_meshRenderer == null)
            {
                _meshRenderer = _meshObject.AddComponent<MeshRenderer>();
                _meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            }

            if (_meshRenderer.sharedMaterial != null)
            {
                DestroyImmediate(_meshRenderer.sharedMaterial);
            }

            {
#if COM3D2
                var material = bundleManager.LoadMaterial("StageLight");
#else
                var material = new Material(Shader.Find("MTE/StageLight"));
                material.SetTexture("_MainTex", Resources.Load<Texture2D>("noise_texture"));
#endif
                _meshRenderer.sharedMaterial = material;
            }

            transform.localPosition = _position;
            transform.localEulerAngles = _eulerAngles;

            UpdateName();
            UpdateMesh();
            UpdateMaterial();
            UpdateTransform();
        }

        void UpdateMesh()
        {
            Mesh mesh = _meshFilter.sharedMesh;
            if (mesh == null)
            {
                mesh = new Mesh();
                _meshFilter.sharedMesh = mesh;
                mesh.name = "StageLight Mesh";
            }

            mesh.Clear();

            float angle = spotAngle * 0.5f * Mathf.Deg2Rad;
            float range = CalculateEffectiveRange();
            
            // 半径を計算
            float radius = Mathf.Tan(angle) * range;

            // 角度方向の分割数
            float radiusStep = radius * 2f / segmentAngle;

            // 範囲方向の分割
            float rangeStep = (range - offsetRange) / segmentRange;

            // 頂点の計算
            int verticesCount = (segmentAngle + 1) * (segmentRange + 1);
            Vector3[] vertices = new Vector3[verticesCount];
            int vertexIndex = 0;

            for (int r = 0; r <= segmentRange; r++)
            {
                float z = offsetRange + rangeStep * r;
                float radiusRate = z / range;

                for (int a = 0; a <= segmentAngle; a++)
                {
                    float x = (-radius + radiusStep * a) * radiusRate;
                    vertices[vertexIndex++] = new Vector3(x, 0, z);
                }
            }

            // インデックスの計算
            int[] triangles = new int[segmentAngle * segmentRange * 6];
            int triangleIndex = 0;

            for (int r = 0; r < segmentRange; r++)
            {
                for (int a = 0; a < segmentAngle; a++)
                {
                    int current = r * (segmentAngle + 1) + a;
                    int next = current + (segmentAngle + 1);

                    // 1つ目の三角形
                    triangles[triangleIndex++] = current;
                    triangles[triangleIndex++] = current + 1;
                    triangles[triangleIndex++] = next + 1;

                    // 2つ目の三角形
                    triangles[triangleIndex++] = current;
                    triangles[triangleIndex++] = next + 1;
                    triangles[triangleIndex++] = next;
                }
            }

            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.RecalculateBounds();
        }

        void UpdateTransform()
        {
            if (spotLight != null)
            {
                if (transform.position != spotLight.transform.position)
                {
                    spotLight.transform.position = transform.position;
                }
                if (transform.rotation != spotLight.transform.rotation)
                {
                    spotLight.transform.rotation = transform.rotation;
                }
            }

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
            if (_meshRenderer != null && _meshRenderer.sharedMaterial != null)
            {
                var material = _meshRenderer.sharedMaterial;
                material.SetFloat(Uniforms._SpotRange, CalculateEffectiveRange());
                material.SetFloat(Uniforms._SpotAngle, spotAngle);
                material.SetColor(Uniforms._Color, color);
                material.SetColor(Uniforms._SubColor, color);
                material.SetFloat(Uniforms._FalloffExp, falloffExp);
                material.SetFloat(Uniforms._NoiseStrength, noiseStrength);
                material.SetFloat(Uniforms._NoiseScaleInv, 1f / noiseScale);
                material.SetFloat(Uniforms._CoreRadius, coreRadius);
                material.SetFloat(Uniforms._TanHalfAngle, Mathf.Tan(spotAngle * 0.5f * Mathf.Deg2Rad));
                material.SetInt(Uniforms._zTest, (int) (zTest ? CompareFunction.LessEqual : CompareFunction.Always));
            }
        }

        private static class Uniforms
        {
            internal static readonly int _SpotRange = Shader.PropertyToID("_SpotRange");
            internal static readonly int _SpotAngle = Shader.PropertyToID("_SpotAngle");
            internal static readonly int _Color = Shader.PropertyToID("_Color");
            internal static readonly int _SubColor = Shader.PropertyToID("_SubColor");
            internal static readonly int _FalloffExp = Shader.PropertyToID("_FalloffExp");
            internal static readonly int _NoiseStrength = Shader.PropertyToID("_NoiseStrength");
            internal static readonly int _NoiseScaleInv = Shader.PropertyToID("_NoiseScaleInv");
            internal static readonly int _CoreRadius = Shader.PropertyToID("_CoreRadius");
            internal static readonly int _TanHalfAngle = Shader.PropertyToID("_TanHalfAngle");
            internal static readonly int _zTest = Shader.PropertyToID("_ZTest");
        }

        private void UpdateName()
        {
            var suffix = " (" + groupIndex + ", " + index + ")";
            name = "StageLight" + suffix;
            displayName = "ステージライト" + suffix;
        }

        private float CalculateEffectiveRange()
        {
            return spotRange * rangeMultiplier;
        }

        private Camera GetCurrentCamera()
        {
#if UNITY_EDITOR
            // EditMode時はSceneViewのカメラを使用
            if (!Application.isPlaying)
            {
                SceneView sceneView = SceneView.lastActiveSceneView;
                return sceneView != null ? sceneView.camera : null;
            }
#endif

            // PlayMode時はメインカメラを使用
            return Camera.main;
        }
    }
}