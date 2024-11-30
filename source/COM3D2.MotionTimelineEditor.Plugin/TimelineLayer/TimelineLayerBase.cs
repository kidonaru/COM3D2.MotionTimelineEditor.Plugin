using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using UnityEngine;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    public abstract partial class TimelineLayerBase : ITimelineLayer
    {
        public static readonly long TimelineAnmId = 26925014;

        public static Func<float, int, float> EasingFunction { get; set; }

        public abstract string className { get; }
        public int slotNo { get; protected set; }
        public virtual bool hasSlotNo => false;
        public virtual bool isCameraLayer => false;
        public virtual bool isPostEffectLayer => false;

        protected List<FrameData> _keyFrames = new List<FrameData>();
        public List<FrameData> keyFrames => _keyFrames.ToList();

        public MaidCache maidCache => maidManager.GetMaidCache(slotNo);

        public Maid maid
        {
            get
            {
                var cache = maidCache;
                if (cache != null)
                {
                    return cache.maid;
                }
                return null;
            }
        }

        public int playingFrameNo
        {
            get
            {
                var cache = maidCache;
                if (cache != null)
                {
                    return cache.playingFrameNo;
                }
                return 0;
            }
        }

        public float playingFrameNoFloat
        {
            get
            {
                var cache = maidCache;
                if (cache != null)
                {
                    return cache.playingFrameNoFloat;
                }
                return 0;
            }
        }

        public float playingTime => playingFrameNoFloat * timeline.frameDuration;

        public bool isMotionPlaying
        {
            get
            {
                var cache = maidCache;
                if (cache != null)
                {
                    return cache.isMotionPlaying;
                }
                return false;
            }
            set
            {
                var cache = maidCache;
                if (cache != null)
                {
                    cache.isMotionPlaying = value;
                }

                if (isCurrent)
                {
                    studioHack.isMotionPlaying = value;
                }
            }
        }

        public float anmSpeed
        {
            get
            {
                var cache = maidCache;
                if (cache != null)
                {
                    return cache.anmSpeed;
                }
                return 0;
            }
        }

        public long anmId
        {
            get
            {
                var cache = maidCache;
                if (cache != null)
                {
                    return cache.anmId;
                }
                return 0;
            }
        }

        // アニメーションと同期しているか
        public bool isAnmSyncing
        {
            get
            {
                var cache = maidCache;
                if (cache != null)
                {
                    return cache.isAnmSyncing;
                }
                return false;
            }
        }

        // タイムラインアニメーションを再生中か
        public bool isAnmPlaying
        {
            get
            {
                var cache = maidCache;
                if (cache != null)
                {
                    return cache.isAnmPlaying;
                }
                return false;
            }
        }

        public virtual bool isDragging => false;

        public abstract List<string> allBoneNames { get; }

        public string errorMessage { get; protected set; }

        public bool isCurrent => timelineManager.currentLayer == this;

        public int maxExistFrameNo
        {
            get
            {
                if (_keyFrames.Count == 0)
                {
                    return 0;
                }
                return _keyFrames[_keyFrames.Count - 1].frameNo;
            }
        }

        public FrameData firstFrame => _keyFrames.Count > 0 ? _keyFrames[0] : null;

        public string anmFileName
        {
            get
            {
                var suffix = slotNo > 0 ? "_" + slotNo : "";
                return timeline.anmName + suffix + ".anm";
            }
        }

        public string anmPath => studioHack.outputAnmPath + "\\" + anmFileName;

        public List<IBoneMenuItem> _allMenuItems = new List<IBoneMenuItem>();
        public List<IBoneMenuItem> allMenuItems => _allMenuItems;

        // ループ補正用の最終フレーム
        protected FrameData _dummyLastFrame = null;

        public Dictionary<string, List<BoneData>> timelineRowsMap => _timelineBonesMap;
        protected Dictionary<string, List<BoneData>> _timelineBonesMap = new Dictionary<string, List<BoneData>>(32);

        protected Dictionary<string, MotionPlayData> _playDataMap = new Dictionary<string, MotionPlayData>(32);

        protected static TimelineManager timelineManager => TimelineManager.instance;

        protected static TimelineData timeline => timelineManager.timeline;

        protected static ITimelineLayer currentLayer => timelineManager.currentLayer;

        protected static ITimelineLayer defaultLayer => timeline.defaultLayer;

        protected static Config config => ConfigManager.config;

        protected static int maxFrameNo => timeline.maxFrameNo;

        protected static bool useMuneKeyL => timeline.useMuneKeyL;

        protected static bool useMuneKeyR => timeline.useMuneKeyR;

        protected static bool isLoopAnm => timeline.isLoopAnm;

        protected static MaidManager maidManager => MaidManager.instance;
        protected static StudioModelManager modelManager => StudioModelManager.instance;
        protected static BGModelManager bgModelManager => BGModelManager.instance;
        protected static ModelHackManager modelHackManager => ModelHackManager.instance;
        protected static StudioLightManager lightManager => StudioLightManager.instance;
        protected static StageLightManager stageLightManager => StageLightManager.instance;
        protected static StudioHackBase studioHack => StudioHackManager.studioHack;
        protected static PhotoBGManager photoBGManager => PhotoBGManager.instance;
        protected static TimelineBundleManager bundleManager => TimelineBundleManager.instance;

        protected TimelineLayerBase(int slotNo)
        {
            PluginUtils.LogDebug("{0}.Create slotNo={1}", className, slotNo);
            this.slotNo = slotNo;
        }

        public virtual void Init()
        {
            if (firstFrame == null)
            {
                var frame = GetOrCreateFrame(0);
                UpdateFrame(frame);
            }
            if (_dummyLastFrame == null)
            {
                _dummyLastFrame = CreateFrame(0);
            }

            InitMenuItems();
        }

        protected abstract void InitMenuItems();

        public virtual void Dispose()
        {
            _keyFrames.Clear();
            _dummyLastFrame = null;
        }

        public abstract bool IsValidData();

        public virtual void Update()
        {
            // do nothing
        }

        public virtual void LateUpdate()
        {
            // do nothing
        }

        public virtual void OnActive()
        {
            // do nothing
        }

        public virtual void OnSave()
        {
            // do nothing
        }

        public virtual void OnLoad()
        {
            // do nothing
        }

        public virtual void OnEndPoseEdit()
        {
            // do nothing
        }

        public virtual void OnPluginEnable()
        {
            // do nothing
        }

        public virtual void OnPluginDisable()
        {
            // do nothing
        }

        public virtual void OnMaidChanged(Maid maid)
        {
            // do nothing
        }

        public virtual void OnModelAdded(StudioModelStat model)
        {
            // do nothing
        }

        public virtual void OnModelRemoved(StudioModelStat model)
        {
            // do nothing
        }

        public virtual void OnCopyModel(StudioModelStat sourceModel, StudioModelStat newModel)
        {
            // do nothing
        }

        public virtual void OnLightAdded(StudioLightStat light)
        {
            // do nothing
        }

        public virtual void OnLightRemoved(StudioLightStat light)
        {
            // do nothing
        }

        public virtual void OnLightUpdated(StudioLightStat light)
        {
            // do nothing
        }

        public virtual void OnCopyLight(StudioLightStat sourceLight, StudioLightStat newLight)
        {
            // do nothing
        }

        public virtual void OnShapeKeyAdded(string shapeKey)
        {
            // do nothing
        }

        public virtual void OnShapeKeyRemoved(string shapeKey)
        {
            // do nothing
        }

        public virtual void OnBoneNameAdded(string extendBoneName)
        {
            // do nothing
        }

        public virtual void OnBoneNameRemoved(string extendBoneName)
        {
            // do nothing
        }

        public abstract void UpdateFrame(FrameData frame);

        public virtual void ApplyAnm(long id, byte[] anmData)
        {
            ApplyPlayData();
        }

        public virtual void ApplyCurrentFrame(bool motionUpdate)
        {
            if (anmId != TimelineAnmId || motionUpdate)
            {
                CreateAndApplyAnm();
            }
            else
            {
                ApplyPlayData();
            }
        }

        protected virtual void ApplyPlayData()
        {
            var maid = this.maid;
            if (maid == null || maid.body0 == null || !maid.body0.isLoadedBody)
            {
                return;
            }

            var playingFrameNoFloat = this.playingFrameNoFloat;

            foreach (var playData in _playDataMap.Values)
            {
                var indexUpdated = playData.Update(playingFrameNoFloat);

                var current = playData.current;
                if (current != null)
                {
                    ApplyMotion(current, playData.lerpFrame, indexUpdated);
                }
            }
        }

        protected abstract void ApplyMotion(MotionData motion, float t, bool indexUpdated);

        protected virtual void BuildPlayData()
        {
            _playDataMap.ClearPlayData();

            foreach (var pair in _timelineBonesMap)
            {
                var name = pair.Key;
                var rows = pair.Value;

                if (rows.Count == 0)
                {
                    continue;
                }

                MotionPlayData playData;
                if (!_playDataMap.TryGetValue(name, out playData))
                {
                    playData = new MotionPlayData(rows.Count);
                    _playDataMap[name] = playData;
                }

                for (var i = 0; i < rows.Count - 1; i++)
                {
                    var start = rows[i];
                    var end = rows[i + 1];
                    playData.motions.Add(new MotionData(start, end));
                }

                var bone = rows[0];
                var singleFrameType = GetSingleFrameType(bone.transform.type);
                playData.Setup(singleFrameType);
            }
        }

        public virtual void OutputAnm()
        {
            // do nothing
        }

        public virtual void OutputDCM(XElement songElement)
        {
            PluginUtils.LogWarning("{0}はDCMに対応していません", className);
        }

        protected virtual void AppendTimelineRow(FrameData frame)
        {
            var isLastFrame = frame.frameNo == maxFrameNo;
            foreach (var bone in frame.bones)
            {
                _timelineBonesMap.AppendBone(bone, isLastFrame);
            }
        }

        protected virtual void BuildTimelineBonesMap()
        {
            _timelineBonesMap.ClearBones();

            foreach (var keyFrame in keyFrames)
            {
                AppendTimelineRow(keyFrame);
            }

            UpdateDummyLastFrame();

            AppendTimelineRow(_dummyLastFrame);
        }

        protected virtual byte[] GetAnmBinaryInternal(bool forOutput, int startFrameNo, int endFrameNo)
        {
            return null;
        }

        public float CalcEasingValue(float t, int easing)
        {
            if (EasingFunction != null)
            {
                return EasingFunction(t, easing);
            }

            return t;
        }

        public virtual void ResetDraw(GUIView view)
        {
            // do nothing
        }

        public virtual void DrawWindow(GUIView view)
        {
            // do nothing
        }

        public void AddKeyFrameAll()
        {
            studioHack.isMotionPlaying = false;

            var maid = this.maid;
            if (maid == null)
            {
                return;
            }

            var frame = GetOrCreateFrame(timelineManager.currentFrameNo);
            UpdateFrame(frame);

            ApplyCurrentFrame(true);

            timelineManager.RequestHistory("キーフレーム全登録");
        }

        public void AddKeyFrameDiff()
        {
            if (timelineManager.initialEditFrame == null)
            {
                PluginUtils.Log("編集モード中のみキーフレームの登録ができます");
                return;
            }

            var maid = this.maid;
            if (maid == null)
            {
                PluginUtils.LogError("メイドが配置されていません");
                return;
            }

            var tmpFrame = CreateFrame(timelineManager.currentFrameNo);
            UpdateFrame(tmpFrame);

            var diffBones = tmpFrame.GetDiffBones(timelineManager.initialEditFrame);
            if (diffBones.Count == 0)
            {
                PluginUtils.Log("変更がないのでキーフレームの登録をスキップしました");
                return;
            }

            UpdateBones(timelineManager.currentFrameNo, diffBones);

            ApplyCurrentFrame(true);

            timelineManager.RequestHistory("キーフレーム登録");
        }

        public void AddKeyFrames(IEnumerable<string> boneNames)
        {
            if (timelineManager.initialEditFrame == null)
            {
                PluginUtils.Log("編集モード中のみキーフレームの登録ができます");
                return;
            }

            var maid = this.maid;
            if (maid == null)
            {
                PluginUtils.LogError("メイドが配置されていません");
                return;
            }

            var tmpFrame = CreateFrame(timelineManager.currentFrameNo);
            UpdateFrame(tmpFrame);

            var filterBones = tmpFrame.GetFilterBones(boneNames);
            if (filterBones.Count == 0)
            {
                PluginUtils.Log("対象のキーフレームがありません");
                return;
            }

            UpdateBones(timelineManager.currentFrameNo, filterBones);

            ApplyCurrentFrame(true);

            timelineManager.RequestHistory("キーフレーム登録");
        }

        public void RemoveKeyFrames(IEnumerable<string> boneNames)
        {
            var frame = GetFrame(timelineManager.currentFrameNo);
            if (frame == null)
            {
                PluginUtils.LogWarning("削除するフレームがありません");
                return;
            }

            var filterBones = frame.GetFilterBones(boneNames);
            if (filterBones.Count == 0)
            {
                PluginUtils.LogWarning("対象のキーフレームがありません");
                return;
            }

            frame.RemoveBones(filterBones);

            CleanFrames();

            ApplyCurrentFrame(true);

            timelineManager.RequestHistory("キーフレーム削除");
        }

        protected void FixRotation(int startFrameNo, int endFrameNo)
        {
            foreach (var bones in _timelineBonesMap.Values)
            {
                if (bones.Count <= 1)
                {
                    continue;
                }

                foreach (var bone in bones)
                {
                    if (bone.frameNo <= startFrameNo || bone.frameNo > endFrameNo)
                    {
                        continue;
                    }

                    if (bone.transform.hasRotation)
                    {
                        var prevBone = GetPrevBone2(bone.frameNo, bones);
                        if (prevBone != null)
                        {
                            bone.transform.FixRotation(prevBone.transform);
                        }
                    }
                    if (bone.transform.hasEulerAngles)
                    {
                        var prevBone = GetPrevBone2(bone.frameNo, bones);
                        if (prevBone != null)
                        {
                            bone.transform.FixEulerAngles(prevBone.transform);
                        }
                    }
                }
            }
        }

        protected void UpdateTangent(int startFrameNo, int endFrameNo)
        {
            foreach (var bones in _timelineBonesMap.Values)
            {
                if (bones.Count <= 1)
                {
                    continue;
                }

                foreach (var bone in bones)
                {
                    if (bone.frameNo < startFrameNo || bone.frameNo > endFrameNo)
                    {
                        continue;
                    }

                    if (!bone.transform.hasTangent)
                    {
                        continue;
                    }

                    int prevFrameNo;
                    var prevBone = GetPrevBone2(bone.frameNo, bones, out prevFrameNo);

                    int nextFrameNo;
                    var nextBone = GetNextBone2(bone.frameNo, bones, out nextFrameNo);

                    // 前後に存在しない場合は自身を使用
                    if (prevBone == null)
                    {
                        prevBone = bone;
                        prevFrameNo = bone.frameNo - 1;
                    }
                    if (nextBone == null)
                    {
                        nextBone = bone;
                        nextFrameNo = bone.frameNo + 1;
                    }

                    // 1フレーム補間が有効な場合は自身を使用
                    if (bone.transform.singleFrameType == SingleFrameType.Delay ||
                        bone.transform.singleFrameType == SingleFrameType.Advance)
                    {
                        if (bone.frameNo - prevFrameNo == 1)
                        {
                            prevBone = bone;
                        }
                        if (nextFrameNo - bone.frameNo == 1)
                        {
                            nextBone = bone;
                        }
                    }

                    var prevTrans = prevBone.transform;
                    var nextTrans = nextBone.transform;

                    // 別ループのキーフレームは回転補正を行う
                    if (prevFrameNo != prevBone.frameNo)
                    {
                        prevTrans = CreateTransformData(prevTrans);
                        prevTrans.FixRotation(bone.transform);
                    }
                    if (nextFrameNo != nextBone.frameNo)
                    {
                        nextTrans = CreateTransformData(nextTrans);
                        nextTrans.FixRotation(bone.transform);
                    }

                    var prevTime = timeline.GetFrameTimeSeconds(prevFrameNo);
                    var currentTime = timeline.GetFrameTimeSeconds(bone.frameNo);
                    var nextTime = timeline.GetFrameTimeSeconds(nextFrameNo);

                    bone.transform.UpdateTangent(
                        prevTrans,
                        nextTrans,
                        prevTime,
                        currentTime,
                        nextTime);
                }
            }
        }

        protected void UpdateDummyLastFrame()
        {
            _dummyLastFrame.frameNo = maxFrameNo;

            foreach (var bones in _timelineBonesMap.Values)
            {
                if (bones.Count == 0)
                {
                    continue;
                }

                BoneData sourceBone;
                if (isLoopAnm)
                {
                    sourceBone = GetNextBone2(-1, bones);
                }
                else
                {
                    sourceBone = GetPrevBone2(maxFrameNo, bones);
                }

                if (sourceBone != null)
                {
                    _dummyLastFrame.UpdateBone(sourceBone);
                }
            }
        }

        public byte[] GetAnmBinary(bool forOutput)
        {
            if (!IsValidData())
            {
                return null;
            }

            PluginUtils.LogDebug(className);

            var stopwatch = new StopwatchDebug();

            BuildTimelineBonesMap();
            stopwatch.ProcessEnd("  BuildTimelineBonesMap");

            var startFrameNo = 0;
            var endFrameNo = timeline.maxFrameNo;
            var activeTrack = timeline.activeTrack;

            if (activeTrack != null && !forOutput)
            {
                startFrameNo = GetStartFrameNo(activeTrack.startFrameNo);
                endFrameNo = GetEndFrameNo(activeTrack.endFrameNo);

                if (config.outputElapsedTime)
                {
                    PluginUtils.Log("  slotNo={0} startFrameNo={1}, endFrameNo={2} ",
                            slotNo, startFrameNo, endFrameNo);
                }
            }

            FixRotation(startFrameNo, endFrameNo);
            stopwatch.ProcessEnd("  FixRotation");

            UpdateTangent(startFrameNo, endFrameNo);
            stopwatch.ProcessEnd("  UpdateTangent");

            BuildPlayData();
            stopwatch.ProcessEnd("  BuildPlayData");

            var anmData = GetAnmBinaryInternal(forOutput, startFrameNo, endFrameNo);
            stopwatch.ProcessEnd("  GetAnmBinary");

            return anmData;
        }

        public void CreateAndApplyAnm()
        {
            var anmData = GetAnmBinary(false);
            ApplyAnm(TimelineAnmId, anmData);
        }

        public virtual SingleFrameType GetSingleFrameType(TransformType transformType)
        {
            return timeline.singleFrameType;
        }

        public abstract TransformType GetTransformType(string name);

        public T CreateTransformData<T>(string name)
            where T : class, ITransformData, new()
        {
            return TimelineManager.CreateTransform<T>(name);
        }

        public ITransformData CreateTransformData(ITransformData transform)
        {
            var type = transform.type;
            if (type == TransformType.None)
            {
                type = GetTransformType(transform.name);
            }

            var newTransform = timelineManager.CreateTransform(type, transform.name);
            newTransform.FromTransformData(transform);
            return newTransform;
        }

        public ITransformData CreateTransformData(TransformXml xml)
        {
            var type = xml.type;
            if (type == TransformType.None)
            {
                type = GetTransformType(xml.name);
            }

            var newTransform = timelineManager.CreateTransform(type, xml.name);
            newTransform.FromXml(xml);
            return newTransform;
        }

        public FrameData CreateFrame(int frameNo)
        {
            return new FrameData(this, frameNo);
        }

        public FrameData CreateFrame(FrameXml xml)
        {
            var frame = new FrameData(this);
            frame.FromXml(xml);
            return frame;
        }

        public FrameData GetFrame(int frameNo)
        {
            foreach (var frame in _keyFrames)
            {
                if (frame.frameNo == frameNo)
                {
                    return frame;
                }
            }
            return null;
        }

        public FrameData GetOrCreateFrame(int frameNo)
        {
            var frame = GetFrame(frameNo);
            if (frame != null)
            {
                return frame;
            }

            frame = CreateFrame(frameNo);
            _keyFrames.Add(frame);
            _keyFrames.Sort((a, b) => a.frameNo - b.frameNo);
            return frame;
        }

        public void SetBone(int frameNo, BoneData bone)
        {
            var frame = GetOrCreateFrame(frameNo);
            frame.SetBone(bone);
        }

        public void SetBones(int frameNo, IEnumerable<BoneData> bones)
        {
            var frame = GetOrCreateFrame(frameNo);
            frame.SetBones(bones);
        }
        
        public void UpdateBone(int frameNo, BoneData bone)
        {
            var frame = GetOrCreateFrame(frameNo);
            frame.UpdateBone(bone);
        }

        public virtual void UpdateBones(int frameNo, IEnumerable<BoneData> bones)
        {
            var frame = GetOrCreateFrame(frameNo);
            frame.UpdateBones(bones);
        }

        public List<string> GetExistBoneNames()
        {
            var nameHash = new HashSet<string>();
            foreach (var frame in _keyFrames)
            {
                foreach (var bone in frame.bones)
                {
                    nameHash.Add(bone.name);
                }
            }
            return nameHash.ToList();
        }

        public void CleanFrames()
        {
            var removeFrames = new List<FrameData>();

            foreach (var key in _keyFrames)
            {
                if (!key.HasBones())
                {
                    removeFrames.Add(key);
                }
            }

            foreach (var key in removeFrames)
            {
                _keyFrames.Remove(key);
            }
        }

        public BoneData GetBone(int frameNo, string name)
        {
            var frame = GetFrame(frameNo);
            if (frame == null)
            {
                return null;
            }

            return frame.GetBone(name);
        }

        public FrameData GetPrevFrame(int frameNo)
        {
            return _keyFrames.LastOrDefault(f => f.frameNo < frameNo);
        }

        public FrameData GetNextFrame(int frameNo)
        {
            return _keyFrames.First(f => f.frameNo > frameNo);
        }

        public BoneData GetPrevBone(
            int frameNo,
            string name,
            out int prevFrameNo,
            bool loopSearch)
        {
            List<BoneData> bones;
            if (!_timelineBonesMap.TryGetValue(name, out bones))
            {
                prevFrameNo = -1;
                return null;
            }

            return GetPrevBone2(frameNo, bones, out prevFrameNo, loopSearch);
        }

        public BoneData GetPrevBone(int frameNo, string name, out int prevFrameNo)
        {
            return GetPrevBone(frameNo, name, out prevFrameNo, true);
        }

        public BoneData GetPrevBone(int frameNo, string name, bool loopSearch)
        {
            int prevFrameNo;
            return GetPrevBone(frameNo, name, out prevFrameNo, loopSearch);
        }

        public BoneData GetPrevBone(int frameNo, string name)
        {
            int prevFrameNo;
            return GetPrevBone(frameNo, name, out prevFrameNo);
        }

        public BoneData GetPrevBone(BoneData bone)
        {
            return GetPrevBone(bone.frameNo, bone.name);
        }

        public List<BoneData> GetPrevBones(IEnumerable<BoneData> bones)
        {
            var prevBones = new List<BoneData>();
            foreach (var bone in bones)
            {
                var prevBone = GetPrevBone(bone);
                if (prevBone != null)
                {
                    prevBones.Add(prevBone);
                }
            }
            return prevBones;
        }

        public BoneData GetNextBone(
            int frameNo,
            string name,
            out int nextFrameNo,
            bool loopSearch)
        {
            List<BoneData> bones;
            if (!_timelineBonesMap.TryGetValue(name, out bones))
            {
                nextFrameNo = -1;
                return null;
            }

            return GetNextBone2(frameNo, bones, out nextFrameNo, loopSearch);
        }

        public BoneData GetNextBone(int frameNo, string name, out int nextFrameNo)
        {
            return GetNextBone(frameNo, name, out nextFrameNo, true);
        }

        public BoneData GetNextBone(int frameNo, string name, bool loopSearch)
        {
            int nextFrameNo;
            return GetNextBone(frameNo, name, out nextFrameNo, loopSearch);
        }

        public BoneData GetNextBone(int frameNo, string name)
        {
            int nextFrameNo;
            return GetNextBone(frameNo, name, out nextFrameNo);
        }

        protected BoneData GetPrevBone2(
            int frameNo,
            List<BoneData> bones,
            out int prevFrameNo,
            bool loopSearch)
        {
            BoneData prevBone = null;
            prevFrameNo = -1;

            foreach (var bone in bones)
            {
                if (bone.frameNo >= frameNo)
                {
                    break;
                }

                prevBone = bone;
                prevFrameNo = bone.frameNo;
            }

            if (prevBone == null && loopSearch)
            {
                if (isLoopAnm)
                {
                    frameNo = (frameNo == 0) ? maxFrameNo : maxFrameNo + 1; // 0Fの場合は最終フレームを除外
                    prevBone = GetPrevBone2(frameNo, bones, out prevFrameNo, false);
                    prevFrameNo -= maxFrameNo;
                }
                else
                {
                    prevBone = GetNextBone2(-1, bones, out prevFrameNo, false);
                    prevFrameNo = -1;
                }
            }

            return prevBone;
        }

        public BoneData GetPrevBone2(int frameNo, List<BoneData> bones, out int prevFrameNo)
        {
            return GetPrevBone2(frameNo, bones, out prevFrameNo, true);
        }

        public BoneData GetPrevBone2(int frameNo, List<BoneData> bones, bool loopSearch)
        {
            int prevFrameNo;
            return GetPrevBone2(frameNo, bones, out prevFrameNo, loopSearch);
        }

        public BoneData GetPrevBone2(int frameNo, List<BoneData> bones)
        {
            int prevFrameNo;
            return GetPrevBone2(frameNo, bones, out prevFrameNo);
        }

        public BoneData GetNextBone2(
            int frameNo,
            List<BoneData> bones,
            out int nextFrameNo,
            bool loopSearch)
        {
            BoneData nextBone = null;
            nextFrameNo = -1;

            foreach (var bone in bones)
            {
                if (bone.frameNo <= frameNo)
                {
                    continue;
                }

                nextBone = bone;
                nextFrameNo = bone.frameNo;
                break;
            }

            if (nextBone == null && loopSearch)
            {
                if (isLoopAnm)
                {
                    frameNo = (frameNo == maxFrameNo) ? 0 : -1; // 最終フレームの場合は0Fを除外
                    nextBone = GetNextBone2(frameNo, bones, out nextFrameNo, false);
                    nextFrameNo += maxFrameNo;
                }
                else
                {
                    nextBone = GetPrevBone2(maxFrameNo + 1, bones, out nextFrameNo, false);
                    nextFrameNo = maxFrameNo + 1;
                }
            }

            return nextBone;
        }

        public BoneData GetNextBone2(int frameNo, List<BoneData> bones, out int nextFrameNo)
        {
            return GetNextBone2(frameNo, bones, out nextFrameNo, true);
        }

        public BoneData GetNextBone2(int frameNo, List<BoneData> bones)
        {
            int nextFrameNo;
            return GetNextBone2(frameNo, bones, out nextFrameNo);
        }

        public void AddFirstBones(List<string> boneNames)
        {
            if (boneNames.Count == 0)
            {
                return;
            }

            var firstFrame = GetOrCreateFrame(0);
            FrameData tmpFrame = null;

            foreach (var boneName in boneNames)
            {
                var bone = firstFrame.GetBone(boneName);
                if (bone == null)
                {
                    if (tmpFrame == null)
                    {
                        tmpFrame = CreateFrame(timelineManager.currentFrameNo);
                        UpdateFrame(tmpFrame);
                    }

                    var tmpBone = tmpFrame.GetBone(boneName);
                    firstFrame.SetBone(tmpBone);
                }
            }

            if (tmpFrame != null)
            {
                timelineManager.RequestHistory("初期フレーム登録: " + boneNames.First());
            }
        }

        public void RemoveAllBones(List<string> boneNames)
        {
            if (boneNames.Count == 0)
            {
                return;
            }
            
            bool removed = false;

            foreach (var frame in keyFrames)
            {
                foreach (var boneName in boneNames)
                {
                    var bone = frame.GetBone(boneName);
                    if (bone != null)
                    {
                        frame.RemoveBone(bone);
                        removed = true;
                    }
                }
            }

            {
                foreach (var boneName in boneNames)
                {
                    var bone = _dummyLastFrame.GetBone(boneName);
                    if (bone != null)
                    {
                        _dummyLastFrame.RemoveBone(bone);
                        removed = true;
                    }
                }
            }

            if (removed)
            {
                timelineManager.RequestHistory("ボーン削除: " + boneNames.First());
            }
        }

        public FrameData GetActiveFrame(float frameNo)
        {
            return _keyFrames.LastOrDefault(f => f.frameNo <= frameNo);
        }

        /// <summary>
        /// 指定したフレームの再生に必要な開始フレーム番号を取得
        /// </summary>
        /// <param name="frameNo"></param>
        /// <returns></returns>
        public int GetStartFrameNo(int frameNo)
        {
            if (frameNo == 0)
            {
                return 0;
            }

            var startFrameNo = frameNo;

            foreach (var bones in _timelineBonesMap.Values)
            {
                if (bones.Count == 0)
                {
                    continue;
                }

                var bone = GetPrevBone2(frameNo + 1, bones, false);
                if (bone != null)
                {
                    startFrameNo = Math.Min(startFrameNo, bone.frameNo);
                }
            }

            return startFrameNo;
        }

        /// <summary>
        /// 指定したフレームの再生に必要な終了フレーム番号を取得
        /// </summary>
        /// <param name="frameNo"></param>
        /// <returns></returns>
        public int GetEndFrameNo(int frameNo)
        {
            if (frameNo == maxFrameNo)
            {
                return maxFrameNo;
            }

            var endFrameNo = frameNo;

            foreach (var bones in _timelineBonesMap.Values)
            {
                if (bones.Count == 0)
                {
                    continue;
                }

                int nextFrameNo;
                var bone = GetNextBone2(frameNo - 1, bones, out nextFrameNo);
                if (bone != null)
                {
                    endFrameNo = Math.Max(endFrameNo, nextFrameNo);
                }
            }

            return endFrameNo;
        }

        public void InsertFrames(int startFrameNo, int endFrameNo)
        {
            var length = endFrameNo - startFrameNo + 1;

            // 指定範囲以降のフレームを後ろにずらす
            for (int i = maxExistFrameNo; i >= startFrameNo; i--)
            {
                var frame = GetFrame(i);
                if (frame != null)
                {
                    frame.frameNo += length;
                }
            }
        }

        public void DuplicateFrames(int startFrameNo, int endFrameNo)
        {
            var length = endFrameNo - startFrameNo + 1;

            // 複製するフレーム数だけ後ろにずらす
            for (int i = maxExistFrameNo; i > endFrameNo; i--)
            {
                var frame = GetFrame(i);
                if (frame != null)
                {
                    frame.frameNo += length;
                }
            }

            // 指定範囲のフレームを複製
            for (int i = startFrameNo; i <= endFrameNo; i++)
            {
                var frame = GetFrame(i);
                if (frame != null)
                {
                    var newFrame = GetOrCreateFrame(i + length);
                    newFrame.FromFrameData(frame);
                }
            }
        }

        public void DeleteFrames(int startFrameNo, int endFrameNo)
        {
            var length = endFrameNo - startFrameNo + 1;

            // 指定範囲のフレームを削除
            for (int i = startFrameNo; i <= endFrameNo; i++)
            {
                var frame = GetFrame(i);
                if (frame != null)
                {
                    _keyFrames.Remove(frame);
                }
            }

            // 削除したフレーム数だけ前にずらす
            for (int i = endFrameNo + 1; i <= maxExistFrameNo; i++)
            {
                var frame = GetFrame(i);
                if (frame != null)
                {
                    frame.frameNo -= length;
                }
            }
        }

        public void InitTangent()
        {
            foreach (var frame in _keyFrames)
            {
                foreach (var bone in frame.bones)
                {
                    bone.transform.InitTangent();
                }
            }
        }

        public int GetEasing(int frameNo, string boneName)
        {
            var bone = GetBone(frameNo, boneName);
            if (bone != null)
            {
                return bone.transform.easing;
            }

            return (int) config.defaultEasingType;
        }

        public void FromXml(TimelineLayerXml xml)
        {
            _keyFrames = new List<FrameData>(xml.keyFrames.Count);

            foreach (var frameXml in xml.keyFrames)
            {
                var frame = CreateFrame(frameXml);
                _keyFrames.Add(frame);
            }
        }

        public TimelineLayerXml ToXml()
        {
            var xml = new TimelineLayerXml();

            xml.className = className;
            xml.slotNo = slotNo;

            xml.keyFrames = new List<FrameXml>(_keyFrames.Count);
            foreach (var frame in _keyFrames)
            {
                var frameXml = frame.ToXml();
                xml.keyFrames.Add(frameXml);
            }

            return xml;
        }

        protected XElement GetMeidElement(XElement songElement)
        {
            var maidElement = songElement.Elements("maid").FirstOrDefault(m => (string) m.Attribute("slotNo") == slotNo.ToString());
            if (maidElement == null)
            {
                maidElement = new XElement("maid");
                maidElement.SetAttributeValue("slotNo", slotNo);
                songElement.Add(maidElement);
            }

            return maidElement;
        }

        public enum TransformEditType
        {
            全て,
            移動,
            回転,
            拡縮,
            X,
            Y,
            Z,
            RX,
            RY,
            RZ,
            SX,
            SY,
            SZ,
        }

        public enum TransformDrawType
        {
            移動 = 1,
            回転 = 2,
            拡縮 = 4,
        }

        public readonly static int DrawMaskNone = 0;
        public readonly static int DrawMaskAll = (int) (TransformDrawType.移動 | TransformDrawType.回転 | TransformDrawType.拡縮);
        public readonly static int DrawMaskPositonAndRotation = (int) (TransformDrawType.移動 | TransformDrawType.回転);
        public readonly static int DrawMaskRotation = (int) (TransformDrawType.回転);
        public readonly static int DrawMaskPosition = (int) (TransformDrawType.移動);

        protected bool IsDrawTransformType(
            TransformDrawType drawType,
            TransformEditType editType,
            int drawMask)
        {
            switch (editType)
            {
                case TransformEditType.移動:
                case TransformEditType.X:
                case TransformEditType.Y:
                case TransformEditType.Z:
                    if (drawType != TransformDrawType.移動) return false;
                    break;
                case TransformEditType.回転:
                case TransformEditType.RX:
                case TransformEditType.RY:
                case TransformEditType.RZ:
                    if (drawType != TransformDrawType.回転) return false;
                    break;
                case TransformEditType.拡縮:
                case TransformEditType.SX:
                case TransformEditType.SY:
                case TransformEditType.SZ:
                    if (drawType != TransformDrawType.拡縮) return false;
                    break;
            }

            {
                if (drawType == TransformDrawType.移動 &&
                    (drawMask & (int) TransformDrawType.移動) == 0)
                {
                    return false;
                }
                if (drawType == TransformDrawType.回転 &&
                    (drawMask & (int) TransformDrawType.回転) == 0)
                {
                    return false;
                }
                if (drawType == TransformDrawType.拡縮 &&
                    (drawMask & (int) TransformDrawType.拡縮) == 0)
                {
                    return false;
                }
            }

            return true;
        }

        protected bool DrawTransform(
            GUIView view,
            Transform transform,
            TransformEditType editType,
            int drawMask,
            string boneName,
            Vector3 initialPosition,
            Vector3 initialEulerAngles,
            Vector3 initialScale)
        {
            var transformCache = view.GetTransformCache(transform);
            var updateTransform = false;

            if (IsDrawTransformType(TransformDrawType.移動, editType, drawMask))
            {
                updateTransform |= DrawPosition(view, transformCache, editType, initialPosition);
            }
            if (IsDrawTransformType(TransformDrawType.回転, editType, drawMask))
            {
                updateTransform |= DrawEulerAngles(view, transformCache, editType, boneName, initialEulerAngles);
            }
            if (IsDrawTransformType(TransformDrawType.拡縮, editType, drawMask))
            {
                updateTransform |= DrawScale(view, transformCache, editType, initialScale);
                updateTransform |= DrawSimpleScale(view, transformCache, editType, initialScale);
            }

            return updateTransform;
        }

        protected bool DrawTransformRect(
            GUIView view,
            Transform transform,
            TransformEditType editType,
            int drawMask,
            string boneName,
            Vector3 initialPosition,
            Vector3 initialEulerAngles,
            Vector3 initialScale)
        {
            var transformCache = view.GetTransformCache(transform);
            var updateTransform = false;

            if (IsDrawTransformType(TransformDrawType.移動, editType, drawMask))
            {
                updateTransform |= DrawPositionRect(view, transformCache, editType, initialPosition);
            }
            if (IsDrawTransformType(TransformDrawType.回転, editType, drawMask))
            {
                updateTransform |= DrawEulerAngles(view, transformCache, editType, boneName, initialEulerAngles);
            }
            if (IsDrawTransformType(TransformDrawType.拡縮, editType, drawMask))
            {
                updateTransform |= DrawScale(view, transformCache, editType, initialScale);
                updateTransform |= DrawSimpleScale(view, transformCache, editType, initialScale);
            }

            return updateTransform;
        }

        protected bool DrawPosition(
            GUIView view,
            TransformCache transform,
            TransformEditType editType,
            Vector3 initialPosition)
        {
            var position = transform.position;
            var updateTransform = false;
            var isFull = editType == TransformEditType.全て || editType == TransformEditType.移動;

            if (isFull || editType == TransformEditType.X)
            {
                updateTransform |= view.DrawSliderValue(
                    new GUIView.SliderOption
                    {
                        label = "X",
                        labelWidth = 30,
                        min = -config.positionRange,
                        max = config.positionRange,
                        step = 0.01f,
                        defaultValue = initialPosition.x,
                        value = position.x,
                        onChanged = x => position.x = x,
                    });
            }

            if (isFull || editType == TransformEditType.Y)
            {
                updateTransform |= view.DrawSliderValue(
                    new GUIView.SliderOption
                    {
                        label = "Y",
                        labelWidth = 30,
                        min = -config.positionRange,
                        max = config.positionRange,
                        step = 0.01f,
                        defaultValue = initialPosition.y,
                        value = position.y,
                        onChanged = y => position.y = y,
                    });
            }

            if (isFull || editType == TransformEditType.Z)
            {
                updateTransform |= view.DrawSliderValue(
                    new GUIView.SliderOption
                    {
                        label = "Z",
                        labelWidth = 30,
                        min = -config.positionRange,
                        max = config.positionRange,
                        step = 0.01f,
                        defaultValue = initialPosition.z,
                        value = position.z,
                        onChanged = z => position.z = z,
                    });
            }

            if (updateTransform)
            {
                transform.position = position;
                transform.Apply();
            }

            return updateTransform;
        }

        protected bool DrawPositionRect(
            GUIView view,
            TransformCache transform,
            TransformEditType editType,
            Vector3 initialPosition)
        {
            var position = transform.position;
            var updateTransform = false;
            var isFull = editType == TransformEditType.全て || editType == TransformEditType.移動;

            if (isFull || editType == TransformEditType.X)
            {
                updateTransform |= view.DrawSliderValue(
                    new GUIView.SliderOption
                    {
                        label = "X",
                        labelWidth = 30,
                        fieldType = FloatFieldType.Int,
                        min = -1000,
                        max = 1000,
                        step = 1,
                        defaultValue = initialPosition.x,
                        value = position.x,
                        onChanged = x => position.x = x,
                    });
            }

            if (isFull || editType == TransformEditType.Y)
            {
                updateTransform |= view.DrawSliderValue(
                    new GUIView.SliderOption
                    {
                        label = "Y",
                        labelWidth = 30,
                        fieldType = FloatFieldType.Int,
                        min = -1000,
                        max = 1000,
                        step = 1,
                        defaultValue = initialPosition.y,
                        value = position.y,
                        onChanged = y => position.y = y,
                    });
            }

            if (isFull || editType == TransformEditType.Z)
            {
                updateTransform |= view.DrawSliderValue(
                    new GUIView.SliderOption
                    {
                        label = "Z",
                        labelWidth = 30,
                        fieldType = FloatFieldType.Int,
                        min = -1000,
                        max = 1000,
                        step = 1,
                        defaultValue = initialPosition.z,
                        value = position.z,
                        onChanged = z => position.z = z,
                    });
            }

            if (updateTransform)
            {
                transform.position = position;
                transform.Apply();
            }

            return updateTransform;
        }

        protected bool DrawEulerAngles(
            GUIView view,
            TransformCache transform,
            TransformEditType editType,
            string boneName,
            Vector3 initialEulerAngles)
        {
            var angles = transform.eulerAngles;
            var prevBone = GetPrevBone(timelineManager.currentFrameNo, boneName);
            var prevAngles = prevBone != null ? prevBone.transform.eulerAngles : initialEulerAngles;
            angles = TransformDataBase.GetFixedEulerAngles(angles, prevAngles);
            var updateTransform = false;
            var isFull = editType == TransformEditType.全て || editType == TransformEditType.回転;

            if (isFull || editType == TransformEditType.RX)
            {
                updateTransform |= view.DrawSliderValue(
                    new GUIView.SliderOption
                    {
                        label = "RX",
                        labelWidth = 30,
                        min = prevAngles.x - 180f,
                        max = prevAngles.x + 180f,
                        step = 1f,
                        defaultValue = initialEulerAngles.x,
                        value = angles.x,
                        onChanged = x => angles.x = x,
                    });
            }

            if (isFull || editType == TransformEditType.RY)
            {
                updateTransform |= view.DrawSliderValue(
                    new GUIView.SliderOption
                    {
                        label = "RY",
                        labelWidth = 30,
                        min = prevAngles.y - 180f,
                        max = prevAngles.y + 180f,
                        step = 1f,
                        defaultValue = initialEulerAngles.y,
                        value = angles.y,
                        onChanged = y => angles.y = y,
                    });
            }

            if (isFull || editType == TransformEditType.RZ)
            {
                updateTransform |= view.DrawSliderValue(
                    new GUIView.SliderOption
                    {
                        label = "RZ",
                        labelWidth = 30,
                        min = prevAngles.z - 180f,
                        max = prevAngles.z + 180f,
                        step = 1f,
                        defaultValue = initialEulerAngles.z,
                        value = angles.z,
                        onChanged = z => angles.z = z,
                    });
            }

            if (updateTransform)
            {
                angles = TransformDataBase.GetFixedEulerAngles(angles, prevAngles);
                transform.eulerAngles = angles;
                transform.Apply();
            }

            return updateTransform;
        }

        protected bool DrawScale(
            GUIView view,
            TransformCache transform,
            TransformEditType editType,
            Vector3 initialScale)
        {
            var scale = transform.scale;
            var updateTransform = false;
            var isFull = editType == TransformEditType.全て || editType == TransformEditType.拡縮;

            if (isFull || editType == TransformEditType.SX)
            {
                updateTransform |= view.DrawSliderValue(
                    new GUIView.SliderOption
                    {
                        label = "SX",
                        labelWidth = 30,
                        min = 0,
                        max = config.scaleRange,
                        step = 0.01f,
                        defaultValue = initialScale.x,
                        value = scale.x,
                        onChanged = x => scale.x = x,
                    });
            }

            if (isFull || editType == TransformEditType.SY)
            {
                updateTransform |= view.DrawSliderValue(
                    new GUIView.SliderOption
                    {
                        label = "SY",
                        labelWidth = 30,
                        min = 0,
                        max = config.scaleRange,
                        step = 0.01f,
                        defaultValue = initialScale.y,
                        value = scale.y,
                        onChanged = y => scale.y = y,
                    });
            }

            if (isFull || editType == TransformEditType.SZ)
            {
                updateTransform |= view.DrawSliderValue(
                    new GUIView.SliderOption
                    {
                        label = "SZ",
                        labelWidth = 30,
                        min = 0,
                        max = config.scaleRange,
                        step = 0.01f,
                        defaultValue = initialScale.z,
                        value = scale.z,
                        onChanged = z => scale.z = z,
                    });
            }

            if (updateTransform)
            {
                transform.scale = scale;
                transform.Apply();
            }

            return updateTransform;
        }

        protected bool DrawSimpleScale(
            GUIView view,
            TransformCache transform,
            TransformEditType editType,
            Vector3 initialScale)
        {
            var scale = transform.scale;
            var updateTransform = false;
            var isFull = editType == TransformEditType.全て || editType == TransformEditType.拡縮;

            if (isFull)
            {
                updateTransform |= view.DrawSliderValue(
                    new GUIView.SliderOption
                    {
                        label = "拡縮",
                        labelWidth = 30,
                        min = 0,
                        max = config.scaleRange,
                        step = 0.01f,
                        defaultValue = initialScale.x,
                        value = scale.x,
                        onChanged = x =>
                        {
                            scale.x = x;
                            scale.y = x;
                            scale.z = x;
                        },
                    });
            }

            if (updateTransform)
            {
                transform.scale = scale;
                transform.Apply();
            }

            return updateTransform;
        }
    }
}