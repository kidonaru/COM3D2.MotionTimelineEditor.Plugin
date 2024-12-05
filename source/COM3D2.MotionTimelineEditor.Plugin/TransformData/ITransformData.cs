using System.Collections.Generic;
using UnityEngine;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    public enum TransformType
    {
        None = 0,
        BG,
        BGColor,
        BGModel,
        Camera,
        DepthOfField,
        DistanceFog,
        ExtendBone,
        Eyes,
        FingerBlend,
        Grounding,
        IKHold,
        Light,
        LookAtTarget,
        Model,
        ModelBone,
        Move,
        Paraffin,
        Rimlight,
        Root,
        Rotation,
        ShapeKey,
        StageLight,
        StageLightController,
        Undress,
        Voice,

        // DCM
        Morph = 1000,
        Se,
        Text,
    }

    public class CustomValueInfo
    {
        public int index;
        public string name;
        public float defaultValue;
    }

    public class StrValueInfo
    {
        public int index;
        public string name;
        public string defaultValue;
    }

    public interface ITransformData
    {
        string name { get; }
        TransformType type { get; }
        int valueCount { get; }
        ValueData[] values { get; }
        int strValueCount { get; }
        string[] strValues { get; }
        Vector3 position { get; set; }
        Vector3 subPosition { get; set; }
        Quaternion rotation { get; set; }
        Quaternion subRotation { get; set; }
        Vector3 eulerAngles { get; set; }
        Vector3 subEulerAngles { get; set; }
        Vector3 normalizedEulerAngles { get; }
        Vector3 normalizedSubEulerAngles { get; }
        Vector3 scale { get; set; }
        Color color { get; set; }
        Color subColor { get; set; }
        bool visible { get; set; }
        int easing { get; set; }

        bool hasPosition { get; }
        bool hasSubPosition { get; }
        bool hasRotation { get; }
        bool hasSubRotation { get; }
        bool hasEulerAngles { get; }
        bool hasSubEulerAngles { get; }
        bool hasScale { get; }
        bool hasColor { get; }
        bool hasSubColor { get; }
        bool hasVisible { get; }
        bool hasEasing { get; }
        bool hasTangent { get; }
        bool isHidden { get; }
        bool isGlobal { get; }

        ValueData[] positionValues { get; }
        ValueData[] subPositionValues { get; }
        ValueData[] rotationValues { get; }
        ValueData[] subRotationValues { get; }
        ValueData[] eulerAnglesValues { get; }
        ValueData[] subEulerAnglesValues { get; }
        ValueData[] scaleValues { get; }
        ValueData[] colorValues { get; }
        ValueData[] subColorValues { get; }
        ValueData visibleValue { get; }
        ValueData easingValue { get; }
        ValueData[] tangentValues { get; }

        Vector3 initialPosition { get; }
        Vector3 initialSubPosition { get; }
        Quaternion initialRotation { get; }
        Quaternion initialSubRotation { get; }
        Vector3 initialEulerAngles { get; }
        Vector3 initialSubEulerAngles { get; }
        Vector3 initialScale { get; }
        Color initialColor { get; }
        Color initialSubColor { get; }
        bool initialVisible { get; }

        SingleFrameType singleFrameType { get; }

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

        void InitTangent();

        void FromXml(TransformXml xml);
        TransformXml ToXml();

        Dictionary<string, CustomValueInfo> GetCustomValueInfoMap();
        CustomValueInfo GetCustomValueInfo(string customKey);
        ValueData GetCustomValue(string customKey);
        string GetCustomValueName(string customKey);
        bool HasCustomValue(string customKey);
        float GetDefaultCustomValue(string customKey);
        Dictionary<string, StrValueInfo> GetStrValueInfoMap();
        StrValueInfo GetStrValueInfo(string keyName);
        string GetStrValue(string keyName);
        string GetStrValueName(string keyName);
        void SetStrValue(string keyName, string value);
        bool HasStrValue(string keyName);
        ValueData[] GetValueDataList(TangentValueType valueType);
        TangentData[] GetInTangentDataList(TangentValueType valueType);
        TangentData[] GetOutTangentDataList(TangentValueType valueType);

        void Reset();

        ITransformData Clone();
    }
}