using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Xml.Serialization;
using UnityEngine;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    using SH = StudioHack;

    public enum TangentValueType
    {
        X,
        Y,
        Z,
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
        Max,
    }

    public class TangentData
    {
        private float _value;
        private float _normalizedValue;

        public static readonly string[] tangentValueTypeNames = new string[] {
            "X移動", "Y移動", "Z移動", "回転", "すべて" };

        public static readonly string[] tangentTypeNames = new string[] {
            "EaseInOut", "EaseIn", "EaseOut", "線形補完" };

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
                if (_normalizedValue == value)
                {
                    return;
                }

                _value = _normalizedValue = value;
            }
        }

        public bool isZero
        {
            get
            {
                return _normalizedValue == 0f;
            }
        }

        public void UpdateDiff(
            float diffTime,
            float diffValue)
        {
            if (isZero || diffValue == 0f || diffTime == 0f)
            {
                _value = 0f;
                return;
            }
            _value = _normalizedValue * diffValue / diffTime;
        }
    }

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

        [XmlElement("InTangent")]
        public float[] normalizedInTangents
        {
            get
            {
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

        [XmlElement("OutTangent")]
        public float[] normalizedOutTangents
        {
            get
            {
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

        public TransformData() : this("")
        {
        }

        public TransformData(string name)
        {
            this.name = name;
        }

        public TransformData(Transform transform)
        {
            this.name = transform.name;
            FromTransform(transform);
        }

        public TransformData(TransformData transform)
        {
            this.name = transform.name;
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

                for (int i = 0; i < length; i++)
                {
                    inTangentDataList[i] = new TangentData();
                    outTangentDataList[i] = new TangentData();
                }
            }
        }

        public bool NeedsTangentUpdate()
        {
            for (int i = 0; i < inTangentDataList.Length; i++)
            {
                if (!inTangentDataList[i].isZero)
                {
                    return true;
                }
            }

            for (int i = 0; i < outTangentDataList.Length; i++)
            {
                if (!outTangentDataList[i].isZero)
                {
                    return true;
                }
            }

            return false;
        }

        public void UpdateTangent(
            TransformData prevTransform,
            TransformData nextTransform,
            float currentTime,
            float prevTime,
            float nextTime)
        {
            var values = this.values;
            if (prevTransform != null)
            {
                var prevValues = prevTransform.values;
                var diffTime = prevTime - currentTime;

                for (int i = 0; i < inTangentDataList.Length; i++)
                {
                    var diffValue = prevValues[i] - values[i];
                    inTangentDataList[i].UpdateDiff(diffTime, diffValue);
                }
            }

            if (nextTransform != null)
            {
                var nextValues = nextTransform.values;
                var diffTime = nextTime - currentTime;

                for (int i = 0; i < outTangentDataList.Length; i++)
                {
                    var diffValue = nextValues[i] - values[i];
                    outTangentDataList[i].UpdateDiff(diffTime, diffValue);
                }
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
                case TangentValueType.Rotation:
                    return new TangentData[] { outTangentDataList[0], outTangentDataList[1], outTangentDataList[2], outTangentDataList[3] };
                case TangentValueType.All:
                    return outTangentDataList;
            }
            return new TangentData[0];
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

    public class BoneData
    {
        [XmlElement("Transform")]
        public TransformData transform;

        [XmlIgnore]
        public FrameData parentFrame = null;

        public string bonePath
        {
            get
            {
                return BoneUtils.ConvertBonePath(transform.name);
            }
        }

        public IKManager.BoneType boneType
        {
            get
            {
                return BoneUtils.GetBoneTypeByName(transform.name);
            }
        }

        public int frameNo
        {
            get
            {
                return parentFrame != null ? parentFrame.frameNo : -1;
            }
        }

        public BoneData()
        {
        }

        public BoneData(TransformData trans)
        {
            this.transform = trans;
        }

        public BoneData(Transform trans) : this(new TransformData(trans))
        {
        }

        /// <summary>
        /// 最短の補間経路に修正
        /// </summary>
        /// <param name="prevBone"></param>
        public void FixRotation(BoneData prevBone)
        {
            if (prevBone == null || prevBone == this)
            {
                return;
            }

            var prevRot = prevBone.transform.localRotation;
            var rot = this.transform.localRotation;
            var dot = Quaternion.Dot(prevRot, rot);

            if (dot < 0.0f)
            {
                this.transform.localRotation = new Quaternion(-rot.x, -rot.y, -rot.z, -rot.w);
            }
        }
    }

    public class FrameData
    {
        [XmlElement("FrameNo")]
        public int frameNo = 0;

        [XmlElement("Bone")]
        public BoneData[] _bones
        {
            get
            {
                return bones.ToArray();
            }
            set
            {
                ClearBones();
                foreach (var bone in value)
                {
                    _boneMap[bone.bonePath] = bone;
                    bone.parentFrame = this;
                }
            }
        }

        [XmlIgnore]
        public Dictionary<string, BoneData>.ValueCollection bones
        {
            get
            {
                return _boneMap.Values;
            }
        }

        [XmlIgnore]
        private Dictionary<string, BoneData> _boneMap = null;

        public bool isFullBone
        {
            get
            {
                return _boneMap.Count == SH.saveBonePaths.Length;
            }
        }

        public FrameData()
        {
            _boneMap = new Dictionary<string, BoneData>(SH.saveBonePaths.Length);
        }

        public FrameData(int frameNo) : this()
        {
            this.frameNo = frameNo;
        }

        public BoneData GetBone(string path)
        {
            BoneData bone;
            if (_boneMap.TryGetValue(path, out bone))
            {
                return bone;
            }
            return null;
        }

        public bool HasBone(BoneData bone)
        {
            return bone != null && _boneMap.ContainsKey(bone.bonePath);
        }

        public bool HasAnyBones(IEnumerable<BoneData> bones)
        {
            return bones.Any(HasBone);
        }

        public void SetBone(BoneData bone)
        {
            if (bone == null)
            {
                return;
            }

            var path = bone.bonePath;
            bone.parentFrame = this;
            _boneMap[path] = bone;
        }

        public void SetBones(IEnumerable<BoneData> bones)
        {
            foreach (var bone in bones)
            {
                SetBone(bone);
            }
        }

        public void UpdateBone(BoneData bone)
        {
            if (bone == null)
            {
                return;
            }

            var path = bone.bonePath;

            BoneData targetBone;
            if (_boneMap.TryGetValue(path, out targetBone))
            {
                targetBone.transform.FromTransformData(bone.transform);
                return;
            }

            targetBone = new BoneData(bone.transform);
            targetBone.parentFrame = this;
            _boneMap[path] = targetBone;
        }

        public void UpdateBones(IEnumerable<BoneData> bones)
        {
            foreach (var bone in bones)
            {
                UpdateBone(bone);
            }
        }

        public void RemoveBone(BoneData bone)
        {
            if (bone != null)
            {
                _boneMap.Remove(bone.bonePath);
                bone.parentFrame = null;
            }
        }

        public void RemoveBones(IEnumerable<BoneData> bones)
        {
            foreach (var bone in bones)
            {
                RemoveBone(bone);
            }
        }

        public void ClearBones()
        {
            _boneMap.Clear();
        }

        public BoneData FindBone(Func<BoneData, bool> match)
        {
            return bones.FirstOrDefault(match);
        }

        public bool HasBones()
        {
            return _boneMap.Count > 0;
        }

        public List<BoneData> GetDiffBones(
            FrameData sourceFrame,
            bool useBustKeyL,
            bool useBustKeyR)
        {
            var diffBones = new List<BoneData>(_boneMap.Count);
            foreach (var pair in _boneMap)
            {
                var path = pair.Key;
                var bone = pair.Value;

                if (bone.transform.isHead)
                {
                    continue;
                }
                if (bone.transform.isBustL && !useBustKeyL)
                {
                    continue;
                }
                if (bone.transform.isBustR && !useBustKeyR)
                {
                    continue;
                }

                var sourceBone = sourceFrame.GetBone(path);
                if (sourceBone == null || !bone.transform.Equals(sourceBone.transform))
                {
                    diffBones.Add(bone);
                }
            }
            return diffBones;
        }

        public byte[] GetAnmBinary(bool use_bust_keyL, bool use_bust_keyR)
        {
            Action<BinaryWriter, BoneData> write_bone_data = delegate (BinaryWriter w, BoneData bone)
            {
                int num = 2;
                w.Write((byte)1);
                w.Write(bone.bonePath);
                float[] values = bone.transform.values;

                for (int i = 0; i < values.Length; i++)
                {
                    w.Write((byte)(100 + i));
                    w.Write(num);
                    for (int j = 0; j < num; j++)
                    {
                        w.Write((float)j);
                        w.Write(values[i]);
                        w.Write(0);
                        w.Write(0);
                    }
                }
            };
            MemoryStream memoryStream = new MemoryStream();
            BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
            binaryWriter.Write("CM3D2_ANIM");
            binaryWriter.Write(1001);
            foreach (var path in SH.saveBonePaths)
            {
                var bone = GetBone(path);
                if (bone == null)
                {
                    Extensions.LogError("ボーンがないのでスキップしました：" + path);
                    continue;
                }
                write_bone_data(binaryWriter, bone);
            }
            binaryWriter.Write((byte)0);
            binaryWriter.Write((byte)(use_bust_keyL ? 1u : 0u));
            binaryWriter.Write((byte)(use_bust_keyR ? 1u : 0u));
            binaryWriter.Close();
            memoryStream.Close();
            byte[] result = memoryStream.ToArray();
            memoryStream.Dispose();
            return result;
        }

        public KeyValuePair<bool, bool> SetAnmBinary(byte[] binary)
        {
            MemoryStream memoryStream = new MemoryStream(binary);
            BinaryReader binaryReader = new BinaryReader(memoryStream);
            string header = binaryReader.ReadString();
            if (header != "CM3D2_ANIM")
            {
                binaryReader.Close();
                memoryStream.Close();
                memoryStream.Dispose();
                Extensions.LogError("SetAnmBinary：ヘッダが不正です。");
                return new KeyValuePair<bool, bool>(false, false);
            }

            int version = binaryReader.ReadInt32();
            bool hasBustKeyInfo = version >= 1001;

            var bustData = new Dictionary<int, KeyValuePair<TransformData, float[]>>();
            while (binaryReader.ReadByte() != 0)
            {
                var path = binaryReader.ReadString();
                var boneName = BoneUtils.ConvertBoneName(path);
                var transform = new TransformData(boneName);
                BoneData boneData = new BoneData(transform);
                float[] values = new float[boneData != null && boneData.transform.isBipRoot ? 7 : 4];

                for (int i = 0; i < values.Length; i++)
                {
                    byte frameInfo = binaryReader.ReadByte();
                    int numFrames = binaryReader.ReadInt32();
                    for (int j = 0; j < numFrames; j++)
                    {
                        binaryReader.ReadSingle();
                        var value = binaryReader.ReadSingle();
                        if (j == 0)
                        {
                            values[i] = value;
                        }
                        binaryReader.ReadInt32();
                        binaryReader.ReadInt32();
                    }
                }

                if (hasBustKeyInfo && boneData.transform.isBust)
                {
                    int key = boneData.transform.isBustR ? 1 : 0;
                    bustData.Add(key, new KeyValuePair<TransformData, float[]>(boneData.transform, values));
                }
                else
                {
                    boneData.transform.values = values;
                }

                SetBone(boneData);
            }

            bool useBustKeyL = false;
            bool useBustKeyR = false;
            if (hasBustKeyInfo)
            {
                useBustKeyL = binaryReader.ReadByte() != 0;
                useBustKeyR = binaryReader.ReadByte() != 0;
                var list = new List<KeyValuePair<TransformData, float[]>>();
                if (useBustKeyL)
                {
                    list.Add(bustData[0]);
                }

                if (useBustKeyR)
                {
                    list.Add(bustData[1]);
                }

                foreach (KeyValuePair<TransformData, float[]> item in list)
                {
                    item.Key.values = item.Value;
                }
            }

            binaryReader.Close();
            memoryStream.Close();
            memoryStream.Dispose();
            return new KeyValuePair<bool, bool>(useBustKeyL, useBustKeyR);
        }

        public void SetCacheBoneDataArray(CacheBoneDataArray dataArray)
        {
            var pathDic = SH.GetBonePathDic(dataArray);
            foreach (var path in SH.saveBonePaths)
            {
                CacheBoneDataArray.BoneData sourceBone;
                if (pathDic.TryGetValue(path, out sourceBone))
                {
                    var bone = new BoneData(sourceBone.transform);
                    SetBone(bone);
                }
            }
        }

        private static bool initializedBoneTypes = false;
        private static HashSet<IKManager.BoneType> notFlipTypes = null;
        private static Dictionary<IKManager.BoneType, IKManager.BoneType> swapFlipDic = null;

        private static List<IKManager.BoneType> leftFingerTypes = null;
        private static List<IKManager.BoneType> rightFingerTypes = null;
        private static List<IKManager.BoneType> leftToeTypes = null;
        private static List<IKManager.BoneType> rightToeTypes = null;

        private void InitBoneTypes()
        {
            if (initializedBoneTypes)
            {
                return;
            }
            initializedBoneTypes = true;

            Extensions.LogDebug("ボーンタイプの初期化");

            notFlipTypes = new HashSet<IKManager.BoneType>
            {
                IKManager.BoneType.TopFixed,
                IKManager.BoneType.Pelvis,
                IKManager.BoneType.Head,
                IKManager.BoneType.Bust_R,
                IKManager.BoneType.Bust_L,
            };
            for (int i = (int) IKManager.BoneType.Mouth; i <= (int) IKManager.BoneType.Nipple_R; i++)
            {
                notFlipTypes.Add((IKManager.BoneType)i);
            }

            leftFingerTypes = new List<IKManager.BoneType>(16);
            rightFingerTypes = new List<IKManager.BoneType>(16);
            leftToeTypes = new List<IKManager.BoneType>(6);
            rightToeTypes = new List<IKManager.BoneType>(6);

            for (int i = (int) IKManager.BoneType.Finger0_Root_L; i <= (int) IKManager.BoneType.Finger4_1_L; i++)
            {
                leftFingerTypes.Add((IKManager.BoneType)i);
            }

            for (int i = (int) IKManager.BoneType.Finger0_Root_R; i <= (int) IKManager.BoneType.Finger4_1_R; i++)
            {
                rightFingerTypes.Add((IKManager.BoneType)i);
            }
            
            for (int i = (int) IKManager.BoneType.Toe0_Root_L; i <= (int) IKManager.BoneType.Toe2_0_L; i++)
            {
                leftToeTypes.Add((IKManager.BoneType)i);
            }

            for (int i = (int) IKManager.BoneType.Toe0_Root_R; i <= (int) IKManager.BoneType.Toe2_0_R; i++)
            {
                rightToeTypes.Add((IKManager.BoneType)i);
            }

            swapFlipDic = new Dictionary<IKManager.BoneType, IKManager.BoneType>
            {
                { IKManager.BoneType.Clavicle_R, IKManager.BoneType.Clavicle_L },
                { IKManager.BoneType.UpperArm_R, IKManager.BoneType.UpperArm_L },
                { IKManager.BoneType.Forearm_R, IKManager.BoneType.Forearm_L },
                { IKManager.BoneType.Thigh_R, IKManager.BoneType.Thigh_L },
                { IKManager.BoneType.Calf_R, IKManager.BoneType.Calf_L },
                { IKManager.BoneType.Hand_R, IKManager.BoneType.Hand_L },
                { IKManager.BoneType.Foot_R, IKManager.BoneType.Foot_L }
            };

            var swapList = swapFlipDic.ToList();
            foreach (var pair in swapList)
            {
                swapFlipDic.Add(pair.Value, pair.Key);
            }

            for (int i = 0; i < leftFingerTypes.Count; i++)
            {
                swapFlipDic.Add(leftFingerTypes[i], rightFingerTypes[i]);
                swapFlipDic.Add(rightFingerTypes[i], leftFingerTypes[i]);
            }

            for (int i = 0; i < leftToeTypes.Count; i++)
            {
                swapFlipDic.Add(leftToeTypes[i], rightToeTypes[i]);
                swapFlipDic.Add(rightToeTypes[i], leftToeTypes[i]);
            }
        }

        public void Flip()
        {
            InitBoneTypes();

            var bones = this.bones;
            var transformMap = new Dictionary<IKManager.BoneType, TransformData>(bones.Count);
            foreach (var bone in bones)
            {
                var boneType = bone.boneType;
                if (!notFlipTypes.Contains(boneType))
                {
                    var transform = bone.transform;
                    transformMap.Add(boneType, transform);
                }
            }

            var newBones = new List<BoneData>(bones.Count);
            foreach (var bone in bones)
            {
                var boneType = bone.boneType;
                if (notFlipTypes.Contains(boneType))
                {
                    newBones.Add(bone);
                }
                else
                {
                    var transform = bone.transform;
                    var localRotation = transform.localRotation;
                    var eulerAngles = localRotation.eulerAngles;
                    var newEulerAngles = eulerAngles;

                    if (swapFlipDic.ContainsKey(boneType))
                    {
                        boneType = swapFlipDic[boneType];
                    }

                    if (boneType == IKManager.BoneType.Root)
                    {
                        newEulerAngles.y = 180f - (eulerAngles.y - 180f);
                        newEulerAngles.z = 270f - (eulerAngles.z - 270f);
                    }
                    else if (boneType == IKManager.BoneType.Spine0)
                    {
                        newEulerAngles.x = 270f - (eulerAngles.x - 270f);
                        //newEulerAngles.z = 90f - (eulerAngles.z - 90f);
                    }
                    else
                    {
                        newEulerAngles.x = -eulerAngles.x;
                        newEulerAngles.y = -eulerAngles.y;
                    }

                    Extensions.LogDebug("Flip Bone：" + boneType + " " + eulerAngles + " -> " + newEulerAngles);

                    var newTransform = new TransformData(BoneUtils.GetBoneName(boneType));
                    newTransform.localRotation = Quaternion.Euler(newEulerAngles);

                    if (boneType == IKManager.BoneType.Root)
                    {
                        var localPosition = transform.localPosition;
                        localPosition.x = -localPosition.x;
                        newTransform.localPosition = localPosition;
                    }

                    var newBone = new BoneData(newTransform);
                    newBones.Add(newBone);
                }
            }

            ClearBones();
            SetBones(newBones);
        }
    }
}