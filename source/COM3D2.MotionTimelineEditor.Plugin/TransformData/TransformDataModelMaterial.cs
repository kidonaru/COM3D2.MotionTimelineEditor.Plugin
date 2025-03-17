
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    public class TransformDataModelMaterial : TransformDataBase
    {
        public enum Index
        {
            Easing = 0,
            ColorR = 1,
            ColorG = 2,
            ColorB = 3,
            ShadowColorR = 4,
            ShadowColorG = 5,
            ShadowColorB = 6,
            RimColorR = 7,
            RimColorG = 8,
            RimColorB = 9,
            OutlineColorR = 10,
            OutlineColorG = 11,
            OutlineColorB = 12,
            Shininess = 13,
            OutlineWidth = 14,
            RimPower = 15,
            RimShift = 16,
            EmissionColorR = 17,
            EmissionColorG = 18,
            EmissionColorB = 19,
            MatcapColorR = 20,
            MatcapColorG = 21,
            MatcapColorB = 22,
            ReflectionColorR = 23,
            ReflectionColorG = 24,
            ReflectionColorB = 25,
            NormalValue = 26,
            ParallaxValue = 27,
            MatcapValue = 28,
            MatcapMaskValue = 29,
            EmissionValue = 30,
            EmissionHDRExposure = 31,
            EmissionPower = 32,
            RimLightValue = 33,
            RimLightPower = 34,
            MetallicValue = 35,
            SmoothnessValue = 36,
            OcclusionValue = 37,
        }

        public static TransformDataModelMaterial defaultTrans = new TransformDataModelMaterial();

        public override TransformType type => TransformType.ModelMaterial;

        public override int valueCount => 38;

        public override bool hasColor => true;
        public override bool hasEasing => true;

        public override ValueData[] colorValues => new ValueData[]
        {
            values[(int)Index.ColorR],
            values[(int)Index.ColorG],
            values[(int)Index.ColorB],
        };

        public override ValueData easingValue => values[(int)Index.Easing];

        public override Color initialColor
        {
            get => Color.white;
        }

        public TransformDataModelMaterial()
        {
        }

        private readonly static Dictionary<string, CustomValueInfo> CustomValueInfoMap = new Dictionary<string, CustomValueInfo>
        {
            {
                "ShadowColor.r",
                new CustomValueInfo
                {
                    index = (int)Index.ShadowColorR,
                    name = "影色 R",
                    defaultValue = 0f,
                }
            },
            {
                "ShadowColor.g",
                new CustomValueInfo
                {
                    index = (int)Index.ShadowColorG,
                    name = "影色 G",
                    defaultValue = 0f,
                }
            },
            {
                "ShadowColor.b",
                new CustomValueInfo
                {
                    index = (int)Index.ShadowColorB,
                    name = "影色 B",
                    defaultValue = 0f,
                }
            },
            {
                "RimColor.r",
                new CustomValueInfo
                {
                    index = (int)Index.RimColorR,
                    name = "リムライト R",
                    defaultValue = 0f,
                }
            },
            {
                "RimColor.g",
                new CustomValueInfo
                {
                    index = (int)Index.RimColorG,
                    name = "リムライト G",
                    defaultValue = 0f,
                }
            },
            {
                "RimColor.b",
                new CustomValueInfo
                {
                    index = (int)Index.RimColorB,
                    name = "リムライト B",
                    defaultValue = 0f,
                }
            },
            {
                "OutlineColor.r",
                new CustomValueInfo
                {
                    index = (int)Index.OutlineColorR,
                    name = "アウトライン R",
                    defaultValue = 0f,
                }
            },
            {
                "OutlineColor.g",
                new CustomValueInfo
                {
                    index = (int)Index.OutlineColorG,
                    name = "アウトライン G",
                    defaultValue = 0f,
                }
            },
            {
                "OutlineColor.b",
                new CustomValueInfo
                {
                    index = (int)Index.OutlineColorB,
                    name = "アウトライン B",
                    defaultValue = 0f,
                }
            },
            {
                "_Shininess",
                new CustomValueInfo
                {
                    index = (int)Index.Shininess,
                    name = "_Shininess",
                    min = 0f,
                    max = 10f,
                    step = 0.01f,
                }
            },
            {
                "_OutlineWidth",
                new CustomValueInfo
                {
                    index = (int)Index.OutlineWidth,
                    name = "_OutlineWidth",
                    min = 0f,
                    max = 1f,
                    step = 0.0001f,
                }
            },
            {
                "_RimPower",
                new CustomValueInfo
                {
                    index = (int)Index.RimPower,
                    name = "_RimPower",
                    min = 0f,
                    max = 100f,
                    step = 0.01f,
                }
            },
            {
                "_RimShift",
                new CustomValueInfo
                {
                    index = (int)Index.RimShift,
                    name = "_RimShift",
                    min = 0f,
                    max = 10f,
                    step = 0.01f,
                }
            },
            // NPRの追加プロパティ
            {
                "EmissionColor.r",
                new CustomValueInfo
                {
                    index = (int)Index.EmissionColorR,
                    name = "発光色 R",
                    defaultValue = 1f,
                }
            },
            {
                "EmissionColor.g",
                new CustomValueInfo
                {
                    index = (int)Index.EmissionColorG,
                    name = "発光色 G",
                    defaultValue = 1f,
                }
            },
            {
                "EmissionColor.b",
                new CustomValueInfo
                {
                    index = (int)Index.EmissionColorB,
                    name = "発光色 B",
                    defaultValue = 1f,
                }
            },
            {
                "MatcapColor.r",
                new CustomValueInfo
                {
                    index = (int)Index.MatcapColorR,
                    name = "マットキャップ色 R",
                    defaultValue = 1f,
                }
            },
            {
                "MatcapColor.g",
                new CustomValueInfo
                {
                    index = (int)Index.MatcapColorG,
                    name = "マットキャップ色 G",
                    defaultValue = 1f,
                }
            },
            {
                "MatcapColor.b",
                new CustomValueInfo
                {
                    index = (int)Index.MatcapColorB,
                    name = "マットキャップ色 B",
                    defaultValue = 1f,
                }
            },
            {
                "ReflectionColor.r",
                new CustomValueInfo
                {
                    index = (int)Index.ReflectionColorR,
                    name = "反射色 R",
                    defaultValue = 1f,
                }
            },
            {
                "ReflectionColor.g",
                new CustomValueInfo
                {
                    index = (int)Index.ReflectionColorG,
                    name = "反射色 G",
                    defaultValue = 1f,
                }
            },
            {
                "ReflectionColor.b",
                new CustomValueInfo
                {
                    index = (int)Index.ReflectionColorB,
                    name = "反射色 B",
                    defaultValue = 1f,
                }
            },
            {
                "_NormalValue",
                new CustomValueInfo
                {
                    index = (int)Index.NormalValue,
                    name = "法線マップ強度",
                    min = 0f,
                    max = 1f,
                    step = 0.01f,
                    defaultValue = 0f,
                }
            },
            {
                "_ParallaxValue",
                new CustomValueInfo
                {
                    index = (int)Index.ParallaxValue,
                    name = "視差効果強度",
                    min = 0f,
                    max = 1f,
                    step = 0.01f,
                    defaultValue = 0f,
                }
            },
            {
                "_MatcapValue",
                new CustomValueInfo
                {
                    index = (int)Index.MatcapValue,
                    name = "マットキャップ強度",
                    min = 0f,
                    max = 1f,
                    step = 0.01f,
                    defaultValue = 0f,
                }
            },
            {
                "_MatcapMaskValue",
                new CustomValueInfo
                {
                    index = (int)Index.MatcapMaskValue,
                    name = "マットキャップマスク強度",
                    min = 0f,
                    max = 1f,
                    step = 0.01f,
                    defaultValue = 0f,
                }
            },
            {
                "_EmissionValue",
                new CustomValueInfo
                {
                    index = (int)Index.EmissionValue,
                    name = "発光強度",
                    min = 0f,
                    max = 1f,
                    step = 0.01f,
                    defaultValue = 0f,
                }
            },
            {
                "_EmissionHDRExposure",
                new CustomValueInfo
                {
                    index = (int)Index.EmissionHDRExposure,
                    name = "発光HDR露出",
                    min = 0f,
                    max = 3f,
                    step = 0.01f,
                    defaultValue = 0f,
                }
            },
            {
                "_EmissionPower",
                new CustomValueInfo
                {
                    index = (int)Index.EmissionPower,
                    name = "発光パワー",
                    min = -3f,
                    max = 3f,
                    step = 0.01f,
                    defaultValue = 0f,
                }
            },
            {
                "_RimLightValue",
                new CustomValueInfo
                {
                    index = (int)Index.RimLightValue,
                    name = "リムライト強度",
                    min = 0f,
                    max = 1f,
                    step = 0.01f,
                    defaultValue = 0f,
                }
            },
            {
                "_RimLightPower",
                new CustomValueInfo
                {
                    index = (int)Index.RimLightPower,
                    name = "リムライトパワー",
                    min = -3f,
                    max = 3f,
                    step = 0.01f,
                    defaultValue = 0f,
                }
            },
            {
                "_MetallicValue",
                new CustomValueInfo
                {
                    index = (int)Index.MetallicValue,
                    name = "金属度",
                    min = 0f,
                    max = 1f,
                    step = 0.01f,
                    defaultValue = 0f,
                }
            },
            {
                "_SmoothnessValue",
                new CustomValueInfo
                {
                    index = (int)Index.SmoothnessValue,
                    name = "滑らかさ",
                    min = 0f,
                    max = 1f,
                    step = 0.01f,
                    defaultValue = 0f,
                }
            },
            {
                "_OcclusionValue",
                new CustomValueInfo
                {
                    index = (int)Index.OcclusionValue,
                    name = "オクルージョン",
                    min = 0f,
                    max = 1f,
                    step = 0.01f,
                    defaultValue = 0f,
                }
            },
        };

        public override Dictionary<string, CustomValueInfo> GetCustomValueInfoMap()
        {
            return CustomValueInfoMap;
        }

        public ValueData[] ShadowColorValues => new ValueData[]
        {
            values[(int)Index.ShadowColorR],
            values[(int)Index.ShadowColorG],
            values[(int)Index.ShadowColorB]
        };

        public ValueData[] RimColorValues => new ValueData[]
        {
            values[(int)Index.RimColorR], 
            values[(int)Index.RimColorG], 
            values[(int)Index.RimColorB] 
        };

        public ValueData[] OutlineColorValues => new ValueData[]
        {
            values[(int)Index.OutlineColorR], 
            values[(int)Index.OutlineColorG], 
            values[(int)Index.OutlineColorB] 
        };

        public ValueData ShininessValue => values[(int)Index.Shininess];
        public ValueData OutlineWidthValue => values[(int)Index.OutlineWidth];
        public ValueData RimPowerValue => values[(int)Index.RimPower];
        public ValueData RimShiftValue => values[(int)Index.RimShift];

        public ValueData[] EmissionColorValues => new ValueData[]
        {
            values[(int)Index.EmissionColorR], 
            values[(int)Index.EmissionColorG], 
            values[(int)Index.EmissionColorB] 
        };

        public ValueData[] MatcapColorValues => new ValueData[]
        {
            values[(int)Index.MatcapColorR], 
            values[(int)Index.MatcapColorG], 
            values[(int)Index.MatcapColorB] 
        };

        public ValueData[] ReflectionColorValues => new ValueData[]
        {
            values[(int)Index.ReflectionColorR], 
            values[(int)Index.ReflectionColorG], 
            values[(int)Index.ReflectionColorB] 
        };

        public ValueData NormalValueValue => values[(int)Index.NormalValue];
        public ValueData ParallaxValueValue => values[(int)Index.ParallaxValue];
        public ValueData MatcapValueValue => values[(int)Index.MatcapValue];
        public ValueData MatcapMaskValueValue => values[(int)Index.MatcapMaskValue];
        public ValueData EmissionValueValue => values[(int)Index.EmissionValue];
        public ValueData EmissionHDRExposureValue => values[(int)Index.EmissionHDRExposure];
        public ValueData EmissionPowerValue => values[(int)Index.EmissionPower];
        public ValueData RimLightValueValue => values[(int)Index.RimLightValue];
        public ValueData RimLightPowerValue => values[(int)Index.RimLightPower];
        public ValueData MetallicValueValue => values[(int)Index.MetallicValue];
        public ValueData SmoothnessValueValue => values[(int)Index.SmoothnessValue];
        public ValueData OcclusionValueValue => values[(int)Index.OcclusionValue];

        public CustomValueInfo ShininessInfo => CustomValueInfoMap["_Shininess"];
        public CustomValueInfo OutlineWidthInfo => CustomValueInfoMap["_OutlineWidth"];
        public CustomValueInfo RimPowerInfo => CustomValueInfoMap["_RimPower"];
        public CustomValueInfo RimShiftInfo => CustomValueInfoMap["_RimShift"];

        public CustomValueInfo NormalValueInfo => CustomValueInfoMap["_NormalValue"];
        public CustomValueInfo ParallaxValueInfo => CustomValueInfoMap["_ParallaxValue"];
        public CustomValueInfo MatcapValueInfo => CustomValueInfoMap["_MatcapValue"];
        public CustomValueInfo MatcapMaskValueInfo => CustomValueInfoMap["_MatcapMaskValue"];
        public CustomValueInfo EmissionValueInfo => CustomValueInfoMap["_EmissionValue"];
        public CustomValueInfo EmissionHDRExposureInfo => CustomValueInfoMap["_EmissionHDRExposure"];
        public CustomValueInfo EmissionPowerInfo => CustomValueInfoMap["_EmissionPower"];
        public CustomValueInfo RimLightValueInfo => CustomValueInfoMap["_RimLightValue"];
        public CustomValueInfo RimLightPowerInfo => CustomValueInfoMap["_RimLightPower"];
        public CustomValueInfo MetallicValueInfo => CustomValueInfoMap["_MetallicValue"];
        public CustomValueInfo SmoothnessValueInfo => CustomValueInfoMap["_SmoothnessValue"];
        public CustomValueInfo OcclusionValueInfo => CustomValueInfoMap["_OcclusionValue"];

        public Color ShadowColor
        {
            get => ShadowColorValues.ToColor();
            set => ShadowColorValues.FromColor(value);
        }

        public Color RimColor
        {
            get => RimColorValues.ToColor();
            set => RimColorValues.FromColor(value);
        }

        public Color OutlineColor
        {
            get => OutlineColorValues.ToColor();
            set => OutlineColorValues.FromColor(value);
        }

        public float Shininess
        {
            get => ShininessValue.value;
            set => ShininessValue.value = value;
        }

        public float OutlineWidth
        {
            get => OutlineWidthValue.value;
            set => OutlineWidthValue.value = value;
        }

        public float RimPower
        {
            get => RimPowerValue.value;
            set => RimPowerValue.value = value;
        }

        public float RimShift
        {
            get => RimShiftValue.value;
            set => RimShiftValue.value = value;
        }

        public Color EmissionColor
        {
            get => EmissionColorValues.ToColor();
            set => EmissionColorValues.FromColor(value);
        }

        public Color MatcapColor
        {
            get => MatcapColorValues.ToColor();
            set => MatcapColorValues.FromColor(value);
        }

        public Color ReflectionColor
        {
            get => ReflectionColorValues.ToColor();
            set => ReflectionColorValues.FromColor(value);
        }

        public float NormalValue
        {
            get => NormalValueValue.value;
            set => NormalValueValue.value = value;
        }
        
        public float ParallaxValue
        {
            get => ParallaxValueValue.value;
            set => ParallaxValueValue.value = value;
        }
        
        public float MatcapValue
        {
            get => MatcapValueValue.value;
            set => MatcapValueValue.value = value;
        }
        
        public float MatcapMaskValue
        {
            get => MatcapMaskValueValue.value;
            set => MatcapMaskValueValue.value = value;
        }
        
        public float EmissionValue
        {
            get => EmissionValueValue.value;
            set => EmissionValueValue.value = value;
        }
        
        public float EmissionHDRExposure
        {
            get => EmissionHDRExposureValue.value;
            set => EmissionHDRExposureValue.value = value;
        }
        
        public float EmissionPower
        {
            get => EmissionPowerValue.value;
            set => EmissionPowerValue.value = value;
        }
        
        public float RimLightValue
        {
            get => RimLightValueValue.value;
            set => RimLightValueValue.value = value;
        }
        
        public float RimLightPower
        {
            get => RimLightPowerValue.value;
            set => RimLightPowerValue.value = value;
        }
        
        public float MetallicValue
        {
            get => MetallicValueValue.value;
            set => MetallicValueValue.value = value;
        }
        
        public float SmoothnessValue
        {
            get => SmoothnessValueValue.value;
            set => SmoothnessValueValue.value = value;
        }
        
        public float OcclusionValue
        {
            get => OcclusionValueValue.value;
            set => OcclusionValueValue.value = value;
        }

        public CustomValueInfo GetCustomValueInfo(ModelMaterial.ValuePropertyType propertyType)
        {
            switch (propertyType)
            {
                case ModelMaterial.ValuePropertyType._Shininess:
                    return ShininessInfo;
                case ModelMaterial.ValuePropertyType._OutlineWidth:
                    return OutlineWidthInfo;
                case ModelMaterial.ValuePropertyType._RimPower:
                    return RimPowerInfo;
                case ModelMaterial.ValuePropertyType._RimShift:
                    return RimShiftInfo;

                // NPR用プロパティの追加
                case ModelMaterial.ValuePropertyType._NormalValue:
                    return NormalValueInfo;
                case ModelMaterial.ValuePropertyType._ParallaxValue:
                    return ParallaxValueInfo;
                case ModelMaterial.ValuePropertyType._MatcapValue:
                    return MatcapValueInfo;
                case ModelMaterial.ValuePropertyType._MatcapMaskValue:
                    return MatcapMaskValueInfo;
                case ModelMaterial.ValuePropertyType._EmissionValue:
                    return EmissionValueInfo;
                case ModelMaterial.ValuePropertyType._EmissionHDRExposure:
                    return EmissionHDRExposureInfo;
                case ModelMaterial.ValuePropertyType._EmissionPower:
                    return EmissionPowerInfo;
                case ModelMaterial.ValuePropertyType._RimLightValue:
                    return RimLightValueInfo;
                case ModelMaterial.ValuePropertyType._RimLightPower:
                    return RimLightPowerInfo;
                case ModelMaterial.ValuePropertyType._MetallicValue:
                    return MetallicValueInfo;
                case ModelMaterial.ValuePropertyType._SmoothnessValue:
                    return SmoothnessValueInfo;
                case ModelMaterial.ValuePropertyType._OcclusionValue:
                    return OcclusionValueInfo;
                default:
                    return null;
            }
        }
    }
}