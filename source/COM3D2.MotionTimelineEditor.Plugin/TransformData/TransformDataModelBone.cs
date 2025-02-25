
using System.IO;
using UnityEngine;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    public class TransformDataModelBone : TransformDataBase
    {
        public override TransformType type => TransformType.ModelBone;

        public override int valueCount => 11;

        public override bool hasPosition => true;
        public override bool hasRotation => true;
        public override bool hasScale => true;
        public override bool hasEasing => !timeline.isTangentModelBone;
        public override bool hasTangent => timeline.isTangentModelBone;

        public override ValueData[] positionValues
        {
            get => new ValueData[] { values[0], values[1], values[2] };
        }

        public override ValueData[] rotationValues
        {
            get => new ValueData[] { values[3], values[4], values[5], values[6] };
        }

        public override ValueData[] scaleValues
        {
            get => new ValueData[] { values[7], values[8], values[9] };
        }

        public override ValueData easingValue => values[10];
        public override ValueData[] tangentValues => baseValues;

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
            get
            {
                var bone = modelManager.GetBone(name);
                if (bone != null)
                {
                    return bone.initialRotation;
                }
                return Quaternion.identity;
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