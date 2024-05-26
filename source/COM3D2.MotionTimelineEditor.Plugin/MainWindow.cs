namespace COM3D2.MotionTimelineEditor.Plugin
{
    using System.Collections.Generic;
    using UnityEngine;
    using MTE = MotionTimelineEditor;

    public enum FileMenuType
    {
        New,
        Save,
        OutputAnm,
        OutputDCM,
    }

    public class MainWindow
    {
        public readonly static int WINDOW_ID = 615814;
        public readonly static int WINDOW_WIDTH = 640;
        public static int WINDOW_HEIGHT = 240;
        public readonly static int HEADER_HEIGHT = 200;

        public readonly static int WIDTH_DPOS = -WINDOW_WIDTH - 30;
        public readonly static int HEIGHT_DPOS = -WINDOW_HEIGHT - 100;

        private static StudioHackBase studioHack
        {
            get
            {
                return MTE.studioHack;
            }
        }

        private static MaidManager maidManager
        {
            get
            {
                return MaidManager.instance;
            }
        }

        private static TimelineManager timelineManager
        {
            get
            {
                return TimelineManager.instance;
            }
        }

        private static TimelineData timeline
        {
            get
            {
                return timelineManager.timeline;
            }
        }

        private static ITimelineLayer currentLayer
        {
            get
            {
                return timelineManager.currentLayer;
            }
        }

        private static Config config
        {
            get
            {
                return MTE.config;
            }
        }

        private static string anmName
        {
            get
            {
                return timeline.anmName;
            }
            set
            {
                timeline.anmName = value;
            }
        }

        public static Color timelineLabelBgColor
        {
            get
            {
                var color = config.timelineMenuBgColor;
                color.a = config.timelineBgAlpha;
                return color;
            }
        }

        private static SubWindow subWindow
        {
            get
            {
                return MTE.subWindow;
            }
        }

        private static BoneMenuManager boneMenuManager
        {
            get
            {
                return BoneMenuManager.Instance;
            }
        }

        public static int windowHeight
        {
            get
            {
                if (config.isEasyEdit)
                {
                    return 260;
                }
                else
                {
                    return 480;
                }
            }
        }

        private static int timelineViewHeight
        {
            get
            {
                return windowHeight - HEADER_HEIGHT - 20;
            }
        }

        private int ScWidth = 0, ScHeight = 0;
        private Vector2 prevWindowPos = Vector2.zero;
        public static Rect rc_stgw = new Rect(
            Screen.width + WIDTH_DPOS,
            Screen.height + HEIGHT_DPOS,
            WINDOW_WIDTH,
            WINDOW_HEIGHT
        );
        private bool initializedGUI = false;
        private bool isFrameDragging = false;
        private Vector2 frameDragDelta = Vector2.zero;
        private bool isAreaDragging = false;
        private Vector2 areaDragPos = Vector2.zero;
        private Rect areaDragRect = new Rect();
        private Vector2 scrollPosition = Vector2.zero;
        private Vector2 menuScrollPosition = Vector2.zero;
        private int selectStartFrameNo = 0;
        private int selectEndFrameNo = 0;
        public bool isMultiSelect = false;

        public class FileMenuItem
        {
            public string name;
            public System.Action action;
        }

        private GUIView headerView = new GUIView(0, 20, WINDOW_WIDTH, HEADER_HEIGHT);
        private ComboBoxValue<FileMenuType> fileMenuComboBox = new ComboBoxValue<FileMenuType>();
        private ComboBoxValue<TimelineLayerInfo> layerComboBox = new ComboBoxValue<TimelineLayerInfo>();
        private ComboBoxValue<Maid> _maidComboBox = new ComboBoxValue<Maid>();

        public GUIStyle gsWin = new GUIStyle("box")
        {
            fontSize = 12,
            alignment = TextAnchor.UpperLeft,
        };
        private GUIStyle gsFrameLabel = new GUIStyle("label")
        {
            fontSize = 12,
            alignment = TextAnchor.MiddleCenter
        };

        private Texture2D texWhite = null;
        private Texture2D texTimelineBG = null;
        private Texture2D texKeyFrame = null;

        public void Init()
        {
            texWhite = GUIView.CreateColorTexture(Color.white);

            fileMenuComboBox.label = "ファイル";
            fileMenuComboBox.items = new List<FileMenuType>
            {
                FileMenuType.New,
                FileMenuType.Save,
                FileMenuType.OutputAnm,
                FileMenuType.OutputDCM,
            };

            fileMenuComboBox.getName = (type, index) =>
            {
                switch (type)
                {
                    case FileMenuType.New:
                        return "新規作成";
                    case FileMenuType.Save:
                        return "セーブ";
                    case FileMenuType.OutputAnm:
                        return "アニメ出力";
                    case FileMenuType.OutputDCM:
                        return "DCM出力";
                    default:
                        return "";
                }
            };
            fileMenuComboBox.getEnabled = (type, index) =>
            {
                switch (type)
                {
                    case FileMenuType.Save:
                    case FileMenuType.OutputAnm:
                    case FileMenuType.OutputDCM:
                        return timelineManager.IsValidData();
                    default:
                        return true;
                }
            };
            fileMenuComboBox.onSelected = (type, index) =>
            {
                switch (type)
                {
                    case FileMenuType.New:
                        timelineManager.CreateNewTimeline();
                        break;
                    case FileMenuType.Save:
                        timelineManager.SaveTimeline();
                        break;
                    case FileMenuType.OutputAnm:
                        timelineManager.OutputAnm();
                        break;
                    case FileMenuType.OutputDCM:
                        timelineManager.OutputDCM();
                        break;
                }
            };

            layerComboBox.getName = (layerInfo, index) =>
            {
                return layerInfo.displayName;
            };
            layerComboBox.onSelected = (layerInfo, index) =>
            {
                var className = layerInfo.className;
                timelineManager.ChangeActiveLayer(className, maidManager.maidSlotNo);
            };

            _maidComboBox.getName = (maid, index) =>
            {
                var name = maid == null ? "未選択" : maid.status.fullNameJpStyle;
                return string.Format("{0}:{1}", index, name);
            };
            _maidComboBox.onSelected = (maid, index) =>
            {
                maidManager.ChangeMaid(maid);
            };
        }

        public void Update()
        {
            // 自動スクロール
            if (config.isAutoScroll &&
                currentLayer.isAnmSyncing &&
                studioHack.isMotionPlaying &&
                !Input.GetMouseButton(0))
            {
                var contentWidth = timeline.maxFrameCount * config.frameWidth;
                var viewWidth = WINDOW_WIDTH - 100;
                scrollPosition.x = Mathf.Clamp(
                    timelineManager.currentFrameNo * config.frameWidth - viewWidth / 2,
                    0,
                    Mathf.Max(0, contentWidth - viewWidth));
            }
        }

        public void UpdateTexture()
        {
            PluginUtils.LogDebug("テクスチャ作成中...");
            if (texTimelineBG != null)
            {
                UnityEngine.Object.Destroy(texTimelineBG);
                texTimelineBG = null;
            }

            var bgWidth = WINDOW_WIDTH - 100 + config.frameWidth * config.frameNoInterval;
            bgWidth = Mathf.Min(bgWidth, config.frameWidth * timeline.maxFrameCount);

            texTimelineBG = timelineManager.timeline.CreateBGTexture(
                config.frameWidth,
                config.frameHeight,
                bgWidth,
                timelineViewHeight + config.frameHeight * 2,
                config.timelineBgColor1,
                config.timelineBgColor2,
                config.timelineLineColor1,
                config.timelineLineColor2,
                config.frameNoInterval);

            if (texKeyFrame != null)
            {
                UnityEngine.Object.Destroy(texKeyFrame);
                texKeyFrame = null;
            }
            texKeyFrame = TextureUtils.CreateDiamondTexture(
                config.frameWidth,
                Color.white);
        }

        public void FixScrollPosition()
        {
            var viewWidth = WINDOW_WIDTH - 100;
            var frameWidth = config.frameWidth;

            var minScrollX = timelineManager.currentFrameNo * frameWidth - (viewWidth - 20 - frameWidth);
            var maxScrollX = timelineManager.currentFrameNo * frameWidth;
            scrollPosition.x = Mathf.Clamp(scrollPosition.x, minScrollX, maxScrollX);
        }

        public void InitGUI()
        {
            if (initializedGUI)
            {
                return;
            }
            initializedGUI = true;

            {
                var windowHoverColor = config.windowHoverColor;
                var hoverTex = GUIView.CreateColorTexture(windowHoverColor);
                gsWin.onHover.background = hoverTex;
                gsWin.hover.background = hoverTex;
                gsWin.onFocused.background = hoverTex;
                gsWin.focused.background = hoverTex;
                gsWin.onHover.textColor = Color.white;
                gsWin.hover.textColor = Color.white;
                gsWin.onFocused.textColor = Color.white;
                gsWin.focused.textColor = Color.white;
            }

            var customStyles = new List<GUIStyle>(GUI.skin.customStyles);

            var names = new string[] {
                "invisible",
                "invisiblethumb",
                "invisibleleftbutton",
                "invisiblerightbutton",
                "invisibleupbutton",
                "invisibledownbutton",
            };

            foreach (var name in names)
            {
                var style = new GUIStyle();
                style.name = name;
                style.fixedWidth = 0;
                style.fixedHeight = 0;
                style.normal.background = null;
                style.hover.background = null;
                style.active.background = null;
                customStyles.Add(style);
            }

            GUI.skin.customStyles = customStyles.ToArray();

            if (config.windowPosX != -1 && config.windowPosY != -1)
            {
                rc_stgw.x = config.windowPosX;
                rc_stgw.y = config.windowPosY;
            }
        }

        public void OnGUI()
        {
            InitGUI();

            if (WINDOW_HEIGHT != windowHeight)
            {
                PluginUtils.AdjustWindowPosition(ref rc_stgw);
                WINDOW_HEIGHT = windowHeight;
                rc_stgw.height = WINDOW_HEIGHT;
            }

            bool isScreenSizeChanged = ScWidth != Screen.width || ScHeight != Screen.height;
            if (isScreenSizeChanged)
            {
                PluginUtils.AdjustWindowPosition(ref rc_stgw);
                ScWidth = Screen.width;
                ScHeight = Screen.height;
            }

            if (prevWindowPos != rc_stgw.position)
            {
                subWindow.ResetPosition();
                prevWindowPos = rc_stgw.position;
            }

            rc_stgw = GUI.Window(WINDOW_ID, rc_stgw, DrawWindow, PluginUtils.WindowName, gsWin);

            if (config.windowPosX != (int)rc_stgw.x ||
                config.windowPosY != (int)rc_stgw.y)
            {
                config.windowPosX = (int)rc_stgw.x;
                config.windowPosY = (int)rc_stgw.y;
            }

            if (config.dirty && Input.GetMouseButtonUp(0))
            {
                MTE.SaveConfigXml();
            }
        }

        private void DrawWindow(int id)
        {
            if (studioHack == null)
            {
                return;
            }

            var isStudioHackValid = studioHack.IsValid();
            var isMaidValid = maidManager.IsValid();

            {
                var view = new GUIView(0, 0, WINDOW_WIDTH, 20);
                view.padding.y = 0;
                view.padding.x = 0;

                view.BeginLayout(GUIView.LayoutDirection.Free);

                view.currentPos.x = WINDOW_WIDTH - 20;

                if (view.DrawButton("x", 20, 20))
                {
                    MTE.instance.isEnable = false;
                }

                view.currentPos.x = WINDOW_WIDTH - 400;

                if (!isStudioHackValid)
                {
                    view.DrawLabel(studioHack.errorMessage, 400 - 20, 20, Color.yellow);
                }
                else if (!isMaidValid)
                {
                    view.DrawLabel(maidManager.errorMessage, 400 - 20, 20, Color.yellow);
                }
                else if (!timelineManager.IsValidData())
                {
                    view.DrawLabel(timelineManager.errorMessage, 400 - 20, 20, Color.yellow);
                }
                else
                {
                    // TIPSを表示したい
                    var message = "";
                    if (!studioHack.isPoseEditing)
                    {
                        var keyName = config.GetKeyName(KeyBindType.EditMode);
                        message = "[" + keyName + "]キーでポーズ編集モードに切り替えます";
                    }
                    else
                    {
                        var keyName = config.GetKeyName(KeyBindType.AddKeyFrame);
                        message = "[" + keyName + "]キーでキーフレームを登録します";
                    }

                    view.DrawLabel(message, 400 - 20, 20, Color.white);
                }
            }

            bool editEnabled = isMaidValid
                            && isStudioHackValid
                            && timeline != null
                            && maidManager.maid != null;

            bool guiEnabled = !fileMenuComboBox.focused
                            && !_maidComboBox.focused
                            && !layerComboBox.focused;

            DrawHeader(editEnabled, guiEnabled);
            DrawTimeline(editEnabled, guiEnabled);
            DrawComboBox(editEnabled, true);

            if (!isFrameDragging && !isAreaDragging)
            {
                GUI.DragWindow();
            }
        }

        private void DrawHeader(bool editEnabled, bool guiEnabled)
        {
            var view = headerView;
            view.SetEnabled(guiEnabled);
            view.ResetLayout();

            view.BeginLayout(GUIView.LayoutDirection.Horizontal);
            {
                view.margin = 0;

                fileMenuComboBox.currentIndex = -1;
                view.DrawComboBoxButton(fileMenuComboBox, 60, 20, false);

                if (view.DrawButton("ロード", 60, 20))
                {
                    if (!studioHack.IsValid())
                    {
                        PluginUtils.ShowDialog(studioHack.errorMessage);
                        return;
                    }
                    subWindow.ToggleSubWindow(SubWindowType.TimelineLoad);
                }

                if (view.DrawButton("ｷｰﾌﾚｰﾑ", 60, 20, editEnabled))
                {
                    subWindow.ToggleSubWindow(SubWindowType.KeyFrame);
                }

                if (view.DrawButton("情報", 60, 20, editEnabled))
                {
                    subWindow.ToggleSubWindow(SubWindowType.TimelineLayer);
                }

                var ikColor = editEnabled && timeline.isHoldActive ? Color.green : Color.white;
                if (view.DrawButton("IK固定", 60, 20, editEnabled, ikColor))
                {
                    subWindow.ToggleSubWindow(SubWindowType.IKHold);
                }

                var trackColor = editEnabled && timeline.activeTrack != null ? Color.green : Color.white;
                if (view.DrawButton("トラック", 60, 20, editEnabled, trackColor))
                {
                    subWindow.ToggleSubWindow(SubWindowType.Track);
                }

                if (view.DrawButton("モデル", 60, 20, editEnabled))
                {
                    subWindow.ToggleSubWindow(SubWindowType.StudioModel);
                }

                if (view.DrawButton("メディア", 60, 20, editEnabled))
                {
                    subWindow.ToggleSubWindow(SubWindowType.MoviePlayer);
                }

                if (view.DrawButton("履歴", 60, 20, editEnabled))
                {
                    subWindow.ToggleSubWindow(SubWindowType.History);
                }

                if (view.DrawButton("設定", 60, 20, editEnabled))
                {
                    subWindow.ToggleSubWindow(SubWindowType.TimelineSetting);
                }

                view.margin = GUIView.defaultMargin;
            }
            view.EndLayout();

            if (!editEnabled)
            {
                return;
            }

            view.BeginLayout(GUIView.LayoutDirection.Horizontal);

            anmName = view.DrawTextField("アニメ名", anmName, 300, 20);

            view.AddSpace(10);

            {
                view.DrawLabel("最終フレーム番号", 100, 20);

                var newMaxFrameNo = timeline.maxFrameNo;

                if (view.DrawButton("<<", 25, 20))
                {
                    newMaxFrameNo -= 10;
                }

                if (view.DrawButton("<", 20, 20))
                {
                    newMaxFrameNo -= 1;
                }

                newMaxFrameNo = view.DrawIntField(newMaxFrameNo, 50, 20);

                if (view.DrawButton(">", 20, 20))
                {
                    newMaxFrameNo += 1;
                }

                if (view.DrawButton(">>", 25, 20))
                {
                    newMaxFrameNo += 10;
                }

                if (newMaxFrameNo != timeline.maxFrameNo)
                {
                    timelineManager.SetMaxFrameNo(newMaxFrameNo);
                }
            }

            view.EndLayout();

            view.BeginLayout(GUIView.LayoutDirection.Horizontal);

            {
                view.DrawLabel("フレーム操作", 100, 20);

                var newFrameNo = timelineManager.currentFrameNo;

                if (view.DrawButton("|<", 25, 20))
                {
                    newFrameNo = 0;
                }
                if (view.DrawButton(".<", 25, 20))
                {
                    var prevFrame = timelineManager.GetPrevFrame(newFrameNo);
                    if (prevFrame != null)
                    {
                        newFrameNo = prevFrame.frameNo;
                    }
                }
                if (view.DrawButton("<", 20, 20))
                {
                    newFrameNo--;
                }

                newFrameNo = view.DrawIntField(newFrameNo, 50, 20);

                if (view.DrawButton(">", 20, 20))
                {
                    newFrameNo++;
                }
                if (view.DrawButton(">.", 25, 20))
                {
                    var nextFrame = timelineManager.GetNextFrame(newFrameNo);
                    if (nextFrame != null)
                    {
                        newFrameNo = nextFrame.frameNo;
                    }
                }
                if (view.DrawButton(">|", 25, 20))
                {
                    newFrameNo = timeline.maxFrameNo;
                }

                if (newFrameNo != timelineManager.currentFrameNo)
                {
                    timelineManager.SeekCurrentFrame(newFrameNo);
                    FixScrollPosition();
                }
            }

            view.AddSpace(10);

            if (currentLayer.isAnmPlaying)
            {
                if (view.DrawButton("■", 20, 20))
                {
                    timelineManager.Stop();
                }
            }
            else
            {
                if (view.DrawButton("▶", 20, 20))
                {
                    timelineManager.Play();
                }
            }

            view.AddSpace(10);

            {
                view.DrawLabel("再生速度", 60, 20);

                var anmSpeed = timelineManager.anmSpeed;

                anmSpeed = view.DrawFloatField(anmSpeed, 50, 20);

                if (view.DrawButton("R", 20, 20))
                {
                    anmSpeed = 1.0f;
                }

                anmSpeed = view.DrawSlider(anmSpeed, 0f, 2.0f, 90, 20);

                if (!Mathf.Approximately(anmSpeed, timelineManager.anmSpeed))
                {
                    timelineManager.SetAnmSpeedAll(anmSpeed);
                }
            }

            view.EndLayout();

            view.BeginLayout(GUIView.LayoutDirection.Horizontal);
            {
                view.DrawLabel("キーフレーム", 100, 20);

                if (view.DrawButton("登録", 50, 20, studioHack.isPoseEditing))
                {
                    currentLayer.AddKeyFrameDiff();
                }

                if (view.DrawButton("全登録", 60, 20))
                {
                    currentLayer.AddKeyFrameAll();
                }

                if (view.DrawButton("削除", 50, 20, timelineManager.HasSelected()))
                {
                    timelineManager.RemoveSelectedFrame();
                }

                if (view.DrawButton("コピー", 60, 20, timelineManager.HasSelected()))
                {
                    timelineManager.CopyFramesToClipboard();
                }

                if (view.DrawButton("ペースト", 60, 20))
                {
                    timelineManager.PasteFramesFromClipboard(false);
                }

                if (view.DrawButton("反転P", 60, 20))
                {
                    timelineManager.PasteFramesFromClipboard(true);
                }

                if (view.DrawButton("ポーズC", 60, 20))
                {
                    timelineManager.CopyPoseToClipboard();
                }

                if (view.DrawButton("ポーズP", 60, 20, studioHack.isPoseEditing))
                {
                    timelineManager.PastePoseFromClipboard();
                }
            }
            view.EndLayout();

            view.BeginLayout(GUIView.LayoutDirection.Horizontal);
            {
                view.DrawLabel("選択操作", 100, 20);

                selectStartFrameNo = view.DrawIntField(selectStartFrameNo, 50, 20);

                view.DrawLabel("～", 15, 20);

                selectEndFrameNo = view.DrawIntField(selectEndFrameNo, 50, 20);

                if (view.DrawButton("範囲選択", 70, 20))
                {
                    timelineManager.SelectFramesRange(selectStartFrameNo, selectEndFrameNo);
                }

                if (view.DrawButton("縦選択", 70, 20, !config.isEasyEdit))
                {
                    timelineManager.SelectVerticalBones();
                }
            }
            view.EndLayout();

            view.BeginLayout(GUIView.LayoutDirection.Horizontal);
            {
                view.DrawLabel("レイヤー", 100, 20);

                var layerInfo = timelineManager.GetLayerInfo(currentLayer.className);
                layerComboBox.currentIndex = layerInfo.index;
                layerComboBox.items = timelineManager.GetLayerInfoList();

                view.DrawComboBoxButton(layerComboBox, 140, 20, true);

                view.AddSpace(10);

                if (currentLayer.hasSlotNo)
                {
                    view.DrawLabel("操作対象", 50, 20);

                    _maidComboBox.currentIndex = currentLayer.slotNo;
                    _maidComboBox.items = studioHack.allMaids;

                    view.DrawComboBoxButton(_maidComboBox, 190, 20, true);
                }
            }
            view.EndLayout();

            view.BeginLayout(GUIView.LayoutDirection.Horizontal);
            {
                var newAllowPrecisionEdit = view.DrawToggle("簡易編集", config.isEasyEdit, 80, 20);
                if (newAllowPrecisionEdit != config.isEasyEdit)
                {
                    config.isEasyEdit = newAllowPrecisionEdit;
                    config.dirty = true;
                    timelineManager.Refresh();
                }

                var isPoseEditing = view.DrawToggle("ポーズ編集", studioHack.isPoseEditing, 80, 20);
                if (isPoseEditing != studioHack.isPoseEditing)
                {
                    studioHack.isPoseEditing = isPoseEditing;
                }

                var isMaidVidible = maidManager.maid.Visible;
                var newIsMaidVidible = view.DrawToggle("メイド表示", isMaidVidible, 80, 20);
                if (newIsMaidVidible != isMaidVidible)
                {
                    maidManager.maid.Visible = newIsMaidVidible;
                }

                var newIsBgVidible = view.DrawToggle("背景表示", timeline.isBackgroundVisible, 80, 20);
                if (newIsBgVidible != timeline.isBackgroundVisible)
                {
                    timeline.isBackgroundVisible = newIsBgVidible;
                }

                if (timelineManager.HasCameraLayer)
                {
                    var isCameraSync = view.DrawToggle("カメラ同期", config.isCameraSync, 80, 20, !currentLayer.isCameraLayer);
                    if (isCameraSync != config.isCameraSync)
                    {
                        config.isCameraSync = isCameraSync;
                        config.dirty = true;
                    }
                }

                if (studioHack.hasIkBoxVisible)
                {
                    var newIsIkBoxVisibleRoot = view.DrawToggle("中心点IK表示", studioHack.isIkBoxVisibleRoot, 100, 20);
                    if (newIsIkBoxVisibleRoot != studioHack.isIkBoxVisibleRoot)
                    {
                        studioHack.isIkBoxVisibleRoot = newIsIkBoxVisibleRoot;
                    }

                    var newIsIkBoxVisibleBody = view.DrawToggle("関節IK表示", studioHack.isIkBoxVisibleBody, 100, 20);
                    if (newIsIkBoxVisibleBody != studioHack.isIkBoxVisibleBody)
                    {
                        studioHack.isIkBoxVisibleBody = newIsIkBoxVisibleBody;
                    }
                }
            }
            view.EndLayout();
        }

        private void DrawTimeline(bool editEnabled, bool guiEnabled)
        {
            if (!editEnabled)
            {
                return;
            }

            var view = new GUIView(0, HEADER_HEIGHT, WINDOW_WIDTH, WINDOW_HEIGHT - HEADER_HEIGHT);
            view.SetEnabled(guiEnabled);

            view.BeginLayout(GUIView.LayoutDirection.Free);
            view.padding = Vector2.zero;

            view.currentPos.x = 100;
            view.currentPos.y = 20;

            var frameWidth = config.frameWidth;
            var frameHeight = config.frameHeight;
            var halfFrameWidth = frameWidth * 0.5f;

            var menuItems = boneMenuManager.GetVisibleItems();

            var contentWidth = timeline.maxFrameCount * frameWidth;
            var contentHeight = menuItems.Count * frameHeight;
            var viewWidth = WINDOW_WIDTH - 100;
            var viewHeight = timelineViewHeight;
            var contentRect = new Rect(0, 0, contentWidth, contentHeight);
            bool alwaysShowHorizontal = true;
            bool alwaysShowVertical = !config.isEasyEdit;
            var newScrollPosition = view.BeginScrollView(
                viewWidth,
                viewHeight,
                contentRect,
                scrollPosition,
                alwaysShowHorizontal,
                alwaysShowVertical);
            if (!isFrameDragging)
            {
                scrollPosition = newScrollPosition;
            }

            // 背景表示
            view.currentPos = Vector2.zero;
            var bgColor = Color.white;
            bgColor.a = config.timelineBgAlpha;

            for (var i = 0; i < menuItems.Count; i += 2)
            {
                view.currentPos.y = i * frameHeight;
                if (view.currentPos.y < scrollPosition.y - frameHeight * 2)
                {
                    continue;
                }

                for (var j = 0; j < timeline.maxFrameCount; j += config.frameNoInterval)
                {
                    view.currentPos.x = j * frameWidth;
                    if (view.currentPos.x < scrollPosition.x - frameWidth * config.frameNoInterval)
                    {
                        continue;
                    }

                    view.DrawTexture(texTimelineBG, bgColor);
                    break;
                }

                break;
            }

            // 選択中のメニュー背景表示
            view.currentPos.x = scrollPosition.x;

            for (var i = 0; i < menuItems.Count; i++)
            {
                view.currentPos.y = i * frameHeight;
                if (view.currentPos.y < scrollPosition.y ||
                    view.currentPos.y > scrollPosition.y + viewHeight)
                {
                    continue;
                }

                var menuItem = menuItems[i];
                if (menuItem.isSelectedMenu)
                {
                    view.DrawTexture(
                        texWhite,
                        viewWidth,
                        frameHeight,
                        config.timelineMenuSelectBgColor);
                }
            }

            // トラック範囲表示
            var activeTrack = timeline.activeTrack;
            if (activeTrack != null)
            {
                view.currentPos.x = activeTrack.startFrameNo * frameWidth + halfFrameWidth;
                view.currentPos.y = 0;
                view.DrawTexture(texWhite, 2, -1, Color.red);

                view.currentPos.x = activeTrack.endFrameNo * frameWidth + halfFrameWidth;
                view.currentPos.y = 0;
                view.DrawTexture(texWhite, 2, -1, Color.red);
            }

            // 現在のフレーム表示
            view.currentPos.x = timelineManager.currentFrameNo * frameWidth + halfFrameWidth;
            view.currentPos.y = 0;
            view.DrawTexture(texWhite, 2, -1, Color.green);

            // キーフレーム表示
            var frames = currentLayer.keyFrames;
            var adjustY = (frameHeight - frameWidth) / 2;
            foreach (var frame in frames)
            {
                var frameNo = frame.frameNo;

                view.currentPos.x = frameNo * frameWidth;
                if (view.currentPos.x < scrollPosition.x ||
                    view.currentPos.x > scrollPosition.x + viewWidth)
                {
                    continue;
                }

                for (var i = 0; i < menuItems.Count; i++)
                {
                    var menuItem = menuItems[i];

                    view.currentPos.y = i * frameHeight + adjustY;
                    if (view.currentPos.y < scrollPosition.y ||
                        view.currentPos.y > scrollPosition.y + viewHeight - 20)
                    {
                        continue;
                    }

                    if (!menuItem.HasBone(frame))
                    {
                        continue;
                    }

                    bool isSelected = menuItem.IsSelectedFrame(frame);

                    if (isAreaDragging)
                    {
                        var keyFrameRect = new Rect(
                            view.currentPos.x,
                            view.currentPos.y,
                            frameWidth,
                            frameWidth);
                        if (areaDragRect.Overlaps(keyFrameRect))
                        {
                            if (!isSelected)
                            {
                                menuItem.SelectFrame(frame, true);
                            }
                        }
                        else
                        {
                            if (isSelected && !isMultiSelect)
                            {
                                menuItem.SelectFrame(frame, true);
                            }
                        }
                    }

                    var keyFrameColor = isSelected ? Color.red : Color.white;

                    if (!menuItem.HasFullBone(frame))
                    {
                        keyFrameColor *= Color.gray;
                    }

                    view.DrawTexture(
                        texKeyFrame,
                        frameWidth,
                        frameWidth,
                        keyFrameColor,
                        EventType.MouseDown,
                        _ =>
                        {
                            if (isAreaDragging) return;
                            isFrameDragging = true;
                            frameDragDelta = Vector2.zero;
                            menuItem.SelectFrame(frame, isMultiSelect);
                        });
                }
            }

            // キーフレームドラッグ処理
            if (isFrameDragging && Event.current.type == EventType.MouseDrag)
            {
                frameDragDelta += Event.current.delta;
                if (Mathf.Abs(frameDragDelta.x) > frameWidth)
                {
                    var deltaFrameNo = (int)(frameDragDelta.x / frameWidth);
                    timelineManager.MoveSelectedBones(deltaFrameNo);
                    frameDragDelta.x -= deltaFrameNo * frameWidth;
                }
                //Extensions.LogInfo(String.Format("Mouse Drag: {0}, {1}", Event.current.delta.x, Event.current.delta.y));
            }

            if (isFrameDragging && !Input.GetMouseButton(0))
            {
                isFrameDragging = false;
            }

            // エリア選択
            view.currentPos = scrollPosition;

            if (!isFrameDragging && !isAreaDragging)
            {
                view.InvokeActionOnEvent(
                    viewWidth - 20,
                    viewHeight - 20,
                    EventType.MouseDown,
                    (pos) =>
                    {
                        isAreaDragging = true;
                        areaDragPos = scrollPosition + pos;
                        areaDragRect = new Rect(
                            areaDragPos.x,
                            areaDragPos.y,
                            0,
                            0);
                        if (!isMultiSelect)
                        {
                            timelineManager.UnselectAll();
                        }
                    });
            }

            if (isAreaDragging)
            {
                view.InvokeActionOnEvent(
                    viewWidth - 20,
                    viewHeight - 20,
                    EventType.MouseDrag,
                    (pos) =>
                    {
                        areaDragRect.position = areaDragPos;
                        areaDragRect.size = scrollPosition + pos - areaDragPos;

                        // エリア選択の座標を正規化
                        if (areaDragRect.width < 0)
                        {
                            areaDragRect.x += areaDragRect.width;
                            areaDragRect.width = -areaDragRect.width;
                        }
                        if (areaDragRect.height < 0)
                        {
                            areaDragRect.y += areaDragRect.height;
                            areaDragRect.height = -areaDragRect.height;
                        }
                    });

                view.currentPos = areaDragRect.position;
                view.DrawRect(
                    areaDragRect.width,
                    areaDragRect.height,
                    new Color(1, 1, 1, 0.5f),
                    2);

                if (!Input.GetMouseButton(0))
                {
                    isAreaDragging = false;
                }
            }

            view.EndScrollView();

            // 時間背景の表示
            view.currentPos.x = 100;
            view.currentPos.y = 0;
            view.DrawTexture(texWhite, -1, 20, timelineLabelBgColor);

            // フレーム移動
            view.InvokeActionOnEvent(
                -1,
                20,
                EventType.MouseDown,
                (pos) =>
                {
                    var frameNo = (int)((scrollPosition.x + pos.x) / frameWidth);
                    timelineManager.SeekCurrentFrame(frameNo);
                });

            // フレーム番号表示
            var frameLabelWidth = 50;
            var halfFrameLabelWidth = frameLabelWidth / 2;
            var adjustX = -halfFrameLabelWidth + halfFrameWidth;
            for (int frameNo = 0; frameNo < timeline.maxFrameCount; frameNo++)
            {
                view.currentPos.x = 100 + frameNo * frameWidth - scrollPosition.x + adjustX;
                if (view.currentPos.x < 100 - halfFrameLabelWidth ||
                    view.currentPos.x > WINDOW_WIDTH - halfFrameLabelWidth)
                {
                    continue;
                }

                if (frameNo == timelineManager.currentFrameNo)
                {
                    view.DrawLabel(frameNo.ToString(), frameLabelWidth, 20, Color.green, gsFrameLabel);
                }
                else if (frameNo % config.frameNoInterval == 0)
                {
                    view.DrawLabel(frameNo.ToString(), frameLabelWidth, 20, Color.white, gsFrameLabel);
                }
            }

            // ボーンメニューの表示
            view.currentPos.x = 0;
            view.currentPos.y = 20;
            view.DrawTexture(texWhite, 100, -1, timelineLabelBgColor);

            menuScrollPosition.y = scrollPosition.y;
            contentWidth = 100;
            contentHeight = menuItems.Count * frameHeight;
            viewWidth = 100;
            viewHeight = timelineViewHeight - 20;
            contentRect = new Rect(0, 0, contentWidth, contentHeight);
            var newMenuScrollPosition = view.BeginScrollView(
                viewWidth,
                viewHeight,
                contentRect,
                menuScrollPosition,
                "invisible",
                "invisible");
            if (!isFrameDragging)
            {
                menuScrollPosition = newMenuScrollPosition;
                scrollPosition.y = menuScrollPosition.y;
            }

            for (int i = 0; i < menuItems.Count; i++)
            {
                var menuItem = menuItems[i];

                view.currentPos.y = i * frameHeight;
                if (view.currentPos.y < scrollPosition.y ||
                    view.currentPos.y > scrollPosition.y + viewHeight)
                {
                    continue;
                }

                var diplayName = menuItem.displayName;
                var isSelected = menuItem.isSelectedMenu;

                view.currentPos.x = 0;

                if (menuItem.isSetMenu)
                {
                    view.DrawLabel(
                        menuItem.isOpenMenu ? "ー" : "＋",
                        20,
                        20,
                        isSelected ? config.timelineMenuSelectTextColor : Color.white,
                        null,
                        () =>
                        {
                            menuItem.isOpenMenu = !menuItem.isOpenMenu;
                        }
                    );
                }

                view.currentPos.x = 20;

                view.DrawLabel(
                    diplayName,
                    80,
                    20,
                    isSelected ? config.timelineMenuSelectTextColor : Color.white,
                    null,
                    () =>
                    {
                        menuItem.SelectMenu(isMultiSelect);
                    }
                );
            }

            view.EndScrollView();
            view.EndLayout();
        }

        private void DrawComboBox(bool editEnabled, bool guiEnabled)
        {
            if (!editEnabled)
            {
                layerComboBox.focused = false;
                _maidComboBox.focused = false;
            }

            headerView.SetEnabled(guiEnabled);

            headerView.DrawComboBoxContent(
                fileMenuComboBox,
                100, 120,
                rc_stgw.width, rc_stgw.height,
                20);

            headerView.DrawComboBoxContent(
                layerComboBox,
                120, 120,
                rc_stgw.width, rc_stgw.height,
                20);

            headerView.DrawComboBoxContent(
                _maidComboBox,
                170, 120,
                rc_stgw.width, rc_stgw.height,
                20);
        }
    }
}