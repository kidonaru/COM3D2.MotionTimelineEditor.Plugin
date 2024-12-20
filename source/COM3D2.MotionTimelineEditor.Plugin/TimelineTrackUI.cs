using System.IO;
using UnityEngine;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    public class TimelineTrackUI : SubWindowUIBase
    {
        public override string title => "トラック設定";

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

                view.DrawToggle("", isActive, 20, 20, newValue =>
                {
                    timelineManager.SetActiveTrack(track, !isActive);
                });

                view.DrawTextField(track.name, width - 30 - view.currentPos.x, 20, newText =>
                {
                    track.name = newText;
                });
            }
            view.EndLayout();

            view.BeginHorizontal();
            {
                view.DrawLabel("範囲", 40, 20);

                var updated = false;
                updated |= view.DrawIntField(new GUIView.IntFieldOption
                {
                    value = track.startFrameNo,
                    width = 50,
                    height = 20,
                    onChanged = x => track.startFrameNo = x
                });

                view.DrawLabel("～", 15, 20);

                updated |= view.DrawIntField(new GUIView.IntFieldOption
                {
                    value = track.endFrameNo,
                    width = 50,
                    height = 20,
                    onChanged = x => track.endFrameNo = x
                });

                if (view.DrawButton("削除", 50, 20))
                {
                    timelineManager.RemoveTrack(track);
                }

                if (updated && track == timeline.activeTrack)
                {
                    timelineManager.ApplyCurrentFrame(true);
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