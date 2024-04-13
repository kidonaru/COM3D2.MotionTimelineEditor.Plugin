using System;
using System.Collections.Generic;
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
        public HashSet<BoneData> selectedBones = new HashSet<BoneData>();        private float prevMotionSliderRate = -1f;
        public string errorMessage = "";
        public FrameData initialEditFrame;
        public Vector3 initialEditPosition = Vector3.zero;
        private bool isPrevPoseEditing;

        public event UnityAction onRefresh;
        public event UnityAction onEditPoseUpdated;

        public int playingFrameNo
        {
            get
            {
                var rate = maidHack.motionSliderRate;
                return (int) Math.Round(rate * timeline.maxFrameNo);
            }
            set
            {
                if (timeline.maxFrameNo > 0)
                {
                    var rate = Mathf.Clamp01((float) value / timeline.maxFrameNo);
                    maidHack.motionSliderRate = rate;
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
                return maidHack.isMotionPlaying && isAnmSyncing;
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
                moviePlayer.UpdateSpeed();
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

        private static MaidHackBase maidHack
        {
            get
            {
                return MTE.maidHack;
            }
        }

        private static Maid maid
        {
            get
            {
                return maidHack.maid;
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

        private static MoviePlayer moviePlayer
        {
            get
            {
                return MoviePlayer.instance;
            }
        }

        private TimelineManager()
        {
        }

        public void Update()
        {
            long.TryParse(maidHack.annName, out playingAnmId);

            var motionSliderRate = maidHack.motionSliderRate;
            if (isAnmSyncing && !Mathf.Approximately(motionSliderRate, prevMotionSliderRate))
            {
                currentFrameNo = playingFrameNo;
            }
            prevMotionSliderRate = maidHack.motionSliderRate;

            if (isAnmPlaying && !Mathf.Approximately(anmSpeed, maidHack.anmSpeed))
            {
                maidHack.anmSpeed = anmSpeed;
            }

            var isPoseEditing = maidHack.isPoseEditing;
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

            if (initialEditFrame != null && initialEditFrame.frameNo != currentFrameNo)
            {
                OnEditPoseUpdated();
            }
        }

        public string GetTimelinePath(string anmName)
        {
            return Extensions.CombinePaths(Extensions.TimelineDirPath, anmName + ".xml");
        }

        public void CreateNewTimeline()
        {
            if (!maidHack.IsValid())
            {
                Extensions.ShowDialog(maidHack.errorMessage);
                return;
            }

            timeline = new TimelineData();
            timeline.anmName = "テスト";
            timeline.version = TimelineData.CurrentVersion;
            timeline.UpdateFrame(0, maidHack.cacheBoneData);

            CreateAndApplyAnm();
            UnselectAll();
            Refresh();
        }

        public void LoadTimeline(string anmName)
        {
            if (!maidHack.IsValid())
            {
                Extensions.ShowDialog(maidHack.errorMessage);
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

            // Extensions.ShowDialog("タイムライン「" + anmName + "」を読み込みました");
        }

        public void SaveTimeline()
        {
            if (!timeline.IsValidData(out errorMessage))
            {
                Extensions.ShowDialog(errorMessage);
                return;
            }

            var path = GetTimelinePath(timeline.anmName);
            using (var stream = new FileStream(path, FileMode.Create))
            {
                var serializer = new XmlSerializer(typeof(TimelineData));
                serializer.Serialize(stream, timeline);
            }

            var thumPath = Extensions.ConvertThumPath(path);
            MTE.instance.SaveScreenShot(thumPath, config.thumWidth, config.thumHeight);

            Extensions.ShowDialog("タイムライン「" + timeline.anmName + "」を保存しました");
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

            var paths = Directory.GetFiles(Extensions.TimelineDirPath, "*.xml");
            foreach (var path in paths)
            {
                var anmName = Path.GetFileNameWithoutExtension(path);
                if (timelineFileList.Exists(t => t.name == anmName))
                {
                    continue;
                }

                var thumPath = Extensions.ConvertThumPath(path);
                var thum = Extensions.LoadTexture(thumPath);

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
            var maid = maidHack.maid;

            float motionRate;
            if (isAnmSyncing)
            {
                motionRate = maidHack.motionSliderRate;
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
                if (Mathf.Approximately(motionRate, 1f))
                {
                    motionRate = 0f;
                }
            }

            maidHack.OnMotionUpdated(maid);

            if (maidHack.isMotionPlaying)
            {
                motionRate += 0.01f; // モーション再生中は再生位置に差分がないと反映されない
            }
            maidHack.motionSliderRate = motionRate;

            if (initialEditFrame != null)
            {
                OnEditPoseUpdated();
            }
        }

        public bool CreateAndApplyAnm()
        {
            Extensions.LogDebug("CreateAndApplyAnm");

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
                    Extensions.ShowDialog(errorMessage);
                    return;
                }
                var anmPath = timeline.anmPath;
                var anmName = timeline.anmName;

                bool isExist = File.Exists(anmPath);
                File.WriteAllBytes(anmPath, anmData);

                maidHack.OnUpdateMyPose(anmPath, isExist);

                Extensions.ShowDialog("モーション「" + timeline.anmName + "」を生成しました");
            }
            catch (Exception e)
            {
                Extensions.LogException(e);
                Extensions.ShowDialog("モーションの出力に失敗しました");
            }
        }

        public CacheBoneDataArray GetCacheBoneDataArray()
        {
            if (maid == null)
            {
                Extensions.LogError("メイドが配置されていません");
                return null;
            }

            var cacheBoneData = maidHack.cacheBoneData;
            if (cacheBoneData == null)
            {
                Extensions.LogError("ボーンデータが取得できませんでした");
                return null;
            }

            return cacheBoneData;
        }

        public void AddKeyFrameAll()
        {
            maidHack.isMotionPlaying = false;

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
        }

        public void AddKeyFrameDiff()
        {
            if (initialEditFrame == null)
            {
                Extensions.Log("ポーズ編集中のみキーフレームの登録ができます");
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
                Extensions.Log("変更がないのでキーフレームの登録をスキップしました");
                return;
            }

            timeline.UpdateBones(currentFrameNo, diffBones);

            ApplyCurrentFrame(true);
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
        }

        public void MoveSelectedBones(int delta)
        {
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

        public void SetCurrentFrame(int frameNo)
        {
            if (this.currentFrameNo == frameNo)
            {
                return;
            }
            this.currentFrameNo = frameNo;

            moviePlayer.UpdateSeekTime();

            ApplyCurrentFrame(false);
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
            }
            catch (Exception e)
            {
                Extensions.LogException(e);
                Extensions.ShowDialog("コピーに失敗しました");
            }
        }

        public void PasteFramesFromClipboard(bool flip)
        {
            var framesXml = GUIUtility.systemCopyBuffer;
            if (framesXml.Length == 0)
            {
                return;
            }

            try
            {
                var serializer = new XmlSerializer(typeof(CopyFrameData));
                using (var reader = new StringReader(framesXml))
                {
                    var copyFrameData = (CopyFrameData) serializer.Deserialize(reader);
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
                }

                ApplyCurrentFrame(true);
            }
            catch (Exception e)
            {
                Extensions.LogException(e);
                Extensions.ShowDialog("貼り付けに失敗しました");
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
            var success = CreateAndApplyAnm();
            if (success)
            {
                maidHack.isMotionPlaying = true;
            }
        }

        public void Stop()
        {
            maidHack.isMotionPlaying = false;
        }

        private void OnEditPoseUpdated()
        {
            OnEndPoseEdit();

            var cacheBoneData = maidHack.cacheBoneData;
            if (cacheBoneData == null)
            {
                Extensions.LogError("ボーンデータが取得できませんでした");
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
    }
}