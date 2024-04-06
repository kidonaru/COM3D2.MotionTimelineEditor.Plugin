using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml.Serialization;
using Accessibility;
using UnityEngine;

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
        public HashSet<FrameData> selectedRefFrames = new HashSet<FrameData>();
        public HashSet<BoneData> selectedBones = new HashSet<BoneData>();        private float prevMotionSliderRate = -1f;
        public string errorMessage = "";
        public Action onRefresh = null;
        public FrameData initialEditFrame;
        private bool isPrevPoseEditing;

        public Vector3[] initialEditIkPositions = new Vector3[(int) IKHoldType.Max]
        {
            Vector3.zero,
            Vector3.zero,
            Vector3.zero,
            Vector3.zero,
            Vector3.zero,
            Vector3.zero,
            Vector3.zero,
            Vector3.zero,
        };

        public int playingFrameNo
        {
            get
            {
                var rate = SH.motionSliderRate;
                return (int) Math.Round(rate * timeline.maxFrameNo);
            }
            set
            {
                if (timeline.maxFrameNo > 0)
                {
                    var rate = Mathf.Clamp01((float) value / timeline.maxFrameNo);
                    SH.motionSliderRate = rate;
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
                return SH.isMotionPlaying && isAnmSyncing;
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

        public static Maid maid
        {
            get
            {
                return SH.maid;
            }
        }

        public static Config config
        {
            get
            {
                return MTE.config;
            }
        }

        public static BoneMenuManager boneMenuManager
        {
            get
            {
                return BoneMenuManager.Instance;
            }
        }

        private TimelineManager()
        {
        }

        public void Update()
        {
            if (SH.animeState != null)
            {
                long.TryParse(SH.animeState.name, out playingAnmId);
            }

            var motionSliderRate = SH.motionSliderRate;
            if (isAnmSyncing && !Mathf.Approximately(motionSliderRate, prevMotionSliderRate))
            {
                currentFrameNo = playingFrameNo;
            }
            prevMotionSliderRate = SH.motionSliderRate;

            var isPoseEditing = SH.isPoseEditing;
            if (isPrevPoseEditing != isPoseEditing)
            {
                if (isPoseEditing)
                {
                    OnStartPoseEditing();
                }
                else
                {
                    OnEndPoseEditing();
                }
                isPrevPoseEditing = isPoseEditing;
            }

            if (initialEditFrame != null && initialEditFrame.frameNo != currentFrameNo)
            {
                OnStartPoseEditing();
            }
        }

        public string GetTimelinePath(string anmName)
        {
            return Extensions.CombinePaths(Extensions.TimelineDirPath, anmName + ".xml");
        }

        public void CreateNewTimeline()
        {
            if (maid == null)
            {
                Extensions.ShowDialog("先にメイドを配置してください");
                return;
            }

            timeline = new TimelineData();
            timeline.anmName = "テスト";
            timeline.version = TimelineData.CurrentVersion;
            timeline.UpdateFrame(0, SH.ikManager.cache_bone_data);

            CreateAndApplyAnm();
            Refresh();
        }

        public void LoadTimeline(string anmName)
        {
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

        private static readonly Maid.AutoTwist[] autoTwists = new Maid.AutoTwist[]
        {
            Maid.AutoTwist.ShoulderL,
            Maid.AutoTwist.ShoulderR,
            Maid.AutoTwist.WristL,
            Maid.AutoTwist.WristR,
            Maid.AutoTwist.ThighL,
            Maid.AutoTwist.ThighR,
        };

        public void ApplyAnm(long id, byte[] anmData)
        {
            var maid = SH.maid;
            var motionRate = SH.motionSliderRate;

            GameMain.Instance.ScriptMgr.StopMotionScript();

            if (!maid.boMAN)
            {
                maid.body0.MuneYureL((float)((!timeline.useMuneKeyL) ? 1 : 0));
                maid.body0.MuneYureR((float)((!timeline.useMuneKeyR) ? 1 : 0));
                maid.body0.jbMuneL.enabled = !timeline.useMuneKeyL;
                maid.body0.jbMuneR.enabled = !timeline.useMuneKeyR;
            }

            maid.body0.CrossFade(id.ToString(), anmData, false, false, false, 0f, 1f);

            for (int i = 0; i < autoTwists.Length; i++)
            {
                maid.SetAutoTwist(autoTwists[i], true);
            }

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

            OnMotionUpdated(motionRate);
        }

        public void OnMotionUpdated()
        {
            var motionRate = SH.motionSliderRate;
            OnMotionUpdated(motionRate);
        }

        public void OnMotionUpdated(float motionRate)
        {
            SH.motionWindow.UpdateAnimationData(maid);
            SH.poseEditWindow.OnMotionUpdate(maid);
            if (SH.isMotionPlaying)
            {
                motionRate += 0.01f; // モーション再生中は再生位置に差分がないと反映されない
            }
            SH.motionSliderRate = motionRate;
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

                var motionWindow = SH.motionWindow;
                if (!isExist)
                {
                    motionWindow.AddMyPose(anmPath);
                }
                else
                {
                    motionWindow.OnUpdateMyPose(anmPath);
                }

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

            var boneDataArray = SH.ikManager.cache_bone_data;
            if (boneDataArray == null)
            {
                Extensions.LogError("ボーンデータが取得できませんでした");
                return null;
            }

            return boneDataArray;
        }

        public void AddKeyFrame()
        {
            var boneDataArray = GetCacheBoneDataArray();
            if (boneDataArray == null)
            {
                return;
            }

            timeline.UpdateFrame(currentFrameNo, boneDataArray);

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

        public bool IsSelectedFrame(FrameData frame)
        {
            return selectedRefFrames.Contains(frame);
        }

        public bool IsSelectedBone(FrameData frame, BoneData bone)
        {
            return selectedBones.Contains(bone);
        }

        public void SelectFrame(FrameData frame, bool isMultiSelect)
        {
            // 通常選択動作
            if (!isMultiSelect)
            {
                if (!selectedRefFrames.Contains(frame))
                {
                    selectedRefFrames.Clear();
                    selectedRefFrames.Add(frame);
                }
            }
            // 複数選択動作
            else
            {
                if (selectedRefFrames.Contains(frame))
                {
                    selectedRefFrames.Remove(frame);
                }
                else
                {
                    selectedRefFrames.Add(frame);
                }
            }
        }

        public void SelectFramesRange(int startFrameNo, int endFrameNo)
        {
            var selectedMenuItems = boneMenuManager.GetSelectedItems();
            
            if (config.isEasyEdit)
            {
                for (int i = startFrameNo; i <= endFrameNo; i++)
                {
                    var frame = timeline.GetFrame(i);
                    if (frame != null)
                    {
                        foreach (var boneMenuItem in selectedMenuItems)
                        {
                            if (boneMenuItem.HasBone(frame))
                            {
                                selectedRefFrames.Add(frame);
                            }
                        }
                    }
                }
            }
            else
            {
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

        public void SelectBones(FrameData frame, List<BoneData> bones, bool isMultiSelect)
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
            return selectedRefFrames.Count > 0 || selectedBones.Count > 0;
        }

        public void UnselectAll()
        {
            selectedRefFrames.Clear();
            selectedBones.Clear();
        }

        public void SelectAllFrames()
        {
            if (config.isEasyEdit)
            {
                selectedRefFrames.Clear();
                foreach (var frame in timeline.keyFrames)
                {
                    selectedRefFrames.Add(frame);
                }
            }
            else
            {
                selectedBones.Clear();
                foreach (var frame in timeline.keyFrames)
                {
                    selectedBones.UnionWith(frame.bones);
                }
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
            if (config.isEasyEdit)
            {
                foreach (var selectedFrame in selectedRefFrames)
                {
                    timeline.RemoveFrame(selectedFrame.frameNo);
                }
                selectedRefFrames.Clear();
            }
            else
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
            }

            ApplyCurrentFrame(true);
        }

        public void MoveSelectedFrames(int delta)
        {
            if (config.isEasyEdit)
            {
                MoveSelectedRefFrames(delta);
            }
            else
            {
                MoveSelectedBones(delta);
            }
        }

        private void MoveSelectedRefFrames(int delta)
        {
            var otherFrameMap = timeline.keyFrames
                    .Except(selectedRefFrames)
                    .ToDictionary(frame => frame.frameNo);

            foreach (var frame in selectedRefFrames)
            {
                // 移動先のフレームが重複していたら移動しない
                if (otherFrameMap.ContainsKey(frame.frameNo + delta))
                {
                    return;
                }

                // マイナスフレームに移動しようとしたら移動しない
                if (frame.frameNo + delta < 0)
                {
                    return;
                }

                // 移動先のフレーム番号が最大フレーム番号を超えたら移動しない
                if (frame.frameNo + delta > timeline.maxFrameNo)
                {
                    return;
                }
            }

            foreach (var frame in selectedRefFrames)
            {
                frame.frameNo += delta;
            }

            ApplyCurrentFrame(true);
        }

        private void MoveSelectedBones(int delta)
        {
            foreach (var selectedBone in selectedBones)
            {
                var selectedFrame = selectedBone.parentFrame;
                var targetFrame = timeline.GetFrame(selectedFrame.frameNo + delta);

                // 移動先のボーンが重複していたら移動しない
                if (targetFrame != null)
                {
                    var targetBone = targetFrame.GetBone(selectedBone.bonePath);
                    if (targetBone != null && !IsSelectedBone(targetFrame, targetBone))
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
            UnselectAll();

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

            if (config.isEasyEdit)
            {
                copyFrameData.frames = selectedRefFrames.ToList();
            }
            else
            {
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
            }

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
                SH.isMotionPlaying = true;
            }
        }

        public void Stop()
        {
            SH.isMotionPlaying = false;
        }

        public void OnStartPoseEditing()
        {
            if (maid == null)
            {
                Extensions.LogError("メイドが配置されていません");
                return;
            }

            ApplyCurrentFrame(false);

            var boneDataArray = SH.ikManager.cache_bone_data;
            if (boneDataArray == null)
            {
                Extensions.LogError("ボーンデータが取得できませんでした");
                return;
            }

            initialEditFrame = new FrameData(currentFrameNo);
            initialEditFrame.SetCacheBoneDataArray(boneDataArray);

            for (int i = 0; i < initialEditIkPositions.Length; i++)
            {
                var dragPoint = SH.GetDragPoint((IKHoldType)i);
                if (dragPoint != null)
                {
                    initialEditIkPositions[i] = dragPoint.transform.position;
                }
            }
        }

        public void OnEndPoseEditing()
        {
            initialEditFrame = null;
        }
    }
}