using System.Collections.Generic;
using UnityEngine;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    public class TransformDataPsylliumAnimation : TransformDataBase
    {
        public static TransformDataPsylliumAnimation defaultTrans = new TransformDataPsylliumAnimation();
        public static PsylliumAnimationConfig defaultConfig = new PsylliumAnimationConfig();

        public override TransformType type => TransformType.PsylliumAnimation;

        public override int valueCount => 38;

        public TransformDataPsylliumAnimation()
        {
        }

        private readonly static Dictionary<string, CustomValueInfo> CustomValueInfoMap = new Dictionary<string, CustomValueInfo>
        {
            {
                "positionLeft1X", new CustomValueInfo
                {
                    index = 0,
                    name = "X1 Left",
                    min = 0f,
                    max = 5f,
                    step = 0.01f,
                    defaultValue = defaultConfig.positionLeft1.x,
                }
            },
            {
                "positionLeft1Y", new CustomValueInfo
                {
                    index = 1,
                    name = "Y1 Left",
                    min = 0f,
                    max = 5f,
                    step = 0.01f,
                    defaultValue = defaultConfig.positionLeft1.y,
                }
            },
            {
                "positionLeft1Z", new CustomValueInfo
                {
                    index = 2,
                    name = "Z1 Left",
                    min = 0f,
                    max = 5f,
                    step = 0.01f,
                    defaultValue = defaultConfig.positionLeft1.z,
                }
            },
            {
                "positionLeft2X", new CustomValueInfo
                {
                    index = 3,
                    name = "X2 Left",
                    min = 0f,
                    max = 5f,
                    step = 0.01f,
                    defaultValue = defaultConfig.positionLeft2.x,
                }
            },
            {
                "positionLeft2Y", new CustomValueInfo
                {
                    index = 4,
                    name = "Y2 Left",
                    min = 0f,
                    max = 5f,
                    step = 0.01f,
                    defaultValue = defaultConfig.positionLeft2.y,
                }
            },
            {
                "positionLeft2Z", new CustomValueInfo
                {
                    index = 5,
                    name = "Z2 Left",
                    min = 0f,
                    max = 5f,
                    step = 0.01f,
                    defaultValue = defaultConfig.positionLeft2.z,
                }
            },
            {
                "positionRight1X", new CustomValueInfo
                {
                    index = 6,
                    name = "X1 Right",
                    min = 0f,
                    max = 5f,
                    step = 0.01f,
                    defaultValue = defaultConfig.positionRight1.x,
                }
            },
            {
                "positionRight1Y", new CustomValueInfo
                {
                    index = 7,
                    name = "Y1 Right",
                    min = 0f,
                    max = 5f,
                    step = 0.01f,
                    defaultValue = defaultConfig.positionRight1.y,
                }
            },
            {
                "positionRight1Z", new CustomValueInfo
                {
                    index = 8,
                    name = "Z1 Right",
                    min = 0f,
                    max = 5f,
                    step = 0.01f,
                    defaultValue = defaultConfig.positionRight1.z,
                }
            },
            {
                "positionRight2X", new CustomValueInfo
                {
                    index = 9,
                    name = "X2 Right",
                    min = 0f,
                    max = 5f,
                    step = 0.01f,
                    defaultValue = defaultConfig.positionRight2.x,
                }
            },
            {
                "positionRight2Y", new CustomValueInfo
                {
                    index = 10,
                    name = "Y2 Right",
                    min = 0f,
                    max = 5f,
                    step = 0.01f,
                    defaultValue = defaultConfig.positionRight2.y,
                }
            },
            {
                "positionRight2Z", new CustomValueInfo
                {
                    index = 11,
                    name = "Z2 Right",
                    min = 0f,
                    max = 5f,
                    step = 0.01f,
                    defaultValue = defaultConfig.positionRight2.z,
                }
            },
            {
                "rotationLeft1X", new CustomValueInfo
                {
                    index = 12,
                    name = "RX1 Left",
                    min = -180f,
                    max = 180f,
                    step = 0.1f,
                    defaultValue = defaultConfig.rotationLeft1.x,
                }
            },
            {
                "rotationLeft1Y", new CustomValueInfo
                {
                    index = 13,
                    name = "RY1 Left",
                    min = -180f,
                    max = 180f,
                    step = 0.1f,
                    defaultValue = defaultConfig.rotationLeft1.y,
                }
            },
            {
                "rotationLeft1Z", new CustomValueInfo
                {
                    index = 14,
                    name = "RZ1 Left",
                    min = -180f,
                    max = 180f,
                    step = 0.1f,
                    defaultValue = defaultConfig.rotationLeft1.z,
                }
            },
            {
                "rotationLeft2X", new CustomValueInfo
                {
                    index = 15,
                    name = "RX2 Left",
                    min = -180f,
                    max = 180f,
                    step = 0.1f,
                    defaultValue = defaultConfig.rotationLeft2.x,
                }
            },
            {
                "rotationLeft2Y", new CustomValueInfo
                {
                    index = 16,
                    name = "RY2 Left",
                    min = -180f,
                    max = 180f,
                    step = 0.1f,
                    defaultValue = defaultConfig.rotationLeft2.y,
                }
            },
            {
                "rotationLeft2Z", new CustomValueInfo
                {
                    index = 17,
                    name = "RZ2 Left",
                    min = -180f,
                    max = 180f,
                    step = 0.1f,
                    defaultValue = defaultConfig.rotationLeft2.z,
                }
            },
            {
                "rotationRight1X", new CustomValueInfo
                {
                    index = 18,
                    name = "RX1 Right",
                    min = -180f,
                    max = 180f,
                    step = 0.1f,
                    defaultValue = defaultConfig.rotationRight1.x,
                }
            },
            {
                "rotationRight1Y", new CustomValueInfo
                {
                    index = 19,
                    name = "RY1 Right",
                    min = -180f,
                    max = 180f,
                    step = 0.1f,
                    defaultValue = defaultConfig.rotationRight1.y,
                }
            },
            {
                "rotationRight1Z", new CustomValueInfo
                {
                    index = 20,
                    name = "RZ1 Right",
                    min = -180f,
                    max = 180f,
                    step = 0.1f,
                    defaultValue = defaultConfig.rotationRight1.z,
                }
            },
            {
                "rotationRight2X", new CustomValueInfo
                {
                    index = 21,
                    name = "RX2 Right",
                    min = -180f,
                    max = 180f,
                    step = 0.1f,
                    defaultValue = defaultConfig.rotationRight2.x,
                }
            },
            {
                "rotationRight2Y", new CustomValueInfo
                {
                    index = 22,
                    name = "RY2 Right",
                    min = -180f,
                    max = 180f,
                    step = 0.1f,
                    defaultValue = defaultConfig.rotationRight2.y,
                }
            },
            {
                "rotationRight2Z", new CustomValueInfo
                {
                    index = 23,
                    name = "RZ2 Right",
                    min = -180f,
                    max = 180f,
                    step = 0.1f,
                    defaultValue = defaultConfig.rotationRight2.z,
                }
            },
            {
                "randomPositionRangeX", new CustomValueInfo
                {
                    index = 24,
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
                    index = 25,
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
                    index = 26,
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
                    index = 27,
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
                    index = 28,
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
                    index = 29,
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
                    index = 30,
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
                    index = 31,
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
                    index = 32,
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
                    index = 33,
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
                    index = 34,
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
                    index = 35,
                    name = "T Offset",
                    min = -5f,
                    max = 5f,
                    step = 0.01f,
                    defaultValue = defaultConfig.timeOffset,
                }
            },
            {
                "easingType1", new CustomValueInfo
                {
                    index = 36,
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
                    index = 37,
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

        public ValueData[] positionLeft1Values
        {
            get => new ValueData[] { this["positionLeft1X"], this["positionLeft1Y"], this["positionLeft1Z"] };
        }
        public ValueData[] positionLeft2Values
        {
            get => new ValueData[] { this["positionLeft2X"], this["positionLeft2Y"], this["positionLeft2Z"] };
        }
        public ValueData[] positionRight1Values
        {
            get => new ValueData[] { this["positionRight1X"], this["positionRight1Y"], this["positionRight1Z"] };
        }
        public ValueData[] positionRight2Values
        {
            get => new ValueData[] { this["positionRight2X"], this["positionRight2Y"], this["positionRight2Z"] };
        }
        public ValueData[] rotationLeft1Values
        {
            get => new ValueData[] { this["rotationLeft1X"], this["rotationLeft1Y"], this["rotationLeft1Z"] };
        }
        public ValueData[] rotationLeft2Values
        {
            get => new ValueData[] { this["rotationLeft2X"], this["rotationLeft2Y"], this["rotationLeft2Z"] };
        }
        public ValueData[] rotationRight1Values
        {
            get => new ValueData[] { this["rotationRight1X"], this["rotationRight1Y"], this["rotationRight1Z"] };
        }
        public ValueData[] rotationRight2Values
        {
            get => new ValueData[] { this["rotationRight2X"], this["rotationRight2Y"], this["rotationRight2Z"] };
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
        public ValueData bpmValue => this["bpm"];
        public ValueData randomTimeCountValue => this["randomTimeCount"];
        public ValueData randomTimeValue => this["randomTime"];
        public ValueData timeRatioValue => this["timeRatio"];
        public ValueData timeOffsetValue => this["timeOffset"];
        public ValueData easingType1Value => this["easingType1"];
        public ValueData easingType2Value => this["easingType2"];

        public CustomValueInfo mirrorRotationZInfo => CustomValueInfoMap["mirrorRotationZ"];
        public CustomValueInfo bpmInfo => CustomValueInfoMap["bpm"];
        public CustomValueInfo randomTimeCountInfo => CustomValueInfoMap["randomTimeCount"];
        public CustomValueInfo randomTimeInfo => CustomValueInfoMap["randomTime"];
        public CustomValueInfo timeRatioInfo => CustomValueInfoMap["timeRatio"];
        public CustomValueInfo timeOffsetInfo => CustomValueInfoMap["timeOffset"];
        public CustomValueInfo easingType1Info => CustomValueInfoMap["easingType1"];
        public CustomValueInfo easingType2Info => CustomValueInfoMap["easingType2"];

        public Vector3 positionLeft1
        {
            get => positionLeft1Values.ToVector3();
            set => positionLeft1Values.FromVector3(value);
        }
        public Vector3 positionLeft2
        {
            get => positionLeft2Values.ToVector3();
            set => positionLeft2Values.FromVector3(value);
        }
        public Vector3 positionRight1
        {
            get => positionRight1Values.ToVector3();
            set => positionRight1Values.FromVector3(value);
        }
        public Vector3 positionRight2
        {
            get => positionRight2Values.ToVector3();
            set => positionRight2Values.FromVector3(value);
        }
        public Vector3 rotationLeft1
        {
            get => rotationLeft1Values.ToVector3();
            set => rotationLeft1Values.FromVector3(value);
        }
        public Vector3 rotationLeft2
        {
            get => rotationLeft2Values.ToVector3();
            set => rotationLeft2Values.FromVector3(value);
        }
        public Vector3 rotationRight1
        {
            get => rotationRight1Values.ToVector3();
            set => rotationRight1Values.FromVector3(value);
        }
        public Vector3 rotationRight2
        {
            get => rotationRight2Values.ToVector3();
            set => rotationRight2Values.FromVector3(value);
        }
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
            positionLeft1 = config.positionLeft1;
            positionLeft2 = config.positionLeft2;
            positionRight1 = config.positionRight1;
            positionRight2 = config.positionRight2;
            rotationLeft1 = config.rotationLeft1;
            rotationLeft2 = config.rotationLeft2;
            rotationRight1 = config.rotationRight1;
            rotationRight2 = config.rotationRight2;
            randomPositionRange = config.randomPositionRange;
            randomRotationRange = config.randomRotationRange;
            mirrorRotationZ = config.mirrorRotationZ;
            bpm = config.bpm;
            randomTimeCount = config.randomTimeCount;
            randomTime = config.randomTime;
            timeRatio = config.timeRatio;
            timeOffset = config.timeOffset;
            easingType1 = config.easingType1;
            easingType2 = config.easingType2;
        }

        private PsylliumAnimationConfig _config = new PsylliumAnimationConfig();

        public PsylliumAnimationConfig ToConfig()
        {
            _config.positionLeft1 = positionLeft1;
            _config.positionLeft2 = positionLeft2;
            _config.positionRight1 = positionRight1;
            _config.positionRight2 = positionRight2;
            _config.rotationLeft1 = rotationLeft1;
            _config.rotationLeft2 = rotationLeft2;
            _config.rotationRight1 = rotationRight1;
            _config.rotationRight2 = rotationRight2;
            _config.randomPositionRange = randomPositionRange;
            _config.randomRotationRange = randomRotationRange;
            _config.mirrorRotationZ = mirrorRotationZ;
            _config.bpm = bpm;
            _config.randomTimeCount = randomTimeCount;
            _config.randomTime = randomTime;
            _config.timeRatio = timeRatio;
            _config.timeOffset = timeOffset;
            _config.easingType1 = easingType1;
            _config.easingType2 = easingType2;
            return _config;
        }
    }
}