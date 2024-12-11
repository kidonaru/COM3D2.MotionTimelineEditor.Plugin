using System.Collections.Generic;
using UnityEngine;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    public class TransformDataRimlight : TransformDataBase
    {
        public static TransformDataRimlight defaultTrans = new TransformDataRimlight();

        public override TransformType type => TransformType.Rimlight;

        public override int valueCount => 27;

        public override bool hasEulerAngles => true;
        public override bool hasColor =>  true;
        public override bool hasSubColor => true;
        public override bool hasVisible => true;
        public override bool hasEasing => true;

        public override ValueData[] eulerAnglesValues
        {
            get => new ValueData[] { values[0], values[1], values[2] };
        }
        public override ValueData[] colorValues
        {
            get => new ValueData[] { values[3], values[4], values[5], values[6] };
        }
        public override ValueData[] subColorValues
        {
            get => new ValueData[] { values[7], values[8], values[9], values[10] };
        }
        public override ValueData visibleValue => values[11];
        public override ValueData easingValue => values[12];

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
                    index = 13,
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
                    index = 14,
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
                    index = 15,
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
                    index = 16,
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
                    index = 17,
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
                    index = 18,
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
                    index = 19,
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
                    index = 20,
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
                    index = 21,
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
                    index = 22,
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
                    index = 23,
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
                    index = 23,
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
                    index = 24,
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
                    index = 25,
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
                    index = 26,
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

        public ValueData lightAreaValue => this["lightArea"];
        public ValueData fadeRangeValue => this["fadeRange"];
        public ValueData fadeExpValue => this["fadeExp"];
        public ValueData depthMinValue => this["depthMin"];
        public ValueData depthMaxValue => this["depthMax"];
        public ValueData depthFadeValue => this["depthFade"];
        public ValueData useNormalValue => this["useNormal"];
        public ValueData useAddValue => this["useAdd"];
        public ValueData useMultiplyValue => this["useMultiply"];
        public ValueData useOverlayValue => this["useOverlay"];
        public ValueData useSubstructValue => this["useSubstruct"];
        public ValueData isWorldSpaceValue => this["isWorldSpace"];
        public ValueData edgeDepthValue => this["edgeDepth"];
        public ValueData edgeRangeValue => this["edgeRange"];
        public ValueData heightMinValue => this["heightMin"];

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