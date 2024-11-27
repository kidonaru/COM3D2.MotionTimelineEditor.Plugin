using System.Collections.Generic;
using UnityEngine;

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

        public void UpdateFromArmFinger(FingerBlend.ArmFinger armFinger)
        {
            if (armFinger == null)
            {
                return;
            }

            this["value_open"].value = armFinger.value_open;
            this["value_fist"].value = armFinger.value_fist;

            this["lock_enabled0"].boolValue = armFinger.lock_enabled0;
            this["lock_enabled1"].boolValue = armFinger.lock_enabled1;
            this["lock_enabled2"].boolValue = armFinger.lock_enabled2;
            this["lock_enabled3"].boolValue = armFinger.lock_enabled3;
            this["lock_enabled4"].boolValue = armFinger.lock_enabled4;

            this["lock_value_open0"].value = armFinger.lock_value0.x;
            this["lock_value_open1"].value = armFinger.lock_value1.x;
            this["lock_value_open2"].value = armFinger.lock_value2.x;
            this["lock_value_open3"].value = armFinger.lock_value3.x;
            this["lock_value_open4"].value = armFinger.lock_value4.x;

            this["lock_value_fist0"].value = armFinger.lock_value0.y;
            this["lock_value_fist1"].value = armFinger.lock_value1.y;
            this["lock_value_fist2"].value = armFinger.lock_value2.y;
            this["lock_value_fist3"].value = armFinger.lock_value3.y;
            this["lock_value_fist4"].value = armFinger.lock_value4.y;
        }

        public void UpdateFromLegFinger(FingerBlend.LegFinger legFinger)
        {
            if (legFinger == null)
            {
                return;
            }

            this["value_open"].value = legFinger.value_open;
            this["value_fist"].value = legFinger.value_fist;

            this["lock_enabled0"].boolValue = legFinger.lock_enabled0;
            this["lock_enabled1"].boolValue = legFinger.lock_enabled1;
            this["lock_enabled2"].boolValue = legFinger.lock_enabled2;

            this["lock_value_open0"].value = legFinger.lock_value0.x;
            this["lock_value_open1"].value = legFinger.lock_value1.x;
            this["lock_value_open2"].value = legFinger.lock_value2.x;

            this["lock_value_fist0"].value = legFinger.lock_value0.y;
            this["lock_value_fist1"].value = legFinger.lock_value1.y;
            this["lock_value_fist2"].value = legFinger.lock_value2.y;
        }

        public void ApplyArmFinger(FingerBlend.ArmFinger armFinger)
        {
            armFinger.lock_enabled0 = this["lock_enabled0"].boolValue;
            armFinger.lock_enabled1 = this["lock_enabled1"].boolValue;
            armFinger.lock_enabled2 = this["lock_enabled2"].boolValue;
            armFinger.lock_enabled3 = this["lock_enabled3"].boolValue;
            armFinger.lock_enabled4 = this["lock_enabled4"].boolValue;

            var lock_value0 = new Vector2(this["lock_value_open0"].value, this["lock_value_fist0"].value);
            var lock_value1 = new Vector2(this["lock_value_open1"].value, this["lock_value_fist1"].value);
            var lock_value2 = new Vector2(this["lock_value_open2"].value, this["lock_value_fist2"].value);
            var lock_value3 = new Vector2(this["lock_value_open3"].value, this["lock_value_fist3"].value);
            var lock_value4 = new Vector2(this["lock_value_open4"].value, this["lock_value_fist4"].value);

            armFinger.lock_value0 = lock_value0;
            armFinger.lock_value1 = lock_value1;
            armFinger.lock_value2 = lock_value2;
            armFinger.lock_value3 = lock_value3;
            armFinger.lock_value4 = lock_value4;

            armFinger.SetValueOpenOnly(this["value_open"].value);
            armFinger.SetValueFistOnly(this["value_fist"].value);
        }

        public void ApplyLegFinger(FingerBlend.LegFinger legFinger)
        {
            legFinger.lock_enabled0 = this["lock_enabled0"].boolValue;
            legFinger.lock_enabled1 = this["lock_enabled1"].boolValue;
            legFinger.lock_enabled2 = this["lock_enabled2"].boolValue;

            var lock_value0 = new Vector2(this["lock_value_open0"].value, this["lock_value_fist0"].value);
            var lock_value1 = new Vector2(this["lock_value_open1"].value, this["lock_value_fist1"].value);
            var lock_value2 = new Vector2(this["lock_value_open2"].value, this["lock_value_fist2"].value);

            legFinger.lock_value0 = lock_value0;
            legFinger.lock_value1 = lock_value1;
            legFinger.lock_value2 = lock_value2;

            legFinger.SetValueOpenOnly(this["value_open"].value);
            legFinger.SetValueFistOnly(this["value_fist"].value);
        }
    }
}