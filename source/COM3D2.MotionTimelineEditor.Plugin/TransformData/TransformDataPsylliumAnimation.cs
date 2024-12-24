using System.Collections.Generic;
using UnityEngine;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    public class TransformDataPsylliumAnimation : TransformDataBase
    {
        public static TransformDataPsylliumAnimation defaultTrans = new TransformDataPsylliumAnimation();
        public static PsylliumAnimationConfig defaultConfig = new PsylliumAnimationConfig();

        public override TransformType type => TransformType.PsylliumAnimation;

        public override int valueCount => 26;

        public override bool hasPosition => true;
        public override bool hasSubPosition => true;
        public override bool hasEulerAngles => true;
        public override bool hasSubEulerAngles => true;

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
        public override ValueData[] subEulerAnglesValues
        {
            get => new ValueData[] { values[9], values[10], values[11] };
        }

        public override Vector3 initialPosition => defaultConfig.position1;
        public override Vector3 initialSubPosition => defaultConfig.position2;
        public override Vector3 initialEulerAngles => defaultConfig.rotation1;
        public override Vector3 initialSubEulerAngles => defaultConfig.rotation2;

        public override bool isFixRotation => false;

        public TransformDataPsylliumAnimation()
        {
        }

        private readonly static Dictionary<string, CustomValueInfo> CustomValueInfoMap = new Dictionary<string, CustomValueInfo>
        {
            {
                "randomPositionRangeX", new CustomValueInfo
                {
                    index = 12,
                    name = "X Random",
                    min = 0f,
                    max = 10f,
                    step = 0.01f,
                    defaultValue = defaultConfig.randomPositionRange.x,
                }
            },
            {
                "randomPositionRangeY", new CustomValueInfo
                {
                    index = 13,
                    name = "Y Random",
                    min = 0f,
                    max = 10f,
                    step = 0.01f,
                    defaultValue = defaultConfig.randomPositionRange.y,
                }
            },
            {
                "randomPositionRangeZ", new CustomValueInfo
                {
                    index = 14,
                    name = "Z Random",
                    min = 0f,
                    max = 10f,
                    step = 0.01f,
                    defaultValue = defaultConfig.randomPositionRange.z,
                }
            },
            {
                "randomRotationRangeX", new CustomValueInfo
                {
                    index = 15,
                    name = "RX Random",
                    min = 0f,
                    max = 180f,
                    step = 0.1f,
                    defaultValue = defaultConfig.randomRotationRange.x,
                }
            },
            {
                "randomRotationRangeY", new CustomValueInfo
                {
                    index = 16,
                    name = "RY Random",
                    min = 0f,
                    max = 180f,
                    step = 0.1f,
                    defaultValue = defaultConfig.randomRotationRange.y,
                }
            },
            {
                "randomRotationRangeZ", new CustomValueInfo
                {
                    index = 17,
                    name = "RZ Random",
                    min = 0f,
                    max = 180f,
                    step = 0.1f,
                    defaultValue = defaultConfig.randomRotationRange.z,
                }
            },
            {
                "mirrorRotationZ", new CustomValueInfo
                {
                    index = 18,
                    name = "ミラーRZ",
                    min = 0f,
                    max = 1f,
                    step = 1f,
                    defaultValue = defaultConfig.mirrorRotationZ ? 1f : 0f,
                }
            },
            {
                "cutoffHeight", new CustomValueInfo
                {
                    index = 19,
                    name = "制限高さ",
                    min = -10f,
                    max = 10f,
                    step = 0.01f,
                    defaultValue = defaultConfig.cutoffHeight,
                }
            },
            {
                "bpm", new CustomValueInfo
                {
                    index = 20,
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
                    index = 21,
                    name = "乱数数",
                    min = 1f,
                    max = 100f,
                    step = 1f,
                    defaultValue = defaultConfig.randomTimeCount,
                }
            },
            {
                "randomTime", new CustomValueInfo
                {
                    index = 22,
                    name = "時間乱数",
                    min = 0f,
                    max = 1f,
                    step = 0.01f,
                    defaultValue = defaultConfig.randomTime,
                }
            },
            {
                "timeRatio", new CustomValueInfo
                {
                    index = 23,
                    name = "時間比率",
                    min = 0f,
                    max = 1f,
                    step = 0.01f,
                    defaultValue = defaultConfig.timeRatio,
                }
            },
            {
                "easingType1", new CustomValueInfo
                {
                    index = 24,
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
                    index = 25,
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

        public ValueData[] randomPositionRangeValues
        {
            get => new ValueData[] { this["randomPositionRangeX"], this["randomPositionRangeY"], this["randomPositionRangeZ"] };
        }
        public ValueData[] randomRotationRangeValues
        {
            get => new ValueData[] { this["randomRotationRangeX"], this["randomRotationRangeY"], this["randomRotationRangeZ"] };
        }
        public ValueData mirrorRotationZValue => this["mirrorRotationZ"];
        public ValueData cutoffHeightValue => this["cutoffHeight"];
        public ValueData bpmValue => this["bpm"];
        public ValueData randomTimeCountValue => this["randomTimeCount"];
        public ValueData randomTimeValue => this["randomTime"];
        public ValueData timeRatioValue => this["timeRatio"];
        public ValueData easingType1Value => this["easingType1"];
        public ValueData easingType2Value => this["easingType2"];

        public CustomValueInfo mirrorRotationZInfo => CustomValueInfoMap["mirrorRotationZ"];
        public CustomValueInfo cutoffHeightInfo => CustomValueInfoMap["cutoffHeight"];
        public CustomValueInfo bpmInfo => CustomValueInfoMap["bpm"];
        public CustomValueInfo randomTimeCountInfo => CustomValueInfoMap["randomTimeCount"];
        public CustomValueInfo randomTimeInfo => CustomValueInfoMap["randomTime"];
        public CustomValueInfo timeRatioInfo => CustomValueInfoMap["timeRatio"];
        public CustomValueInfo easingType1Info => CustomValueInfoMap["easingType1"];
        public CustomValueInfo easingType2Info => CustomValueInfoMap["easingType2"];

        public Vector3 randomPositionRange
        {
            get => randomPositionRangeValues.ToVector3();
            set => randomPositionRangeValues.FromVector3(value);
        }
        public Vector3 randomRotationRange
        {
            get => randomRotationRangeValues.ToVector3();
            set => randomRotationRangeValues.FromVector3(value);
        }
        public bool mirrorRotationZ
        {
            get => mirrorRotationZValue.boolValue;
            set => mirrorRotationZValue.boolValue = value;
        }
        public float cutoffHeight
        {
            get => cutoffHeightValue.value;
            set => cutoffHeightValue.value = value;
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
            position = config.position1;
            subPosition = config.position2;
            eulerAngles = config.rotation1;
            subEulerAngles = config.rotation2;
            randomPositionRange = config.randomPositionRange;
            randomRotationRange = config.randomRotationRange;
            mirrorRotationZ = config.mirrorRotationZ;
            cutoffHeight = config.cutoffHeight;
            bpm = config.bpm;
            randomTimeCount = config.randomTimeCount;
            randomTime = config.randomTime;
            timeRatio = config.timeRatio;
            easingType1 = config.easingType1;
            easingType2 = config.easingType2;
        }

        private PsylliumAnimationConfig _config = new PsylliumAnimationConfig();

        public PsylliumAnimationConfig ToConfig()
        {
            _config.position1 = position;
            _config.position2 = subPosition;
            _config.rotation1 = eulerAngles;
            _config.rotation2 = subEulerAngles;
            _config.randomPositionRange = randomPositionRange;
            _config.randomRotationRange = randomRotationRange;
            _config.mirrorRotationZ = mirrorRotationZ;
            _config.cutoffHeight = cutoffHeight;
            _config.bpm = bpm;
            _config.randomTimeCount = randomTimeCount;
            _config.randomTime = randomTime;
            _config.timeRatio = timeRatio;
            _config.easingType1 = easingType1;
            _config.easingType2 = easingType2;
            return _config;
        }
    }
}