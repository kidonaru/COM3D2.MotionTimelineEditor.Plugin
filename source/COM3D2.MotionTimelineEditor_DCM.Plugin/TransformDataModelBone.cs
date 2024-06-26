
using System.IO;
using COM3D2.MotionTimelineEditor.Plugin;
using UnityEngine;

namespace COM3D2.MotionTimelineEditor_DCM.Plugin
{
    public class TransformDataModelBone : TransformDataBase
    {
        public override int valueCount
        {
            get
            {
                return 10;
            }
        }

        public override bool hasPosition
        {
            get
            {
                return true;
            }
        }

        public override bool hasEulerAngles
        {
            get
            {
                return true;
            }
        }

        public override bool hasScale
        {
            get
            {
                return true;
            }
        }

        public override bool hasEasing
        {
            get
            {
                return true;
            }
        }

        public override ValueData[] positionValues
        {
            get
            {
                return new ValueData[] { values[0], values[1], values[2] };
            }
        }

        public override ValueData[] eulerAnglesValues
        {
            get
            {
                return new ValueData[] { values[3], values[4], values[5] };
            }
        }

        public override ValueData[] scaleValues
        {
            get
            {
                return new ValueData[] { values[6], values[7], values[8] };
            }
        }

        public override ValueData easingValue
        {
            get
            {
                return values[9];
            }
        }

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
                return Quaternion.Euler(initialEulerAngles);
            }
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