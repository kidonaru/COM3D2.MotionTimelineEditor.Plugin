using System.Collections.Generic;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    public class TransformDataLookAtTarget : TransformDataBase
    {
        public override TransformType type => TransformType.LookAtTarget;

        public override int valueCount => 3;

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

        private readonly static Dictionary<string, CustomValueInfo> CustomValueInfoMap = new Dictionary<string, CustomValueInfo>
        {
            {
                "targetType",
                new CustomValueInfo
                {
                    index = 0,
                    name = "注視先",
                    defaultValue = 0,
                }
            },
            {
                "targetIndex",
                new CustomValueInfo
                {
                    index = 1,
                    name = "対象番号",
                    defaultValue = 0,
                }
            },
            {
                "maidPointType",
                new CustomValueInfo
                {
                    index = 2,
                    name = "ポイント",
                    defaultValue = 0,
                }
            },
        };

        public override Dictionary<string, CustomValueInfo> GetCustomValueInfoMap()
        {
            return CustomValueInfoMap;
        }

        public ValueData targetTypeValue => this["targetType"];

        public ValueData targetIndexValue => this["targetIndex"];

        public ValueData maidPointTypeValue => this["maidPointType"];

        public LookAtTargetType targetType
        {
            get => (LookAtTargetType) targetTypeValue.intValue;
            set => targetTypeValue.intValue = (int) value;
        }

        public int targetIndex
        {
            get => targetIndexValue.intValue;
            set => targetIndexValue.intValue = value;
        }

        public MaidPointType maidPointType
        {
            get => (MaidPointType) maidPointTypeValue.intValue;
            set => maidPointTypeValue.intValue = (int) value;
        }
    }
}