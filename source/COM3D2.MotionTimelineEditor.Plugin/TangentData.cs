using System.Collections.Generic;
using UnityEngine;

namespace COM3D2.MotionTimelineEditor.Plugin
{
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
        public static readonly List<string> TangentValueTypeNames = new List<string>
        {
            "X移動",
            "Y移動",
            "Z移動",
            "X回転",
            "Y回転",
            "Z回転",
            "W回転",
            "移動",
            "回転",
            "すべて",
        };

        public static readonly List<string> TangentTypeNames = new List<string> {
            "EaseInOut", "EaseIn", "EaseOut", "線形補間", "自動補間" };

        public float value { get; private set; }
        public float normalizedValue { get; set; }
        public bool isSmooth { get; set; }

        public bool shouldSerialize
        {
            get
            {
                return !isSmooth && normalizedValue != 0.0f;
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
            value = normalizedValue * baseTangent;
        }

        public void FromTangentData(TangentData tangent)
        {
            value = tangent.value;
            normalizedValue = tangent.normalizedValue;
            isSmooth = tangent.isSmooth;
        }
    }
}