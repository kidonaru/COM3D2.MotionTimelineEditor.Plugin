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

        public abstract int priority { get; }
        public abstract string className { get; }

        public int slotNo { get; protected set; }

        public virtual bool hasSlotNo
        {
            get
            {
                return false;
            }
        }

        public virtual bool isCameraLayer
        {
            get
            {
                return false;
            }
        }

        protected List<FrameData> _keyFrames = new List<FrameData>();
        public List<FrameData> keyFrames
        {
            get
            {
                return _keyFrames.Cast<FrameData>().ToList();
            }
        }

        public MaidCache maidCache
        {
            get
            {
                return maidManager.GetMaidCache(slotNo);
            }
        }

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

        public float playingTime
        {
            get
            {
                return playingFrameNoFloat * timeline.frameDuration;
            }
        }

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

        public virtual bool isDragging
        {
            get
            {
                return false;
            }
        }

        public abstract List<string> allBoneNames { get; }

        public string errorMessage { get; protected set; }

        public bool isCurrent
        {
            get
            {
                return timelineManager.currentLayer == this;
            }
        }

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

        public FrameData firstFrame
        {
            get
            {
                return _keyFrames.Count > 0 ? _keyFrames[0] : null;
            }
        }

        public string anmFileName
        {
            get
            {
                var suffix = slotNo > 0 ? "_" + slotNo : "";
                return timeline.anmName + suffix + ".anm";
            }
        }

        public string anmPath
        {
            get
            {
                return studioHack.outputAnmPath + "\\" + anmFileName;
            }
        }

        public List<IBoneMenuItem> _allMenuItems = new List<IBoneMenuItem>();
        public List<IBoneMenuItem> allMenuItems
        {
            get
            {
                return _allMenuItems;
            }
        }

        // ループ補正用の最終フレーム
        protected FrameData _dummyLastFrame = null;

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

        protected static ITimelineLayer currentLayer
        {
            get
            {
                return timelineManager.currentLayer;
            }
        }

        protected static ITimelineLayer defaultLayer
        {
            get
            {
                return timeline.defaultLayer;
            }
        }

        protected static Config config
        {
            get
            {
                return ConfigManager.config;
            }
        }

        protected static int maxFrameNo
        {
            get
            {
                return timeline.maxFrameNo;
            }
        }

        protected static bool useMuneKeyL
        {
            get
            {
                return timeline.useMuneKeyL;
            }
        }

        protected static bool useMuneKeyR
        {
            get
            {
                return timeline.useMuneKeyR;
            }
        }

        protected static bool isLoopAnm
        {
            get
            {
                return timeline.isLoopAnm;
            }
        }

        protected static MaidManager maidManager
        {
            get
            {
                return MaidManager.instance;
            }
        }

        protected static StudioModelManager modelManager
        {
            get
            {
                return StudioModelManager.instance;
            }
        }

        protected static ModelHackManager modelHackManager
        {
            get
            {
                return ModelHackManager.instance;
            }
        }

        protected static StudioLightManager lightManager
        {
            get
            {
                return StudioLightManager.instance;
            }
        }

        protected static LightHackManager lightHackManager
        {
            get
            {
                return LightHackManager.instance;
            }
        }

        protected static StudioHackBase studioHack
        {
            get
            {
                return StudioHackManager.studioHack;
            }
        }

        protected static PhotoBGManager photoBGManager
        {
            get
            {
                return PhotoBGManager.instance;
            }
        }

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
        public abstract void ApplyAnm(long id, byte[] anmData);
        public abstract void ApplyCurrentFrame(bool motionUpdate);
        public abstract void OutputAnm();
        protected abstract byte[] GetAnmBinaryInternal(bool forOutput, int startFrameNo, int endFrameNo);
        public abstract void OutputDCM(XElement songElement);
        public abstract float CalcEasingValue(float t, int easing);

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

            foreach (var bone in tmpFrame.bones)
            {
                PluginUtils.LogDebug("current trans: {0}", bone.transform.ToString());
            }

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

        protected void FixRotationFrame(FrameData frame)
        {
            foreach (var bone in frame.bones)
            {
                if (bone.transform.hasRotation)
                {
                    var prevBone = GetPrevBone(frame.frameNo, bone.name);
                    if (prevBone != null)
                    {
                        bone.transform.FixRotation(prevBone.transform);
                    }
                }
                if (bone.transform.hasEulerAngles)
                {
                    var prevBone = GetPrevBone(frame.frameNo, bone.name);
                    if (prevBone != null)
                    {
                        bone.transform.FixEulerAngles(prevBone.transform);
                    }
                }
            }
        }

        protected void FixRotation(int startFrameNo, int endFrameNo)
        {
            foreach (var frame in _keyFrames)
            {
                if (frame.frameNo <= startFrameNo || frame.frameNo > endFrameNo)
                {
                    continue;
                }

                FixRotationFrame(frame);
            }
        }

        protected void UpdateTangentFrame(FrameData frame)
        {
            var currentTime = timeline.GetFrameTimeSeconds(frame.frameNo);
            foreach (var bone in frame.bones)
            {
                int prevFrameNo;
                var prevBone = GetPrevBone(frame.frameNo, bone.name, out prevFrameNo);

                int nextFrameNo;
                var nextBone = GetNextBone(frame.frameNo, bone.name, out nextFrameNo);

                // 前後に存在しない場合は自身を使用
                if (prevBone == null)
                {
                    prevBone = bone;
                    prevFrameNo = frame.frameNo - 1;
                }
                if (nextBone == null)
                {
                    nextBone = bone;
                    nextFrameNo = frame.frameNo + 1;
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
                var nextTime = timeline.GetFrameTimeSeconds(nextFrameNo);

                bone.transform.UpdateTangent(
                    prevTrans,
                    nextTrans,
                    prevTime,
                    currentTime,
                    nextTime);
            }
        }

        protected void UpdateTangent(int startFrameNo, int endFrameNo)
        {
            foreach (var frame in _keyFrames)
            {
                if (frame.frameNo < startFrameNo || frame.frameNo > endFrameNo)
                {
                    continue;
                }
                UpdateTangentFrame(frame);
            }
        }

        protected void UpdateDummyLastFrame()
        {
            _dummyLastFrame.frameNo = maxFrameNo;

            foreach (var name in allBoneNames)
            {
                BoneData sourceBone;
                if (isLoopAnm)
                {
                    sourceBone = GetNextBone(-1, name);
                }
                else
                {
                    sourceBone = GetPrevBone(maxFrameNo, name);
                }

                if (sourceBone != null)
                {
                   _dummyLastFrame.UpdateBone(sourceBone);
                }
            }

            FixRotationFrame(_dummyLastFrame);
            UpdateTangentFrame(_dummyLastFrame);
        }

        protected void PrepareAnmBinary(int startFrameNo, int endFrameNo)
        {
            var stopwatch = new StopwatchDebug();

            FixRotation(startFrameNo, endFrameNo);
            stopwatch.ProcessEnd("FixRotation");

            UpdateTangent(startFrameNo, endFrameNo);
            stopwatch.ProcessEnd("UpdateTangent");

            UpdateDummyLastFrame();
            stopwatch.ProcessEnd("UpdateDummyLastFrame");
        }

        public byte[] GetAnmBinary(bool forOutput)
        {
            if (!IsValidData())
            {
                return null;
            }

            var startFrameNo = 0;
            var endFrameNo = timeline.maxFrameNo;
            var activeTrack = timeline.activeTrack;

            if (activeTrack != null && !forOutput)
            {
                startFrameNo = GetStartFrameNo(activeTrack.startFrameNo);
                endFrameNo = GetEndFrameNo(activeTrack.endFrameNo);

                PluginUtils.LogDebug("GetAnmBinary: slotNo={0} startFrameNo={1}, endFrameNo={2} ",
                    slotNo, startFrameNo, endFrameNo);
            }

            PrepareAnmBinary(startFrameNo, endFrameNo);

            var stopwatch = new StopwatchDebug();
            var anmData = GetAnmBinaryInternal(forOutput, startFrameNo, endFrameNo);
            stopwatch.ProcessEnd("GetAnmBinary");

            return anmData;
        }

        public void CreateAndApplyAnm()
        {
            PluginUtils.LogDebug("CreateAndApplyAnm");
            var anmData = GetAnmBinary(false);
            ApplyAnm(TimelineAnmId, anmData);
        }

        public abstract ITransformData CreateTransformData(string name);

        public ITransformData CreateTransformData(ITransformData transform)
        {
            var newTransform = CreateTransformData(transform.name);
            newTransform.FromTransformData(transform);
            return newTransform;
        }

        public ITransformData CreateTransformData(TransformXml xml)
        {
            var newTransform = CreateTransformData(xml.name);
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
            BoneData prevBone = null;
            prevFrameNo = -1;

            foreach (var frame in _keyFrames)
            {
                if (frame.frameNo >= frameNo)
                {
                    break;
                }

                var bone = frame.GetBone(name);
                if (bone != null)
                {
                    prevBone = bone;
                    prevFrameNo = frame.frameNo;
                }
            }

            if (prevBone == null && loopSearch)
            {
                if (isLoopAnm)
                {
                    frameNo = (frameNo == 0) ? maxFrameNo : maxFrameNo + 1; // 0Fの場合は最終フレームを除外
                    prevBone = GetPrevBone(frameNo, name, out prevFrameNo, false);
                    prevFrameNo -= maxFrameNo;
                }
                else
                {
                    prevBone = GetNextBone(-1, name, out prevFrameNo, false);
                    prevFrameNo = -1;
                }
            }

            return prevBone;
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
            BoneData nextBone = null;
            nextFrameNo = -1;
            foreach (var frame in _keyFrames)
            {
                if (frame.frameNo <= frameNo)
                {
                    continue;
                }

                var bone = frame.GetBone(name);
                if (bone != null)
                {
                    nextBone = bone;
                    nextFrameNo = frame.frameNo;
                    break;
                }
            }

            if (nextBone == null && loopSearch)
            {
                if (isLoopAnm)
                {
                    frameNo = (frameNo == maxFrameNo) ? 0 : -1; // 最終フレームの場合は0Fを除外
                    nextBone = GetNextBone(frameNo, name, out nextFrameNo, false);
                    nextFrameNo += maxFrameNo;
                }
                else
                {
                    nextBone = GetPrevBone(maxFrameNo + 1, name, out nextFrameNo, false);
                    nextFrameNo = maxFrameNo + 1;
                }
            }

            return nextBone;
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

            foreach (var firstBone in firstFrame.bones)
            {
                var name = firstBone.name;
                var bone = GetPrevBone(frameNo + 1, name, false);
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

            foreach (var firstBone in firstFrame.bones)
            {
                var name = firstBone.name;
                int nextFrameNo;
                var bone = GetNextBone(frameNo - 1, name, out nextFrameNo);
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

        public readonly static int DrawMaskAll = 0;
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

            if (drawMask != 0)
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