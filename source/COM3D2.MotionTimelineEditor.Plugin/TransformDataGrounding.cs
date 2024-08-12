using System.Collections.Generic;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    public class TransformDataGrounding : TransformDataBase
    {
        public override int valueCount
        {
            get
            {
                return 6;
            }
        }

        public TransformDataGrounding()
        {
        }

        private readonly static Dictionary<string, CustomValueInfo> CustomValueInfoMap = new Dictionary<string, CustomValueInfo>
        {
            {
                "isFootGrounding",
                new CustomValueInfo
                {
                    index = 0,
                    name = "足の接地処理",
                    defaultValue = 0,
                }
            },
            {
                "floorHeight",
                new CustomValueInfo
                {
                    index = 1,
                    name = "床の高さ",
                    defaultValue = 0f,
                }
            },
            {
                "footBaseOffset",
                new CustomValueInfo
                {
                    index = 2,
                    name = "足首の高さ",
                    defaultValue = 0.05f,
                }
            },
            {
                "footStretchHeight",
                new CustomValueInfo
                {
                    index = 3,
                    name = "伸ばす高さ",
                    defaultValue = 0.1f,
                }
            },
            {
                "footStretchAngle",
                new CustomValueInfo
                {
                    index = 4,
                    name = "伸ばす角度",
                    defaultValue = 45f,
                }
            },
            {
                "footGroundAngle",
                new CustomValueInfo
                {
                    index = 5,
                    name = "接地時角度",
                    defaultValue = 90f,
                }
            },

        };

        public override Dictionary<string, CustomValueInfo> GetCustomValueInfoMap()
        {
            return CustomValueInfoMap;
        }
    }
}