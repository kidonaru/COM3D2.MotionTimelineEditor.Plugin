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
        public override string title => "タイムライン 設定";

        private enum TabType
        {
            個別,
            共通,
            BGM,
            動画,
            ｸﾞﾘｯﾄﾞ,
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

        private static string[] SingleFrameTypeNames = new string[]
        {
            "なし",
            "1F遅らせる",
            "1F早める",
        };

        private GUIComboBox<SingleFrameType> _singleFrameTypeComboBox = new GUIComboBox<SingleFrameType>
        {
            items = Enum.GetValues(typeof(SingleFrameType)).Cast<SingleFrameType>().ToList(),
            getName = (type, index) => SingleFrameTypeNames[index],
            onSelected = (type, index) =>
            {
                timeline.singleFrameType = type;
                timelineManager.ApplyCurrentFrame(true);
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

            view.AddSpace(5);

            view.BeginScrollView();

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
                case TabType.ｸﾞﾘｯﾄﾞ:
                    DrawGridSetting(view);
                    break;
            }

            view.EndScrollView();
        }

        private void DrawSongSetting(GUIView view)
        {
            view.DrawLabel("個別設定", 100, 20);

            view.DrawLabel("格納ディレクトリ名", -1, 20);
            view.DrawTextField(timeline.directoryName, -1, 20, newText => timeline.directoryName = newText);

            view.BeginHorizontal();
            {
                var newFrameRate = timeline.frameRate;

                view.DrawFloatField(new GUIView.FloatFieldOption
                {
                    label = "フレームレート",
                    value = timeline.frameRate,
                    width = 150,
                    height = 20,
                    onChanged = x => newFrameRate = x,
                });

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

            _singleFrameTypeComboBox.currentIndex = (int)timeline.singleFrameType;
            _singleFrameTypeComboBox.DrawButton("1フレーム調整", view);

            view.DrawToggle("顔/瞳の固定化", timeline.useHeadKey, 120, 20, newValue =>
            {
                timeline.useHeadKey = newValue;
            });

            view.BeginHorizontal();
            {
                view.DrawToggle("胸(左)の物理無効", timeline.useMuneKeyL, 120, 20, newValue =>
                {
                    timeline.useMuneKeyL = newValue;
                });

                view.DrawToggle("胸(右)の物理無効", timeline.useMuneKeyR, 120, 20, newValue =>
                {
                    timeline.useMuneKeyR = newValue;
                });
            }
            view.EndLayout();

            view.DrawToggle("ループアニメーション", timeline.isLoopAnm, -1, 20, newValue =>
            {
                timeline.isLoopAnm = newValue;
                timelineManager.ApplyCurrentFrame(true);
            });

            view.DrawToggle("イージングを次のキーフレームに適用", timeline.isEasingAppliedToNextKeyframe, -1, 20, newValue =>
            {
                timeline.isEasingAppliedToNextKeyframe = newValue;
                timelineManager.ApplyCurrentFrame(true);
            });

            view.DrawToggle("カメラのタンジェント補間を有効化", timeline.isTangentCamera, -1, 20, newValue =>
            {
                timeline.isTangentCamera = newValue;

                var targetLayer = timelineManager.GetLayer(typeof(CameraTimelineLayer));
                if (targetLayer != null)
                {
                    targetLayer.InitTangent();
                    targetLayer.ApplyCurrentFrame(true);
                }
            });

            view.DrawToggle("メイド移動のタンジェント補間を有効化", timeline.isTangentMove, -1, 20, newValue =>
            {
                timeline.isTangentMove = newValue;

                var targetLayers = timelineManager.FindLayers(typeof(MoveTimelineLayer));
                foreach (var targetLayer in targetLayers)
                {
                    targetLayer.InitTangent();
                    targetLayer.ApplyCurrentFrame(true);
                }
            });

            view.DrawToggle("ポストエフェクトの色拡張", timeline.usePostEffectExtraColor, -1, 20, newValue =>
            {
                timeline.usePostEffectExtraColor = newValue;
            });

            view.DrawToggle("ポストエフェクトのブレンド拡張", timeline.usePostEffectExtraBlend, -1, 20, newValue =>
            {
                timeline.usePostEffectExtraBlend = newValue;
            });

            view.DrawToggle("地面色表示を背景表示と連動", timeline.isGroundLinkedToBackground, -1, 20, newValue =>
            {
                timeline.isGroundLinkedToBackground = newValue;
            });

            view.BeginHorizontal();
            {
                view.DrawLabel("オフセット時間", 70, 20);

                view.DrawFloatField(new GUIView.FloatFieldOption
                {
                    label = "開始",
                    labelWidth = 30,
                    value = timeline.startOffsetTime,
                    width = 80,
                    height = 20,
                    onChanged = x => timeline.startOffsetTime = x,
                });

                view.DrawFloatField(new GUIView.FloatFieldOption
                {
                    label = "終了",
                    labelWidth = 30,
                    value = timeline.endOffsetTime,
                    width = 80,
                    height = 20,
                    onChanged = x => timeline.endOffsetTime = x,
                });
            }
            view.EndLayout();

            view.BeginHorizontal();
            {
                view.DrawLabel("フェード時間", 70, 20);

                view.DrawFloatField(new GUIView.FloatFieldOption
                {
                    label = "開始",
                    labelWidth = 30,
                    value = timeline.startFadeTime,
                    width = 80,
                    height = 20,
                    onChanged = x => timeline.startFadeTime = x,
                });

                view.DrawFloatField(new GUIView.FloatFieldOption
                {
                    label = "終了",
                    labelWidth = 30,
                    value = timeline.endFadeTime,
                    width = 80,
                    height = 20,
                    onChanged = x => timeline.endFadeTime = x,
                });
            }
            view.EndLayout();

            view.BeginHorizontal();
            {
                view.DrawLabel("アスペクト比", 70, 20);

                view.DrawFloatField(new GUIView.FloatFieldOption
                {
                    label = "幅",
                    labelWidth = 30,
                    value = timeline.aspectWidth,
                    width = 80,
                    height = 20,
                    onChanged = x => timeline.aspectWidth = x,
                });

                view.DrawFloatField(new GUIView.FloatFieldOption
                {
                    label = "高さ",
                    labelWidth = 30,
                    value = timeline.aspectHeight,
                    width = 80,
                    height = 20,
                    onChanged = x => timeline.aspectHeight = x,
                });
            }
            view.EndLayout();

            view.DrawSliderValue(new GUIView.SliderOption
            {
                label = "ﾚﾀｰﾎﾞｯｸｽ透過度",
                labelWidth = 100,
                min = 0f,
                max = 1f,
                defaultValue = 1f,
                value = timeline.letterBoxAlpha,
                onChanged = value =>
                {
                    timeline.letterBoxAlpha = value;
                    cameraManager.ResetCache();
                },
            });

            view.DrawHorizontalLine(Color.gray);

            view.DrawLabel("連番画像出力設定", 100, 20);

            view.BeginHorizontal();
            {
                var newFrameRate = timeline.imageOutputFrameRate;

                view.DrawFloatField(new GUIView.FloatFieldOption
                {
                    label = "フレームレート",
                    value = timeline.imageOutputFrameRate,
                    width = 150,
                    height = 20,
                    onChanged = x => newFrameRate = x,
                });

                if (view.DrawButton("30", 30, 20))
                {
                    newFrameRate = 30;
                }

                if (view.DrawButton("60", 30, 20))
                {
                    newFrameRate = 60;
                }

                if (newFrameRate != timeline.imageOutputFrameRate)
                {
                    timeline.imageOutputFrameRate = newFrameRate;
                }
            }
            view.EndLayout();

            view.DrawTextField(new GUIView.TextFieldOption
            {
                label = "出力名",
                labelWidth = 50,
                value = timeline.imageOutputFormat,
                onChanged = value => timeline.imageOutputFormat = value,
                hiddenButton = true,
            });

            view.BeginHorizontal();
            {
                view.DrawLabel("画像サイズ", 70, 20);

                view.DrawFloatField(new GUIView.FloatFieldOption
                {
                    label = "幅",
                    labelWidth = 30,
                    fieldType = FloatFieldType.Int,
                    value = timeline.imageOutputSize.x,
                    width = 80,
                    height = 20,
                    onChanged = x => timeline.imageOutputSize.x = x,
                });

                view.DrawFloatField(new GUIView.FloatFieldOption
                {
                    label = "高さ",
                    labelWidth = 30,
                    fieldType = FloatFieldType.Int,
                    value = timeline.imageOutputSize.y,
                    width = 80,
                    height = 20,
                    onChanged = x => timeline.imageOutputSize.y = x,
                });
            }
            view.EndLayout();

            view.DrawHorizontalLine(Color.gray);

            view.BeginHorizontal();
            {
                var enabled = PluginUtils.IsExistsDcmSongDirPath(timeline.dcmSongName);
                if (view.DrawButton("DCM出力先を開く", 120, 20, enabled))
                {
                    var dirPath = PluginUtils.GetDcmSongDirPath(timeline.dcmSongName);
                    MTEUtils.OpenDirectory(dirPath);
                }

                enabled = PluginUtils.IsExistsImageOutputDirPath(timeline.anmName);
                if (view.DrawButton("画像出力先を開く", 120, 20, enabled))
                {
                    var dirPath = PluginUtils.GetImageOutputDirPath(timeline.anmName);
                    MTEUtils.OpenDirectory(dirPath);
                }
            }
            view.EndLayout();

            view.BeginHorizontal();
            {
                if (view.DrawButton("初期化", 100, 20))
                {
                    MTEUtils.ShowConfirmDialog("個別設定を初期化しますか？", () =>
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

            if (view.DrawButton("デバッグ情報表示", 120, 20))
            {
                MotionTimelineEditor.instance.DumpDebugInfo();
            }
        }

        private void DrawCommonSetting(GUIView view)
        {
            view.DrawLabel("共通設定", 100, 20);

            _defaultTangentTypeComboBox.currentIndex = (int)config.defaultTangentType;
            _defaultTangentTypeComboBox.DrawButton("初期補間曲線", view);

            _defaultEasingTypeComboBox.currentIndex = (int)config.defaultEasingType;
            _defaultEasingTypeComboBox.DrawButton("初期イージング", view);

            view.DrawIntField(new GUIView.IntFieldOption
            {
                label = "Trans詳細表示数",
                value = config.detailTransformCount,
                width = 150,
                height = 20,
                onChanged = x =>
                {
                    config.detailTransformCount = x;
                    config.dirty = true;
                }
            });

            view.DrawIntField(new GUIView.IntFieldOption
            {
                label = "Tangent表示数",
                value = config.detailTangentCount,
                width = 150,
                height = 20,
                onChanged = x =>
                {
                    config.detailTangentCount = x;
                    config.dirty = true;
                }
            });

            view.DrawSliderValue(new GUIView.SliderOption
            {
                label = "移動範囲",
                labelWidth = 50,
                min = 1f,
                max = 100f,
                step = 0.1f,
                defaultValue = 5f,
                value = config.positionRange,
                onChanged = value =>
                {
                    config.positionRange = value;
                    config.dirty = true;
                },
            });

            view.DrawSliderValue(new GUIView.SliderOption
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
                view.DrawToggle("自動スクロール", config.isAutoScroll, 120, 20, newValue =>
                {
                    config.isAutoScroll = newValue;
                    config.dirty = true;
                });

                view.DrawToggle("ポーズ履歴無効", config.disablePoseHistory, 120, 20, newValue =>
                {
                    config.disablePoseHistory = newValue;
                    config.dirty = true;
                });
            }
            view.EndLayout();

            view.BeginHorizontal();
            {
                view.DrawToggle("自動揺れボーン", config.isAutoYureBone, 120, 20, newValue =>
                {
                    config.isAutoYureBone = newValue;
                    config.dirty = true;
                });
            }
            view.EndLayout();

            view.DrawSliderValue(new GUIView.SliderOption
            {
                label = "動画先読み秒数",
                labelWidth = 100,
                min = 0f,
                max = 1f,
                step = 0,
                defaultValue = 0.5f,
                value = config.videoPrebufferTime,
                onChanged = value =>
                {
                    config.videoPrebufferTime = value;
                    config.dirty = true;
                },
            });

            view.DrawSliderValue(new GUIView.SliderOption
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

            view.DrawSliderValue(new GUIView.SliderOption
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

            view.DrawSliderValue(new GUIView.SliderOption
            {
                label = "ボーンリスト幅",
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
                    mainWindow.requestUpdateTexture = true;
                    config.dirty = true;
                },
            });
            
            view.DrawSliderValue(new GUIView.SliderOption
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
                    mainWindow.requestUpdateTexture = true;
                    config.dirty = true;
                },
            });
            
            view.DrawSliderValue(new GUIView.SliderOption
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
                    mainWindow.requestUpdateTexture = true;
                    config.dirty = true;
                },
            });

            view.DrawToggle("自動でBackgroundCustomに登録", config.autoResisterBackgroundCustom, -1, 20, newValue =>
            {
                config.autoResisterBackgroundCustom = newValue;
                config.dirty = true;
            });

            view.DrawToggle("処理時間出力", config.outputElapsedTime, 120, 20, newValue =>
            {
                config.outputElapsedTime = newValue;
                config.dirty = true;
            });

            view.DrawToggle("メモリ使用量出力", MTEUtils.showMemoryUsage, 120, 20, newValue =>
            {
                MTEUtils.showMemoryUsage = newValue;
            });

            if (view.DrawButton("初期化", 100, 20))
            {
                MTEUtils.ShowConfirmDialog("共通設定を初期化しますか？", () =>
                {
                    GameMain.Instance.SysDlg.Close();
                    ConfigManager.instance.ResetConfig();
                    timelineManager.Refresh();
                }, null);
            }
        }

        private void DrawBGMSetting(GUIView view)
        {
            view.DrawLabel("BGM設定", 100, 20);

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

            view.DrawSliderValue(new GUIView.SliderOption
            {
                label = "音量",
                labelWidth = 50,
                fieldType = FloatFieldType.Int,
                min = 0,
                max = 100,
                step = 0,
                defaultValue = 100,
                value = bgmManager.volumeDance,
                onChanged = value =>
                {
                    bgmManager.volumeDance = (int) value;
                    config.dirty = true;
                },
            });

            view.DrawToggle("BPMライン表示", timeline.isShowBPMLine, 120, 20, newValue =>
            {
                timeline.isShowBPMLine = newValue;
            });

            view.DrawSliderValue(new GUIView.SliderOption
            {
                label = "BPM",
                labelWidth = 50,
                min = 1,
                max = 300,
                step = 0.1f,
                defaultValue = 120,
                value = timeline.bpm,
                onChanged = value => timeline.bpm = value,
            });

            view.DrawSliderValue(new GUIView.SliderOption
            {
                label = "オフセット",
                labelWidth = 50,
                min = -timeline.frameRate,
                max = timeline.frameRate,
                step = 0.1f,
                defaultValue = 0,
                value = timeline.bpmLineOffsetFrame,
                onChanged = value => timeline.bpmLineOffsetFrame = value,
            });

            view.AddSpace(10);
            view.DrawHorizontalLine(Color.gray);
        }

        public static readonly List<string> VideoDisplayTypeNames = new List<string>
        {
            "GUI",
            "3Dビュー",
            "最背面",
            "最前面",
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
                view.DrawLabel("動画設定", 100, 20);

                view.DrawToggle("有効", isEnabled, 60, 20, newValue =>
                {
                    timeline.videoEnabled = newValue;
                    if (newValue)
                    {
                        movieManager.LoadMovie();
                    }
                    else
                    {
                        movieManager.UnloadMovie();
                    }
                });
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

            view.DrawSliderValue(new GUIView.SliderOption
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

                    view.DrawSliderValue(new GUIView.SliderOption
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

                view.DrawSliderValue(new GUIView.SliderOption
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
                
                view.DrawSliderValue(new GUIView.SliderOption
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

                    view.DrawSliderValue(new GUIView.SliderOption
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

                    view.DrawSliderValue(new GUIView.SliderOption
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

                view.DrawSliderValue(new GUIView.SliderOption
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
                
                view.DrawSliderValue(new GUIView.SliderOption
                {
                    label = "透過度",
                    labelWidth = 60,
                    min = 0f,
                    max = 1f,
                    step = 0.01f,
                    defaultValue = 1f,
                    value = timeline.videoAlpha,
                    onChanged = value =>
                    {
                        timeline.videoAlpha = value;
                        movieManager.UpdateColor();
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

                    view.DrawSliderValue(new GUIView.SliderOption
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

                view.DrawSliderValue(new GUIView.SliderOption
                {
                    label = "表示サイズ",
                    labelWidth = 60,
                    min = 0f,
                    max = 2f,
                    step = 0.1f,
                    defaultValue = 1f,
                    value = timeline.videoBackmostScale,
                    onChanged = value =>
                    {
                        timeline.videoBackmostScale = value;
                        movieManager.UpdateTransform();
                    },
                });

                view.DrawSliderValue(new GUIView.SliderOption
                {
                    label = "透過度",
                    labelWidth = 60,
                    min = 0f,
                    max = 1f,
                    step = 0.01f,
                    defaultValue = 0.5f,
                    value = timeline.videoBackmostAlpha,
                    onChanged = value =>
                    {
                        timeline.videoBackmostAlpha = value;
                        movieManager.UpdateColor();
                    },
                });
            }

            if (timeline.videoDisplayType == VideoDisplayType.Frontmost)
            {
                var position = timeline.videoFrontmostPosition;
                var newPosition = position;
                for (var i = 0; i < 2; i++)
                {
                    var value = position[i];

                    view.DrawSliderValue(new GUIView.SliderOption
                    {
                        label = TransformDataBase.PositionNames[i],
                        labelWidth = 60,
                        min = -2f,
                        max = 2f,
                        step = 0.01f,
                        defaultValue = i == 0 ? -0.8f : 0.8f,
                        value = value,
                        onChanged = newValue => newPosition[i] = newValue,
                    });
                }

                if (newPosition != position)
                {
                    timeline.videoFrontmostPosition = newPosition;
                    movieManager.UpdateMesh();
                }

                view.DrawSliderValue(new GUIView.SliderOption
                {
                    label = "表示サイズ",
                    labelWidth = 60,
                    min = 0f,
                    max = 2f,
                    step = 0.1f,
                    defaultValue = 0.38f,
                    value = timeline.videoFrontmostScale,
                    onChanged = value =>
                    {
                        timeline.videoFrontmostScale = value;
                        movieManager.UpdateTransform();
                    },
                });

                view.DrawSliderValue(new GUIView.SliderOption
                {
                    label = "透過度",
                    labelWidth = 60,
                    min = 0f,
                    max = 1f,
                    step = 0.01f,
                    defaultValue = 1f,
                    value = timeline.videoFrontmostAlpha,
                    onChanged = value =>
                    {
                        timeline.videoFrontmostAlpha = value;
                        movieManager.UpdateColor();
                    },
                });
            }

            view.DrawSliderValue(new GUIView.SliderOption
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

            /*view.DrawTextField("シェーダー", config.videoShaderName, -1, 20, newText =>
            {
                config.videoShaderName = newText;
                movieManager.UpdateShader();
            });*/

            view.SetEnabled(true);
        }

        private void DrawGridSetting(GUIView view)
        {
            view.BeginHorizontal();
            {
                view.DrawLabel("グリッド設定", 100, 20);

                view.DrawToggle("有効", config.isGridVisible, 60, 20, newValue =>
                {
                    config.isGridVisible = newValue;
                    config.dirty = true;
                });
            }
            view.EndLayout();

            view.SetEnabled(config.isGridVisible);

            view.DrawToggle("画面上に表示", config.isGridVisibleInDisplay, 120, 20, newValue =>
            {
                config.isGridVisibleInDisplay = newValue;
                config.dirty = true;
            });

            view.DrawToggle("3D上に表示", config.isGridVisibleInWorld, 120, 20, newValue =>
            {
                config.isGridVisibleInWorld = newValue;
                config.dirty = true;
            });

            view.DrawToggle("動画上に表示", config.isGridVisibleInVideo, 120, 20, newValue =>
            {
                config.isGridVisibleInVideo = newValue;
                config.dirty = true;
            });

            view.DrawToggle("編集中のみ表示", config.isGridVisibleOnlyEdit, 120, 20, newValue =>
            {
                config.isGridVisibleOnlyEdit = newValue;
                config.dirty = true;
            });

            view.DrawSliderValue(new GUIView.SliderOption
            {
                label = "分割数",
                labelWidth = 60,
                fieldType = FloatFieldType.Int,
                min = 1,
                max = 20,
                step = 1,
                defaultValue = 4,
                value = config.gridCount,
                onChanged = value =>
                {
                    config.gridCount = (int) value;
                    config.dirty = true;
                    gridViewManager.UpdateGridLines();
                },
            });

            view.DrawSliderValue(new GUIView.SliderOption
            {
                label = "透過度",
                labelWidth = 60,
                min = 0f,
                max = 1f,
                step = 0.01f,
                defaultValue = 0.3f,
                value = config.gridAlpha,
                onChanged = value =>
                {
                    config.gridAlpha = value;
                    config.dirty = true;
                },
            });

            view.DrawSliderValue(new GUIView.SliderOption
            {
                label = "線幅",
                labelWidth = 60,
                min = 0f,
                max = 10f,
                step = 0.1f,
                defaultValue = 1f,
                value = config.gridLineWidth,
                onChanged = value =>
                {
                    config.gridLineWidth = value;
                    config.dirty = true;
                    gridViewManager.UpdateGridLines();
                },
            });

            view.DrawSliderValue(new GUIView.SliderOption
            {
                label = "3D分割数",
                labelWidth = 60,
                fieldType = FloatFieldType.Int,
                min = 1,
                max = 100,
                step = 1,
                defaultValue = 20,
                value = config.gridCountInWorld,
                onChanged = value =>
                {
                    config.gridCountInWorld = (int) value;
                    config.dirty = true;
                    gridViewManager.UpdateGridLines();
                },
            });

            view.DrawSliderValue(new GUIView.SliderOption
            {
                label = "3D透過度",
                labelWidth = 60,
                min = 0f,
                max = 1f,
                step = 0.01f,
                defaultValue = 0.3f,
                value = config.gridAlphaInWorld,
                onChanged = value =>
                {
                    config.gridAlphaInWorld = value;
                    config.dirty = true;
                },
            });

            view.DrawSliderValue(new GUIView.SliderOption
            {
                label = "3D線幅",
                labelWidth = 60,
                min = 0f,
                max = 10f,
                step = 0.1f,
                defaultValue = 1f,
                value = config.gridLineWidthInWorld,
                onChanged = value =>
                {
                    config.gridLineWidthInWorld = value;
                    config.dirty = true;
                    gridViewManager.UpdateGridLines();
                },
            });

            view.DrawSliderValue(new GUIView.SliderOption
            {
                label = "セルサイズ",
                labelWidth = 60,
                min = 0f,
                max = 10f,
                step = 0.01f,
                defaultValue = 0.5f,
                value = config.gridCellSize,
                onChanged = value =>
                {
                    config.gridCellSize = value;
                    config.dirty = true;
                },
            });
        }
    }
}