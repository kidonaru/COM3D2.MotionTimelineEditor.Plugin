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

        public override bool hasPosition
        {
            get
            {
                return false;
            }
        }

        public override bool hasRotation
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
                return false;
            }
        }

        public override bool hasScale
        {
            get
            {
                return false;
            }
        }

        public override bool hasEasing
        {
            get
            {
                return false;
            }
        }

        public override bool hasTangent
        {
            get
            {
                return true;
            }
        }

        public TransformDataRotation()
        {
        }

        public override ValueData[] GetPositionValues()
        {
            return new ValueData[0];
        }

        public override ValueData[] GetRotationValues()
        {
            return new ValueData[] { values[0], values[1], values[2], values[3] };
        }

        public override ValueData[] GetEulerAnglesValues()
        {
            return new ValueData[0];
        }

        public override ValueData[] GetScaleValues()
        {
            return new ValueData[0];
        }

        public override ValueData GetEasingValue()
        {
            return null;
        }

        public override void Reset()
        {
            var boneType = BoneUtils.GetBoneTypeByName(name);
            var initialRotation = BoneUtils.GetInitialRotation(boneType);
            rotation = Quaternion.Euler(initialRotation);
        }
    }
}