using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml.Serialization;
using Accessibility;
using UnityEngine;
using UnityEngine.Events;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    using SH = StudioHack;
    using MTE = MotionTimelineEditor;

    public class TimelineFile : ITileViewContent
    {
        public string name { get; set; }
        public Texture2D thum { get; set; }
    }

    public class TimelineManager
    {
        public static readonly long TimelineAnmId = 26925014;

        public TimelineData timeline = null;
        public List<TimelineFile> timelineFileList = new List<TimelineFile>(64);
        private long playingAnmId = -1;
        public HashSet<BoneData> selectedBones = new HashSet<BoneData>();
        private float prevMotionSliderRate = -1f;
        public string errorMessage = "";
        public FrameData initialEditFrame;
        public Vector3 initialEditPosition = Vector3.zero;
        private bool isPrevPoseEditing;

        public event UnityAction onRefresh;
        public event UnityAction onEditPoseUpdated;
        public event UnityAction onAnmSpeedChanged;
        public event UnityAction onSeekCurrentFrame;

        public int playingFrameNo
        {
            get
            {
                var rate = studioHack.motionSliderRate;
                return (int) Math.Round(rate * timeline.maxFrameNo);
            }
            set
            {
                if (timeline.maxFrameNo > 0)
                {
                    var rate = Mathf.Clamp01((float) value / timeline.maxFrameNo);
                    studioHack.motionSliderRate = rate;
                    prevMotionSliderRate = rate;
                }
            }
        }

        // アニメーションと同期しているか
        public bool isAnmSyncing
        {
            get
            {
                return IsValidData() && playingAnmId == TimelineAnmId;
            }
        }

        // 現在のアニメーションを再生中か
        public bool isAnmPlaying
        {
            get
            {
                return studioHack.isMotionPlaying && isAnmSyncing;
            }
        }

        private int _currentFrameNo = 0;
        public int currentFrameNo
        {
            get
            {
                return _currentFrameNo;
            }
            set
            {
                _currentFrameNo = Mathf.Clamp(value, 0, timeline.maxFrameNo);
            }
        }

        public float currentTime
        {
            get
            {
                return currentFrameNo * timeline.frameDuration;
            }
        }

        public float _anmSpeed = 1.0f;
        public float anmSpeed
        {
            get
            {
                return _anmSpeed;
            }
            set
            {
                if (_anmSpeed == value)
                {
                    return;
                }

                _anmSpeed = value;

                if (onAnmSpeedChanged != null)
                {
                    onAnmSpeedChanged();
                }
            }
        }

        private static TimelineManager _instance;
        public static TimelineManager instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new TimelineManager();
                }
                return _instance;
            }
        }

        private static StudioHackBase studioHack
        {
            get
            {
                return MTE.studioHack;
            }
        }

        private static MaidManager maidManager
        {
            get
            {
                return MaidManager.instance;
            }
        }

        private static Maid maid
        {
            get
            {
                return maidManager.maid;
            }
        }

        private static Config config
        {
            get
            {
                return MTE.config;
            }
        }

        private static BoneMenuManager boneMenuManager
        {
            get
            {
                return BoneMenuManager.Instance;
            }
        }

        private static TimelineHistoryManager historyManager
        {
            get
            {
                return TimelineHistoryManager.instance;
            }
        }

        private TimelineManager()
        {
            maidManager.onMaidChanged += OnMaidChanged;
        }

        public void Update()
        {
            long.TryParse(maidManager.annName, out playingAnmId);

            var motionSliderRate = studioHack.motionSliderRate;
            if (isAnmSyncing && !Mathf.Approximately(motionSliderRate, prevMotionSliderRate))
            {
                currentFrameNo = playingFrameNo;
            }
            prevMotionSliderRate = studioHack.motionSliderRate;

            if (isAnmPlaying && !Mathf.Approximately(anmSpeed, maidManager.anmSpeed))
            {
                maidManager.anmSpeed = anmSpeed;
            }

            var isPoseEditing = studioHack.isPoseEditing;
            if (isPrevPoseEditing != isPoseEditing)
            {
                if (isPoseEditing)
                {
                    OnStartPoseEdit();
                }
                else
                {
                    OnEndPoseEdit();
                }
                isPrevPoseEditing = isPoseEditing;
            }

            if (isPoseEditing && config.disablePoseHistory)
            {
                studioHack.ClearPoseHistory();
            }

            if (initialEditFrame != null && initialEditFrame.frameNo != currentFrameNo)
            {
                OnEditPoseUpdated();
            }

            if (requestedHistoryDesc.Length > 0 && !Input.GetMouseButton(0))
            {
                historyManager.AddHistory(timeline, requestedHistoryDesc);
                requestedHistoryDesc = "";
            }
        }

        public string GetTimelinePath(string anmName)
        {
            return PluginUtils.CombinePaths(PluginUtils.TimelineDirPath, anmName + ".xml");
        }

        public void CreateNewTimeline()
        {
            if (!studioHack.IsValid())
            {
                PluginUtils.ShowDialog(studioHack.errorMessage);
                return;
            }
            if (maid == null)
            {
                PluginUtils.ShowDialog("メイドが配置されていません");
                return;
            }

            timeline = new TimelineData();
            timeline.anmName = "テスト";
            timeline.version = TimelineData.CurrentVersion;
            timeline.UpdateFrame(0, maidManager.cacheBoneData);

            CreateAndApplyAnm();
            UnselectAll();
            Refresh();

            RequestHistory("タイムライン新規作成");
        }

        public void LoadTimeline(string anmName)
        {
            if (!studioHack.IsValid())
            {
                PluginUtils.ShowDialog(studioHack.errorMessage);
                return;
            }
            if (maid == null)
            {
                PluginUtils.ShowDialog("メイドが配置されていません");
                return;
            }

            var path = GetTimelinePath(anmName);
            if (!File.Exists(path))
            {
                return;
            }

            using (var stream = new FileStream(path, FileMode.Open))
            {
                var serializer = new XmlSerializer(typeof(TimelineData));
                timeline = (TimelineData)serializer.Deserialize(stream);
                timeline.anmName = anmName;
                timeline.ConvertVersion();
            }

            CreateAndApplyAnm();
            UnselectAll();
            Refresh();

            RequestHistory("「" + anmName + "」読み込み");
            // Extensions.ShowDialog("タイムライン「" + anmName + "」を読み込みました");
        }

        public void UpdateTimeline(TimelineData timeline)
        {
            this.timeline = timeline;
            CreateAndApplyAnm();
            UnselectAll();
            Refresh();
        }

        public void SaveTimeline()
        {
            if (!timeline.IsValidData(out errorMessage))
            {
                PluginUtils.ShowDialog(errorMessage);
                return;
            }

            var path = GetTimelinePath(timeline.anmName);
            using (var stream = new FileStream(path, FileMode.Create))
            {
                var serializer = new XmlSerializer(typeof(TimelineData));
                serializer.Serialize(stream, timeline);
            }

            var thumPath = PluginUtils.ConvertThumPath(path);
            MTE.instance.SaveScreenShot(thumPath, config.thumWidth, config.thumHeight);

            PluginUtils.ShowDialog("タイムライン「" + timeline.anmName + "」を保存しました");
        }

        public void UpdateTimelineFileList(bool reload)
        {
            if (reload)
            {
                foreach (var file in timelineFileList)
                {
                    if (file.thum != null)
                    {
                        UnityEngine.Object.Destroy(file.thum);
                        file.thum = null;
                    }
                }
                timelineFileList.Clear();
            }

            var paths = Directory.GetFiles(PluginUtils.TimelineDirPath, "*.xml");
            foreach (var path in paths)
            {
                var anmName = Path.GetFileNameWithoutExtension(path);
                if (timelineFileList.Exists(t => t.name == anmName))
                {
                    continue;
                }

                var thumPath = PluginUtils.ConvertThumPath(path);
                var thum = PluginUtils.LoadTexture(thumPath);

                var file = new TimelineFile
                {
                    name = anmName,
                    thum = thum,
                };
                timelineFileList.Add(file);
            }
        }

        public void ApplyAnm(long id, byte[] anmData)
        {
            var maid = maidManager.maid;
            if (maid == null)
            {
                PluginUtils.LogError("メイドが配置されていません");
                return;
            }

            float motionRate;
            if (isAnmSyncing)
            {
                motionRate = studioHack.motionSliderRate;
            }
            else
            {
                motionRate = Mathf.Clamp01((float) currentFrameNo / timeline.maxFrameNo);
            }

            GameMain.Instance.ScriptMgr.StopMotionScript();

            if (!maid.boMAN)
            {
                maid.body0.MuneYureL((float)((!timeline.useMuneKeyL) ? 1 : 0));
                maid.body0.MuneYureR((float)((!timeline.useMuneKeyR) ? 1 : 0));
                maid.body0.jbMuneL.enabled = !timeline.useMuneKeyL;
                maid.body0.jbMuneR.enabled = !timeline.useMuneKeyR;
            }

            maid.body0.CrossFade(id.ToString(), anmData, false, false, false, 0f, 1f);
            maid.SetAutoTwistAll(true);

            if (timeline.isLoopAnm)
            {
                var animation = maid.GetAnimation();
                if (animation != null)
                {
                    animation.wrapMode = WrapMode.Loop;
                }
            }
            else
            {
                var animation = maid.GetAnimation();
                if (animation != null)
                {
                    animation.wrapMode = WrapMode.ClampForever;
                }
                if (Mathf.Approximately(motionRate, 1f))
                {
                    motionRate = 0f;
                }
            }

            studioHack.OnMotionUpdated(maid);
            maidManager.OnMotionUpdated();

            if (studioHack.isMotionPlaying)
            {
                motionRate += 0.01f; // モーション再生中は再生位置に差分がないと反映されない
            }
            studioHack.motionSliderRate = motionRate;

            if (initialEditFrame != null)
            {
                OnEditPoseUpdated();
            }
        }

        public bool CreateAndApplyAnm()
        {
            PluginUtils.LogDebug("CreateAndApplyAnm");

            timeline.FixRotation();
            timeline.UpdateTangent();
            timeline.UpdateDummyLastFrame();

            var anmData = GetAnmBinary();
            if (anmData == null)
            {
                return false;
            }

            ApplyAnm(TimelineAnmId, anmData);

            return true;
        }

        public void OutputAnm()
        {
            try
            {
                var anmData = GetAnmBinary();
                if (anmData == null)
                {
                    PluginUtils.ShowDialog(errorMessage);
                    return;
                }
                var anmPath = timeline.anmPath;
                var anmName = timeline.anmName;

                bool isExist = File.Exists(anmPath);
                File.WriteAllBytes(anmPath, anmData);

                studioHack.OnUpdateMyPose(anmPath, isExist);

                PluginUtils.ShowDialog("モーション「" + timeline.anmName + "」を生成しました");
            }
            catch (Exception e)
            {
                PluginUtils.LogException(e);
                PluginUtils.ShowDialog("モーションの出力に失敗しました");
            }
        }

        public CacheBoneDataArray GetCacheBoneDataArray()
        {
            if (maid == null)
            {
                PluginUtils.LogError("メイドが配置されていません");
                return null;
            }

            var cacheBoneData = maidManager.cacheBoneData;
            if (cacheBoneData == null)
            {
                PluginUtils.LogError("ボーンデータが取得できませんでした");
                return null;
            }

            return cacheBoneData;
        }

        public void AddKeyFrameAll()
        {
            studioHack.isMotionPlaying = false;

            var boneDataArray = GetCacheBoneDataArray();
            if (boneDataArray == null)
            {
                return;
            }

            timeline.UpdateFrame(currentFrameNo, boneDataArray);

            // ポーズ編集中の移動は中心ボーンに反映  
            if (initialEditFrame != null)
            {
                var diffPosition = maid.transform.position - initialEditPosition;
                timeline.GetFrame(currentFrameNo).AddRootPosition(diffPosition);
                maid.transform.position = initialEditPosition;
            }

            ApplyCurrentFrame(true);

            RequestHistory("キーフレーム全追加");
        }

        public void AddKeyFrameDiff()
        {
            if (initialEditFrame == null)
            {
                PluginUtils.Log("ポーズ編集中のみキーフレームの登録ができます");
                return;
            }

            var boneDataArray = GetCacheBoneDataArray();
            if (boneDataArray == null)
            {
                return;
            }

            var tmpFrame = new FrameData(currentFrameNo);
            tmpFrame.SetCacheBoneDataArray(boneDataArray);

            var diffPosition = maid.transform.position - initialEditPosition;
            tmpFrame.AddRootPosition(diffPosition);
            maid.transform.position = initialEditPosition;

            var diffBones = tmpFrame.GetDiffBones(
                initialEditFrame,
                timeline.useMuneKeyL,
                timeline.useMuneKeyR);
            if (diffBones.Count == 0)
            {
                PluginUtils.Log("変更がないのでキーフレームの登録をスキップしました");
                return;
            }

            timeline.UpdateBones(currentFrameNo, diffBones);

            ApplyCurrentFrame(true);

            RequestHistory("キーフレーム追加");
        }

        public bool IsSelectedBone(BoneData bone)
        {
            return selectedBones.Contains(bone);
        }

        public void SelectFramesRange(int startFrameNo, int endFrameNo)
        {
            if (config.isEasyEdit)
            {
                for (int i = startFrameNo; i <= endFrameNo; i++)
                {
                    var frame = timeline.GetFrame(i);
                    if (frame != null)
                    {
                        foreach (var bone in frame.bones)
                        {
                            selectedBones.Add(bone);
                        }
                    }
                }
            }
            else
            {
                var selectedMenuItems = boneMenuManager.GetSelectedItems();
                for (int i = startFrameNo; i <= endFrameNo; i++)
                {
                    var frame = timeline.GetFrame(i);
                    if (frame != null)
                    {
                        foreach (var bone in frame.bones)
                        {
                            foreach (var boneMenuItem in selectedMenuItems)
                            {
                                if (boneMenuItem.IsTargetBone(bone))
                                {
                                    selectedBones.Add(bone);
                                    break;
                                }
                            }
                        }
                    }
                }
            }
        }

        private FrameData FindFrame(int start, int step)
        {
            if (config.isEasyEdit)
            {
                for (int i = start; i >= 0 && i <= timeline.maxFrameNo; i += step)
                {
                    var frame = timeline.GetFrame(i);
                    if (frame != null)
                    {
                        return frame;
                    }
                }
                return null;
            }
            else
            {
                var selectedMenuItems = boneMenuManager.GetSelectedItems();
                for (int i = start; i >= 0 && i <= timeline.maxFrameNo; i += step)
                {
                    var frame = timeline.GetFrame(i);
                    if (frame != null)
                    {
                        if (selectedMenuItems.Count == 0)
                        {
                            return frame;
                        }

                        foreach (var bone in frame.bones)
                        {
                            foreach (var boneMenuItem in selectedMenuItems)
                            {
                                if (boneMenuItem.IsTargetBone(bone))
                                {
                                    return frame;
                                }
                            }
                        }
                    }
                }
                return null;
            }
        }

        public FrameData GetPrevFrame(int frameNo)
        {
            return FindFrame(frameNo - 1, -1);
        }

        public FrameData GetNextFrame(int frameNo)
        {
            return FindFrame(frameNo + 1, 1);
        }

        public void SelectBones(List<BoneData> bones, bool isMultiSelect)
        {
            if (bones.Count == 0)
            {
                return;
            }

            bool hasSelected = false;
            foreach (var bone in bones)
            {
                if (selectedBones.Contains(bone))
                {
                    hasSelected = true;
                    break;
                }
            }

            // 通常選択動作
            if (!isMultiSelect)
            {
                if (!hasSelected)
                {
                    selectedBones.Clear();
                    selectedBones.UnionWith(bones);
                }
            }
            // 複数選択動作
            else
            {
                if (hasSelected)
                {
                    foreach (var bone in bones)
                    {
                        selectedBones.Remove(bone);
                    }
                }
                else
                {
                    selectedBones.UnionWith(bones);
                }
            }
        }

        public bool HasSelected()
        {
            return selectedBones.Count > 0;
        }

        public void UnselectAll()
        {
            selectedBones.Clear();
        }

        public void SelectAllFrames()
        {
            selectedBones.Clear();
            foreach (var frame in timeline.keyFrames)
            {
                selectedBones.UnionWith(frame.bones);
            }
        }

        public void SelectVerticalBones()
        {
            if (config.isEasyEdit)
            {
                return;
            }

            var frames = new HashSet<FrameData>();
            foreach (var bone in selectedBones)
            {
                var frame = bone.parentFrame;
                if (frame != null)
                {
                    frames.Add(frame);
                }
            }

            foreach (var frame in frames)
            {
                selectedBones.UnionWith(frame.bones);
            }
        }

        public void RemoveSelectedFrame()
        {
            if (selectedBones.Count == 0)
            {
                PluginUtils.LogWarning("削除するキーフレームが選択されていません");
                return;
            }

            foreach (var bone in selectedBones)
            {
                var frame = bone.parentFrame;
                if (frame != null)
                {
                    frame.RemoveBone(bone);
                }
            }
            timeline.CleanFrames();
            selectedBones.Clear();

            ApplyCurrentFrame(true);

            RequestHistory("キーフレーム削除");
        }

        public void MoveSelectedBones(int delta)
        {
            if (selectedBones.Count == 0)
            {
                PluginUtils.LogWarning("移動するキーフレームが選択されていません");
                return;
            }

            foreach (var selectedBone in selectedBones)
            {
                var selectedFrame = selectedBone.parentFrame;
                var targetFrame = timeline.GetFrame(selectedFrame.frameNo + delta);

                // 移動先のボーンが重複していたら移動しない
                if (targetFrame != null)
                {
                    var targetBone = targetFrame.GetBone(selectedBone.bonePath);
                    if (targetBone != null && !IsSelectedBone(targetBone))
                    {
                        return;
                    }
                }

                // マイナスフレームに移動しようとしたら移動しない
                if (selectedFrame.frameNo + delta < 0)
                {
                    return;
                }

                // 移動先のフレーム番号が最大フレーム番号を超えたら移動しない
                if (selectedFrame.frameNo + delta > timeline.maxFrameNo)
                {
                    return;
                }
            }

            var sortedBones = selectedBones.ToList();
            if (delta < 0)
            {
                sortedBones.Sort((a, b) => a.frameNo - b.frameNo);
            }
            else
            {
                sortedBones.Sort((a, b) => b.frameNo - a.frameNo);
            }

            foreach (var selectedBone in sortedBones)
            {
                var targetFrameNo = selectedBone.frameNo + delta;
                var sourceFrame = selectedBone.parentFrame;
                sourceFrame.RemoveBone(selectedBone);

                timeline.SetBone(targetFrameNo, selectedBone);
            }

            timeline.CleanFrames();

            ApplyCurrentFrame(true);

            RequestHistory("キーフレーム移動");
        }

        public void SetMaxFrameNo(int maxFrameNo)
        {
            timeline.maxFrameNo = maxFrameNo;

            ApplyCurrentFrame(true);
            Refresh();
        }

        public void Refresh()
        {
            if (onRefresh != null)
            {
                onRefresh();
            }
        }

        public void SeekCurrentFrame(int frameNo)
        {
            studioHack.isMotionPlaying = false;

            if (this.currentFrameNo == frameNo)
            {
                return;
            }
            this.currentFrameNo = frameNo;

            ApplyCurrentFrame(false);

            if (onSeekCurrentFrame != null)
            {
                onSeekCurrentFrame();
            }
        }

        public void ApplyCurrentFrame(bool motionUpdate)
        {
            if (playingAnmId != TimelineAnmId || motionUpdate)
            {
                CreateAndApplyAnm();
            }
            else
            {
                playingFrameNo = currentFrameNo;
            }
        }

        public class CopyFrameData
        {
            [XmlElement("Frame")]
            public List<FrameData> frames;
        }

        public void CopyFramesToClipboard()
        {
            if (selectedBones.Count == 0)
            {
                PluginUtils.LogWarning("コピーするキーフレームが選択されていません");
                return;
            }

            var copyFrameData = new CopyFrameData();

            var tmpFrames = new Dictionary<int, FrameData>();
            foreach (var bone in selectedBones)
            {
                FrameData tmpFrame;
                if (!tmpFrames.TryGetValue(bone.frameNo, out tmpFrame))
                {
                    tmpFrame = new FrameData(bone.frameNo);
                    tmpFrames[bone.frameNo] = tmpFrame;
                }

                tmpFrame.UpdateBone(bone);
            }
            copyFrameData.frames = tmpFrames.Values.ToList();

            try
            {
                var serializer = new XmlSerializer(typeof(CopyFrameData));
                using (var writer = new StringWriter())
                {
                    serializer.Serialize(writer, copyFrameData);
                    var framesXml = writer.ToString();
                    GUIUtility.systemCopyBuffer = framesXml;
                }

                PluginUtils.Log("クリップボードにコピーしました");
            }
            catch (Exception e)
            {
                PluginUtils.LogException(e);
                PluginUtils.ShowDialog("コピーに失敗しました");
            }
        }

        public void PasteFramesFromClipboard(bool flip)
        {
            try
            {
                var framesXml = GUIUtility.systemCopyBuffer;
                var serializer = new XmlSerializer(typeof(CopyFrameData));
                using (var reader = new StringReader(framesXml))
                {
                    var copyFrameData = (CopyFrameData) serializer.Deserialize(reader);

                    if (copyFrameData.frames.Count == 0)
                    {
                        PluginUtils.LogWarning("ペーストするキーフレームがありません");
                        return;
                    }

                    var tmpFrames = copyFrameData.frames;
                    var minFrameNo = tmpFrames.Min(frame => frame.frameNo);
                    foreach (var tmpFrame in tmpFrames)
                    {
                        if (flip)
                        {
                            tmpFrame.Flip();
                        }
                        var frameNo = currentFrameNo + tmpFrame.frameNo - minFrameNo;
                        timeline.UpdateBones(frameNo, tmpFrame.bones);
                    }

                    if (flip)
                    {
                        RequestHistory("反転ペースト");
                    }
                    else
                    {
                        RequestHistory("ペースト");
                    }
                }

                ApplyCurrentFrame(true);
            }
            catch (Exception e)
            {
                PluginUtils.LogException(e);
                PluginUtils.ShowDialog("ペーストに失敗しました");
            }
        }

        public bool IsValidData()
        {
            if (maid == null)
            {
                errorMessage = "メイドを配置してください";
                return false;
            }

            if (timeline == null)
            {
                errorMessage = "新規作成かロードをしてください";
                return false;
            }

            return timeline.IsValidData(out errorMessage);
        }

        public byte[] GetAnmBinary()
        {
            if (timeline == null)
            {
                return null;
            }

            return timeline.GetAnmBinary(out errorMessage);
        }

        public void Play()
        {
            studioHack.isPoseEditing = false;

            var success = CreateAndApplyAnm();
            if (success)
            {
                studioHack.isMotionPlaying = true;
            }
        }

        public void Stop()
        {
            studioHack.isMotionPlaying = false;
        }

        private string requestedHistoryDesc = "";

        public void RequestHistory(string description)
        {
            requestedHistoryDesc = description;
        }

        private void OnEditPoseUpdated()
        {
            OnEndPoseEdit();

            var cacheBoneData = maidManager.cacheBoneData;
            if (cacheBoneData == null)
            {
                PluginUtils.LogError("ボーンデータが取得できませんでした");
                return;
            }

            initialEditFrame = new FrameData(currentFrameNo);
            initialEditFrame.SetCacheBoneDataArray(cacheBoneData);
            initialEditPosition = maid.transform.position;

            if (onEditPoseUpdated != null)
            {
                onEditPoseUpdated();
            }
        }

        private void OnStartPoseEdit()
        {
            ApplyCurrentFrame(false);
            OnEditPoseUpdated();
        }

        private void OnEndPoseEdit()
        {
            if (initialEditFrame != null)
            {
                initialEditFrame = null;
                maid.transform.position = initialEditPosition;
            }
        }


        private void OnMaidChanged(Maid maid)
        {
            if (IsValidData())
            {
                studioHack.useMuneKeyL = timeline.useMuneKeyL;
                studioHack.useMuneKeyR = timeline.useMuneKeyR;
            }
        }
    }
}