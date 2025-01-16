using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.Events;
using System.Xml.Linq;
using System.Text;
using UnityEngine.SceneManagement;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    using MTE = MotionTimelineEditor;

    public class FadeTimeLineRow
    {
        public float stTime;
        public float edTime;
        public float inTime;
        public float outTime;
        public bool isWhite;
    }

    public partial class TimelineManager : ManagerBase
    {
        private TimelineData _timeline = null;
        public override TimelineData timeline => _timeline;
        public HashSet<BoneData> selectedBones = new HashSet<BoneData>();
        private int prevPlayingFrameNo = -1;
        public string errorMessage = "";
        public FrameData initialEditFrame;
        public Vector3 initialEditPosition = Vector3.zero;
        public Quaternion initialEditRotation = Quaternion.identity;
        private bool isPrevPoseEditing;

        public static event UnityAction onPlay;
        public static event UnityAction onStop;
        public static event UnityAction onRefresh;
        public static event UnityAction onEditPoseUpdated;
        public static event UnityAction onAnmSpeedChanged;
        public static event UnityAction onSeekCurrentFrame;

        private int _currentFrameNo = 0;
        public int currentFrameNo
        {
            get => _currentFrameNo;
            set => _currentFrameNo = Mathf.Clamp(value, 0, timeline.maxFrameNo);
        }

        public float currentTime
        {
            get => currentFrameNo * timeline.frameDuration;
        }

        private float _anmSpeed = 1.0f;
        public float anmSpeed => _anmSpeed;

        public List<ITimelineLayer> layers
        {
            get => timeline != null ? timeline.layers : new List<ITimelineLayer>();
        }

        public int currentLayerIndex = 0;

        public override ITimelineLayer currentLayer
        {
            get
            {
                if (currentLayerIndex < 0 || currentLayerIndex >= layers.Count)
                {
                    return null;
                }
                return layers[currentLayerIndex];
            }
        }

        public override ITimelineLayer defaultLayer
        {
            get => timeline.defaultLayer;
        }

        public bool hasCameraLayer
        {
            get => FindLayers(typeof(CameraTimelineLayer)).Count > 0;
        }

        public bool hasPostEffectLayer
        {
            get => FindLayers(typeof(PostEffectTimelineLayer)).Count > 0;
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

        private TimelineManager()
        {
            MaidManager.onMaidSlotNoChanged += OnMaidSlotNoChanged;
            MaidCache.onMaidChanged += OnMaidChanged;
        }

        public override void Update()
        {
            if (defaultLayer.isAnmSyncing)
            {
                var playingFrameNo = defaultLayer.playingFrameNo;
                if (playingFrameNo != prevPlayingFrameNo)
                {
                    currentFrameNo = playingFrameNo;
                }
                prevPlayingFrameNo = playingFrameNo;

                var activeTrack = timeline.activeTrack;
                if (timeline.IsValidTrack(activeTrack))
                {
                    if (currentFrameNo < activeTrack.startFrameNo)
                    {
                        SetPlayingFrameNoAll(activeTrack.startFrameNo);
                    }
                    if (currentFrameNo > activeTrack.endFrameNo)
                    {
                        SetPlayingFrameNoAll(activeTrack.startFrameNo);
                    }
                    if (currentFrameNo == activeTrack.endFrameNo && defaultLayer.isAnmPlaying)
                    {
                        SetPlayingFrameNoAll(activeTrack.startFrameNo);
                    }
                }
            }

            if (defaultLayer.isAnmPlaying)
            {
                var playingFrameNo = defaultLayer.playingFrameNoFloat;
                if ((int) playingFrameNo == timeline.maxFrameNo)
                {
                    SetPlayingFrameNoAll(0);
                }

                if (!Mathf.Approximately(anmSpeed, maidManager.anmSpeed))
                {
                    SetAnmSpeedAll(anmSpeed);
                }
            }

            foreach (var layer in layers)
            {
                try
                {
                    layer.Update();
                }
                catch (Exception e)
                {
                    MTEUtils.LogException(e);
                }
            }

            var isPoseEditing = studioHackManager.isPoseEditing;
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

        public override void LateUpdate()
        {
            foreach (var layer in layers)
            {
                try
                {
                    layer.LateUpdate();
                }
                catch (Exception e)
                {
                    MTEUtils.LogException(e);
                }
            }
        }

        public bool IsValidFileName(string fileName)
        {
            errorMessage = "";

            if (fileName.Length == 0)
            {
                errorMessage = "ファイル名が入力されていません";
                return false;
            }

            if (fileName.Contains(Path.DirectorySeparatorChar.ToString()))
            {
                errorMessage = "パス区切り文字は使用できません";
                return false;
            }

            if (fileName.Contains(".."))
            {
                errorMessage = "'..'は使用できません";
                return false;
            }

            if (fileName.IndexOfAny(Path.GetInvalidPathChars()) >= 0)
            {
                errorMessage = "無効な文字が含まれています";
                return false;
            }

            return true;
        }

        public bool IsValidDirName(string dirName)
        {
            errorMessage = "";

            if (dirName.Length == 0)
            {
                return true;
            }

            if (Path.IsPathRooted(dirName))
            {
                errorMessage = "フルパスは使用できません";
                return false;
            }

            if (dirName.Contains(".."))
            {
                errorMessage = "'..'は使用できません";
                return false;
            }

            if (dirName.IndexOfAny(Path.GetInvalidPathChars()) >= 0)
            {
                errorMessage = "無効な文字が含まれています";
                return false;
            }

            return true;
        }

        public void ClearTimeline()
        {
            UnselectAll();

            if (timeline != null)
            {
                _timeline.Dispose();
                _timeline = null;
                _usingLayerInfoList = null;
                _unusingLayerInfoList = null;
            }
        }

        public void CreateNewTimeline()
        {
            if (!studioHack.IsValid())
            {
                MTEUtils.ShowDialog(studioHack.errorMessage);
                return;
            }
            if (maid == null)
            {
                MTEUtils.ShowDialog("メイドが配置されていません");
                return;
            }

            ClearTimeline();
            currentLayerIndex = 0;

            _timeline = new TimelineData
            {
                anmName = "テスト",
                version = TimelineData.CurrentVersion
            };
            _usingLayerInfoList = null;
            _unusingLayerInfoList = null;

            _timeline.Initialize();
            mte.OnLoad();
            _timeline.LayerInit();

            CreateAndApplyAnmAll();
            Refresh();

            RequestHistory("タイムライン新規作成");
        }

        public void LoadTimeline(string anmName, string directoryName)
        {
            if (!studioHack.IsValid())
            {
                MTEUtils.ShowDialog(studioHack.errorMessage);
                return;
            }
            if (maid == null)
            {
                MTEUtils.ShowDialog("メイドが配置されていません");
                return;
            }
            if (!IsValidFileName(anmName) || !IsValidDirName(directoryName))
            {
                MTEUtils.ShowDialog(errorMessage);
                return;
            }

            var path = PluginUtils.GetTimelinePath(anmName, directoryName);
            if (!File.Exists(path))
            {
                return;
            }

            ClearTimeline();
            currentLayerIndex = 0;

            using (var stream = new FileStream(path, FileMode.Open))
            {
                var serializer = new XmlSerializer(typeof(TimelineXml));
                var xml = (TimelineXml)serializer.Deserialize(stream);
                xml.Initialize();

                _timeline = new TimelineData();
                _timeline.FromXml(xml);
                _timeline.anmName = anmName;
                _timeline.directoryName = directoryName;
                _timeline.Initialize();
                mte.OnLoad();
                _timeline.LayerInit();

                _usingLayerInfoList = null;
                _unusingLayerInfoList = null;
            }

            CreateAndApplyAnmAll();
            SeekCurrentFrame(0);
            Refresh();

            RequestHistory("「" + anmName + "」読み込み");
            // Extensions.ShowDialog("タイムライン「" + anmName + "」を読み込みました");
        }

        public void SaveTimeline()
        {
            if (!IsValidData())
            {
                MTEUtils.ShowDialog(errorMessage);
                return;
            }

            var path = timeline.timelinePath;

            var dirPath = Path.GetDirectoryName(path);
            if (!Directory.Exists(dirPath))
            {
                Directory.CreateDirectory(dirPath);
            }

            using (var stream = new FileStream(path, FileMode.Create))
            {
                var serializer = new XmlSerializer(typeof(TimelineXml));
                var xml = timeline.ToXml();
                serializer.Serialize(stream, xml);
            }

            var thumPath = PluginUtils.ConvertThumPath(path);
            if (!File.Exists(thumPath))
            {
                MTE.instance.SaveScreenShot(thumPath, config.thumWidth, config.thumHeight);
            }

            MTEUtils.ShowDialog("タイムライン「" + timeline.anmName + "」を保存しました");
        }

        public void UpdateTimeline(TimelineXml xml)
        {
            ClearTimeline();

            _timeline = new TimelineData();
            _timeline.FromXml(xml);
            _timeline.Initialize();

            _usingLayerInfoList = null;
            _unusingLayerInfoList = null;

            if (currentLayerIndex >= layers.Count)
            {
                currentLayerIndex = 0;
            }

            mte.OnLoad();
            _timeline.LayerInit();

            CreateAndApplyAnmAll();
            Refresh();
        }

        public void SaveThumbnail()
        {
            if (!IsValidData())
            {
                MTEUtils.ShowDialog(errorMessage);
                return;
            }

            MTE.instance.SaveScreenShot(timeline.thumPath, config.thumWidth, config.thumHeight);

            MTEUtils.ShowDialog("サムネイルを更新しました");
        }

        public CacheBoneDataArray GetCacheBoneDataArray()
        {
            if (maid == null)
            {
                MTEUtils.LogError("メイドが配置されていません");
                return null;
            }

            var cacheBoneData = maidManager.cacheBoneData;
            if (cacheBoneData == null)
            {
                MTEUtils.LogError("ボーンデータが取得できませんでした");
                return null;
            }

            return cacheBoneData;
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
                    var frame = currentLayer.GetFrame(i);
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
                    var frame = currentLayer.GetFrame(i);
                    if (frame != null)
                    {
                        foreach (var bone in frame.bones)
                        {
                            if (selectedMenuItems.Count == 0)
                            {
                                selectedBones.Add(bone);
                            }
                            else
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
        }

        public bool IsValidFrameRnage(int startFrameNo, int endFrameNo)
        {
            if (startFrameNo == 0 && endFrameNo == 0)
            {
                return false;
            }

            if (startFrameNo < 0 || endFrameNo > timeline.maxFrameNo)
            {
                return false;
            }

            if (startFrameNo > endFrameNo)
            {
                return false;
            }

            return true;
        }

        public void InsertFrames(int startFrameNo, int endFrameNo)
        {
            if (!IsValidFrameRnage(startFrameNo, endFrameNo))
            {
                MTEUtils.LogWarning("範囲が不正です start={0} end={1}", startFrameNo, endFrameNo);
                return;
            }

            var length = endFrameNo - startFrameNo + 1;

            timeline.maxFrameNo += length;

            foreach (var layer in layers)
            {
                layer.InsertFrames(startFrameNo, endFrameNo);
            }

            ApplyCurrentFrame(true);
            Refresh();

            RequestHistory("フレーム挿入: " + startFrameNo + " - " + endFrameNo);
        }

        public void DuplicateFrames(int startFrameNo, int endFrameNo)
        {
            if (!IsValidFrameRnage(startFrameNo, endFrameNo))
            {
                MTEUtils.LogWarning("範囲が不正です start={0} end={1}", startFrameNo, endFrameNo);
                return;
            }

            var length = endFrameNo - startFrameNo + 1;

            timeline.maxFrameNo += length;

            foreach (var layer in layers)
            {
                layer.DuplicateFrames(startFrameNo, endFrameNo);
            }

            ApplyCurrentFrame(true);
            Refresh();

            RequestHistory("フレーム複製: " + startFrameNo + " - " + endFrameNo);
        }

        public void DeleteFrames(int startFrameNo, int endFrameNo)
        {
            if (!IsValidFrameRnage(startFrameNo, endFrameNo))
            {
                MTEUtils.LogWarning("範囲が不正です start={0} end={1}", startFrameNo, endFrameNo);
                return;
            }

            var length = endFrameNo - startFrameNo + 1;

            if (timeline.maxFrameNo - length <= 1)
            {
                MTEUtils.LogWarning("最低1フレームは残す必要があります");
                return;
            }

            UnselectAll();

            foreach (var layer in layers)
            {
                layer.DeleteFrames(startFrameNo, endFrameNo);
            }

            timeline.maxFrameNo -= length;

            ApplyCurrentFrame(true);
            Refresh();

            RequestHistory("フレーム削除: " + startFrameNo + " - " + endFrameNo);
        }

        private FrameData FindFrame(int start, int step)
        {
            if (config.isEasyEdit)
            {
                for (int i = start; i >= 0 && i <= timeline.maxFrameNo; i += step)
                {
                    var frame = currentLayer.GetFrame(i);
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
                    var frame = currentLayer.GetFrame(i);
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
            foreach (var frame in currentLayer.keyFrames)
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
                MTEUtils.LogWarning("削除するキーフレームが選択されていません");
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
            currentLayer.CleanFrames();
            currentLayer.ApplyCurrentFrame(true);
            selectedBones.Clear();

            RequestHistory("キーフレーム削除");
        }

        public void MoveSelectedBones(int delta)
        {
            if (selectedBones.Count == 0)
            {
                MTEUtils.LogWarning("移動するキーフレームが選択されていません");
                return;
            }

            foreach (var selectedBone in selectedBones)
            {
                var selectedFrame = selectedBone.parentFrame;
                var targetFrame = currentLayer.GetFrame(selectedFrame.frameNo + delta);

                // 移動先のボーンが重複していたら移動しない
                if (targetFrame != null)
                {
                    var targetBone = targetFrame.GetBone(selectedBone.name);
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

                currentLayer.SetBone(targetFrameNo, selectedBone);
            }

            currentLayer.CleanFrames();
            currentLayer.ApplyCurrentFrame(true);

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

            int startFrameNo = 0;
            int endFrameNo = timeline.maxFrameNo;

            var activeTrack = timeline.activeTrack;
            if (timeline.IsValidTrack(activeTrack))
            {
                startFrameNo = activeTrack.startFrameNo;
                endFrameNo = activeTrack.endFrameNo;
            }

            frameNo = Mathf.Clamp(frameNo, startFrameNo, endFrameNo);

            if (this.currentFrameNo == frameNo)
            {
                return;
            }
            this.currentFrameNo = frameNo;

            bool isPoseEditing = studioHackManager.isPoseEditing;
            if (isPoseEditing)
            {
                OnEndPoseEdit();
            }

            ApplyCurrentFrame(false);

            if (isPoseEditing)
            {
                OnEditPoseUpdated();
            }

            if (onSeekCurrentFrame != null)
            {
                onSeekCurrentFrame();
            }
        }

        public void ApplyCurrentFrame(bool motionUpdate)
        {
            foreach (var layer in layers)
            {
                layer.ApplyCurrentFrame(motionUpdate);
            }
        }

        public void CreateAndApplyAnmAll()
        {
            foreach (var layer in layers)
            {
                layer.CreateAndApplyAnm();
            }

            if (initialEditFrame != null)
            {
                OnEditPoseUpdated();
            }
        }

        public void OutputAnm()
        {
            if (!IsValidData())
            {
                MTEUtils.ShowDialog(errorMessage);
                return;
            }

            foreach (var layer in layers)
            {
                layer.OutputAnm();
            }

            MTEUtils.ShowDialog("モーション「" + timeline.anmName + "」を生成しました");
        }

        public void SaveFadeTimeLine(
            List<FadeTimeLineRow> rows,
            string filePath)
        {
            var builder = new StringBuilder();
            builder.Append("stTime,edTime,inTime,outTime,isWhite\r\n");
            foreach (var row in rows)
            {
                builder.Append(row.stTime.ToString("0.000") + ",");
                builder.Append(row.edTime.ToString("0.000") + ",");
                builder.Append(row.inTime.ToString("0.000") + ",");
                builder.Append(row.outTime.ToString("0.000") + ",");
                builder.Append(row.isWhite ? "1" : "0");
                builder.Append("\r\n");
            }

            using (var streamWriter = new StreamWriter(filePath, false))
            {
                streamWriter.Write(builder.ToString());
            }
        }

        public void OutputDCM()
        {
            if (!IsValidData())
            {
                MTEUtils.ShowDialog(errorMessage);
                return;
            }

            var songName = timeline.dcmSongName;
            var endTime = timeline.maxFrameNo * timeline.frameDuration + timeline.startOffsetTime + timeline.endOffsetTime;

            XElement songElement = new XElement("song",
                new XAttribute("label", songName),
                new XAttribute("type", "song"),
                new XElement("folder", songName),
                new XElement("endTime", endTime)
            );

            // BGMの追加
            {
                var bgmName = "song.ogg";
                var bgmData = bundleManager.LoadBytes(bgmName);

                if (File.Exists(timeline.bgmPath))
                {
                    bgmName = Path.GetFileName(timeline.bgmPath);
                    bgmData = File.ReadAllBytes(timeline.bgmPath);
                }

                var outputBgmPath = timeline.GetDcmSongFilePath(bgmName);
                File.WriteAllBytes(outputBgmPath, bgmData);

                songElement.Add(new XElement("bgm", bgmName));
            }

            // フェードの追加
            {
                var fadeRows = new List<FadeTimeLineRow>();

                var firstFade = new FadeTimeLineRow
                {
                    stTime = 0,
                    edTime = timeline.startOffsetTime,
                    inTime = 0,
                    outTime = timeline.startFadeTime,
                    isWhite = false,
                };
                fadeRows.Add(firstFade);

                if (timeline.endFadeTime > 0f)
                {
                    var lastFade = new FadeTimeLineRow
                    {
                        stTime = endTime - timeline.endOffsetTime - timeline.endFadeTime,
                        edTime = endTime - timeline.endOffsetTime,
                        inTime = timeline.endFadeTime,
                        outTime = 0,
                        isWhite = false,
                    };
                    fadeRows.Add(lastFade);
                }

                var outputFileName = string.Format("fade.csv");
                var outputPath = timeline.GetDcmSongFilePath(outputFileName);
                SaveFadeTimeLine(fadeRows, outputPath);

                songElement.Add(new XElement("changeFade", outputFileName));
            }

            // 背景非表示
            if (!timeline.isBackgroundVisible)
            {
                songElement.Add(new XElement("changeBg", "delete"));
            }

            foreach (var layer in layers)
            {
                layer.OutputDCM(songElement);
            }

            XDocument doc = new XDocument(
                new XElement("SongList",
                    songElement
                )
            );

            doc.Save(timeline.dcmSongListPath);

            MTEUtils.ShowDialog("「" + songName + "」を生成しました");
        }

        public void AddTrack()
        {
            var trackName = "";
            for (var i = 1; i < 50; ++i)
            {
                trackName = "トラック" + i;
                if (timeline.tracks.All(track => track.name != trackName))
                {
                    break;
                }
            }

            timeline.tracks.Add(new TrackData
            {
                name = trackName,
                startFrameNo = 0,
                endFrameNo = timeline.maxFrameNo,
            });
        }

        public int GetTrackIndex(TrackData track)
        {
            return timeline.tracks.IndexOf(track);
        }

        public void SetActiveTrack(TrackData track, bool isActive)
        {
            var index = GetTrackIndex(track);
            timeline.activeTrackIndex = index >= 0 && isActive ? index : -1;

            ApplyCurrentFrame(true);

            if (timeline.activeTrackIndex >= 0)
            {
                SetPlayingFrameNoAll(track.startFrameNo);
            }
        }

        public void RemoveTrack(TrackData track)
        {
            var activeTrack = timeline.activeTrack;
            timeline.tracks.Remove(track);

            SetActiveTrack(activeTrack, true);

            RequestHistory("トラック削除");
        }

        public void MoveUpTrack(TrackData track)
        {
            var index = GetTrackIndex(track);
            if (index <= 0)
            {
                return;
            }

            var activeTrack = timeline.activeTrack;
            timeline.tracks.RemoveAt(index);
            timeline.tracks.Insert(index - 1, track);

            SetActiveTrack(activeTrack, true);
        }

        public void MoveDownTrack(TrackData track)
        {
            var index = GetTrackIndex(track);
            if (index < 0 || index >= timeline.tracks.Count - 1)
            {
                return;
            }

            var activeTrack = timeline.activeTrack;
            timeline.tracks.RemoveAt(index);
            timeline.tracks.Insert(index + 1, track);

            SetActiveTrack(activeTrack, true);
        }

        [XmlRoot("Layer")]
        public class CopyLayerData
        {
            [XmlElement("ClassName")]
            public string className;
            [XmlElement("Frame")]
            public List<FrameXml> frames;
        }

        public void CopyFramesToClipboard()
        {
            if (selectedBones.Count == 0)
            {
                MTEUtils.LogWarning("コピーするキーフレームが選択されていません");
                return;
            }

            var copyFrameData = new CopyLayerData
            {
                className = currentLayer.layerName
            };

            var tmpFrames = new Dictionary<int, FrameData>();
            foreach (var bone in selectedBones)
            {
                FrameData tmpFrame;
                if (!tmpFrames.TryGetValue(bone.frameNo, out tmpFrame))
                {
                    tmpFrame = currentLayer.CreateFrame(bone.frameNo);
                    tmpFrames[bone.frameNo] = tmpFrame;
                }

                tmpFrame.UpdateBone(bone);
            }
            copyFrameData.frames = tmpFrames.Values
                .Select(frame => frame.ToXml())
                .ToList();

            try
            {
                var serializer = new XmlSerializer(typeof(CopyLayerData));
                using (var writer = new StringWriter())
                {
                    serializer.Serialize(writer, copyFrameData);
                    var framesXml = writer.ToString();
                    GUIUtility.systemCopyBuffer = framesXml;
                }

                MTEUtils.Log("クリップボードにコピーしました");
            }
            catch (Exception e)
            {
                MTEUtils.LogException(e);
                MTEUtils.ShowDialog("コピーに失敗しました");
            }
        }

        public void CopyPoseToClipboard()
        {
            var boneDataArray = GetCacheBoneDataArray();
            if (boneDataArray == null)
            {
                return;
            }

            var tmpFrame = currentLayer.CreateFrame(currentFrameNo);
            currentLayer.UpdateFrame(tmpFrame);

            var copyFrameData = new CopyLayerData
            {
                className = currentLayer.layerName,
                frames = new List<FrameXml> { tmpFrame.ToXml() }
            };

            try
            {
                var serializer = new XmlSerializer(typeof(CopyLayerData));
                using (var writer = new StringWriter())
                {
                    serializer.Serialize(writer, copyFrameData);
                    var framesXml = writer.ToString();
                    GUIUtility.systemCopyBuffer = framesXml;
                }

                MTEUtils.Log("クリップボードにコピーしました");
            }
            catch (Exception e)
            {
                MTEUtils.LogException(e);
                MTEUtils.ShowDialog("コピーに失敗しました");
            }
        }

        public void PasteFramesFromClipboard(bool flip)
        {
            try
            {
                var data = GUIUtility.systemCopyBuffer;
                var serializer = new XmlSerializer(typeof(CopyLayerData));
                using (var reader = new StringReader(data))
                {
                    var copyFrameData = (CopyLayerData) serializer.Deserialize(reader);

                    if (copyFrameData.className != currentLayer.layerName)
                    {
                        MTEUtils.ShowDialog("ペーストするレイヤーが一致しません");
                        return;
                    }

                    if (copyFrameData.frames.Count == 0)
                    {
                        MTEUtils.LogWarning("ペーストするキーフレームがありません");
                        return;
                    }

                    var framesXml = copyFrameData.frames;
                    var minFrameNo = framesXml.Min(frame => frame.frameNo);
                    foreach (var frameXml in framesXml)
                    {
                        var tmpFrame = currentLayer.CreateFrame(frameXml);
                        if (flip)
                        {
                            tmpFrame.Flip();
                        }

                        var frameNo = currentFrameNo + tmpFrame.frameNo - minFrameNo;
                        currentLayer.UpdateBones(frameNo, tmpFrame.bones);
                    }

                    timeline.AdjustMaxFrameNo();

                    if (flip)
                    {
                        RequestHistory("反転ペースト");
                    }
                    else
                    {
                        RequestHistory("ペースト");
                    }
                }

                currentLayer.ApplyCurrentFrame(true);
            }
            catch (Exception e)
            {
                MTEUtils.LogException(e);
                MTEUtils.ShowDialog("ペーストに失敗しました");
            }
        }

        public void PastePoseFromClipboard()
        {
            try
            {
                if (!studioHackManager.isPoseEditing)
                {
                    MTEUtils.LogWarning("編集モード中のみペーストできます");
                    return;
                }

                var boneDataArray = GetCacheBoneDataArray();
                if (boneDataArray == null)
                {
                    return;
                }

                var pathDic = boneDataArray.GetPathDic();

                var data = GUIUtility.systemCopyBuffer;
                var serializer = new XmlSerializer(typeof(CopyLayerData));
                using (var reader = new StringReader(data))
                {
                    var copyFrameData = (CopyLayerData) serializer.Deserialize(reader);

                    if (copyFrameData.className != currentLayer.layerName)
                    {
                        MTEUtils.ShowDialog("ペーストするレイヤーが一致しません");
                        return;
                    }

                    if (copyFrameData.frames.Count == 0)
                    {
                        MTEUtils.LogWarning("ペーストするキーフレームがありません");
                        return;
                    }

                    var framesXml = copyFrameData.frames;
                    foreach (var frameXml in framesXml)
                    {
                        var tmpFrame = currentLayer.CreateFrame(frameXml);

                        foreach (var tmpBone in tmpFrame.bones)
                        {
                            var path = maidCache.GetBonePath(tmpBone.name);
                            CacheBoneDataArray.BoneData bone;
                            if (pathDic.TryGetValue(path, out bone))
                            {
                                if (tmpBone.transform.hasRotation)
                                {
                                    bone.transform.localRotation = tmpBone.transform.rotation;
                                }
                                if (tmpBone.transform.hasPosition)
                                {
                                     bone.transform.localPosition = tmpBone.transform.position;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                MTEUtils.LogException(e);
                MTEUtils.ShowDialog("ペーストに失敗しました");
            }
        }

        public ITimelineLayer GetLayer(Type layerType, int slotNo = 0)
        {
            foreach (var layer in FindLayers(layerType))
            {
                if (layer.hasSlotNo && layer.slotNo != slotNo)
                {
                    continue;
                }

                return layer;
            }
            return null;
        }

        public void ChangeActiveLayer(Type layerType, int slotNo = 0)
        {
            var layer = GetLayer(layerType, slotNo);

            if (layer == currentLayer)
            {
                return;
            }
            if (layer != null)
            {
                SetActiveLayer(layer);
                return;
            }

            var newLayer = CreateLayer(layerType, slotNo);
            if (newLayer == null)
            {
                return;
            }

            timeline.AddLayer(newLayer);
            _usingLayerInfoList = null;
            _unusingLayerInfoList = null;

            SetActiveLayer(newLayer);
            newLayer.Init();
            newLayer.CreateAndApplyAnm();

            if (partsEditHack != null)
            {
                partsEditHack.SetBone(null);
            }

            var info = GetLayerInfo(layerType);
            RequestHistory("「" + info.displayName + "」レイヤー新規作成");
        }

        public void RemoveLayers(Type layerType)
        {
            var targetLayers = FindLayers(layerType).ToList();
            foreach (var layer in targetLayers)
            {
                RemoveLayer(layer);
            }
        }

        public void RemoveLayer(ITimelineLayer layer)
        {
            if (layer.layerType == typeof(MotionTimelineLayer))
            {
                MTEUtils.LogWarning("アニメレイヤーは削除できません");
                return;
            }

            if (layer == null)
            {
                return;
            }

            if (currentLayer == layer)
            {
                SetActiveLayer(GetLayer(typeof(MotionTimelineLayer)));
            }

            var current = currentLayer;

            layer.Dispose();
            timeline.RemoveLayer(layer);
            _usingLayerInfoList = null;
            _unusingLayerInfoList = null;

            currentLayerIndex = layers.IndexOf(current);

            RequestHistory("「" + layer.layerName + "」レイヤー削除");
        }

        public void SetActiveLayer(ITimelineLayer layer)
        {
            UnselectAll();

            bool isPoseEditing = studioHackManager.isPoseEditing;
            if (isPoseEditing)
            {
                OnEndPoseEdit();
            }

            currentLayerIndex = layers.IndexOf(layer);
            //Refresh();

            if (isPoseEditing)
            {
                OnStartPoseEdit();
            }
        }
        
        public void SetPlayingFrameNoAll(int frameNo)
        {
            prevPlayingFrameNo = frameNo;
            currentFrameNo = frameNo;
            maidManager.SetPlayingFrameNoAll(frameNo);
        }

        public void SetAnmSpeedAll(float speed)
        {
            _anmSpeed = speed;
            maidManager.SetAnmSpeedAll(speed);

            if (onAnmSpeedChanged != null)
            {
                onAnmSpeedChanged();
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

            if (!IsValidFileName(timeline.anmName) || !IsValidDirName(timeline.directoryName))
            {
                return false;
            }

            return timeline.IsValidData(out errorMessage);
        }

        public void Play()
        {
            studioHackManager.isPoseEditing = false;

            if (this.currentFrameNo >= timeline.maxFrameNo)
            {
                SetPlayingFrameNoAll(0);
            }

            ApplyCurrentFrame(false);
            studioHack.isMotionPlaying = true;

            if (onPlay != null)
            {
                onPlay();
            }
        }

        public void Stop()
        {
            studioHack.isMotionPlaying = false;

            if (onStop != null)
            {
                onStop();
            }
        }

        private string requestedHistoryDesc = "";

        public void RequestHistory(string description)
        {
            requestedHistoryDesc = description;
        }

        private List<TimelineLayerInfo> _layerInfoList = new List<TimelineLayerInfo>();
        public List<TimelineLayerInfo> layerInfoList => _layerInfoList;

        public List<TimelineLayerInfo> _usingLayerInfoList = null;
        public List<TimelineLayerInfo> usingLayerInfoList
        {
            get
            {
                if (_usingLayerInfoList == null)
                {
                    _usingLayerInfoList = new List<TimelineLayerInfo>();
                    foreach (var layerInfo in _layerInfoList)
                    {
                        if (timeline?.FindLayers(layerInfo.layerType).Count > 0)
                        {
                            _usingLayerInfoList.Add(layerInfo);
                        }
                    }
                }
                return _usingLayerInfoList;
            }
        }

        public List<TimelineLayerInfo> _unusingLayerInfoList = null;
        public List<TimelineLayerInfo> unusingLayerInfoList
        {
            get
            {
                if (_unusingLayerInfoList == null)
                {
                    _unusingLayerInfoList = new List<TimelineLayerInfo>();
                    foreach (var layerInfo in _layerInfoList)
                    {
                        if (timeline?.FindLayers(layerInfo.layerType).Count == 0)
                        {
                            _unusingLayerInfoList.Add(layerInfo);
                        }
                    }
                }
                return _unusingLayerInfoList;
            }
        }

        public TimelineLayerInfo GetLayerInfo(Type layerType)
        {
            foreach (var layerInfo in _layerInfoList)
            {
                if (layerInfo.layerType == layerType)
                {
                    return layerInfo;
                }
            }
            return null;
        }

        public TimelineLayerInfo GetLayerInfo(string layerName)
        {
            foreach (var layerInfo in _layerInfoList)
            {
                if (layerInfo.className == layerName)
                {
                    return layerInfo;
                }
            }
            return null;
        }

        public void RegisterLayer(
            Type layerType,
            Func<int, ITimelineLayer> createLayer)
        {
            var info = new TimelineLayerInfo(
                layerType,
                createLayer
            );

            if (!info.ValidateLayer())
            {
                MTEUtils.LogError("レイヤークラスの登録に失敗しました: {0}", layerType.Name);
                return;
            }

            _layerInfoList.Add(info);

            _layerInfoList.Sort((a, b) => a.priority - b.priority);

            for (var i = 0; i < _layerInfoList.Count; i++)
            {
                _layerInfoList[i].index = i;
            }
        }

        public ITimelineLayer CreateLayer(Type layerType, int slotNo)
        {
            var layerInfo = GetLayerInfo(layerType);
            if (layerInfo != null)
            {
                return layerInfo.createLayer(slotNo);
            }

            MTEUtils.LogError("未登録のレイヤークラス: " + layerType.Name);
            return null;
        }

        public ITimelineLayer CreateLayer(string layerName, int slotNo)
        {
            var layerInfo = GetLayerInfo(layerName);
            if (layerInfo != null)
            {
                return layerInfo.createLayer(slotNo);
            }

            MTEUtils.LogError("未登録のレイヤークラス: " + layerName);
            return null;
        }

        public List<ITimelineLayer> FindLayers(Type type)
        {
            return timeline.FindLayers(type);
        }

        private Dictionary<TransformType, TransformInfo> transformInfoMap
            = new Dictionary<TransformType, TransformInfo>();

        public TransformInfo GetTransformInfo(TransformType type)
        {
            TransformInfo info;
            if (transformInfoMap.TryGetValue(type, out info))
            {
                return info;
            }
            return null;
        }

        public void RegisterTransform(TransformType type, Func<string, ITransformData> createTransform)
        {
            var info = new TransformInfo(type, createTransform);
            transformInfoMap[type] = info;
        }

        public ITransformData CreateTransform(TransformType type, string name)
        {
            var info = GetTransformInfo(type);
            if (info == null)
            {
                MTEUtils.LogError("未登録のトランスフォーム type:{0} name:{1}", type, name);
                return null;
            }

            return info.createTransform(name);
        }

        public static T CreateTransform<T>(string name)
            where T : class, ITransformData, new()
        {
            var trans = new T();
            trans.Initialize(name);
            return trans;
        }

        public void CopyModel(StudioModelStat model)
        {
            if (model == null || timeline == null)
            {
                return;
            }

            var newModel = new StudioModelStat();
            newModel.FromModel(model);

            var group = newModel.group;
            while (modelManager.GetModel(newModel.name) != null)
            {
                group++;
                if (group == 1) group++; // 1は使わない
                newModel.SetGroup(group);
            }

            timeline.OnCopyModel(model, newModel);
            modelManager.CreateModel(newModel);
        }

        public override void OnPluginDisable()
        {
            if (timeline != null)
            {
                timeline.OnPluginDisable();
            }
        }

        public void OnEditPoseUpdated()
        {
            OnEndPoseEdit();

            var frame = currentLayer.CreateFrame(currentFrameNo);
            currentLayer.UpdateFrame(frame);
            initialEditFrame = frame;

            if (maid != null)
            {
                initialEditPosition = maid.transform.position;
                initialEditRotation = maid.transform.rotation;

                //MTEUtils.LogDebug("Save Maid Position name={0} initialEditPosition={1} initialEditRotation={2}",
                //    maid.name, initialEditPosition, initialEditRotation);
            }

            onEditPoseUpdated?.Invoke();
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
                maid.transform.rotation = initialEditRotation;

                //MTEUtils.LogDebug("Restore Maid Position name={0} initialEditPosition={1} initialEditRotation={2}",
                //    maid.name, initialEditPosition, initialEditRotation);
            }

            foreach (var layer in layers)
            {
                layer.OnEndPoseEdit();
            }
        }

        private void OnMaidSlotNoChanged(int maidSlotNo)
        {
            if (IsValidData())
            {
                ChangeActiveLayer(currentLayer.layerType, maidSlotNo);
            }
        }

        private void OnMaidChanged(int maidSlotNo, Maid maid)
        {
            if (IsValidData())
            {
                foreach (var layer in layers)
                {
                    if (layer.hasSlotNo && layer.slotNo == maidSlotNo)
                    {
                        layer.OnMaidChanged(maid);
                    }
                }
            }
        }

        public override void OnChangedSceneLevel(Scene scene, LoadSceneMode sceneMode)
        {
            if (timeline != null)
            {
                ClearTimeline();
                currentLayerIndex = 0;
            }
        }
    }
}