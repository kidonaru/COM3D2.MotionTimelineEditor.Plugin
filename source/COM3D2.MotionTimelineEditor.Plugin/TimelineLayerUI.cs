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

        public TimelineLayerUI(SubWindow subWindow) : base(subWindow)
        {
        }

        public override void DrawContent(GUIView view)
        {
            if (timeline == null || currentLayer == null)
            {
                return;
            }

            currentLayer.ResetDraw(view);
            currentLayer.DrawWindow(view);
        }

        public override bool IsDragging()
        {
            return currentLayer.isDragging;
        }
    }
}