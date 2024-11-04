using System.Collections.Generic;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    public class TransformDataFingerBlend : TransformDataBase
    {
        public override int valueCount
        {
            get
            {
                return 17;
            }
        }

        public TransformDataFingerBlend()
        {
        }

        private readonly static Dictionary<string, CustomValueInfo> CustomValueInfoMap = new Dictionary<string, CustomValueInfo>
        {
            {
                "value_open",
                new CustomValueInfo
                {
                    index = 0,
                    name = "開き具合",
                    defaultValue = 0,
                }
            },
            {
                "value_fist",
                new CustomValueInfo
                {
                    index = 1,
                    name = "閉じ具合",
                    defaultValue = 0,
                }
            },
            {
                "lock_enabled0",
                new CustomValueInfo
                {
                    index = 2,
                    name = "ロック(親)",
                    defaultValue = 0,
                }
            },
            {
                "lock_enabled1",
                new CustomValueInfo
                {
                    index = 3,
                    name = "ロック(人)",
                    defaultValue = 0,
                }
            },
            {
                "lock_enabled2",
                new CustomValueInfo
                {
                    index = 4,
                    name = "ロック(中)",
                    defaultValue = 0,
                }
            },
            {
                "lock_enabled3",
                new CustomValueInfo
                {
                    index = 5,
                    name = "ロック(薬)",
                    defaultValue = 0,
                }
            },
            {
                "lock_enabled4",
                new CustomValueInfo
                {
                    index = 6,
                    name = "ロック(子)",
                    defaultValue = 0,
                }
            },
            {
                "lock_value_open0",
                new CustomValueInfo
                {
                    index = 7,
                    name = "開き(親)",
                    defaultValue = 0,
                }
            },
            {
                "lock_value_open1",
                new CustomValueInfo
                {
                    index = 8,
                    name = "開き(人)",
                    defaultValue = 0,
                }
            },
            {
                "lock_value_open2",
                new CustomValueInfo
                {
                    index = 9,
                    name = "開き(中)",
                    defaultValue = 0,
                }
            },
            {
                "lock_value_open3",
                new CustomValueInfo
                {
                    index = 10,
                    name = "開き(薬)",
                    defaultValue = 0,
                }
            },
            {
                "lock_value_open4",
                new CustomValueInfo
                {
                    index = 11,
                    name = "開き(子)",
                    defaultValue = 0,
                }
            },
            {
                "lock_value_fist0",
                new CustomValueInfo
                {
                    index = 12,
                    name = "閉じ(親)",
                    defaultValue = 0,
                }
            },
            {
                "lock_value_fist1",
                new CustomValueInfo
                {
                    index = 13,
                    name = "閉じ(人)",
                    defaultValue = 0,
                }
            },
            {
                "lock_value_fist2",
                new CustomValueInfo
                {
                    index = 14,
                    name = "閉じ(中)",
                    defaultValue = 0,
                }
            },
            {
                "lock_value_fist3",
                new CustomValueInfo
                {
                    index = 15,
                    name = "閉じ(薬)",
                    defaultValue = 0,
                }
            },
            {
                "lock_value_fist4",
                new CustomValueInfo
                {
                    index = 16,
                    name = "閉じ(子)",
                    defaultValue = 0,
                }
            },
        };

        public override Dictionary<string, CustomValueInfo> GetCustomValueInfoMap()
        {
            return CustomValueInfoMap;
        }
    }
}