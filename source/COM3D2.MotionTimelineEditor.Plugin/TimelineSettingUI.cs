using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    using MTE = MotionTimelineEditor;

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

        public override void DrawWindow(int id)
        {
            {
                var view = new GUIView(0, 20, WINDOW_WIDTH, WINDOW_HEIGHT - 20);

                view.DrawLabel("個別設定", 200, 20);

                view.DrawHorizontalLine(Color.gray);

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

                var newEyeMoveType = (Maid.EyeMoveType) view.DrawSelectList(
                    "メイド目線",
                    EyeMoveTypeNames,
                    (name, index) => name,
                    250,
                    20,
                    (int) timeline.eyeMoveType);
                if (newEyeMoveType != timeline.eyeMoveType)
                {
                    timeline.eyeMoveType = newEyeMoveType;
                    maidManager.maid.EyeToCamera(newEyeMoveType, 0f);
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

                view.AddSpace(10);

                view.DrawLabel("共通設定", 200, 20);

                view.DrawHorizontalLine(Color.gray);

                var newDefaultTangentType = (TangentType) view.DrawSelectList(
                    "初期補正曲線",
                    TangentData.TangentTypeNames,
                    (name, index) => name,
                    250,
                    20,
                    (int) config.defaultTangentType);
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
                    PluginUtils.ShowConfirmDialog("共通設定を初期化しますか？", () =>
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