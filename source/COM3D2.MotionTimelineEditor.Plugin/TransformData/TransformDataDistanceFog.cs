using System.Collections.Generic;
using UnityEngine;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    public class TransformDataDistanceFog : TransformDataBase
    {
        public enum Index
        {
            ColorR = 0,
            ColorG = 1,
            ColorB = 2,
            ColorA = 3,
            SubColorR = 4,
            SubColorG = 5,
            SubColorB = 6,
            SubColorA = 7,
            Visible = 8,
            Easing = 9,
            FogStart = 10,
            FogEnd = 11,
            FogExp = 12,
            UseNormal = 13,
            UseAdd = 14,
            UseMultiply = 15,
            UseOverlay = 16,
            UseSubstruct = 17
        }

        public static TransformDataDistanceFog defaultTrans = new TransformDataDistanceFog();

        public override TransformType type => TransformType.DistanceFog;

        public override int valueCount => 18;

        public override bool hasColor => true;
        public override bool hasSubColor => true;
        public override bool hasVisible => true;
        public override bool hasEasing => true;

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

        public TransformDataDistanceFog()
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
                "fogStart", new CustomValueInfo
                {
                    index = (int)Index.FogStart,
                    name = "開始深度",
                    min = 0f,
                    max = 100f,
                    step = 0.1f,
                    defaultValue = 0f,
                }
            },
            {
                "fogEnd", new CustomValueInfo
                {
                    index = (int)Index.FogEnd,
                    name = "終了深度",
                    min = 0f,
                    max = 100f,
                    step = 0.1f,
                    defaultValue = 50f,
                }
            },
            {
                "fogExp", new CustomValueInfo
                {
                    index = (int)Index.FogExp,
                    name = "指数",
                    min = 0f,
                    max = 5f,
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
                    defaultValue = 1f,
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
                    defaultValue = 0f,
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
        };

        public override Dictionary<string, CustomValueInfo> GetCustomValueInfoMap()
        {
            return CustomValueInfoMap;
        }

        public ValueData fogStartValue => values[(int)Index.FogStart];
        public ValueData fogEndValue => values[(int)Index.FogEnd];
        public ValueData fogExpValue => values[(int)Index.FogExp];
        public ValueData useNormalValue => values[(int)Index.UseNormal];
        public ValueData useAddValue => values[(int)Index.UseAdd];
        public ValueData useMultiplyValue => values[(int)Index.UseMultiply];
        public ValueData useOverlayValue => values[(int)Index.UseOverlay];
        public ValueData useSubstructValue => values[(int)Index.UseSubstruct];

        public CustomValueInfo fogStartInfo => CustomValueInfoMap["fogStart"];
        public CustomValueInfo fogEndInfo => CustomValueInfoMap["fogEnd"];
        public CustomValueInfo fogExpInfo => CustomValueInfoMap["fogExp"];
        public CustomValueInfo useNormalInfo => CustomValueInfoMap["useNormal"];
        public CustomValueInfo useAddInfo => CustomValueInfoMap["useAdd"];
        public CustomValueInfo useMultiplyInfo => CustomValueInfoMap["useMultiply"];
        public CustomValueInfo useOverlayInfo => CustomValueInfoMap["useOverlay"];
        public CustomValueInfo useSubstructInfo => CustomValueInfoMap["useSubstruct"];

        public float fogStart
        {
            get => fogStartValue.value;
            set => fogStartValue.value = value;
        }

        public float fogEnd
        {
            get => fogEndValue.value;
            set => fogEndValue.value = value;
        }

        public float fogExp
        {
            get => fogExpValue.value;
            set => fogExpValue.value = value;
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

        public DistanceFogData distanceFog
        {
            get => new DistanceFogData
            {
                enabled = visible,
                color1 = color,
                color2 = subColor,
                fogStart = fogStart,
                fogEnd = fogEnd,
                fogExp = fogExp,
                useNormal = useNormal,
                useAdd = useAdd,
                useMultiply = useMultiply,
                useOverlay = useOverlay,
                useSubstruct = useSubstruct,
            };
            set
            {
                visible = value.enabled;
                color = value.color1;
                subColor = value.color2;
                fogStart = value.fogStart;
                fogEnd = value.fogEnd;
                fogExp = value.fogExp;
                useNormal = value.useNormal;
                useAdd = value.useAdd;
                useMultiply = value.useMultiply;
                useOverlay = value.useOverlay;
                useSubstruct = value.useSubstruct;
            }
        }

        public int index;
    }
}