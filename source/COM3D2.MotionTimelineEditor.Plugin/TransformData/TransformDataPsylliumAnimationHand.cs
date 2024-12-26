using UnityEngine;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    public class TransformDataPsylliumAnimationHand : TransformDataBase
    {
        public static TransformDataPsylliumAnimationHand defaultTrans = new TransformDataPsylliumAnimationHand();
        public static PsylliumAnimationHandConfig defaultConfig = new PsylliumAnimationHandConfig();

        public override TransformType type => TransformType.PsylliumAnimationHand;

        public override int valueCount => 12;

        public override bool hasPosition => true;
        public override bool hasSubPosition => true;
        public override bool hasEulerAngles => true;
        public override bool hasSubEulerAngles => true;

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

        public override Vector3 initialPosition => defaultConfig.position1;
        public override Vector3 initialSubPosition => defaultConfig.position2;
        public override Vector3 initialEulerAngles => defaultConfig.eulerAngles1;
        public override Vector3 initialSubEulerAngles => defaultConfig.eulerAngles2;

        public TransformDataPsylliumAnimationHand()
        {
        }

        public override void Initialize(string name)
        {
            base.Initialize(name);
            isLeftHand = name == "PsylliumAnimationHandConfigLeft";
        }

        public bool isLeftHand { get; protected set; }

        public void FromConfig(PsylliumAnimationHandConfig config)
        {
            position = config.position1;
            subPosition = config.position2;
            eulerAngles = config.eulerAngles1;
            subEulerAngles = config.eulerAngles2;
        }

        private PsylliumAnimationHandConfig _config = new PsylliumAnimationHandConfig();

        public PsylliumAnimationHandConfig ToConfig()
        {
            _config.position1 = position;
            _config.position2 = subPosition;
            _config.eulerAngles1 = eulerAngles;
            _config.eulerAngles2 = subEulerAngles;
            return _config;
        }
    }
}