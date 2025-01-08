using UnityEngine;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    public class TimelineLayerUI : SubWindowUIBase
    {
        public override string title
        {
            get
            {
                if (currentLayer == null)
                {
                    return "レイヤー情報";
                }

                var layerInfo = timelineManager.GetLayerInfo(currentLayer.layerType);
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
            if (currentLayer == null)
            {
                return false;
            }

            return currentLayer.isDragging;
        }
    }
}