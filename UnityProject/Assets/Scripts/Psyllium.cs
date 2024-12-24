using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace COM3D2.MotionTimelineEditor.Plugin
{
    [ExecuteInEditMode]
    public class Psyllium : MonoBehaviour
    {
        public Material material;
        public PsylliumController controller;
        public int randomTimeIndex;
        public int colorIndex;
        public Vector3 randomPositionParam;
        public Vector3 randomRotationParam;
        public Vector3 barPosition;
        public Vector3 barRotation;
        public bool isLeftHand;

        private MeshFilter _filter;
        private MeshRenderer _renderer;

        public PsylliumBarConfig barConfig
        {
            get
            {
                return controller.barConfig;
            }
        }

        public PsylliumAnimationConfig animationConfig
        {
            get
            {
                return controller.animationConfig;
            }
        }

#if COM3D2
        private static TimelineBundleManager bundleManager => TimelineBundleManager.instance;
#endif

        void Awake()
        {
            _filter = GetOrAddComponent<MeshFilter>();
            _renderer = GetOrAddComponent<MeshRenderer>();
            _renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            _renderer.receiveShadows = false;

#if COM3D2
            material = bundleManager.LoadMaterial("Psyllium");
#else
            material = new Material(Shader.Find("MTE/Psyllium"));
            material.SetTexture("_MainTex", Resources.Load<Texture2D>("psyllium"));
#endif
            _renderer.material = material;
        }

        public void UpdateMesh()
        {
            var halfWidth = barConfig.width / 2;
            var barHeight = barConfig.height;
            var barRadius = barConfig.radius;
            var barTopThreshold = barConfig.topThreshold;

            Mesh mesh = new Mesh ();
            var vertices = new Vector3[] {
                new Vector3(-halfWidth, 0, 0),  // 0
                new Vector3(-halfWidth, 0, 0),  // 1
                new Vector3( halfWidth, 0, 0),  // 2
                new Vector3( halfWidth, 0, 0),  // 3
                new Vector3(-halfWidth, barHeight, 0),  // 4
                new Vector3( halfWidth, barHeight, 0),  // 5
                new Vector3(-halfWidth, barHeight, 0),  // 6 
                new Vector3( halfWidth, barHeight, 0),  // 7
            };

            for (int i = 0; i < vertices.Length; i++)
            {
                var v = vertices[i];
                v.y += barConfig.positionY;
                vertices[i] = v;
            }

            mesh.vertices = vertices;

            mesh.uv = new Vector2[] {
                new Vector2(0, 0),
                new Vector2(0, barTopThreshold),
                new Vector2(1, 0),
                new Vector2(1, barTopThreshold),
                new Vector2(0, 1 - barTopThreshold),
                new Vector2(1, 1 - barTopThreshold),
                new Vector2(0, 1),
                new Vector2(1, 1),
            };

            mesh.uv2 = new Vector2[] {
                new Vector2(-barRadius, 0),
                new Vector2(0, 0),
                new Vector2(-barRadius, 0),
                new Vector2(0, 0),
                new Vector2(0, 0),
                new Vector2(0, 0),
                new Vector2(barRadius, 0),
                new Vector2(barRadius, 0),
            };

            mesh.triangles = new int[] {
                0, 1, 2,
                1, 3, 2,
                1, 4, 3,
                4, 5, 3,
                4, 6, 5,
                6, 7, 5,
            };

            _filter.sharedMesh = mesh;
        }

        public void UpdateMaterial()
        {
            if (material == null || controller == null)
            {
                return;
            }

            var position1 = animationConfig.position1;
            var position2 = animationConfig.position2;
            var rotation1 = animationConfig.rotation1;
            var rotation2 = animationConfig.rotation2;

            position1 += new Vector3(
                randomPositionParam.x * animationConfig.randomPositionRange.x,
                randomPositionParam.y * animationConfig.randomPositionRange.y,
                randomPositionParam.z * animationConfig.randomPositionRange.z
            );

            position2 += new Vector3(
                randomPositionParam.x * animationConfig.randomPositionRange.x,
                randomPositionParam.y * animationConfig.randomPositionRange.y,
                randomPositionParam.z * animationConfig.randomPositionRange.z
            );

            rotation1 += new Vector3(
                randomRotationParam.x * animationConfig.randomRotationRange.x,
                randomRotationParam.y * animationConfig.randomRotationRange.y,
                randomRotationParam.z * animationConfig.randomRotationRange.z
            );

            rotation2 += new Vector3(
                randomRotationParam.x * animationConfig.randomRotationRange.x,
                randomRotationParam.y * animationConfig.randomRotationRange.y,
                randomRotationParam.z * animationConfig.randomRotationRange.z
            );

            if (animationConfig.mirrorRotationZ && !isLeftHand)
            {
                rotation1.z = -rotation1.z;
                rotation2.z = -rotation2.z;
            }

            var cutoffHeight = animationConfig.cutoffHeight;

            material.SetColor(Uniforms._Color1, barConfig.GetColorA(colorIndex));
            material.SetColor(Uniforms._Color2, barConfig.GetColorB(colorIndex));
            material.SetColor(Uniforms._Color3, barConfig.GetColorC(colorIndex));
            material.SetFloat(Uniforms._CutoffAlpha, barConfig.cutoffAlpha);
            material.SetFloat(Uniforms._CutoffHeight, cutoffHeight);
            material.SetVector(Uniforms._Position1, position1);
            material.SetVector(Uniforms._Position2, position2);
            material.SetVector(Uniforms._Rotation1, rotation1);
            material.SetVector(Uniforms._Rotation2, rotation2);
            material.SetVector(Uniforms._BarPosition, barPosition);
            material.SetVector(Uniforms._BarRotation, barRotation);
        }

        private static class Uniforms
        {
            internal static readonly int _Color1 = Shader.PropertyToID("_Color1");
            internal static readonly int _Color2 = Shader.PropertyToID("_Color2");
            internal static readonly int _Color3 = Shader.PropertyToID("_Color3");
            internal static readonly int _CutoffAlpha = Shader.PropertyToID("_CutoffAlpha");
            internal static readonly int _CutoffHeight = Shader.PropertyToID("_CutoffHeight");
            internal static readonly int _AnimationTime = Shader.PropertyToID("_AnimationTime");
            internal static readonly int _Position1 = Shader.PropertyToID("_Position1");
            internal static readonly int _Rotation1 = Shader.PropertyToID("_Rotation1");
            internal static readonly int _Position2 = Shader.PropertyToID("_Position2");
            internal static readonly int _Rotation2 = Shader.PropertyToID("_Rotation2");
            internal static readonly int _BarPosition = Shader.PropertyToID("_BarPosition");
            internal static readonly int _BarRotation = Shader.PropertyToID("_BarRotation");
        }

        public void UpdateTime()
        {
            if (material == null || controller == null)
            {
                return;
            }

            var t = controller.GetAnimationTime(randomTimeIndex);

            material.SetFloat(Uniforms._AnimationTime, t);
        }

        public T GetOrAddComponent<T>() where T : Component
        {
            T val = this.GetComponent<T>();
            if ((Object)val == (Object)null)
            {
                val = this.gameObject.AddComponent<T>();
            }
            return val;
        }
    }
}
