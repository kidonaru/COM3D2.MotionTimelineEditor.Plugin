using System.Collections.Generic;
using UnityEngine;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    public interface ITransformData
    {
        string name { get; }
        int valueCount { get; }
        ValueData[] values { get; }
        Vector3 position { get; set; }
        Quaternion rotation { get; set; }
        Vector3 eulerAngles { get; set; }
        Vector3 scale { get; set; }
        int easing { get; set; }

        bool hasPosition { get; }
        bool hasRotation { get; }
        bool hasEulerAngles { get; }
        bool hasScale { get; }
        bool hasEasing { get; }
        bool hasTangent { get; }
        bool isHidden { get; }

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

        ValueData[] GetPositionValues();
        ValueData[] GetRotationValues();
        ValueData[] GetEulerAnglesValues();
        ValueData[] GetScaleValues();
        ValueData GetEasingValue();
        Dictionary<string, int> GetCustomValueIndexMap();
        ValueData GetCustomValue(string customName);
        bool HasCustomValue(string customName);
        Vector3 GetInitialPosition();
        Quaternion GetInitialRotation();
        Vector3 GetInitialEulerAngles();
        Vector3 GetInitialScale();
        float GetInitialCustomValue(string customName);
        ValueData[] GetValueDataList(TangentValueType valueType);
        TangentData[] GetInTangentDataList(TangentValueType valueType);
        TangentData[] GetOutTangentDataList(TangentValueType valueType);

        void Reset();
    }
}