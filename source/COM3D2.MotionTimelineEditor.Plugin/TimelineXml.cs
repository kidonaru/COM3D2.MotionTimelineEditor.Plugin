using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    using AttachPoint = PhotoTransTargetObject.AttachPoint;

    public class TimelineModelXml
    {
        [XmlElement("Name")]
        public string name;
        [XmlElement("AttachPoint")]
        public AttachPoint attachPoint;
        [XmlElement("AttachMaidSlotNo")]
        public int attachMaidSlotNo = -1;
        [XmlElement("PluginName")]
        public string pluginName;
    }

    public class TimelineLightXml
    {
        [XmlElement("Name")]
        public string name;
        [XmlElement("Type")]
        public LightType type;
    }

    public class TimelineMaidShapeKeyXml
    {
        [XmlElement("MaidSlotNo")]
        public int maidSlotNo;
        [XmlElement("ShapeKey")]
        public string shapeKey;
    }

    public class TimelineExtendBoneXml
    {
        [XmlElement("MaidSlotNo")]
        public int maidSlotNo;
        [XmlElement("ExtendBoneName")]
        public List<string> extendBoneNames;
    }

    public class TimelineBGModelXml
    {
        [XmlElement("SourceName")]
        public string sourceName;
        [XmlElement("Group")]
        public int group;
    }

    public class TimelinePngObjectXml
    {
        [XmlElement("ImageName")]
        public string imageName;
        [XmlElement("Group")]
        public int group;
        [XmlElement("Primitive")]
        public int primitive;
        [XmlElement("SquareUV")]
        public bool squareUV;
        [XmlElement("ShaderDisplay")]
        public string shaderDisplay;
        [XmlElement("RenderQueue")]
        public int renderQueue;
    }

    public class TimelinePsylliumXml
    {
        [XmlElement("AreaCount")]
        public int areaCount;
        [XmlElement("PatternCount")]
        public int patternCount;
    }

    [XmlRoot("TimelineData")]
    public class TimelineXml
    {
        [XmlAttribute("version")]
        public int version = 0;

        [XmlElement("Frame", IsNullable = true)]
        public List<FrameXml> _keyFrames = null;

        [XmlElement("Layer")]
        public List<TimelineLayerXml> layers = new List<TimelineLayerXml>();

        [XmlArray("Tracks")]
        [XmlArrayItem("Track")]
        public List<TrackXml> tracks = new List<TrackXml>();

        [XmlArray("Models")]
        [XmlArrayItem("Model")]
        public List<TimelineModelXml> models = new List<TimelineModelXml>();

        [XmlArray("Lights")]
        [XmlArrayItem("Light")]
        public List<TimelineLightXml> lights = new List<TimelineLightXml>();

        [XmlArray("BGModels")]
        [XmlArrayItem("BGModel")]
        public List<TimelineBGModelXml> bgModels = new List<TimelineBGModelXml>();

        [XmlArray("Psylliums")]
        [XmlArrayItem("Psyllium")]
        public List<TimelinePsylliumXml> psylliums = new List<TimelinePsylliumXml>();

        [XmlArray("PngObjects")]
        [XmlArrayItem("PngObject")]
        public List<TimelinePngObjectXml> pngObjects = new List<TimelinePngObjectXml>();

        [XmlArray("MaidShapeKeys")]
        [XmlArrayItem("MaidShapeKey")]
        public List<TimelineMaidShapeKeyXml> maidShapeKeys = new List<TimelineMaidShapeKeyXml>();

        [XmlArray("ExtendBones")]
        [XmlArrayItem("ExtendBone")]
        public List<TimelineExtendBoneXml> extendBones = new List<TimelineExtendBoneXml>();

        [XmlElement("MaxFrameNo")]
        public int maxFrameNo;

        [XmlElement("FrameRate")]
        public float frameRate;

        [XmlElement("AnmName")]
        public string anmName = "";

        [XmlElement("DirectoryName")]
        public string directoryName = "";

        [XmlElement("IsHold")]
        public bool[] isHoldList = new bool[(int) IKHoldType.Max];

        [XmlElement("IsIKAnime")]
        public bool isIKAnime = false;

        [XmlElement("UseMuneKeyL")]
        public bool useMuneKeyL;

        [XmlElement("UseMuneKeyR")]
        public bool useMuneKeyR;
    
        [XmlElement("UseHeadKey")]
        public bool useHeadKey = false;

        [XmlElement("IsLoopAnm")]
        public bool isLoopAnm = true;

        [XmlElement("IsBackgroundVisible")]
        public bool isBackgroundVisible = true;

        [XmlElement("StartOffsetTime")]
        public float startOffsetTime = 0.5f;

        [XmlElement("EndOffsetTime")]
        public float endOffsetTime = 0.5f;

        [XmlElement("StartFadeTime")]
        public float startFadeTime = 0.1f;

        [XmlElement("EndFadeTime")]
        public float endFadeTime = 0f;

        [XmlElement("SingleFrameType")]
        public SingleFrameType singleFrameType = SingleFrameType.Delay;

        [XmlElement("IsTangentCamera")]
        public bool isTangentCamera = false;

        [XmlElement("IsTangentLight")]
        public bool isTangentLight = false;

        [XmlElement("IsTangentMove")]
        public bool isTangentMove = false;

        [XmlElement("IsLightColorEasing")]
        public bool isLightColorEasing = true;

        [XmlElement("IsLightExtraEasing")]
        public bool isLightExtraEasing = false;

        [XmlElement("IsLightCompatibilityMode")]
        public bool isLightCompatibilityMode = true;

        [XmlElement("StageLaserCountList")]
        public List<int> stageLaserCountList = new List<int>();

        [XmlElement("StageLightCountList")]
        public List<int> stageLightCountList = new List<int>();

        [XmlElement("ActiveTrackIndex")]
        public int activeTrackIndex = -1;

        [XmlElement("BGMPath")]
        public string bgmPath = "";

        [XmlElement("BPM")]
        public float bpm = 120f;

        [XmlElement("IsShowBPMLine")]
        public bool isShowBPMLine = false;

        [XmlElement("BPMLineOffsetFrame")]
        public float bpmLineOffsetFrame = 0f;

        [XmlElement("AspectWidth")]
        public float aspectWidth = 0f;

        [XmlElement("AspectHeight")]
        public float aspectHeight = 0f;

        [XmlElement("LetterBoxAlpha")]
        public float letterBoxAlpha = 1f;

        [XmlElement("TextCount")]
        public int textCount = 1;

        [XmlElement("FingerBlendEnabled")]
        public bool fingerBlendEnabled = true;

        [XmlElement("UsePostEffectExtra")]
        public bool usePostEffectExtraColor = false;

        [XmlElement("UsePostEffectBlend")]
        public bool usePostEffectExtraBlend = false;

        [XmlElement("ParaffinCount")]
        public int paraffinCount = 1;

        [XmlElement("DistanceFogCount")]
        public int distanceFogCount = 1;

        [XmlElement("RimlightCount")]
        public int rimlightCount = 1;

        [XmlElement("VideoEnabled")]
        public bool videoEnabled = true;

        [XmlElement("VideoDisplayOnGUI")]
        public bool videoDisplayOnGUI = true;

        [XmlElement("VideoDisplayType")]
        public VideoDisplayType videoDisplayType = VideoDisplayType.GUI;

        [XmlElement("VideoPath")]
        public string videoPath = "";

        [XmlElement("VideoPosition")]
        public Vector3 videoPosition = new Vector3(0, 0, 0);

        [XmlElement("VideoRotation")]
        public Vector3 videoRotation = new Vector3(0, 0, 0);

        [XmlElement("VideoScale")]
        public float videoScale = 1f;

        [XmlElement("VideoStartTime")]
        public float videoStartTime = 0f;

        [XmlElement("VideoVolume")]
        public float videoVolume = 0.5f;

        [XmlElement("VideoAlpha")]
        public float videoAlpha = 1f;

        [XmlElement("VideoGUIPosition")]
        public Vector2 videoGUIPosition = new Vector2(0, 0);

        [XmlElement("VideoGUIScale")]
        public float videoGUIScale = 1f;

        [XmlElement("VideoGUIAlpha")]
        public float videoGUIAlpha = 1f;

        [XmlElement("VideoBackmostPosition")]
        public Vector2 videoBackmostPosition = new Vector2(0, 0);

        [XmlElement("VideoBackmostScale")]
        public float videoBackmostScale = 1f;

        [XmlElement("VideoBackmostAlpha")]
        public float videoBackmostAlpha = 0.5f;

        [XmlElement("VideoFrontmostPosition")]
        public Vector2 videoFrontmostPosition = new Vector2(-0.8f, 0.8f);

        [XmlElement("VideoFrontmostScale")]
        public float videoFrontmostScale = 0.38f;

        [XmlElement("VideoFrontmostAlpha")]
        public float videoFrontmostAlpha = 1f;

        public TimelineXml()
        {
        }

        public void Initialize()
        {
            // 旧バージョンではkeyFramesが直接格納されている
            if (_keyFrames != null && _keyFrames.Count > 0 && layers.Count == 0)
            {
                var layer = new TimelineLayerXml();
                layers.Add(layer);

                layer.className = typeof(MotionTimelineLayer).Name;
                layer.keyFrames = _keyFrames;
                _keyFrames = null;
            }

            if (version < 4)
            {
                // 旧バージョンでは動画開始時間にオフセット時間が反映されていない
                videoStartTime -= startOffsetTime;

                // 旧バージョンでは動画表示タイプがbool
                videoDisplayType = videoDisplayOnGUI ? VideoDisplayType.GUI : VideoDisplayType.Mesh;
            }

            if (version < 6)
            {
                // 旧バージョンではモデル情報が格納されていない
                models.Clear();

                foreach (var layer in layers)
                {
                    if (layer.className == "ModelTimelineLayer")
                    {
                        var modelNames = new HashSet<string>();
                        foreach (var keyFrame in layer.keyFrames)
                        {
                            foreach (var bone in keyFrame.bones)
                            {
                                modelNames.Add(bone.transform.name);
                            }
                        }

                        foreach (var modelName in modelNames)
                        {
                            var model = new TimelineModelXml
                            {
                                name = modelName
                            };
                            models.Add(model);
                        }
                        break;
                    }
                }
            }

            if (version < 7)
            {
                // BGTimelineLayerにSY/SZを追加
                foreach (var layer in layers)
                {
                    if (layer.className == "BGTimelineLayer")
                    {
                        foreach (var keyFrame in layer.keyFrames)
                        {
                            foreach (var bone in keyFrame.bones)
                            {
                                var transform = bone.transform;
                                var values = new List<float>(transform.values);
                                if (values.Count == 7)
                                {
                                    PluginUtils.LogDebug("Add SY/SZ to BGTimelineLayer name={0}", transform.name);
                                    values.Add(values[6]);
                                    values.Add(values[6]);
                                }
                                transform.values = values.ToArray();
                            }
                        }
                        break;
                    }
                }
            }

            if (version < 8)
            {
                // EyesTimelineLayerにEyesRot/LookAtTargetを追加
                foreach (var layer in layers)
                {
                    if (layer.className == "EyesTimelineLayer")
                    {
                        foreach (var keyFrame in layer.keyFrames)
                        {
                            if (keyFrame.frameNo == 0)
                            {
                                var names = new string[] { "EyesRot", "LookAtTarget" };
                                foreach (var name in names)
                                {
                                    var transform = new TransformXml
                                    {
                                        name = name,
                                        values = new float[] {},
                                    };
                                    keyFrame.bones.Add(new BoneXml
                                    {
                                        transform = transform,
                                    });
                                }
                            }
                        }
                        break;
                    }
                }
            }

            if (version < 9)
            {
                // MotionTimelineLayerのExtendBoneにposition/scaleを追加
                foreach (var layer in layers)
                {
                    if (layer.className == "MotionTimelineLayer")
                    {
                        foreach (var keyFrame in layer.keyFrames)
                        {
                            foreach (var bone in keyFrame.bones)
                            {
                                var transform = bone.transform;
                                var boneName = transform.name;
                                
                                var holdtype = MaidCache.GetIKHoldType(boneName);
                                var isDefaultBoneName = BoneUtils.IsDefaultBoneName(boneName);
                                if (holdtype != IKHoldType.Max)
                                {
                                    continue;
                                }
                                else if (boneName == MotionTimelineLayer.GroundingBoneName)
                                {
                                    continue;
                                }
                                else if (isDefaultBoneName)
                                {
                                    continue;
                                }

                                var values = new List<float>(transform.values);
                                if (values.Count == 4)
                                {
                                    PluginUtils.LogDebug("Add Position/Scale to MotionTimelineLayer name={0}", transform.name);
                                    values.Add(float.MinValue);
                                    values.Add(float.MinValue);
                                    values.Add(float.MinValue);
                                    values.Add(1f);
                                    values.Add(1f);
                                    values.Add(1f);
                                }
                                transform.values = values.ToArray();

                                if (transform.inSmoothBit == 15)
                                {
                                    transform.inSmoothBit = 1023;
                                }
                                if (transform.outSmoothBit == 15)
                                {
                                    transform.outSmoothBit = 1023;
                                }
                            }
                        }
                        break;
                    }
                }
            }

            if (version < 10)
            {
                // 前のバージョンでは次のフレームを拡張する仕様
                singleFrameType = SingleFrameType.Advance;
            }

            if (version < 11)
            {
                // LightTimelineLayerにmaidSlotIdを追加
                foreach (var layer in layers)
                {
                    if (layer.className == "LightTimelineLayer")
                    {
                        foreach (var keyFrame in layer.keyFrames)
                        {
                            foreach (var bone in keyFrame.bones)
                            {
                                var transform = bone.transform;
                                var values = new List<float>(transform.values);
                                if (values.Count == 15)
                                {
                                    PluginUtils.LogDebug("Add maidSlotId to LightTimelineLayer name={0}", transform.name);
                                    values.Add(-1f);
                                }
                                transform.values = values.ToArray();
                            }
                        }
                        break;
                    }
                }
            }

            if (version < 12)
            {
                // MotionTimelineLayerにisGroundingFootRを追加
                foreach (var layer in layers)
                {
                    if (layer.className == "MotionTimelineLayer")
                    {
                        foreach (var keyFrame in layer.keyFrames)
                        {
                            foreach (var bone in keyFrame.bones)
                            {
                                if (bone.transform.name != MotionTimelineLayer.GroundingBoneName)
                                    continue;

                                var transform = bone.transform;
                                var values = new List<float>(transform.values);
                                if (values.Count == 6)
                                {
                                    PluginUtils.LogDebug("Add isGroundingFootR to MotionTimelineLayer name={0}", transform.name);
                                    values.Add(values[0]);
                                }
                                transform.values = values.ToArray();
                            }
                        }
                        break;
                    }
                }
            }

            if (version < 13)
            {
                // MotionTimelineLayerにisAnimeを追加
                foreach (var layer in layers)
                {
                    if (layer.className == "MotionTimelineLayer")
                    {
                        foreach (var keyFrame in layer.keyFrames)
                        {
                            foreach (var bone in keyFrame.bones)
                            {
                                var transform = bone.transform;
                                var boneName = transform.name;

                                var holdtype = MaidCache.GetIKHoldType(boneName);
                                if (holdtype == IKHoldType.Max)
                                {
                                    continue;
                                }

                                var values = new List<float>(transform.values);
                                if (values.Count == 4)
                                {
                                    PluginUtils.LogDebug("Add isAnime to MotionTimelineLayer name={0}", transform.name);
                                    values.Add(isIKAnime ? 1f : 0f);
                                }
                                transform.values = values.ToArray();
                            }
                        }
                        break;
                    }
                }
            }

            if (version < 14)
            {
                // MotionTimelineLayerにFingerBlendを追加
                foreach (var layer in layers)
                {
                    if (layer.className == "MotionTimelineLayer")
                    {
                        foreach (var keyFrame in layer.keyFrames)
                        {
                            if (keyFrame.frameNo == 0)
                            {
                                foreach (var name in MotionTimelineLayer.FingerBlendBoneNames)
                                {
                                    var transform = new TransformXml
                                    {
                                        name = name,
                                        values = new float[] {},
                                    };
                                    keyFrame.bones.Add(new BoneXml
                                    {
                                        transform = transform,
                                    });
                                    PluginUtils.LogDebug("Add FingerBlend to MotionTimelineLayer name={0}", name);
                                }
                                break;
                            }
                        }
                        break;
                    }
                }
            }

            if (version < 16)
            {
                // ModelTimelineLayerにvisibleを追加
                foreach (var layer in layers)
                {
                    if (layer.className == "ModelTimelineLayer")
                    {
                        foreach (var keyFrame in layer.keyFrames)
                        {
                            foreach (var bone in keyFrame.bones)
                            {
                                var transform = bone.transform;

                                var values = new List<float>(transform.values);
                                if (values.Count == 10)
                                {
                                    PluginUtils.LogDebug("Add visible to ModelTimelineLayer name={0}", transform.name);
                                    values.Add(1f);
                                }
                                transform.values = values.ToArray();
                            }
                        }
                        break;
                    }
                }
            }

#if DEBUG
            if (version < 17)
            {
                // StageLightTimelineLayerのrotationをeulerAnglesに変更
                foreach (var layer in layers)
                {
                    if (layer.className == "StageLightTimelineLayer")
                    {
                        foreach (var keyFrame in layer.keyFrames)
                        {
                            foreach (var bone in keyFrame.bones)
                            {
                                var transform = bone.transform;
                                if (transform.name.StartsWith("StageLightController", StringComparison.Ordinal))
                                {
                                    continue;
                                }

                                var values = new List<float>(transform.values);
                                if (values.Count > 6)
                                {
                                    PluginUtils.LogDebug("Change rotation to eulerAngles in StageLightTimelineLayer name={0}", transform.name);
                                    var rotation = new Quaternion(values[3], values[4], values[5], values[6]);
                                    var eulerAngles = rotation.eulerAngles;
                                    values[3] = eulerAngles.x;
                                    values[4] = eulerAngles.y;
                                    values[5] = eulerAngles.z;
                                    values[6] = 0f; // not used
                                }
                                transform.values = values.ToArray();
                            }
                        }
                        break;
                    }
                }
            }

            if (version < 18)
            {
                // StageLightTimelineLayerのにzTest追加
                foreach (var layer in layers)
                {
                    if (layer.className == "StageLightTimelineLayer")
                    {
                        foreach (var keyFrame in layer.keyFrames)
                        {
                            foreach (var bone in keyFrame.bones)
                            {
                                var transform = bone.transform;
                                if (transform.name.StartsWith("StageLightController", StringComparison.Ordinal))
                                {
                                    var values = new List<float>(transform.values);
                                    if (values.Count == 36)
                                    {
                                        PluginUtils.LogDebug("Add zTest to StageLightTimelineLayer name={0}", transform.name);
                                        values.Add(1f);
                                    }
                                    transform.values = values.ToArray();
                                }
                                else
                                {
                                    var values = new List<float>(transform.values);
                                    if (values.Count == 22)
                                    {
                                        PluginUtils.LogDebug("Add zTest to StageLightTimelineLayer name={0}", transform.name);
                                        values.Add(1f);
                                    }
                                    transform.values = values.ToArray();
                                }
                            }
                        }
                        break;
                    }
                }
            }

            if (version < 19)
            {
                // StageLightTimelineLayerのsegmentAngle上書き
                foreach (var layer in layers)
                {
                    if (layer.className == "StageLightTimelineLayer")
                    {
                        foreach (var keyFrame in layer.keyFrames)
                        {
                            foreach (var bone in keyFrame.bones)
                            {
                                var transform = bone.transform;
                                if (transform.type == TransformType.StageLight)
                                {
                                    var values = new List<float>(transform.values);
                                    if (values.Count > 20)
                                    {
                                        PluginUtils.LogDebug("Overwrite segmentAngle in StageLightTimelineLayer name={0}", transform.name);
                                        values[20] = 10f;
                                    }
                                    transform.values = values.ToArray();
                                }
                                else
                                {
                                    var values = new List<float>(transform.values);
                                    if (values.Count > 29)
                                    {
                                        PluginUtils.LogDebug("Overwrite segmentAngle in StageLightTimelineLayer name={0}", transform.name);
                                        values[29] = 10f;
                                    }
                                    transform.values = values.ToArray();
                                }
                            }
                        }
                        break;
                    }
                }
            }

            if (version < 21)
            {
                // StageLaserTimelineLayerのintensityのindex変更
                foreach (var layer in layers)
                {
                    if (layer.className == "StageLaserTimelineLayer")
                    {
                        foreach (var keyFrame in layer.keyFrames)
                        {
                            foreach (var bone in keyFrame.bones)
                            {
                                var transform = bone.transform;
                                if (transform.type == TransformType.StageLaser)
                                {
                                    var values = new List<float>(transform.values);
                                    if (values.Count > 26)
                                    {
                                        PluginUtils.LogDebug("Change intensity index in StageLaserTimelineLayer name={0}", transform.name);
                                        values.Insert(16, values[26]);
                                        values.RemoveAt(27);
                                    }
                                    transform.values = values.ToArray();
                                }
                                else
                                {
                                    var values = new List<float>(transform.values);
                                    if (values.Count > 33)
                                    {
                                        PluginUtils.LogDebug("Change intensity index in StageLaserTimelineLayer name={0}", transform.name);
                                        values.Insert(18, values[33]);
                                        values.RemoveAt(34);
                                    }
                                    transform.values = values.ToArray();
                                }
                            }
                        }
                        break;
                    }
                }
            }

            if (version < 22)
            {
                // StageLaserTimelineLayerのposition削除
                foreach (var layer in layers)
                {
                    if (layer.className == "StageLaserTimelineLayer")
                    {
                        foreach (var keyFrame in layer.keyFrames)
                        {
                            foreach (var bone in keyFrame.bones)
                            {
                                var transform = bone.transform;
                                if (transform.type == TransformType.StageLaser)
                                {
                                    var values = new List<float>(transform.values);
                                    if (values.Count > 3)
                                    {
                                        PluginUtils.LogDebug("Remove position in StageLaserTimelineLayer name={0}", transform.name);
                                        values.RemoveRange(0, 3);
                                    }
                                    transform.values = values.ToArray();
                                }
                                else
                                {
                                    var values = new List<float>(transform.values);
                                    if (values.Count > 33)
                                    {
                                        PluginUtils.LogDebug("Fix rotation in StageLaserTimelineLayer name={0}", transform.name);
                                        var eulerAngles = new float[] {0, 0, 0};
                                        var rotationMin = new float[] {values[3], values[4], values[5]};
                                        var rotationMax = new float[] {values[6], values[7], values[8]};
                                        values.RemoveRange(3, 6);
                                        values.InsertRange(3, eulerAngles);
                                        values.InsertRange(31, rotationMin);
                                        values.InsertRange(34, rotationMax);
                                    }
                                    transform.values = values.ToArray();
                                    if (transform.inSmoothBit == -1)
                                    {
                                        transform.inSmoothBit = 137438953471;
                                    }
                                    if (transform.outSmoothBit == -1)
                                    {
                                        transform.outSmoothBit = 137438953471;
                                    }
                                }
                            }
                        }
                        break;
                    }
                }
            }
#endif

            if (version < 23)
            {
                // MoveTimelineLayerにscaleを追加
                foreach (var layer in layers)
                {
                    if (layer.className == "MoveTimelineLayer")
                    {
                        foreach (var keyFrame in layer.keyFrames)
                        {
                            foreach (var bone in keyFrame.bones)
                            {
                                var transform = bone.transform;
                                var values = new List<float>(transform.values);
                                if (values.Count == 7)
                                {
                                    PluginUtils.LogDebug("Add scale to MoveTimelineLayer name={0}", transform.name);
                                    values.Add(1f);
                                    values.Add(1f);
                                    values.Add(1f);
                                }
                                transform.values = values.ToArray();
                            }
                        }
                        break;
                    }
                }
            }

            if (version < 25)
            {
                // LightTimelineLayerにvisibleを追加
                foreach (var layer in layers)
                {
                    if (layer.className == "LightTimelineLayer")
                    {
                        foreach (var keyFrame in layer.keyFrames)
                        {
                            foreach (var bone in keyFrame.bones)
                            {
                                var transform = bone.transform;
                                var values = new List<float>(transform.values);
                                if (values.Count == 16)
                                {
                                    PluginUtils.LogDebug("Add visible to LightTimelineLayer name={0}", transform.name);
                                    values.Add(1f);
                                }
                                transform.values = values.ToArray();
                            }
                        }
                        break;
                    }
                }
            }

            if (version < 26)
            {
                // 旧バージョンではライト互換モード無効
                isLightCompatibilityMode = false;
            }
        }
    }
}