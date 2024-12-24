using System.Collections.Generic;
using UnityEngine;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    public class TransformDataPsylliumArea : TransformDataBase
    {
        public static TransformDataPsylliumArea defaultTrans = new TransformDataPsylliumArea();
        public static PsylliumAreaConfig defaultConfig = new PsylliumAreaConfig();

        public override TransformType type => TransformType.PsylliumArea;

        public override int valueCount => 14;

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

        public override Vector3 initialPosition => defaultConfig.position;
        public override Vector3 initialEulerAngles => defaultConfig.rotation;

        public TransformDataPsylliumArea()
        {
        }

        private readonly static Dictionary<string, CustomValueInfo> CustomValueInfoMap = new Dictionary<string, CustomValueInfo>
        {
            {
                "sizeX", new CustomValueInfo
                {
                    index = 6,
                    name = "SX",
                    min = 0f,
                    max = 100f,
                    step = 0.1f,
                    defaultValue = defaultConfig.size.x,
                }
            },
            {
                "sizeY", new CustomValueInfo
                {
                    index = 7,
                    name = "SY",
                    min = 0f,
                    max = 100f,
                    step = 0.1f,
                    defaultValue = defaultConfig.size.y,
                }
            },
            {
                "seatDistanceX", new CustomValueInfo
                {
                    index = 8,
                    name = "席幅",
                    min = 0f,
                    max = 10f,
                    step = 0.01f,
                    defaultValue = defaultConfig.seatDistance.x,
                }
            },
            {
                "seatDistanceY", new CustomValueInfo
                {
                    index = 9,
                    name = "列幅",
                    min = 0f,
                    max = 10f,
                    step = 0.01f,
                    defaultValue = defaultConfig.seatDistance.y,
                }
            },
            {
                "randomPositionRangeX", new CustomValueInfo
                {
                    index = 10,
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
                    index = 11,
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
                    index = 12,
                    name = "Z Random",
                    min = 0f,
                    max = 10f,
                    step = 0.01f,
                    defaultValue = defaultConfig.randomPositionRange.z,
                }
            },
            {
                "randomSeed", new CustomValueInfo
                {
                    index = 13,
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

        public ValueData[] sizeValues
        {
            get => new ValueData[] { this["sizeX"], this["sizeY"] };
        }
        public ValueData[] seatDistanceValues
        {
            get => new ValueData[] { this["seatDistanceX"], this["seatDistanceY"] };
        }
        public ValueData[] randomPositionRangeValues
        {
            get => new ValueData[] { this["randomPositionRangeX"], this["randomPositionRangeY"], this["randomPositionRangeZ"] };
        }
        public ValueData randomSeedValue => this["randomSeed"];

        public CustomValueInfo sizeXInfo => CustomValueInfoMap["sizeX"];
        public CustomValueInfo sizeYInfo => CustomValueInfoMap["sizeY"];
        public CustomValueInfo seatDistanceXInfo => CustomValueInfoMap["seatDistanceX"];
        public CustomValueInfo seatDistanceYInfo => CustomValueInfoMap["seatDistanceY"];
        public CustomValueInfo randomPositionRangeXInfo => CustomValueInfoMap["randomPositionRangeX"];
        public CustomValueInfo randomPositionRangeYInfo => CustomValueInfoMap["randomPositionRangeY"];
        public CustomValueInfo randomPositionRangeZInfo => CustomValueInfoMap["randomPositionRangeZ"];
        public CustomValueInfo randomSeedInfo => CustomValueInfoMap["randomSeed"];

        public Vector2 size
        {
            get => sizeValues.ToVector2();
            set => sizeValues.FromVector2(value);
        }
        public Vector2 seatDistance
        {
            get => seatDistanceValues.ToVector2();
            set => seatDistanceValues.FromVector2(value);
        }
        public Vector3 randomPositionRange
        {
            get => randomPositionRangeValues.ToVector3();
            set => randomPositionRangeValues.FromVector3(value);
        }
        public int randomSeed
        {
            get => randomSeedValue.intValue;
            set => randomSeedValue.intValue = value;
        }

        public void FromConfig(PsylliumAreaConfig config)
        {
            position = config.position;
            size = config.size;
            seatDistance = config.seatDistance;
            randomPositionRange = config.randomPositionRange;
            randomSeed = config.randomSeed;
        }

        private PsylliumAreaConfig _config = new PsylliumAreaConfig();

        public PsylliumAreaConfig ToConfig()
        {
            _config.position = position;
            _config.size = size;
            _config.seatDistance = seatDistance;
            _config.randomPositionRange = randomPositionRange;
            _config.randomSeed = randomSeed;
            return _config;
        }
    }
}