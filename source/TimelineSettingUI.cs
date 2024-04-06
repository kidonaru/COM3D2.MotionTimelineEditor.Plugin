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

        private Texture2D texWhite = null;

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
            if (texWhite == null)
            {
                texWhite = GUIView.CreateColorTexture(Color.white);
            }
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

                view.DrawLabel("個別設定", 200, 20);

                view.DrawTexture(texWhite, -1, 1, Color.gray);

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
                    timelineManager.ApplyCurrentFrame(true);
                }

                view.BeginLayout(GUIView.LayoutDirection.Horizontal);
                {
                    view.DrawLabel("Tangent範囲", 100, 20);

                    var newMinTangent = view.DrawFloatField(timeline.minTangent, 50, 20);

                    view.DrawLabel("～", 15, 20);

                    var newMaxTangent = view.DrawFloatField(timeline.maxTangent, 50, 20);

                    if (newMinTangent != timeline.minTangent)
                    {
                        timeline.minTangent = newMinTangent;
                        timelineManager.ApplyCurrentFrame(true);
                    }

                    if (newMaxTangent != timeline.maxTangent)
                    {
                        timeline.maxTangent = newMaxTangent;
                        timelineManager.ApplyCurrentFrame(true);
                    }
                }
                view.EndLayout();

                if (view.DrawButton("初期化", 100, 20))
                {
                    Extensions.ShowConfirmDialog("個別設定を初期化しますか？", () =>
                    {
                        GameMain.Instance.SysDlg.Close();
                        timeline.ResetSettings();
                        timelineManager.Refresh();
                        timelineManager.ApplyCurrentFrame(true);
                    }, null);
                }

                view.AddSpace(10);

                view.DrawLabel("共通設定", 200, 20);

                view.DrawTexture(texWhite, -1, 1, Color.gray);

                var newDefaultTangentType = (TangentType) view.DrawSelectList("初期補正曲線", TangentData.TangentTypeNames, 250, 20, (int) config.defaultTangentType);
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

                if (view.DrawButton("初期化", 100, 20))
                {
                    Extensions.ShowConfirmDialog("共通設定を初期化しますか？", () =>
                    {
                        GameMain.Instance.SysDlg.Close();
                        MTE.ResetConfig();
                        timelineManager.Refresh();
                    }, null);
                }
            }

            GUI.DragWindow();
        }
    }
}