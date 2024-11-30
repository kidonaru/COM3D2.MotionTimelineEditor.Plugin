
using System.IO;
using UnityEngine;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    public class TransformDataModelBone : TransformDataBase
    {
        public override TransformType type => TransformType.ModelBone;

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

        public override Vector3 initialPosition
        {
            get
            {
                var bone = modelManager.GetBone(name);
                if (bone != null)
                {
                    return bone.initialPosition;
                }
                return Vector3.zero;
            }
        }

        public override Quaternion initialRotation
        {
            get => Quaternion.Euler(initialEulerAngles);
        }

        public override Vector3 initialEulerAngles
        {
            get
            {
                var bone = modelManager.GetBone(name);
                if (bone != null)
                {
                    return bone.initialEulerAngles;
                }
                return Vector3.zero;
            }
        }

        public TransformDataModelBone()
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