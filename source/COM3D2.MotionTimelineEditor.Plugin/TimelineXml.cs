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
        [XmlElement("Visible")]
        public bool visible = true;
    }

    public class TimelineLightXml
    {
        [XmlElement("Name")]
        public string name;
        [XmlElement("Type")]
        public LightType type;
        [XmlElement("Visible")]
        public bool visible = true;
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

        [XmlElement("ActiveTrackIndex")]
        public int activeTrackIndex = -1;

        [XmlElement("BGMPath")]
        public string bgmPath = "";

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
        public float videoBackmostAlpha = 1f;

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
        }
    }
}