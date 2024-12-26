using System.Collections.Generic;
using UnityEngine;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    public class TransformDataPsylliumHand : TransformDataBase
    {
        public static TransformDataPsylliumHand defaultTrans = new TransformDataPsylliumHand();
        public static PsylliumHandConfig defaultConfig = new PsylliumHandConfig();

        public override TransformType type => TransformType.PsylliumHand;

        public override int valueCount => 9;

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
                "barOffsetPositionX", new CustomValueInfo
                {
                    index = 1,
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
                    index = 2,
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
                    index = 3,
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
                    index = 4,
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
                    index = 5,
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
                    index = 6,
                    name = "RZ",
                    min = -180f,
                    max = 180f,
                    step = 0.1f,
                    defaultValue = defaultConfig.barOffsetRotation.z,
                }
            },
        };

        public override Dictionary<string, CustomValueInfo> GetCustomValueInfoMap()
        {
            return CustomValueInfoMap;
        }

        public ValueData handSpacingValue => this["handSpacing"];
        public ValueData[] barOffsetPositionValues
        {
            get => new ValueData[] { this["barOffsetPositionX"], this["barOffsetPositionY"], this["barOffsetPositionZ"] };
        }
        public ValueData[] barOffsetRotationValues
        {
            get => new ValueData[] { this["barOffsetRotationX"], this["barOffsetRotationY"], this["barOffsetRotationZ"] };
        }

        public CustomValueInfo handSpacingInfo => CustomValueInfoMap["handSpacing"];

        public float handSpacing
        {
            get => handSpacingValue.value;
            set => handSpacingValue.value = value;
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

        public void FromConfig(PsylliumHandConfig config)
        {
            handSpacing = config.handSpacing;
            barOffsetPosition = config.barOffsetPosition;
            barOffsetRotation = config.barOffsetRotation;
        }

        private PsylliumHandConfig _config = new PsylliumHandConfig();

        public PsylliumHandConfig ToConfig()
        {
            _config.handSpacing = handSpacing;
            _config.barOffsetPosition = barOffsetPosition;
            _config.barOffsetRotation = barOffsetRotation;
            return _config;
        }
    }
}