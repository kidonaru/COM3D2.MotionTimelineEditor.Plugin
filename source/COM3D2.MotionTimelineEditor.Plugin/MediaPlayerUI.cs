using System.Collections.Generic;
using System.Windows.Forms;
using UnityEngine;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    public class MediaPlayerUI : SubWindowUIBase
    {
        public override string title
        {
            get
            {
                return "メディア設定";
            }
        }

        private FloatFieldValue[] positionFieldValues = FloatFieldValue.CreateArray(
            new string[] { "X", "Y", "Z" }
        );
        private FloatFieldValue[] rotationFieldValues = FloatFieldValue.CreateArray(
            new string[] { "RX", "RY", "RZ" }
        );
        private FloatFieldValue[] guiPositionFieldValues = FloatFieldValue.CreateArray(
            new string[] { "X", "Y" }
        );
        private FloatFieldValue[] backmostPositionFieldValues = FloatFieldValue.CreateArray(
            new string[] { "X", "Y" }
        );
        private FloatFieldValue startTimeFieldValue = new FloatFieldValue("開始位置");

        private static MovieManager movieManager
        {
            get
            {
                return MovieManager.instance;
            }
        }

        private static BGMManager bgmManager
        {
            get
            {
                return BGMManager.instance;
            }
        }

        public MediaPlayerUI(SubWindow subWindow) : base(subWindow)
        {
        }

        public override void DrawContent(GUIView view)
        {
            if (timeline == null)
            {
                return;
            }

            DrawAudio(view);
            DrawVideo(view);
        }

        private void DrawAudio(GUIView view)
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

        private void DrawVideo(GUIView view)
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
                var fieldValue = startTimeFieldValue;

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
                for (var i = 0; i < guiPositionFieldValues.Length; i++)
                {
                    var value = guiPosition[i];
                    var fieldValue = guiPositionFieldValues[i];

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
                for (var i = 0; i < positionFieldValues.Length; i++)
                {
                    var value = position[i];
                    var fieldValue = positionFieldValues[i];

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
                for (var i = 0; i < rotationFieldValues.Length; i++)
                {
                    var value = rotation[i];
                    var fieldValue = rotationFieldValues[i];

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
                for (var i = 0; i < backmostPositionFieldValues.Length; i++)
                {
                    var value = position[i];
                    var fieldValue = backmostPositionFieldValues[i];

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