using System.Collections.Generic;
using UnityEngine;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    public class TransformDataDistanceFog : TransformDataBase
    {
        public override TransformType type => TransformType.DistanceFog;

        public override int valueCount => 13;

        public override bool hasColor =>  true;

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
                    defaultValue = 0f,
                }
            },
            {
                "fogEnd", new CustomValueInfo
                {
                    index = 11,
                    name = "終了深度",
                    defaultValue = 50f,
                }
            },
            {
                "fogExp", new CustomValueInfo
                {
                    index = 12,
                    name = "指数",
                    defaultValue = 1f,
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
            };
            set
            {
                visible = value.enabled;
                color = value.color1;
                subColor = value.color2;
                fogStart = value.fogStart;
                fogEnd = value.fogEnd;
                fogExp = value.fogExp;
            }
        }

        public int index;
    }
}