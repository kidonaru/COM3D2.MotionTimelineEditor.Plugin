using UnityEngine;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    public class TransformDataRotation : TransformDataBase
    {
        public override TransformType type => TransformType.Rotation;

        public override int valueCount => 4;

        public override bool hasRotation => true;

        public override bool hasTangent => true;

        public override bool isHidden
        {
            get
            {
                if (isHead)
                {
                    return !timeline.useHeadKey;
                }

                if (isBustL)
                {
                    return !timeline.useMuneKeyL;
                }

                if (isBustR)
                {
                    return !timeline.useMuneKeyR;
                }

                return false;
            }
        }

        public override ValueData[] rotationValues
        {
            get => new ValueData[] { values[0], values[1], values[2], values[3] };
        }

        public override ValueData[] tangentValues => values;

        public override Quaternion initialRotation
        {
            get => Quaternion.Euler(initialEulerAngles);
        }

        public override Vector3 initialEulerAngles
        {
            get
            {
                if (maidCache != null)
                {
                    return maidCache.GetInitialEulerAngles(name);
                }
                return Vector3.zero;
            }
        }

        public override SingleFrameType singleFrameType => SingleFrameType.None;

        public TransformDataRotation()
        {
        }

        public override void Initialize(string name)
        {
            base.Initialize(name);

            isBustL = name == "Mune_L";
            isBustR = name == "Mune_R";
            isHead = name == "Bip01 Head";
        }

        public bool isBustL { get; protected set; }
        public bool isBustR { get; protected set; }
        public bool isHead { get; protected set; }
    }
}