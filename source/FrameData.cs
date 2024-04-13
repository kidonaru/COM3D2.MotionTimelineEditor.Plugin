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
    using MTE = MotionTimelineEditor;

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
                return _boneMap.Count == Extensions.saveBonePaths.Length;
            }
        }

        private static MaidHackBase maidHack
        {
            get
            {
                return MTE.maidHack;
            }
        }

        public FrameData()
        {
            _boneMap = new Dictionary<string, BoneData>(Extensions.saveBonePaths.Length);
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

        public BoneData GetBone(IKManager.BoneType boneType)
        {
            return GetBone(BoneUtils.GetBonePath(boneType));
        }

        public bool HasBone(BoneData bone)
        {
            return bone != null && _boneMap.ContainsKey(bone.bonePath);
        }

        public bool HasAnyBones(IEnumerable<BoneData> bones)
        {
            return bones.Any(HasBone);
        }

        public BoneData GetOrCreateBone(string path)
        {
            BoneData bone;
            if (!_boneMap.TryGetValue(path, out bone))
            {
                bone = new BoneData(new TransformData(BoneUtils.ConvertBoneName(path)));
                bone.parentFrame = this;
                _boneMap[path] = bone;
            }
            return bone;
        }

        public BoneData GetOrCreateBone(IKManager.BoneType boneType)
        {
            return GetOrCreateBone(BoneUtils.GetBonePath(boneType));
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

            var targetBone = GetOrCreateBone(bone.bonePath);
            targetBone.transform.FromTransformData(bone.transform);
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
            foreach (var path in Extensions.saveBonePaths)
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

        public void SetCacheBoneDataArray(CacheBoneDataArray cacheBoneData)
        {
            var pathDic = cacheBoneData.GetPathDic();
            foreach (var path in Extensions.saveBonePaths)
            {
                CacheBoneDataArray.BoneData sourceBone;
                if (pathDic.TryGetValue(path, out sourceBone))
                {
                    if (sourceBone == null || sourceBone.transform == null)
                    {
                        Extensions.LogError("SetCacheBoneDataArray：ボーンがnullです Maidを読み込み直してください：" + path);
                        break;
                    }

                    var bone = new BoneData(sourceBone.transform);
                    UpdateBone(bone);
                }
            }
        }

        public void AddRootPosition(Vector3 position)
        {
            var rootBone = GetOrCreateBone(IKManager.BoneType.Root);
            rootBone.transform.localPosition += position;
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