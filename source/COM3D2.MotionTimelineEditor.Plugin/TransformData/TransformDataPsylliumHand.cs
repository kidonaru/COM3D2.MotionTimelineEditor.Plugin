using System.Collections.Generic;
using UnityEngine;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    public class TransformDataPsylliumHand : TransformDataBase
    {
        public static TransformDataPsylliumHand defaultTrans = new TransformDataPsylliumHand();
        public static PsylliumHandConfig defaultConfig = new PsylliumHandConfig();

        public override TransformType type => TransformType.PsylliumHand;

        public override int valueCount => 15;

        public TransformDataPsylliumHand()
        {
        }

        private readonly static Dictionary<string, CustomValueInfo> CustomValueInfoMap = new Dictionary<string, CustomValueInfo>
        {
            {
                "handSpacing", new CustomValueInfo
                {
                    index = 0,
                    name = "両手間",
                    min = 0f,
                    max = 10f,
                    step = 0.01f,
                    defaultValue = defaultConfig.handSpacing,
                }
            },
            {
                "barCountWeight0", new CustomValueInfo
                {
                    index = 1,
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
                    index = 2,
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
                    index = 3,
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
                    index = 4,
                    name = "3個重み",
                    min = 0f,
                    max = 1f,
                    step = 0.01f,
                    defaultValue = defaultConfig.barCountWeight3,
                }
            },
            {
                "barOffsetPositionX", new CustomValueInfo
                {
                    index = 5,
                    name = "X",
                    min = -1f,
                    max = 1f,
                    step = 0.01f,
                    defaultValue = defaultConfig.barOffsetPosition.x,
                }
            },
            {
                "barOffsetPositionY", new CustomValueInfo
                {
                    index = 6,
                    name = "Y",
                    min = -1f,
                    max = 1f,
                    step = 0.01f,
                    defaultValue = defaultConfig.barOffsetPosition.y,
                }
            },
            {
                "barOffsetPositionZ", new CustomValueInfo
                {
                    index = 7,
                    name = "Z",
                    min = -1f,
                    max = 1f,
                    step = 0.01f,
                    defaultValue = defaultConfig.barOffsetPosition.z,
                }
            },
            {
                "barOffsetRotationX", new CustomValueInfo
                {
                    index = 8,
                    name = "RX",
                    min = -180f,
                    max = 180f,
                    step = 0.1f,
                    defaultValue = defaultConfig.barOffsetRotation.x,
                }
            },
            {
                "barOffsetRotationY", new CustomValueInfo
                {
                    index = 9,
                    name = "RY",
                    min = -180f,
                    max = 180f,
                    step = 0.1f,
                    defaultValue = defaultConfig.barOffsetRotation.y,
                }
            },
            {
                "barOffsetRotationZ", new CustomValueInfo
                {
                    index = 10,
                    name = "RZ",
                    min = -180f,
                    max = 180f,
                    step = 0.1f,
                    defaultValue = defaultConfig.barOffsetRotation.z,
                }
            },
            {
                "colorWeight1", new CustomValueInfo
                {
                    index = 11,
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
                    index = 12,
                    name = "色2重み",
                    min = 0f,
                    max = 1f,
                    step = 0.01f,
                    defaultValue = defaultConfig.colorWeight2,
                }
            },
            {
                "timeShiftMin", new CustomValueInfo
                {
                    index = 13,
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
                    index = 14,
                    name = "ShiftMax",
                    min = 0f,
                    max = 10f,
                    step = 0.1f,
                    defaultValue = defaultConfig.timeShiftMax,
                }
            },
        };

        public override Dictionary<string, CustomValueInfo> GetCustomValueInfoMap()
        {
            return CustomValueInfoMap;
        }

        public ValueData handSpacingValue => this["handSpacing"];
        public ValueData barCountWeight0Value => this["barCountWeight0"];
        public ValueData barCountWeight1Value => this["barCountWeight1"];
        public ValueData barCountWeight2Value => this["barCountWeight2"];
        public ValueData barCountWeight3Value => this["barCountWeight3"];
        public ValueData[] barOffsetPositionValues
        {
            get => new ValueData[] { this["barOffsetPositionX"], this["barOffsetPositionY"], this["barOffsetPositionZ"] };
        }
        public ValueData[] barOffsetRotationValues
        {
            get => new ValueData[] { this["barOffsetRotationX"], this["barOffsetRotationY"], this["barOffsetRotationZ"] };
        }
        public ValueData colorWeight1Value => this["colorWeight1"];
        public ValueData colorWeight2Value => this["colorWeight2"];
        public ValueData timeShiftMinValue => this["timeShiftMin"];
        public ValueData timeShiftMaxValue => this["timeShiftMax"];

        public CustomValueInfo handSpacingInfo => CustomValueInfoMap["handSpacing"];
        public CustomValueInfo barCountWeight0Info => CustomValueInfoMap["barCountWeight0"];
        public CustomValueInfo barCountWeight1Info => CustomValueInfoMap["barCountWeight1"];
        public CustomValueInfo barCountWeight2Info => CustomValueInfoMap["barCountWeight2"];
        public CustomValueInfo barCountWeight3Info => CustomValueInfoMap["barCountWeight3"];
        public CustomValueInfo colorWeight1Info => CustomValueInfoMap["colorWeight1"];
        public CustomValueInfo colorWeight2Info => CustomValueInfoMap["colorWeight2"];
        public CustomValueInfo timeShiftMinInfo => CustomValueInfoMap["timeShiftMin"];
        public CustomValueInfo timeShiftMaxInfo => CustomValueInfoMap["timeShiftMax"];

        public float handSpacing
        {
            get => handSpacingValue.value;
            set => handSpacingValue.value = value;
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
        public Vector3 barOffsetPosition
        {
            get => barOffsetPositionValues.ToVector3();
            set => barOffsetPositionValues.FromVector3(value);
        }
        public Vector3 barOffsetRotation
        {
            get => barOffsetRotationValues.ToVector3();
            set => barOffsetRotationValues.FromVector3(value);
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

        public void FromConfig(PsylliumHandConfig config)
        {
            handSpacing = config.handSpacing;
            barCountWeight0 = config.barCountWeight0;
            barCountWeight1 = config.barCountWeight1;
            barCountWeight2 = config.barCountWeight2;
            barCountWeight3 = config.barCountWeight3;
            barOffsetPosition = config.barOffsetPosition;
            barOffsetRotation = config.barOffsetRotation;
            colorWeight1 = config.colorWeight1;
            colorWeight2 = config.colorWeight2;
            timeShiftMin = config.timeShiftMin;
            timeShiftMax = config.timeShiftMax;
        }

        private PsylliumHandConfig _config = new PsylliumHandConfig();

        public PsylliumHandConfig ToConfig()
        {
            _config.handSpacing = handSpacing;
            _config.barCountWeight0 = barCountWeight0;
            _config.barCountWeight1 = barCountWeight1;
            _config.barCountWeight2 = barCountWeight2;
            _config.barCountWeight3 = barCountWeight3;
            _config.barOffsetPosition = barOffsetPosition;
            _config.barOffsetRotation = barOffsetRotation;
            _config.colorWeight1 = colorWeight1;
            _config.colorWeight2 = colorWeight2;
            _config.timeShiftMin = timeShiftMin;
            _config.timeShiftMax = timeShiftMax;
            return _config;
        }
    }
}