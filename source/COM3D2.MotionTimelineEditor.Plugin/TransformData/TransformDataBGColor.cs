
using UnityEngine;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    public class TransformDataBGColor : TransformDataBase
    {
        public override TransformType type => TransformType.BGColor;

        public override int valueCount => 3;

        public override bool hasColor => true;

        public override ValueData[] colorValues
        {
            get => new ValueData[] { values[0], values[1], values[2] };
        }

        public override Color initialColor => Color.black;

        public TransformDataBGColor()
        {
        }
    }
}