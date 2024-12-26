using System.Collections.Generic;
using UnityEngine;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    public class TransformDataPsylliumAnimation : TransformDataBase
    {
        public static TransformDataPsylliumAnimation defaultTrans = new TransformDataPsylliumAnimation();
        public static PsylliumAnimationConfig defaultConfig = new PsylliumAnimationConfig();

        public override TransformType type => TransformType.PsylliumAnimation;

        public override int valueCount => 16;

        public override bool hasPosition => true;
        public override bool hasEulerAngles => true;

        public override ValueData[] positionValues
        {
            get => new ValueData[] { values[0], values[1], values[2] };
        }

        public override ValueData[] eulerAnglesValues
        {
            get => new ValueData[] { values[3], values[4], values[5] };
        }

        public override Vector3 initialPosition => defaultConfig.randomPositionRange;
        public override Vector3 initialEulerAngles => defaultConfig.randomRotationRange;

        public TransformDataPsylliumAnimation()
        {
        }

        private readonly static Dictionary<string, CustomValueInfo> CustomValueInfoMap = new Dictionary<string, CustomValueInfo>
        {
            {
                "mirrorRotationZ", new CustomValueInfo
                {
                    index = 6,
                    name = "ミラーRZ",
                    min = 0f,
                    max = 1f,
                    step = 1f,
                    defaultValue = defaultConfig.mirrorRotationZ ? 1f : 0f,
                }
            },
            {
                "bpm", new CustomValueInfo
                {
                    index = 7,
                    name = "BPM",
                    min = 1f,
                    max = 300f,
                    step = 0.1f,
                    defaultValue = defaultConfig.bpm,
                }
            },
            {
                "randomTimeCount", new CustomValueInfo
                {
                    index = 8,
                    name = "T Count",
                    min = 1f,
                    max = 100f,
                    step = 1f,
                    defaultValue = defaultConfig.randomTimeCount,
                }
            },
            {
                "randomTime", new CustomValueInfo
                {
                    index = 9,
                    name = "T Random",
                    min = 0f,
                    max = 1f,
                    step = 0.01f,
                    defaultValue = defaultConfig.randomTime,
                }
            },
            {
                "timeRatio", new CustomValueInfo
                {
                    index = 10,
                    name = "T Ratio",
                    min = 0f,
                    max = 1f,
                    step = 0.01f,
                    defaultValue = defaultConfig.timeRatio,
                }
            },
            {
                "timeOffset", new CustomValueInfo
                {
                    index = 11,
                    name = "T Offset",
                    min = -5f,
                    max = 5f,
                    step = 0.01f,
                    defaultValue = defaultConfig.timeOffset,
                }
            },
            {
                "timeShiftMin", new CustomValueInfo
                {
                    index = 12,
                    name = "ShiftMin",
                    min = 0f,
                    max = 10f,
                    step = 0.1f,
                    defaultValue = defaultConfig.timeShiftMin,
                }
            },
            {
                "timeShiftMax", new CustomValueInfo
                {
                    index = 13,
                    name = "ShiftMax",
                    min = 0f,
                    max = 10f,
                    step = 0.1f,
                    defaultValue = defaultConfig.timeShiftMax,
                }
            },
            {
                "easingType1", new CustomValueInfo
                {
                    index = 14,
                    name = "Easing1",
                    min = 0f,
                    max = (int) MoveEasingType.Max - 1,
                    step = 1f,
                    defaultValue = (int) defaultConfig.easingType1,
                }
            },
            {
                "easingType2", new CustomValueInfo
                {
                    index = 15,
                    name = "Easing2",
                    min = 0f,
                    max = (int) MoveEasingType.Max - 1,
                    step = 1f,
                    defaultValue = (int) defaultConfig.easingType2,
                }
            },
        };

        public override Dictionary<string, CustomValueInfo> GetCustomValueInfoMap()
        {
            return CustomValueInfoMap;
        }

        public ValueData mirrorRotationZValue => this["mirrorRotationZ"];
        public ValueData bpmValue => this["bpm"];
        public ValueData randomTimeCountValue => this["randomTimeCount"];
        public ValueData randomTimeValue => this["randomTime"];
        public ValueData timeRatioValue => this["timeRatio"];
        public ValueData timeOffsetValue => this["timeOffset"];
        public ValueData timeShiftMinValue => this["timeShiftMin"];
        public ValueData timeShiftMaxValue => this["timeShiftMax"];
        public ValueData easingType1Value => this["easingType1"];
        public ValueData easingType2Value => this["easingType2"];

        public CustomValueInfo mirrorRotationZInfo => CustomValueInfoMap["mirrorRotationZ"];
        public CustomValueInfo bpmInfo => CustomValueInfoMap["bpm"];
        public CustomValueInfo randomTimeCountInfo => CustomValueInfoMap["randomTimeCount"];
        public CustomValueInfo randomTimeInfo => CustomValueInfoMap["randomTime"];
        public CustomValueInfo timeRatioInfo => CustomValueInfoMap["timeRatio"];
        public CustomValueInfo timeOffsetInfo => CustomValueInfoMap["timeOffset"];
        public CustomValueInfo timeShiftMinInfo => CustomValueInfoMap["timeShiftMin"];
        public CustomValueInfo timeShiftMaxInfo => CustomValueInfoMap["timeShiftMax"];
        public CustomValueInfo easingType1Info => CustomValueInfoMap["easingType1"];
        public CustomValueInfo easingType2Info => CustomValueInfoMap["easingType2"];

        public bool mirrorRotationZ
        {
            get => mirrorRotationZValue.boolValue;
            set => mirrorRotationZValue.boolValue = value;
        }
        public float bpm
        {
            get => bpmValue.value;
            set => bpmValue.value = value;
        }
        public int randomTimeCount
        {
            get => randomTimeCountValue.intValue;
            set => randomTimeCountValue.intValue = value;
        }
        public float randomTime
        {
            get => randomTimeValue.value;
            set => randomTimeValue.value = value;
        }
        public float timeRatio
        {
            get => timeRatioValue.value;
            set => timeRatioValue.value = value;
        }
        public float timeOffset
        {
            get => timeOffsetValue.value;
            set => timeOffsetValue.value = value;
        }
        public float timeShiftMin
        {
            get => timeShiftMinValue.value;
            set => timeShiftMinValue.value = value;
        }
        public float timeShiftMax
        {
            get => timeShiftMaxValue.value;
            set => timeShiftMaxValue.value = value;
        }
        public MoveEasingType easingType1
        {
            get => (MoveEasingType) easingType1Value.intValue;
            set => easingType1Value.intValue = (int) value;
        }
        public MoveEasingType easingType2
        {
            get => (MoveEasingType) easingType2Value.intValue;
            set => easingType2Value.intValue = (int) value;
        }

        public void FromConfig(PsylliumAnimationConfig config)
        {
            position = config.randomPositionRange;
            eulerAngles = config.randomRotationRange;
            mirrorRotationZ = config.mirrorRotationZ;
            bpm = config.bpm;
            randomTimeCount = config.randomTimeCount;
            randomTime = config.randomTime;
            timeRatio = config.timeRatio;
            timeOffset = config.timeOffset;
            timeShiftMin = config.timeShiftMin;
            timeShiftMax = config.timeShiftMax;
            easingType1 = config.easingType1;
            easingType2 = config.easingType2;
        }

        private PsylliumAnimationConfig _config = new PsylliumAnimationConfig();

        public PsylliumAnimationConfig ToConfig()
        {
            _config.randomPositionRange = position;
            _config.randomRotationRange = eulerAngles;
            _config.mirrorRotationZ = mirrorRotationZ;
            _config.bpm = bpm;
            _config.randomTimeCount = randomTimeCount;
            _config.randomTime = randomTime;
            _config.timeRatio = timeRatio;
            _config.timeOffset = timeOffset;
            _config.timeShiftMin = timeShiftMin;
            _config.timeShiftMax = timeShiftMax;
            _config.easingType1 = easingType1;
            _config.easingType2 = easingType2;
            return _config;
        }
    }
}