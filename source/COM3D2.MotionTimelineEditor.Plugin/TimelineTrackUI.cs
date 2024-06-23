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

        public TimelineTrackUI(SubWindow subWindow) : base(subWindow)
        {
        }

        public override void DrawContent(GUIView view)
        {
            if (timeline == null)
            {
                return;
            }

            view.BeginHorizontal();
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
                    55);
            }
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

            view.BeginHorizontal();
            {
                bool isActive = timeline.activeTrack == track;

                var newIsActive = view.DrawToggle("", isActive, 20, 20);
                if (newIsActive != isActive)
                {
                    timelineManager.SetActiveTrack(track, !isActive);
                }

                view.DrawTextField(track.name, width - 30 - view.currentPos.x, 20, newText => track.name = newText);
            }
            view.EndLayout();

            view.BeginHorizontal();
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