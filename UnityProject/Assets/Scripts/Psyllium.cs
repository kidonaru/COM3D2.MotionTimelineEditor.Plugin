using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace COM3D2.MotionTimelineEditor.Plugin
{
    [ExecuteInEditMode]
    public class Psyllium : MonoBehaviour
    {
        public PsylliumController controller;
        public int colorIndex;

        private MeshFilter _filter;
        private MeshRenderer _renderer;

        public PsylliumBarConfig barConfig
        {
            get
            {
                return controller.barConfig;
            }
        }

        void Awake()
        {
            _filter = GetOrAddComponent<MeshFilter>();
            _renderer = GetOrAddComponent<MeshRenderer>();
            _renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            _renderer.receiveShadows = false;
        }

        public void Setup(PsylliumController controller, int colorIndex)
        {
            this.controller = controller;
            this.colorIndex = colorIndex;

            _filter.sharedMesh = controller.meshes[colorIndex];
            _renderer.sharedMaterials = controller.materials;
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
