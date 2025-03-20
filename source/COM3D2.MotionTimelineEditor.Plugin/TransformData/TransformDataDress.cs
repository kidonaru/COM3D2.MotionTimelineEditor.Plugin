using System.Collections.Generic;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    public class TransformDataDress : TransformDataBase
    {
        public enum Index
        {
            Rid = 0,
        }

        public enum StrIndex
        {
            PropName = 0,
        }

        public static TransformDataDress defaultTrans = new TransformDataDress();
        public override TransformType type => TransformType.Dress;

        public override int valueCount => 1;
        public override int strValueCount => 1;

        public TransformDataDress()
        {
        }

        private readonly static Dictionary<string, CustomValueInfo> CustomValueInfoMap = new Dictionary<string, CustomValueInfo>
        {
            {
                "rid", new CustomValueInfo
                {
                    index = (int)Index.Rid,
                    name = "RID",
                    min = int.MinValue,
                    max = int.MinValue,
                    step = 1,
                    defaultValue = 0,
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
                "propName",
                new StrValueInfo
                {
                    index = (int)StrIndex.PropName,
                    name = "装備名",
                }
            },
        };

        public override Dictionary<string, StrValueInfo> GetStrValueInfoMap()
        {
            return StrValueInfoMap;
        }

        public ValueData ridValue => values[(int)Index.Rid];

        public CustomValueInfo ridInfo => CustomValueInfoMap["rid"];

        public int rid
        {
            get => ridValue.intValue;
            set => ridValue.intValue = value;
        }

        public string propName
        {
            get => strValues[(int)StrIndex.PropName];
            set => strValues[(int)StrIndex.PropName] = value;
        }
    }
}