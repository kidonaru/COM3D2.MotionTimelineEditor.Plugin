using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;

namespace COM3D2.MotionTimelineEditor.Plugin
{
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

        [XmlElement("UseMuneKeyL")]
        public bool useMuneKeyL;

        [XmlElement("UseMuneKeyR")]
        public bool useMuneKeyR;

        [XmlElement("IsLoopAnm")]
        public bool isLoopAnm = true;

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

        [XmlElement("VideoGUIPosition")]
        public Vector2 videoGUIPosition = new Vector2(0, 0);

        [XmlElement("VideoGUIScale")]
        public float videoGUIScale = 1f;

        [XmlElement("VideoGUIAlpha")]
        public float videoGUIAlpha = 1f;

        [XmlElement("VideoBackmostPosition")]
        public Vector2 videoBackmostPosition = new Vector2(0, 0);

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
        }
    }
}