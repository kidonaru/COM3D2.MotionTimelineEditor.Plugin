using System.Collections.Generic;
using COM3D2.MotionTimelineEditor.Plugin;

namespace COM3D2.MotionTimelineEditor_DCM.Plugin
{
    public class TransformDataLookAtTarget : TransformDataBase
    {
        public override int valueCount
        {
            get
            {
                return 3;
            }
        }

        public static readonly string[] TargetTypeNames = new string[]
        {
            "手動",
            "カメラ",
            "メイド",
            "モデル",
        };

        public TransformDataLookAtTarget()
        {
        }

        private readonly static Dictionary<string, int> CustomValueIndexMap = new Dictionary<string, int>
        {
            { "targetType", 0 },
            { "targetIndex", 1 },
            { "maidPointType", 2 },
        };

        public override Dictionary<string, int> GetCustomValueIndexMap()
        {
            return CustomValueIndexMap;
        }
    }
}