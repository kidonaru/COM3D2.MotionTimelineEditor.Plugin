using System.Collections.Generic;
using UnityEngine;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    public class TransformDataPsylliumAnimation : TransformDataBase
    {
        public static TransformDataPsylliumAnimation defaultTrans = new TransformDataPsylliumAnimation();
        public static PsylliumAnimationConfig defaultConfig = new PsylliumAnimationConfig();

        public override TransformType type => TransformType.PsylliumAnimation;

        public override int valueCount => 20;

        public override bool hasPosition => true;
        public override bool hasSubPosition => true;
        public override bool hasEulerAngles => true;

        public override ValueData[] positionValues
        {
            get => new ValueData[] { values[0], values[1], values[2] };
        }

        public override ValueData[] subPositionValues
        {
            get => new ValueData[] { values[3], values[4], values[5] };
        }

        public override ValueData[] eulerAnglesValues
        {
            get => new ValueData[] { values[6], values[7], values[8] };
        }

        public override Vector3 initialPosition => defaultConfig.randomPosition1Range;
        public override Vector3 initialSubPosition => defaultConfig.randomPosition2Range;
        public override Vector3 initialEulerAngles => defaultConfig.randomEulerAnglesRange;

        public TransformDataPsylliumAnimation()
        {
        }

        private readonly static Dictionary<string, CustomValueInfo> CustomValueInfoMap = new Dictionary<string, CustomValueInfo>
        {
            {
                "positionSyncRate", new CustomValueInfo
                {
                    index = 9,
                    name = "位置同期",
                    min = 0f,
                    max = 1f,
                    step = 0.01f,
                    defaultValue = defaultConfig.positionSyncRate,
                }
            },
            {
                "bpm", new CustomValueInfo
                {
                    index = 10,
                    name = "BPM",
                    min = 1f,
                    max = 300f,
                    step = 0.1f,
                    defaultValue = defaultConfig.bpm,
                }
            },
            {
                "patternCount", new CustomValueInfo
                {
                    index = 11,
                    name = "T Count",
                    min = 1f,
                    max = 100f,
                    step = 1f,
                    defaultValue = defaultConfig.patternCount,
                }
            },
            {
                "randomTime", new CustomValueInfo
                {
                    index = 12,
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
                    index = 13,
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
                    index = 14,
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
                    index = 15,
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
                    index = 16,
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
                    index = 17,
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
                    index = 18,
                    name = "Easing2",
                    min = 0f,
                    max = (int) MoveEasingType.Max - 1,
                    step = 1f,
                    defaultValue = (int) defaultConfig.easingType2,
                }
            },
            {
                "randomSeed", new CustomValueInfo
                {
                    index = 19,
                    name = "乱数Seed",
                    min = 0f,
                    max = int.MaxValue,
                    step = 1f,
                    defaultValue = defaultConfig.randomSeed,
                }
            },
        };

        public override Dictionary<string, CustomValueInfo> GetCustomValueInfoMap()
        {
            return CustomValueInfoMap;
        }

        public ValueData positionSyncRateValue => this["positionSyncRate"];
        public ValueData bpmValue => this["bpm"];
        public ValueData patternCountValue => this["patternCount"];
        public ValueData randomTimeValue => this["randomTime"];
        public ValueData timeRatioValue => this["timeRatio"];
        public ValueData timeOffsetValue => this["timeOffset"];
        public ValueData timeShiftMinValue => this["timeShiftMin"];
        public ValueData timeShiftMaxValue => this["timeShiftMax"];
        public ValueData easingType1Value => this["easingType1"];
        public ValueData easingType2Value => this["easingType2"];
        public ValueData randomSeedValue => this["randomSeed"];

        public CustomValueInfo positionSyncRateInfo => CustomValueInfoMap["positionSyncRate"];
        public CustomValueInfo bpmInfo => CustomValueInfoMap["bpm"];
        public CustomValueInfo patternCountInfo => CustomValueInfoMap["patternCount"];
        public CustomValueInfo randomTimeInfo => CustomValueInfoMap["randomTime"];
        public CustomValueInfo timeRatioInfo => CustomValueInfoMap["timeRatio"];
        public CustomValueInfo timeOffsetInfo => CustomValueInfoMap["timeOffset"];
        public CustomValueInfo timeShiftMinInfo => CustomValueInfoMap["timeShiftMin"];
        public CustomValueInfo timeShiftMaxInfo => CustomValueInfoMap["timeShiftMax"];
        public CustomValueInfo easingType1Info => CustomValueInfoMap["easingType1"];
        public CustomValueInfo easingType2Info => CustomValueInfoMap["easingType2"];
        public CustomValueInfo randomSeedInfo => CustomValueInfoMap["randomSeed"];

        public float positionSyncRate
        {
            get => positionSyncRateValue.value;
            set => positionSyncRateValue.value = value;
        }
        public float bpm
        {
            get => bpmValue.value;
            set => bpmValue.value = value;
        }
        public int patternCount
        {
            get => patternCountValue.intValue;
            set => patternCountValue.intValue = value;
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
        public int randomSeed
        {
            get => randomSeedValue.intValue;
            set => randomSeedValue.intValue = value;
        }

        public void FromConfig(PsylliumAnimationConfig config)
        {
            position = config.randomPosition1Range;
            subPosition = config.randomPosition2Range;
            eulerAngles = config.randomEulerAnglesRange;
            positionSyncRate = config.positionSyncRate;
            bpm = config.bpm;
            patternCount = config.patternCount;
            randomTime = config.randomTime;
            timeRatio = config.timeRatio;
            timeOffset = config.timeOffset;
            timeShiftMin = config.timeShiftMin;
            timeShiftMax = config.timeShiftMax;
            easingType1 = config.easingType1;
            easingType2 = config.easingType2;
            randomSeed = config.randomSeed;
        }

        private PsylliumAnimationConfig _config = new PsylliumAnimationConfig();

        public PsylliumAnimationConfig ToConfig()
        {
            _config.randomPosition1Range = position;
            _config.randomPosition2Range = subPosition;
            _config.randomEulerAnglesRange = eulerAngles;
            _config.positionSyncRate = positionSyncRate;
            _config.bpm = bpm;
            _config.patternCount = patternCount;
            _config.randomTime = randomTime;
            _config.timeRatio = timeRatio;
            _config.timeOffset = timeOffset;
            _config.timeShiftMin = timeShiftMin;
            _config.timeShiftMax = timeShiftMax;
            _config.easingType1 = easingType1;
            _config.easingType2 = easingType2;
            _config.randomSeed = randomSeed;
            return _config;
        }
    }
}