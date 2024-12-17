using System.Collections.Generic;
using UnityEngine;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    public class TransformDataDistanceFog : TransformDataBase
    {
        public static TransformDataDistanceFog defaultTrans = new TransformDataDistanceFog();

        public override TransformType type => TransformType.DistanceFog;

        public override int valueCount => 18;

        public override bool hasColor => true;

        public override bool hasSubColor => true;

        public override bool hasVisible => true;

        public override bool hasEasing => true;

        public override ValueData[] colorValues
        {
            get => new ValueData[] { values[0], values[1], values[2], values[3] };
        }

        public override ValueData[] subColorValues
        {
            get => new ValueData[] { values[4], values[5], values[6], values[7] };
        }

        public override ValueData visibleValue => values[8];

        public override ValueData easingValue => values[9];

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
                    index = 10,
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
                    index = 11,
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
                    index = 12,
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
                    index = 13,
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
                    index = 14,
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
                    index = 15,
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
                    index = 16,
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
                    index = 17,
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

        public ValueData fogStartValue => this["fogStart"];
        public ValueData fogEndValue => this["fogEnd"];
        public ValueData fogExpValue => this["fogExp"];
        public ValueData useNormalValue => this["useNormal"];
        public ValueData useAddValue => this["useAdd"];
        public ValueData useMultiplyValue => this["useMultiply"];
        public ValueData useOverlayValue => this["useOverlay"];
        public ValueData useSubstructValue => this["useSubstruct"];

        public CustomValueInfo fogStartInfo => GetCustomValueInfo("fogStart");
        public CustomValueInfo fogEndInfo => GetCustomValueInfo("fogEnd");
        public CustomValueInfo fogExpInfo => GetCustomValueInfo("fogExp");
        public CustomValueInfo useNormalInfo => GetCustomValueInfo("useNormal");
        public CustomValueInfo useAddInfo => GetCustomValueInfo("useAdd");
        public CustomValueInfo useMultiplyInfo => GetCustomValueInfo("useMultiply");
        public CustomValueInfo useOverlayInfo => GetCustomValueInfo("useOverlay");
        public CustomValueInfo useSubstructInfo => GetCustomValueInfo("useSubstruct");

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