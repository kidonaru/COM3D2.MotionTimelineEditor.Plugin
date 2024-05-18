using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    using MTE = MotionTimelineEditor;

    public abstract class TransformDataBase : ITransformData
    {
        public string name { get; protected set; }

        public abstract int valueCount { get; }

        private ValueData[] _values = new ValueData[0];
        public ValueData[] values
        {
            get
            {
                return _values;
            }
        }

        public Vector3 position
        {
            get
            {
                return GetPositionValues().ToVector3();
            }
            set
            {
                GetPositionValues().FromVector3(value);
            }
        }

        public Quaternion rotation
        {
            get
            {
                return GetRotationValues().ToQuaternion();
            }
            set
            {
                GetRotationValues().FromQuaternion(value);
            }
        }

        public Vector3 eulerAngles
        {
            get
            {
                if (hasEulerAngles)
                {
                    return GetEulerAnglesValues().ToVector3();
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
                    GetEulerAnglesValues().FromVector3(value);
                }
                else if (hasRotation)
                {
                    rotation = Quaternion.Euler(value);
                }
            }
        }

        public Vector3 scale
        {
            get
            {
                return GetScaleValues().ToVector3();
            }
            set
            {
                GetScaleValues().FromVector3(value);
            }
        }

        public int easing
        {
            get
            {
                return GetEasingValue().intValue;
            }
            set
            {
                GetEasingValue().intValue = value;
            }
        }

        public abstract bool hasPosition { get; }
        public abstract bool hasRotation { get; }
        public abstract bool hasEulerAngles { get; }
        public abstract bool hasScale { get; }
        public abstract bool hasEasing { get; }
        public abstract bool hasTangent { get; }

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

        protected static Config config
        {
            get
            {
                return MTE.config;
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
            var diff = euler - prevEuler;

            for (int i = 0; i < 3; i++)
            {
                int iDiff = (int) diff[i];
                if (iDiff > 180)
                {
                    euler[i] -= (iDiff + 180) / 360 * 360;
                }
                else if (iDiff < -180)
                {
                    euler[i] += (iDiff + 180) / 360 * 360;
                }
            }

            this.eulerAngles = euler;
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

            var prevValues = prevTransform.values;
            var currentValues = this.values;
            var nextValues = nextTransform.values;
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

        public void FromXml(TransformXml xml)
        {
            name = xml.name;
            _valuesForXml = xml.values;
            _normalizedInTangents = xml.inTangents;
            _normalizedOutTangents = xml.outTangents;
            _inSmoothBit = xml.inSmoothBit;
            _outSmoothBit = xml.outSmoothBit;
        }

        public TransformXml ToXml()
        {
            var xml = new TransformXml();
            xml.name = name;
            xml.values = _valuesForXml;
            xml.inTangents = _normalizedInTangents;
            xml.outTangents = _normalizedOutTangents;
            xml.inSmoothBit = _inSmoothBit;
            xml.outSmoothBit = _outSmoothBit;
            return xml;
        }

        public abstract ValueData[] GetPositionValues();
        public abstract ValueData[] GetRotationValues();
        public abstract ValueData[] GetEulerAnglesValues();
        public abstract ValueData[] GetScaleValues();
        public abstract ValueData GetEasingValue();

        public virtual Dictionary<string, int> GetCustomValueIndexMap()
        {
            return new Dictionary<string, int>();
        }

        public ValueData GetCustomValue(string customName)
        {
            int index;
            if (GetCustomValueIndexMap().TryGetValue(customName, out index))
            {
                return values[index];
            }

            PluginUtils.LogError("CustomValueが見つかりません customName={0}", customName);
            return new ValueData();
        }

        public bool HasCustomValue(string customName)
        {
            return GetCustomValueIndexMap().ContainsKey(customName);
        }

        public virtual Vector3 GetInitialPosition()
        {
            return Vector3.zero;
        }

        public virtual Quaternion GetInitialRotation()
        {
            return Quaternion.identity;
        }

        public virtual Vector3 GetInitialEulerAngles()
        {
            return Vector3.zero;
        }

        public virtual Vector3 GetInitialScale()
        {
            return Vector3.one;
        }

        public virtual float GetInitialCustomValue(string customName)
        {
            return 0f;
        }

        public ValueData[] GetValueDataList(TangentValueType valueType)
        {
            switch (valueType)
            {
                case TangentValueType.X:
                    if (hasPosition)
                    {
                        return new ValueData[] { GetPositionValues()[0] };
                    }
                    break;
                case TangentValueType.Y:
                    if (hasPosition)
                    {
                        return new ValueData[] { GetPositionValues()[1] };
                    }
                    break;
                case TangentValueType.Z:
                    if (hasPosition)
                    {
                        return new ValueData[] { GetPositionValues()[2] };
                    }
                    break;
                case TangentValueType.Move:
                    if (hasPosition)
                    {
                        return GetPositionValues();
                    }
                    break;
                case TangentValueType.RX:
                    if (hasRotation)
                    {
                        return new ValueData[] { GetRotationValues()[0] };
                    }
                    break;
                case TangentValueType.RY:
                    if (hasRotation)
                    {
                        return new ValueData[] { GetRotationValues()[1] };
                    }
                    break;
                case TangentValueType.RZ:
                    if (hasRotation)
                    {
                        return new ValueData[] { GetRotationValues()[2] };
                    }
                    break;
                case TangentValueType.RW:
                    if (hasRotation)
                    {
                        return new ValueData[] { GetRotationValues()[3] };
                    }
                    break;
                case TangentValueType.Rotation:
                    if (hasRotation)
                    {
                        return GetRotationValues();
                    }
                    break;
                case TangentValueType.All:
                    return values;
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
                position = GetInitialPosition();
            }
            if (hasRotation)
            {
                rotation = GetInitialRotation();
            }
            if (hasEulerAngles)
            {
                eulerAngles = GetInitialEulerAngles();
            }
            if (hasScale)
            {
                scale = GetInitialScale();
            }
            if (hasEasing)
            {
                easing = 0;
            }

            foreach (var customName in GetCustomValueIndexMap().Keys)
            {
                GetCustomValue(customName).value = GetInitialCustomValue(customName);
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

            return true;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + name.GetHashCode();
                hash = hash * 23 + values.GetHashCode();
                return hash;
            }
        }
    }
}