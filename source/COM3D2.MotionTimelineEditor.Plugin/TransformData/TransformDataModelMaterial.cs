
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using static COM3D2.MotionTimelineEditor.Plugin.ModelMaterial;

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
            ColorA = 4,
            ShadowColorR = 5,
            ShadowColorG = 6,
            ShadowColorB = 7,
            ShadowColorA = 8,
            RimColorR = 9,
            RimColorG = 10,
            RimColorB = 11,
            RimColorA = 12,
            OutlineColorR = 13,
            OutlineColorG = 14,
            OutlineColorB = 15,
            OutlineColorA = 16,
            Shininess = 17,
            OutlineWidth = 18,
            RimPower = 19,
            RimShift = 20,
            EmissionColorR = 21,
            EmissionColorG = 22,
            EmissionColorB = 23,
            EmissionColorA = 24,
            MatcapColorR = 25,
            MatcapColorG = 26,
            MatcapColorB = 27,
            MatcapColorA = 28,
            MatcapMaskColorR = 29,
            MatcapMaskColorG = 30,
            MatcapMaskColorB = 31,
            MatcapMaskColorA = 32,
            RimLightColorR = 33,
            RimLightColorG = 34,
            RimLightColorB = 35,
            RimLightColorA = 36,
            NormalValue = 37,
            ParallaxValue = 38,
            MatcapValue = 39,
            MatcapMaskValue = 40,
            EmissionValue = 41,
            EmissionHDRExposure = 42,
            EmissionPower = 43,
            RimLightValue = 44,
            RimLightPower = 45,
            MetallicValue = 46,
            SmoothnessValue = 47,
            OcclusionValue = 48,
        }

        public static TransformDataModelMaterial defaultTrans = new TransformDataModelMaterial();

        public override TransformType type => TransformType.ModelMaterial;

        public override int valueCount => 49;

        public override bool hasColor => true;
        public override bool hasEasing => true;

        public override ValueData[] colorValues => new ValueData[]
        {
            values[(int)Index.ColorR],
            values[(int)Index.ColorG],
            values[(int)Index.ColorB],
            values[(int)Index.ColorA],
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
                "ShadowColor.a",
                new CustomValueInfo
                {
                    index = (int)Index.ShadowColorA,
                    name = "影色 A",
                    defaultValue = 1f,
                }
            },
            {
                "RimColor.r",
                new CustomValueInfo
                {
                    index = (int)Index.RimColorR,
                    name = "リム色 R",
                    defaultValue = 0f,
                }
            },
            {
                "RimColor.g",
                new CustomValueInfo
                {
                    index = (int)Index.RimColorG,
                    name = "リム色 G",
                    defaultValue = 0f,
                }
            },
            {
                "RimColor.b",
                new CustomValueInfo
                {
                    index = (int)Index.RimColorB,
                    name = "リム色 B",
                    defaultValue = 0f,
                }
            },
            {
                "RimColor.a",
                new CustomValueInfo
                {
                    index = (int)Index.RimColorA,
                    name = "リム色 A",
                    defaultValue = 1f,
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
                "OutlineColor.a",
                new CustomValueInfo
                {
                    index = (int)Index.OutlineColorA,
                    name = "アウトライン A",
                    defaultValue = 1f,
                }
            },
            {
                "_Shininess",
                new CustomValueInfo
                {
                    index = (int)Index.Shininess,
                    name = "光沢度",
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
                    name = "アウトライン幅",
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
                    name = "リムパワー",
                    min = -30f,
                    max = 30f,
                    step = 0.01f,
                }
            },
            {
                "_RimShift",
                new CustomValueInfo
                {
                    index = (int)Index.RimShift,
                    name = "リムシフト",
                    min = 0f,
                    max = 1f,
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
                "EmissionColor.a",
                new CustomValueInfo
                {
                    index = (int)Index.EmissionColorA,
                    name = "発光色 A",
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
                "MatcapColor.a",
                new CustomValueInfo
                {
                    index = (int)Index.MatcapColorA,
                    name = "マットキャップ色 A",
                    defaultValue = 1f,
                }
            },
            {
                "MatcapMaskColor.r",
                new CustomValueInfo
                {
                    index = (int)Index.MatcapMaskColorR,
                    name = "マットキャップマスク色 R",
                    defaultValue = 1f,
                }
            },
            {
                "MatcapMaskColor.g",
                new CustomValueInfo
                {
                    index = (int)Index.MatcapMaskColorG,
                    name = "マットキャップマスク色 G",
                    defaultValue = 1f,
                }
            },
            {
                "MatcapMaskColor.b",
                new CustomValueInfo
                {
                    index = (int)Index.MatcapMaskColorB,
                    name = "マットキャップマスク色 B",
                    defaultValue = 1f,
                }
            },
            {
                "MatcapMaskColor.a",
                new CustomValueInfo
                {
                    index = (int)Index.MatcapMaskColorA,
                    name = "マットキャップマスク色 A",
                    defaultValue = 1f,
                }
            },
            {
                "RimLightColor.r",
                new CustomValueInfo
                {
                    index = (int)Index.RimLightColorR,
                    name = "リムライト色 R",
                    defaultValue = 1f,
                }
            },
            {
                "RimLightColor.g",
                new CustomValueInfo
                {
                    index = (int)Index.RimLightColorG,
                    name = "リムライト色 G",
                    defaultValue = 1f,
                }
            },
            {
                "RimLightColor.b",
                new CustomValueInfo
                {
                    index = (int)Index.RimLightColorB,
                    name = "リムライト色 B",
                    defaultValue = 1f,
                }
            },
            {
                "RimLightColor.a",
                new CustomValueInfo
                {
                    index = (int)Index.RimLightColorA,
                    name = "リムライト色 A",
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
            values[(int)Index.ShadowColorB],
            values[(int)Index.ShadowColorA],
        };

        public ValueData[] RimColorValues => new ValueData[]
        {
            values[(int)Index.RimColorR], 
            values[(int)Index.RimColorG], 
            values[(int)Index.RimColorB],
            values[(int)Index.RimColorA],
        };

        public ValueData[] OutlineColorValues => new ValueData[]
        {
            values[(int)Index.OutlineColorR], 
            values[(int)Index.OutlineColorG], 
            values[(int)Index.OutlineColorB],
            values[(int)Index.OutlineColorA],
        };

        public ValueData ShininessValue => values[(int)Index.Shininess];
        public ValueData OutlineWidthValue => values[(int)Index.OutlineWidth];
        public ValueData RimPowerValue => values[(int)Index.RimPower];
        public ValueData RimShiftValue => values[(int)Index.RimShift];

        public ValueData[] EmissionColorValues => new ValueData[]
        {
            values[(int)Index.EmissionColorR], 
            values[(int)Index.EmissionColorG], 
            values[(int)Index.EmissionColorB],
            values[(int)Index.EmissionColorA],
        };

        public ValueData[] MatcapColorValues => new ValueData[]
        {
            values[(int)Index.MatcapColorR], 
            values[(int)Index.MatcapColorG], 
            values[(int)Index.MatcapColorB],
            values[(int)Index.MatcapColorA],
        };

        public ValueData[] MatcapMaskColorValues => new ValueData[]
        {
            values[(int)Index.MatcapMaskColorR], 
            values[(int)Index.MatcapMaskColorG], 
            values[(int)Index.MatcapMaskColorB],
            values[(int)Index.MatcapMaskColorA],
        };

        public ValueData[] RimLightColorValues => new ValueData[]
        {
            values[(int)Index.RimLightColorR], 
            values[(int)Index.RimLightColorG], 
            values[(int)Index.RimLightColorB],
            values[(int)Index.RimLightColorA],
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

        public Color MatcapMaskColor
        {
            get => MatcapMaskColorValues.ToColor();
            set => MatcapMaskColorValues.FromColor(value);
        }

        public Color RimLightColor
        {
            get => RimLightColorValues.ToColor();
            set => RimLightColorValues.FromColor(value);
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

        public void Apply(ModelMaterial material)
        {
            color = material.GetColor(ColorPropertyType._Color);
            ShadowColor = material.GetColor(ColorPropertyType._ShadowColor);
            RimColor = material.GetColor(ColorPropertyType._RimColor);
            OutlineColor = material.GetColor(ColorPropertyType._OutlineColor);

            Shininess = material.GetValue(ValuePropertyType._Shininess);
            OutlineWidth = material.GetValue(ValuePropertyType._OutlineWidth);
            RimPower = material.GetValue(ValuePropertyType._RimPower);
            RimShift = material.GetValue(ValuePropertyType._RimShift);

            // NPR用プロパティの追加
            EmissionColor = material.GetColor(ColorPropertyType._EmissionColor);
            MatcapColor = material.GetColor(ColorPropertyType._MatcapColor);
            MatcapMaskColor = material.GetColor(ColorPropertyType._MatcapMaskColor);
            RimLightColor = material.GetColor(ColorPropertyType._RimLightColor);

            NormalValue = material.GetValue(ValuePropertyType._NormalValue);
            ParallaxValue = material.GetValue(ValuePropertyType._ParallaxValue);
            MatcapValue = material.GetValue(ValuePropertyType._MatcapValue);
            MatcapMaskValue = material.GetValue(ValuePropertyType._MatcapMaskValue);
            EmissionValue = material.GetValue(ValuePropertyType._EmissionValue);
            EmissionHDRExposure = material.GetValue(ValuePropertyType._EmissionHDRExposure);
            EmissionPower = material.GetValue(ValuePropertyType._EmissionPower);
            RimLightValue = material.GetValue(ValuePropertyType._RimLightValue);
            RimLightPower = material.GetValue(ValuePropertyType._RimLightPower);
            MetallicValue = material.GetValue(ValuePropertyType._MetallicValue);
            SmoothnessValue = material.GetValue(ValuePropertyType._SmoothnessValue);
            OcclusionValue = material.GetValue(ValuePropertyType._OcclusionValue);
        }
    }
}