using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    public class FrameData
    {
        public ITimelineLayer parentLayer { get; set; }

        public int frameNo { get; set; }

        public Dictionary<string, BoneData>.ValueCollection bones
        {
            get => _boneMap.Values;
        }

        public Dictionary<string, BoneData>.KeyCollection boneNames
        {
            get => _boneMap.Keys;
        }

        private Dictionary<string, BoneData> _boneMap = null;

        public bool isFullBone
        {
            get => _boneMap.Count == parentLayer.allBoneNames.Count;
        }

        private static TimelineManager timelineManager => TimelineManager.instance;

        public FrameData(ITimelineLayer parentLayer)
        {
            this.parentLayer = parentLayer;
            _boneMap = new Dictionary<string, BoneData>(BoneUtils.saveBoneNames.Count);
        }

        public FrameData(ITimelineLayer parentLayer, int frameNo) : this(parentLayer)
        {
            this.frameNo = frameNo;
        }

        public BoneData CreateBone(ITransformData transform)
        {
            return new BoneData(this, transform);
        }

        public BoneData CreateBone(BoneXml xml)
        {
            var bone = new BoneData(this);
            bone.FromXml(xml);
            return bone;
        }

        public T GetOrCreateTransformData<T>(string name)
            where T : class, ITransformData, new()
        {
            var bone = GetBone(name);
            if (bone != null)
            {
                return bone.transform as T;
            }

            var trans = TimelineManager.CreateTransform<T>(name);
            bone = CreateBone(trans);
            _boneMap[name] = bone;
            return trans;
        }

        public BoneData GetBone(string name)
        {
            BoneData bone;
            if (_boneMap.TryGetValue(name, out bone))
            {
                return bone;
            }
            return null;
        }

        public bool HasBone(BoneData bone)
        {
            return bone != null && _boneMap.ContainsKey(bone.name);
        }

        public bool HasAnyBones(IEnumerable<BoneData> bones)
        {
            return bones.Any(HasBone);
        }

        public BoneData GetOrCreateBone(TransformType transformType, string name)
        {
            BoneData bone;
            if (!_boneMap.TryGetValue(name, out bone))
            {
                var trans = timelineManager.CreateTransform(transformType, name);
                bone = CreateBone(trans);
                _boneMap[name] = bone;
            }
            return bone;
        }

        public void SetBone(BoneData bone)
        {
            if (bone == null)
            {
                return;
            }

            var name = bone.name;
            bone.parentFrame = this;
            _boneMap[name] = bone;
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

            var targetBone = GetOrCreateBone(bone.transform.type, bone.name);
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
                _boneMap.Remove(bone.name);
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

        public List<BoneData> GetDiffBones(FrameData sourceFrame)
        {
            var diffBones = new List<BoneData>(_boneMap.Count);
            foreach (var pair in _boneMap)
            {
                var name = pair.Key;
                var bone = pair.Value;

                if (bone.transform.isHidden)
                {
                    continue;
                }

                var sourceBone = sourceFrame.GetBone(name);
                if (sourceBone == null || !bone.transform.Equals(sourceBone.transform))
                {
                    diffBones.Add(bone);
                }
            }
            return diffBones;
        }

        public List<BoneData> GetFilterBones(IEnumerable<string> boneNames)
        {
            var diffBones = new List<BoneData>(_boneMap.Count);
            var boneNamesSet = new HashSet<string>(boneNames);
            foreach (var pair in _boneMap)
            {
                var name = pair.Key;
                var bone = pair.Value;

                if (bone.transform.isHidden)
                {
                    continue;
                }

                if (boneNamesSet.Contains(name))
                {
                    diffBones.Add(bone);
                }
            }
            return diffBones;
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

            PluginUtils.LogDebug("ボーンタイプの初期化");

            notFlipTypes = new HashSet<IKManager.BoneType>
            {
                IKManager.BoneType.TopFixed,
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
                { IKManager.BoneType.Foot_R, IKManager.BoneType.Foot_L },
                { IKManager.BoneType.Bust_L, IKManager.BoneType.Bust_R },
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
            var transformMap = new Dictionary<IKManager.BoneType, ITransformData>(bones.Count);
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
                    var eulerAngles = transform.eulerAngles;
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
                    else if (boneType == IKManager.BoneType.Pelvis)
                    {
                        newEulerAngles.y = eulerAngles.y + 180f;
                        newEulerAngles.z = eulerAngles.z + 180f;
                    }
                    else if (boneType == IKManager.BoneType.Spine0)
                    {
                        newEulerAngles.x = 270f - (eulerAngles.x - 270f);
                        //newEulerAngles.z = 90f - (eulerAngles.z - 90f);
                    }
                    else if (boneType == IKManager.BoneType.Bust_L || boneType == IKManager.BoneType.Bust_R)
                    {
                        newEulerAngles.y = 360f - (eulerAngles.y - 180f);
                        newEulerAngles.z = 270f - (eulerAngles.z - 270f);
                    }
                    else
                    {
                        newEulerAngles.x = -eulerAngles.x;
                        newEulerAngles.y = -eulerAngles.y;
                    }

                    PluginUtils.LogDebug("Flip Bone：" + boneType + " " + eulerAngles + " -> " + newEulerAngles);

                    var newTransform = timelineManager.CreateTransform(transform.type, BoneUtils.GetBoneName(boneType));
                    newTransform.eulerAngles = newEulerAngles;

                    if (boneType == IKManager.BoneType.Root)
                    {
                        var localPosition = transform.position;
                        localPosition.x = -localPosition.x;
                        newTransform.position = localPosition;
                    }

                    var newBone = CreateBone(newTransform);
                    newBones.Add(newBone);
                }
            }

            ClearBones();
            SetBones(newBones);
        }

        public void FromFrameData(FrameData sourceFrame)
        {
            ClearBones();

            foreach (var bone in sourceFrame.bones)
            {
                var newBone = GetOrCreateBone(bone.transform.type, bone.transform.name);
                newBone.transform.FromTransformData(bone.transform);
                SetBone(newBone);
            }
        }

        public void FromXml(FrameXml xml)
        {
            frameNo = xml.frameNo;

            ClearBones();
            foreach (var boneXml in xml.bones)
            {
                var bone = CreateBone(boneXml);
                SetBone(bone);
            }
        }

        public FrameXml ToXml()
        {
            var xml = new FrameXml();
            xml.frameNo = frameNo;
            xml.bones = new List<BoneXml>(bones.Count);
            foreach (var bone in bones)
            {
                xml.bones.Add(bone.ToXml());
            }
            return xml;
        }
    }
}