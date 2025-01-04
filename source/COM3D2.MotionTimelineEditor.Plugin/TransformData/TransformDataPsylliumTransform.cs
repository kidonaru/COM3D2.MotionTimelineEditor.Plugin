using System.Collections.Generic;
using UnityEngine;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    public class TransformDataPsylliumTransform : TransformDataBase
    {
        public static TransformDataPsylliumTransform defaultTrans = new TransformDataPsylliumTransform();
        public static PsylliumTransformConfig defaultConfig = new PsylliumTransformConfig();

        public override TransformType type => TransformType.PsylliumTransform;

        public override int valueCount => 12;

        public override bool hasPosition => true;
        public override bool hasSubPosition => true;
        public override bool hasEulerAngles => true;
        public override bool hasSubEulerAngles => true;
        public override bool hasTangent => true;

        public override bool isFixRotation => false;

        public override ValueData[] positionValues
        {
            get => new ValueData[] { values[0], values[1], values[2] };
        }

        public override ValueData[] subPositionValues
        {
            get => new ValueData[] { values[3], values[4], values[5] };
        }

        public override ValueData[] eulerAnglesValues
        {
            get => new ValueData[] { values[6], values[7], values[8] };
        }

        public override ValueData[] subEulerAnglesValues
        {
            get => new ValueData[] { values[9], values[10], values[11] };
        }

        public override ValueData[] tangentValues => values;

        public override Vector3 initialPosition => defaultConfig.positionLeft;
        public override Vector3 initialSubPosition => defaultConfig.positionRight;
        public override Vector3 initialEulerAngles => defaultConfig.eulerAnglesLeft;
        public override Vector3 initialSubEulerAngles => defaultConfig.eulerAnglesRight;

        public TransformDataPsylliumTransform()
        {
        }

        public void FromConfig(PsylliumTransformConfig config)
        {
            position = config.positionLeft;
            subPosition = config.positionRight;
            eulerAngles = config.eulerAnglesLeft;
            subEulerAngles = config.eulerAnglesRight;
        }

        private PsylliumTransformConfig _config = new PsylliumTransformConfig();

        public PsylliumTransformConfig ToConfig()
        {
            _config.positionLeft = position;
            _config.positionRight = subPosition;
            _config.eulerAnglesLeft = eulerAngles;
            _config.eulerAnglesRight = subEulerAngles;
            return _config;
        }
    }
}