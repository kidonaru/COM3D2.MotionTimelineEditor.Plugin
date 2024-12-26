using System.Collections.Generic;
using UnityEngine;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    public class TransformDataPsylliumArea : TransformDataBase
    {
        public static TransformDataPsylliumArea defaultTrans = new TransformDataPsylliumArea();
        public static PsylliumAreaConfig defaultConfig = new PsylliumAreaConfig();

        public override TransformType type => TransformType.PsylliumArea;

        public override int valueCount => 20;

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
                "barCountWeight0", new CustomValueInfo
                {
                    index = 13,
                    name = "0個重み",
                    min = 0f,
                    max = 1f,
                    step = 0.01f,
                    defaultValue = defaultConfig.barCountWeight0,
                }
            },
            {
                "barCountWeight1", new CustomValueInfo
                {
                    index = 14,
                    name = "1個重み",
                    min = 0f,
                    max = 1f,
                    step = 0.01f,
                    defaultValue = defaultConfig.barCountWeight1,
                }
            },
            {
                "barCountWeight2", new CustomValueInfo
                {
                    index = 15,
                    name = "2個重み",
                    min = 0f,
                    max = 1f,
                    step = 0.01f,
                    defaultValue = defaultConfig.barCountWeight2,
                }
            },
            {
                "barCountWeight3", new CustomValueInfo
                {
                    index = 16,
                    name = "3個重み",
                    min = 0f,
                    max = 1f,
                    step = 0.01f,
                    defaultValue = defaultConfig.barCountWeight3,
                }
            },
            {
                "colorWeight1", new CustomValueInfo
                {
                    index = 17,
                    name = "色1重み",
                    min = 0f,
                    max = 1f,
                    step = 0.01f,
                    defaultValue = defaultConfig.colorWeight1,
                }
            },
            {
                "colorWeight2", new CustomValueInfo
                {
                    index = 18,
                    name = "色2重み",
                    min = 0f,
                    max = 1f,
                    step = 0.01f,
                    defaultValue = defaultConfig.colorWeight2,
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
        public ValueData barCountWeight0Value => this["barCountWeight0"];
        public ValueData barCountWeight1Value => this["barCountWeight1"];
        public ValueData barCountWeight2Value => this["barCountWeight2"];
        public ValueData barCountWeight3Value => this["barCountWeight3"];
        public ValueData colorWeight1Value => this["colorWeight1"];
        public ValueData colorWeight2Value => this["colorWeight2"];
        public ValueData randomSeedValue => this["randomSeed"];

        public CustomValueInfo sizeXInfo => CustomValueInfoMap["sizeX"];
        public CustomValueInfo sizeYInfo => CustomValueInfoMap["sizeY"];
        public CustomValueInfo seatDistanceXInfo => CustomValueInfoMap["seatDistanceX"];
        public CustomValueInfo seatDistanceYInfo => CustomValueInfoMap["seatDistanceY"];
        public CustomValueInfo randomPositionRangeXInfo => CustomValueInfoMap["randomPositionRangeX"];
        public CustomValueInfo randomPositionRangeYInfo => CustomValueInfoMap["randomPositionRangeY"];
        public CustomValueInfo randomPositionRangeZInfo => CustomValueInfoMap["randomPositionRangeZ"];
        public CustomValueInfo barCountWeight0Info => CustomValueInfoMap["barCountWeight0"];
        public CustomValueInfo barCountWeight1Info => CustomValueInfoMap["barCountWeight1"];
        public CustomValueInfo barCountWeight2Info => CustomValueInfoMap["barCountWeight2"];
        public CustomValueInfo barCountWeight3Info => CustomValueInfoMap["barCountWeight3"];
        public CustomValueInfo colorWeight1Info => CustomValueInfoMap["colorWeight1"];
        public CustomValueInfo colorWeight2Info => CustomValueInfoMap["colorWeight2"];
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
        public float barCountWeight0
        {
            get => barCountWeight0Value.value;
            set => barCountWeight0Value.value = value;
        }
        public float barCountWeight1
        {
            get => barCountWeight1Value.value;
            set => barCountWeight1Value.value = value;
        }
        public float barCountWeight2
        {
            get => barCountWeight2Value.value;
            set => barCountWeight2Value.value = value;
        }
        public float barCountWeight3
        {
            get => barCountWeight3Value.value;
            set => barCountWeight3Value.value = value;
        }
        public float colorWeight1
        {
            get => colorWeight1Value.value;
            set => colorWeight1Value.value = value;
        }
        public float colorWeight2
        {
            get => colorWeight2Value.value;
            set => colorWeight2Value.value = value;
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
            barCountWeight0 = config.barCountWeight0;
            barCountWeight1 = config.barCountWeight1;
            barCountWeight2 = config.barCountWeight2;
            barCountWeight3 = config.barCountWeight3;
            colorWeight1 = config.colorWeight1;
            colorWeight2 = config.colorWeight2;
            randomSeed = config.randomSeed;
        }

        private PsylliumAreaConfig _config = new PsylliumAreaConfig();

        public PsylliumAreaConfig ToConfig()
        {
            _config.position = position;
            _config.size = size;
            _config.seatDistance = seatDistance;
            _config.randomPositionRange = randomPositionRange;
            _config.barCountWeight0 = barCountWeight0;
            _config.barCountWeight1 = barCountWeight1;
            _config.barCountWeight2 = barCountWeight2;
            _config.barCountWeight3 = barCountWeight3;
            _config.colorWeight1 = colorWeight1;
            _config.colorWeight2 = colorWeight2;
            _config.randomSeed = randomSeed;
            return _config;
        }
    }
}