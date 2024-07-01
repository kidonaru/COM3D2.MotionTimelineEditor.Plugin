using COM3D2.DanceCameraMotion.Plugin;
using COM3D2.MotionTimelineEditor.Plugin;
using UnityEngine;

namespace COM3D2.MotionTimelineEditor_DCM.Plugin
{
    public abstract class LightTimelineLayerBase : TimelineLayerBase
    {
        protected LightTimelineLayerBase(int slotNo) : base(slotNo)
        {
        }

        public override float CalcEasingValue(float t, int easing)
        {
            return TimelineMotionEasing.MotionEasing(t, (EasingType) easing);
        }

        protected void DrawLightManage(GUIView view)
        {
            if (timeline == null)
            {
                return;
            }

            var lights = lightManager.lights;
            if (lights.Count == 0)
            {
                view.DrawLabel("ライトがありません", -1, 20);
                return;
            }

            view.DrawHorizontalLine(Color.gray);

            view.AddSpace(5);

            view.padding = Vector2.zero;

            view.DrawContentListView(
                lights,
                DrawLightContent,
                -1,
                -1,
                80);
        }

        protected void DrawLightContent(
            GUIView view,
            StudioLightStat light,
            int lightIndex)
        {
            if (light == null)
            {
                return;
            }
            if (lightIndex < 0)
            {
                return;
            }

            var width = view.viewRect.width;
            var height = view.viewRect.height;

            view.currentPos.x = 5;
            view.currentPos.y = 5;

            view.BeginHorizontal();
            {
                view.DrawToggle(light.visible, 20, 20, newValue =>
                {
                    light.visible = newValue;
                    lightManager.ApplyLight(light);
                });

                view.DrawLabel(light.displayName, -1, 20);
            }
            view.EndLayout();

            view.BeginHorizontal();
            {
                if (view.DrawButton("複製", 45, 20))
                {
                    //timelineManager.CopyLight(light);
                }

                if (view.DrawButton("削除", 45, 20))
                {
                    lightManager.DeleteLight(light);
                }
            }
            view.EndLayout();

            view.DrawHorizontalLine(Color.gray);
        }
    }
}