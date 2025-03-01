using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using UnityEngine;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    public enum KeyBindType
    {
        PluginToggle,
        Visible,
        AddKeyFrame,
        AddKeyFrameAll,
        RemoveKeyFrame,
        Play,
        EditMode,
        Copy,
        Paste,
        FlipPaste,
        PoseCopy,
        PosePaste,
        PrevFrame,
        NextFrame,
        PrevKeyFrame,
        NextKeyFrame,
        MultiSelect,
        Undo,
        Redo,
    }

    public class Config
    {
        public static readonly int CurrentVersion = 2;

        [XmlAttribute]
        public int version = 0;

        // 動作設定
        public bool pluginEnabled = true;
        public bool isEasyEdit = false;
        public bool isCameraSync = true;
        public bool isFixedFoV = false;
        public bool isPostEffectSync = true;
        public bool isAutoScroll = false;
        public TangentType defaultTangentType = TangentType.Smooth;
        public MoveEasingType defaultEasingType = MoveEasingType.SineInOut;
        public int detailTransformCount = 16;
        public int detailTangentCount = 32;
        public float positionRange = 5.0f;
        public float scaleRange = 5.0f;
        public float voiceMaxLength = 20.0f;
        public bool isAutoYureBone = true;
        public bool disablePoseHistory = true;
        public int historyLimit = 20;
        public float keyRepeatTimeFirst = 0.15f;
        public float keyRepeatTime = 1f / 30f;
        public string videoShaderName = "CM3D2/Unlit_Texture_Photo_MyObject";
        public bool dofHighResolution = false;
        public bool dofNearBlur = false;
        public bool dofVisualizeFocus = false;
        public bool paraffinDebug = false;
        public bool distanceFogDebug = false;
        public bool rimlightDebug = false;
        public bool psylliumAreaCopyIgnoreTransform = false;
        public float videoPrebufferTime = 0.5f;
        public bool outputElapsedTime = false;
        public bool useHSVColor = false;
        public bool autoResisterBackgroundCustom = true;
        public string backgroundCustomCategoryName = "MotionTimelineEditor";

        // 表示設定
        public int frameWidth = 11;
        public int frameHeight = 20;
        public int frameNoInterval = 5;
        public int thumWidth = 256;
        public int thumHeight = 192;
        public int windowWidth = 640;
        public int windowHeight = 480;
        public int windowPosX = -1;
        public int windowPosY = -1;
        public int menuWidth = 100;

        // グリッド
        public bool isGridVisible = true;
        public bool isGridVisibleInDisplay = true;
        public bool isGridVisibleInWorld = true;
        public bool isGridVisibleInVideo = true;
        public bool isGridVisibleOnlyEdit = true;
        public int gridCount = 4;
        public float gridAlpha = 0.3f;
        public float gridLineWidth = 1.0f;
        public int gridCountInWorld = 20;
        public float gridAlphaInWorld = 0.3f;
        public float gridLineWidthInWorld = 1.0f;
        public float gridCellSize = 0.5f;

        // 色設定
        public Color timelineBgColor1 = new Color(0 / 255f, 0 / 255f, 0 / 255f);
        public Color timelineBgColor2 = new Color(64 / 255f, 64 / 255f, 72 / 255f);
        public Color timelineLineColor1 = new Color(127 / 255f, 127 / 255f, 127 / 255f);
        public Color timelineLineColor2 = new Color(70 / 255f, 93 / 255f, 170 / 255f);
        public Color timelineMenuBgColor = new Color(105 / 255f, 28 / 255f, 42 / 255f);
        public Color timelineMenuSelectBgColor = new Color(255 / 255f, 0 / 255f, 0 / 255f, 0.2f);
        public Color timelineMenuSelectTextColor = new Color(249 / 255f, 193 / 255f, 207/ 255f);
        public Color timelineSelectRangeColor = new Color(255 / 255f, 229 / 255f, 0/ 255f, 0.2f);
        public float timelineBgAlpha = 0.5f;
        public Color curveLineColor = new Color(101 / 255f, 154 / 255f, 210 / 255f);
        public Color curveLineSmoothColor = new Color(90 / 255f, 255 / 255f, 25 / 255f);
        public Color curveBgColor = new Color(0 / 255f, 0 / 255f, 0 / 255f, 0.3f);
        public Color windowHoverColor = new Color(48 / 255f, 48 / 255f, 48 / 255f, 224 / 255f);
        public Color gridColorInDisplay = new Color(1, 1, 1);
        public Color gridColorInWorld = new Color(1, 1, 1);
        public Color gridColorInVideo = new Color(1, 1, 1);
        public Color bpmLineColor = new Color(1f, 47f / 51f, 0.015686275f, 0.5f);

        [XmlIgnore]
        public Dictionary<KeyBindType, KeyBind> keyBinds = new Dictionary<KeyBindType, KeyBind>
        {
            { KeyBindType.PluginToggle, new KeyBind("Ctrl+M") },
            { KeyBindType.Visible, new KeyBind("Tab") },
            { KeyBindType.AddKeyFrame, new KeyBind("Return") },
            { KeyBindType.AddKeyFrameAll, new KeyBind("Shift+Return") },
            { KeyBindType.RemoveKeyFrame, new KeyBind("Backspace") },
            { KeyBindType.Play, new KeyBind("Space") },
            { KeyBindType.EditMode, new KeyBind("F1") },
            { KeyBindType.Copy, new KeyBind("Ctrl+C") },
            { KeyBindType.Paste, new KeyBind("Ctrl+V") },
            { KeyBindType.FlipPaste, new KeyBind("Ctrl+Shift+V") },
            { KeyBindType.PoseCopy, new KeyBind("Ctrl+Alt+C") },
            { KeyBindType.PosePaste, new KeyBind("Ctrl+Alt+V") },
            { KeyBindType.PrevFrame, new KeyBind("A") },
            { KeyBindType.NextFrame, new KeyBind("D") },
            { KeyBindType.PrevKeyFrame, new KeyBind("Ctrl+A") },
            { KeyBindType.NextKeyFrame, new KeyBind("Ctrl+D") },
            { KeyBindType.MultiSelect, new KeyBind("Shift") },
            { KeyBindType.Undo, new KeyBind("Ctrl+Z") },
            { KeyBindType.Redo, new KeyBind("Ctrl+X") },
        };

        public struct KeyBindPair
        {
            public KeyBindType key;
            public string value;
        }

        [XmlElement("keyBind")]
        public KeyBindPair[] keyBindsXml
        {
            get
            {
                var result = new List<KeyBindPair>(keyBinds.Count);
                foreach (var pair in keyBinds)
                {
                    result.Add(new KeyBindPair { key = pair.Key, value = pair.Value.ToString() });
                }
                return result.ToArray();
            }
            set
            {
                if (value == null)
                {
                    return;
                }

                foreach (var pair in value)
                {
                    //PluginUtils.LogDebug("keyBind: " + pair.key + " = " + pair.value);
                    keyBinds[pair.key] = new KeyBind(pair.value);
                }
            }
        }

        [XmlIgnore]
        private Dictionary<string, bool> _boneSetMenuOpenMap = new Dictionary<string, bool>();

        public struct SetMenuOpenPair
        {
            public string name;
            public bool value;
        }

        [XmlElement("boneSetMenuOpen")]
        public SetMenuOpenPair[] boneSetMenuOpenList
        {
            get
            {
                var result = new List<SetMenuOpenPair>(_boneSetMenuOpenMap.Count);
                foreach (var pair in _boneSetMenuOpenMap)
                {
                    result.Add(new SetMenuOpenPair { name = pair.Key, value = pair.Value });
                }
                return result.ToArray();
            }
            set
            {
                if (value == null)
                {
                    return;
                }
                foreach (var pair in value)
                {
                    _boneSetMenuOpenMap[pair.name] = pair.value;
                }
            }
        }

        public int subWindowCount = 1;

        [XmlIgnore]
        private Dictionary<int, SubWindowInfo> _subWindowInfoMap = new Dictionary<int, SubWindowInfo>();

        [XmlElement("subWindow")]
        public SubWindowInfo[] subWindowList
        {
            get
            {
                return _subWindowInfoMap.Values.ToArray();
            }
            set
            {
                if (value == null)
                {
                    return;
                }
                foreach (var info in value)
                {
                    _subWindowInfoMap[info.windowIndex] = info;
                }
            }
        }

        [XmlIgnore]
        public bool dirty = false;

        public TangentPair defaultTangentPair
        {
            get => TangentPair.GetDefault(defaultTangentType);
        }

        public void ConvertVersion()
        {
            version = CurrentVersion;
        }

        [XmlIgnore]
        public bool isKeyInputEnabled = true;

        public bool GetKey(KeyBindType keyBindType)
        {
            if (!isKeyInputEnabled) return false;
            return keyBinds[keyBindType].GetKey();
        }

        public bool GetKeyDown(KeyBindType keyBindType)
        {
            if (!isKeyInputEnabled) return false;
            return keyBinds[keyBindType].GetKeyDown();
        }

        public bool GetKeyDownRepeat(KeyBindType keyBindType)
        {
            if (!isKeyInputEnabled) return false;
            return keyBinds[keyBindType].GetKeyDownRepeat(keyRepeatTimeFirst, keyRepeatTime);
        }

        public bool GetKeyUp(KeyBindType keyBindType)
        {
            if (!isKeyInputEnabled) return false;
            return keyBinds[keyBindType].GetKeyUp();
        }

        public string GetKeyName(KeyBindType keyBindType)
        {
            return keyBinds[keyBindType].ToString();
        }

        public bool IsBoneSetMenuOpen(string name)
        {
            bool value;
            if (_boneSetMenuOpenMap.TryGetValue(name, out value))
            {
                return value;
            }
            return false;
        }

        public void SetBoneSetMenuOpen(string name, bool value)
        {
            _boneSetMenuOpenMap[name] = value;
        }

        public SubWindowInfo GetSubWindowInfo(int windowIndex)
        {
            SubWindowInfo info;
            if (_subWindowInfoMap.TryGetValue(windowIndex, out info))
            {
                return info;
            }

            info = new SubWindowInfo
            {
                windowIndex = windowIndex
            };
            _subWindowInfoMap[windowIndex] = info;

            return info;
        }

        public void SetSubWindowInfo(int windowIndex, SubWindowInfo info)
        {
            _subWindowInfoMap[windowIndex] = info;
        }
    }
}

