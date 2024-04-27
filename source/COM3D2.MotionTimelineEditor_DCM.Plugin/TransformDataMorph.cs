using System.Collections.Generic;
using COM3D2.MotionTimelineEditor.Plugin;
using UnityEngine;

namespace COM3D2.MotionTimelineEditor_DCM.Plugin
{
    public class TransformDataMorph : TransformDataBase
    {
        public override int valueCount
        {
            get
            {
                return 1;
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
                return false;
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
                return false;
            }
        }

        public TransformDataMorph()
        {
        }

        public override ValueData[] GetPositionValues()
        {
            return new ValueData[0];
        }

        public override ValueData[] GetRotationValues()
        {
            return new ValueData[0];
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

        private readonly static Dictionary<string, int> CustomValueIndexMap = new Dictionary<string, int>
        {
            { "morphValue", 0 },
        };

        public override Dictionary<string, int> GetCustomValueIndexMap()
        {
            return CustomValueIndexMap;
        }
    }
}