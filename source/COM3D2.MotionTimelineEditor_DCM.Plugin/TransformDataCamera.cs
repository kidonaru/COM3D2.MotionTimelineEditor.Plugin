using System.Collections.Generic;
using COM3D2.MotionTimelineEditor.Plugin;
using UnityEngine;

namespace COM3D2.MotionTimelineEditor_DCM.Plugin
{
    public class TransformDataCamera : TransformDataBase
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
                return !timeline.isTangentCamera;
            }
        }

        public override bool hasTangent
        {
            get
            {
                return timeline.isTangentCamera;
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
                return new ValueData[] { values[7], values[8], values[9] };
            }
        }

        public override ValueData easingValue
        {
            get
            {
                return values[6];
            }
        }

        public override ValueData[] tangentValues
        {
            get
            {
                return values;
            }
        }

        public override Vector3 initialScale
        {
            get
            {
                return new Vector3(1f, 35f, 0f); // 距離, FoV, ダミー
            }
        }

        public TransformDataCamera()
        {
        }
    }
}