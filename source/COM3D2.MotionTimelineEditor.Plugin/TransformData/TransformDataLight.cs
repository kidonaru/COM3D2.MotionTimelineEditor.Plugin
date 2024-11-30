using System.Collections.Generic;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    public class TransformDataLight : TransformDataBase
    {
        public override TransformType type => TransformType.Light;

        public override int valueCount => 16;

        public override bool hasPosition => true;

        public override bool hasEulerAngles =>  true;

        public override bool hasColor =>  true;

        public override bool hasEasing => true;

        public override ValueData[] positionValues
        {
            get => new ValueData[] { values[0], values[1], values[2] };
        }

        public override ValueData[] eulerAnglesValues
        {
            get => new ValueData[] { values[3], values[4], values[5] };
        }

        public override ValueData[] colorValues
        {
            get => new ValueData[] { values[6], values[7], values[8] };
        }

        public override ValueData easingValue => values[9];

        public TransformDataLight()
        {
        }

        private readonly static Dictionary<string, CustomValueInfo> CustomValueInfoMap = new Dictionary<string, CustomValueInfo>
        {
            {
                "range", new CustomValueInfo
                {
                    index = 10,
                    name = "範囲",
                    defaultValue = 3f,
                }
            },
            {
                "intensity", new CustomValueInfo
                {
                    index = 11,
                    name = "強度",
                    defaultValue = 0.95f,
                }
            },
            {
                "spotAngle", new CustomValueInfo
                {
                    index = 12,
                    name = "角度",
                    defaultValue = 50f,
                }
            },
            {
                "shadowStrength", new CustomValueInfo
                {
                    index = 13,
                    name = "影濃",
                    defaultValue = 0.1f,
                }
            },
            {
                "shadowBias", new CustomValueInfo
                {
                    index = 14,
                    name = "影距",
                    defaultValue = 0.01f,
                }
            },
            {
                "maidSlotNo", new CustomValueInfo
                {
                    index = 15,
                    name = "追従",
                    defaultValue = -1f,
                }
            },
        };

        public override Dictionary<string, CustomValueInfo> GetCustomValueInfoMap()
        {
            return CustomValueInfoMap;
        }

        public ValueData rangeValue => this["range"];

        public ValueData intensityValue => this["intensity"];

        public ValueData spotAngleValue => this["spotAngle"];

        public ValueData shadowStrengthValue => this["shadowStrength"];

        public ValueData shadowBiasValue => this["shadowBias"];

        public ValueData maidSlotNoValue => this["maidSlotNo"];

        public float range
        {
            get => rangeValue.value;
            set => rangeValue.value = value;
        }

        public float intensity
        {
            get => intensityValue.value;
            set => intensityValue.value = value;
        }

        public float spotAngle
        {
            get => spotAngleValue.value;
            set => spotAngleValue.value = value;
        }

        public float shadowStrength
        {
            get => shadowStrengthValue.value;
            set => shadowStrengthValue.value = value;
        }

        public float shadowBias
        {
            get => shadowBiasValue.value;
            set => shadowBiasValue.value = value;
        }

        public int maidSlotNo
        {
            get => maidSlotNoValue.intValue;
            set => maidSlotNoValue.intValue = value;
        }
    }
}