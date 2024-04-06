using System.Xml.Serialization;
using UnityEngine;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    using MTE = MotionTimelineEditor;

    public class TransformData
    {
        [XmlIgnore]
        private string _name;
        [XmlElement("Name")]
        public string name
        {
            get
            {
                return _name;
            }
            set
            {
                _name = value;
                Initialize();
            }
        }

        [XmlIgnore]
        public Vector3 localPosition = Vector3.zero;
        [XmlIgnore]
        public Quaternion localRotation = Quaternion.identity;

        [XmlIgnore]
        public bool isBipRoot { get; private set; }
        [XmlIgnore]
        public bool isBustL { get; private set; }
        [XmlIgnore]
        public bool isBustR { get; private set; }
        [XmlIgnore]
        public bool isHead { get; private set; }

        [XmlIgnore]
        private float[] _values = new float[0];

        [XmlElement("Value")]
        public float[] values
        {
            get
            {
                _values[0] = localRotation.x;
                _values[1] = localRotation.y;
                _values[2] = localRotation.z;
                _values[3] = localRotation.w;

                if (isBipRoot)
                {
                    _values[4] = localPosition.x;
                    _values[5] = localPosition.y;
                    _values[6] = localPosition.z;
                }

                return _values;
            }
            set
            {
                localRotation = new Quaternion(value[0], value[1], value[2], value[3]);

                if (isBipRoot)
                {
                    localPosition = new Vector3(value[4], value[5], value[6]);
                }
            }
        }

        [XmlIgnore]
        public TangentData[] inTangentDataList = new TangentData[0];
        [XmlIgnore]
        public TangentData[] outTangentDataList = new TangentData[0];

        [XmlIgnore]
        private float[] _inTangents = new float[0];

        [XmlIgnore]
        public float[] inTangents
        {
            get
            {
                for (int i = 0; i < inTangentDataList.Length; i++)
                {
                    _inTangents[i] = inTangentDataList[i].value;
                }
                return _inTangents;
            }
        }

        [XmlArray("InTangents")]
        [XmlArrayItem("Value")]
        public float[] normalizedInTangents
        {
            get
            {
                if (!ShouldSerializeInTangents())
                {
                    return null;
                }
                var result = new float[inTangentDataList.Length];
                for (int i = 0; i < inTangentDataList.Length; i++)
                {
                    result[i] = inTangentDataList[i].normalizedValue;
                }
                return result;
            }
            set
            {
                for (int i = 0; i < inTangentDataList.Length; i++)
                {
                    if (value != null && i < value.Length)
                    {
                        inTangentDataList[i].normalizedValue = value[i];
                    }
                    else
                    {
                        inTangentDataList[i].normalizedValue = 0.0f;
                    }
                }
            }
        }

        public bool ShouldSerializeInTangents()
        {
            foreach (var tangent in inTangentDataList)
            {
                if (!tangent.isSmooth && tangent.normalizedValue != 0.0f)
                {
                    return true;
                }
            }
            return false;
        }

        [XmlIgnore]
        private float[] _outTangents = new float[0];

        [XmlIgnore]
        public float[] outTangents
        {
            get
            {
                for (int i = 0; i < outTangentDataList.Length; i++)
                {
                    _outTangents[i] = outTangentDataList[i].value;
                }
                return _outTangents;
            }
        }

        [XmlArray("OutTangents")]
        [XmlArrayItem("Value")]
        public float[] normalizedOutTangents
        {
            get
            {
                if (!ShouldSerializeOutTangents())
                {
                    return null;
                }
                var result = new float[outTangentDataList.Length];
                for (int i = 0; i < outTangentDataList.Length; i++)
                {
                    result[i] = outTangentDataList[i].normalizedValue;
                }
                return result;
            }
            set
            {
                for (int i = 0; i < outTangentDataList.Length; i++)
                {
                    if (value != null && i < value.Length)
                    {
                        outTangentDataList[i].normalizedValue = value[i];
                    }
                    else
                    {
                        outTangentDataList[i].normalizedValue = 0.0f;
                    }
                }
            }
        }

        public bool ShouldSerializeOutTangents()
        {
            foreach (var tangent in outTangentDataList)
            {
                if (!tangent.isSmooth && tangent.normalizedValue != 0.0f)
                {
                    return true;
                }
            }
            return false;
        }

        [XmlElement("InSmoothBit")]
        public int inSmoothBit
        {
            get
            {
                var result = 0;
                for (int i = 0; i < inTangentDataList.Length; i++)
                {
                    int value = inTangentDataList[i].isSmooth ? 1 : 0;
                    result |= value << i;
                }
                return result;
            }
            set
            {
                for (int i = 0; i < inTangentDataList.Length; i++)
                {
                    inTangentDataList[i].isSmooth = (value & (1 << i)) != 0;
                }
            }
        }

        [XmlElement("OutSmoothBit")]
        public int outSmoothBit
        {
            get
            {
                var result = 0;
                for (int i = 0; i < outTangentDataList.Length; i++)
                {
                    int value = outTangentDataList[i].isSmooth ? 1 : 0;
                    result |= value << i;
                }
                return result;
            }
            set
            {
                for (int i = 0; i < outTangentDataList.Length; i++)
                {
                    outTangentDataList[i].isSmooth = (value & (1 << i)) != 0;
                }
            }
        }

        public bool isBust
        {
            get
            {
                return isBustL || isBustR;
            }
        }

        public int valueCount
        {
            get
            {
                return isBipRoot ? 7 : 4;
            }
        }

        private static Config config
        {
            get
            {
                return MTE.config;
            }
        }

        private static TimelineManager timelineManager
        {
            get
            {
                return TimelineManager.instance;
            }
        }

        private static TimelineData timeline
        {
            get
            {
                return timelineManager.timeline;
            }
        }

        public TransformData() : this("")
        {
        }

        public TransformData(string name)
        {
            this.name = name;
        }

        public TransformData(Transform transform) : this(transform.name)
        {
            FromTransform(transform);
        }

        public TransformData(TransformData transform) : this(transform.name)
        {
            FromTransformData(transform);
        }

        private void Initialize()
        {
            isBipRoot = name == "Bip01";
            isBustL = name == "Mune_L";
            isBustR = name == "Mune_R";
            isHead = name == "Bip01 Head";

            var length = valueCount;
            if (_values.Length != length)
            {
                _values = new float[length];
                _inTangents = new float[length];
                _outTangents = new float[length];
                inTangentDataList = new TangentData[length];
                outTangentDataList = new TangentData[length];

                var tangentPair = config.defaultTangentPair;

                for (int i = 0; i < length; i++)
                {
                    inTangentDataList[i] = new TangentData
                    {
                        normalizedValue = tangentPair.inTangent,
                        isSmooth = tangentPair.isSmooth,
                    };

                    outTangentDataList[i] = new TangentData
                    {
                        normalizedValue = tangentPair.outTangent,
                        isSmooth = tangentPair.isSmooth,
                    };
                }
            }
        }

        /// <summary>
        /// 最短の補間経路に修正
        /// </summary>
        /// <param name="prevBone"></param>
        public void FixRotation(TransformData prevTrans)
        {
            if (prevTrans == null || prevTrans == this)
            {
                return;
            }

            var prevRot = prevTrans.localRotation;
            var rot = this.localRotation;
            var dot = Quaternion.Dot(prevRot, rot);

            if (dot < 0.0f)
            {
                this.localRotation = new Quaternion(-rot.x, -rot.y, -rot.z, -rot.w);
            }
        }

        public void UpdateTangent(
            TransformData prevTransform,
            TransformData nextTransform,
            float prevTime,
            float currentTime,
            float nextTime)
        {
            if (prevTransform == null || nextTransform == null)
            {
                Extensions.LogError("UpdateTangent：前後のTransformが見つかりません。");
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
                Extensions.LogError("UpdateTangent：フレーム時間が不正です。");
                return;
            }

            float dt0_inv = 1f / dt0;
            float dt1_inv = 1f / dt1;
            float dt_inv = 1f / dt;

            for (int i = 0; i < currentValues.Length; i++)
            {
                var inTangent = inTangentDataList[i];
                var outTangent = outTangentDataList[i];

                float x0 = prevValues[i];
                float x1 = currentValues[i];
                float x2 = nextValues[i];
                float dx0 = x1 - x0;
                float dx1 = x2 - x1;
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
                        inTangent.normalizedValue = timeline.ClampTangent(tan0);
                    }
                    if (outTangent.isSmooth)
                    {
                        outTangent.normalizedValue = timeline.ClampTangent(tan1);
                    }
                }

                inTangent.UpdateValue(v0);
                outTangent.UpdateValue(v1);
            }
        }

        /// <summary>
        /// InTangentのみ更新 (最終フレーム用)
        /// </summary>
        /// <param name="prevTransform"></param>
        /// <param name="prevTime"></param>
        /// <param name="currentTime"></param>
        public void UpdateInTangent(
            TransformData prevTransform,
            float prevTime,
            float currentTime)
        {
            if (prevTransform == null)
            {
                Extensions.LogError("UpdateTangent：前のTransformが見つかりません。");
                return;
            }

            var prevValues = prevTransform.values;
            var currentValues = this.values;
            float dt0 = currentTime - prevTime;

            if (dt0 <= 0f)
            {
                Extensions.LogError("UpdateTangent：フレーム時間が不正です。");
                return;
            }

            float dt0_inv = 1f / dt0;

            for (int i = 0; i < currentValues.Length; i++)
            {
                var inTangent = inTangentDataList[i];

                float x0 = prevValues[i];
                float x1 = currentValues[i];
                float dx0 = x1 - x0;
                float v0 = dx0 * dt0_inv;

                inTangent.UpdateValue(v0);
            }
        }

        public void FromTransform(Transform transform)
        {
            localRotation = transform.localRotation;
            if (isBipRoot)
            {
                localPosition = transform.localPosition;
            }
        }

        public void FromTransformData(TransformData transform)
        {
            localRotation = transform.localRotation;
            if (isBipRoot)
            {
                localPosition = transform.localPosition;
            }

            for (int i = 0; i < valueCount; i++)
            {
                inTangentDataList[i].FromTangentData(transform.inTangentDataList[i]);
                outTangentDataList[i].FromTangentData(transform.outTangentDataList[i]);
            }
        }

        public TangentData[] GetInTangentDataList(TangentValueType valueType)
        {
            switch (valueType)
            {
                case TangentValueType.X:
                    if (isBipRoot)
                    {
                        return new TangentData[] { inTangentDataList[4] };
                    }
                    return new TangentData[0];
                case TangentValueType.Y:
                    if (isBipRoot)
                    {
                        return new TangentData[] { inTangentDataList[5] };
                    }
                    return new TangentData[0];
                case TangentValueType.Z:
                    if (isBipRoot)
                    {
                        return new TangentData[] { inTangentDataList[6] };
                    }
                    return new TangentData[0];
                case TangentValueType.RX:
                    return new TangentData[] { inTangentDataList[0] };
                case TangentValueType.RY:
                    return new TangentData[] { inTangentDataList[1] };
                case TangentValueType.RZ:
                    return new TangentData[] { inTangentDataList[2] };
                case TangentValueType.RW:
                    return new TangentData[] { inTangentDataList[3] };
                case TangentValueType.Move:
                    if (isBipRoot)
                    {
                        return new TangentData[] { inTangentDataList[4], inTangentDataList[5], inTangentDataList[6] };
                    }
                    return new TangentData[0];
                case TangentValueType.Rotation:
                    return new TangentData[] { inTangentDataList[0], inTangentDataList[1], inTangentDataList[2], inTangentDataList[3] };
                case TangentValueType.All:
                    return inTangentDataList;
            }
            return new TangentData[0];
        }

        public TangentData[] GetOutTangentDataList(TangentValueType valueType)
        {
            switch (valueType)
            {
                case TangentValueType.X:
                    if (isBipRoot)
                    {
                        return new TangentData[] { outTangentDataList[4] };
                    }
                    return new TangentData[0];
                case TangentValueType.Y:
                    if (isBipRoot)
                    {
                        return new TangentData[] { outTangentDataList[5] };
                    }
                    return new TangentData[0];
                case TangentValueType.Z:
                    if (isBipRoot)
                    {
                        return new TangentData[] { outTangentDataList[6] };
                    }
                    return new TangentData[0];
                case TangentValueType.RX:
                    return new TangentData[] { outTangentDataList[0] };
                case TangentValueType.RY:
                    return new TangentData[] { outTangentDataList[1] };
                case TangentValueType.RZ:
                    return new TangentData[] { outTangentDataList[2] };
                case TangentValueType.RW:
                    return new TangentData[] { outTangentDataList[3] };
                case TangentValueType.Move:
                    if (isBipRoot)
                    {
                        return new TangentData[] { outTangentDataList[4], outTangentDataList[5], outTangentDataList[6] };
                    }
                    return new TangentData[0];
                case TangentValueType.Rotation:
                    return new TangentData[] { outTangentDataList[0], outTangentDataList[1], outTangentDataList[2], outTangentDataList[3] };
                case TangentValueType.All:
                    return outTangentDataList;
            }
            return new TangentData[0];
        }

        public void Reset()
        {
            var boneType = BoneUtils.GetBoneTypeByName(name);
            var initialRotation = BoneUtils.GetInitialRotation(boneType);
            localRotation = Quaternion.Euler(initialRotation);

            if (isBipRoot)
            {
                localPosition = new Vector3(0f, 0.9f, 0f);
            }
        }

        public override bool Equals(object obj)
        {
            var other = obj as TransformData;
            if (other == null)
            {
                return false;
            }

            if (name != other.name)
            {
                return false;
            }

            if (!localRotation.Equals(other.localRotation))
            {
                return false;
            }

            if (isBipRoot && !localPosition.Equals(other.localPosition))
            {
                return false;
            }

            if (inTangentDataList.Length != other.inTangentDataList.Length)
            {
                return false;
            }

            for (int i = 0; i < inTangentDataList.Length; i++)
            {
                if (inTangentDataList[i].value != other.inTangentDataList[i].value)
                {
                    return false;
                }
            }

            if (outTangentDataList.Length != other.outTangentDataList.Length)
            {
                return false;
            }

            for (int i = 0; i < outTangentDataList.Length; i++)
            {
                if (outTangentDataList[i].value != other.outTangentDataList[i].value)
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
                hash = hash * 23 + localPosition.GetHashCode();
                hash = hash * 23 + localRotation.GetHashCode();
                hash = hash * 23 + inTangents.GetHashCode();
                hash = hash * 23 + outTangents.GetHashCode();
                return hash;
            }
        }
    }
}