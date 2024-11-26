using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    public abstract class TransformDataBase : ITransformData
    {
        public string name { get; protected set; }

        public virtual int valueCount
        {
            get
            {
                return 0;
            }
        }

        private ValueData[] _values = new ValueData[0];
        public ValueData[] values
        {
            get
            {
                return _values;
            }
        }

        public virtual int strValueCount
        {
            get
            {
                return 0;
            }
        }

        private string[] _strValues = new string[0];
        public string[] strValues
        {
            get
            {
                return _strValues;
            }
        }

        public Vector3 position
        {
            get
            {
                return positionValues.ToVector3();
            }
            set
            {
                positionValues.FromVector3(value);
            }
        }

        public Vector3 subPosition
        {
            get
            {
                return subPositionValues.ToVector3();
            }
            set
            {
                subPositionValues.FromVector3(value);
            }
        }

        public Quaternion rotation
        {
            get
            {
                return rotationValues.ToQuaternion();
            }
            set
            {
                rotationValues.FromQuaternion(value);
            }
        }

        public Quaternion subRotation
        {
            get
            {
                return subRotationValues.ToQuaternion();
            }
            set
            {
                subRotationValues.FromQuaternion(value);
            }
        }

        public Vector3 eulerAngles
        {
            get
            {
                if (hasEulerAngles)
                {
                    return eulerAnglesValues.ToVector3();
                }
                else if (hasRotation)
                {
                    return rotation.eulerAngles;
                }
                return Vector3.zero;
            }
            set
            {
                if (hasEulerAngles)
                {
                    eulerAnglesValues.FromVector3(value);
                }
                else if (hasRotation)
                {
                    rotation = Quaternion.Euler(value);
                }
            }
        }

        public Vector3 subEulerAngles
        {
            get
            {
                if (hasSubEulerAngles)
                {
                    return subEulerAnglesValues.ToVector3();
                }
                else if (hasSubRotation)
                {
                    return subRotation.eulerAngles;
                }
                return Vector3.zero;
            }
            set
            {
                if (hasSubEulerAngles)
                {
                    subEulerAnglesValues.FromVector3(value);
                }
                else if (hasSubRotation)
                {
                    subRotation = Quaternion.Euler(value);
                }
            }
        }

        public Vector3 normalizedEulerAngles
        {
            get
            {
                return GetNormalizedEulerAngles(eulerAngles);
            }
        }

        public Vector3 normalizedSubEulerAngles
        {
            get
            {
                return GetNormalizedEulerAngles(subEulerAngles);
            }
        }

        public Vector3 scale
        {
            get
            {
                return scaleValues.ToVector3();
            }
            set
            {
                scaleValues.FromVector3(value);
            }
        }

        public Color color
        {
            get
            {
                return colorValues.ToColor();
            }
            set
            {
                colorValues.FromColor(value);
            }
        }

        public Color subColor
        {
            get
            {
                return subColorValues.ToColor();
            }
            set
            {
                subColorValues.FromColor(value);
            }
        }

        public bool visible
        {
            get
            {
                return visibleValue.boolValue;
            }
            set
            {
                visibleValue.boolValue = value;
            }
        }

        public int easing
        {
            get
            {
                return easingValue.intValue;
            }
            set
            {
                easingValue.intValue = value;
            }
        }

        public virtual bool hasPosition
        {
            get
            {
                return false;
            }
        }
        public virtual bool hasSubPosition
        {
            get
            {
                return false;
            }
        }
        public virtual bool hasRotation
        {
            get
            {
                return false;
            }
        }
        public virtual bool hasSubRotation
        {
            get
            {
                return false;
            }
        }
        public virtual bool hasEulerAngles
        {
            get
            {
                return false;
            }
        }
        public virtual bool hasSubEulerAngles
        {
            get
            {
                return false;
            }
        }
        public virtual bool hasScale
        {
            get
            {
                return false;
            }
        }
        public virtual bool hasColor
        {
            get
            {
                return false;
            }
        }
        public virtual bool hasSubColor
        {
            get
            {
                return false;
            }
        }
        public virtual bool hasVisible
        {
            get
            {
                return false;
            }
        }
        public virtual bool hasEasing
        {
            get
            {
                return false;
            }
        }
        public virtual bool hasTangent
        {
            get
            {
                return false;
            }
        }

        public bool isHidden
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

        public virtual bool isGlobal
        {
            get
            {
                return false;
            }
        }

        public virtual ValueData[] positionValues
        {
            get
            {
                return new ValueData[0];
            }
        }
        public virtual ValueData[] subPositionValues
        {
            get
            {
                return new ValueData[0];
            }
        }
        public virtual ValueData[] rotationValues
        {
            get
            {
                return new ValueData[0];
            }
        }
        public virtual ValueData[] subRotationValues
        {
            get
            {
                return new ValueData[0];
            }
        }
        public virtual ValueData[] eulerAnglesValues
        {
            get
            {
                return new ValueData[0];
            }
        }
        public virtual ValueData[] subEulerAnglesValues
        {
            get
            {
                return new ValueData[0];
            }
        }
        public virtual ValueData[] scaleValues
        {
            get
            {
                return new ValueData[0];
            }
        }
        public virtual ValueData[] colorValues
        {
            get
            {
                return new ValueData[0];
            }
        }
        public virtual ValueData[] subColorValues
        {
            get
            {
                return new ValueData[0];
            }
        }
        public virtual ValueData visibleValue
        {
            get
            {
                return new ValueData();
            }
        }
        public virtual ValueData easingValue
        {
            get
            {
                return new ValueData();
            }
        }

        public virtual ValueData[] tangentValues
        {
            get
            {
                return new ValueData[0];
            }
        }

        private ValueData[] _baseValues = null;

        public ValueData[] baseValues
        {
            get
            {
                if (_baseValues == null)
                {
                    var length = 0;
                    if (hasPosition) length += 3;
                    if (hasRotation) length += 4;
                    if (hasScale) length += 3;
                    _baseValues = new ValueData[length];
                }

                int index = 0;
                if (hasPosition)
                {
                    var positionValues = this.positionValues;
                    _baseValues[index++] = positionValues[0];
                    _baseValues[index++] = positionValues[1];
                    _baseValues[index++] = positionValues[2];
                }
                if (hasRotation)
                {
                    var rotationValues = this.rotationValues;
                    _baseValues[index++] = rotationValues[0];
                    _baseValues[index++] = rotationValues[1];
                    _baseValues[index++] = rotationValues[2];
                    _baseValues[index++] = rotationValues[3];
                }
                if (hasScale)
                {
                    var scaleValues = this.scaleValues;
                    _baseValues[index++] = scaleValues[0];
                    _baseValues[index++] = scaleValues[1];
                    _baseValues[index++] = scaleValues[2];
                }

                return _baseValues;
            }
        }

        public virtual Vector3 initialPosition
        {
            get
            {
                return Vector3.zero;
            }
        }

        public virtual Vector3 initialSubPosition
        {
            get
            {
                return Vector3.zero;
            }
        }

        public virtual Quaternion initialRotation
        {
            get
            {
                return Quaternion.identity;
            }
        }

        public virtual Quaternion initialSubRotation
        {
            get
            {
                return Quaternion.identity;
            }
        }

        public virtual Vector3 initialEulerAngles
        {
            get
            {
                return Vector3.zero;
            }
        }

        public virtual Vector3 initialSubEulerAngles
        {
            get
            {
                return Vector3.zero;
            }
        }

        public virtual Vector3 initialScale
        {
            get
            {
                return Vector3.one;
            }
        }

        public virtual Color initialColor
        {
            get
            {
                return Color.white;
            }
        }

        public virtual Color initialSubColor
        {
            get
            {
                return Color.white;
            }
        }

        public virtual bool initialVisible
        {
            get
            {
                return true;
            }
        }

        public virtual SingleFrameType singleFrameType
        {
            get
            {
                return timeline.singleFrameType;
            }
        }

        public ValueData this[string name]
        {
            get
            {
                return GetCustomValue(name);
            }
        }

        public bool isBustL { get; protected set; }
        public bool isBustR { get; protected set; }
        public bool isHead { get; protected set; }

        public static readonly string[] PositionNames = new string[] { "X", "Y", "Z" };
        public static readonly string[] RotationNames = new string[] { "RX", "RY", "RZ", "RW" };
        public static readonly string[] ScaleNames = new string[] { "SX", "SY", "SZ" };

        protected static Config config
        {
            get
            {
                return ConfigManager.config;
            }
        }

        protected static TimelineManager timelineManager
        {
            get
            {
                return TimelineManager.instance;
            }
        }

        protected static TimelineData timeline
        {
            get
            {
                return timelineManager.timeline;
            }
        }

        protected static MaidManager maidManager
        {
            get
            {
                return MaidManager.instance;
            }
        }

        protected static MaidCache maidCache
        {
            get
            {
                return maidManager.maidCache;
            }
        }

        protected static StudioModelManager modelManager
        {
            get
            {
                return StudioModelManager.instance;
            }
        }

        public void Initialize(string name)
        {
            this.name = name;
            isBustL = name == "Mune_L";
            isBustR = name == "Mune_R";
            isHead = name == "Bip01 Head";

            var length = valueCount;
            if (_values.Length != length)
            {
                _values = new ValueData[length];

                var tangentPair = config.defaultTangentPair;

                for (int i = 0; i < length; i++)
                {
                    _values[i] = new ValueData
                    {
                        inTangent = new TangentData
                        {
                            normalizedValue = tangentPair.inTangent,
                            isSmooth = tangentPair.isSmooth,
                        },
                        outTangent = new TangentData
                        {
                            normalizedValue = tangentPair.outTangent,
                            isSmooth = tangentPair.isSmooth,
                        },
                    };
                }
            }

            if (_strValues.Length != strValueCount)
            {
                _strValues = new string[strValueCount];
                for (int i = 0; i < strValueCount; i++)
                {
                    _strValues[i] = string.Empty;
                }
            }
        }

        /// <summary>
        /// 最短の補間経路に修正
        /// </summary>
        /// <param name="prevBone"></param>
        public void FixRotation(ITransformData _prevTrans)
        {
            if (!hasRotation)
            {
                return;
            }

            var prevTrans = _prevTrans as TransformDataBase;
            if (prevTrans == null || prevTrans == this)
            {
                return;
            }

            var prevRot = prevTrans.rotation;
            var rot = this.rotation;
            var dot = Quaternion.Dot(prevRot, rot);

            if (dot < 0.0f)
            {
                this.rotation = new Quaternion(-rot.x, -rot.y, -rot.z, -rot.w);
            }
        }

        public static Vector3 GetFixedEulerAngles(Vector3 angles, Vector3 prevAngles)
        {
            var diff = angles - prevAngles;

            for (int i = 0; i < 3; i++)
            {
                int iDiff = (int) diff[i];
                if (iDiff > 180)
                {
                    angles[i] -= (iDiff + 180) / 360 * 360;
                }
                else if (iDiff < -180)
                {
                    angles[i] -= (iDiff - 180) / 360 * 360;
                }
            }

            return angles;
        }

        public static Vector3 GetNormalizedEulerAngles(Vector3 angles)
        {
            for (int i = 0; i < 3; i++)
            {
                int value = (int) angles[i];
                if (value > 180)
                {
                    angles[i] -= (value + 180) / 360 * 360;
                }
                else if (value < -180)
                {
                    angles[i] -= (value - 180) / 360 * 360;
                }
            }

            return angles;
        }

        public void FixEulerAngles(ITransformData _prevTrans)
        {
            if (!hasEulerAngles)
            {
                return;
            }

            var prevTrans = _prevTrans as TransformDataBase;
            if (prevTrans == null || prevTrans == this)
            {
                return;
            }

            var prevEuler = prevTrans.eulerAngles;
            var euler = this.eulerAngles;
            this.eulerAngles = GetFixedEulerAngles(euler, prevEuler);
        }

        public void UpdateTangent(
            ITransformData prevTransform,
            ITransformData nextTransform,
            float prevTime,
            float currentTime,
            float nextTime)
        {
            if (!hasTangent)
            {
                return;
            }

            if (prevTransform == null || nextTransform == null)
            {
                PluginUtils.LogError("UpdateTangent：前後のTransformが見つかりません。");
                return;
            }

            var prevValues = prevTransform.tangentValues;
            var currentValues = this.tangentValues;
            var nextValues = nextTransform.tangentValues;
            float dt0 = currentTime - prevTime;
            float dt1 = nextTime - currentTime;
            float dt = dt0 + dt1;

            if (dt0 <= 0f || dt1 <= 0f)
            {
                PluginUtils.LogError("UpdateTangent：フレーム時間が不正です。");
                return;
            }

            float dt0_inv = 1f / dt0;
            float dt1_inv = 1f / dt1;
            float dt_inv = 1f / dt;

            for (int i = 0; i < currentValues.Length; i++)
            {
                var inTangent = values[i].inTangent;
                var outTangent = values[i].outTangent;

                float x0 = prevValues[i].value;
                float x1 = currentValues[i].value;
                float x2 = nextValues[i].value;
                float dx0 = x1 - x0;
                float dx1 = x2 - x1;

                if (dx0 == 0f && dx1 == 0f)
                {
                    // do nothing
                }
                else if (dx0 == 0f)
                {
                    dx0 = Mathf.Sign(dx1) * 0.01f;
                }
                else if (dx1 == 0f)
                {
                    dx1 = Mathf.Sign(dx0) * 0.01f;
                }

                float v0 = dx0 * dt0_inv;
                float v1 = dx1 * dt1_inv;

                if (inTangent.isSmooth || outTangent.isSmooth)
                {
                    var tan = (x2 - x0) * dt_inv;
                    float tan0 = 0f;
                    float tan1 = 0f;

                    if (v0 != 0f && v1 != 0f)
                    {
                        tan0 = tan / v0;
                        tan1 = tan / v1;
                    }

                    if (inTangent.isSmooth)
                    {
                        inTangent.normalizedValue = tan0;
                    }
                    if (outTangent.isSmooth)
                    {
                        outTangent.normalizedValue = tan1;
                    }
                }

                inTangent.UpdateValue(v0);
                outTangent.UpdateValue(v1);
            }
        }

        public void FromTransformData(ITransformData transform)
        {
            for (int i = 0; i < valueCount; i++)
            {
                values[i].FromValue(transform.values[i]);
            }

            for (int i = 0; i < strValueCount; i++)
            {
                strValues[i] = transform.strValues[i];
            }
        }

        public void InitTangent()
        {
            if (hasTangent)
            {
                var tangentPair = config.defaultTangentPair;

                foreach (var value in _values)
                {
                    value.inTangent = new TangentData
                    {
                        normalizedValue = tangentPair.inTangent,
                        isSmooth = tangentPair.isSmooth,
                    };
                    value.outTangent = new TangentData
                    {
                        normalizedValue = tangentPair.outTangent,
                        isSmooth = tangentPair.isSmooth,
                    };
                }
            }

            if (hasEasing)
            {
                easing = (int) config.defaultEasingType;
            }
        }

        protected float[] _valuesForXml
        {
            get
            {
                var result = new float[values.Length];
                for (int i = 0; i < values.Length; i++)
                {
                    result[i] = values[i].value;
                }
                return result;
            }
            set
            {
                for (int i = 0; i < values.Length; i++)
                {
                    if (i < value.Length)
                    {
                        values[i].value = value[i];
                    }
                    else
                    {
                        values[i].value = 0f;
                    }
                }
            }
        }

        protected float[] _normalizedInTangents
        {
            get
            {
                if (!ShouldSerializeInTangents())
                {
                    return null;
                }
                var result = new float[values.Length];
                for (int i = 0; i < values.Length; i++)
                {
                    result[i] = values[i].inTangent.normalizedValue;
                }
                return result;
            }
            set
            {
                for (int i = 0; i < values.Length; i++)
                {
                    if (value != null && i < value.Length)
                    {
                        values[i].inTangent.normalizedValue = value[i];
                    }
                    else
                    {
                        values[i].inTangent.normalizedValue = 0f;
                    }
                }
            }
        }

        public bool ShouldSerializeInTangents()
        {
            if (!hasTangent)
            {
                return false;
            }
            return values.Any(value => value.inTangent.shouldSerialize);
        }

        protected float[] _normalizedOutTangents
        {
            get
            {
                if (!ShouldSerializeOutTangents())
                {
                    return null;
                }
                var result = new float[values.Length];
                for (int i = 0; i < values.Length; i++)
                {
                    result[i] = values[i].outTangent.normalizedValue;
                }
                return result;
            }
            set
            {
                for (int i = 0; i < values.Length; i++)
                {
                    if (value != null && i < value.Length)
                    {
                        values[i].outTangent.normalizedValue = value[i];
                    }
                    else
                    {
                        values[i].outTangent.normalizedValue = 0f;
                    }
                }
            }
        }

        public bool ShouldSerializeOutTangents()
        {
            if (!hasTangent)
            {
                return false;
            }
            return values.Any(value => value.outTangent.shouldSerialize);
        }

        protected int _inSmoothBit
        {
            get
            {
                if (!hasTangent)
                {
                    return 0;
                }

                var result = 0;
                for (int i = 0; i < values.Length; i++)
                {
                    int value = values[i].inTangent.isSmooth ? 1 : 0;
                    result |= value << i;
                }
                return result;
            }
            set
            {
                for (int i = 0; i < values.Length; i++)
                {
                    values[i].inTangent.isSmooth = (value & (1 << i)) != 0;
                }
            }
        }

        protected int _outSmoothBit
        {
            get
            {
                if (!hasTangent)
                {
                    return 0;
                }

                var result = 0;
                for (int i = 0; i < values.Length; i++)
                {
                    int value = values[i].outTangent.isSmooth ? 1 : 0;
                    result |= value << i;
                }
                return result;
            }
            set
            {
                for (int i = 0; i < values.Length; i++)
                {
                    values[i].outTangent.isSmooth = (value & (1 << i)) != 0;
                }
            }
        }

        private string[] _strValuesForXml
        {
            get
            {
                if (strValues.Length == 0)
                {
                    return null;
                }
                return strValues;
            }
            set
            {
                if (value == null)
                {
                    return;
                }

                for (int i = 0; i < value.Length; i++)
                {
                    if (i >= strValues.Length)
                    {
                        break;
                    }
                    strValues[i] = value[i];
                }
            }
        }

        public virtual void FromXml(TransformXml xml)
        {
            name = xml.name;
            _valuesForXml = xml.values;
            _normalizedInTangents = xml.inTangents;
            _normalizedOutTangents = xml.outTangents;
            _inSmoothBit = xml.inSmoothBit;
            _outSmoothBit = xml.outSmoothBit;
            _strValuesForXml = xml.strValues;
        }

        public virtual TransformXml ToXml()
        {
            var xml = new TransformXml
            {
                name = name,
                values = _valuesForXml,
                inTangents = _normalizedInTangents,
                outTangents = _normalizedOutTangents,
                inSmoothBit = _inSmoothBit,
                outSmoothBit = _outSmoothBit,
                strValues = _strValuesForXml,
            };
            return xml;
        }

        public virtual Dictionary<string, CustomValueInfo> GetCustomValueInfoMap()
        {
            return new Dictionary<string, CustomValueInfo>();
        }

        public CustomValueInfo GetCustomValueInfo(string customKey)
        {
            CustomValueInfo info;
            if (GetCustomValueInfoMap().TryGetValue(customKey, out info))
            {
                return info;
            }

            PluginUtils.LogError("CustomValueが見つかりません customKey={0}", customKey);
            return null;
        }

        public ValueData GetCustomValue(string customKey)
        {
            var info = GetCustomValueInfo(customKey);
            if (info != null)
            {
                return values[info.index];
            }
            return new ValueData();
        }

        public string GetCustomValueName(string customKey)
        {
            var info = GetCustomValueInfo(customKey);
            if (info != null)
            {
                return info.name;
            }
            return customKey;
        }

        public bool HasCustomValue(string customKey)
        {
            return GetCustomValueInfoMap().ContainsKey(customKey);
        }

        public float GetDefaultCustomValue(string customKey)
        {
            var info = GetCustomValueInfo(customKey);
            if (info != null)
            {
                return info.defaultValue;
            }
            return 0f;
        }

        public virtual Dictionary<string, StrValueInfo> GetStrValueInfoMap()
        {
            return new Dictionary<string, StrValueInfo>();
        }

        public StrValueInfo GetStrValueInfo(string keyName)
        {
            StrValueInfo info;
            if (GetStrValueInfoMap().TryGetValue(keyName, out info))
            {
                return info;
            }

            PluginUtils.LogError("StrValueが見つかりません keyName={0}", keyName);
            return null;
        }

        public string GetStrValue(string keyName)
        {
            var info = GetStrValueInfo(keyName);
            if (info != null)
            {
                return strValues[info.index];
            }
            return string.Empty;
        }

        public string GetStrValueName(string keyName)
        {
            var info = GetStrValueInfo(keyName);
            if (info != null)
            {
                return info.name;
            }
            return keyName;
        }

        public void SetStrValue(string keyName, string value)
        {
            var info = GetStrValueInfo(keyName);
            if (info != null)
            {
                strValues[info.index] = value;
            }
        }

        public bool HasStrValue(string customKey)
        {
            return GetStrValueInfoMap().ContainsKey(customKey);
        }

        public ValueData[] GetValueDataList(TangentValueType valueType)
        {
            switch (valueType)
            {
                case TangentValueType.X移動:
                    if (hasPosition)
                    {
                        return new ValueData[] { positionValues[0] };
                    }
                    break;
                case TangentValueType.Y移動:
                    if (hasPosition)
                    {
                        return new ValueData[] { positionValues[1] };
                    }
                    break;
                case TangentValueType.Z移動:
                    if (hasPosition)
                    {
                        return new ValueData[] { positionValues[2] };
                    }
                    break;
                case TangentValueType.移動:
                    if (hasPosition)
                    {
                        return positionValues;
                    }
                    break;
                case TangentValueType.X回転:
                    if (hasRotation)
                    {
                        return new ValueData[] { rotationValues[0] };
                    }
                    if (hasEulerAngles)
                    {
                        return new ValueData[] { eulerAnglesValues[0] };
                    }
                    break;
                case TangentValueType.Y回転:
                    if (hasRotation)
                    {
                        return new ValueData[] { rotationValues[1] };
                    }
                    if (hasEulerAngles)
                    {
                        return new ValueData[] { eulerAnglesValues[1] };
                    }
                    break;
                case TangentValueType.Z回転:
                    if (hasRotation)
                    {
                        return new ValueData[] { rotationValues[2] };
                    }
                    if (hasEulerAngles)
                    {
                        return new ValueData[] { eulerAnglesValues[2] };
                    }
                    break;
                case TangentValueType.W回転:
                    if (hasRotation)
                    {
                        return new ValueData[] { rotationValues[3] };
                    }
                    break;
                case TangentValueType.回転:
                    if (hasRotation)
                    {
                        return rotationValues;
                    }
                    if (hasEulerAngles)
                    {
                        return eulerAnglesValues;
                    }
                    break;
                case TangentValueType.X拡縮:
                    if (hasScale)
                    {
                        return new ValueData[] { scaleValues[0] };
                    }
                    break;
                case TangentValueType.Y拡縮:
                    if (hasScale)
                    {
                        return new ValueData[] { scaleValues[1] };
                    }
                    break;
                case TangentValueType.Z拡縮:
                    if (hasScale)
                    {
                        return new ValueData[] { scaleValues[2] };
                    }
                    break;
                case TangentValueType.拡縮:
                    if (hasScale)
                    {
                        return scaleValues;
                    }
                    break;
                case TangentValueType.すべて:
                    return baseValues;
            }

            return new ValueData[0];
        }

        public TangentData[] GetInTangentDataList(TangentValueType valueType)
        {
            var dataList = GetValueDataList(valueType);
            var result = new TangentData[dataList.Length];
            for (int i = 0; i < dataList.Length; i++)
            {
                result[i] = dataList[i].inTangent;
            }
            return result;
        }

        public TangentData[] GetOutTangentDataList(TangentValueType valueType)
        {
            var dataList = GetValueDataList(valueType);
            var result = new TangentData[dataList.Length];
            for (int i = 0; i < dataList.Length; i++)
            {
                result[i] = dataList[i].outTangent;
            }
            return result;
        }

        public virtual void Reset()
        {
            if (hasPosition)
            {
                position = initialPosition;
            }
            if (hasSubPosition)
            {
                subPosition = initialSubPosition;
            }
            if (hasRotation)
            {
                rotation = initialRotation;
            }
            if (hasSubRotation)
            {
                subRotation = initialSubRotation;
            }
            if (hasEulerAngles)
            {
                eulerAngles = initialEulerAngles;
            }
            if (hasSubEulerAngles)
            {
                subEulerAngles = initialSubEulerAngles;
            }
            if (hasScale)
            {
                scale = initialScale;
            }
            if (hasColor)
            {
                color = initialColor;
            }
            if (hasSubColor)
            {
                subColor = initialSubColor;
            }
            if (hasVisible)
            {
                visible = initialVisible;
            }
            if (hasEasing)
            {
                easing = 0;
            }

            foreach (var customKey in GetCustomValueInfoMap().Keys)
            {
                GetCustomValue(customKey).value = GetDefaultCustomValue(customKey);
            }

            foreach (var strKey in GetStrValueInfoMap().Keys)
            {
                SetStrValue(strKey, string.Empty);
            }
        }

        public override bool Equals(object obj)
        {
            var other = obj as TransformDataBase;
            if (other == null)
            {
                return false;
            }

            if (name != other.name)
            {
                return false;
            }

            for (int i = 0; i < values.Length; i++)
            {
                if (!values[i].Equals(other.values[i]))
                {
                    return false;
                }
            }

            for (int i = 0; i < strValues.Length; i++)
            {
                if (strValues[i] != other.strValues[i])
                {
                    return false;
                }
            }

            return true;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + name.GetHashCode();
                hash = hash * 23 + values.GetHashCode();
                hash = hash * 23 + strValues.GetHashCode();
                return hash;
            }
        }

        public override string ToString()
        {
            var ret = "TransformData";
            ret += string.Format(" name={0} ", name);
            if (valueCount > 0)
            {
                ret += " values=";
                ret += string.Join(", ", values.Select(value => value.value.ToString()).ToArray());
            }
            if (strValueCount > 0)
            {
                ret += " strValues=";
                ret += string.Join(", ", strValues.Select(value => value).ToArray());
            }
            return ret;
        }
    }
}