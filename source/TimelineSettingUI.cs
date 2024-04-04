using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    using SH = StudioHack;
    using MTE = MotionTimelineEditor;

    public struct TangentPair
    {
        public float outTangent;
        public float inTangent;

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            var other = (TangentPair)obj;
            return outTangent == other.outTangent && inTangent == other.inTangent;
        }

        public override int GetHashCode()
        {
            return outTangent.GetHashCode() ^ inTangent.GetHashCode();
        }

        public static TangentPair GetDefault(TangentType tangentType)
        {
            switch (tangentType)
            {
                case TangentType.EaseInOut:
                    return new TangentPair
                    {
                        outTangent = 0f,
                        inTangent = 0f,
                    };
                case TangentType.EaseIn:
                    return new TangentPair
                    {
                        outTangent = 1f,
                        inTangent = 0f,
                    };
                case TangentType.EaseOut:
                    return new TangentPair
                    {
                        outTangent = 0f,
                        inTangent = 1f,
                    };
                case TangentType.Linear:
                    return new TangentPair
                    {
                        outTangent = 1f,
                        inTangent = 1f,
                    };
            }

            return new TangentPair();
        }
    }

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
            if (tangentTextures == null)
            {
                tangentTextures = new Texture2D[(int) TangentType.Max];

                for (var i = 0; i < tangentTextures.Length; i++)
                {
                    var tangentType = (TangentType) i;
                    var texture = new Texture2D(40, 40);
                    var tangentPair = TangentPair.GetDefault(tangentType);

                    tangentTextures[i] = texture;
                    TimelineData.ClearTexture(texture, config.curveBgColor);

                    TimelineData.UpdateCurveTexture(
                        texture,
                        tangentPair.outTangent,
                        tangentPair.inTangent,
                        config.curveLineColor,
                        3
                    );
                }
            }

            if (curveTex == null)
            {
                curveTex = new Texture2D(150, 150);

                TimelineData.ClearTexture(curveTex, config.curveBgColor);
            }
        }

        public void OnClose()
        {
        }

        public void Update()
        {
        }

        private Texture2D curveTex = null;
        private TangentValueType tangentValueType = TangentValueType.All;
        private HashSet<TangentPair> cachedTangents = new HashSet<TangentPair>();
        private Texture2D[] tangentTextures = null;

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

                var newDefaultTangentType = (TangentType) view.DrawSelectList("初期補正曲線", TangentData.tangentTypeNames, 250, 20, (int) config.defaultTangentType);
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

                view.AddSpace(10);

                var tangents = new HashSet<TangentPair>();
                var selectedBones = timelineManager.selectedBones;
                if (selectedBones.Count > 0)
                {
                    foreach (var bone in selectedBones)
                    {
                        var prevBone = timeline.GetPrevBone(bone.frameNo, bone.bonePath);
                        var outTangentDataList = prevBone.transform.GetOutTangentDataList(tangentValueType);
                        var inTangentDataList = bone.transform.GetInTangentDataList(tangentValueType);

                        for (var i = 0; i < outTangentDataList.Length && i < inTangentDataList.Length; i++)
                        {
                            var outTangentData = outTangentDataList[i];
                            var inTangentData = inTangentDataList[i];

                            tangents.Add(new TangentPair
                            {
                                outTangent = outTangentData.normalizedValue,
                                inTangent = inTangentData.normalizedValue
                            });
                        }
                    }
                }

                bool isUpdated = false;

                if (cachedTangents.Count != tangents.Count)
                {
                    isUpdated = true;
                }
                else
                {
                    foreach (var tangent in cachedTangents)
                    {
                        if (!tangents.Contains(tangent))
                        {
                            isUpdated = true;
                            break;
                        }
                    }
                }

                if (isUpdated)
                {
                    cachedTangents = tangents;

                    TimelineData.ClearTexture(curveTex, config.curveBgColor);

                    foreach (var tangent in tangents)
                    {
                        TimelineData.UpdateCurveTexture(
                            curveTex,
                            tangent.outTangent,
                            tangent.inTangent,
                            config.curveLineColor,
                            1
                        );
                    }
                }

                view.DrawLabel("補完曲線", 100, 20);

                view.BeginLayout(GUIView.LayoutDirection.Free);

                view.DrawTexture(curveTex);

                float outTangent = float.MinValue;
                float inTangent = float.MinValue;

                if (tangents.Count > 0)
                {
                    var tangentPair = tangents.First();
                    outTangent = tangentPair.outTangent;
                    inTangent = tangentPair.inTangent;

                    view.currentPos.x += curveTex.width + 10;

                    view.DrawLabel("対象", 100, 20);

                    view.currentPos.y += 20;

                    tangentValueType = (TangentValueType) view.DrawSelectList(TangentData.tangentValueTypeNames, 100, 20, (int) tangentValueType);

                    view.currentPos.y += 25;

                    view.DrawLabel("OutTangent", 100, 20);

                    view.currentPos.y += 20; 

                    var newOutTangent = view.DrawFloatField(
                        outTangent,
                        100, 20);
                    if (!Mathf.Approximately(newOutTangent, outTangent))
                    {
                        foreach (var selectedBone in selectedBones)
                        {
                            var prevBone = timeline.GetPrevBone(selectedBone.frameNo, selectedBone.bonePath);
                            var outTangentDataList = prevBone.transform.GetOutTangentDataList(tangentValueType);

                            foreach (var outTangentData in outTangentDataList)
                            {
                                outTangentData.normalizedValue = newOutTangent;
                            }
                        }
                        timelineManager.ApplyCurrentFrame(true);
                    }

                    view.currentPos.y += 25;

                    view.DrawLabel("InTangent", 100, 20);

                    view.currentPos.y += 20;

                    var newInTangent = view.DrawFloatField(
                        inTangent,
                        100, 20);
                    if (!Mathf.Approximately(newInTangent, inTangent))
                    {
                        foreach (var selectedBone in selectedBones)
                        {
                            var inTangentDataList = selectedBone.transform.GetInTangentDataList(tangentValueType);

                            foreach (var inTangentData in inTangentDataList)
                            {
                                inTangentData.normalizedValue = newInTangent;
                            }
                        }
                        timelineManager.ApplyCurrentFrame(true);
                    }
                }

                view.EndLayout();

                view.currentPos.y += curveTex.height + view.margin;

                view.BeginLayout(GUIView.LayoutDirection.Horizontal);

                for (int i = 0; i < tangentTextures.Length; ++i)
                {
                    var texture = tangentTextures[i];
                    var tangentType = (TangentType) i;

                    view.DrawTexture(texture, texture.width, texture.height, Color.white, () =>
                    {
                        if (selectedBones.Count == 0)
                        {
                            return;
                        }

                        var tangentPair = TangentPair.GetDefault(tangentType);

                        foreach (var selectedBone in selectedBones)
                        {
                            var prevBone = timeline.GetPrevBone(selectedBone.frameNo, selectedBone.bonePath);
                            var outTangentDataList = prevBone.transform.GetOutTangentDataList(tangentValueType);
                            var inTangentDataList = selectedBone.transform.GetInTangentDataList(tangentValueType);

                            for (var j = 0; j < outTangentDataList.Length && j < inTangentDataList.Length; j++)
                            {
                                var outTangentData = outTangentDataList[j];
                                var inTangentData = inTangentDataList[j];

                                outTangentData.normalizedValue = tangentPair.outTangent;
                                inTangentData.normalizedValue = tangentPair.inTangent;
                            }
                        }

                        timelineManager.ApplyCurrentFrame(true);
                    });
                }

                view.EndLayout();
            }

            GUI.DragWindow();
        }
    }
}