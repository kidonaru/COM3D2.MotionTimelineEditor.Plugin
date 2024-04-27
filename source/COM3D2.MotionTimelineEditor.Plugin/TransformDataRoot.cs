using UnityEngine;

namespace COM3D2.MotionTimelineEditor.Plugin
{

    public class TransformDataRoot : TransformDataBase
    {
        public override int valueCount
        {
            get
            {
                return 7;
            }
        }

        public override bool hasPosition
        {
            get
            {
                return true;
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

        public TransformDataRoot()
        {
        }

        public override ValueData[] GetPositionValues()
        {
            return new ValueData[] { values[4], values[5], values[6] };
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
            return new ValueData[] { values[7], values[8], values[9] };
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
            position = new Vector3(0f, 0.9f, 0f);
        }
    }
}