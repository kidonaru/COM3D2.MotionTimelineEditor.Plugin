using System.Collections.Generic;
using COM3D2.MotionTimelineEditor.Plugin;
using UnityEngine;

namespace COM3D2.MotionTimelineEditor_DCM.Plugin
{
    public class TransformDataDepthOfField : TransformDataBase
    {
        public override int valueCount
        {
            get
            {
                return 7;
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

        public TransformDataDepthOfField()
        {
        }

        private readonly static Dictionary<string, CustomValueInfo> CustomValueInfoMap = new Dictionary<string, CustomValueInfo>
        {
            {
                "enabled", new CustomValueInfo
                {
                    index = 1,
                    name = "有効",
                    defaultValue = 0f,
                }
            },
            {
                "focalLength", new CustomValueInfo
                {
                    index = 2,
                    name = "焦点距離",
                    defaultValue = 10f,
                }
            },
            {
                "focalSize", new CustomValueInfo
                {
                    index = 3,
                    name = "焦点サイズ",
                    defaultValue = 0.05f,
                }
            },
            {
                "aperture", new CustomValueInfo
                {
                    index = 4,
                    name = "絞り値",
                    defaultValue = 11.5f,
                }
            },
            {
                "maxBlurSize", new CustomValueInfo
                {
                    index = 5,
                    name = "ブラーサイズ",
                    defaultValue = 2f,
                }
            },
            {
                "maidSlotNo", new CustomValueInfo
                {
                    index = 6,
                    name = "追従",
                    defaultValue = -1f,
                }
            },
        };

        public override Dictionary<string, CustomValueInfo> GetCustomValueInfoMap()
        {
            return CustomValueInfoMap;
        }
    }
}