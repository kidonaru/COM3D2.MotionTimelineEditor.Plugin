using System.Collections.Generic;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    public class TransformDataUndress : TransformDataBase
    {
        public override TransformType type => TransformType.Undress;

        public override int valueCount => 1;

        public TransformDataUndress()
        {
        }

        private readonly static Dictionary<string, CustomValueInfo> CustomValueInfoMap = new Dictionary<string, CustomValueInfo>
        {
            {
                "isVisible",
                new CustomValueInfo
                {
                    index = 0,
                    name = "表示",
                    defaultValue = 0f,
                }
            },
        };

        public override Dictionary<string, CustomValueInfo> GetCustomValueInfoMap()
        {
            return CustomValueInfoMap;
        }

        public ValueData isVisibleValue => this["isVisible"];

        public bool isVisible
        {
            get => isVisibleValue.boolValue;
            set => isVisibleValue.boolValue = value;
        }

        public DressSlotID slotId
        {
            get => DressUtils.GetDressSlotId(name);
        }

        public int maidSlotNo;
    }
}