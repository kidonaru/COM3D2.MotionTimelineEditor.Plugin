using System.IO;
using UnityEngine;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    public class TimelineTrackUI : SubWindowUIBase
    {
        public override string title
        {
            get
            {
                return "トラック設定";
            }
        }

        private Vector2 scrollPosition = Vector2.zero;

        public override void OnOpen()
        {
        }

        public override void DrawWindow(int id)
        {
            {
                var view = new GUIView(0, 0, WINDOW_WIDTH, 20);
                view.padding = Vector2.zero;
            }

            {
                var view = new GUIView(0, 20, WINDOW_WIDTH, WINDOW_HEIGHT - 20);

                view.BeginLayout(GUIView.LayoutDirection.Horizontal);
                {
                    if (view.DrawButton("追加", 80, 20))
                    {
                        timelineManager.AddTrack();
                    }
                }
                view.EndLayout();

                view.AddSpace(10);

                var tracks = timeline.tracks;
                if (tracks.Count == 0)
                {
                    view.DrawLabel("トラックがありません", -1, 20);
                }
                else
                {
                    view.padding = Vector2.zero;
                    var currentIndex = timeline.activeTrackIndex;

                    view.DrawContentListView(
                        tracks,
                        DrawTrack,
                        -1,
                        -1,
                        ref scrollPosition,
                        55);
                }
            }

            GUI.DragWindow();
        }

        public void DrawTrack(
            GUIView view,
            TrackData track,
            int index)
        {
            if (track == null)
            {
                return;
            }

            var width = view.viewRect.width;
            var height = view.viewRect.height;

            view.currentPos.x = 5;
            view.currentPos.y = 5;

            view.BeginLayout(GUIView.LayoutDirection.Horizontal);
            {
                bool isActive = timeline.activeTrack == track;

                var newIsActive = view.DrawToggle("", isActive, 20, 20);
                if (newIsActive != isActive)
                {
                    timelineManager.SetActiveTrack(track, !isActive);
                }

                track.name = view.DrawTextField(track.name, width - 30 - view.currentPos.x, 20);
            }
            view.EndLayout();

            view.BeginLayout(GUIView.LayoutDirection.Horizontal);
            {
                view.DrawLabel("範囲", 40, 20);

                var newStartFrameNo = view.DrawIntField(track.startFrameNo, 50, 20);

                view.DrawLabel("～", 15, 20);

                var newEndFrameNo = view.DrawIntField(track.endFrameNo, 50, 20);

                if (view.DrawButton("削除", 50, 20))
                {
                    timelineManager.RemoveTrack(track);
                }

                if (newStartFrameNo != track.startFrameNo || newEndFrameNo != track.endFrameNo)
                {
                    track.startFrameNo = newStartFrameNo;
                    track.endFrameNo = newEndFrameNo;

                    if (track == timeline.activeTrack)
                    {
                        timelineManager.ApplyCurrentFrame(true);
                    }
                }
            }
            view.EndLayout();

            view.DrawHorizontalLine(Color.gray);

            view.BeginLayout(GUIView.LayoutDirection.Free);
            {
                view.currentPos.x = width - 30;
                view.currentPos.y = 5;

                if (view.DrawButton("∧", 20, 20))
                {
                    timelineManager.MoveUpTrack(track);
                }

                view.currentPos.y += 25;
                if (view.DrawButton("∨", 20, 20))
                {
                    timelineManager.MoveDownTrack(track);
                }
            }
        }
    }
}