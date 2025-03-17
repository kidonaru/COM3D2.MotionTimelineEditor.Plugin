using System.Collections.Generic;
using UnityEngine;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    public class TransformDataFingerBlend : TransformDataBase
    {
        public enum Index
        {
            ValueOpen = 0,
            ValueFist = 1,
            LockEnabled0 = 2,
            LockEnabled1 = 3,
            LockEnabled2 = 4,
            LockEnabled3 = 5,
            LockEnabled4 = 6,
            LockValueOpen0 = 7,
            LockValueOpen1 = 8,
            LockValueOpen2 = 9,
            LockValueOpen3 = 10,
            LockValueOpen4 = 11,
            LockValueFist0 = 12,
            LockValueFist1 = 13,
            LockValueFist2 = 14,
            LockValueFist3 = 15,
            LockValueFist4 = 16
        }

        public override TransformType type => TransformType.FingerBlend;

        public override int valueCount => 17;

        public TransformDataFingerBlend()
        {
        }

        private readonly static Dictionary<string, CustomValueInfo> CustomValueInfoMap = new Dictionary<string, CustomValueInfo>
        {
            {
                "value_open",
                new CustomValueInfo
                {
                    index = (int)Index.ValueOpen,
                    name = "開き具合",
                    defaultValue = 0,
                }
            },
            {
                "value_fist",
                new CustomValueInfo
                {
                    index = (int)Index.ValueFist,
                    name = "閉じ具合",
                    defaultValue = 0,
                }
            },
            {
                "lock_enabled0",
                new CustomValueInfo
                {
                    index = (int)Index.LockEnabled0,
                    name = "ロック(親)",
                    defaultValue = 0,
                }
            },
            {
                "lock_enabled1",
                new CustomValueInfo
                {
                    index = (int)Index.LockEnabled1,
                    name = "ロック(人)",
                    defaultValue = 0,
                }
            },
            {
                "lock_enabled2",
                new CustomValueInfo
                {
                    index = (int)Index.LockEnabled2,
                    name = "ロック(中)",
                    defaultValue = 0,
                }
            },
            {
                "lock_enabled3",
                new CustomValueInfo
                {
                    index = (int)Index.LockEnabled3,
                    name = "ロック(薬)",
                    defaultValue = 0,
                }
            },
            {
                "lock_enabled4",
                new CustomValueInfo
                {
                    index = (int)Index.LockEnabled4,
                    name = "ロック(子)",
                    defaultValue = 0,
                }
            },
            {
                "lock_value_open0",
                new CustomValueInfo
                {
                    index = (int)Index.LockValueOpen0,
                    name = "開き(親)",
                    defaultValue = 0,
                }
            },
            {
                "lock_value_open1",
                new CustomValueInfo
                {
                    index = (int)Index.LockValueOpen1,
                    name = "開き(人)",
                    defaultValue = 0,
                }
            },
            {
                "lock_value_open2",
                new CustomValueInfo
                {
                    index = (int)Index.LockValueOpen2,
                    name = "開き(中)",
                    defaultValue = 0,
                }
            },
            {
                "lock_value_open3",
                new CustomValueInfo
                {
                    index = (int)Index.LockValueOpen3,
                    name = "開き(薬)",
                    defaultValue = 0,
                }
            },
            {
                "lock_value_open4",
                new CustomValueInfo
                {
                    index = (int)Index.LockValueOpen4,
                    name = "開き(子)",
                    defaultValue = 0,
                }
            },
            {
                "lock_value_fist0",
                new CustomValueInfo
                {
                    index = (int)Index.LockValueFist0,
                    name = "閉じ(親)",
                    defaultValue = 0,
                }
            },
            {
                "lock_value_fist1",
                new CustomValueInfo
                {
                    index = (int)Index.LockValueFist1,
                    name = "閉じ(人)",
                    defaultValue = 0,
                }
            },
            {
                "lock_value_fist2",
                new CustomValueInfo
                {
                    index = (int)Index.LockValueFist2,
                    name = "閉じ(中)",
                    defaultValue = 0,
                }
            },
            {
                "lock_value_fist3",
                new CustomValueInfo
                {
                    index = (int)Index.LockValueFist3,
                    name = "閉じ(薬)",
                    defaultValue = 0,
                }
            },
            {
                "lock_value_fist4",
                new CustomValueInfo
                {
                    index = (int)Index.LockValueFist4,
                    name = "閉じ(子)",
                    defaultValue = 0,
                }
            },
        };

        public override Dictionary<string, CustomValueInfo> GetCustomValueInfoMap()
        {
            return CustomValueInfoMap;
        }

        public ValueData ValueOpenValue => values[(int)Index.ValueOpen];
        public ValueData ValueFistValue => values[(int)Index.ValueFist];
        public ValueData LockEnabled0Value => values[(int)Index.LockEnabled0];
        public ValueData LockEnabled1Value => values[(int)Index.LockEnabled1];
        public ValueData LockEnabled2Value => values[(int)Index.LockEnabled2];
        public ValueData LockEnabled3Value => values[(int)Index.LockEnabled3];
        public ValueData LockEnabled4Value => values[(int)Index.LockEnabled4];

        public ValueData[] LockValue0Values => new ValueData[]
        {
            values[(int)Index.LockValueOpen0],
            values[(int)Index.LockValueFist0]
        };

        public ValueData[] LockValue1Values => new ValueData[]
        {
            values[(int)Index.LockValueOpen1],
            values[(int)Index.LockValueFist1]
        };

        public ValueData[] LockValue2Values => new ValueData[]
        {
            values[(int)Index.LockValueOpen2],
            values[(int)Index.LockValueFist2]
        };

        public ValueData[] LockValue3Values => new ValueData[]
        {
            values[(int)Index.LockValueOpen3],
            values[(int)Index.LockValueFist3]
        };

        public ValueData[] LockValue4Values => new ValueData[]
        {
            values[(int)Index.LockValueOpen4],
            values[(int)Index.LockValueFist4]
        };

        // プロパティアクセサ
        public float ValueOpen
        {
            get => ValueOpenValue.value;
            set => ValueOpenValue.value = value;
        }

        public float ValueFist
        {
            get => ValueFistValue.value;
            set => ValueFistValue.value = value;
        }

        public bool LockEnabled0
        {
            get => LockEnabled0Value.boolValue;
            set => LockEnabled0Value.boolValue = value;
        }

        public bool LockEnabled1
        {
            get => LockEnabled1Value.boolValue;
            set => LockEnabled1Value.boolValue = value;
        }

        public bool LockEnabled2
        {
            get => LockEnabled2Value.boolValue;
            set => LockEnabled2Value.boolValue = value;
        }

        public bool LockEnabled3
        {
            get => LockEnabled3Value.boolValue;
            set => LockEnabled3Value.boolValue = value;
        }

        public bool LockEnabled4
        {
            get => LockEnabled4Value.boolValue;
            set => LockEnabled4Value.boolValue = value;
        }

        public Vector2 LockValue0
        {
            get => LockValue0Values.ToVector2();
            set => LockValue0Values.FromVector2(value);
        }

        public Vector2 LockValue1
        {
            get => LockValue1Values.ToVector2();
            set => LockValue1Values.FromVector2(value);
        }

        public Vector2 LockValue2
        {
            get => LockValue2Values.ToVector2();
            set => LockValue2Values.FromVector2(value);
        }

        public Vector2 LockValue3
        {
            get => LockValue3Values.ToVector2();
            set => LockValue3Values.FromVector2(value);
        }

        public Vector2 LockValue4
        {
            get => LockValue4Values.ToVector2();
            set => LockValue4Values.FromVector2(value);
        }

        public void UpdateFromArmFinger(FingerBlend.ArmFinger armFinger)
        {
            if (armFinger == null)
            {
                return;
            }

            ValueOpen = armFinger.value_open;
            ValueFist = armFinger.value_fist;

            LockEnabled0 = armFinger.lock_enabled0;
            LockEnabled1 = armFinger.lock_enabled1;
            LockEnabled2 = armFinger.lock_enabled2;
            LockEnabled3 = armFinger.lock_enabled3;
            LockEnabled4 = armFinger.lock_enabled4;

            LockValue0 = armFinger.lock_value0;
            LockValue1 = armFinger.lock_value1;
            LockValue2 = armFinger.lock_value2;
            LockValue3 = armFinger.lock_value3;
            LockValue4 = armFinger.lock_value4;
        }

        public void UpdateFromLegFinger(FingerBlend.LegFinger legFinger)
        {
            if (legFinger == null)
            {
                return;
            }

            ValueOpen = legFinger.value_open;
            ValueFist = legFinger.value_fist;

            LockEnabled0 = legFinger.lock_enabled0;
            LockEnabled1 = legFinger.lock_enabled1;
            LockEnabled2 = legFinger.lock_enabled2;

            LockValue0 = legFinger.lock_value0;
            LockValue1 = legFinger.lock_value1;
            LockValue2 = legFinger.lock_value2;
        }

        public void ApplyArmFinger(FingerBlend.ArmFinger armFinger)
        {
            armFinger.lock_enabled0 = LockEnabled0;
            armFinger.lock_enabled1 = LockEnabled1;
            armFinger.lock_enabled2 = LockEnabled2;
            armFinger.lock_enabled3 = LockEnabled3;
            armFinger.lock_enabled4 = LockEnabled4;

            armFinger.lock_value0 = LockValue0;
            armFinger.lock_value1 = LockValue1;
            armFinger.lock_value2 = LockValue2;
            armFinger.lock_value3 = LockValue3;
            armFinger.lock_value4 = LockValue4;

            armFinger.SetValueOpenOnly(ValueOpen);
            armFinger.SetValueFistOnly(ValueFist);
        }

        public void ApplyLegFinger(FingerBlend.LegFinger legFinger)
        {
            legFinger.lock_enabled0 = LockEnabled0;
            legFinger.lock_enabled1 = LockEnabled1;
            legFinger.lock_enabled2 = LockEnabled2;

            legFinger.lock_value0 = LockValue0;
            legFinger.lock_value1 = LockValue1;
            legFinger.lock_value2 = LockValue2;

            legFinger.SetValueOpenOnly(ValueOpen);
            legFinger.SetValueFistOnly(ValueFist);
        }
    }
}