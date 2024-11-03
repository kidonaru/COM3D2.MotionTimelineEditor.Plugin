using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using UnityEngine;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    using MotionPlayData = MotionPlayData<MotionData>;
    using IKHoldPlayData = MotionPlayData<IKHoldMotionData>;
    using GroundingPlayData = MotionPlayData<GroundingMotionData>;

    public class PoseTimeLineRow
    {
        public float time;
        public int poseType;
        public string animation;
        public float fadeTime;
        public float speed;
        public Vector3 position;
        public Vector3 rotation;
        public Maid.EyeMoveType eyeMoveType;
        public string option;
    }

    public class MotionTimeLineRow
    {
        public int frame;
        public string name;
        public Vector3 position;
        public Quaternion rotation;
        public Vector3 scale;
        public Vector3 positionInTangents;
        public Vector3 positionOutTangents;
        public Vector4 rotationInTangents;
        public Vector4 rotationOutTangents;
        public Vector3 scaleInTangents;
        public Vector3 scaleOutTangents;
    }

    public class MotionData : MotionDataBase
    {
        public string name;
        public Vector3 stPos;
        public Vector3 edPos;
        public Quaternion stRot;
        public Quaternion edRot;
        public Vector3 stScale;
        public Vector3 edScale;
        public Vector3 posOutTangents;
        public Vector3 posInTangents;
        public Vector4 rotOutTangents;
        public Vector4 rotInTangents;
        public Vector3 scaleOutTangents;
        public Vector3 scaleInTangents;
    }
    
    public class IKHoldTimeLineRow
    {
        public int frame;
        public string name;
        public Vector3 position;
        public Vector3 inTangents;
        public Vector3 outTangents;
        public bool isHold;
        public bool isAnime;
    }

    public class IKHoldMotionData : MotionDataBase
    {
        public string name;
        public Vector3 stPos;
        public Vector3 edPos;
        public Vector3 outTangents;
        public Vector3 inTangents;
        public bool isHold;
        public bool isAnime;
    }

    public class GroundingTimeLineRow
    {
        public int frame;

        public bool isGroundingFootL;
        public bool isGroundingFootR;
        public float floorHeight;
        public float footBaseOffset;
        public float footStretchHeight;
        public float footStretchAngle;
        public float footGroundAngle;
    }

    public class GroundingMotionData : MotionDataBase
    {
        public bool isGroundingFootL;
        public bool isGroundingFootR;
        public float floorHeight;
        public float footBaseOffset;
        public float footStretchHeight;
        public float footStretchAngle;
        public float footGroundAngle;
    }

    [LayerDisplayName("メイドアニメ")]
    public class MotionTimelineLayer : TimelineLayerBase
    {
        public override int priority
        {
            get
            {
                return 0;
            }
        }

        public override string className
        {
            get
            {
                return typeof(MotionTimelineLayer).Name;
            }
        }

        public override bool hasSlotNo
        {
            get
            {
                return true;
            }
        }

        public static string GroundingBoneName = "Grounding";
        public static string GroundingDisplayName = "接地";

        private List<string> _allBoneNames = null;
        public override List<string> allBoneNames
        {
            get
            {
                if (_allBoneNames == null)
                {
                    _allBoneNames = new List<string>(BoneUtils.saveBoneNames);
                    _allBoneNames.AddRange(timeline.GetExtendBoneNames(slotNo));
                    _allBoneNames.AddRange(MaidCache.ikHoldTypeMap.Keys);
                    _allBoneNames.Add(GroundingBoneName);
                }
                return _allBoneNames;
            }
        }

        private Dictionary<string, List<MotionTimeLineRow>> _motionTimelineRowsMap = new Dictionary<string, List<MotionTimeLineRow>>();
        private Dictionary<string, MotionPlayData> _motionPlayDataMap = new Dictionary<string, MotionPlayData>(); 
        private Dictionary<string, List<IKHoldTimeLineRow>> _ikTimelineRowsMap = new Dictionary<string, List<IKHoldTimeLineRow>>();
        private Dictionary<string, IKHoldPlayData> _ikPlayDataMap = new Dictionary<string, IKHoldPlayData>();
        private List<GroundingTimeLineRow> _groundingTimelineRows = new List<GroundingTimeLineRow>();
        private GroundingPlayData _groundingPlayData = new GroundingPlayData();
        private List<PoseTimeLineRow> _dcmOutputRows = new List<PoseTimeLineRow>();
        private List<string> _extendSlotNames = new List<string>();

        private MotionTimelineLayer(int slotNo) : base(slotNo)
        {
        }

        public static MotionTimelineLayer Create(int slotNo)
        {
            return new MotionTimelineLayer(slotNo);
        }

        public override void Init()
        {
            base.Init();

            foreach (var frame in keyFrames)
            {
                foreach (var extendBoneName in timeline.GetExtendBoneNames(slotNo))
                {
                    var bone = frame.GetBone(extendBoneName);
                    if (bone == null)
                    {
                        continue;
                    }

                    var transform = bone.transform;
                    if (transform.position.x == float.MinValue)
                    {
                        PluginUtils.LogDebug("ボーンの初期位置を設定 boneName={0}", extendBoneName);
                        transform.position = maidCache.GetInitialPosition(extendBoneName);
                    }
                }
            }
        }

        protected override void InitMenuItems()
        {
            _allMenuItems.Clear();
            _extendSlotNames.Clear();
            _allBoneNames = null;

            var setMenuItemMap = new Dictionary<BoneSetMenuType, BoneSetMenuItem>(12);

            foreach (var pair in BoneUtils.BoneTypeToSetMenuTypeMap)
            {
                var boneType = pair.Key;
                var boneSetType = pair.Value;

                var boneName = BoneUtils.GetBoneName(boneType);
                var displayName = BoneUtils.GetBoneJpName(boneType);
                var menuItem = new MaidBoneMenuItem(boneName, displayName);

                if (boneSetType == BoneSetMenuType.None)
                {
                    _allMenuItems.Add(menuItem);
                    continue;
                }

                BoneSetMenuItem setMenuItem;
                if (!setMenuItemMap.TryGetValue(boneSetType, out setMenuItem))
                {
                    var boneSetName = boneSetType.ToString();
                    var displaySetName = BoneUtils.GetBoneSetMenuJpName(boneSetType);
                    setMenuItem = new BoneSetMenuItem(boneSetName, displaySetName);
                    setMenuItemMap[boneSetType] = setMenuItem;
                    _allMenuItems.Add(setMenuItem);
                }

                setMenuItem.AddChild(menuItem);
            }

            var slotMenuItemMap = new Dictionary<string, BoneSetMenuItem>(12);

            foreach (var extendBoneName in timeline.GetExtendBoneNames(slotNo))
            {
                if (maidCache == null)
                {
                    break;
                }
                var entity = maidCache.extendBoneCache.GetEntity(extendBoneName);
                if (entity == null)
                {
                    continue;
                }

                var slotName = entity.slotName;
                var boneName = entity.boneName;

                var menuItem = new ExtendBoneMenuItem(extendBoneName, boneName);

                BoneSetMenuItem setMenuItem;
                if (!slotMenuItemMap.TryGetValue(slotName, out setMenuItem))
                {
                    setMenuItem = new BoneSetMenuItem(slotName, slotName);
                    slotMenuItemMap[slotName] = setMenuItem;
                    _allMenuItems.Add(setMenuItem);
                }

                setMenuItem.AddChild(menuItem);
            }

            _extendSlotNames.AddRange(slotMenuItemMap.Keys);

            var ikSetMenuItem = new BoneSetMenuItem("IK", "IK");
            allMenuItems.Add(ikSetMenuItem);

            foreach (var pair in MaidCache.ikHoldTypeMap)
            {
                var boneName = pair.Key;
                var holdType = pair.Value;
                var displayName = IKHoldEntity.GetHoldTypeName(holdType);

                var menuItem = new BoneMenuItem(boneName, displayName);
                ikSetMenuItem.AddChild(menuItem);
            }

            var groundingMenuItem = new BoneMenuItem(GroundingBoneName, GroundingDisplayName);
            allMenuItems.Add(groundingMenuItem);
        }

        public override bool IsValidData()
        {
            errorMessage = "";

            var firstFrame = this.firstFrame;
            if (firstFrame == null || firstFrame.frameNo != 0)
            {
                errorMessage = "0フレーム目にキーフレームが必要です";
                return false;
            }

            return true;
        }

        public override void Update()
        {
            base.Update();
        }

        public override void LateUpdate()
        {
            base.LateUpdate();

            if (!studioHack.isPoseEditing)
            {
                ApplyPlayData();
            }
        }

        private void ApplyPlayData()
        {
            var playingFrameNoFloat = this.playingFrameNoFloat;

            foreach (var boneName in _motionPlayDataMap.Keys)
            {
                var playData = _motionPlayDataMap[boneName];

                playData.Update(playingFrameNoFloat);

                var current = playData.current;
                if (current != null)
                {
                    ApplyMotion(current, playData.lerpFrame);
                }
            }

            foreach (var boneName in _ikPlayDataMap.Keys)
            {
                if (!MaidCache.ikHoldTypeMap.ContainsKey(boneName))
                {
                    continue;
                }

                var playData = _ikPlayDataMap[boneName];

                playData.Update(playingFrameNoFloat);

                var current = playData.current;
                if (current != null)
                {
                    ApplyIKHoldMotion(current, playData.lerpFrame);
                }
            }

            {
                var playData = _groundingPlayData;

                playData.Update(playingFrameNoFloat);

                var current = playData.current;
                if (current != null)
                {
                    ApplyGroundingMotion(current);
                }
            }
        }

        private void ApplyMotion(MotionData motion, float lerpFrame)
        {
            if (maidCache == null)
            {
                return;
            }

            var boneName = motion.name;
            var bone = maidCache.GetBoneTransform(boneName);
            if (bone == null)
            {
                return;
            }

            var t0 = motion.stFrame * timeline.frameDuration;
            var t1 = motion.edFrame * timeline.frameDuration;

            bone.localPosition = PluginUtils.HermiteVector3(
                t0,
                t1,
                motion.stPos,
                motion.edPos,
                motion.posOutTangents,
                motion.posInTangents,
                lerpFrame);

            /*bone.localRotation = PluginUtils.HermiteQuaternion(
                t0,
                t1,
                motion.stRot,
                motion.edRot,
                motion.rotOutTangents,
                motion.rotInTangents,
                lerpFrame);*/

            bone.localScale = PluginUtils.HermiteVector3(
                t0,
                t1,
                motion.stScale,
                motion.edScale,
                motion.scaleOutTangents,
                motion.scaleInTangents,
                lerpFrame);
        }

        private void ApplyIKHoldMotion(IKHoldMotionData motion, float lerpFrame)
        {
            if (maidCache == null)
            {
                return;
            }

            var ikHoldEntity = maidCache.GetIKHoldEntity(motion.name);
            if (ikHoldEntity == null)
            {
                return;
            }

            var t0 = motion.stFrame * timeline.frameDuration;
            var t1 = motion.edFrame * timeline.frameDuration;

            ikHoldEntity.isHold = motion.isHold;
            ikHoldEntity.isAnime = motion.isAnime;
            ikHoldEntity.targetPosition = PluginUtils.HermiteVector3(
                t0,
                t1,
                motion.stPos,
                motion.edPos,
                motion.outTangents,
                motion.inTangents,
                lerpFrame);
        }

        private void ApplyGroundingMotion(GroundingMotionData motion)
        {
            maidCache.isGroundingFootL = motion.isGroundingFootL;
            maidCache.isGroundingFootR = motion.isGroundingFootR;
            maidCache.floorHeight = motion.floorHeight;
            maidCache.footBaseOffset = motion.footBaseOffset;
            maidCache.footStretchHeight = motion.footStretchHeight;
            maidCache.footStretchAngle = motion.footStretchAngle;
            maidCache.footGroundAngle = motion.footGroundAngle;
        }

        public override void OnMaidChanged(Maid maid)
        {
            InitMenuItems();
        }

        public override void OnBoneNameAdded(string extendBoneName)
        {
            InitMenuItems();

            var boneNames = new List<string> { extendBoneName };
            AddFirstBones(boneNames);
            ApplyCurrentFrame(true);
        }

        public override void OnBoneNameRemoved(string extendBoneName)
        {
            InitMenuItems();

            var boneNames = new List<string> { extendBoneName };
            RemoveAllBones(boneNames);
            ApplyCurrentFrame(true);
        }

        public override void UpdateFrame(FrameData frame)
        {
            var cacheBoneData = maidManager.cacheBoneData;
            if (cacheBoneData == null)
            {
                PluginUtils.LogError("ボーンデータが取得できませんでした");
                return;
            }

            var rootBone = cacheBoneData.GetBoneData("Bip01");
            if (rootBone == null)
            {
                PluginUtils.LogError("中心ボーンが取得できませんでした");
                return;
            }

            // 編集モード中の移動は中心ボーンに反映  
            if (timelineManager.initialEditFrame != null)
            {
                var targetPosition = rootBone.transform.position;
                var targetRotation = rootBone.transform.rotation;

                maid.transform.position = timelineManager.initialEditPosition;
                maid.transform.rotation = timelineManager.initialEditRotation;

                rootBone.transform.position = targetPosition;
                rootBone.transform.rotation = targetRotation;
            }

            var maidCache = this.maidCache;

            foreach (var name in allBoneNames)
            {
                ITransformData trans;

                var holdtype = MaidCache.GetIKHoldType(name);
                if (holdtype != IKHoldType.Max)
                {
                    var ikHoldEntity = maidCache.GetIKHoldEntity(name);
                    if (ikHoldEntity == null)
                    {
                        continue;
                    }

                    trans = CreateTransformData(name);
                    trans.position = ikHoldEntity.position;
                    trans["isHold"].boolValue = ikHoldEntity.isHold;
                    trans["isAnime"].boolValue = ikHoldEntity.isAnime;
                }
                else if (name == GroundingBoneName)
                {
                    trans = CreateTransformData(name);
                    trans["isGroundingFootL"].boolValue = maidCache.isGroundingFootL;
                    trans["isGroundingFootR"].boolValue = maidCache.isGroundingFootR;
                    trans["floorHeight"].value = maidCache.floorHeight;
                    trans["footBaseOffset"].value = maidCache.footBaseOffset;
                    trans["footStretchHeight"].value = maidCache.footStretchHeight;
                    trans["footStretchAngle"].value = maidCache.footStretchAngle;
                    trans["footGroundAngle"].value = maidCache.footGroundAngle;
                }
                else
                {
                    var transform = maidCache.GetBoneTransform(name);
                    if (transform == null)
                    {
                        PluginUtils.LogDebug("UpdateFrame: ボーンがないのでスキップしました name={0}", name);
                        continue;
                    }

                    trans = CreateTransformData(name);
                    if (trans.hasPosition)
                    {
                        trans.position = transform.localPosition;
                    }
                    if (trans.hasRotation)
                    {
                        trans.rotation = transform.localRotation;
                    }
                    if (trans.hasScale)
                    {
                        trans.scale = transform.localScale;
                    }
                }

                var bone = frame.CreateBone(trans);
                frame.UpdateBone(bone);
            }
        }

        public override void ApplyAnm(long id, byte[] anmData)
        {
            PluginUtils.LogDebug("ApplyAnm: id={0}", id);
            if (anmData == null)
            {
                return;
            }

            var maid = this.maid;
            if (maid == null)
            {
                PluginUtils.LogError("メイドが配置されていません");
                return;
            }

            float playingFrameNoFloat = defaultLayer.playingFrameNoFloat;
            var isMotionPlaying = this.isMotionPlaying;
            if (isMotionPlaying)
            {
                playingFrameNoFloat += 0.01f; // モーション再生中は再生位置に差分がないと反映されない
            }

            PluginUtils.LogDebug("playingFrameNoFloat={0}", playingFrameNoFloat);

            maidCache.PlayAnm(id, anmData);
            studioHack.OnMotionUpdated(maid);
            maidManager.OnMotionUpdated(maid);

            this.isMotionPlaying = isMotionPlaying;
            maidCache.playingFrameNoFloat = playingFrameNoFloat;

            if (config.isAutoYureBone)
            {
                foreach (var slotName in maidCache.extendBoneCache.yureSlotNames)
                {
                    var yureState = !_extendSlotNames.Contains(slotName);
                    if (yureState != maidCache.GetYureState(slotName))
                    {
                        maidCache.SetYureState(slotName, yureState);
                    }
                }
            }

            ApplyPlayData();
        }

        public override void ApplyCurrentFrame(bool motionUpdate)
        {
            if (anmId != TimelineAnmId || motionUpdate)
            {
                CreateAndApplyAnm();
            }
            else
            {
                maidCache.playingFrameNo = timelineManager.currentFrameNo;
                ApplyPlayData();
            }
        }

        public override void OutputAnm()
        {
            try
            {
                var anmData = GetAnmBinary(true);
                if (anmData == null)
                {
                    PluginUtils.ShowDialog(errorMessage);
                    return;
                }
                var anmPath = this.anmPath;
                var anmFileName = this.anmFileName;

                bool isExist = File.Exists(anmPath);
                File.WriteAllBytes(anmPath, anmData);

                studioHack.OnUpdateMyPose(anmPath, isExist);
            }
            catch (Exception e)
            {
                PluginUtils.LogException(e);
                PluginUtils.ShowDialog("モーションの出力に失敗しました");
            }
        }

        private void BuildPlayData(bool forOutput)
        {
            PluginUtils.LogDebug("BuildPlayData");
            _ikPlayDataMap.Clear();

            foreach (var pair in _motionTimelineRowsMap)
            {
                var name = pair.Key;
                var rows = pair.Value;

                MotionPlayData playData;
                if (!_motionPlayDataMap.TryGetValue(name, out playData))
                {
                    playData = new MotionPlayData
                    {
                        motions = new List<MotionData>(rows.Count),
                    };
                    _motionPlayDataMap[name] = playData;
                }

                playData.ResetIndex();
                playData.motions.Clear();

                for (var i = 0; i < rows.Count - 1; i++)
                {
                    var start = rows[i];
                    var end = rows[i + 1];

                    var stFrame = start.frame;
                    var edFrame = end.frame;

                    var motion = new MotionData
                    {
                        name = name,
                        stFrame = stFrame,
                        edFrame = edFrame,
                        stPos = start.position,
                        edPos = end.position,
                        stRot = start.rotation,
                        edRot = end.rotation,
                        stScale = start.scale,
                        edScale = end.scale,
                        posOutTangents = start.positionOutTangents,
                        posInTangents = end.positionInTangents,
                        rotOutTangents = start.rotationOutTangents,
                        rotInTangents = end.rotationInTangents,
                        scaleOutTangents = start.scaleOutTangents,
                        scaleInTangents = end.scaleInTangents,
                    };

                    playData.motions.Add(motion);
                }
            }

            foreach (var pair in _motionPlayDataMap)
            {
                var playData = pair.Value;
                playData.Setup(timeline.singleFrameType);
            }

            foreach (var pair in _ikTimelineRowsMap)
            {
                var name = pair.Key;
                var rows = pair.Value;

                IKHoldPlayData playData;
                if (!_ikPlayDataMap.TryGetValue(name, out playData))
                {
                    playData = new IKHoldPlayData
                    {
                        motions = new List<IKHoldMotionData>(rows.Count),
                    };
                    _ikPlayDataMap[name] = playData;
                }

                playData.ResetIndex();
                playData.motions.Clear();

                for (var i = 0; i < rows.Count - 1; i++)
                {
                    var start = rows[i];
                    var end = rows[i + 1];

                    var stFrame = start.frame;
                    var edFrame = end.frame;

                    var motion = new IKHoldMotionData
                    {
                        name = name,
                        stFrame = stFrame,
                        edFrame = edFrame,
                        stPos = start.position,
                        edPos = end.position,
                        outTangents = start.outTangents,
                        inTangents = end.inTangents,
                        isHold = start.isHold,
                        isAnime = start.isAnime,
                    };

                    playData.motions.Add(motion);
                }
            }

            foreach (var pair in _ikPlayDataMap)
            {
                var playData = pair.Value;
                playData.Setup(timeline.singleFrameType);
            }

            {
                var rows = _groundingTimelineRows;
                var playData = _groundingPlayData;

                playData.ResetIndex();
                playData.motions.Clear();

                for (var i = 0; i < rows.Count - 1; i++)
                {
                    var start = rows[i];
                    var end = rows[i + 1];

                    var stFrame = start.frame;
                    var edFrame = end.frame;

                    var motion = new GroundingMotionData
                    {
                        stFrame = stFrame,
                        edFrame = edFrame,
                        isGroundingFootL = start.isGroundingFootL,
                        isGroundingFootR = start.isGroundingFootR,
                        floorHeight = start.floorHeight,
                        footBaseOffset = start.footBaseOffset,
                        footStretchHeight = start.footStretchHeight,
                        footStretchAngle = start.footStretchAngle,
                        footGroundAngle = start.footGroundAngle,
                    };

                    playData.motions.Add(motion);
                }
            }

            _groundingPlayData.Setup(SingleFrameType.None);
        }

        protected override byte[] GetAnmBinaryInternal(bool forOutput, int startFrameNo, int endFrameNo)
        {
            if (maidCache == null)
            {
                return null;
            }

            var startSecond = timeline.GetFrameTimeSeconds(startFrameNo);
            var endSecond = timeline.GetFrameTimeSeconds(endFrameNo);

            var times = new List<float>(_keyFrames.Count);
            var valuesList = new List<ValueData[]>(_keyFrames.Count);

            int _startFrameNo = startFrameNo;
            int _endFrameNo = endFrameNo;
            Action<BinaryWriter, BoneData> write_bone_data = delegate (
                BinaryWriter w,
                BoneData firstBone)
            {
                var name = firstBone.name;
                var path = maidCache.GetBonePath(name);
                if (string.IsNullOrEmpty(path))
                {
                    PluginUtils.LogWarning("ボーンがないのでスキップしました boneName={0}", name);
                    return;
                }

                w.Write((byte)1);
                w.Write(path);

                times.Clear();
                valuesList.Clear();

                bool hasLastKey = false;
                foreach (var frame in _keyFrames)
                {
                    if (frame.frameNo < _startFrameNo || frame.frameNo > _endFrameNo)
                    {
                        continue;
                    }

                    var bone = frame.GetBone(name);
                    if (bone == null && frame.frameNo == _startFrameNo)
                    {
                        bone = GetPrevBone(frame.frameNo, name, false);
                    }

                    if (bone == null && frame.frameNo == _endFrameNo)
                    {
                        bone = GetNextBone(frame.frameNo, name, false);
                    }

                    if (bone != null)
                    {
                        times.Add(timeline.GetFrameTimeSeconds(frame.frameNo) - startSecond);
                        valuesList.Add(bone.transform.values);
                        hasLastKey = frame.frameNo == _endFrameNo;
                    }
                }

                if (!hasLastKey)
                {
                    var bone = _dummyLastFrame.GetBone(name);
                    if (bone != null)
                    {
                        times.Add(endSecond - startSecond);
                        valuesList.Add(bone.transform.values);
                    }
                }

                for (int i = 0; i < firstBone.transform.valueCount; i++)
                {
                    w.Write((byte)(100 + i));
                    w.Write(times.Count);
                    for (int j = 0; j < times.Count; j++)
                    {
                        w.Write(times[j]);
                        w.Write(valuesList[j][i].value);
                        w.Write(valuesList[j][i].inTangent.value);
                        w.Write(valuesList[j][i].outTangent.value);
                    }
                }
            };
            MemoryStream memoryStream = new MemoryStream();
            BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
            binaryWriter.Write("CM3D2_ANIM");
            binaryWriter.Write(1001);
            foreach (var name in allBoneNames)
            {
                if (MaidCache.ikHoldTypeMap.ContainsKey(name))
                {
                    continue;
                }
                if (name == GroundingBoneName)
                {
                    continue;
                }

                var bone = firstFrame.GetBone(name);
                if (bone == null)
                {
                    PluginUtils.Log("0フレーム目にキーフレームがないのでスキップしました boneName={0}", name);
                    continue;
                }
                write_bone_data(binaryWriter, bone);
            }
            binaryWriter.Write((byte)0);
            binaryWriter.Write((byte)(useMuneKeyL ? 1u : 0u));
            binaryWriter.Write((byte)(useMuneKeyR ? 1u : 0u));
            binaryWriter.Close();
            memoryStream.Close();
            byte[] result = memoryStream.ToArray();
            memoryStream.Dispose();

            if (!forOutput && maidCache != null)
            {
                maidCache.anmStartFrameNo = startFrameNo;
                maidCache.anmEndFrameNo = endFrameNo;
            }

            {
                _motionTimelineRowsMap.Clear();
                _ikTimelineRowsMap.Clear();
                _groundingTimelineRows.Clear();

                foreach (var keyFrame in keyFrames)
                {
                    AppendTimelineRow(keyFrame);
                }

                AppendTimelineRow(_dummyLastFrame);

                BuildPlayData(forOutput);
            }

            return result;
        }

        private void AppendTimelineRow(FrameData frame)
        {
            foreach (var bone in frame.bones)
            {
                var name = bone.name;

                var extendBoneEntity = maidCache.extendBoneCache.GetEntity(name);
                if (extendBoneEntity == null)
                {
                    continue;
                }

                List<MotionTimeLineRow> rows;
                if (!_motionTimelineRowsMap.TryGetValue(name, out rows))
                {
                    rows = new List<MotionTimeLineRow>();
                    _motionTimelineRowsMap[name] = rows;
                }

                var trans = bone.transform;

                var row = new MotionTimeLineRow
                {
                    frame = frame.frameNo,
                    name = bone.name,
                    position = trans.position,
                    rotation = trans.rotation,
                    scale = trans.scale,
                    positionInTangents = trans.positionInTangents.ToVector3(),
                    positionOutTangents = trans.positionOutTangents.ToVector3(),
                    rotationInTangents = trans.rotationInTangents.ToVector4(),
                    rotationOutTangents = trans.rotationOutTangents.ToVector4(),
                    scaleInTangents = trans.scaleInTangents.ToVector3(),
                    scaleOutTangents = trans.scaleOutTangents.ToVector3(),
                };

                rows.Add(row);
            }

            foreach (var name in MaidCache.ikHoldTypeMap.Keys)
            {
                var bone = frame.GetBone(name);
                if (bone == null)
                {
                    continue;
                }

                List<IKHoldTimeLineRow> rows;
                if (!_ikTimelineRowsMap.TryGetValue(name, out rows))
                {
                    rows = new List<IKHoldTimeLineRow>();
                    _ikTimelineRowsMap[name] = rows;
                }

                var trans = bone.transform;

                var row = new IKHoldTimeLineRow
                {
                    frame = frame.frameNo,
                    name = bone.name,
                    position = trans.position,
                    inTangents = trans.positionInTangents.ToVector3(),
                    outTangents = trans.positionOutTangents.ToVector3(),
                    isHold = trans["isHold"].boolValue,
                    isAnime = trans["isAnime"].boolValue,
                };

                rows.Add(row);
            }

            {
                var bone = frame.GetBone(GroundingBoneName);
                if (bone == null)
                {
                    return;
                }

                var rows = _groundingTimelineRows;

                var trans = bone.transform;

                var row = new GroundingTimeLineRow
                {
                    frame = frame.frameNo,
                    isGroundingFootL = trans["isGroundingFootL"].boolValue,
                    isGroundingFootR = trans["isGroundingFootR"].boolValue,
                    floorHeight = trans["floorHeight"].value,
                    footBaseOffset = trans["footBaseOffset"].value, 
                    footStretchHeight = trans["footStretchHeight"].value,
                    footStretchAngle = trans["footStretchAngle"].value,
                    footGroundAngle = trans["footGroundAngle"].value,
                };

                rows.Add(row);
            }
        }

        public void SavePoseTimeLine(
            List<PoseTimeLineRow> rows,
            string filePath)
        {
            var offsetTime = timeline.startOffsetTime;

            var builder = new StringBuilder();
            builder.Append("time,poseType,animation,fadeTime,speed,posX,posY,posZ,rotX,rotY,rotZ,eyeMoveType,option\r\n");

            Action<PoseTimeLineRow, bool> appendRow = (row, isFirst) =>
            {
                var time = row.time;

                if (!isFirst)
                {
                    time += offsetTime;
                }

                builder.Append(time.ToString("0.000") + ",");
                builder.Append(row.poseType + ",");
                builder.Append(row.animation + ",");
                builder.Append(row.fadeTime.ToString("0.000") + ",");
                builder.Append(row.speed.ToString("0.000") + ",");
                builder.Append(row.position.x.ToString("0.000") + ",");
                builder.Append(row.position.y.ToString("0.000") + ",");
                builder.Append(row.position.z.ToString("0.000") + ",");
                builder.Append(row.rotation.x.ToString("0.000") + ",");
                builder.Append(row.rotation.y.ToString("0.000") + ",");
                builder.Append(row.rotation.z.ToString("0.000") + ",");
                builder.Append((int) row.eyeMoveType + ",");
                builder.Append(row.option);
                builder.Append("\r\n");
            };

            if (rows.Count > 0 && offsetTime > 0f)
            {
                appendRow(rows.First(), true);
            }

            foreach (var row in rows)
            {
                appendRow(row, false);
            }

            using (var streamWriter = new StreamWriter(filePath, false))
            {
                streamWriter.Write(builder.ToString());
            }
        }

        public override void OutputDCM(XElement songElement)
        {
            try
            {
                var anmFileName = this.anmFileName;

                {
                    var anmData = GetAnmBinary(true);
                    if (anmData == null)
                    {
                        PluginUtils.ShowDialog(errorMessage);
                        return;
                    }

                    var anmPath = timeline.GetDcmSongFilePath(anmFileName);

                    bool isExist = File.Exists(anmPath);
                    File.WriteAllBytes(anmPath, anmData);

                    var maidElement = GetMeidElement(songElement);
                    //maidElement.Add(new XElement("customAnimation", anmFileName));
                }

                {
                    _dcmOutputRows.Clear();

                    var row = new PoseTimeLineRow
                    {
                        time = 0f,
                        poseType = 0,
                        animation = anmFileName,
                        fadeTime = 0f,
                        speed = 1f,
                        position = Vector3.zero,
                        rotation = Vector3.zero,
                        eyeMoveType = timeline.eyeMoveType,
                        option = string.Empty
                    };
                    _dcmOutputRows.Add(row);

                    var outputFileName = string.Format("pose_{0}.csv", slotNo);
                    var outputPath = timeline.GetDcmSongFilePath(outputFileName);
                    SavePoseTimeLine(_dcmOutputRows, outputPath);

                    var maidElement = GetMeidElement(songElement);
                    maidElement.Add(new XElement("pose", outputFileName));
                }
            }
            catch (Exception e)
            {
                PluginUtils.LogException(e);
                PluginUtils.ShowDialog("モーションの出力に失敗しました");
            }
        }

        public override float CalcEasingValue(float t, int easing)
        {
            return t;
        }

        private GUIComboBox<TransformEditType> _transComboBox = new GUIComboBox<TransformEditType>
        {
            items = Enum.GetValues(typeof(TransformEditType)).Cast<TransformEditType>().ToList(),
            getName = (type, index) =>
            {
                return type.ToString();
            },
        };

        private GUIComboBox<string> _slotNameComboBox = new GUIComboBox<string>
        {
            getName = (slotName, index) =>
            {
                return slotName;
            },
        };

        private GUIComboBox<IBoneMenuItem> _menuItemComboBox = new GUIComboBox<IBoneMenuItem>
        {
            getName = (menuItem, index) =>
            {
                return menuItem.displayName;
            },
        };

        private enum TabType
        {
            編集,
            追加,
            手指,
            足指,
        }

        private TabType _tabType = TabType.編集;

        public override void DrawWindow(GUIView view)
        {
            view.SetEnabled(!view.IsComboBoxFocused());

            if (maidCache == null)
            {
                view.DrawLabel("メイドが配置されていません", 200, 20);
                return;
            }

            _tabType = view.DrawTabs(_tabType, 50, 20);

            switch (_tabType)
            {
                case TabType.編集:
                    DrawTransformEdit(view);
                    break;
                case TabType.追加:
                    DrawExtendBone(view);
                    break;
                case TabType.手指:
                    view.DrawHorizontalLine(Color.gray);
                    DrawFingerBlend(
                        view,
                        FingerSlotNames,
                        WindowPartsFingerBlend.Type.RightArm,
                        WindowPartsFingerBlend.Type.LeftArm);
                    DrawFingerBlend(
                        view,
                        FingerSlotNames,
                        WindowPartsFingerBlend.Type.LeftArm,
                        WindowPartsFingerBlend.Type.RightArm);
                    break;
                case TabType.足指:
                    view.DrawHorizontalLine(Color.gray);
                    DrawFingerBlend(
                        view,
                        LegSlotNames,
                        WindowPartsFingerBlend.Type.RightLeg,
                        WindowPartsFingerBlend.Type.LeftLeg);
                    DrawFingerBlend(
                        view,
                        LegSlotNames,
                        WindowPartsFingerBlend.Type.LeftLeg,
                        WindowPartsFingerBlend.Type.RightLeg);
                    break;
            }

            view.DrawComboBox();
        }

        private void DrawTransformEdit(GUIView view)
        {
            _menuItemComboBox.items = allMenuItems;
            _menuItemComboBox.DrawButton("対象カテゴリ", view);

            var menuItem = _menuItemComboBox.currentItem;
            if (menuItem == null)
            {
                view.DrawLabel("カテゴリを選択してください", -1, 20);
                return;
            }

            _transComboBox.DrawButton("操作種類", view);

            var editType = _transComboBox.currentItem;

            view.DrawHorizontalLine(Color.gray);

            view.AddSpace(5);

            view.BeginScrollView();

            view.SetEnabled(!view.IsComboBoxFocused() && studioHack.isPoseEditing);

            if (menuItem.name == "IK")
            {
                foreach (var child in menuItem.children)
                {
                    DrawIKMenuItem(view, editType, child);
                }
            }
            else if (menuItem.children != null)
            {
                foreach (var child in menuItem.children)
                {
                    DrawMenuItem(view, editType, child);
                }
            }
            else
            {
                DrawMenuItem(view, editType, menuItem);
            }

            view.SetEnabled(!view.IsComboBoxFocused());
            view.EndScrollView();
        }

        private void DrawMenuItem(
            GUIView view,
            TransformEditType editType,
            IBoneMenuItem menuItem)
        {
            var boneName = menuItem.name;
            var displayName = menuItem.displayName;
            var isDefaultBoneName = BoneUtils.IsDefaultBoneName(boneName);
            var boneType = BoneUtils.GetBoneTypeByName(boneName);
            var holdtype = MaidCache.GetIKHoldType(boneName);
            var transform = maidCache.GetBoneTransform(boneName);
            var initialPosition = maidCache.GetInitialPosition(boneName);
            var initialEulerAngles = maidCache.GetInitialEulerAngles(boneName);
            var initialScale = Vector3.one;

            var drawMask = DrawMaskPositonAndRotation;
            if (holdtype != IKHoldType.Max)
            {
                drawMask = DrawMaskPosition;
            }
            else if (boneName == GroundingBoneName)
            {
                //
            }
            else if (boneName == "Bip01")
            {
                drawMask = DrawMaskPositonAndRotation;
            }
            else if (isDefaultBoneName)
            {
                drawMask = DrawMaskRotation;
            }
            else
            {
                drawMask = DrawMaskAll;
            }

            if (transform == null)
            {
                return;
            }

            view.DrawLabel(displayName, 200, 20);

            DrawTransform(
                view,
                transform,
                editType,
                drawMask,
                boneName,
                initialPosition,
                initialEulerAngles,
                initialScale);

            view.DrawHorizontalLine(Color.gray);
        }

        private void DrawIKMenuItem(
            GUIView view,
            TransformEditType editType,
            IBoneMenuItem menuItem)
        {
            var boneName = menuItem.name;
            var displayName = menuItem.displayName;
            var ikHoldEntity = maidCache.GetIKHoldEntity(boneName);
            var transformCache = view.GetTransformCache(null);
            var initialPosition = maidCache.GetInitialPosition(boneName);
            if (ikHoldEntity == null)
            {
                return;
            }

            view.DrawLabel(displayName, 200, 20);

            transformCache.position = ikHoldEntity.targetPosition;
            var updateTransform = DrawPosition(view, transformCache, editType, initialPosition);

            view.DrawHorizontalLine(Color.gray);

            if (updateTransform)
            {
                var position = transformCache.position;
                if (ikHoldEntity != null)
                {
                    ikHoldEntity.targetPosition = position;
                }
            }
        }

        private static readonly string[] FingerBrendNames = new string[]
        {
            "右手",
            "左手",
            "右足",
            "左足",
        };

        private static readonly string[] FingerSlotNames = new string[]
        {
            "親",
            "人",
            "中",
            "薬",
            "小",
        };

        private static readonly string[] LegSlotNames = new string[]
        {
            "親",
            "中",
            "小",
        };

        private FingerBlend.BaseFinger GetBaseFingerClass(WindowPartsFingerBlend.Type type)
        {
            var finger_blend = maidManager.ikManager.finger_blend;
            FingerBlend.BaseFinger result = null;
            if (type == WindowPartsFingerBlend.Type.RightArm)
            {
                result = finger_blend.right_arm_finger;
            }
            else if (type == WindowPartsFingerBlend.Type.LeftArm)
            {
                result = finger_blend.left_arm_finger;
            }
            else if (type == WindowPartsFingerBlend.Type.RightLeg)
            {
                result = finger_blend.right_leg_finger;
            }
            else if (type == WindowPartsFingerBlend.Type.LeftLeg)
            {
                result = finger_blend.left_leg_finger;
            }
            return result;
        }

        public void DrawFingerBlend(
            GUIView view,
            string[] slotNames,
            WindowPartsFingerBlend.Type blendType,
            WindowPartsFingerBlend.Type otherBlendType)
        {
            var baseFinger = GetBaseFingerClass(blendType);

            view.SetEnabled(!view.IsComboBoxFocused() && studioHack.isPoseEditing);

            view.BeginHorizontal();
            {
                view.DrawLabel(FingerBrendNames[(int)blendType], 40, 20);

                view.DrawToggle("有効", baseFinger.enabled, 60, 20, newValue =>
                {
                    baseFinger.enabled = newValue;
                    baseFinger.Apply();
                });

                if (view.DrawButton("更新", 50, 20))
                {
                    baseFinger.Apply();
                }
            }
            view.EndLayout();

            view.BeginHorizontal();
            {
                view.DrawLabel("ロック", 40, 20);

                bool isAllLock = true;
                for (int i = 0; i < slotNames.Length; i++)
                {
                    var isLock = baseFinger.IsLock(i);
                    isAllLock &= isLock;
                    if (view.DrawButton(slotNames[i], 25, 20, true, isLock ? Color.green : Color.white))
                    {
                        baseFinger.LockSingleItem(!isLock, i);
                        baseFinger.Apply();
                    }
                }

                if (view.DrawButton("全", 25, 20, true, isAllLock ? Color.green : Color.white))
                {
                    baseFinger.LockAllItems(!isAllLock);
                    baseFinger.Apply();
                }

                if (view.DrawButton("反", 25, 20))
                {
                    baseFinger.LockReverse();
                    baseFinger.Apply();
                }
            }
            view.EndLayout();

            view.DrawSliderValue(
                new GUIView.SliderOption
                {
                    label = "開き具合",
                    labelWidth = 60,
                    min = 0f,
                    max = 1f,
                    step = 0f,
                    defaultValue = 0f,
                    value = baseFinger.value_open,
                    onChanged = value =>
                    {
                        baseFinger.value_open = value;
                        baseFinger.Apply();
                    },
                });

            view.DrawSliderValue(
                new GUIView.SliderOption
                {
                    label = "閉じ具合",
                    labelWidth = 60,
                    min = 0f,
                    max = 1f,
                    step = 0f,
                    defaultValue = 0f,
                    value = baseFinger.value_fist,
                    onChanged = value =>
                    {
                        baseFinger.value_fist = value;
                        baseFinger.Apply();
                    },
                });

            var otherName = FingerBrendNames[(int)otherBlendType];
            if (view.DrawButton(otherName + "にコピー", 100, 20))
            {
                var otherBaseFinger = GetBaseFingerClass(otherBlendType);
                otherBaseFinger.CopyFrom(baseFinger);
                baseFinger.Apply();
            }

            view.DrawHorizontalLine(Color.gray);
        }

        private bool _isExtendBoneAllEnabled = true;

        public void DrawExtendBone(GUIView view)
        {
            var extendedBoneCache = this.maidCache.extendBoneCache;

            _slotNameComboBox.items = extendedBoneCache.slotNames;
            _slotNameComboBox.DrawButton("対象スロット", view);

            var slotName = _slotNameComboBox.currentItem;

            if (string.IsNullOrEmpty(slotName))
            {
                view.DrawLabel("対象スロットがありません", -1, 20);
                return;
            }

            var entities = extendedBoneCache.entities.Values
                .Where(entity => entity.slotName == slotName)
                .ToList();

            view.BeginHorizontal();
            {
                if (maidCache.IsYureSlot(slotName))
                {
                    var yureState = maidCache.GetYureState(slotName);
                    view.DrawToggle("揺れボーン", yureState, 120, 20, newValue =>
                    {
                        maidCache.SetYureState(slotName, newValue);
                    });
                }

                view.currentPos.x = view.viewRect.width - 130;

                if (_isExtendBoneAllEnabled)
                {
                    if (view.DrawButton("全解除", 60, 20))
                    {
                        foreach (var entity in entities)
                        {
                            timeline.RemoveExtendBoneName(slotNo, entity.extendBoneName);
                        }
                    }
                }
                else
                {
                    if (view.DrawButton("全選択", 60, 20))
                    {
                        foreach (var entity in entities)
                        {
                            timeline.AddExtendBoneName(slotNo, entity.extendBoneName);
                        }
                    }
                }

                if (view.DrawButton("更新", 50, 20))
                {
                    maidCache.extendBoneCache.Refresh();
                }
            }
            view.EndLayout();

            view.DrawHorizontalLine(Color.gray);

            view.AddSpace(5);

            view.BeginScrollView();

            _isExtendBoneAllEnabled = true;

            foreach (var entity in entities)
            {
                var extendBoneName = entity.extendBoneName;

                var enabled = timeline.HasExtendBoneName(slotNo, extendBoneName);

                view.DrawToggle(entity.boneName, enabled, -1, 20, newValue =>
                {
                    if (newValue)
                    {
                        timeline.AddExtendBoneName(slotNo, extendBoneName);
                    }
                    else
                    {
                        timeline.RemoveExtendBoneName(slotNo, extendBoneName);
                    }
                });

                _isExtendBoneAllEnabled &= enabled;
            }

            view.SetEnabled(!view.IsComboBoxFocused());
            view.EndScrollView();
        }

        public override ITransformData CreateTransformData(string name)
        {
            ITransformData transform;

            var holdtype = MaidCache.GetIKHoldType(name);
            var isDefaultBoneName = BoneUtils.IsDefaultBoneName(name);
            if (holdtype != IKHoldType.Max)
            {
                transform = new TransformDataIKHold();
            }
            else if (name == GroundingBoneName)
            {
                transform = new TransformDataGrounding();
            }
            else if (name == "Bip01")
            {
                transform = new TransformDataRoot();
            }
            else if (isDefaultBoneName)
            {
                transform = new TransformDataRotation();
            }
            else
            {
                transform = new TransformDataExtendBone();
            }
            transform.Initialize(name);
            return transform;
        }
    }
}