using System.Collections.Generic;
using COM3D2.MotionTimelineEditor.Plugin;

namespace COM3D2.MotionTimelineEditor_DCM.Plugin
{
    public class TransformDataSe : TransformDataBase
    {
        public enum Index
        {
            Interval = 0,
            IsLoop = 1
        }

        public enum StrIndex
        {
            FileName = 0
        }

        public override TransformType type => TransformType.Se;

        public override int valueCount => 2;
        public override int strValueCount => 1;

        public TransformDataSe()
        {
        }

        private readonly static Dictionary<string, CustomValueInfo> CustomValueInfoMap = new Dictionary<string, CustomValueInfo>
        {
            {
                "interval",
                new CustomValueInfo
                {
                    index = (int)Index.Interval,
                    name = "間隔",
                    defaultValue = 0f,
                }
            },
            {
                "isLoop",
                new CustomValueInfo
                {
                    index = (int)Index.IsLoop,
                    name = "Loop",
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
                "fileName",
                new StrValueInfo
                {
                    index = (int)StrIndex.FileName,
                    name = "SE名",
                }
            },
        };

        public override Dictionary<string, StrValueInfo> GetStrValueInfoMap()
        {
            return StrValueInfoMap;
        }

        public ValueData intervalValue => values[(int)Index.Interval];

        public ValueData isLoopValue => values[(int)Index.IsLoop];

        public string fileName
        {
            get => strValues[(int)StrIndex.FileName];
            set => strValues[(int)StrIndex.FileName] = value;
        }

        public float interval
        {
            get => intervalValue.value;
            set => intervalValue.value = value;
        }

        public bool isLoop
        {
            get => isLoopValue.boolValue;
            set => isLoopValue.boolValue = value;
        }
    }
}