using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    using SH = StudioHack;
    using MTE = MotionTimelineEditor;

    public class TimelineSettingUI : ISubWindowUI
    {
        public string title
        {
            get
            {
                return "タイムライン 設定";
            }
        }

        public static int WINDOW_WIDTH
        {
            get
            {
                return SubWindow.WINDOW_WIDTH;
            }
        }
        public static int WINDOW_HEIGHT
        {
            get
            {
                return SubWindow.WINDOW_HEIGHT;
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

        public static Config config
        {
            get
            {
                return MTE.config;
            }
        }

        public void OnOpen()
        {
        }

        public void OnClose()
        {
        }

        public void Update()
        {
        }

        public void DrawWindow(int id)
        {
            {
                var view = new GUIView(0, 20, WINDOW_WIDTH, WINDOW_HEIGHT - 20);

                view.BeginLayout(GUIView.LayoutDirection.Horizontal);
                {
                    var newFrameRate = view.DrawFloatField("フレームレート", timeline.frameRate, 150, 20);

                    if (view.DrawButton("24", 30, 20))
                    {
                        newFrameRate = 24;
                    }

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
                }

                var newDefaultTangentType = (TangentType) view.DrawSelectList("初期補正曲線", TangentData.TangentTypeNames, 250, 20, (int) config.defaultTangentType);
                if (newDefaultTangentType != config.defaultTangentType)
                {
                    config.defaultTangentType = newDefaultTangentType;
                    config.dirty = true;
                }

                var newIsAutoScroll = view.DrawToggle("自動スクロール", config.isAutoScroll, 100, 20);
                if (newIsAutoScroll != config.isAutoScroll)
                {
                    config.isAutoScroll = newIsAutoScroll;
                    config.dirty = true;
                }

                view.BeginLayout(GUIView.LayoutDirection.Horizontal);
                {
                    view.DrawLabel("背景透過度", 100, 20);

                    var newTimelineBgAlpha = view.DrawFloatField(config.timelineBgAlpha, 50, 20);

                    newTimelineBgAlpha = view.DrawSlider(newTimelineBgAlpha, 0f, 1.0f, 100, 20);

                    if (newTimelineBgAlpha != config.timelineBgAlpha)
                    {
                        config.timelineBgAlpha = newTimelineBgAlpha;
                        config.dirty = true;
                    }
                }
                view.EndLayout();

                view.AddSpace(10);

                if (view.DrawButton("設定初期化", 100, 20))
                {
                    Extensions.ShowConfirmDialog("設定を初期化しますか？", () =>
                    {
                        GameMain.Instance.SysDlg.Close();
                        MTE.ResetConfig();
                    }, null);
                }
            }

            GUI.DragWindow();
        }
    }
}