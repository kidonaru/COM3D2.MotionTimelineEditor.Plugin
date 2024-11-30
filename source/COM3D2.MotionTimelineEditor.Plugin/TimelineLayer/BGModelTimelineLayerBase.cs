using UnityEngine;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    public abstract class BGModelTimelineLayerBase : TimelineLayerBase
    {
        protected BGModelTimelineLayerBase(int slotNo) : base(slotNo)
        {
        }

        protected void DrawModelManage(GUIView view)
        {
            if (timeline == null)
            {
                return;
            }

            var infoMap = bgModelManager.modelInfoMap;
            if (infoMap.Count == 0)
            {
                view.DrawLabel("背景モデルがありません", -1, 20);
                return;
            }

            view.DrawHorizontalLine(Color.gray);

            view.AddSpace(5);

            view.BeginScrollView();

            view.SetEnabled(!view.IsComboBoxFocused() && studioHack.isPoseEditing);

            view.margin = 0f;

            foreach (var info in infoMap.Values)
            {
                var models = bgModelManager.GetModels(info.sourceName);

                view.BeginHorizontal();
                {
                    var indent = new string(' ', info.depth);
                    var name = $"{indent}└{info.displayName}";

                    var labelWidth = view.viewRect.width - view.currentPos.x - 60 - 10;
                    var labelColor = models.Count > 0 ? Color.green : Color.white;
                    view.DrawLabel(name, labelWidth, 20, labelColor);

                    if (view.DrawButton("-", 20, 20))
                    {
                        bgModelManager.DeleteModelBySourceName(info.sourceName);
                    }

                    view.DrawLabel(models.Count.ToString(), 20, 20);

                    if (view.DrawButton("+", 20, 20))
                    {
                        bgModelManager.AddModelBySourceName(info.sourceName);
                    }
                }
                view.EndLayout();
            }

            view.SetEnabled(!view.IsComboBoxFocused());

            view.EndScrollView();
        }
    }
}