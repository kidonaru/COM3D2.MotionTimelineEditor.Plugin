using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using UnityEngine;

namespace COM3D2.MotionTimelineEditor.Plugin
{
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

    [TimelineLayerDesc("メイドアニメ", 0)]
    public class MotionTimelineLayer : TimelineLayerBase
    {
        public override Type layerType => typeof(MotionTimelineLayer);
        public override string layerName => nameof(MotionTimelineLayer);

        public override bool hasSlotNo => true;

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
                    _allBoneNames.AddRange(FingerBlendBoneNames);
                }
                return _allBoneNames;
            }
        }

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

            if (maidCache == null)
            {
                return;
            }

            maidCache.ResetIkHoldEntities();
            maidCache.ResetGrounding();

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

            var fingerBlendSetMenuItem = new BoneSetMenuItem("FingerBlend", "指ブレンド");
            allMenuItems.Add(fingerBlendSetMenuItem);

            foreach (var boneName in FingerBlendBoneNames)
            {
                var blendType = ConvertToFingerBlendType(boneName);
                var blendName = FingerBrendNames[(int)blendType];
                var menuItem = new BoneMenuItem(boneName, blendName);
                fingerBlendSetMenuItem.AddChild(menuItem);
            }
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

            if (!studioHackManager.isPoseEditing)
            {
                ApplyPlayData();
            }
        }

        protected override void ApplyPlayData()
        {
            var maid = this.maid;
            if (maid == null || maid.body0 == null || !maid.body0.isLoadedBody)
            {
                return;
            }

            ApplyPlayDataByType(TransformType.ExtendBone);
            ApplyPlayDataByType(TransformType.IKHold);
            ApplyPlayDataByType(TransformType.Grounding);
            ApplyPlayDataByType(TransformType.FingerBlend);
        }

        protected override void ApplyMotion(MotionData motion, float t, bool indexUpdated)
        {
            switch (motion.start.type)
            {
                case TransformType.ExtendBone:
                    ApplyExtendBoneMotion(motion, t);
                    break;
                case TransformType.IKHold:
                    ApplyIKHoldMotion(motion, t);
                    break;
                case TransformType.Grounding:
                    ApplyGroundingMotion(motion);
                    break;
                case TransformType.FingerBlend:
                    ApplyFingerBlendMotion(motion);
                    break;
            }
        }

        private void ApplyExtendBoneMotion(MotionData motion, float t)
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

            var start = motion.start;
            var end = motion.end;

            var t0 = motion.stFrame * timeline.frameDuration;
            var t1 = motion.edFrame * timeline.frameDuration;

            bone.localPosition = PluginUtils.HermiteVector3(
                t0,
                t1,
                start.positionValues,
                end.positionValues,
                t);

            bone.localScale = PluginUtils.HermiteVector3(
                t0,
                t1,
                start.scaleValues,
                end.scaleValues,
                t);
        }

        private void ApplyIKHoldMotion(MotionData motion, float t)
        {
            if (maidCache == null)
            {
                return;
            }

            var boneName = motion.name;

            if (!MaidCache.ikHoldTypeMap.ContainsKey(boneName))
            {
                return;
            }

            var ikHoldEntity = maidCache.GetIKHoldEntity(boneName);
            if (ikHoldEntity == null)
            {
                return;
            }

            var start = motion.start as TransformDataIKHold;
            var end = motion.end as TransformDataIKHold;

            var t0 = motion.stFrame * timeline.frameDuration;
            var t1 = motion.edFrame * timeline.frameDuration;

            ikHoldEntity.isHold = start.isHold;
            ikHoldEntity.isAnime = start.isAnime;

            ikHoldEntity.targetPosition = PluginUtils.HermiteVector3(
                t0,
                t1,
                start.positionValues,
                end.positionValues,
                t);
        }

        private void ApplyGroundingMotion(MotionData motion)
        {
            if (maidCache == null)
            {
                return;
            }

            var start = motion.start as TransformDataGrounding;

            maidCache.isGroundingFootL = start.isGroundingFootL;
            maidCache.isGroundingFootR = start.isGroundingFootR;
            maidCache.floorHeight = start.floorHeight;
            maidCache.footBaseOffset = start.footBaseOffset;
            maidCache.footStretchHeight = start.footStretchHeight;
            maidCache.footStretchAngle = start.footStretchAngle;
            maidCache.footGroundAngle = start.footGroundAngle;
        }

        private void ApplyFingerBlendMotion(MotionData motion)
        {
            // 指ブレンドはポーズ編集中のみ反映
            if (!studioHackManager.isPoseEditing)
            {
                return;
            }

            var boneName = motion.name;
            var blendType = ConvertToFingerBlendType(boneName);
            var trans = motion.start as TransformDataFingerBlend;

            //PluginUtils.LogDebug("ApplyFingerBlendMotion: type={0} stFrame={1}", motion.row.type, motion.stFrame);

            switch (blendType)
            {
                case WindowPartsFingerBlend.Type.RightArm:
                case WindowPartsFingerBlend.Type.LeftArm:
                    trans.ApplyArmFinger(GetArmFinger(blendType));
                    break;
                case WindowPartsFingerBlend.Type.RightLeg:
                case WindowPartsFingerBlend.Type.LeftLeg:
                    trans.ApplyLegFinger(GetLegFinger(blendType));
                    break;
            }
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

            foreach (var name in BoneUtils.saveBoneNames)
            {
                var transform = maidCache.GetBoneTransform(name);
                if (transform == null)
                {
                    PluginUtils.LogDebug("UpdateFrame: ボーンがないのでスキップしました name={0}", name);
                    continue;
                }

                ITransformData trans;
                if (name == "Bip01")
                {
                    trans = CreateTransformData<TransformDataRoot>(name);
                }
                else
                {
                    trans = CreateTransformData<TransformDataRotation>(name);
                }

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

                var bone = frame.CreateBone(trans);
                frame.UpdateBone(bone);
            }

            foreach (var name in timeline.GetExtendBoneNames(slotNo))
            {
                var transform = maidCache.GetBoneTransform(name);
                if (transform == null)
                {
                    PluginUtils.LogDebug("UpdateFrame: ボーンがないのでスキップしました name={0}", name);
                    continue;
                }

                var trans = CreateTransformData<TransformDataExtendBone>(name);
                trans.position = transform.localPosition;
                trans.rotation = transform.localRotation;
                trans.scale = transform.localScale;

                var bone = frame.CreateBone(trans);
                frame.UpdateBone(bone);
            }

            foreach (var name in MaidCache.ikHoldTypeMap.Keys)
            {
                var ikHoldEntity = maidCache.GetIKHoldEntity(name);
                if (ikHoldEntity == null)
                {
                    continue;
                }

                var trans = CreateTransformData<TransformDataIKHold>(name);
                trans.position = ikHoldEntity.position;
                trans.isHold = ikHoldEntity.isHold;
                trans.isAnime = ikHoldEntity.isAnime;

                var bone = frame.CreateBone(trans);
                frame.UpdateBone(bone);
            }

            {
                var name = GroundingBoneName;

                var trans = CreateTransformData<TransformDataGrounding>(name);
                trans.isGroundingFootL = maidCache.isGroundingFootL;
                trans.isGroundingFootR = maidCache.isGroundingFootR;
                trans.floorHeight = maidCache.floorHeight;
                trans.footBaseOffset = maidCache.footBaseOffset;
                trans.footStretchHeight = maidCache.footStretchHeight;
                trans.footStretchAngle = maidCache.footStretchAngle;
                trans.footGroundAngle = maidCache.footGroundAngle;

                var bone = frame.CreateBone(trans);
                frame.UpdateBone(bone);
            }

            foreach (var name in FingerBlendBoneNames)
            {
                var blendType = ConvertToFingerBlendType(name);

                if (blendType == WindowPartsFingerBlend.Type.RightArm ||
                    blendType == WindowPartsFingerBlend.Type.LeftArm)
                {
                    var fingerBlend = GetBaseFinger(blendType) as FingerBlend.ArmFinger;
                    if (fingerBlend == null)
                    {
                        continue;
                    }

                    var trans = CreateTransformData<TransformDataFingerBlend>(name);
                    trans.UpdateFromArmFinger(fingerBlend);

                    var bone = frame.CreateBone(trans);
                    frame.UpdateBone(bone);
                }
                else
                {
                    var fingerBlend = GetBaseFinger(blendType) as FingerBlend.LegFinger;
                    if (fingerBlend == null)
                    {
                        continue;
                    }

                    var trans = CreateTransformData<TransformDataFingerBlend>(name);
                    trans.UpdateFromLegFinger(fingerBlend);

                    var bone = frame.CreateBone(trans);
                    frame.UpdateBone(bone);
                }
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

            var stopwatch = new StopwatchDebug();
            ApplyPlayData();
            stopwatch.ProcessEnd("  ApplyPlayData: " + layerName);
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
                var stopwatch = new StopwatchDebug();
                ApplyPlayData();
                stopwatch.ProcessEnd("  ApplyPlayData: " + layerName);
            }
        }

        public override void OutputAnm()
        {
            try
            {
                var anmData = GetAnmBinary(true);
                if (anmData == null)
                {
                    PluginUtils.LogError("モーションの出力に失敗しました");
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

        List<float> _timesCache = new List<float>(128);
        List<ValueData[]> _valuesListCache = new List<ValueData[]>(128);

        protected override byte[] GetAnmBinaryInternal(bool forOutput, int startFrameNo, int endFrameNo)
        {
            if (maidCache == null)
            {
                return null;
            }

            var startSecond = timeline.GetFrameTimeSeconds(startFrameNo);
            var endSecond = timeline.GetFrameTimeSeconds(endFrameNo);

            int _startFrameNo = startFrameNo;
            int _endFrameNo = endFrameNo;
            Action<BinaryWriter, List<BoneData>> write_bones = delegate (
                BinaryWriter w,
                List<BoneData> bones)
            {
                if (bones.Count == 0)
                {
                    return;
                }

                var firstBone = bones[0];
                var name = firstBone.name;
                var path = maidCache.GetBonePath(name);
                if (string.IsNullOrEmpty(path))
                {
                    PluginUtils.LogWarning("ボーンがないのでスキップしました boneName={0}", name);
                    return;
                }

                w.Write((byte)1);
                w.Write(path);

                _timesCache.Clear();
                _valuesListCache.Clear();

                BoneData prevBone = null;

                foreach (var bone in bones)
                {
                    if (bone.frameNo < _startFrameNo)
                    {
                        prevBone = bone;
                        continue;
                    }

                    // 開始フレームにキーフレームがない場合は前のキーフレームを使う
                    if (_timesCache.Count == 0 && bone.frameNo != _startFrameNo && prevBone != null)
                    {
                        _timesCache.Add(0f);
                        _valuesListCache.Add(prevBone.transform.values);
                    }

                    if (bone.frameNo > _endFrameNo)
                    {
                        // 終了フレームにキーフレームがない場合は後のキーフレームを使う
                        if (prevBone != null && prevBone.frameNo != _endFrameNo)
                        {
                            _timesCache.Add(endSecond - startSecond);
                            _valuesListCache.Add(bone.transform.values);
                        }
                        break;
                    }

                    _timesCache.Add(timeline.GetFrameTimeSeconds(bone.frameNo) - startSecond);
                    _valuesListCache.Add(bone.transform.values);
                    prevBone = bone;
                }

                for (int i = 0; i < firstBone.transform.valueCount; i++)
                {
                    w.Write((byte)(100 + i));
                    w.Write(_timesCache.Count);
                    for (int j = 0; j < _timesCache.Count; j++)
                    {
                        w.Write(_timesCache[j]);
                        w.Write(_valuesListCache[j][i].value);
                        w.Write(_valuesListCache[j][i].inTangent.value);
                        w.Write(_valuesListCache[j][i].outTangent.value);
                    }
                }
            };

            MemoryStream memoryStream = new MemoryStream();
            BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
            binaryWriter.Write("CM3D2_ANIM");
            binaryWriter.Write(1001);

            foreach (var bones in _timelineBonesMap.Values)
            {
                if (bones.Count == 0)
                {
                    continue;
                }

                switch (bones[0].transform.type)
                {
                    case TransformType.Root:
                    case TransformType.Rotation:
                    case TransformType.ExtendBone:
                        write_bones(binaryWriter, bones);
                        break;
                }
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

            return result;
        }

        public void OutputPoseCsv(
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
                        PluginUtils.LogError("モーションの出力に失敗しました");
                        return;
                    }

                    var anmPath = timeline.GetDcmSongFilePath(anmFileName);

                    bool isExist = File.Exists(anmPath);
                    File.WriteAllBytes(anmPath, anmData);

                    var maidElement = GetMeidElement(songElement);
                    //maidElement.Add(new XElement("customAnimation", anmFileName));
                }

                {
                    var outputRows = new List<PoseTimeLineRow>(64);

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
                    outputRows.Add(row);

                    var outputFileName = string.Format("pose_{0}.csv", slotNo);
                    var outputPath = timeline.GetDcmSongFilePath(outputFileName);
                    OutputPoseCsv(outputRows, outputPath);

                    var maidElement = GetMeidElement(songElement);
                    maidElement.Add(new XElement("pose", outputFileName));
                }
            }
            catch (Exception e)
            {
                PluginUtils.LogException(e);
                PluginUtils.LogError("モーションの出力に失敗しました");
            }
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

                    view.DrawToggle("ブレンド有効", timeline.fingerBlendEnabled, -1, 20, newValue =>
                    {
                        timeline.fingerBlendEnabled = newValue;
                    });

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

                    view.DrawToggle("ブレンド有効", timeline.fingerBlendEnabled, -1, 20, newValue =>
                    {
                        timeline.fingerBlendEnabled = newValue;
                    });

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

            view.SetEnabled(!view.IsComboBoxFocused() && studioHackManager.isPoseEditing);

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
            var updateTransform = DrawPosition(
                view,
                transformCache,
                editType,
                initialPosition);

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

        public static readonly string[] FingerBlendBoneNames = new string[]
        {
            "ArmFingerBlendR",
            "ArmFingerBlendL",
            "LegFingerBlendR",
            "LegFingerBlendL",
        };

        public static readonly Dictionary<string, WindowPartsFingerBlend.Type> FingerBlendBoneTypeMap =
            new Dictionary<string, WindowPartsFingerBlend.Type>
            {
                { "ArmFingerBlendR", WindowPartsFingerBlend.Type.RightArm },
                { "ArmFingerBlendL", WindowPartsFingerBlend.Type.LeftArm },
                { "LegFingerBlendR", WindowPartsFingerBlend.Type.RightLeg },
                { "LegFingerBlendL", WindowPartsFingerBlend.Type.LeftLeg },
            };

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

        private static WindowPartsFingerBlend.Type ConvertToFingerBlendType(string boneName)
        {
            WindowPartsFingerBlend.Type type;
            if (FingerBlendBoneTypeMap.TryGetValue(boneName, out type))
            {
                return type;
            }

            PluginUtils.LogError("ConvertToFingerBlendType: 不明なボーン名です boneName={0}", boneName);
            return WindowPartsFingerBlend.Type.RightArm;
        }

        private static bool IsFingerBlendBone(string boneName)
        {
            return FingerBlendBoneTypeMap.ContainsKey(boneName);
        }

        private FingerBlend.BaseFinger GetBaseFinger(WindowPartsFingerBlend.Type type)
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

            if (result.enabled != timeline.fingerBlendEnabled)
            {
                result.SetEnabledOnly(timeline.fingerBlendEnabled);
            }

            return result;
        }

        private FingerBlend.ArmFinger GetArmFinger(WindowPartsFingerBlend.Type type)
        {
            return GetBaseFinger(type) as FingerBlend.ArmFinger;
        }

        private FingerBlend.LegFinger GetLegFinger(WindowPartsFingerBlend.Type type)
        {
            return GetBaseFinger(type) as FingerBlend.LegFinger;
        }

        public void DrawFingerBlend(
            GUIView view,
            string[] slotNames,
            WindowPartsFingerBlend.Type blendType,
            WindowPartsFingerBlend.Type otherBlendType)
        {
            var baseFinger = GetBaseFinger(blendType);

            view.SetEnabled(!view.IsComboBoxFocused() && studioHackManager.isPoseEditing && timeline.fingerBlendEnabled);

            view.BeginHorizontal();
            {
                view.DrawLabel(FingerBrendNames[(int)blendType], 40, 20);

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
                var otherBaseFinger = GetBaseFinger(otherBlendType);
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

        public override SingleFrameType GetSingleFrameType(TransformType transformType)
        {
            switch (transformType)
            {
                case TransformType.Grounding:
                case TransformType.FingerBlend:
                    return SingleFrameType.None;
            }

            return base.GetSingleFrameType(transformType);
        }

        private static Dictionary<string, TransformType> _transformTypeCache = new Dictionary<string, TransformType>();

        public override TransformType GetTransformType(string name)
        {
            TransformType type;
            if (_transformTypeCache.TryGetValue(name, out type))
            {
                return type;
            }

            type = GetTransformTypeInternal(name);
            _transformTypeCache[name] = type;
            return type;
        }

        private TransformType GetTransformTypeInternal(string name)
        {
            var holdtype = MaidCache.GetIKHoldType(name);
            if (holdtype != IKHoldType.Max)
            {
                return TransformType.IKHold;
            }
            else if (name == GroundingBoneName)
            {
                return TransformType.Grounding;
            }
            else if (name == "Bip01")
            {
                return TransformType.Root;
            }
            else if (BoneUtils.IsDefaultBoneName(name))
            {
                return TransformType.Rotation;
            }
            else if (IsFingerBlendBone(name))
            {
                return TransformType.FingerBlend;
            }
            else
            {
                return TransformType.ExtendBone;
            }
        }
    }
}