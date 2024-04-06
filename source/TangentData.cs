using UnityEngine;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    using MTE = MotionTimelineEditor;

    public enum TangentValueType
    {
        X,
        Y,
        Z,
        RX,
        RY,
        RZ,
        RW,
        Move,
        Rotation,
        All,
        Max,
    }

    public enum TangentType
    {
        EaseInOut,
        EaseIn,
        EaseOut,
        Linear,
        Smooth,
    }

    public class TangentData
    {
        public static readonly string[] TangentValueTypeNames = new string[] {
            "X移動", "Y移動", "Z移動", "X回転", "Y回転", "Z回転", "W回転", "移動", "回転", "すべて" };

        public static readonly string[] TangentTypeNames = new string[] {
            "EaseInOut", "EaseIn", "EaseOut", "線形補完", "自動補完" };

        private float _value;
        private float _normalizedValue;
        private bool _isSmooth;

        public float value
        {
            get
            {
                return _value;
            }
        }

        public float normalizedValue
        {
            get
            {
                return _normalizedValue;
            }
            set
            {
                _normalizedValue = value;
            }
        }

        public bool isSmooth
        {
            get
            {
                return _isSmooth;
            }
            set
            {
                _isSmooth = value;
            }
        }

        public TangentData()
        {
        }

        public TangentData(TangentData tangent)
        {
            FromTangentData(tangent);
        }

        public void UpdateValue(float baseTangent)
        {
            _value = _normalizedValue * baseTangent;
        }

        public void FromTangentData(TangentData tangent)
        {
            _value = tangent._value;
            _normalizedValue = tangent._normalizedValue;
            _isSmooth = tangent._isSmooth;
        }
    }
}