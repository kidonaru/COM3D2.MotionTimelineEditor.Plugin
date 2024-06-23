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

        private enum TabType
        {
            個別,
            共通,
            BGM,
            動画,
        }

        private TabType _tabType = TabType.個別;

        private GUIComboBox<TangentType> _defaultTangentTypeComboBox = new GUIComboBox<TangentType>
        {
            items = Enum.GetValues(typeof(TangentType)).Cast<TangentType>().ToList(),
            getName = (type, index) => TangentData.TangentTypeNames[index],
            onSelected = (type, index) =>
            {
                config.defaultTangentType = type;
                config.dirty = true;
            },
        };

        private GUIComboBox<MoveEasingType> _defaultEasingTypeComboBox = new GUIComboBox<MoveEasingType>
        {
            items = Enum.GetValues(typeof(MoveEasingType)).Cast<MoveEasingType>().ToList(),
            getName = (type, index) => type.ToString(),
            onSelected = (type, index) =>
            {
                config.defaultEasingType = type;
                config.dirty = true;
            },
        };

        private GUIComboBox<Maid.EyeMoveType> _eyeMoveTypeComboBox = new GUIComboBox<Maid.EyeMoveType>
        {
            items = Enum.GetValues(typeof(Maid.EyeMoveType)).Cast<Maid.EyeMoveType>().ToList(),
            getName = (type, index) => type.ToString(),
            onSelected = (type, index) =>
            {
                timeline.eyeMoveType = type;
            },
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

            _tabType = view.DrawTabs(_tabType, 50, 20);

            view.DrawHorizontalLine(Color.gray);

            switch (_tabType)
            {
                case TabType.個別:
                    DrawSongSetting(view);
                    break;
                case TabType.共通:
                    DrawCommonSetting(view);
                    break;
                case TabType.BGM:
                    DrawBGMSetting(view);
                    break;
                case TabType.動画:
                    DrawVideoSetting(view);
                    break;
            }
        }

        private void DrawSongSetting(GUIView view)
        {
            view.DrawLabel("個別設定", 80, 20);

            view.DrawLabel("格納ディレクトリ名", -1, 20);
            view.DrawTextField(timeline.directoryName, -1, 20, newText => timeline.directoryName = newText);

            view.BeginHorizontal();
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

            _eyeMoveTypeComboBox.currentIndex = (int)timeline.eyeMoveType;
            _eyeMoveTypeComboBox.DrawButton("メイド目線", view);

            var newUseHeadKey = view.DrawToggle("顔/瞳の固定化", timeline.useHeadKey, 120, 20);
            if (newUseHeadKey != timeline.useHeadKey)
            {
                timeline.useHeadKey = newUseHeadKey;
            }

            view.BeginHorizontal();
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

            var newIsLoopAnm = view.DrawToggle("ループアニメーション", timeline.isLoopAnm, 150, 20);
            if (newIsLoopAnm != timeline.isLoopAnm)
            {
                timeline.isLoopAnm = newIsLoopAnm;
                timelineManager.ApplyCurrentFrame(true);
            }

            view.BeginHorizontal();
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

            view.BeginHorizontal();
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

            view.BeginHorizontal();
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

            _defaultTangentTypeComboBox.currentIndex = (int)config.defaultTangentType;
            _defaultTangentTypeComboBox.DrawButton("初期補間曲線", view);

            _defaultEasingTypeComboBox.currentIndex = (int)config.defaultEasingType;
            _defaultEasingTypeComboBox.DrawButton("初期イージング", view);

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
                new GUIView.SliderOption
                {
                    label = "移動範囲",
                    labelWidth = 50,
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
                });

            view.DrawSliderValue(
                new GUIView.SliderOption
                {
                    label = "拡縮範囲",
                    labelWidth = 50,
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
                });

            view.BeginHorizontal();
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

            view.BeginHorizontal();
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
                new GUIView.SliderOption
                {
                    label = "ボイス最大秒数",
                    labelWidth = 100,
                    min = 1f,
                    max = 30f,
                    step = 0,
                    defaultValue = 20f,
                    value = config.voiceMaxLength,
                    onChanged = value =>
                    {
                        config.voiceMaxLength = value;
                        config.dirty = true;
                    },
                });

            view.DrawSliderValue(
                new GUIView.SliderOption
                {
                    label = "背景透過度",
                    labelWidth = 100,
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
                });

            view.DrawSliderValue(
                new GUIView.SliderOption
                {
                    label = "メニュー幅",
                    labelWidth = 100,
                    fieldType = FloatFieldType.Int,
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
                });
            
            view.DrawSliderValue(
                new GUIView.SliderOption
                {
                    label = "ウィンドウ幅",
                    labelWidth = 100,
                    fieldType = FloatFieldType.Int,
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
                });
            
            view.DrawSliderValue(
                new GUIView.SliderOption
                {
                    label = "ウィンドウ高さ",
                    labelWidth = 100,
                    fieldType = FloatFieldType.Int,
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

            view.BeginHorizontal();
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

            view.DrawTextField(timeline.bgmPath, 240, 20, newText => timeline.bgmPath = newText);

            view.AddSpace(10);
            view.DrawHorizontalLine(Color.gray);
        }

        public static readonly List<string> VideoDisplayTypeNames = new List<string>
        {
            "GUI",
            "3Dビュー",
            "最背面",
        };

        private GUIComboBox<VideoDisplayType> _videoDisplayTypeComboBox = new GUIComboBox<VideoDisplayType>
        {
            items = Enum.GetValues(typeof(VideoDisplayType)).Cast<VideoDisplayType>().ToList(),
            getName = (type, index) => VideoDisplayTypeNames[index],
            onSelected = (type, index) =>
            {
                timeline.videoDisplayType = type;
                movieManager.ReloadMovie();
            },
        }; 

        private void DrawVideoSetting(GUIView view)
        {
            var isEnabled = timeline.videoEnabled;

            view.BeginHorizontal();
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

            _videoDisplayTypeComboBox.currentIndex = (int)timeline.videoDisplayType;
            _videoDisplayTypeComboBox.DrawButton("表示形式", view);

            view.SetEnabled(isEnabled);

            view.BeginHorizontal();
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

            view.DrawTextField(timeline.videoPath, 240, 20, newText => timeline.videoPath = newText);

            view.DrawSliderValue(
                new GUIView.SliderOption
                {
                    label = "開始位置",
                    labelWidth = 60,
                    min = -1f,
                    max = movieManager.duration,
                    step = movieManager.frameRate > 0f ? 1f / movieManager.frameRate : 0.01f,
                    defaultValue = 0f,
                    value = timeline.videoStartTime,
                    onChanged = newValue =>
                    {
                        timeline.videoStartTime = newValue;
                        movieManager.UpdateSeekTime();
                    },
                });

            if (timeline.videoDisplayType == VideoDisplayType.GUI)
            {
                var guiPosition = timeline.videoGUIPosition;
                var newGUIPosition = guiPosition;
                for (var i = 0; i < 2; i++)
                {
                    var value = guiPosition[i];

                    view.DrawSliderValue(
                        new GUIView.SliderOption
                        {
                            label = TransformDataBase.PositionNames[i],
                            labelWidth = 60,
                            min = -1f,
                            max = 1f,
                            step = 0.01f,
                            defaultValue = 0f,
                            value = value,
                            onChanged = newValue => newGUIPosition[i] = newValue,
                        });
                }

                if (newGUIPosition != guiPosition)
                {
                    timeline.videoGUIPosition = newGUIPosition;
                    movieManager.UpdateTransform();
                }

                view.DrawSliderValue(
                    new GUIView.SliderOption
                    {
                        label = "表示サイズ",
                        labelWidth = 60,
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
                    });
                
                view.DrawSliderValue(
                    new GUIView.SliderOption
                    {
                        label = "透過度",
                        labelWidth = 60,
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
                    });
            }
            if (timeline.videoDisplayType == VideoDisplayType.Mesh)
            {
                var position = timeline.videoPosition;
                var newPosition = position;
                for (var i = 0; i < 3; i++)
                {
                    var value = position[i];

                    view.DrawSliderValue(
                        new GUIView.SliderOption
                        {
                            label = TransformDataBase.PositionNames[i],
                            labelWidth = 60,
                            min = -config.positionRange,
                            max = config.positionRange,
                            step = 0.01f,
                            defaultValue = 0f,
                            value = value,
                            onChanged = newValue => newPosition[i] = newValue,
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

                    view.DrawSliderValue(
                        new GUIView.SliderOption
                        {
                            label = TransformDataBase.RotationNames[i],
                            labelWidth = 60,
                            min = -180f,
                            max = 180f,
                            step = 1f,
                            defaultValue = 0f,
                            value = value,
                            onChanged = newValue => newRotation[i] = newValue,
                        });
                }

                if (newRotation != rotation)
                {
                    timeline.videoRotation = newRotation;
                    movieManager.UpdateTransform();
                }

                view.DrawSliderValue(
                    new GUIView.SliderOption
                    {
                        label = "表示サイズ",
                        labelWidth = 60,
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
                    });
            }
            if (timeline.videoDisplayType == VideoDisplayType.Backmost)
            {
                var position = timeline.videoBackmostPosition;
                var newPosition = position;
                for (var i = 0; i < 2; i++)
                {
                    var value = position[i];

                    view.DrawSliderValue(
                        new GUIView.SliderOption
                        {
                            label = TransformDataBase.PositionNames[i],
                            labelWidth = 60,
                            min = -2f,
                            max = 2f,
                            step = 0.01f,
                            defaultValue = 0f,
                            value = value,
                            onChanged = newValue => newPosition[i] = newValue,
                        });
                }

                if (newPosition != position)
                {
                    timeline.videoBackmostPosition = newPosition;
                    movieManager.UpdateMesh();
                }
            }

            view.DrawSliderValue(
                new GUIView.SliderOption
                {
                    label = "音量",
                    labelWidth = 60,
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
                });

            view.SetEnabled(true);
        }
    }
}