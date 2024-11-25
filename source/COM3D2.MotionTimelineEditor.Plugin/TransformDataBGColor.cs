
using UnityEngine;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    public class TransformDataBGColor : TransformDataBase
    {
        public override int valueCount
        {
            get
            {
                return 3;
            }
        }

        public override bool hasColor
        {
            get
            {
                return true;
            }
        }

        public override ValueData[] colorValues
        {
            get
            {
                return new ValueData[] { values[0], values[1], values[2] };
            }
        }

        public override Color initialColor
        {
            get
            {
                return Color.black;
            }
        }

        public TransformDataBGColor()
        {
        }
    }
}