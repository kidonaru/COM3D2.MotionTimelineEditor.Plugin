using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    public class ModelMaterial
    {
        public enum ColorPropertyType
        {
            _Color,
            _ShadowColor,
            _RimColor,
            _OutlineColor,
        }

        public static readonly List<ColorPropertyType> ColorPropertyTypes =
            MTEUtils.GetEnumValues<ColorPropertyType>().ToList();

        public static readonly List<int> ColorPropertyNameIds =
            ColorPropertyTypes.Select(x => Shader.PropertyToID(x.ToString())).ToList();

        public ModelMaterialController controller;
        public Material material;

        public StudioModelStat model => controller.model;

        public string name
        {
            get => string.Format("{0}/{1}", model.name, material != null ? material.name : "");
        }

        public string displayName
        {
            get => material.name;
        }

        private HashSet<ColorPropertyType> hasColorProperties = new HashSet<ColorPropertyType>();
        private List<Color> initialColors = new List<Color>();

        public ModelMaterial(ModelMaterialController controller, Material material)
        {
            this.controller = controller;
            this.material = material;

            if (material == null)
            {
                return;
            }

            Init();
        }

        public void Init()
        {
            hasColorProperties.Clear();
            initialColors.Clear();

            for (int i = 0; i < ColorPropertyNameIds.Count; i++)
            {
                var nameId = ColorPropertyNameIds[i];
                if (material.HasProperty(nameId))
                {
                    hasColorProperties.Add((ColorPropertyType)i);
                    initialColors.Add(material.GetColor(nameId));
                }
                else
                {
                    initialColors.Add(Color.black);
                }
            }
        }

        public bool HasColor(ColorPropertyType type)
        {
            return hasColorProperties.Contains(type);
        }

        public Color GetColor(ColorPropertyType type)
        {
            if (!hasColorProperties.Contains(type))
            {
                return Color.black;
            }

            return material.GetColor(ColorPropertyNameIds[(int)type]);
        }

        public void SetColor(ColorPropertyType type, Color color)
        {
            if (!hasColorProperties.Contains(type))
            {
                return;
            }

            material.SetColor(ColorPropertyNameIds[(int)type], color);
        }

        public Color GetInitialColor(ColorPropertyType type)
        {
            return initialColors[(int)type];
        }
    }

    public class ModelMaterialController : MonoBehaviour
    {
        public StudioModelStat model;

        private SkinnedMeshRenderer _meshRenderer = null;
        public SkinnedMeshRenderer meshRenderer
        {
            get
            {
                if (_meshRenderer == null)
                {
                    _meshRenderer = GetComponentInChildren<SkinnedMeshRenderer>();
                }
                return _meshRenderer;
            }
        }

        public List<ModelMaterial> materials = new List<ModelMaterial>();

        public static ModelMaterialController GetOrCreate(StudioModelStat model)
        {
            var transform = model.transform;
            var go = transform.gameObject;

            var controller = go.GetComponent<ModelMaterialController>();
            if (controller == null)
            {
                controller = go.AddComponent<ModelMaterialController>();
                controller.Init();
            }

            controller.model = model;
            return controller;
        }

        public void Init()
        {
            materials.Clear();

            if (meshRenderer == null || meshRenderer.materials == null)
            {
                return;
            }

            foreach (var material in meshRenderer.materials)
            {
                materials.Add(new ModelMaterial(this, material));
            }
        }

        public ModelMaterial GetMaterial(int index)
        {
            if (index < 0 || index >= materials.Count)
            {
                return null;
            }

            return materials[index];
        }
    }
}