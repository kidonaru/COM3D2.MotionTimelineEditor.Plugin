using System.Linq;
using System.Windows.Forms;
using UnityEngine;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    public class MoviePlayerUI : SubWindowUIBase
    {
        public override string title
        {
            get
            {
                return "動画再生";
            }
        }

        private FloatFieldValue[] positionFieldValues = FloatFieldValue.CreateArray(3);
        private FloatFieldValue[] rotationFieldValues = FloatFieldValue.CreateArray(3);
        private FloatFieldValue[] guiPositionFieldValues = FloatFieldValue.CreateArray(2);
        private FloatFieldValue startTimeFieldValue = new FloatFieldValue();

        private static MoviePlayer moviePlayer
        {
            get
            {
                return MoviePlayer.instance;
            }
        }

        public override void OnOpen()
        {
            moviePlayer.LoadMovie();
        }

        public override void Update()
        {
            moviePlayer.Update();
        }

        public override void DrawWindow(int id)
        {
            {
                var view = new GUIView(0, 20, WINDOW_WIDTH, WINDOW_HEIGHT - 20);

                var isEnabled = timeline.videoEnabled;

                view.BeginLayout(GUIView.LayoutDirection.Horizontal);
                {
                    isEnabled = view.DrawToggle("有効", isEnabled, 100, 20);
                    if (isEnabled != timeline.videoEnabled)
                    {
                        timeline.videoEnabled = isEnabled;
                        if (isEnabled)
                        {
                            moviePlayer.LoadMovie();
                        }
                        else
                        {
                            moviePlayer.UnloadMovie();
                        }
                    }

                    var newDisplayOnUI = view.DrawToggle("GUI表示", timeline.videoDisplayOnGUI, 100, 20);
                    if (newDisplayOnUI != timeline.videoDisplayOnGUI)
                    {
                        timeline.videoDisplayOnGUI = newDisplayOnUI;
                        moviePlayer.ReloadMovie();
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
                            moviePlayer.LoadMovie();
                        }
                    }

                    if (view.DrawButton("再読込", 80, 20))
                    {
                        moviePlayer.ReloadMovie();
                    }
                }
                view.EndLayout();

                timeline.videoPath = view.DrawTextField(timeline.videoPath, 240, 20);

                view.BeginLayout(GUIView.LayoutDirection.Horizontal);
                {
                    var newValue = timeline.videoStartTime;

                    var fieldValue = startTimeFieldValue;
                    fieldValue.UpdateValue(newValue, true);

                    view.BeginLayout(GUIView.LayoutDirection.Horizontal);

                    view.DrawLabel("開始位置", 50, 20);

                    var diffValue = 0f;

                    if (view.DrawButton("<<", 25, 20))
                    {
                        diffValue = -1f;
                    }
                    if (view.DrawButton("<", 20, 20))
                    {
                        diffValue = -1f / moviePlayer.frameRate;
                    }

                    newValue = view.DrawFloatFieldValue(fieldValue, 50, 20);

                    if (view.DrawButton(">", 20, 20))
                    {
                        diffValue = 1f / moviePlayer.frameRate;
                    }
                    if (view.DrawButton(">>", 25, 20))
                    {
                        diffValue = 1f;
                    }
                    if (view.DrawButton("0", 20, 20))
                    {
                        newValue = 0f;
                    }

                    newValue += diffValue;

                    view.EndLayout();

                    if (newValue != timeline.videoStartTime)
                    {
                        timeline.videoStartTime = newValue;
                        moviePlayer.UpdateSeekTime();
                    }
                }
                view.EndLayout();

                if (timeline.videoDisplayOnGUI)
                {
                    var guiPosition = timeline.videoGUIPosition;
                    var newGUIPosition = guiPosition;
                    for (var i = 0; i < guiPositionFieldValues.Length; i++)
                    {
                        var value = guiPosition[i];
                        var fieldValue = guiPositionFieldValues[i];
                        fieldValue.UpdateValue(value, true);

                        view.BeginLayout(GUIView.LayoutDirection.Horizontal);

                        var label = KeyFrameUI.TransValueLabels[i];
                        view.DrawLabel(label, 50, 20);

                        var diffValue = 0f;

                        if (view.DrawButton("<<", 25, 20))
                        {
                            diffValue = -0.1f;
                        }
                        if (view.DrawButton("<", 20, 20))
                        {
                            diffValue = -0.01f;
                        }

                        var newValue = view.DrawFloatFieldValue(fieldValue, 50, 20);

                        if (view.DrawButton(">", 20, 20))
                        {
                            diffValue = 0.01f;
                        }
                        if (view.DrawButton(">>", 25, 20))
                        {
                            diffValue = 0.1f;
                        }
                        if (view.DrawButton("0", 20, 20))
                        {
                            newValue = 0f;
                        }

                        newGUIPosition[i] = newValue + diffValue;

                        view.EndLayout();
                    }

                    if (newGUIPosition != guiPosition)
                    {
                        timeline.videoGUIPosition = newGUIPosition;
                        moviePlayer.UpdateTransform();
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
                            moviePlayer.UpdateTransform();
                        }
                    }
                    view.EndLayout();

                    view.BeginLayout(GUIView.LayoutDirection.Horizontal);
                    {
                        view.DrawLabel("透過度", 60, 20);

                        var newAlpha = view.DrawFloatField(timeline.videoGUIAlpha, 50, 20);

                        if (view.DrawButton("R", 20, 20))
                        {
                            newAlpha = 0.5f;
                        }

                        newAlpha = view.DrawSlider(newAlpha, 0f, 1.0f, 100, 20);

                        if (newAlpha != timeline.videoGUIAlpha)
                        {
                            timeline.videoGUIAlpha = newAlpha;
                            moviePlayer.UpdateColor();
                        }
                    }
                    view.EndLayout();
                }
                else
                {
                    var position = timeline.videoPosition;
                    var newPosition = position;
                    for (var i = 0; i < positionFieldValues.Length; i++)
                    {
                        var value = position[i];
                        var fieldValue = positionFieldValues[i];
                        fieldValue.UpdateValue(value, true);

                        view.BeginLayout(GUIView.LayoutDirection.Horizontal);

                        var label = KeyFrameUI.TransValueLabels[i];
                        view.DrawLabel(label, 50, 20);

                        var diffValue = 0f;

                        if (view.DrawButton("<<", 25, 20))
                        {
                            diffValue = -0.1f;
                        }
                        if (view.DrawButton("<", 20, 20))
                        {
                            diffValue = -0.01f;
                        }

                        var newValue = view.DrawFloatFieldValue(fieldValue, 50, 20);

                        if (view.DrawButton(">", 20, 20))
                        {
                            diffValue = 0.01f;
                        }
                        if (view.DrawButton(">>", 25, 20))
                        {
                            diffValue = 0.1f;
                        }
                        if (view.DrawButton("0", 20, 20))
                        {
                            newValue = 0f;
                        }

                        newPosition[i] = newValue + diffValue;

                        view.EndLayout();
                    }

                    if (newPosition != position)
                    {
                        timeline.videoPosition = newPosition;
                        moviePlayer.UpdateTransform();
                    }

                    var rotation = timeline.videoRotation;
                    var newRotation = rotation;
                    for (var i = 0; i < rotationFieldValues.Length; i++)
                    {
                        var value = rotation[i];
                        var fieldValue = rotationFieldValues[i];
                        fieldValue.UpdateValue(value, true);

                        view.BeginLayout(GUIView.LayoutDirection.Horizontal);

                        var label = KeyFrameUI.TransValueLabels[i + 3];
                        view.DrawLabel(label, 50, 20);

                        var diffValue = 0f;

                        if (view.DrawButton("<<", 25, 20))
                        {
                            diffValue = -10f;
                        }
                        if (view.DrawButton("<", 20, 20))
                        {
                            diffValue = -1f;
                        }

                        var newValue = view.DrawFloatFieldValue(fieldValue, 50, 20);

                        if (view.DrawButton(">", 20, 20))
                        {
                            diffValue = 1f;
                        }
                        if (view.DrawButton(">>", 25, 20))
                        {
                            diffValue = 10f;
                        }
                        if (view.DrawButton("0", 20, 20))
                        {
                            newValue = 0f;
                        }

                        newRotation[i] = newValue + diffValue;

                        view.EndLayout();
                    }

                    if (newRotation != rotation)
                    {
                        timeline.videoRotation = newRotation;
                        moviePlayer.UpdateTransform();
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
                            moviePlayer.UpdateTransform();
                        }
                    }
                    view.EndLayout();
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
                        moviePlayer.UpdateVolume();
                    }
                }
                view.EndLayout();

                GUI.enabled = true;
            }

            GUI.DragWindow();
        }
    }
}