using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    using AttachPoint = PhotoTransTargetObject.AttachPoint;

    public enum SingleFrameType
    {
        None, // なし
        Delay, // 1F遅らせる
        Advance, // 1F早める
    }

    public class TimelineModelData
    {
        public string name;
        public AttachPoint attachPoint;
        public int attachMaidSlotNo = -1;
        public string pluginName;

        public TimelineModelData()
        {
        }

        public TimelineModelData(StudioModelStat model)
        {
            FromModel(model);
        }

        public void FromModel(StudioModelStat model)
        {
            name = model.name;
            attachPoint = model.attachPoint;
            attachMaidSlotNo = model.attachMaidSlotNo;
            pluginName = model.pluginName;
        }

        public void FromXml(TimelineModelXml xml)
        {
            name = xml.name;
            attachPoint = xml.attachPoint;
            attachMaidSlotNo = xml.attachMaidSlotNo;
            pluginName = xml.pluginName;
        }

        public TimelineModelXml ToXml()
        {
            var xml = new TimelineModelXml
            {
                name = name,
                attachPoint = attachPoint,
                attachMaidSlotNo = attachMaidSlotNo,
                pluginName = pluginName,
            };
            return xml;
        }
    }

    public class TimelineLightData
    {
        public string name;
        public LightType type;

        public TimelineLightData()
        {
        }

        public TimelineLightData(StudioLightStat light)
        {
            FromStat(light);
        }

        public void FromStat(StudioLightStat light)
        {
            name = light.name;
            type = light.type;
        }

        public void FromXml(TimelineLightXml xml)
        {
            name = xml.name;
            type = xml.type;
        }

        public TimelineLightXml ToXml()
        {
            var xml = new TimelineLightXml
            {
                name = name,
                type = type,
            };
            return xml;
        }
    }

    public class TimelineBGModelData
    {
        public string sourceName;
        public int group;

        public string name
        {
            get
            {
                var groupSuffix = PluginUtils.GetGroupSuffix(group);
                return sourceName + groupSuffix;
            }
        }

        public TimelineBGModelData()
        {
        }

        public TimelineBGModelData(BGModelStat model)
        {
            FromModel(model);
        }

        public void FromModel(BGModelStat model)
        {
            sourceName = model.sourceName;
            group = model.group;
        }

        public void FromXml(TimelineBGModelXml xml)
        {
            sourceName = xml.sourceName;
            group = xml.group;
        }

        public TimelineBGModelXml ToXml()
        {
            var xml = new TimelineBGModelXml
            {
                sourceName = sourceName,
                group = group,
            };
            return xml;
        }
    }

    public class TimelinePsylliumData
    {
        public int areaCount;
        public int patternCount;

        public TimelinePsylliumData()
        {
        }

        public TimelinePsylliumData(PsylliumController controller)
        {
            FromController(controller);
        }

        public void FromController(PsylliumController controller)
        {
            areaCount = controller.areas.Count;
            patternCount = controller.patterns.Count;
        }

        public void FromXml(TimelinePsylliumXml xml)
        {
            areaCount = xml.areaCount;
            patternCount = xml.patternCount;
        }

        public TimelinePsylliumXml ToXml()
        {
            var xml = new TimelinePsylliumXml
            {
                areaCount = areaCount,
                patternCount = patternCount,
            };
            return xml;
        }
    }

    public class TimelinePngObjectData
    {
        public string imageName;
        public int group;
        public int primitive;
        public bool squareUV;
        public string shaderDisplay;
        public int renderQueue;

        public string name
        {
            get
            {
                var groupSuffix = PluginUtils.GetGroupSuffix(group);
                return imageName + groupSuffix;
            }
        }

        public TimelinePngObjectData()
        {
        }

        public void FromXml(TimelinePngObjectXml xml)
        {
            imageName = xml.imageName;
            group = xml.group;
            primitive = xml.primitive;
            squareUV = xml.squareUV;
            shaderDisplay = xml.shaderDisplay;
            renderQueue = xml.renderQueue;
        }

        public TimelinePngObjectXml ToXml()
        {
            var xml = new TimelinePngObjectXml
            {
                imageName = imageName,
                group = group,
                primitive = primitive,
                squareUV = squareUV,
                shaderDisplay = shaderDisplay,
                renderQueue = renderQueue,
            };
            return xml;
        }
    }

    public class TimelineData
    {
        public static readonly int CurrentVersion = 26;
        public static readonly TimelineData DefaultTimeline = new TimelineData();

        public int version = 0;

        public List<ITimelineLayer> layers = new List<ITimelineLayer>(16);
        public Dictionary<Type, List<ITimelineLayer>> layersMap = new Dictionary<Type, List<ITimelineLayer>>(16);

        public List<TrackData> tracks = new List<TrackData>(16);
        public List<TimelineModelData> models = new List<TimelineModelData>(16);
        public List<TimelineLightData> lights = new List<TimelineLightData>(16);
        public List<TimelineBGModelData> bgModels = new List<TimelineBGModelData>(16);
        public List<TimelinePsylliumData> psylliums = new List<TimelinePsylliumData>(16);
        public List<TimelinePngObjectData> pngObjects = new List<TimelinePngObjectData>(16);

        // maidSlotNo -> shapeKeys
        public Dictionary<int, HashSet<string>> maidShapeKeysMap = new Dictionary<int, HashSet<string>>();

        // maidSlotNo -> extendBoneNames
        public Dictionary<int, HashSet<string>> extendBoneNamesMap = new Dictionary<int, HashSet<string>>();

        private int _maxFrameNo = 30;
        public int maxFrameNo
        {
            get => _maxFrameNo;
            set
            {
                if (_maxFrameNo == value)
                {
                    return;
                }

                _maxFrameNo = value;
                AdjustMaxFrameNo();
            }
        }

        private float _frameRate = 30f;
        private float _frameDuration = 1f / 30f;

        public float frameRate
        {
            get => _frameRate;
            set
            {
                if (_frameRate == value)
                {
                    return;
                }
                _frameRate = Mathf.Max(1f, value);
                _frameDuration = 1f / _frameRate;
            }
        }

        public float frameDuration => _frameDuration;

        public string anmName = "";

        public string directoryName = "";

        private bool _useMuneKeyL = false;
        public bool useMuneKeyL
        {
            get => _useMuneKeyL;
            set
            {
                if (_useMuneKeyL == value)
                {
                    return;
                }

                _useMuneKeyL = value;
                maidManager.UpdateMuneYure();
                studioHack.useMuneKeyL = value;
            }
        }

        private bool _useMuneKeyR = false;
        public bool useMuneKeyR
        {
            get => _useMuneKeyR;
            set
            {
                if (_useMuneKeyR == value)
                {
                    return;
                }

                _useMuneKeyR = value;
                maidManager.UpdateMuneYure();
                studioHack.useMuneKeyR = value;
            }
        }

        private bool _useHeadKey = false;
        public bool useHeadKey
        {
            get => _useHeadKey;
            set
            {
                if (_useHeadKey == value)
                {
                    return;
                }

                _useHeadKey = value;
                maidManager.UpdateHeadLook();
            }
        }

        public int maxExistFrameNo
        {
            get
            {
                var frameNo = 0;
                foreach (var layer in layers)
                {
                    frameNo = Math.Max(frameNo, layer.maxExistFrameNo);
                }
                return frameNo;
            }
        }

        public bool isLoopAnm = true;

        private bool _isBackgroundVisible = true;
        public bool isBackgroundVisible
        {
            get => _isBackgroundVisible;
            set
            {
                if (_isBackgroundVisible == value)
                {
                    return;
                }

                _isBackgroundVisible = value;
                studioHack.SetBackgroundVisible(value);
            }
        }

        public float startOffsetTime = 0.5f;
        public float endOffsetTime = 0.5f;
        public float startFadeTime = 0.1f;
        public float endFadeTime = 0f;

        private Maid.EyeMoveType _eyeMoveType = Maid.EyeMoveType.無し;
        public Maid.EyeMoveType eyeMoveType
        {
            get => _eyeMoveType;
            set
            {
                if (_eyeMoveType == value)
                {
                    return;
                }

                _eyeMoveType = value;
                maidManager.UpdateHeadLook();
            }
        }

        public SingleFrameType singleFrameType = SingleFrameType.Delay;

        public bool isTangentCamera = false;
        public bool isTangentLight = false;
        public bool isTangentMove = false;

        public bool isLightColorEasing = true;
        public bool isLightExtraEasing = false;
        public bool isLightCompatibilityMode = true;

        public List<int> stageLaserCountList = new List<int>();
        public List<int> stageLightCountList = new List<int>();

        public int activeTrackIndex = -1;

        public string bgmPath = "";
        public float bpm = 120f;
        public bool isShowBPMLine = false;
        public float bpmLineOffsetFrame = 0f;

        public float aspectWidth = 0f;
        public float aspectHeight = 0f;
        public float letterBoxAlpha = 1f;
        public int textCount = 1;
        public bool fingerBlendEnabled = true;
        public bool usePostEffectExtraColor = false;
        public bool usePostEffectExtraBlend = false;
        public int paraffinCount = 1;
        public int distanceFogCount = 1;
        public int rimlightCount = 1;

        // 動画
        public bool videoEnabled = true;
        public VideoDisplayType videoDisplayType = VideoDisplayType.GUI;
        public string videoPath = "";
        public Vector3 videoPosition = new Vector3(0, 0, 0);
        public Vector3 videoRotation = new Vector3(0, 0, 0);
        public float videoScale = 1f;
        public float videoStartTime = 0f;
        public float videoVolume = 0.5f;
        public float videoAlpha = 1f;
        public Vector2 videoGUIPosition = new Vector2(0, 0);
        public float videoGUIScale = 1f;
        public float videoGUIAlpha = 1f;
        public Vector2 videoBackmostPosition = new Vector2(0, 0);
        public float videoBackmostScale = 1f;
        public float videoBackmostAlpha = 0.5f;
        public Vector2 videoFrontmostPosition = new Vector2(-0.8f, 0.8f);
        public float videoFrontmostScale = 0.38f;
        public float videoFrontmostAlpha = 1f;

        public int maxFrameCount => maxFrameNo + 1;

        private static TimelineManager timelineManager => TimelineManager.instance;

        private static MaidManager maidManager => MaidManager.instance;

        private static StudioHackBase studioHack => StudioHackManager.instance.studioHack;

        public TrackData activeTrack
        {
            get
            {
                if (activeTrackIndex < 0 || activeTrackIndex >= tracks.Count)
                {
                    return null;
                }
                return tracks[activeTrackIndex];
            }
        }

        public ITimelineLayer defaultLayer { get; private set; }

        public string timelinePath => PluginUtils.GetTimelinePath(anmName, directoryName);
        public string thumPath => PluginUtils.ConvertThumPath(timelinePath);
        public string dcmSongName => "【MTE】" + anmName;
        public string dcmSongListPath => PluginUtils.GetDcmSongListFilePath(dcmSongName);

        public float aspectRatio
        {
            get
            {
                if (aspectHeight == 0f)
                {
                    return 0f;
                }
                return aspectWidth / aspectHeight;
            }
        }

        public TimelineData()
        {
        }

        public void Dispose()
        {
            foreach (var layer in layers)
            {
                layer.Dispose();
            }
            layers.Clear();
            layersMap.Clear();
        }

        public float GetFrameTimeSeconds(int frameNo)
        {
            return frameNo * _frameDuration;
        }

        public bool IsValidData(out string message)
        {
            message = "";

            if (maidManager.maid == null)
            {
                message = "メイドを配置してください";
                return false;
            }

            if (anmName.Length == 0)
            {
                message = "アニメ名を入力してください";
                return false;
            }

            var activeTrack = this.activeTrack;
            if (activeTrack != null && !IsValidTrack(activeTrack))
            {
                message = "トラックの範囲が不正です";
                return false;
            }

            foreach (var layer in layers)
            {
                if (!layer.IsValidData())
                {
                    message = layer.errorMessage;
                    return false;
                }
            }

            return true;
        }

        public bool IsValidTrack(TrackData track)
        {
            if (track == null)
            {
                return false;
            }

            if (track.startFrameNo < 0 ||
                track.endFrameNo > maxFrameNo ||
                track.startFrameNo >= activeTrack.endFrameNo - 1)
            {
                return false;
            }

            return true;
        }

        public Texture2D CreateBGTexture(
            int frameWidth,
            int frameHeight,
            int width,
            int height,
            Color bgColor1,
            Color bgColor2,
            Color frameLineColor1,
            Color frameLineColor2,
            int frameNoInterval)
        {
            var tex = new Texture2D(width, height);
            var pixels = new Color[width * height];

            for (int x = 0; x < width; x++)
            {
                var frameNo = x / frameWidth;
                var framePos = x - frameNo * frameWidth;
                bool isSecondColorLine = frameNo % frameNoInterval == 0;
                bool isCenterLine = framePos == frameWidth / 2 ||
                        (isSecondColorLine && framePos == frameWidth / 2 + 1);

                for (int y = 0; y < height; y++)
                {
                    if (isCenterLine)
                    {
                        pixels[y * width + x] = isSecondColorLine ? frameLineColor2 : frameLineColor1;
                    }
                    else
                    {
                        bool isSecondColorBg = (y / frameHeight) % 2 == 1;
                        pixels[y * width + x] = isSecondColorBg ? bgColor2 : bgColor1;
                    }
                }
            }
            tex.SetPixels(pixels);
            tex.Apply();
            return tex;
        }

        public void Initialize()
        {
            if (layers.Count() == 0)
            {
                var layer = timelineManager.CreateLayer(typeof(MotionTimelineLayer), 0);
                AddLayer(layer);
            }

            defaultLayer = layers[0];

            version = CurrentVersion;
        }

        public void LayerInit()
        {
            MTEUtils.LogDebug("TimelineData.LayerInit");
            foreach (var layer in layers)
            {
                layer.Init();
            }
        }

        public void AddLayer(ITimelineLayer layer)
        {
            layers.Add(layer);
            layersMap.GetOrCreate(layer.layerType).Add(layer);
        }

        public void RemoveLayer(ITimelineLayer layer)
        {
            layers.Remove(layer);
            layersMap.GetOrCreate(layer.layerType).Remove(layer);
        }

        public List<ITimelineLayer> FindLayers(Type type)
        {
            return layersMap.GetOrCreate(type);
        }

        public void ResetSettings()
        {
            //maxFrameNo = DefaultTimeline.maxFrameNo;
            frameRate = DefaultTimeline.frameRate;
            useMuneKeyL = DefaultTimeline.useMuneKeyL;
            useMuneKeyR = DefaultTimeline.useMuneKeyR;
            isLoopAnm = DefaultTimeline.isLoopAnm;
            isBackgroundVisible = DefaultTimeline.isBackgroundVisible;
            startOffsetTime = DefaultTimeline.startOffsetTime;
            endOffsetTime = DefaultTimeline.endOffsetTime;
            startFadeTime = DefaultTimeline.startFadeTime;
            endFadeTime = DefaultTimeline.endFadeTime;
        }

        public string GetDcmSongFilePath(string fileName)
        {
            return PluginUtils.GetDcmSongFilePath(dcmSongName, fileName);
        }

        public void OnPluginDisable()
        {
            foreach (var layer in layers)
            {
                layer.OnPluginDisable();
            }

            studioHack?.SetBackgroundVisible(true);
        }

        public void OnCopyModel(StudioModelStat sourceModel, StudioModelStat newModel)
        {
            foreach (var layer in layers)
            {
                layer.OnCopyModel(sourceModel, newModel);
            }
        }

        public bool HasMaidShapeKey(int maidSlotNo, string shapeKey)
        {
            var shapeKeys = GetMaidShapeKeys(maidSlotNo);
            return shapeKeys.Contains(shapeKey);
        }

        public HashSet<string> GetMaidShapeKeys(int maidSlotNo)
        {
            HashSet<string> shapeKeys;
            if (!maidShapeKeysMap.TryGetValue(maidSlotNo, out shapeKeys))
            {
                shapeKeys = new HashSet<string>();
                maidShapeKeysMap[maidSlotNo] = shapeKeys;
            }

            return shapeKeys;
        }

        public void AddMaidShapeKey(int maidSlotNo, string shapeKey)
        {
            var shapeKeys = GetMaidShapeKeys(maidSlotNo);
            shapeKeys.Add(shapeKey);

            foreach (var layer in layers)
            {
                if (layer.hasSlotNo && layer.slotNo == maidSlotNo)
                {
                    layer.OnShapeKeyAdded(shapeKey);
                }
            }
        }

        public void RemoveMaidShapeKey(int maidSlotNo, string shapeKey)
        {
            var shapeKeys = GetMaidShapeKeys(maidSlotNo);
            shapeKeys.Remove(shapeKey);

            foreach (var layer in layers)
            {
                if (layer.hasSlotNo && layer.slotNo == maidSlotNo)
                {
                    layer.OnShapeKeyRemoved(shapeKey);
                }
            }
        }

        public bool HasExtendBoneName(int maidSlotNo, string extendBoneName)
        {
            var boneNames = GetExtendBoneNames(maidSlotNo);
            return boneNames.Contains(extendBoneName);
        }

        public HashSet<string> GetExtendBoneNames(int maidSlotNo)
        {
            HashSet<string> boneNames;
            if (!extendBoneNamesMap.TryGetValue(maidSlotNo, out boneNames))
            {
                boneNames = new HashSet<string>();
                extendBoneNamesMap[maidSlotNo] = boneNames;
            }

            return boneNames;
        }

        public void AddExtendBoneName(int maidSlotNo, string extendBoneName)
        {
            var boneNames = GetExtendBoneNames(maidSlotNo);
            boneNames.Add(extendBoneName);

            foreach (var layer in layers)
            {
                if (layer.hasSlotNo && layer.slotNo == maidSlotNo)
                {
                    layer.OnBoneNameAdded(extendBoneName);
                }
            }
        }

        public void RemoveExtendBoneName(int maidSlotNo, string extendBoneName)
        {
            var boneNames = GetExtendBoneNames(maidSlotNo);
            boneNames.Remove(extendBoneName);

            foreach (var layer in layers)
            {
                if (layer.hasSlotNo && layer.slotNo == maidSlotNo)
                {
                    layer.OnBoneNameRemoved(extendBoneName);
                }
            }
        }

        public void AdjustMaxFrameNo()
        {
            _maxFrameNo = Mathf.Max(_maxFrameNo, maxExistFrameNo, 1);
        }

        public void FromXml(TimelineXml xml)
        {
            version = xml.version;

            tracks = new List<TrackData>(xml.tracks.Count);
            foreach (var trackXml in xml.tracks)
            {
                var track = new TrackData(trackXml);
                tracks.Add(track);
            }

            models = new List<TimelineModelData>(xml.models.Count);
            foreach (var modelXml in xml.models)
            {
                var model = new TimelineModelData();
                model.FromXml(modelXml);
                models.Add(model);
            }

            lights = new List<TimelineLightData>(xml.lights.Count);
            foreach (var lightXml in xml.lights)
            {
                var light = new TimelineLightData();
                light.FromXml(lightXml);
                lights.Add(light);
            }

            bgModels = new List<TimelineBGModelData>(xml.bgModels.Count);
            foreach (var bgModelXml in xml.bgModels)
            {
                var bgModel = new TimelineBGModelData();
                bgModel.FromXml(bgModelXml);
                bgModels.Add(bgModel);
            }

            psylliums = new List<TimelinePsylliumData>(xml.psylliums.Count);
            foreach (var psylliumXml in xml.psylliums)
            {
                var psyllium = new TimelinePsylliumData();
                psyllium.FromXml(psylliumXml);
                psylliums.Add(psyllium);
            }

            pngObjects = new List<TimelinePngObjectData>(xml.pngObjects.Count);
            foreach (var pngObjectXml in xml.pngObjects)
            {
                var pngObject = new TimelinePngObjectData();
                pngObject.FromXml(pngObjectXml);
                pngObjects.Add(pngObject);
            }

            maidShapeKeysMap.Clear();
            foreach (var shapeKeyml in xml.maidShapeKeys)
            {
                var shapeKeys = GetMaidShapeKeys(shapeKeyml.maidSlotNo);
                shapeKeys.Add(shapeKeyml.shapeKey);
            }

            extendBoneNamesMap.Clear();
            foreach (var extendBone in xml.extendBones)
            {
                var boneNames = GetExtendBoneNames(extendBone.maidSlotNo);
                foreach (var extendBoneName in extendBone.extendBoneNames)
                {
                    boneNames.Add(extendBoneName);
                }
            }

            maxFrameNo = xml.maxFrameNo;
            frameRate = xml.frameRate;
            anmName = xml.anmName;
            directoryName = xml.directoryName;
            useMuneKeyL = xml.useMuneKeyL;
            useMuneKeyR = xml.useMuneKeyR;
            useHeadKey = xml.useHeadKey;
            isLoopAnm = xml.isLoopAnm;
            isBackgroundVisible = xml.isBackgroundVisible;
            startOffsetTime = xml.startOffsetTime;
            endOffsetTime = xml.endOffsetTime;
            startFadeTime = xml.startFadeTime;
            endFadeTime = xml.endFadeTime;
            singleFrameType = xml.singleFrameType;
            isTangentCamera = xml.isTangentCamera;
            isTangentLight = xml.isTangentLight;
            isTangentMove = xml.isTangentMove;
            isLightColorEasing = xml.isLightColorEasing;
            isLightExtraEasing = xml.isLightExtraEasing;
            isLightCompatibilityMode = xml.isLightCompatibilityMode;
            stageLaserCountList = xml.stageLaserCountList.ToList();
            stageLightCountList = xml.stageLightCountList.ToList();
            activeTrackIndex = xml.activeTrackIndex;
            bgmPath = xml.bgmPath;
            bpm = xml.bpm;
            isShowBPMLine = xml.isShowBPMLine;
            bpmLineOffsetFrame = xml.bpmLineOffsetFrame;
            aspectWidth = xml.aspectWidth;
            aspectHeight = xml.aspectHeight;
            letterBoxAlpha = xml.letterBoxAlpha;
            textCount = xml.textCount;
            fingerBlendEnabled = xml.fingerBlendEnabled;
            usePostEffectExtraColor = xml.usePostEffectExtraColor;
            usePostEffectExtraBlend = xml.usePostEffectExtraBlend;
            paraffinCount = xml.paraffinCount;
            distanceFogCount = xml.distanceFogCount;
            rimlightCount = xml.rimlightCount;
            videoEnabled = xml.videoEnabled;
            videoDisplayType = xml.videoDisplayType;
            videoPath = xml.videoPath;
            videoPosition = xml.videoPosition;
            videoRotation = xml.videoRotation;
            videoScale = xml.videoScale;
            videoStartTime = xml.videoStartTime;
            videoVolume = xml.videoVolume;
            videoAlpha = xml.videoAlpha;
            videoGUIPosition = xml.videoGUIPosition;
            videoGUIScale = xml.videoGUIScale;
            videoGUIAlpha = xml.videoGUIAlpha;
            videoBackmostPosition = xml.videoBackmostPosition;
            videoBackmostScale = xml.videoBackmostScale;
            videoBackmostAlpha = xml.videoBackmostAlpha;
            videoFrontmostPosition = xml.videoFrontmostPosition;
            videoFrontmostScale = xml.videoFrontmostScale;
            videoFrontmostAlpha = xml.videoFrontmostAlpha;

            layers = new List<ITimelineLayer>(xml.layers.Count);
            foreach (var layerXml in xml.layers)
            {
                var layer = timelineManager.CreateLayer(layerXml.className, layerXml.slotNo);
                if (layer != null)
                {
                    AddLayer(layer);
                    layer.FromXml(layerXml);
                }
            }
        }

        public TimelineXml ToXml()
        {
            var stopwatch = new StopwatchDebug();

            var xml = new TimelineXml();
            xml.version = version;

            xml.layers = new List<TimelineLayerXml>(layers.Count);
            foreach (var layer in layers)
            {
                var layerXml = layer.ToXml();
                xml.layers.Add(layerXml);
            }

            xml.tracks = new List<TrackXml>(tracks.Count);
            foreach (var track in tracks)
            {
                var trackXml = track.ToXml();
                xml.tracks.Add(trackXml);
            }

            xml.models = new List<TimelineModelXml>(models.Count);
            foreach (var model in models)
            {
                var modelXml = model.ToXml();
                xml.models.Add(modelXml);
            }

            xml.lights = new List<TimelineLightXml>(lights.Count);
            foreach (var light in lights)
            {
                var lightXml = light.ToXml();
                xml.lights.Add(lightXml);
            }

            xml.bgModels = new List<TimelineBGModelXml>(bgModels.Count);
            foreach (var bgModel in bgModels)
            {
                var bgModelXml = bgModel.ToXml();
                xml.bgModels.Add(bgModelXml);
            }

            xml.psylliums = new List<TimelinePsylliumXml>(psylliums.Count);
            foreach (var psyllium in psylliums)
            {
                var psylliumXml = psyllium.ToXml();
                xml.psylliums.Add(psylliumXml);
            }

            xml.pngObjects = new List<TimelinePngObjectXml>(pngObjects.Count);
            foreach (var pngObject in pngObjects)
            {
                var pngObjectXml = pngObject.ToXml();
                xml.pngObjects.Add(pngObjectXml);
            }

            xml.maidShapeKeys = new List<TimelineMaidShapeKeyXml>();
            foreach (var pair in maidShapeKeysMap)
            {
                var maidSlotNo = pair.Key;
                foreach (var shapeKey in pair.Value)
                {
                    var shapeKeyXml = new TimelineMaidShapeKeyXml
                    {
                        maidSlotNo = maidSlotNo,
                        shapeKey = shapeKey,
                    };
                    xml.maidShapeKeys.Add(shapeKeyXml);
                }
            }

            xml.extendBones = new List<TimelineExtendBoneXml>();
            foreach (var pair in extendBoneNamesMap)
            {
                var maidSlotNo = pair.Key;
                var extendBoneXml = new TimelineExtendBoneXml
                {
                    maidSlotNo = maidSlotNo,
                    extendBoneNames = pair.Value.ToList(),
                };
                xml.extendBones.Add(extendBoneXml);
            }

            xml.maxFrameNo = maxFrameNo;
            xml.frameRate = frameRate;
            xml.anmName = anmName;
            xml.directoryName = directoryName;
            xml.useMuneKeyL = useMuneKeyL;
            xml.useMuneKeyR = useMuneKeyR;
            xml.useHeadKey = useHeadKey;
            xml.isLoopAnm = isLoopAnm;
            xml.isBackgroundVisible = isBackgroundVisible;
            xml.startOffsetTime = startOffsetTime;
            xml.endOffsetTime = endOffsetTime;
            xml.startFadeTime = startFadeTime;
            xml.endFadeTime = endFadeTime;
            xml.singleFrameType = singleFrameType;
            xml.isTangentCamera = isTangentCamera;
            xml.isTangentLight = isTangentLight;
            xml.isTangentMove = isTangentMove;
            xml.isLightColorEasing = isLightColorEasing;
            xml.isLightExtraEasing = isLightExtraEasing;
            xml.isLightCompatibilityMode = isLightCompatibilityMode;
            xml.stageLaserCountList = stageLaserCountList.ToList();
            xml.stageLightCountList = stageLightCountList.ToList();
            xml.activeTrackIndex = activeTrackIndex;
            xml.bgmPath = bgmPath;
            xml.bpm = bpm;
            xml.isShowBPMLine = isShowBPMLine;
            xml.bpmLineOffsetFrame = bpmLineOffsetFrame;
            xml.aspectWidth = aspectWidth;
            xml.aspectHeight = aspectHeight;
            xml.letterBoxAlpha = letterBoxAlpha;
            xml.textCount = textCount;
            xml.fingerBlendEnabled = fingerBlendEnabled;
            xml.usePostEffectExtraColor = usePostEffectExtraColor;
            xml.usePostEffectExtraBlend = usePostEffectExtraBlend;
            xml.paraffinCount = paraffinCount;
            xml.distanceFogCount = distanceFogCount;
            xml.rimlightCount = rimlightCount;
            xml.videoEnabled = videoEnabled;
            xml.videoDisplayType = videoDisplayType;
            xml.videoPath = videoPath;
            xml.videoPosition = videoPosition;
            xml.videoRotation = videoRotation;
            xml.videoScale = videoScale;
            xml.videoStartTime = videoStartTime;
            xml.videoVolume = videoVolume;
            xml.videoAlpha = videoAlpha;
            xml.videoGUIPosition = videoGUIPosition;
            xml.videoGUIScale = videoGUIScale;
            xml.videoGUIAlpha = videoGUIAlpha;
            xml.videoBackmostPosition = videoBackmostPosition;
            xml.videoBackmostScale = videoBackmostScale;
            xml.videoBackmostAlpha = videoBackmostAlpha;
            xml.videoFrontmostPosition = videoFrontmostPosition;
            xml.videoFrontmostScale = videoFrontmostScale;
            xml.videoFrontmostAlpha = videoFrontmostAlpha;

            stopwatch.ProcessEnd("TimelineData.ToXml");

            return xml;
        }
    }
}