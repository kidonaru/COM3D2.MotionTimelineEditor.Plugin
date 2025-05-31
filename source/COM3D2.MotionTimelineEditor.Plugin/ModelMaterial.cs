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
            _MatcapMaskColor,
            _RimLightColor,
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

        public ModelMaterialController controller { get; private set; }
        public Material material { get; private set; }

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

            material.name = material.name.Replace(" (Instance)", "");
        }

        public void UpdateMaterial(Material material)
        {
            this.material = material;
            Init();
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
                MTEUtils.LogDebug($"Material '{material.name}' does not have color property '{type}'.");
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

        public void Reset()
        {
            foreach (var type in hasColorProperties)
            {
                if (HasColor(type))
                {
                    SetColor(type, GetInitialColor(type));
                }
            }

            foreach (var type in hasValueProperties)
            {
                if (HasValue(type))
                {
                    SetValue(type, GetInitialValue(type));
                }
            }
        }

        public void Apply(TransformDataModelMaterial trans)
        {
            SetColor(ColorPropertyType._Color, trans.color);
            SetColor(ColorPropertyType._ShadowColor, trans.ShadowColor);
            SetColor(ColorPropertyType._RimColor, trans.RimColor);
            SetColor(ColorPropertyType._OutlineColor, trans.OutlineColor);
            SetValue(ValuePropertyType._Shininess, trans.Shininess);
            SetValue(ValuePropertyType._OutlineWidth, trans.OutlineWidth);
            SetValue(ValuePropertyType._RimPower, trans.RimPower);
            SetValue(ValuePropertyType._RimShift, trans.RimShift);

            // NPR用プロパティ
            SetColor(ColorPropertyType._EmissionColor, trans.EmissionColor);
            SetColor(ColorPropertyType._MatcapColor, trans.MatcapColor);
            SetColor(ColorPropertyType._MatcapMaskColor, trans.MatcapMaskColor);
            SetColor(ColorPropertyType._RimLightColor, trans.RimLightColor);
            SetValue(ValuePropertyType._NormalValue, trans.NormalValue);
            SetValue(ValuePropertyType._ParallaxValue, trans.ParallaxValue);
            SetValue(ValuePropertyType._MatcapValue, trans.MatcapValue);
            SetValue(ValuePropertyType._MatcapMaskValue, trans.MatcapMaskValue);
            SetValue(ValuePropertyType._EmissionValue, trans.EmissionValue);
            SetValue(ValuePropertyType._EmissionHDRExposure, trans.EmissionHDRExposure);
            SetValue(ValuePropertyType._EmissionPower, trans.EmissionPower);
            SetValue(ValuePropertyType._RimLightValue, trans.RimLightValue);
            SetValue(ValuePropertyType._RimLightPower, trans.RimLightPower);
            SetValue(ValuePropertyType._MetallicValue, trans.MetallicValue);
            SetValue(ValuePropertyType._SmoothnessValue, trans.SmoothnessValue);
            SetValue(ValuePropertyType._OcclusionValue, trans.OcclusionValue);
        }

        public void Lerp(
            TransformDataModelMaterial start,
            TransformDataModelMaterial end,
            float t)
        {
            if (start.color != end.color)
            {
                SetColor(ColorPropertyType._Color, Color.Lerp(start.color, end.color, t));
            }

            if (start.ShadowColor != end.ShadowColor)
            {
                SetColor(ColorPropertyType._ShadowColor, Color.Lerp(start.ShadowColor, end.ShadowColor, t));
            }

            if (start.RimColor != end.RimColor)
            {
                SetColor(ColorPropertyType._RimColor, Color.Lerp(start.RimColor, end.RimColor, t));
            }

            if (start.OutlineColor != end.OutlineColor)
            {
                SetColor(ColorPropertyType._OutlineColor, Color.Lerp(start.OutlineColor, end.OutlineColor, t));
            }

            if (start.Shininess != end.Shininess)
            {
                SetValue(ValuePropertyType._Shininess, Mathf.Lerp(start.Shininess, end.Shininess, t));
            }

            if (start.OutlineWidth != end.OutlineWidth)
            {
                SetValue(ValuePropertyType._OutlineWidth, Mathf.Lerp(start.OutlineWidth, end.OutlineWidth, t));
            }

            if (start.RimPower != end.RimPower)
            {
                SetValue(ValuePropertyType._RimPower, Mathf.Lerp(start.RimPower, end.RimPower, t));
            }

            if (start.RimShift != end.RimShift)
            {
                SetValue(ValuePropertyType._RimShift, Mathf.Lerp(start.RimShift, end.RimShift, t));
            }

            // NPR用プロパティ
            if (start.EmissionColor != end.EmissionColor)
            {
                SetColor(ColorPropertyType._EmissionColor, Color.Lerp(start.EmissionColor, end.EmissionColor, t));
            }

            if (start.MatcapColor != end.MatcapColor)
            {
                SetColor(ColorPropertyType._MatcapColor, Color.Lerp(start.MatcapColor, end.MatcapColor, t));
            }

            if (start.MatcapMaskColor != end.MatcapMaskColor)
            {
                SetColor(ColorPropertyType._MatcapMaskColor, Color.Lerp(start.MatcapMaskColor, end.MatcapMaskColor, t));
            }

            if (start.RimLightColor != end.RimLightColor)
            {
                SetColor(ColorPropertyType._RimLightColor, Color.Lerp(start.RimLightColor, end.RimLightColor, t));
            }

            if (start.NormalValue != end.NormalValue)
            {
                SetValue(ValuePropertyType._NormalValue, Mathf.Lerp(start.NormalValue, end.NormalValue, t));
            }

            if (start.ParallaxValue != end.ParallaxValue)
            {
                SetValue(ValuePropertyType._ParallaxValue, Mathf.Lerp(start.ParallaxValue, end.ParallaxValue, t));
            }

            if (start.MatcapValue != end.MatcapValue)
            {
                SetValue(ValuePropertyType._MatcapValue, Mathf.Lerp(start.MatcapValue, end.MatcapValue, t));
            }

            if (start.MatcapMaskValue != end.MatcapMaskValue)
            {
                SetValue(ValuePropertyType._MatcapMaskValue, Mathf.Lerp(start.MatcapMaskValue, end.MatcapMaskValue, t));
            }

            if (start.EmissionValue != end.EmissionValue)
            {
                SetValue(ValuePropertyType._EmissionValue, Mathf.Lerp(start.EmissionValue, end.EmissionValue, t));
            }

            if (start.EmissionHDRExposure != end.EmissionHDRExposure)
            {
                SetValue(ValuePropertyType._EmissionHDRExposure, Mathf.Lerp(start.EmissionHDRExposure, end.EmissionHDRExposure, t));
            }

            if (start.EmissionPower != end.EmissionPower)
            {
                SetValue(ValuePropertyType._EmissionPower, Mathf.Lerp(start.EmissionPower, end.EmissionPower, t));
            }

            if (start.RimLightValue != end.RimLightValue)
            {
                SetValue(ValuePropertyType._RimLightValue, Mathf.Lerp(start.RimLightValue, end.RimLightValue, t));
            }

            if (start.RimLightPower != end.RimLightPower)
            {
                SetValue(ValuePropertyType._RimLightPower, Mathf.Lerp(start.RimLightPower, end.RimLightPower, t));
            }

            if (start.MetallicValue != end.MetallicValue)
            {
                SetValue(ValuePropertyType._MetallicValue, Mathf.Lerp(start.MetallicValue, end.MetallicValue, t));
            }

            if (start.SmoothnessValue != end.SmoothnessValue)
            {
                SetValue(ValuePropertyType._SmoothnessValue, Mathf.Lerp(start.SmoothnessValue, end.SmoothnessValue, t));
            }

            if (start.OcclusionValue != end.OcclusionValue)
            {
                SetValue(ValuePropertyType._OcclusionValue, Mathf.Lerp(start.OcclusionValue, end.OcclusionValue, t));
            }
        }
    }
}