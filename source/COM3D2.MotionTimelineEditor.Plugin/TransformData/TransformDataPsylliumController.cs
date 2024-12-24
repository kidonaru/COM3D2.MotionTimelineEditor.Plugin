
using System.IO;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    public class TransformDataPsylliumController : TransformDataBase
    {
        public static TransformDataPsylliumController defaultTrans = new TransformDataPsylliumController();

        public override TransformType type => TransformType.PsylliumController;

        public override int valueCount => 7;

        public override bool hasPosition => true;
        public override bool hasEulerAngles => true;
        public override bool hasVisible => true;

        public override ValueData[] positionValues
        {
            get => new ValueData[] { values[0], values[1], values[2] };
        }

        public override ValueData[] eulerAnglesValues
        {
            get => new ValueData[] { values[3], values[4], values[5] };
        }

        public override ValueData visibleValue => values[6];

        public TransformDataPsylliumController()
        {
        }
    }

}