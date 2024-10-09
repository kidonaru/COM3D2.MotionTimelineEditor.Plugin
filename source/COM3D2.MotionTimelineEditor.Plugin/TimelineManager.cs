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

    public partial class TimelineManager
    {
        public TimelineData timeline = null;
        public HashSet<BoneData> selectedBones = new HashSet<BoneData>();
        private int prevPlayingFrameNo = -1;
        public string errorMessage = "";
        public FrameData initialEditFrame;
        public Vector3 initialEditPosition = Vector3.zero;
        public Quaternion initialEditRotation = Quaternion.identity;
        private bool isPrevPoseEditing;

        public static event UnityAction onPlay;
        public static event UnityAction onRefresh;
        public static event UnityAction onEditPoseUpdated;
        public static event UnityAction onAnmSpeedChanged;
        public static event UnityAction onSeekCurrentFrame;

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

        private float _anmSpeed = 1.0f;
        public float anmSpeed
        {
            get
            {
                return _anmSpeed;
            }
        }

        public List<ITimelineLayer> layers
        {
            get
            {
                return timeline != null ? timeline.layers : new List<ITimelineLayer>();
            }
        }

        public int currentLayerIndex = 0;

        public ITimelineLayer currentLayer
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

        public ITimelineLayer defaultLayer
        {
            get
            {
                return timeline.defaultLayer;
            }
        }

        public bool HasCameraLayer
        {
            get
            {
                return layers.Any(layer => layer.isCameraLayer);
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
                return StudioHackManager.studioHack;
            }
        }

        private static MaidManager maidManager
        {
            get
            {
                return MaidManager.instance;
            }
        }

        private static StudioModelManager modelManager
        {
            get
            {
                return StudioModelManager.instance;
            }
        }

        private static StudioLightManager lightManager
        {
            get
            {
                return StudioLightManager.instance;
            }
        }

        private static ModelHackManager modelHackManager
        {
            get
            {
                return ModelHackManager.instance;
            }
        }

        private static IPartsEditHack partsEditHack
        {
            get
            {
                return PartsEditHackManager.instance.partsEditHack;
            }
        }

        private static Maid maid
        {
            get
            {
                return maidManager.maid;
            }
        }

        private static MaidCache maidCache
        {
            get
            {
                return maidManager.maidCache;
            }
        }

        private static Config config
        {
            get
            {
                return ConfigManager.config;
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
            SceneManager.sceneLoaded += OnChangedSceneLevel;
            MaidManager.onMaidSlotNoChanged += OnMaidSlotNoChanged;
            MaidCache.onMaidChanged += OnMaidChanged;
            StudioModelManager.onModelAdded += OnModelAdded;
            StudioModelManager.onModelRemoved += OnModelRemoved;
            StudioModelManager.onModelUpdated += OnModelUpdated;
            StudioLightManager.onLightAdded += OnLightAdded;
            StudioLightManager.onLightRemoved += OnLightRemoved;
            StudioLightManager.onLightUpdated += OnLightUpdated;
        }

        public void Update()
        {
            foreach (var layer in layers)
            {
                layer.Update();
            }

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

        public void LateUpdate()
        {
            foreach (var layer in layers)
            {
                layer.LateUpdate();
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
                timeline.Dispose();
                timeline = null;
            }
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

            ClearTimeline();
            currentLayerIndex = 0;

            timeline = new TimelineData
            {
                anmName = "テスト",
                version = TimelineData.CurrentVersion
            };
            timeline.Initialize();
            currentLayer.OnActive();

            CreateAndApplyAnmAll();
            Refresh();

            RequestHistory("タイムライン新規作成");
        }

        public void LoadTimeline(string anmName, string directoryName)
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
            if (!IsValidFileName(anmName) || !IsValidDirName(directoryName))
            {
                PluginUtils.ShowDialog(errorMessage);
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

                timeline = new TimelineData();
                timeline.FromXml(xml);
                timeline.anmName = anmName;
                timeline.directoryName = directoryName;
                timeline.Initialize();
                timeline.OnLoad();
            }

            currentLayer.OnActive();

            maidManager.ChangeMaid(currentLayer.maid);
            modelManager.SetupModels(timeline.models);
            lightManager.SetupLights(timeline.lights);

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
                PluginUtils.ShowDialog(errorMessage);
                return;
            }

            timeline.OnSave();

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

            PluginUtils.ShowDialog("タイムライン「" + timeline.anmName + "」を保存しました");
        }

        public void UpdateTimeline(TimelineXml xml)
        {
            ClearTimeline();

            timeline = new TimelineData();
            timeline.FromXml(xml);
            timeline.Initialize();
            timeline.OnLoad();

            if (currentLayerIndex >= layers.Count)
            {
                currentLayerIndex = 0;
            }
            currentLayer.OnActive();

            modelManager.SetupModels(timeline.models);
            lightManager.SetupLights(timeline.lights);

            CreateAndApplyAnmAll();
            Refresh();
        }

        public void SaveThumbnail()
        {
            if (!IsValidData())
            {
                PluginUtils.ShowDialog(errorMessage);
                return;
            }

            MTE.instance.SaveScreenShot(timeline.thumPath, config.thumWidth, config.thumHeight);

            PluginUtils.ShowDialog("サムネイルを更新しました");
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
                PluginUtils.LogWarning("範囲が不正です start={0} end={1}", startFrameNo, endFrameNo);
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
                PluginUtils.LogWarning("範囲が不正です start={0} end={1}", startFrameNo, endFrameNo);
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
                PluginUtils.LogWarning("範囲が不正です start={0} end={1}", startFrameNo, endFrameNo);
                return;
            }

            var length = endFrameNo - startFrameNo + 1;

            if (timeline.maxFrameNo - length <= 1)
            {
                PluginUtils.LogWarning("最低1フレームは残す必要があります");
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
            currentLayer.CleanFrames();
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
            UpdateTimelineModels();
            UpdateTimelineLights();

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

            ApplyCurrentFrame(false);

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
                PluginUtils.ShowDialog(errorMessage);
                return;
            }

            foreach (var layer in layers)
            {
                layer.OutputAnm();
            }

            PluginUtils.ShowDialog("モーション「" + timeline.anmName + "」を生成しました");
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
                PluginUtils.ShowDialog(errorMessage);
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
                var bgmData = PluginUtils.DefaultBgmData;

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

            PluginUtils.ShowDialog("「" + songName + "」を生成しました");
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
                PluginUtils.LogWarning("コピーするキーフレームが選択されていません");
                return;
            }

            var copyFrameData = new CopyLayerData
            {
                className = currentLayer.className
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

                PluginUtils.Log("クリップボードにコピーしました");
            }
            catch (Exception e)
            {
                PluginUtils.LogException(e);
                PluginUtils.ShowDialog("コピーに失敗しました");
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
                className = currentLayer.className,
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
                var data = GUIUtility.systemCopyBuffer;
                var serializer = new XmlSerializer(typeof(CopyLayerData));
                using (var reader = new StringReader(data))
                {
                    var copyFrameData = (CopyLayerData) serializer.Deserialize(reader);

                    if (copyFrameData.className != currentLayer.className)
                    {
                        PluginUtils.ShowDialog("ペーストするレイヤーが一致しません");
                        return;
                    }

                    if (copyFrameData.frames.Count == 0)
                    {
                        PluginUtils.LogWarning("ペーストするキーフレームがありません");
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

                ApplyCurrentFrame(true);
            }
            catch (Exception e)
            {
                PluginUtils.LogException(e);
                PluginUtils.ShowDialog("ペーストに失敗しました");
            }
        }

        public void PastePoseFromClipboard()
        {
            try
            {
                if (!studioHack.isPoseEditing)
                {
                    PluginUtils.LogWarning("編集モード中のみペーストできます");
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

                    if (copyFrameData.className != currentLayer.className)
                    {
                        PluginUtils.ShowDialog("ペーストするレイヤーが一致しません");
                        return;
                    }

                    if (copyFrameData.frames.Count == 0)
                    {
                        PluginUtils.LogWarning("ペーストするキーフレームがありません");
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
                PluginUtils.LogException(e);
                PluginUtils.ShowDialog("ペーストに失敗しました");
            }
        }

        public ITimelineLayer GetLayer(string className, int slotNo)
        {
            foreach (var layer in layers)
            {
                if (layer.className != className)
                {
                    continue;
                }

                if (layer.hasSlotNo && layer.slotNo != slotNo)
                {
                    continue;
                }

                return layer;
            }
            return null;
        }

        public void ChangeActiveLayer(string className, int slotNo)
        {
            var layer = GetLayer(className, slotNo);

            if (layer == currentLayer)
            {
                return;
            }
            if (layer != null)
            {
                SetActiveLayer(layer);
                return;
            }

            var newLayer = CreateLayer(className, slotNo);
            if (newLayer == null)
            {
                return;
            }

            layers.Add(newLayer);
            SetActiveLayer(newLayer);
            newLayer.Init();
            newLayer.CreateAndApplyAnm();

            if (partsEditHack != null)
            {
                partsEditHack.SetBone(null);
            }

            var info = GetLayerInfo(className);
            RequestHistory("「" + info.displayName + "」レイヤー新規作成");
        }

        public void SetActiveLayer(ITimelineLayer layer)
        {
            UnselectAll();

            studioHack.isPoseEditing = false;
            OnEndPoseEdit();
            isPrevPoseEditing = false;

            currentLayerIndex = layers.IndexOf(layer);
            Refresh();

            layer.OnActive();
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
            studioHack.isPoseEditing = false;

            if (this.currentFrameNo >= timeline.maxFrameNo)
            {
                SetPlayingFrameNoAll(0);
            }

            CreateAndApplyAnmAll();
            studioHack.isMotionPlaying = true;

            if (onPlay != null)
            {
                onPlay();
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

        private List<TimelineLayerInfo> layerInfoList
            = new List<TimelineLayerInfo>();

        public List<TimelineLayerInfo> GetLayerInfoList()
        {
            return layerInfoList;
        }

        public TimelineLayerInfo GetLayerInfo(string className)
        {
            foreach (var layerInfo in layerInfoList)
            {
                if (layerInfo.className == className)
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
            var index = layerInfoList.Count;
            layerInfoList.Add(new TimelineLayerInfo(
                index,
                layerType,
                createLayer
            ));
        }

        public ITimelineLayer CreateLayer(string className, int slotNo)
        {
            var layerInfo = GetLayerInfo(className);
            if (layerInfo != null)
            {
                return layerInfo.createLayer(slotNo);
            }

            PluginUtils.LogError("未登録のレイヤークラス: " + className);
            return null;
        }

        public List<T> FindLayers<T>(string className)
            where T : class
        {
            var layers = new List<T>();

            foreach (var layer in this.layers)
            {
                if (layer.className == className)
                {
                    layers.Add(layer as T);
                }
            }

            return layers;
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

        public void OnPluginEnable()
        {
            if (timeline != null)
            {
                timeline.OnPluginEnable();
                modelManager.SetupModels(timeline.models);
                lightManager.SetupLights(timeline.lights);
            }
        }

        public void OnPluginDisable()
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

                PluginUtils.LogDebug("Save Maid Position name={0} initialEditPosition={1} initialEditRotation={2}",
                    maid.name, initialEditPosition, initialEditRotation);
            }

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
                maid.transform.rotation = initialEditRotation;

                PluginUtils.LogDebug("Restore Maid Position name={0} initialEditPosition={1} initialEditRotation={2}",
                    maid.name, initialEditPosition, initialEditRotation);
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
                ChangeActiveLayer(currentLayer.className, maidSlotNo);
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

        private void UpdateTimelineModels()
        {
            if (timeline == null)
            {
                return;
            }

            var models = modelManager.models;
            var timelineModels = timeline.models;

            if (models.Count != timelineModels.Count)
            {
                timelineModels.Clear();
                foreach (var model in models)
                {
                    var timelineModel = new TimelineModelData(model);
                    timelineModels.Add(timelineModel);
                }
            }
            else
            {
                for (int i = 0; i < models.Count; i++)
                {
                    var model = models[i];
                    var timelineModel = timelineModels[i];
                    timelineModel.FromModel(model);
                }
            }
        }

        private void OnModelAdded(StudioModelStat model)
        {
            if (IsValidData())
            {
                UpdateTimelineModels();

                foreach (var layer in layers)
                {
                    layer.OnModelAdded(model);
                }
            }
        }

        private void OnModelRemoved(StudioModelStat model)
        {
            if (IsValidData())
            {
                UpdateTimelineModels();

                foreach (var layer in layers)
                {
                    layer.OnModelRemoved(model);
                }
            }
        }

        private void OnModelUpdated(StudioModelStat model)
        {
            if (IsValidData())
            {
                UpdateTimelineModels();
            }
        }

        private void UpdateTimelineLights()
        {
            if (timeline == null)
            {
                return;
            }

            var lights = lightManager.lights;
            var timelineLights = timeline.lights;

            if (lights.Count != timelineLights.Count)
            {
                timelineLights.Clear();
                foreach (var light in lights)
                {
                    var timelineLight = new TimelineLightData(light);
                    timelineLights.Add(timelineLight);
                }
            }
            else
            {
                for (int i = 0; i < lights.Count; i++)
                {
                    var light = lights[i];
                    var timelineModel = timelineLights[i];
                    timelineModel.FromStat(light);
                }
            }
        }

        private void OnLightAdded(StudioLightStat light)
        {
            if (IsValidData())
            {
                UpdateTimelineLights();

                foreach (var layer in layers)
                {
                    layer.OnLightAdded(light);
                }
            }
        }

        private void OnLightRemoved(StudioLightStat light)
        {
            if (IsValidData())
            {
                UpdateTimelineLights();

                foreach (var layer in layers)
                {
                    layer.OnLightRemoved(light);
                }
            }
        }

        private void OnLightUpdated(StudioLightStat light)
        {
            if (IsValidData())
            {
                UpdateTimelineLights();

                foreach (var layer in layers)
                {
                    layer.OnLightUpdated(light);
                }
            }
        }

        private void OnChangedSceneLevel(Scene sceneName, LoadSceneMode sceneMode)
        {
            if (timeline != null)
            {
                ClearTimeline();
                currentLayerIndex = 0;
            }
        }
    }
}