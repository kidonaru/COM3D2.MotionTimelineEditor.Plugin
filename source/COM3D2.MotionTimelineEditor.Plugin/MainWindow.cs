using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    using MTE = MotionTimelineEditor;

    public enum FileMenuType
    {
        New,
        OutputAnm,
        OutputDCM,
        OutputImage,
    }

    public class MainWindow : IWindow
    {
        public readonly static int WINDOW_ID = 615814;
        public readonly static int MIN_WINDOW_WIDTH = 640;
        public readonly static int MIN_WINDOW_HEIGHT = 480;
        public readonly static int MIN_MENU_WIDTH = 100;
        public readonly static int MAX_MENU_WIDTH = 300;
        public readonly static int HEADER_HEIGHT = 200;
        public readonly static int EASY_EDIT_WINDOW_HEIGHT = 260;


        private static StudioHackManager studioHackManager => StudioHackManager.instance;
        private static StudioHackBase studioHack => StudioHackManager.instance.studioHack;
        private static MaidManager maidManager => MaidManager.instance;
        private static StudioModelManager modelManager => StudioModelManager.instance;
        private static TimelineManager timelineManager => TimelineManager.instance;
        private static TimelineData timeline => timelineManager.timeline;
        private static ITimelineLayer currentLayer => timelineManager.currentLayer;
        private static Config config => ConfigManager.instance.config;

        private static string anmName
        {
            get => timeline.anmName;
            set => timeline.anmName = value;
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

        public static WindowManager windowManager => WindowManager.instance;
        private static SubWindow subWindow => windowManager.subWindows[0];
        private static BoneMenuManager boneMenuManager => BoneMenuManager.Instance;
        private static int timelineViewHeight => GetWindowHeight() - HEADER_HEIGHT - 20;

        public int windowIndex { get; set; }
        public bool isShowWnd { get; set; }
        public bool requestUpdateTexture { get; set; }

        private Rect _windowRect;
        public Rect windowRect
        {
            get => _windowRect;
            set => _windowRect = value;
        }

        private int _windowWidth = 640;
        private int _windowHeight = 240;
        private bool initializedGUI = false;
        private GUIView.DragInfo frameDragInfo = new GUIView.DragInfo();
        private BoneData frameDragBoneData = null;
        private GUIView.DragInfo areaDragInfo = new GUIView.DragInfo();
        private Rect areaDragRect = new Rect();
        private int selectStartFrameNo = 0;
        private int selectEndFrameNo = 0;
        public bool isMultiSelect = false;

        public class FileMenuItem
        {
            public string name;
            public System.Action action;
        }

        private GUIView headerView = null;
        private GUIView contentView = null;
        private GUIView timelineView = null;
        private GUIView boneMenuView = null;

        private GUIComboBox<FileMenuType> fileMenuComboBox = new GUIComboBox<FileMenuType>
        {
            defaultName = "ファイル",
            items = new List<FileMenuType>
            {
                FileMenuType.New,
                FileMenuType.OutputAnm,
                FileMenuType.OutputDCM,
                FileMenuType.OutputImage,
            },
            getName = (type, index) =>
            {
                switch (type)
                {
                    case FileMenuType.New:
                        return "新規作成";
                    case FileMenuType.OutputAnm:
                        return "アニメ出力";
                    case FileMenuType.OutputDCM:
                        return "DCM出力";
                    case FileMenuType.OutputImage:
                        return "連番画像出力";
                    default:
                        return "";
                }
            },
            getEnabled = (type, index) =>
            {
                switch (type)
                {
                    case FileMenuType.OutputAnm:
                    case FileMenuType.OutputDCM:
                    case FileMenuType.OutputImage:
                        return timelineManager.IsValidData();
                    default:
                        return true;
                }
            },
            onSelected = (type, index) =>
            {
                switch (type)
                {
                    case FileMenuType.New:
                        timelineManager.CreateNewTimeline();
                        break;
                    case FileMenuType.OutputAnm:
                        timelineManager.OutputAnm();
                        break;
                    case FileMenuType.OutputDCM:
                        timelineManager.OutputDCM();
                        break;
                    case FileMenuType.OutputImage:
                        timelineManager.OutputImage();
                        break;
                }
            },
            showArrow = false,
            buttonSize = new Vector2(60, 20),
        };

        private GUIComboBox<TimelineLayerInfo> _layerComboBox = new GUIComboBox<TimelineLayerInfo>
        {
            getName = (layerInfo, index) => layerInfo.displayName,
            onSelected = (layerInfo, index) =>
            {
                timelineManager.ChangeActiveLayer(layerInfo.layerType, maidManager.maidSlotNo);
                subWindow.SetSubWindowType(SubWindowType.TimelineLayer);
            },
            contentSize = new Vector2(150, 300),
        };

        private GUIComboBox<TimelineLayerInfo> _addLayerComboBox = new GUIComboBox<TimelineLayerInfo>
        {
            getName = (layerInfo, index) => layerInfo.displayName,
            onSelected = (layerInfo, index) =>
            {
                timelineManager.ChangeActiveLayer(layerInfo.layerType, maidManager.maidSlotNo);
                subWindow.SetSubWindowType(SubWindowType.TimelineLayer);
            },
            defaultName = "+",
            buttonSize = new Vector2(20, 20),
            contentSize = new Vector2(150, 300),
            showArrow = false,
        };

        private GUIComboBox<MaidCache> _maidComboBox = new GUIComboBox<MaidCache>
        {
            getName = (maidCache, _) => maidCache == null ? "未選択" : maidCache.fullName,
            onSelected = (maidCache, index) =>
            {
                maidManager.ChangeMaid(maidCache.maid);
            },
            buttonSize = new Vector2(150, 20),
            contentSize = new Vector2(150, 300),
        };

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

        private Texture2D texWhite => GUIView.texWhite;
        private Texture2D texTimelineBG = null;
        private Texture2D texKeyFrame = null;

        private GUIView.DragInfo _windowSizeDraggableInfo = new GUIView.DragInfo();
        private GUIView.DragInfo _menuWidthDraggableInfo = new GUIView.DragInfo();

        public MainWindow()
        {
            this.windowIndex = 0;
            this.isShowWnd = true;
            this._windowRect = new Rect(
                Screen.width - _windowWidth - 30,
                Screen.height - _windowHeight - 100,
                _windowWidth,
                _windowHeight
            );
            this.headerView = new GUIView(0, 0, _windowWidth, 20);
            this.contentView = new GUIView(0, 0, _windowWidth, _windowHeight);
            this.timelineView = new GUIView(0, HEADER_HEIGHT, _windowWidth, _windowHeight - HEADER_HEIGHT);
            this.boneMenuView = new GUIView(0, HEADER_HEIGHT, config.menuWidth, _windowHeight - HEADER_HEIGHT);
        }

        public void Init()
        {
        }

        public void OnLoad()
        {
            MTEUtils.AdjustWindowPosition(ref _windowRect);
        }

        public void Update()
        {
            if (requestUpdateTexture && Input.GetMouseButtonUp(0))
            {
                requestUpdateTexture = false;
                UpdateTexture();
            }
        }

        public void UpdateTexture()
        {
            MTEUtils.LogDebug("テクスチャ作成中...");
            if (texTimelineBG != null)
            {
                UnityEngine.Object.Destroy(texTimelineBG);
                texTimelineBG = null;
            }

            var bgWidth = _windowWidth - config.menuWidth + config.frameWidth * config.frameNoInterval;
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

            if (texKeyFrame == null)
            {
                texKeyFrame = TextureUtils.CreateDiamondTexture(
                    config.frameWidth,
                    Color.white);
            }
        }

        public void FixScrollPosition()
        {
            var viewWidth = _windowWidth - config.menuWidth;
            var frameWidth = config.frameWidth;

            var minScrollX = timelineManager.currentFrameNo * frameWidth - (viewWidth - 20 - frameWidth);
            var maxScrollX = timelineManager.currentFrameNo * frameWidth;
            timelineView.scrollPosition.x = Mathf.Clamp(timelineView.scrollPosition.x, minScrollX, maxScrollX);
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
                var style = new GUIStyle
                {
                    name = name,
                    fixedWidth = 0,
                    fixedHeight = 0
                };
                style.normal.background = null;
                style.hover.background = null;
                style.active.background = null;
                customStyles.Add(style);
            }

            GUI.skin.customStyles = customStyles.ToArray();

            if (config.windowPosX != -1 && config.windowPosY != -1)
            {
                _windowRect.x = config.windowPosX;
                _windowRect.y = config.windowPosY;
            }

            MTEUtils.AdjustWindowPosition(ref _windowRect);
        }

        public void OnGUI()
        {
            InitGUI();

            if (_windowHeight != GetWindowHeight())
            {
                _windowHeight = GetWindowHeight();
                _windowRect.height = _windowHeight;
            }

            if (_windowWidth != GetWindowWidth())
            {
                _windowWidth = GetWindowWidth();
                _windowRect.width = _windowWidth;
            }

            windowRect = GUI.Window(WINDOW_ID, windowRect, DrawWindow, PluginInfo.WindowName, gsWin);
            MTEUtils.ResetInputOnScroll(windowRect);

            if (config.windowPosX != (int)windowRect.x ||
                config.windowPosY != (int)windowRect.y)
            {
                config.windowPosX = (int)windowRect.x;
                config.windowPosY = (int)windowRect.y;
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

            DrawHeader(isStudioHackValid, isMaidValid);

            bool editEnabled = isMaidValid
                            && isStudioHackValid
                            && timeline != null
                            && maidManager.maid != null;

            bool guiEnabled = !contentView.IsComboBoxFocused();

            DrawContent(editEnabled, guiEnabled);
            DrawTimeline(editEnabled, guiEnabled);
            DrawBoneMenu(editEnabled, guiEnabled);

            contentView.DrawComboBox();

            if (!frameDragInfo.isDragging && !areaDragInfo.isDragging)
            {
                GUI.DragWindow();
            }
        }

        private void DrawHeader(bool isStudioHackValid, bool isMaidValid)
        {
            var view = headerView;
            view.ResetLayout();

            view.padding.y = 0;
            view.padding.x = 0;

            view.BeginLayout(GUIView.LayoutDirection.Free);

            view.currentPos.x = _windowWidth - 20;

            if (view.DrawButton("x", 20, 20))
            {
                MTE.instance.isEnable = false;
            }

            view.currentPos.x = _windowWidth - 400;

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
                if (!studioHackManager.isPoseEditing)
                {
                    var keyName = config.GetKeyName(KeyBindType.EditMode);
                    message = "[" + keyName + "]キーで編集モードに切り替えます";
                }
                else
                {
                    var keyName = config.GetKeyName(KeyBindType.AddKeyFrame);
                    message = "[" + keyName + "]キーでキーフレームを登録します";
                }

                view.DrawLabel(message, 400 - 20, 20, Color.white);
            }
        }

        private void DrawContent(bool editEnabled, bool guiEnabled)
        {
            var view = contentView;
            view.Init(0, 0, _windowWidth, _windowHeight);
            view.SetEnabled(guiEnabled);

            view.currentPos.y = 20;

            view.margin = 0;
            view.padding = new Vector2(3, 3);

            view.BeginHorizontal();
            {
                view.AddSpace(5);

                fileMenuComboBox.currentIndex = -1;
                fileMenuComboBox.DrawButton(view);

                if (view.DrawButton("セーブ", 60, 20, editEnabled))
                {
                    if (!studioHack.IsValid())
                    {
                        MTEUtils.ShowDialog(studioHack.errorMessage);
                        return;
                    }
                    if (!timelineManager.IsValidData())
                    {
                        MTEUtils.ShowDialog(timelineManager.errorMessage);
                        return;
                    }
                    timelineManager.SaveTimeline();
                }
                
                if (view.DrawButton("ロード", 60, 20))
                {
                    if (!studioHack.IsValid())
                    {
                        MTEUtils.ShowDialog(studioHack.errorMessage);
                        return;
                    }
                    subWindow.SetSubWindowType(SubWindowType.TimelineLoad);
                }

                if (view.DrawButton("情報", 60, 20, editEnabled))
                {
                    subWindow.SetSubWindowType(SubWindowType.TimelineLayer);
                }

                if (view.DrawButton("ｷｰﾌﾚｰﾑ", 60, 20, editEnabled))
                {
                    subWindow.SetSubWindowType(SubWindowType.KeyFrame);
                }

                if (view.DrawButton("IK固定", 60, 20, editEnabled))
                {
                    subWindow.SetSubWindowType(SubWindowType.IKHold);
                }

                var trackColor = editEnabled && timeline.activeTrack != null ? Color.green : Color.white;
                if (view.DrawButton("トラック", 60, 20, editEnabled, trackColor))
                {
                    subWindow.SetSubWindowType(SubWindowType.Track);
                }

                if (view.DrawButton("テンプレ", 60, 20, editEnabled))
                {
                    subWindow.SetSubWindowType(SubWindowType.Template);
                }

                if (view.DrawButton("履歴", 60, 20, editEnabled))
                {
                    subWindow.SetSubWindowType(SubWindowType.History);
                }

                if (view.DrawButton("設定", 60, 20, editEnabled))
                {
                    subWindow.SetSubWindowType(SubWindowType.TimelineSetting);
                }
            }
            view.EndLayout();

            view.margin = GUIView.defaultMargin;
            view.padding = GUIView.defaultPadding;

            if (!editEnabled)
            {
                return;
            }

            view.BeginHorizontal();
            {
                view.DrawTextField("アニメ名", 0, anmName, 310, 20, newText => anmName = newText);

                view.AddSpace(10);

                view.DrawLabel("最終フレーム", 75, 20);

                var newMaxFrameNo = timeline.maxFrameNo;

                view.DrawIntSelect(
                    "",
                    1,
                    10,
                    null,
                    newMaxFrameNo,
                    value => newMaxFrameNo = value,
                    diff => newMaxFrameNo += diff
                );

                if (newMaxFrameNo != timeline.maxFrameNo)
                {
                    timelineManager.SetMaxFrameNo(newMaxFrameNo);
                }
            }
            view.EndLayout();

            view.BeginHorizontal();
            {
                view.DrawLabel("フレーム操作", 100, 20);

                var newFrameNo = timelineManager.currentFrameNo;

                view.margin = 0;

                if (view.DrawButton("|<", 25, 20))
                {
                    newFrameNo = 0;
                }
                if (view.DrawRepeatButton(".<", 25, 20))
                {
                    var prevFrame = timelineManager.GetPrevFrame(newFrameNo);
                    if (prevFrame != null)
                    {
                        newFrameNo = prevFrame.frameNo;
                    }
                }
                if (view.DrawRepeatButton("<", 25, 20))
                {
                    newFrameNo--;
                }

                view.DrawIntField(new GUIView.IntFieldOption
                {
                    value = newFrameNo,
                    width = 50,
                    height = 20,
                    onChanged = value => newFrameNo = value,
                });

                if (view.DrawRepeatButton(">", 25, 20))
                {
                    newFrameNo++;
                }
                if (view.DrawRepeatButton(">.", 25, 20))
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

                view.margin = GUIView.defaultMargin;

                if (newFrameNo != timelineManager.currentFrameNo)
                {
                    timelineManager.SeekCurrentFrame(newFrameNo);
                    FixScrollPosition();
                }

                view.AddSpace(10);

                if (currentLayer.isAnmPlaying)
                {
                    if (view.DrawButton("■", 20, 20))
                    {
                        timelineManager.Pause();
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

                view.DrawSliderValue(
                    new GUIView.SliderOption
                    {
                        label = "再生速度",
                        labelWidth = 50,
                        min = 0f,
                        max = 2f,
                        step = 0.01f,
                        defaultValue = 1f,
                        value = timelineManager.anmSpeed,
                        onChanged = value => timelineManager.anmSpeed = value,
                    });
            }
            view.EndLayout();

            view.BeginHorizontal();
            {
                view.DrawLabel("キーフレーム", 100, 20);

                if (view.DrawButton("登録", 50, 20, studioHackManager.isPoseEditing))
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

                if (view.DrawButton("ポーズP", 60, 20, studioHackManager.isPoseEditing))
                {
                    timelineManager.PastePoseFromClipboard();
                }
            }
            view.EndLayout();

            view.BeginHorizontal();
            {
                view.DrawLabel("範囲操作", 100, 20);

                view.DrawIntField(new GUIView.IntFieldOption
                {
                    value = selectStartFrameNo,
                    width = 50,
                    height = 20,
                    onChanged = value => selectStartFrameNo = value,
                });

                view.DrawLabel("～", 15, 20);

                view.DrawIntField(new GUIView.IntFieldOption
                {
                    value = selectEndFrameNo,
                    width = 50,
                    height = 20,
                    onChanged = value => selectEndFrameNo = value,
                });

                if (view.DrawButton("R", 20, 20))
                {
                    selectStartFrameNo = 0;
                    selectEndFrameNo = 0;
                }

                var isValidRange = timelineManager.IsValidFrameRnage(selectStartFrameNo, selectEndFrameNo);

                if (view.DrawButton("範囲選択", 65, 20))
                {
                    timelineManager.SelectFramesRange(selectStartFrameNo, selectEndFrameNo);
                }

                if (view.DrawButton("ﾌﾚｰﾑ挿入", 65, 20, isValidRange && selectStartFrameNo > 0))
                {
                    timelineManager.InsertFrames(selectStartFrameNo, selectEndFrameNo);
                }

                if (view.DrawButton("ﾌﾚｰﾑ削除", 65, 20, isValidRange && selectStartFrameNo > 0))
                {
                    timelineManager.DeleteFrames(selectStartFrameNo, selectEndFrameNo);
                }

                if (view.DrawButton("ﾌﾚｰﾑ複製", 65, 20, isValidRange))
                {
                    timelineManager.DuplicateFrames(selectStartFrameNo, selectEndFrameNo);
                }

                if (view.DrawButton("縦選択", 60, 20, !config.isEasyEdit))
                {
                    timelineManager.SelectVerticalBones();
                }
            }
            view.EndLayout();

            view.BeginHorizontal();
            view.margin = 0;
            {
                var layerType = currentLayer.layerType;
                var layerInfo = timelineManager.GetLayerInfo(layerType);
                _layerComboBox.currentItem = layerInfo;
                _layerComboBox.items = timelineManager.usingLayerInfoList;
                _layerComboBox.DrawButton("レイヤー", view);

                if (view.DrawButton("-", 20, 20, layerType != typeof(MotionTimelineLayer)))
                {
                    timelineManager.RemoveLayers(layerType);
                }

                _addLayerComboBox.currentIndex = -1;
                _addLayerComboBox.items = timelineManager.unusingLayerInfoList;
                _addLayerComboBox.DrawButton(null, view);

                view.AddSpace(10);

                if (currentLayer.hasSlotNo)
                {
                    view.DrawLabel("操作対象", 60, 20);

                    _maidComboBox.currentIndex = currentLayer.slotNo;
                    _maidComboBox.items = maidManager.maidCaches;
                    _maidComboBox.DrawButton(view);
                }
            }
            view.margin = GUIView.defaultMargin;
            view.EndLayout();

            view.BeginHorizontal();
            {
                DrawEasySetting(view, EasySettingType.簡易表示, config.isEasyEdit, 80, 20, newValue =>
                {
                    config.isEasyEdit = newValue;
                    config.dirty = true;
                    timelineManager.Refresh();
                });

                DrawEasySetting(view, EasySettingType.編集モード, studioHackManager.isPoseEditing, 80, 20, newValue =>
                {
                    studioHackManager.isPoseEditing = newValue;
                });

                DrawEasySetting(view, EasySettingType.メイド表示, maidManager.maid.Visible, 80, 20, newValue =>
                {
                    maidManager.maid.Visible = newValue;
                });

                DrawEasySetting(view, EasySettingType.モデル表示, modelManager.Visible, 80, 20, newValue =>
                {
                    modelManager.Visible = newValue;
                });

                DrawEasySetting(view, EasySettingType.背景表示, timeline.isBackgroundVisible, 80, 20, newValue =>
                {
                    timeline.isBackgroundVisible = newValue;
                });

                if (timelineManager.hasCameraLayer)
                {
                    var cameraUpdated = false;
                    cameraUpdated |= DrawEasySetting(view, EasySettingType.カメラ同期, config.isCameraSync, 80, 20, !currentLayer.isCameraLayer, newValue =>
                    {
                        config.isCameraSync = newValue;
                        config.dirty = true;
                    });

                    cameraUpdated |= DrawEasySetting(view, EasySettingType.視野角固定, config.isFixedFoV, 80, 20, !currentLayer.isCameraLayer && studioHackManager.isPoseEditing, newValue =>
                    {
                        config.isFixedFoV = newValue;
                        config.dirty = true;
                    });

                    cameraUpdated |= DrawEasySetting(view, EasySettingType.フォーカス固定, config.isFixedFocus, 100, 20, !currentLayer.isCameraLayer && studioHackManager.isPoseEditing, newValue =>
                    {
                        config.isFixedFocus = newValue;
                        config.dirty = true;
                    });

                    if (cameraUpdated)
                    {
                        var cameraLayer = timelineManager.GetLayer(typeof(CameraTimelineLayer));
                        if (cameraLayer != null)
                        {
                            cameraLayer.ApplyCurrentFrame(false);
                        }
                    }
                }

                if (timelineManager.hasPostEffectLayer)
                {
                    DrawEasySetting(view, EasySettingType.ポスプロ同期, config.isPostEffectSync, 100, 20, !currentLayer.isPostEffectLayer, newValue =>
                    {
                        config.isPostEffectSync = newValue;
                        config.dirty = true;
                    });
                }

                if (studioHack.hasIkBoxVisible)
                {
                    DrawEasySetting(view, EasySettingType.中心点IK表示, config.isIkBoxVisibleRoot, 100, 20, studioHackManager.isPoseEditing, newValue =>
                    {
                        config.isIkBoxVisibleRoot = newValue;
                        config.dirty = true;
                        studioHack.isIkBoxVisibleRoot = newValue;
                    });

                    DrawEasySetting(view, EasySettingType.関節IK表示, config.isIkBoxVisibleBody, 100, 20, studioHackManager.isPoseEditing, newValue =>
                    {
                        config.isIkBoxVisibleBody = newValue;
                        config.dirty = true;
                        studioHack.isIkBoxVisibleBody = newValue;
                    });
                }
            }
            view.EndLayout();
        }

        private bool DrawEasySetting(
            GUIView view,
            EasySettingType type,
            bool value,
            float width,
            float height,
            bool enabled,
            Action<bool> onChanged)
        {
            if (!config.IsEasySettingVisible(type))
            {
                return false;
            }

            return view.DrawToggle(
                type.ToName(),
                value,
                width,
                height,
                enabled,
                newValue => onChanged(newValue)
            );
        }

        private bool DrawEasySetting(
            GUIView view,
            EasySettingType type,
            bool value,
            float width,
            float height,
            Action<bool> onChanged)
        {
            return DrawEasySetting(view, type, value, width, height, true, onChanged);
        }

        private void DrawTimeline(bool editEnabled, bool guiEnabled)
        {
            if (!editEnabled)
            {
                return;
            }

            var view = timelineView;
            view.Init(0, HEADER_HEIGHT, _windowWidth, _windowHeight - HEADER_HEIGHT);
            view.SetEnabled(guiEnabled);

            view.BeginLayout(GUIView.LayoutDirection.Free);
            view.padding = Vector2.zero;

            view.currentPos.x = config.menuWidth;
            view.currentPos.y = 20;

            var frameWidth = config.frameWidth;
            var frameHeight = config.frameHeight;
            var halfFrameWidth = frameWidth * 0.5f;

            var menuItems = boneMenuManager.GetVisibleItems();

            var contentWidth = timeline.maxFrameCount * frameWidth;
            var contentHeight = menuItems.Count * frameHeight;
            var viewWidth = _windowWidth - config.menuWidth;
            var viewHeight = timelineViewHeight;
            var contentRect = new Rect(0, 0, contentWidth, contentHeight);
            bool alwaysShowHorizontal = true;
            bool alwaysShowVertical = !config.isEasyEdit;

            // 自動スクロール
            if (config.isAutoScroll &&
                currentLayer.isAnmSyncing &&
                studioHack.isAnmPlaying &&
                !(view.IsMouseOverRect(viewWidth, viewHeight) && Input.GetMouseButton(0)))
            {
                timelineView.scrollPosition.x = Mathf.Clamp(
                    timelineManager.currentFrameNo * config.frameWidth - viewWidth / 2,
                    0,
                    Mathf.Max(0, contentWidth - viewWidth));
            }

            view.BeginScrollView(
                viewWidth,
                viewHeight,
                contentRect,
                alwaysShowHorizontal,
                alwaysShowVertical);

            var scrollPosition = view.scrollPosition;

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

            // 範囲選択表示
            if (selectStartFrameNo > 0 || selectEndFrameNo > 0)
            {
                var length = selectEndFrameNo - selectStartFrameNo + 1;
                view.currentPos.x = selectStartFrameNo * frameWidth;
                view.currentPos.y = scrollPosition.y;
                view.DrawTexture(texWhite, length * frameWidth, viewHeight, config.timelineSelectRangeColor);
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

            // BPMライン表示
            if (timeline.isShowBPMLine && timeline.bpm > 0)
            {
                var frameNoPerBeat = timeline.frameRate * 60.0 / timeline.bpm;
                var offsetFrame = timeline.bpmLineOffsetFrame;
                var beatCount = timeline.maxFrameCount / frameNoPerBeat;
                for (var i = 1; i < beatCount; i++)
                {
                    var frameNo = Mathf.Round((float)(i * frameNoPerBeat) + offsetFrame);
                    view.currentPos.x = frameNo * frameWidth + halfFrameWidth;
                    if (view.currentPos.x < scrollPosition.x ||
                        view.currentPos.x > scrollPosition.x + viewWidth)
                    {
                        continue;
                    }

                    view.currentPos.y = 0;
                    view.DrawTexture(texWhite, 2, -1, config.bpmLineColor);
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

                    if (!menuItem.HasVisibleBone(frame))
                    {
                        continue;
                    }

                    bool isSelected = menuItem.IsSelectedFrame(frame);

                    var keyFrameRect = new Rect(
                            view.currentPos.x,
                            view.currentPos.y,
                            frameWidth,
                            frameWidth);

                    // エリア選択範囲内のキーフレームを選択
                    if (areaDragInfo.isDragging)
                    {
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

                    // フレームのドラッグ開始
                    if (!areaDragInfo.isDragging && !frameDragInfo.isDragging)
                    {
                        view.InvokeActionOnDragStart(
                            keyFrameRect,
                            frameDragInfo,
                            view.currentPos,
                            newPos =>
                            {
                                menuItem.SelectFrame(frame, isMultiSelect);
                                frameDragBoneData = timelineManager.selectedBones
                                    .Where(bone => bone.frameNo == frameNo)
                                    .FirstOrDefault();
                            }
                        );
                    }

                    var keyFrameColor = isSelected ? Color.red : Color.white;

                    if (!menuItem.IsFullBones(frame))
                    {
                        keyFrameColor *= Color.gray;
                    }

                    view.DrawTexture(
                        texKeyFrame,
                        frameWidth,
                        frameWidth,
                        keyFrameColor);
                }
            }

            // フレームのドラッグ中処理
            if (frameDragInfo.isDragging)
            {
                view.InvokeActionOnDragging(
                    frameDragInfo,
                    newPos =>
                    {
                        newPos.x = Mathf.Clamp(newPos.x, scrollPosition.x, scrollPosition.x + viewWidth - 20);
                        newPos.y = Mathf.Clamp(newPos.y, scrollPosition.y, scrollPosition.y + viewHeight - 20);

                        if (frameDragBoneData != null)
                        {
                            var targetFrameNo = (int)((newPos.x + halfFrameWidth) / frameWidth);
                            timelineManager.MoveSelectedBones(targetFrameNo - frameDragBoneData.frameNo);
                        }
                    });
            }

            view.currentPos = scrollPosition;

            // エリア選択のドラッグ開始
            if (!areaDragInfo.isDragging && !frameDragInfo.isDragging)
            {
                var drawRect = view.GetDrawRect(viewWidth - 20, viewHeight - 20);

                var pos = Event.current.mousePosition;
                pos.x -= drawRect.x;
                pos.y -= drawRect.y;
                pos += scrollPosition;

                view.InvokeActionOnDragStart(
                    drawRect,
                    areaDragInfo,
                    pos,
                    newPos =>
                    {
                        areaDragRect = new Rect(
                            areaDragInfo.startPos.x,
                            areaDragInfo.startPos.y,
                            0,
                            0);
                        if (!isMultiSelect)
                        {
                            timelineManager.UnselectAll();
                        }
                    }
                );
            }

            // エリア選択のドラッグ中処理
            if (areaDragInfo.isDragging)
            {
                view.InvokeActionOnDragging(
                    areaDragInfo,
                    newPos =>
                    {
                        newPos.x = Mathf.Clamp(newPos.x, scrollPosition.x, scrollPosition.x + viewWidth - 20);
                        newPos.y = Mathf.Clamp(newPos.y, scrollPosition.y, scrollPosition.y + viewHeight - 20);

                        areaDragRect.position = areaDragInfo.startPos;
                        areaDragRect.size = newPos - areaDragRect.position;

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
                    }
                );

                view.currentPos = areaDragRect.position;
                view.DrawRect(
                    areaDragRect.width,
                    areaDragRect.height,
                    new Color(1, 1, 1, 0.5f),
                    2);
            }

            view.EndScrollView();

            // 時間背景の表示
            view.currentPos.x = config.menuWidth;
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
                view.currentPos.x = config.menuWidth + frameNo * frameWidth - scrollPosition.x + adjustX;
                if (view.currentPos.x < config.menuWidth - halfFrameLabelWidth ||
                    view.currentPos.x > _windowWidth - halfFrameLabelWidth)
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

            view.currentPos.x = view.viewRect.width - 20;
            view.currentPos.y = view.viewRect.height - 20;

            view.DrawDraggableButton("□", 20, 20,
                _windowSizeDraggableInfo,
                new Vector2(_windowWidth, _windowHeight),
                null,
                value =>
            {
                config.windowWidth = (int)value.x;
                config.windowWidth = Mathf.Clamp(config.windowWidth, MIN_WINDOW_WIDTH, Screen.width);

                if (!config.isEasyEdit)
                {
                    config.windowHeight = (int)value.y;
                    config.windowHeight = Mathf.Clamp(config.windowHeight, MIN_WINDOW_HEIGHT, Screen.height);
                }

                requestUpdateTexture = true;
                config.dirty = true;
            });

            view.EndLayout();
        }

        private void DrawBoneMenu(bool editEnabled, bool guiEnabled)
        {
            if (!editEnabled)
            {
                return;
            }

            var view = boneMenuView;
            view.Init(0, HEADER_HEIGHT, config.menuWidth, _windowHeight - HEADER_HEIGHT);
            view.SetEnabled(guiEnabled);

            view.BeginLayout(GUIView.LayoutDirection.Free);
            view.padding = Vector2.zero;

            var frameHeight = config.frameHeight;

            var menuItems = boneMenuManager.GetVisibleItems();

            // ボーンメニューの表示
            view.currentPos.x = 0;
            view.currentPos.y = 20;
            view.DrawTexture(texWhite, config.menuWidth, -1, timelineLabelBgColor);

            view.scrollPosition.y = timelineView.scrollPosition.y;
            var contentWidth = config.menuWidth;
            var contentHeight = menuItems.Count * frameHeight;
            var viewWidth = config.menuWidth;
            var viewHeight = timelineViewHeight - 20;
            var contentRect = new Rect(0, 0, contentWidth, contentHeight);
            view.BeginScrollView(
                viewWidth,
                viewHeight,
                contentRect,
                "invisible",
                "invisible");

            var scrollPosition = view.scrollPosition;
            timelineView.scrollPosition.y = scrollPosition.y;

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
                    config.menuWidth - 20,
                    20,
                    isSelected ? config.timelineMenuSelectTextColor : Color.white
                );

                view.InvokeActionOnEvent(
                    config.menuWidth - 40,
                    20,
                    EventType.MouseDown,
                    (pos) =>
                    {
                        menuItem.SelectMenu(isMultiSelect);
                    });

                if (studioHackManager.isPoseEditing)
                {
                    view.InvokeActionOnMouse(
                        config.menuWidth - 20,
                        20,
                        _ =>
                        {
                            view.currentPos.x = config.menuWidth - 20;

                            var frame = currentLayer.GetFrame(timelineManager.currentFrameNo);
                            if (menuItem.IsFullBones(frame))
                            {
                                if (view.DrawButton("D", 20, 20))
                                {
                                    menuItem.RemoveKey();
                                }
                            }
                            else
                            {
                                if (view.DrawButton("A", 20, 20))
                                {
                                    menuItem.AddKey();
                                }
                            }
                        });
                }
            }
            view.EndScrollView();

            view.currentPos.x = view.viewRect.width - 20;
            view.currentPos.y = view.viewRect.height - 20;

            var buttonRect = view.GetDrawRect(20, 20);
            if (buttonRect.Contains(Event.current.mousePosition) ||
                _menuWidthDraggableInfo.isDragging)
            {
                view.DrawDraggableButton("□", 20, 20,
                    _menuWidthDraggableInfo,
                    new Vector2(config.menuWidth, 0f),
                    null,
                    value =>
                {
                    config.menuWidth = (int)value.x;
                    config.menuWidth = Mathf.Clamp(config.menuWidth, MIN_MENU_WIDTH, MAX_MENU_WIDTH);

                    requestUpdateTexture = true;
                    config.dirty = true;
                });
            }

            view.EndLayout();
        }

        public void OnScreenSizeChanged()
        {
            MTEUtils.AdjustWindowPosition(ref _windowRect);
        }

        public static int GetWindowWidth()
        {
            return config.windowWidth;
        }

        public static int GetWindowHeight()
        {
            if (config.isEasyEdit)
            {
                return EASY_EDIT_WINDOW_HEIGHT;
            }
            else
            {
                return config.windowHeight;
            }
        }
    }
}