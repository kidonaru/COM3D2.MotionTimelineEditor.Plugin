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

            // NPR
            _EmissionColor,
            _MatcapColor,
            _ReflectionColor,
        }

        public enum ValuePropertyType
        {
            _Shininess,
            _OutlineWidth,
            _RimPower,
            _RimShift,

            // NPR
            _NormalValue,
            _ParallaxValue,
            _MatcapValue,
            _MatcapMaskValue,
            _EmissionValue,
            _EmissionHDRExposure,
            _EmissionPower,
            _RimLightValue,
            _RimLightPower,
            _MetallicValue,
            _SmoothnessValue,
            _OcclusionValue,
        }

        public static readonly List<ColorPropertyType> ColorPropertyTypes =
            MTEUtils.GetEnumValues<ColorPropertyType>().ToList();

        public static readonly List<int> ColorPropertyNameIds =
            ColorPropertyTypes.Select(x => Shader.PropertyToID(x.ToString())).ToList();

        public static readonly List<ValuePropertyType> ValuePropertyTypes =
            MTEUtils.GetEnumValues<ValuePropertyType>().ToList();

        public static readonly List<int> ValuePropertyNameIds =
            ValuePropertyTypes.Select(x => Shader.PropertyToID(x.ToString())).ToList();

        public ModelMaterialController controller;
        public Material material;

        public IModelStat model => controller.model;

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

        private HashSet<ValuePropertyType> hasValueProperties = new HashSet<ValuePropertyType>();
        private List<float> initialValues = new List<float>();

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
            hasValueProperties.Clear();
            initialValues.Clear();

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

            for (int i = 0; i < ValuePropertyNameIds.Count; i++)
            {
                var nameId = ValuePropertyNameIds[i];
                if (material.HasProperty(nameId))
                {
                    hasValueProperties.Add((ValuePropertyType)i);
                    initialValues.Add(material.GetFloat(nameId));
                }
                else
                {
                    initialValues.Add(0f);
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

        public bool HasValue(ValuePropertyType type)
        {
            return hasValueProperties.Contains(type);
        }

        public float GetValue(ValuePropertyType type)
        {
            if (!hasValueProperties.Contains(type))
            {
                return 0f;
            }

            return material.GetFloat(ValuePropertyNameIds[(int)type]);
        }

        public void SetValue(ValuePropertyType type, float value)
        {
            if (!hasValueProperties.Contains(type))
            {
                return;
            }

            material.SetFloat(ValuePropertyNameIds[(int)type], value);
        }

        public float GetInitialValue(ValuePropertyType type)
        {
            return initialValues[(int)type];
        }
    }

    public class ModelMaterialController : MonoBehaviour
    {
        public IModelStat model;

        private Renderer _renderer = null;
        public Renderer renderer
        {
            get
            {
                if (_renderer == null)
                {
                    _renderer = GetComponentInChildren<Renderer>();
                }
                return _renderer;
            }
        }

        public List<ModelMaterial> materials = new List<ModelMaterial>();

        public static ModelMaterialController GetOrCreate(IModelStat model)
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

            if (renderer == null || renderer.sharedMaterials == null)
            {
                return;
            }

            foreach (var material in renderer.sharedMaterials)
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