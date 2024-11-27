using System.Collections.Generic;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    public class TransformDataShapeKey : TransformDataBase
    {
        public override TransformType type
        {
            get
            {
                return TransformType.ShapeKey;
            }
        }

        public override int valueCount
        {
            get
            {
                return 2;
            }
        }

        public override bool hasEasing
        {
            get
            {
                return true;
            }
        }

        public override ValueData easingValue
        {
            get
            {
                return values[0];
            }
        }

        public TransformDataShapeKey()
        {
        }

        private readonly static Dictionary<string, CustomValueInfo> CustomValueInfoMap = new Dictionary<string, CustomValueInfo>
        {
            {
                "weight",
                new CustomValueInfo
                {
                    index = 1,
                    name = "値",
                    defaultValue = 0f,
                }
            },
        };

        public override Dictionary<string, CustomValueInfo> GetCustomValueInfoMap()
        {
            return CustomValueInfoMap;
        }

        public ValueData weightValue
        {
            get
            {
                return this["weight"];
            }
        }

        public float weight
        {
            get
            {
                return weightValue.value;
            }
            set
            {
                weightValue.value = value;
            }
        }

        public string slotName;
        public int maidSlotNo;
    }
}