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
                view.GetFieldCache("移動範囲"),
                new GUIView.SliderOption
                {
                    min = 1f,
                    max = 10f,
                    step = 0.1f,
                    defaultValue = 5f,
                    value = config.positionRange,
                    onChanged = value =>
                    {
                        config.positionRange = value;
                        config.dirty = true;
                    },
                    labelWidth = 50,
                });

            view.DrawSliderValue(
                view.GetFieldCache("拡縮範囲"),
                new GUIView.SliderOption
                {
                    min = 1f,
                    max = 10f,
                    step = 0.1f,
                    defaultValue = 5f,
                    value = config.scaleRange,
                    onChanged = value =>
                    {
                        config.scaleRange = value;
                        config.dirty = true;
                    },
                    labelWidth = 50,
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
                view.GetFieldCache("背景透過度"),
                new GUIView.SliderOption
                {
                    min = 0f,
                    max = 1f,
                    step = 0,
                    defaultValue = 0.5f,
                    value = config.timelineBgAlpha,
                    onChanged = value =>
                    {
                        config.timelineBgAlpha = value;
                        config.dirty = true;
                    },
                    labelWidth = 100,
                });

            view.DrawSliderValue(
                view.GetIntFieldCache("メニュー幅"),
                new GUIView.SliderOption
                {
                    min = 100,
                    max = 300,
                    step = 0,
                    defaultValue = 100,
                    value = config.menuWidth,
                    onChanged = value =>
                    {
                        config.menuWidth = (int) value;
                        mainWindow.UpdateTexture();
                        config.dirty = true;
                    },
                    labelWidth = 100,
                });
            
            view.DrawSliderValue(
                view.GetIntFieldCache("ウィンドウ幅"),
                new GUIView.SliderOption
                {
                    min = MainWindow.MIN_WINDOW_WIDTH,
                    max = UnityEngine.Screen.width,
                    step = 0,
                    defaultValue = MainWindow.MIN_WINDOW_WIDTH,
                    value = config.windowWidth,
                    onChanged = value =>
                    {
                        config.windowWidth = (int) value;
                        mainWindow.UpdateTexture();
                        config.dirty = true;
                    },
                    labelWidth = 100,
                });
            
            view.DrawSliderValue(
                view.GetIntFieldCache("ウィンドウ高さ"),
                new GUIView.SliderOption
                {
                    min = MainWindow.MIN_WINDOW_HEIGHT,
                    max = UnityEngine.Screen.height,
                    step = 0,
                    defaultValue = MainWindow.MIN_WINDOW_HEIGHT,
                    value = config.windowHeight,
                    onChanged = value =>
                    {
                        config.windowHeight = (int) value;
                        mainWindow.UpdateTexture();
                        config.dirty = true;
                    },
                    labelWidth = 100,
                });

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
                var fieldCache = view.GetFieldCache("開始位置");
                var step = movieManager.frameRate > 0f ? 1f / movieManager.frameRate : 0.01f;

                view.DrawSliderValue(
                    fieldCache,
                    new GUIView.SliderOption
                    {
                        min = -1f,
                        max = movieManager.duration,
                        step = step,
                        defaultValue = 0f,
                        value = value,
                        onChanged = v => newValue = v,
                        labelWidth = 60,
                    });

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
                    var fieldCache = view.GetFieldCache(TransformDataBase.PositionNames[i]);

                    view.DrawSliderValue(
                        fieldCache,
                        new GUIView.SliderOption
                        {
                            min = -1f,
                            max = 1f,
                            step = 0.01f,
                            defaultValue = 0f,
                            value = value,
                            onChanged = newValue => newGUIPosition[i] = newValue,
                            labelWidth = 60,
                        });
                }

                if (newGUIPosition != guiPosition)
                {
                    timeline.videoGUIPosition = newGUIPosition;
                    movieManager.UpdateTransform();
                }

                view.DrawSliderValue(
                    view.GetFieldCache("表示サイズ"),
                    new GUIView.SliderOption
                    {
                        min = 0f,
                        max = 1f,
                        step = 0.01f,
                        defaultValue = 1f,
                        value = timeline.videoGUIScale,
                        onChanged = value =>
                        {
                            timeline.videoGUIScale = value;
                            movieManager.UpdateTransform();
                        },
                        labelWidth = 60,
                    });
                
                view.DrawSliderValue(
                    view.GetFieldCache("透過度"),
                    new GUIView.SliderOption
                    {
                        min = 0f,
                        max = 1f,
                        step = 0.01f,
                        defaultValue = 1f,
                        value = timeline.videoGUIAlpha,
                        onChanged = value =>
                        {
                            timeline.videoGUIAlpha = value;
                            movieManager.UpdateColor();
                        },
                        labelWidth = 60,
                    });
            }
            if (timeline.videoDisplayType == VideoDisplayType.Mesh)
            {
                var position = timeline.videoPosition;
                var newPosition = position;
                for (var i = 0; i < 3; i++)
                {
                    var value = position[i];
                    var fieldCache = view.GetFieldCache(TransformDataBase.PositionNames[i]);

                    view.DrawSliderValue(
                        fieldCache,
                        new GUIView.SliderOption
                        {
                            min = -config.positionRange,
                            max = config.positionRange,
                            step = 0.01f,
                            defaultValue = 0f,
                            value = value,
                            onChanged = newValue => newPosition[i] = newValue,
                            labelWidth = 60,
                        });
                }

                if (newPosition != position)
                {
                    timeline.videoPosition = newPosition;
                    movieManager.UpdateTransform();
                }

                var rotation = TransformDataBase.GetNormalizedEulerAngles(timeline.videoRotation);
                var newRotation = rotation;
                for (var i = 0; i < 3; i++)
                {
                    var value = rotation[i];
                    var fieldCache = view.GetFieldCache(TransformDataBase.RotationNames[i]);

                    view.DrawSliderValue(
                        fieldCache,
                        new GUIView.SliderOption
                        {
                            min = -180f,
                            max = 180f,
                            step = 1f,
                            defaultValue = 0f,
                            value = value,
                            onChanged = newValue => newRotation[i] = newValue,
                            labelWidth = 60,
                        });
                }

                if (newRotation != rotation)
                {
                    timeline.videoRotation = newRotation;
                    movieManager.UpdateTransform();
                }

                view.DrawSliderValue(
                    view.GetFieldCache("表示サイズ"),
                    new GUIView.SliderOption
                    {
                        min = 0f,
                        max = 5f,
                        step = 0.01f,
                        defaultValue = 1f,
                        value = timeline.videoScale,
                        onChanged = value =>
                        {
                            timeline.videoScale = value;
                            movieManager.UpdateTransform();
                        },
                        labelWidth = 60,
                    });
            }
            if (timeline.videoDisplayType == VideoDisplayType.Backmost)
            {
                var position = timeline.videoBackmostPosition;
                var newPosition = position;
                for (var i = 0; i < 2; i++)
                {
                    var value = position[i];
                    var fieldCache = view.GetFieldCache(TransformDataBase.PositionNames[i]);

                    view.DrawSliderValue(
                        fieldCache,
                        new GUIView.SliderOption
                        {
                            min = -1f,
                            max = 1f,
                            step = 0.01f,
                            defaultValue = 0f,
                            value = value,
                            onChanged = newValue => newPosition[i] = newValue,
                            labelWidth = 60,
                        });
                }

                if (newPosition != position)
                {
                    timeline.videoBackmostPosition = newPosition;
                    movieManager.UpdateMesh();
                }
            }

            view.DrawSliderValue(
                view.GetFieldCache("音量"),
                new GUIView.SliderOption
                {
                    min = 0f,
                    max = 1f,
                    step = 0.01f,
                    defaultValue = 0.5f,
                    value = timeline.videoVolume,
                    onChanged = newValue =>
                    {
                        timeline.videoVolume = newValue;
                        movieManager.UpdateVolume();
                    },
                    labelWidth = 60,
                });

            GUI.enabled = true;
        }
    }
}