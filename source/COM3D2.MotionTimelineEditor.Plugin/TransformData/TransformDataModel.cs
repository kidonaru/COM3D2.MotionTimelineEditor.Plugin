
using System.IO;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    public class TransformDataModel : TransformDataBase
    {
        public override TransformType type => TransformType.Model;

        public override int valueCount => 10;

        public override bool hasPosition => true;
        public override bool hasEulerAngles =>  true;
        public override bool hasScale => true;
        public override bool hasEasing => true;

        public override ValueData[] positionValues
        {
            get => new ValueData[] { values[0], values[1], values[2] };
        }

        public override ValueData[] eulerAnglesValues
        {
            get => new ValueData[] { values[3], values[4], values[5] };
        }

        public override ValueData[] scaleValues
        {
            get => new ValueData[] { values[6], values[7], values[8] };
        }

        public override ValueData easingValue => values[9];

        public TransformDataModel()
        {
        }

        public override void FromXml(TransformXml xml)
        {
            base.FromXml(xml);

            if (name.EndsWith(".menu", System.StringComparison.Ordinal))
            {
                name = Path.GetFileName(name);
            }
        }
    }

}