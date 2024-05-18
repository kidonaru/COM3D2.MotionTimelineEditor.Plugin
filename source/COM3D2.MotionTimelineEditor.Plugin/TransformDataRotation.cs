using UnityEngine;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    public class TransformDataRotation : TransformDataBase
    {
        public override int valueCount
        {
            get
            {
                return 4;
            }
        }

        public override bool hasRotation
        {
            get
            {
                return true;
            }
        }

        public override bool hasTangent
        {
            get
            {
                return true;
            }
        }

        public override ValueData[] rotationValues
        {
            get
            {
                return new ValueData[] { values[0], values[1], values[2], values[3] };
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
                var boneType = BoneUtils.GetBoneTypeByName(name);
                var angles = BoneUtils.GetInitialEulerAngles(boneType);
                return angles;
            }
        }

        public TransformDataRotation()
        {
        }
    }
}