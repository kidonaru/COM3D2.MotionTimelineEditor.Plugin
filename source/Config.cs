using System.Xml.Serialization;
using UnityEngine;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    public class Config
    {
        [XmlIgnore]
        public bool dirty = false;

        // キーバインド
        public KeyCode keyPluginToggle = KeyCode.M;
        public KeyCode keyPluginToggleSub1 = KeyCode.LeftControl;
        public KeyCode keyPluginToggleSub2 = KeyCode.RightControl;
        public KeyCode keyAddKeyFrame = KeyCode.Return;
        public KeyCode keyRemoveKeyFrame = KeyCode.Backspace;
        public KeyCode keyPlay = KeyCode.Space;
        public KeyCode keyEditMode = KeyCode.Alpha1;
        public KeyCode keyPrevFrame = KeyCode.A;
        public KeyCode keyNextFrame = KeyCode.D;
        public KeyCode keyMultiSelect1 = KeyCode.LeftShift;
        public KeyCode keyMultiSelect2 = KeyCode.RightShift;

        // 設定
        public bool pluginEnabled = true;
        public bool isEasyEdit = false;
        public bool isAutoScroll = false;
        public int frameWidth = 11;
        public int frameHeight = 20;
        public int frameNoInterval = 5;
        public int thumWidth = 256;
        public int thumHeight = 256;
        public int windowPosX = -1;
        public int windowPosY = -1;

        // 色設定
        public Color timelineBgColor1 = new Color(0 / 255f, 0 / 255f, 0 / 255f);
        public Color timelineBgColor2 = new Color(64 / 255f, 64 / 255f, 72 / 255f);
        public Color timelineLineColor1 = new Color(127 / 255f, 127 / 255f, 127 / 255f);
        public Color timelineLineColor2 = new Color(70 / 255f, 93 / 255f, 170 / 255f);
        public Color timelineMenuBgColor = new Color(105 / 255f, 28 / 255f, 42 / 255f);
        public Color timelineMenuSelectBgColor = new Color(255 / 255f, 0 / 255f, 0 / 255f, 0.2f);
        public Color timelineMenuSelectTextColor = new Color(249 / 255f, 193 / 255f, 207/ 255f);
        public float timelineBgAlpha = 0.5f;
    }
}