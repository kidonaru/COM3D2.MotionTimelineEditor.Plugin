using UnityEngine;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    public class TimelineLayerUI : SubWindowUIBase
    {
        public override string title
        {
            get
            {
                var layerInfo = timelineManager.GetLayerInfo(currentLayer.className);
                return layerInfo.displayName + "情報";
            }
        }

        public override void DrawWindow(int id)
        {
            var view = new GUIView(0, 20, WINDOW_WIDTH, WINDOW_HEIGHT - 20);
            currentLayer.DrawWindow(view);

            if (!currentLayer.isDragging)
            {
                GUI.DragWindow();
            }
        }
    }
}