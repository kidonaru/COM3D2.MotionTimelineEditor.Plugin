using System.Collections.Generic;
using UnityEngine;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    public class TransformDataPsylliumPattern : TransformDataBase
    {
        public enum Index
        {
            PositionX = 0,
            PositionY = 1,
            PositionZ = 2,
            EulerX = 3,
            EulerY = 4,
            EulerZ = 5,
            TimeCount = 6,
            TimeRange = 7,
            TimeShiftMin = 8,
            TimeShiftMax = 9,
            RandomSeed = 10
        }

        public static TransformDataPsylliumPattern defaultTrans = new TransformDataPsylliumPattern();
        public static PsylliumPatternConfig defaultConfig = new PsylliumPatternConfig();

        public override TransformType type => TransformType.PsylliumPattern;

        public override int valueCount => 11;

        public override bool hasPosition => true;
        public override bool hasEulerAngles => true;

        public override ValueData[] positionValues
        {
            get => new ValueData[] { 
                values[(int)Index.PositionX], 
                values[(int)Index.PositionY], 
                values[(int)Index.PositionZ] 
            };
        }

        public override ValueData[] eulerAnglesValues
        {
            get => new ValueData[] { 
                values[(int)Index.EulerX], 
                values[(int)Index.EulerY], 
                values[(int)Index.EulerZ] 
            };
        }

        public override Vector3 initialPosition => defaultConfig.randomPositionRange;
        public override Vector3 initialEulerAngles => defaultConfig.randomEulerAnglesRange;

        public TransformDataPsylliumPattern()
        {
        }

        private readonly static Dictionary<string, CustomValueInfo> CustomValueInfoMap = new Dictionary<string, CustomValueInfo>
        {
            {
                "timeCount", new CustomValueInfo
                {
                    index = (int)Index.TimeCount,
                    name = "T Count",
                    min = 1f,
                    max = 100f,
                    step = 1f,
                    defaultValue = defaultConfig.timeCount,
                }
            },
            {
                "timeRange", new CustomValueInfo
                {
                    index = (int)Index.TimeRange,
                    name = "T Range",
                    min = 0f,
                    max = 1f,
                    step = 0.01f,
                    defaultValue = defaultConfig.timeRange,
                }
            },
            {
                "timeShiftMin", new CustomValueInfo
                {
                    index = (int)Index.TimeShiftMin,
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
                    index = (int)Index.TimeShiftMax,
                    name = "ShiftMax",
                    min = 0f,
                    max = 10f,
                    step = 0.1f,
                    defaultValue = defaultConfig.timeShiftMax,
                }
            },
            {
                "randomSeed", new CustomValueInfo
                {
                    index = (int)Index.RandomSeed,
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

        public ValueData timeCountValue => values[(int)Index.TimeCount];
        public ValueData timeRangeValue => values[(int)Index.TimeRange];
        public ValueData timeShiftMinValue => values[(int)Index.TimeShiftMin];
        public ValueData timeShiftMaxValue => values[(int)Index.TimeShiftMax];
        public ValueData randomSeedValue => values[(int)Index.RandomSeed];

        public CustomValueInfo timeCountInfo => CustomValueInfoMap["timeCount"];
        public CustomValueInfo timeRangeInfo => CustomValueInfoMap["timeRange"];
        public CustomValueInfo timeShiftMinInfo => CustomValueInfoMap["timeShiftMin"];
        public CustomValueInfo timeShiftMaxInfo => CustomValueInfoMap["timeShiftMax"];
        public CustomValueInfo randomSeedInfo => CustomValueInfoMap["randomSeed"];

        public int timeCount
        {
            get => timeCountValue.intValue;
            set => timeCountValue.intValue = value;
        }
        public float timeRange
        {
            get => timeRangeValue.value;
            set => timeRangeValue.value = value;
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
        public int randomSeed
        {
            get => randomSeedValue.intValue;
            set => randomSeedValue.intValue = value;
        }

        public void FromConfig(PsylliumPatternConfig config)
        {
            position = config.randomPositionRange;
            eulerAngles = config.randomEulerAnglesRange;
            timeCount = config.timeCount;
            timeRange = config.timeRange;
            timeShiftMin = config.timeShiftMin;
            timeShiftMax = config.timeShiftMax;
            randomSeed = config.randomSeed;
        }

        private PsylliumPatternConfig _config = new PsylliumPatternConfig();

        public PsylliumPatternConfig ToConfig()
        {
            _config.randomPositionRange = position;
            _config.randomEulerAnglesRange = eulerAngles;
            _config.timeCount = timeCount;
            _config.timeRange = timeRange;
            _config.timeShiftMin = timeShiftMin;
            _config.timeShiftMax = timeShiftMax;
            _config.randomSeed = randomSeed;
            return _config;
        }
    }
}