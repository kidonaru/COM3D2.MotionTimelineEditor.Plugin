using UnityEngine;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    public abstract class LightTimelineLayerBase : TimelineLayerBase
    {
        protected LightTimelineLayerBase(int slotNo) : base(slotNo)
        {
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

            view.BeginScrollView();

            foreach (var light in lights)
            {
                DrawLightContent(view, light, light.index);
            }

            view.SetEnabled(!view.IsComboBoxFocused());

            view.DrawHorizontalLine(Color.gray);

            view.DrawToggle("ライトのタンジェント補間を有効化", timeline.isTangentLight, -1, 20, newValue =>
            {
                timeline.isTangentLight = newValue;
                InitTangent();
                ApplyCurrentFrame(true);
            });

            view.DrawToggle("ライトで色補間を有効化", timeline.isLightColorEasing, -1, 20, newValue =>
            {
                timeline.isLightColorEasing = newValue;
            });

            view.DrawToggle("ライトで拡張補間を有効化", timeline.isLightExtraEasing, -1, 20, newValue =>
            {
                timeline.isLightExtraEasing = newValue;
            });

            view.DrawToggle("ライトの互換性モードを有効化", timeline.isLightCompatibilityMode, -1, 20, newValue =>
            {
                timeline.isLightCompatibilityMode = newValue;
                studioHack.SetLightCompatibilityMode(newValue);
            });

            view.EndScrollView();
        }

        protected void DrawLightContent(
            GUIView view,
            StudioLightStat light,
            int lightIndex)
        {
            if (light == null || lightIndex < 0)
            {
                return;
            }

            view.BeginHorizontal();
            {
                view.SetEnabled(!view.IsComboBoxFocused() && studioHackManager.isPoseEditing);

                view.DrawToggle(light.displayName, light.visible, 200, 20, newValue =>
                {
                    light.visible = newValue;
                    lightManager.ApplyLight(light);
                });

                view.SetEnabled(!view.IsComboBoxFocused() && light.index > 0 && lightManager.CanCreateLight());

                if (view.DrawButton("削除", 45, 20))
                {
                    MTEUtils.EnqueueAction(() =>
                    {
                        lightManager.DeleteLight(light);
                    });
                }
            }
            view.EndLayout();
        }
    }
}