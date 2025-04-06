using System.Collections.Generic;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    public class TransformDataAnimation : TransformDataBase
    {
        public enum Index
        {
            Easing = 0,
            StartTime = 1,
            Weight = 2,
            Speed = 3,
            Loop = 4,
            OverrideTime = 5,
            Max = 6,
        }

        public enum StrIndex
        {
            AnmName = 0,
            Max = 1,
        }

        public static TransformDataAnimation defaultTrans = new TransformDataAnimation();

        public override TransformType type => TransformType.Animation;

        public override int valueCount => (int) Index.Max;
        public override int strValueCount => (int) StrIndex.Max;

        public override bool hasEasing => true;
        public override ValueData easingValue => values[(int)Index.Easing];

        public TransformDataAnimation()
        {
        }

        private readonly static Dictionary<string, CustomValueInfo> CustomValueInfoMap = new Dictionary<string, CustomValueInfo>
        {
            {
                "StartTime",
                new CustomValueInfo
                {
                    index = (int)Index.StartTime,
                    name = "開始時間",
                    min = 0f,
                    max = 10f,
                    step = 0.01f,
                    defaultValue = 0f,
                }
            },
            {
                "Weight",
                new CustomValueInfo
                {
                    index = (int)Index.Weight,
                    name = "重み",
                    min = 0f,
                    max = 1f,
                    step = 0.01f,
                    defaultValue = 1f,
                }
            },
            {
                "Speed",
                new CustomValueInfo
                {
                    index = (int)Index.Speed,
                    name = "再生速度",
                    min = 0f,
                    max = 2f,
                    step = 0.01f,
                    defaultValue = 1f,
                }
            },
            {
                "Loop",
                new CustomValueInfo
                {
                    index = (int)Index.Loop,
                    name = "ループ",
                    min = 0f,
                    max = 1f,
                    step = 1f,
                    defaultValue = 1f,
                }
            },
            {
                "OverrideTime",
                new CustomValueInfo
                {
                    index = (int)Index.OverrideTime,
                    name = "時間上書き",
                    min = 0f,
                    max = 1f,
                    step = 1f,
                    defaultValue = 0f,
                }
            },
        };

        public override Dictionary<string, CustomValueInfo> GetCustomValueInfoMap()
        {
            return CustomValueInfoMap;
        }

        private readonly static Dictionary<string, StrValueInfo> StrValueInfoMap = new Dictionary<string, StrValueInfo>
        {
            {
                "AnmName",
                new StrValueInfo
                {
                    index = (int)StrIndex.AnmName,
                    name = "アニメ名",
                }
            },
        };

        public override Dictionary<string, StrValueInfo> GetStrValueInfoMap()
        {
            return StrValueInfoMap;
        }

        public ValueData StartTimeValue => values[(int)Index.StartTime];
        public ValueData WeightValue => values[(int)Index.Weight];
        public ValueData SpeedValue => values[(int)Index.Speed];
        public ValueData LoopValue => values[(int)Index.Loop];
        public ValueData OverrideTimeValue => values[(int)Index.OverrideTime];

        public CustomValueInfo StartTimeInfo => CustomValueInfoMap["StartTime"];
        public CustomValueInfo WeightInfo => CustomValueInfoMap["Weight"];
        public CustomValueInfo SpeedInfo => CustomValueInfoMap["Speed"];
        public CustomValueInfo LoopInfo => CustomValueInfoMap["Loop"];
        public CustomValueInfo OverrideTimeInfo => CustomValueInfoMap["OverrideTime"];

        public string AnmName
        {
            get => strValues[(int)StrIndex.AnmName];
            set => strValues[(int)StrIndex.AnmName] = value;
        }

        public float StartTime
        {
            get => StartTimeValue.value;
            set => StartTimeValue.value = value;
        }

        public float Weight
        {
            get => WeightValue.value;
            set => WeightValue.value = value;
        }

        public float Speed
        {
            get => SpeedValue.value;
            set => SpeedValue.value = value;
        }

        public bool Loop
        {
            get => LoopValue.boolValue;
            set => LoopValue.boolValue = value;
        }

        public bool OverrideTime
        {
            get => OverrideTimeValue.boolValue;
            set => OverrideTimeValue.boolValue = value;
        }
    }
}