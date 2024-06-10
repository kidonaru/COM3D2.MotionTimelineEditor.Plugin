using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using UnityEngine;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    public class TimelineSettingUI : SubWindowUIBase
    {
        public override string title
        {
            get
            {
                return "タイムライン 設定";
            }
        }

        public static readonly List<string> EyeMoveTypeNames =
            Enum.GetNames(typeof(Maid.EyeMoveType)).ToList();

        private enum TabType
        {
            Song,
            Common,
            BGM,
            Video,
            Max,
        }

        private TabType tabType = TabType.Song;

        public static readonly List<string> TabTypeNames = new List<string>
        {
            "個別",
            "共通",
            "BGM",
            "動画",
        };

        private static MainWindow mainWindow
        {
            get
            {
                return WindowManager.instance.mainWindow;
            }
        }

        public TimelineSettingUI(SubWindow subWindow) : base(subWindow)
        {
        }

        public override void DrawContent(GUIView view)
        {
            if (timeline == null)
            {
                return;
            }

            view.BeginLayout(GUIView.LayoutDirection.Horizontal);
            for (var i = 0; i < (int)TabType.Max; i++)
            {
                var type = (TabType)i;
                var color = tabType == type ? Color.green : Color.white;
                if (view.DrawButton(TabTypeNames[i], 50, 20, true, color))
                {
                    tabType = type;
                }
            }
            view.EndLayout();

            view.DrawHorizontalLine(Color.gray);

            switch (tabType)
            {
                case TabType.Song:
                    DrawSongSetting(view);
                    break;
                case TabType.Common:
                    DrawCommonSetting(view);
                    break;
                case TabType.BGM:
                    DrawBGMSetting(view);
                    break;
                case TabType.Video:
                    DrawVideoSetting(view);
                    break;
            }
        }

        private void DrawSongSetting(GUIView view)
        {
            view.DrawLabel("個別設定", 80, 20);

            view.DrawLabel("格納ディレクトリ名", -1, 20);
            timeline.directoryName = view.DrawTextField(timeline.directoryName, -1, 20);

            view.BeginLayout(GUIView.LayoutDirection.Horizontal);
            {
                var newFrameRate = view.DrawFloatField("フレームレート", timeline.frameRate, 150, 20);

                if (view.DrawButton("30", 30, 20))
                {
                    newFrameRate = 30;
                }

                if (view.DrawButton("60", 30, 20))
                {
                    newFrameRate = 60;
                }

                if (newFrameRate != timeline.frameRate)
                {
                    timeline.frameRate = newFrameRate;
                    timelineManager.ApplyCurrentFrame(true);
                }
            }
            view.EndLayout();

            var newEyeMoveType = (Maid.EyeMoveType)view.DrawSelectList(
                "メイド目線",
                EyeMoveTypeNames,
                (name, index) => name,
                250,
                20,
                (int)timeline.eyeMoveType);
            if (newEyeMoveType != timeline.eyeMoveType)
            {
                timeline.eyeMoveType = newEyeMoveType;
            }

            var newUseHeadKey = view.DrawToggle("顔/瞳の固定化", timeline.useHeadKey, 200, 20);
            if (newUseHeadKey != timeline.useHeadKey)
            {
                timeline.useHeadKey = newUseHeadKey;
            }

            view.BeginLayout(GUIView.LayoutDirection.Horizontal);
            {
                var newUseAnimekeyMuneL = view.DrawToggle("胸(左)の物理無効", timeline.useMuneKeyL, 120, 20);
                if (newUseAnimekeyMuneL != timeline.useMuneKeyL)
                {
                    timeline.useMuneKeyL = newUseAnimekeyMuneL;
                }

                var newUseAnimekeyMuneR = view.DrawToggle("胸(右)の物理無効", timeline.useMuneKeyR, 120, 20);
                if (newUseAnimekeyMuneR != timeline.useMuneKeyR)
                {
                    timeline.useMuneKeyR = newUseAnimekeyMuneR;
                }
            }
            view.EndLayout();

            var newIsLoopAnm = view.DrawToggle("ループアニメーション", timeline.isLoopAnm, 200, 20);
            if (newIsLoopAnm != timeline.isLoopAnm)
            {
                timeline.isLoopAnm = newIsLoopAnm;
                timelineManager.ApplyCurrentFrame(true);
            }

            view.BeginLayout(GUIView.LayoutDirection.Horizontal);
            {
                view.DrawLabel("オフセット時間", 70, 20);

                view.DrawLabel("開始", 30, 20);
                var newDcmStartOffset = view.DrawFloatField(timeline.startOffsetTime, 50, 20);
                if (newDcmStartOffset != timeline.startOffsetTime)
                {
                    timeline.startOffsetTime = newDcmStartOffset;
                }

                view.DrawLabel("終了", 30, 20);
                var newDcmEndOffset = view.DrawFloatField(timeline.endOffsetTime, 50, 20);
                if (newDcmEndOffset != timeline.endOffsetTime)
                {
                    timeline.endOffsetTime = newDcmEndOffset;
                }
            }
            view.EndLayout();

            view.BeginLayout(GUIView.LayoutDirection.Horizontal);
            {
                view.DrawLabel("フェード時間", 70, 20);

                view.DrawLabel("開始", 30, 20);
                var newDcmStartFade = view.DrawFloatField(timeline.startFadeTime, 50, 20);
                if (newDcmStartFade != timeline.startFadeTime)
                {
                    timeline.startFadeTime = newDcmStartFade;
                }

                view.DrawLabel("終了", 30, 20);
                var newDcmEndFade = view.DrawFloatField(timeline.endFadeTime, 50, 20);
                if (newDcmEndFade != timeline.endFadeTime)
                {
                    timeline.endFadeTime = newDcmEndFade;
                }
            }
            view.EndLayout();

            view.BeginLayout(GUIView.LayoutDirection.Horizontal);
            {
                if (view.DrawButton("初期化", 100, 20))
                {
                    PluginUtils.ShowConfirmDialog("個別設定を初期化しますか？", () =>
                    {
                        GameMain.Instance.SysDlg.Close();
                        timeline.ResetSettings();
                        timelineManager.Refresh();
                        timelineManager.ApplyCurrentFrame(true);
                    }, null);
                }

                view.AddSpace(10);

                var thumPath = timeline.thumPath;
                if (view.DrawButton("サムネ更新", 100, 20, File.Exists(thumPath)))
                {
                    timelineManager.SaveThumbnail();
                }
            }
            view.EndLayout();
        }

        private void DrawCommonSetting(GUIView view)
        {
            view.DrawLabel("共通設定", 80, 20);

            var newDefaultTangentType = (TangentType)view.DrawSelectList(
                                "初期補正曲線",
                                TangentData.TangentTypeNames,
                                (name, index) => name,
                                250,
                                20,
                                (int)config.defaultTangentType);
            if (newDefaultTangentType != config.defaultTangentType)
            {
                config.defaultTangentType = newDefaultTangentType;
                config.dirty = true;
            }

            var newDetailTransformCount = view.DrawIntField("Trans詳細表示数", config.detailTransformCount, 150, 20);
            if (newDetailTransformCount != config.detailTransformCount)
            {
                config.detailTransformCount = newDetailTransformCount;
                config.dirty = true;
            }

            var newDetailTangentCount = view.DrawIntField("Tangent表示数", config.detailTangentCount, 150, 20);
            if (newDetailTangentCount != config.detailTangentCount)
            {
                config.detailTangentCount = newDetailTangentCount;
                config.dirty = true;
            }

            view.DrawSliderValue(
                GetFieldValue("移動範囲"),
                1f, 10f, 0.1f,
                5f,
                config.positionRange,
                value =>
                {
                    config.positionRange = value;
                    config.dirty = true;
                });

            view.DrawSliderValue(
                GetFieldValue("拡縮範囲"),
                1f, 10f, 0.1f,
                5f,
                config.scaleRange,
                value =>
                {
                    config.scaleRange = value;
                    config.dirty = true;
                });

            view.BeginLayout(GUIView.LayoutDirection.Horizontal);
            {
                var newIsAutoScroll = view.DrawToggle("自動スクロール", config.isAutoScroll, 120, 20);
                if (newIsAutoScroll != config.isAutoScroll)
                {
                    config.isAutoScroll = newIsAutoScroll;
                    config.dirty = true;
                }

                var newDisablePoseHistory = view.DrawToggle("ポーズ履歴無効", config.disablePoseHistory, 120, 20);
                if (newDisablePoseHistory != config.disablePoseHistory)
                {
                    config.disablePoseHistory = newDisablePoseHistory;
                    config.dirty = true;
                }
            }
            view.EndLayout();

            view.BeginLayout(GUIView.LayoutDirection.Horizontal);
            {
                var newIsAutoYureBone = view.DrawToggle("自動揺れボーン", config.isAutoYureBone, 120, 20);
                if (newIsAutoYureBone != config.isAutoYureBone)
                {
                    config.isAutoYureBone = newIsAutoYureBone;
                    config.dirty = true;
                }
            }
            view.EndLayout();

            view.DrawSliderValue(
                GetFieldValue("背景透過度"),
                0f, 1f, 0.01f,
                0.5f,
                config.timelineBgAlpha,
                value =>
                {
                    config.timelineBgAlpha = value;
                    config.dirty = true;
                });

            view.BeginLayout(GUIView.LayoutDirection.Horizontal);
            {
                view.DrawLabel("メニュー幅", 100, 20);

                var newMenuWidth = view.DrawIntField(config.menuWidth, 50, 20);

                newMenuWidth = (int) view.DrawSlider(newMenuWidth, 100, 300, 100, 20);

                if (newMenuWidth != config.menuWidth)
                {
                    config.menuWidth = newMenuWidth;
                    mainWindow.UpdateTexture();
                    config.dirty = true;
                }
            }
            view.EndLayout();

            view.BeginLayout(GUIView.LayoutDirection.Horizontal);
            {
                view.DrawLabel("ウィンドウ幅", 100, 20);

                var newWindowWidth = view.DrawIntField(config.windowWidth, 50, 20);

                newWindowWidth = (int) view.DrawSlider(newWindowWidth, MainWindow.MIN_WINDOW_WIDTH, UnityEngine.Screen.width, 100, 20);

                if (newWindowWidth != config.windowWidth)
                {
                    config.windowWidth = newWindowWidth;
                    mainWindow.UpdateTexture();
                    config.dirty = true;
                }
            }
            view.EndLayout();

            view.BeginLayout(GUIView.LayoutDirection.Horizontal);
            {
                view.DrawLabel("ウィンドウ高さ", 100, 20);

                var newWindowHeight = view.DrawIntField(config.windowHeight, 50, 20);

                newWindowHeight = (int) view.DrawSlider(newWindowHeight, MainWindow.MIN_WINDOW_HEIGHT, UnityEngine.Screen.height, 100, 20);

                if (newWindowHeight != config.windowHeight)
                {
                    config.windowHeight = newWindowHeight;
                    mainWindow.UpdateTexture();
                    config.dirty = true;
                }
            }
            view.EndLayout();

            if (view.DrawButton("初期化", 100, 20))
            {
                PluginUtils.ShowConfirmDialog("共通設定を初期化しますか？", () =>
                {
                    GameMain.Instance.SysDlg.Close();
                    ConfigManager.instance.ResetConfig();
                    timelineManager.Refresh();
                }, null);
            }
        }

        private void DrawBGMSetting(GUIView view)
        {
            view.DrawLabel("BGM設定", 80, 20);

            view.BeginLayout(GUIView.LayoutDirection.Horizontal);
            {
                view.DrawLabel("BGMパス", 50, 20);

                if (view.DrawButton("選択", 50, 20))
                {
                    var openFileDialog = new OpenFileDialog
                    {
                        Title = "BGMファイルを選択してください",
                        Filter = "音楽ファイル (*.wav;*.ogg)|*.wav;*.ogg",
                        InitialDirectory = timeline.bgmPath,
                    };

                    if (openFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        var path = openFileDialog.FileName;
                        timeline.bgmPath = path;
                        bgmManager.Load();
                    }
                }

                if (view.DrawButton("再読込", 80, 20))
                {
                    bgmManager.Reload();
                }
            }
            view.EndLayout();

            timeline.bgmPath = view.DrawTextField(timeline.bgmPath, 240, 20);

            view.AddSpace(10);
            view.DrawHorizontalLine(Color.gray);
        }

        public static readonly List<string> VideoDisplayTypeNames = new List<string>
        {
            "GUI",
            "3Dビュー",
            "最背面",
        };

        private void DrawVideoSetting(GUIView view)
        {
            var isEnabled = timeline.videoEnabled;

            view.BeginLayout(GUIView.LayoutDirection.Horizontal);
            {
                view.DrawLabel("動画設定", 80, 20);

                isEnabled = view.DrawToggle("有効", isEnabled, 60, 20);
                if (isEnabled != timeline.videoEnabled)
                {
                    timeline.videoEnabled = isEnabled;
                    if (isEnabled)
                    {
                        movieManager.LoadMovie();
                    }
                    else
                    {
                        movieManager.UnloadMovie();
                    }
                }
            }
            view.EndLayout();

            view.BeginLayout(GUIView.LayoutDirection.Horizontal);
            {
                view.DrawLabel("表示形式", 80, 20);

                var newVideoDisplayType = (VideoDisplayType) view.DrawSelectList(
                    VideoDisplayTypeNames,
                    (name, index) => name,
                    100,
                    20,
                    (int) timeline.videoDisplayType
                );

                if (newVideoDisplayType != timeline.videoDisplayType)
                {
                    timeline.videoDisplayType = newVideoDisplayType;
                    movieManager.ReloadMovie();
                }
            }
            view.EndLayout();

            GUI.enabled = isEnabled;

            view.BeginLayout(GUIView.LayoutDirection.Horizontal);
            {
                view.DrawLabel("動画パス", 50, 20);

                if (view.DrawButton("選択", 50, 20))
                {
                    var openFileDialog = new OpenFileDialog
                    {
                        Title = "動画ファイルを選択してください",
                        Filter = "動画ファイル (*.mp4;*.avi;*.wmv;*.mov;*.flv;*.mkv;*.webm)|*.mp4;*.avi;*.wmv;*.mov;*.flv;*.mkv;*.webm|すべてのファイル (*.*)|*.*",
                        InitialDirectory = timeline.videoPath
                    };

                    if (openFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        var path = openFileDialog.FileName;
                        timeline.videoPath = path;
                        movieManager.LoadMovie();
                    }
                }

                if (view.DrawButton("再読込", 80, 20))
                {
                    movieManager.ReloadMovie();
                }
            }
            view.EndLayout();

            timeline.videoPath = view.DrawTextField(timeline.videoPath, 240, 20);

            view.BeginLayout(GUIView.LayoutDirection.Horizontal);
            {
                var value = timeline.videoStartTime;
                var newValue = value;
                var fieldValue = GetFieldValue("開始位置");

                view.DrawValue(
                    fieldValue,
                    1f / movieManager.frameRate,
                    1f,
                    0f,
                    value,
                    _newValue => newValue = _newValue,
                    _diffValue => newValue += _diffValue
                );

                if (newValue != value)
                {
                    timeline.videoStartTime = newValue;
                    movieManager.UpdateSeekTime();
                }
            }
            view.EndLayout();

            if (timeline.videoDisplayType == VideoDisplayType.GUI)
            {
                var guiPosition = timeline.videoGUIPosition;
                var newGUIPosition = guiPosition;
                for (var i = 0; i < 2; i++)
                {
                    var value = guiPosition[i];
                    var fieldValue = GetFieldValue(TransformDataBase.PositionNames[i]);

                    view.DrawValue(
                        fieldValue, 0.01f, 0.1f, 0f,
                        value,
                        newValue => newGUIPosition[i] = newValue,
                        diffValue => newGUIPosition[i] += diffValue
                    );
                }

                if (newGUIPosition != guiPosition)
                {
                    timeline.videoGUIPosition = newGUIPosition;
                    movieManager.UpdateTransform();
                }

                view.BeginLayout(GUIView.LayoutDirection.Horizontal);
                {
                    view.DrawLabel("表示サイズ", 60, 20);

                    var newScale = view.DrawFloatField(timeline.videoGUIScale, 50, 20);

                    if (view.DrawButton("R", 20, 20))
                    {
                        newScale = 1.0f;
                    }

                    newScale = view.DrawSlider(newScale, 0f, 1f, 100, 20);

                    if (newScale != timeline.videoGUIScale)
                    {
                        timeline.videoGUIScale = newScale;
                        movieManager.UpdateTransform();
                    }
                }
                view.EndLayout();

                view.BeginLayout(GUIView.LayoutDirection.Horizontal);
                {
                    view.DrawLabel("透過度", 60, 20);

                    var newAlpha = view.DrawFloatField(timeline.videoGUIAlpha, 50, 20);

                    if (view.DrawButton("R", 20, 20))
                    {
                        newAlpha = 1f;
                    }

                    newAlpha = view.DrawSlider(newAlpha, 0f, 1.0f, 100, 20);

                    if (newAlpha != timeline.videoGUIAlpha)
                    {
                        timeline.videoGUIAlpha = newAlpha;
                        movieManager.UpdateColor();
                    }
                }
                view.EndLayout();
            }
            if (timeline.videoDisplayType == VideoDisplayType.Mesh)
            {
                var position = timeline.videoPosition;
                var newPosition = position;
                for (var i = 0; i < 3; i++)
                {
                    var value = position[i];
                    var fieldValue = GetFieldValue(TransformDataBase.PositionNames[i]);

                    view.DrawValue(
                        fieldValue, 0.01f, 0.1f, 0f,
                        value,
                        newValue => newPosition[i] = newValue,
                        diffValue => newPosition[i] += diffValue
                    );
                }

                if (newPosition != position)
                {
                    timeline.videoPosition = newPosition;
                    movieManager.UpdateTransform();
                }

                var rotation = timeline.videoRotation;
                var newRotation = rotation;
                for (var i = 0; i < 3; i++)
                {
                    var value = rotation[i];
                    var fieldValue = GetFieldValue(TransformDataBase.RotationNames[i]);

                    view.DrawValue(
                        fieldValue, 1f, 10f, 0f,
                        value,
                        newValue => newRotation[i] = newValue,
                        diffValue => newRotation[i] += diffValue
                    );
                }

                if (newRotation != rotation)
                {
                    timeline.videoRotation = newRotation;
                    movieManager.UpdateTransform();
                }

                view.BeginLayout(GUIView.LayoutDirection.Horizontal);
                {
                    view.DrawLabel("表示サイズ", 60, 20);

                    var newScale = view.DrawFloatField(timeline.videoScale, 50, 20);

                    if (view.DrawButton("R", 20, 20))
                    {
                        newScale = 1.0f;
                    }

                    newScale = view.DrawSlider(newScale, 0f, 5f, 100, 20);

                    if (newScale != timeline.videoScale)
                    {
                        timeline.videoScale = newScale;
                        movieManager.UpdateTransform();
                    }
                }
                view.EndLayout();
            }
            if (timeline.videoDisplayType == VideoDisplayType.Backmost)
            {
                var position = timeline.videoBackmostPosition;
                var newPosition = position;
                for (var i = 0; i < 2; i++)
                {
                    var value = position[i];
                    var fieldValue = GetFieldValue(TransformDataBase.PositionNames[i]);

                    view.DrawValue(
                        fieldValue, 0.01f, 0.1f, 0f,
                        value,
                        newValue => newPosition[i] = newValue,
                        diffValue => newPosition[i] += diffValue
                    );
                }

                if (newPosition != position)
                {
                    timeline.videoBackmostPosition = newPosition;
                    movieManager.UpdateMesh();
                }
            }

            view.BeginLayout(GUIView.LayoutDirection.Horizontal);
            {
                view.DrawLabel("音量", 60, 20);

                var newVolume = view.DrawFloatField(timeline.videoVolume, 50, 20);

                if (view.DrawButton("R", 20, 20))
                {
                    newVolume = 0.5f;
                }

                newVolume = view.DrawSlider(newVolume, 0f, 1.0f, 100, 20);

                if (newVolume != timeline.videoVolume)
                {
                    timeline.videoVolume = newVolume;
                    movieManager.UpdateVolume();
                }
            }
            view.EndLayout();

            GUI.enabled = true;
        }
    }
}