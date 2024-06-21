using System.Collections.Generic;
using UnityEngine;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    public interface ITransformData
    {
        string name { get; }
        int valueCount { get; }
        ValueData[] values { get; }
        int strValueCount { get; }
        string[] strValues { get; }
        Vector3 position { get; set; }
        Quaternion rotation { get; set; }
        Vector3 eulerAngles { get; set; }
        Vector3 normalizedEulerAngles { get; }
        Vector3 scale { get; set; }
        Color color { get; set; }
        int easing { get; set; }

        bool hasPosition { get; }
        bool hasRotation { get; }
        bool hasEulerAngles { get; }
        bool hasScale { get; }
        bool hasColor { get; }
        bool hasEasing { get; }
        bool hasTangent { get; }
        bool isHidden { get; }

        ValueData[] positionValues { get; }
        ValueData[] rotationValues { get; }
        ValueData[] eulerAnglesValues { get; }
        ValueData[] scaleValues { get; }
        ValueData[] colorValues { get; }
        ValueData easingValue { get; }

        Vector3 initialPosition { get; }
        Quaternion initialRotation { get; }
        Vector3 initialEulerAngles { get; }
        Vector3 initialScale { get; }
        Color initialColor { get; }

        ValueData this[string name] { get; }

        void Initialize(string name);

        void FixRotation(ITransformData prevTrans);
        void FixEulerAngles(ITransformData prevTrans);
        void UpdateTangent(
            ITransformData prevTransform,
            ITransformData nextTransform,
            float prevTime,
            float currentTime,
            float nextTime);

        void FromTransformData(ITransformData transform);

        void FromXml(TransformXml xml);
        TransformXml ToXml();

        Dictionary<string, int> GetCustomValueIndexMap();
        ValueData GetCustomValue(string customName);
        bool HasCustomValue(string customName);
        float GetInitialCustomValue(string customName);
        Dictionary<string, int> GetStrValueIndexMap();
        string GetStrValue(string keyName);
        void SetStrValue(string keyName, string value);
        bool HasStrValue(string keyName);
        ValueData[] GetValueDataList(TangentValueType valueType);
        TangentData[] GetInTangentDataList(TangentValueType valueType);
        TangentData[] GetOutTangentDataList(TangentValueType valueType);

        void Reset();
    }
}