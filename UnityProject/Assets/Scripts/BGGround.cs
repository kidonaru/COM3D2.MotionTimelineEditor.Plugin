using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace COM3D2.MotionTimelineEditor.Plugin
{
    [ExecuteInEditMode]
    public class BGGround : MonoBehaviour
    {
        [SerializeField]
        private Color _color = DefaultColor;
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
                transform.localPosition = _position;
            }
        }

        [SerializeField]
        private Vector3 _scale = DefaultScale;
        public Vector3 scale
        {
            get
            {
                return _scale;
            }
            set
            {
                if (_scale == value) return;
                _scale = value;
                transform.localScale = _scale;
            }
        }

        public bool visible
        {
            get
            {
                return gameObject.activeSelf;
            }
            set
            {
                if (gameObject.activeSelf != value)
                {
                    gameObject.SetActive(value);
                }
            }
        }

        public static Vector3 DefaultPosition = new Vector3(0f, 0f, 0f);
        public static Vector3 DefaultScale = new Vector3(100f, 100f, 100f);
        public static Color DefaultColor = new Color(0f, 0f, 0f, 1f);

        private Mesh _mesh;
        private Material _material;
        private MeshFilter _meshFilter;
        private MeshRenderer _meshRenderer;

        void OnEnable()
        {
            Initialize();
        }

        void Reset()
        {
            Initialize();
        }

        void OnDestroy()
        {
            Refresh();
        }

        void Refresh()
        {
            if (_material != null)
            {
                DestroyImmediate(_material);
                _material = null;
            }

            if (_mesh != null)
            {
                DestroyImmediate(_mesh);
                _mesh = null;
            }
        }

        void OnValidate()
        {
            UpdateMaterial();
        }

        public void Initialize()
        {
            Refresh();

            _meshFilter = gameObject.GetComponent<MeshFilter>();
            if (_meshFilter == null)
            {
                _meshFilter = gameObject.AddComponent<MeshFilter>();
            }

            _meshRenderer = gameObject.GetComponent<MeshRenderer>();
            if (_meshRenderer == null)
            {
                _meshRenderer = gameObject.AddComponent<MeshRenderer>();
                _meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            }

            if (_material == null)
            {
#if COM3D2
                _material = new Material(Shader.Find("CM3D2/Lighted"));
#else
                _material = new Material(Shader.Find("Standard"));
#endif
                _meshRenderer.sharedMaterial = _material;
            }

            if (_mesh == null)
            {
                _mesh = new Mesh();
                _meshFilter.sharedMesh = _mesh;
            }

            transform.localPosition = _position;
            transform.localScale = _scale;

            UpdateMesh();
            UpdateMaterial();
        }

        void UpdateMesh()
        {
            _mesh.Clear();

            // 頂点の計算
            var size = 0.5f;
            Vector3[] vertices = new Vector3[]
            {
                new Vector3(-size, 0, -size),
                new Vector3(-size, 0, size),
                new Vector3(size, 0, size),
                new Vector3(size, 0, -size),
            };

            // インデックスの計算
            int[] triangles = new int[]
            {
                0, 1, 2,
                0, 2, 3,
            };

            _mesh.vertices = vertices;
            _mesh.triangles = triangles;
        }

        private void UpdateMaterial()
        {
            if (_material != null)
            {
                _material.SetColor(Uniforms._Color, color);
            }
        }

        private static class Uniforms
        {
            internal static readonly int _Color = Shader.PropertyToID("_Color");
        }
    }
}