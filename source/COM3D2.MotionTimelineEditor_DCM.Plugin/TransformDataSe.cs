
using System.Collections.Generic;
using COM3D2.MotionTimelineEditor.Plugin;

namespace COM3D2.MotionTimelineEditor_DCM.Plugin
{
    public class TransformDataSe : TransformDataBase
    {
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
                    index = 0,
                    name = "間隔",
                    defaultValue = 0f,
                }
            },
            {
                "isLoop",
                new CustomValueInfo
                {
                    index = 1,
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
                    index = 0,
                    name = "SE名",
                }
            },
        };

        public override Dictionary<string, StrValueInfo> GetStrValueInfoMap()
        {
            return StrValueInfoMap;
        }

        public ValueData intervalValue => this["interval"];

        public ValueData isLoopValue => this["isLoop"];

        public string fileName
        {
            get => GetStrValue("fileName");
            set => SetStrValue("fileName", value);
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