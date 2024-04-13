using System;
using System.IO;
using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityInjector;
using UnityInjector.Attributes;
using System.Collections.Generic;
using System.Linq;
using System.Collections;
using UnityEngine.AI;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    [PluginFilter( "COM3D2x64" ), PluginName("COM3D2.MotionTimelineEditor.Plugin"), PluginVersion( "1.1.2.0" )]
    public class MotionTimelineEditor : PluginBase
    {
        public readonly static int WINDOW_ID = 615814;
        public readonly static int WINDOW_WIDTH = 640;
        public readonly static int HEADER_HEIGHT = 180;
        public static int WINDOW_HEIGHT = 240;
        public readonly static int WIDTH_DPOS = -WINDOW_WIDTH - 30;
        public readonly static int HEIGHT_DPOS = -WINDOW_HEIGHT - 100;
        public Rect rc_stgw = new Rect(
            Screen.width + WIDTH_DPOS,
            Screen.height + HEIGHT_DPOS,
            WINDOW_WIDTH,
            WINDOW_HEIGHT
        );
        private int ScWidth = 0, ScHeight = 0;
        private Vector2 prevWindowPos = Vector2.zero;

        public static Color timelineLabelBgColor
        {
            get
            {
                var color = config.timelineMenuBgColor;
                color.a = config.timelineBgAlpha;
                return color;
            }
        }

        private bool _isShowWnd = false;
        public bool isShowWnd
        {
            get
            {
                return _isShowWnd;
            }
            set
            {
                if (_isShowWnd == value)
                {
                    return;
                }

                _isShowWnd = value;
                UpdateGearMenu();

                if (!value) 
                {
                    subWindow.isShowWnd = false;
                }
            }
        }

        private bool isOnStateStudioMode = false;
        private bool isOnStateEdit = false;
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
        private bool isMultiSelect = false;

        private Texture2D texWhite = null;
        private Texture2D texTimelineBG = null;
        private Texture2D texKeyFrame = null;

        public static MotionTimelineEditor instance { get; private set; }

        private static TimelineManager timelineManager
        {
            get
            {
                return TimelineManager.instance;
            }
        }

        private static BoneMenuManager boneMenuManager
        {
            get
            {
                return BoneMenuManager.Instance;
            }
        }

        public static SubWindow subWindow = new SubWindow();
        public static Config config = new Config();
        public static MaidHackBase maidHack = null;

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

        private static String anmName
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

        private static TimelineData timeline
        {
            get
            {
                return timelineManager.timeline;
            }
        }

        public int windowHeight
        {
            get
            {
                if (config.isEasyEdit)
                {
                    return 240;
                }
                else
                {
                    return 480;
                }
            }
        }

        public int timelineViewHeight
        {
            get
            {
                return windowHeight - HEADER_HEIGHT - 20;
            }
        }

        public void Awake()
        {
            try
            {
                GameObject.DontDestroyOnLoad(this);
                SceneManager.sceneLoaded += OnChangedSceneLevel;
                timelineManager.onRefresh += OnRefresh;
                instance = this;
                Initialize();
            }
            catch (Exception e)
            {
                Extensions.LogException(e);
            }
        }

        public bool isPluginActive
        {
            get
            {
                if (!isOnStateStudioMode && !isOnStateEdit)
                {
                    return false;
                }

                if (GameMain.Instance.CharacterMgr.IsBusy())
                {
                    return false;
                }

                return true;
            }
        }

        public void Update()
        {
            try
            {
                if (!isPluginActive)
                {
                    return;
                }

                if ((Input.GetKey(config.keyPluginToggleSub1) || Input.GetKey(config.keyPluginToggleSub2)) &&
                    Input.GetKeyDown(config.keyPluginToggle))
                {
                    isShowWnd = !isShowWnd;
                }

                if (!maidHack.IsValid())
                {
                    return;
                }

                if (isShowWnd)
                {
                    if (Input.GetKeyDown(config.keyAddKeyFrame))
                    {
                        timelineManager.AddKeyFrameDiff();
                    }
                    if (Input.GetKeyDown(config.keyRemoveKeyFrame))
                    {
                        timelineManager.RemoveSelectedFrame();
                    }
                    if (Input.GetKeyDown(config.keyPrevFrame))
                    {
                        timelineManager.SetCurrentFrame(timelineManager.currentFrameNo - 1);
                    }
                    if (Input.GetKeyDown(config.keyNextFrame))
                    {
                        timelineManager.SetCurrentFrame(timelineManager.currentFrameNo + 1);
                    }
                    if (Input.GetKeyDown(config.keyPlay))
                    {
                        if (timelineManager.isAnmPlaying)
                        {
                            timelineManager.Stop();
                        }
                        else
                        {
                            timelineManager.Play();
                        }
                    }
                    if (Input.GetKeyDown(config.keyEditMode))
                    {
                        maidHack.isPoseEditing = !maidHack.isPoseEditing;
                    }

                    isMultiSelect = false;
                    if (Input.GetKey(config.keyMultiSelect1) || Input.GetKey(config.keyMultiSelect2))
                    {
                        isMultiSelect = true;
                    }

                    maidHack.Update();
                    timelineManager.Update();
                    subWindow.Update();

                    // 自動スクロール
                    if (config.isAutoScroll &&
                        timelineManager.isAnmSyncing &&
                        maidHack.isMotionPlaying &&
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
            }
            catch (Exception e)
            {
                Extensions.LogException(e);
            }
        }

        public void LateUpdate()
        {
            if (!isPluginActive)
            {
                return;
            }
            if (!maidHack.IsValid())
            {
                return;
            }

            if (isShowWnd)
            {
                subWindow.LateUpdate();
            }
        }

        public void OnChangedSceneLevel(Scene sceneName, LoadSceneMode SceneMode)
        {
            if (!config.pluginEnabled)
            {
                return;
            }

            if (sceneName.name == "SceneTitle")
            {
                this.isShowWnd = false;
            }

            var isOldStateStudioMode = this.isOnStateStudioMode;
            var isOldStateEdit = this.isOnStateEdit;
            this.isOnStateStudioMode = sceneName.name == "ScenePhotoMode";
            this.isOnStateEdit = sceneName.name == "SceneEdit" || sceneName.name == "SceneDaily";

            if (this.isOnStateStudioMode)
            {
                maidHack = new StudioHack();
                maidHack.Init();
                boneMenuManager.Init();
                AddGearMenu();
                return;
            }
            if (this.isOnStateEdit)
            {
                maidHack = new MultipleMaidsHack();
                maidHack.Init();
                boneMenuManager.Init();
                AddGearMenu();
                return;
            }

            if (isOldStateStudioMode && !this.isOnStateStudioMode)
            {
                this.isShowWnd = false;
                RemoveGearMenu();
                return;
            }
            if (isOldStateEdit && !this.isOnStateEdit)
            {
                this.isShowWnd = false;
                RemoveGearMenu();
                return;
            }
        }

        void OnApplicationQuit()
        {
            SaveConfigXml();
        }

        public void OnRefresh()
        {
            UpdateTexture();
        }

        public static void LoadConfigXml()
        {
            try
            {
                var path = Extensions.ConfigPath;
                if (!File.Exists(path))
                {
                    return;
                }

                var serializer = new XmlSerializer(typeof(Config));
                using (var stream = new FileStream(path, FileMode.Open))
                {
                    config = (Config)serializer.Deserialize(stream);
                    config.ConvertVersion();
                }
            }
            catch (Exception e)
            {
                Extensions.LogException(e);
            }
        }

        public static void SaveConfigXml()
        {
            config.dirty = false;

            Extensions.Log("設定保存中...");
            try
            {
                var path = Extensions.ConfigPath;
                var serializer = new XmlSerializer(typeof(Config));
                using (var stream = new FileStream(path, FileMode.Create))
                {
                    serializer.Serialize(stream, config);
                }
            }
            catch (Exception e)
            {
                Extensions.LogException(e);
            }
        }

        public static void ResetConfig()
        {
            config = new Config();
            SaveConfigXml();
        }

        public void UpdateTexture()
        {
            //Extensions.Log("テクスチャ作成中...");
            if (texTimelineBG != null)
            {
                UnityEngine.Object.Destroy(texTimelineBG);
                texTimelineBG = null;
            }
            texTimelineBG = timelineManager.timeline.CreateBGTexture(
                config.frameWidth,
                config.frameHeight,
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
            texKeyFrame = TimelineData.CreateKeyFrameTexture(
                config.frameWidth,
                Color.white);
        }

        private void Initialize()
        {
            Extensions.Log("初期化中...");

            texWhite = GUIView.CreateColorTexture(Color.white);

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

            LoadConfigXml();
            SaveConfigXml();

            subWindow.Init();
        }

        private void InitGUI()
        {
            if (initializedGUI)
            {
                return;
            }
            initializedGUI = true;

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

        GameObject gearMenuIcon = null;

        private void AddGearMenu()
        {
            gearMenuIcon = GUIExtBase.GUIExt.Add(
                Extensions.PluginName,
                Extensions.PluginName,
                Extensions.icon,
                (go) => {
                    if (!isPluginActive)
                    {
                        return;
                    }

                    isShowWnd = !isShowWnd;
                });
        }

        private void RemoveGearMenu()
        {
            if (gearMenuIcon != null)
            {
                GUIExtBase.GUIExt.Destroy(gearMenuIcon);
                gearMenuIcon = null;
            }
        }

        private void UpdateGearMenu()
        {
            if (gearMenuIcon != null)
            {
                GUIExtBase.GUIExt.SetFrameColor(gearMenuIcon, isShowWnd ? Color.blue : Color.white);
            }
        }

        public void SaveScreenShot(string filePath, int width, int height)
        {
            StartCoroutine(SaveScreenShotInternal(filePath, width, height));
        }

        protected IEnumerator SaveScreenShotInternal(string filePath, int width, int height)
        {
            Extensions.UIHide();
            var subWindowType = subWindow.subWindowType;
            isShowWnd = false;
            yield return new WaitForEndOfFrame();
            var texture = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
            texture.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
            yield return new WaitForEndOfFrame();
            isShowWnd = true;
            subWindow.subWindowType = subWindowType;
            Extensions.UIResume();

            texture.ResizeTexture(width, height);
            UTY.SaveImage(texture, filePath);

            UnityEngine.Object.Destroy(texture);

            yield break;
        }

        public void OnGUI()
        {
            if (isShowWnd)
            {
                InitGUI();

                if (WINDOW_HEIGHT != windowHeight)
                {
                    Extensions.AdjustWindowPosition(ref rc_stgw);
                    WINDOW_HEIGHT = windowHeight;
                    rc_stgw.height = WINDOW_HEIGHT;
                }

                bool isScreenSizeChanged = ScWidth != Screen.width || ScHeight != Screen.height;
                if (isScreenSizeChanged)
                {
                    Extensions.AdjustWindowPosition(ref rc_stgw);
                    ScWidth = Screen.width;
                    ScHeight = Screen.height;
                }

                if (prevWindowPos != rc_stgw.position)
                {
                    subWindow.ResetPosition();
                    prevWindowPos = rc_stgw.position;
                }

                rc_stgw = GUI.Window(WINDOW_ID, rc_stgw, DrawWindow, Extensions.WindowName, gsWin);

                subWindow.OnGUI();

                if (config.windowPosX != (int)rc_stgw.x ||
                    config.windowPosY != (int)rc_stgw.y)
                {
                    config.windowPosX = (int)rc_stgw.x;
                    config.windowPosY = (int)rc_stgw.y;
                }

                if (config.dirty && Input.GetMouseButtonUp(0))
                {
                    SaveConfigXml();
                }
            }
        }

        private void DrawWindow(int id)
        {
            if (maidHack == null)
            {
                return;
            }

            var isMaidHackValid = maidHack.IsValid();
            var isTimelineValid = timelineManager.IsValidData();

            {
                var view = new GUIView(0, 0, WINDOW_WIDTH, 20);
                view.padding.y = 0;
                view.padding.x = 0;

                view.BeginLayout(GUIView.LayoutDirection.Free);

                view.currentPos.x = WINDOW_WIDTH - 20;

                if (view.DrawButton("x", 20, 20))
                {
                    isShowWnd = false;
                }

                view.currentPos.x = WINDOW_WIDTH - 400;

                if (!isMaidHackValid)
                {
                    view.DrawLabel(maidHack.errorMessage, 400 - 20, 20, Color.yellow);
                }
                else if (!isTimelineValid)
                {
                    view.DrawLabel(timelineManager.errorMessage, 400 - 20, 20, Color.yellow);
                }
                else
                {
                    // TIPSを表示したい
                    var message = "";
                    if (!maidHack.isPoseEditing)
                    {
                        var keyName = config.keyEditMode.GetKeyName();
                        message = "[" + keyName + "]キーでポーズ編集モードに切り替えます";
                    }
                    else
                    {
                        var keyName = config.keyAddKeyFrame.GetKeyName();
                        message = "[" + keyName + "]キーでキーフレームを追加します";
                    }

                    view.DrawLabel(message, 400 - 20, 20, Color.white);
                }
            }

            bool editEnabled = isMaidHackValid && isTimelineValid;

            {
                var view = new GUIView(0, 20, WINDOW_WIDTH, HEADER_HEIGHT);

                view.BeginLayout(GUIView.LayoutDirection.Horizontal);
                {
                    float buttonWidth = 70;

                    if (view.DrawButton("新規作成", buttonWidth, 20))
                    {
                        timelineManager.CreateNewTimeline();
                    }
                    
                    if (view.DrawButton("ロード", buttonWidth, 20))
                    {
                        subWindow.ToggleSubWindow(SubWindowType.TimelineLoad);
                    }
                    
                    if (view.DrawButton("セーブ", buttonWidth, 20, editEnabled))
                    {
                        timelineManager.SaveTimeline();
                    }

                    if (view.DrawButton("アニメ出力", buttonWidth, 20, editEnabled))
                    {
                        timelineManager.OutputAnm();
                    }

                    if (view.DrawButton("ｷｰﾌﾚｰﾑ", buttonWidth, 20, editEnabled))
                    {
                        subWindow.ToggleSubWindow(SubWindowType.KeyFrame);
                    }

                    var ikColor = editEnabled && timeline.isHoldActive ? Color.green : Color.white;
                    if (view.DrawButton("IK固定", buttonWidth, 20, editEnabled, ikColor))
                    {
                        subWindow.ToggleSubWindow(SubWindowType.IKHold);
                    }

                    if (view.DrawButton("動画再生", buttonWidth, 20, editEnabled))
                    {
                        subWindow.ToggleSubWindow(SubWindowType.MoviePlayer);
                    }

                    if (view.DrawButton("設定", buttonWidth, 20, editEnabled))
                    {
                        subWindow.ToggleSubWindow(SubWindowType.TimelineSetting);
                    }
                }
                view.EndLayout();

                if (!editEnabled)
                {
                    GUI.DragWindow();
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

                    if (view.DrawButton(".<", 30, 20))
                    {
                        newFrameNo = 0;
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
                    if (view.DrawButton(">.", 30, 20))
                    {
                        newFrameNo = timeline.maxFrameNo;
                    }

                    if (newFrameNo != timelineManager.currentFrameNo)
                    {
                        maidHack.isMotionPlaying = false;
                        timelineManager.SetCurrentFrame(newFrameNo);
                    }
                }

                view.AddSpace(10);

                if (timelineManager.isAnmPlaying)
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

                    anmSpeed = view.DrawSlider(anmSpeed, 0f, 2.0f, 100, 20);

                    if (!Mathf.Approximately(anmSpeed, timelineManager.anmSpeed))
                    {
                        timelineManager.anmSpeed = anmSpeed;
                    }
                }

                view.EndLayout();

                view.BeginLayout(GUIView.LayoutDirection.Horizontal);
                {
                    view.DrawLabel("キーフレーム", 100, 20);

                    if (view.DrawButton("追加", 50, 20, maidHack.isPoseEditing))
                    {
                        timelineManager.AddKeyFrameDiff();
                    }

                    if (view.DrawButton("全追加", 70, 20))
                    {
                        timelineManager.AddKeyFrameAll();
                    }

                    if (view.DrawButton("削除", 50, 20, timelineManager.HasSelected()))
                    {
                        timelineManager.RemoveSelectedFrame();
                    }

                    if (view.DrawButton("コピー", 70, 20, timelineManager.HasSelected()))
                    {
                        timelineManager.CopyFramesToClipboard();
                    }

                    if (view.DrawButton("ペースト", 70, 20))
                    {
                        timelineManager.PasteFramesFromClipboard(false);
                    }

                    if (view.DrawButton("反転P", 70, 20))
                    {
                        timelineManager.PasteFramesFromClipboard(true);
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
                    var newAllowPrecisionEdit = view.DrawToggle("簡易編集", config.isEasyEdit, 100, 20);
                    if (newAllowPrecisionEdit != config.isEasyEdit)
                    {
                        config.isEasyEdit = newAllowPrecisionEdit;
                        SaveConfigXml();
                        timelineManager.Refresh();
                    }

                    var isPoseEditing = view.DrawToggle("ポーズ編集", maidHack.isPoseEditing, 100, 20);
                    if (isPoseEditing != maidHack.isPoseEditing)
                    {
                        maidHack.isPoseEditing = isPoseEditing;
                    }

                    if (isOnStateStudioMode)
                    {
                        var newIsIkBoxVisibleRoot = view.DrawToggle("中心点IK表示", maidHack.isIkBoxVisibleRoot, 100, 20, isPoseEditing);
                        if (newIsIkBoxVisibleRoot != maidHack.isIkBoxVisibleRoot)
                        {
                            maidHack.isIkBoxVisibleRoot = newIsIkBoxVisibleRoot;
                        }

                        var newIsIkBoxVisibleBody = view.DrawToggle("関節IK表示", maidHack.isIkBoxVisibleBody, 100, 20, isPoseEditing);
                        if (newIsIkBoxVisibleBody != maidHack.isIkBoxVisibleBody)
                        {
                            maidHack.isIkBoxVisibleBody = newIsIkBoxVisibleBody;
                        }
                    }
                }
                view.EndLayout();
            }

            // タイムライン表示
            {
                var view = new GUIView(0, HEADER_HEIGHT, WINDOW_WIDTH, WINDOW_HEIGHT - HEADER_HEIGHT);

                view.BeginLayout(GUIView.LayoutDirection.Free);
                view.padding = Vector2.zero;

                view.currentPos.x = 100;
                view.currentPos.y = 20;

                var frameWidth = config.frameWidth;
                var frameHeight = config.frameHeight;

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

                    view.DrawTexture(texTimelineBG, bgColor);
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

                // 現在のフレーム表示
                view.currentPos.x = timelineManager.currentFrameNo * frameWidth + frameWidth / 2;
                view.currentPos.y = 0;
                view.DrawTexture(texWhite, 2, -1, Color.green);

                // キーフレーム表示
                var frames = timeline.keyFrames;
                var adjustY = (frameHeight - frameWidth) / 2;
                foreach (var frame in frames)
                {
                    var frameNo = frame.frameNo;

                    view.currentPos.x = frameNo * frameWidth;
                    if (view.currentPos.x < scrollPosition.x ||
                        view.currentPos.x > scrollPosition.x + viewWidth - 20)
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
                            () =>
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
                        var deltaFrameNo = (int) (frameDragDelta.x / frameWidth);
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
                        texWhite,
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
                        var frameNo = (int) ((scrollPosition.x + pos.x) / frameWidth);
                        maidHack.isMotionPlaying = false;
                        timelineManager.SetCurrentFrame(frameNo);
                    });

                // フレーム番号表示
                var frameNoWidth = 50;
                var adjustX = -frameNoWidth / 2 + frameWidth / 2;
                for (int frameNo = 0; frameNo < timeline.maxFrameCount; frameNo++)
                {
                    view.currentPos.x = 100 + frameNo * frameWidth - scrollPosition.x + adjustX;
                    if (view.currentPos.x < 100 - frameNoWidth / 2 ||
                        view.currentPos.x > WINDOW_WIDTH - frameNoWidth / 2)
                    {
                        continue;
                    }

                    if (frameNo == timelineManager.currentFrameNo)
                    {
                        view.DrawLabel(frameNo.ToString(), frameNoWidth, 20, Color.green, gsFrameLabel);
                    }
                    else if (frameNo % config.frameNoInterval == 0)
                    {
                        view.DrawLabel(frameNo.ToString(), frameNoWidth, 20, Color.white, gsFrameLabel);
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

                    var diplayName = menuItem.diplayName;
                    var isSelected = menuItem.isSelectedMenu;

                    view.currentPos.x = 0;

                    if (menuItem.isSetMenu) {
                        view.DrawLabel(
                            menuItem.isOpenMenu ? "ー" : "＋",
                            20,
                            20,
                            isSelected ? config.timelineMenuSelectTextColor: Color.white,
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
                        isSelected ? config.timelineMenuSelectTextColor: Color.white,
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

            if (!isFrameDragging && !isAreaDragging)
            {
                GUI.DragWindow();
            }
        }
    }
}