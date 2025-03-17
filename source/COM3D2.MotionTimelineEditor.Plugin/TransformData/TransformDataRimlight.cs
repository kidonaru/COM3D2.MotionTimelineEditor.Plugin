using System.Collections.Generic;
using UnityEngine;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    public class TransformDataRimlight : TransformDataBase
    {
        public enum Index
        {
            EulerX = 0,
            EulerY = 1,
            EulerZ = 2,
            ColorR = 3,
            ColorG = 4,
            ColorB = 5,
            ColorA = 6,
            SubColorR = 7,
            SubColorG = 8,
            SubColorB = 9,
            SubColorA = 10,
            Visible = 11,
            Easing = 12,
            LightArea = 13,
            FadeRange = 14,
            FadeExp = 15,
            DepthMin = 16,
            DepthMax = 17,
            DepthFade = 18,
            UseNormal = 19,
            UseAdd = 20,
            UseMultiply = 21,
            UseOverlay = 22,
            UseSubstruct = 23,
            IsWorldSpace = 24,
            EdgeDepth = 25,
            EdgeRange = 26,
            HeightMin = 27
        }

        public static TransformDataRimlight defaultTrans = new TransformDataRimlight();

        public override TransformType type => TransformType.Rimlight;

        public override int valueCount => 28;

        public override bool hasEulerAngles => true;
        public override bool hasColor => true;
        public override bool hasSubColor => true;
        public override bool hasVisible => true;
        public override bool hasEasing => true;

        public override ValueData[] eulerAnglesValues
        {
            get => new ValueData[] { 
                values[(int)Index.EulerX], 
                values[(int)Index.EulerY], 
                values[(int)Index.EulerZ] 
            };
        }
        public override ValueData[] colorValues
        {
            get => new ValueData[] { 
                values[(int)Index.ColorR], 
                values[(int)Index.ColorG], 
                values[(int)Index.ColorB], 
                values[(int)Index.ColorA] 
            };
        }
        public override ValueData[] subColorValues
        {
            get => new ValueData[] { 
                values[(int)Index.SubColorR], 
                values[(int)Index.SubColorG], 
                values[(int)Index.SubColorB], 
                values[(int)Index.SubColorA] 
            };
        }
        public override ValueData visibleValue => values[(int)Index.Visible];
        public override ValueData easingValue => values[(int)Index.Easing];

        public override Color initialColor => new Color(1f, 1f, 1f, 1f);
        public override Color initialSubColor => new Color(1f, 1f, 1f, 0f);

        public TransformDataRimlight()
        {
        }

        public override void Initialize(string name)
        {
            base.Initialize(name);
            index = PostEffectUtils.GetEffectIndex(name);
        }

        private readonly static Dictionary<string, CustomValueInfo> CustomValueInfoMap = new Dictionary<string, CustomValueInfo>
        {
            {
                "lightArea", new CustomValueInfo
                {
                    index = (int)Index.LightArea,
                    name = "影響",
                    min = 0f,
                    max = 2f,
                    step = 0.01f,
                    defaultValue = 1f,
                }
            },
            {
                "fadeRange", new CustomValueInfo
                {
                    index = (int)Index.FadeRange,
                    name = "幅",
                    min = 0f,
                    max = 2f,
                    step = 0.01f,
                    defaultValue = 0.2f,
                }
            },
            {
                "fadeExp", new CustomValueInfo
                {
                    index = (int)Index.FadeExp,
                    name = "指数",
                    min = 0f,
                    max = 5f,
                    step = 0.01f,
                    defaultValue = 1f,
                }
            },
            {
                "depthMin", new CustomValueInfo
                {
                    index = (int)Index.DepthMin,
                    name = "最小深度",
                    min = 0f,
                    max = 100f,
                    step = 0.1f,
                    defaultValue = 0f,
                }
            },
            {
                "depthMax", new CustomValueInfo
                {
                    index = (int)Index.DepthMax,
                    name = "最大深度",
                    min = 0f,
                    max = 100f,
                    step = 0.1f,
                    defaultValue = 5f,
                }
            },
            {
                "depthFade", new CustomValueInfo
                {
                    index = (int)Index.DepthFade,
                    name = "深度幅",
                    min = 0f,
                    max = 10f,
                    step = 0.01f,
                    defaultValue = 1f,
                }
            },
            {
                "useNormal", new CustomValueInfo
                {
                    index = (int)Index.UseNormal,
                    name = "通常",
                    min = 0f,
                    max = 2f,
                    step = 0.01f,
                    defaultValue = 0f,
                }
            },
            {
                "useAdd", new CustomValueInfo
                {
                    index = (int)Index.UseAdd,
                    name = "加算",
                    min = 0f,
                    max = 2f,
                    step = 0.01f,
                    defaultValue = 0.8f,
                }
            },
            {
                "useMultiply", new CustomValueInfo
                {
                    index = (int)Index.UseMultiply,
                    name = "乗算",
                    min = 0f,
                    max = 2f,
                    step = 0.01f,
                    defaultValue = 0f,
                }
            },
            {
                "useOverlay", new CustomValueInfo
                {
                    index = (int)Index.UseOverlay,
                    name = "Overlay",
                    min = 0f,
                    max = 2f,
                    step = 0.01f,
                    defaultValue = 0f,
                }
            },
            {
                "useSubstruct", new CustomValueInfo
                {
                    index = (int)Index.UseSubstruct,
                    name = "減算",
                    min = 0f,
                    max = 2f,
                    step = 0.01f,
                    defaultValue = 0f,
                }
            },
            {
                "isWorldSpace", new CustomValueInfo
                {
                    index = (int)Index.IsWorldSpace,
                    name = "ワールド空間",
                    min = 0f,
                    max = 1f,
                    step = 1f,
                    defaultValue = 0f,
                }
            },
            {
                "edgeDepth", new CustomValueInfo
                {
                    index = (int)Index.EdgeDepth,
                    name = "Edge深度",
                    min = 0f,
                    max = 10f,
                    step = 0.01f,
                    defaultValue = 0f,
                }
            },
            {
                "edgeRange", new CustomValueInfo
                {
                    index = (int)Index.EdgeRange,
                    name = "Edge幅",
                    min = 0f,
                    max = 10f,
                    step = 0.01f,
                    defaultValue = 0f,
                }
            },
            {
                "heightMin", new CustomValueInfo
                {
                    index = (int)Index.HeightMin,
                    name = "最小高さ",
                    min = -10f,
                    max = 10f,
                    step = 0.01f,
                    defaultValue = 0.01f,
                }
            }
        };

        public override Dictionary<string, CustomValueInfo> GetCustomValueInfoMap()
        {
            return CustomValueInfoMap;
        }

        // 値アクセサ
        public ValueData lightAreaValue => values[(int)Index.LightArea];
        public ValueData fadeRangeValue => values[(int)Index.FadeRange];
        public ValueData fadeExpValue => values[(int)Index.FadeExp];
        public ValueData depthMinValue => values[(int)Index.DepthMin];
        public ValueData depthMaxValue => values[(int)Index.DepthMax];
        public ValueData depthFadeValue => values[(int)Index.DepthFade];
        public ValueData useNormalValue => values[(int)Index.UseNormal];
        public ValueData useAddValue => values[(int)Index.UseAdd];
        public ValueData useMultiplyValue => values[(int)Index.UseMultiply];
        public ValueData useOverlayValue => values[(int)Index.UseOverlay];
        public ValueData useSubstructValue => values[(int)Index.UseSubstruct];
        public ValueData isWorldSpaceValue => values[(int)Index.IsWorldSpace];
        public ValueData edgeDepthValue => values[(int)Index.EdgeDepth];
        public ValueData edgeRangeValue => values[(int)Index.EdgeRange];
        public ValueData heightMinValue => values[(int)Index.HeightMin];

        // CustomValueInfoアクセサ
        public CustomValueInfo lightAreaInfo => GetCustomValueInfo("lightArea");
        public CustomValueInfo fadeRangeInfo => GetCustomValueInfo("fadeRange");
        public CustomValueInfo fadeExpInfo => GetCustomValueInfo("fadeExp");
        public CustomValueInfo depthMinInfo => GetCustomValueInfo("depthMin");
        public CustomValueInfo depthMaxInfo => GetCustomValueInfo("depthMax");
        public CustomValueInfo depthFadeInfo => GetCustomValueInfo("depthFade");
        public CustomValueInfo useNormalInfo => GetCustomValueInfo("useNormal");
        public CustomValueInfo useAddInfo => GetCustomValueInfo("useAdd");
        public CustomValueInfo useMultiplyInfo => GetCustomValueInfo("useMultiply");
        public CustomValueInfo useOverlayInfo => GetCustomValueInfo("useOverlay");
        public CustomValueInfo useSubstructInfo => GetCustomValueInfo("useSubstruct");
        public CustomValueInfo isWorldSpaceInfo => GetCustomValueInfo("isWorldSpace");
        public CustomValueInfo edgeDepthInfo => GetCustomValueInfo("edgeDepth");
        public CustomValueInfo edgeRangeInfo => GetCustomValueInfo("edgeRange");
        public CustomValueInfo heightMinInfo => GetCustomValueInfo("heightMin");

        // プロパティアクセサ
        public float lightArea
        {
            get => lightAreaValue.value;
            set => lightAreaValue.value = value;
        }

        public float fadeRange
        {
            get => fadeRangeValue.value;
            set => fadeRangeValue.value = value;
        }

        public float fadeExp
        {
            get => fadeExpValue.value;
            set => fadeExpValue.value = value;
        }

        public float depthMin
        {
            get => depthMinValue.value;
            set => depthMinValue.value = value;
        }

        public float depthMax
        {
            get => depthMaxValue.value;
            set => depthMaxValue.value = value;
        }

        public float depthFade
        {
            get => depthFadeValue.value;
            set => depthFadeValue.value = value;
        }

        public float useNormal
        {
            get => useNormalValue.value;
            set => useNormalValue.value = value;
        }

        public float useAdd
        {
            get => useAddValue.value;
            set => useAddValue.value = value;
        }

        public float useMultiply
        {
            get => useMultiplyValue.value;
            set => useMultiplyValue.value = value;
        }

        public float useOverlay
        {
            get => useOverlayValue.value;
            set => useOverlayValue.value = value;
        }

        public float useSubstruct
        {
            get => useSubstructValue.value;
            set => useSubstructValue.value = value;
        }

        public bool isWorldSpace
        {
            get => isWorldSpaceValue.boolValue;
            set => isWorldSpaceValue.boolValue = value;
        }

        public float edgeDepth
        {
            get => edgeDepthValue.value;
            set => edgeDepthValue.value = value;
        }

        public float edgeRange
        {
            get => edgeRangeValue.value;
            set => edgeRangeValue.value = value;
        }

        public float heightMin
        {
            get => heightMinValue.value;
            set => heightMinValue.value = value;
        }

        public RimlightData rimlight
        {
            get => new RimlightData
            {
                enabled = visible,
                color1 = color,
                color2 = subColor,
                rotation = eulerAngles,
                lightArea = lightArea,
                fadeRange = fadeRange,
                fadeExp = fadeExp,
                depthMin = depthMin,
                depthMax = depthMax,
                depthFade = depthFade,
                useNormal = useNormal,
                useAdd = useAdd,
                useMultiply = useMultiply,
                useOverlay = useOverlay,
                useSubstruct = useSubstruct,
                isWorldSpace = isWorldSpace,
                edgeDepth = edgeDepth,
                edgeRange = edgeRange,
                heightMin = heightMin,
            };
            set
            {
                visible = value.enabled;
                color = value.color1;
                subColor = value.color2;
                eulerAngles = value.rotation;
                lightArea = value.lightArea;
                fadeRange = value.fadeRange;
                fadeExp = value.fadeExp;
                depthMin = value.depthMin;
                depthMax = value.depthMax;
                depthFade = value.depthFade;
                useNormal = value.useNormal;
                useAdd = value.useAdd;
                useMultiply = value.useMultiply;
                useOverlay = value.useOverlay;
                useSubstruct = value.useSubstruct;
                isWorldSpace = value.isWorldSpace;
                edgeDepth = value.edgeDepth;
                edgeRange = value.edgeRange;
                heightMin = value.heightMin;
            }
        }

        public int index;
    }
}