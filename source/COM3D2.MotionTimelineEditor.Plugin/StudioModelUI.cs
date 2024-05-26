using UnityEngine;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    public class StudioModelUI : SubWindowUIBase
    {
        public override string title
        {
            get
            {
                return "モデル設定";
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

                var models = modelManager.models;
                if (models.Count == 0)
                {
                    view.DrawLabel("モデルがありません", -1, 20);
                }
                else
                {
                    view.padding = Vector2.zero;
                    var currentIndex = timeline.activeTrackIndex;

                    view.DrawContentListView(
                        models,
                        DrawModel,
                        -1,
                        -1,
                        ref scrollPosition,
                        55);
                }
            }

            GUI.DragWindow();
        }

        public void DrawModel(
            GUIView view,
            StudioModelStat model)
        {
            if (model == null)
            {
                return;
            }

            var width = view.viewRect.width;
            var height = view.viewRect.height;

            view.currentPos.x = 5;
            view.currentPos.y = 5;

            view.BeginLayout(GUIView.LayoutDirection.Horizontal);
            {
                view.DrawLabel(model.displayName, width - 30 - view.currentPos.x, 20);
            }
            view.EndLayout();

            view.BeginLayout(GUIView.LayoutDirection.Horizontal);
            {
                if (view.DrawButton("複製", 50, 20))
                {
                    timelineManager.CopyModel(model);
                }

                if (view.DrawButton("削除", 50, 20))
                {
                    timelineManager.DeleteModel(model);
                }
            }
            view.EndLayout();

            view.DrawHorizontalLine(Color.gray);
        }
    }
}