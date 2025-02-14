using System;
using System.Collections.Generic;
using UnityEngine;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    using AttachPoint = PhotoTransTargetObject.AttachPoint;

    public abstract class ModelTimelineLayerBase : TimelineLayerBase
    {
        protected ModelTimelineLayerBase(int slotNo) : base(slotNo)
        {
        }

        private List<GUIComboBox<string>> _pluginComboBoxList = new List<GUIComboBox<string>>();
        private List<GUIComboBox<MaidCache>> _maidComboBoxList = new List<GUIComboBox<MaidCache>>();
        private List<GUIComboBox<string>> _attachPointComboBoxList = new List<GUIComboBox<string>>();
        private List<string> _pluginNames = new List<string>();
        private Dictionary<string, int> _pluginNameToIndex = new Dictionary<string, int>();
        private List<MaidCache> _maidCaches = new List<MaidCache>();

        protected void DrawModelManage(GUIView view)
        {
            if (timeline == null)
            {
                return;
            }

            _pluginNames = modelHackManager.pluginNames;

            _pluginNameToIndex.Clear();
            for (var i = 0; i < _pluginNames.Count; i++)
            {
                _pluginNameToIndex[_pluginNames[i]] = i;
            }

            _maidCaches.Clear();
            _maidCaches.Add(null);
            _maidCaches.AddRange(maidManager.maidCaches);

            var models = modelManager.models;
            if (models.Count == 0)
            {
                view.DrawLabel("モデルがありません", -1, 20);
                return;
            }

            while (_pluginComboBoxList.Count < models.Count)
            {
                _pluginComboBoxList.Add(new GUIComboBox<string>
                {
                    getName = (name, _) => string.IsNullOrEmpty(name) ? "Default" : name,
                });
            }

            while (_maidComboBoxList.Count < models.Count)
            {
                _maidComboBoxList.Add(new GUIComboBox<MaidCache>
                {
                    getName = (maidCache, _) => maidCache == null ? "未選択" : maidCache.fullName,
                    contentSize = new Vector2(150, 300),
                });
            }

            while (_attachPointComboBoxList.Count < models.Count)
            {
                _attachPointComboBoxList.Add(new GUIComboBox<string>
                {
                    getName = (name, _) => name,
                    items = BoneUtils.AttachPointNames,
                    buttonSize = new Vector2(60, 20),
                });
            }

            view.DrawHorizontalLine(Color.gray);

            view.AddSpace(5);

            view.padding = Vector2.zero;

            view.BeginScrollView();

            view.SetEnabled(!view.IsComboBoxFocused() && studioHackManager.isPoseEditing);

            for (var i = 0; i < models.Count; i++)
            {
                DrawModelContent(view, models[i], i);
            }

            view.SetEnabled(!view.IsComboBoxFocused());

            view.DrawHorizontalLine(Color.gray);

            view.DrawToggle("モデルのタンジェント補間を有効化", timeline.isTangentModel, -1, 20, newValue =>
            {
                timeline.isTangentModel = newValue;

                var layer = timelineManager.GetLayer<ModelTimelineLayer>();
                if (layer != null)
                {
                    layer.InitTangent();
                    layer.ApplyCurrentFrame(true);
                }
            });

            view.DrawToggle("モデルボーンのタンジェント補間を有効化", timeline.isTangentModelBone, -1, 20, newValue =>
            {
                timeline.isTangentModelBone = newValue;

                var layer = timelineManager.GetLayer<ModelBoneTimelineLayer>();
                if (layer != null)
                {
                    layer.InitTangent();
                    layer.ApplyCurrentFrame(true);
                }
            });

            view.DrawToggle("モデルシェイプのタンジェント補間を有効化", timeline.isTangentModelShapeKey, -1, 20, newValue =>
            {
                timeline.isTangentModelShapeKey = newValue;

                var layer = timelineManager.GetLayer<ModelShapeKeyTimelineLayer>();
                if (layer != null)
                {
                    layer.InitTangent();
                    layer.ApplyCurrentFrame(true);
                }
            });

            view.EndScrollView();
        }

        protected void DrawModelContent(
            GUIView view,
            StudioModelStat model,
            int modelIndex)
        {
            if (model == null)
            {
                return;
            }
            if (modelIndex < 0 || modelIndex >= _maidComboBoxList.Count || modelIndex >= _attachPointComboBoxList.Count)
            {
                return;
            }

            view.margin = GUIView.defaultMargin;
            view.padding = GUIView.defaultPadding;

            view.DrawToggle(model.displayName, model.visible, -1, 20, newValue =>
            {
                modelManager.SetModelVisible(model, newValue);
                model.visible = newValue;
            });

            view.BeginHorizontal();
            {
                // プラグイン選択
                {
                    var pluginComboBox = _pluginComboBoxList[modelIndex];

                    pluginComboBox.currentIndex = GetPluginIndex(model.pluginName);
                    pluginComboBox.items = _pluginNames;
                    pluginComboBox.onSelected = (pluginName, index) =>
                    {
                        modelManager.ChangePluginName(model, pluginName);
                    };

                    pluginComboBox.DrawButton(view);
                }

                if (view.DrawButton("複製", 45, 20))
                {
                    timelineManager.CopyModel(model);
                }

                if (view.DrawButton("削除", 45, 20))
                {
                    modelManager.DeleteModel(model);
                }
            }
            view.EndLayout();

            view.BeginHorizontal();
            {
                // メイド選択
                {
                    var maidComboBox = _maidComboBoxList[modelIndex];

                    maidComboBox.currentIndex = model.attachMaidSlotNo + 1;
                    maidComboBox.items = _maidCaches;
                    maidComboBox.onSelected = (maidCache, index) =>
                    {
                        model.attachMaidSlotNo = index - 1;
                        if (model.attachPoint == AttachPoint.Null)
                        {
                            model.attachPoint = AttachPoint.Head;
                        }
                        modelManager.UpdateAttachPoint(model);
                    };

                    maidComboBox.DrawButton(view);
                }

                // アタッチポイント選択
                if (model.attachMaidSlotNo >= 0)
                {
                    var attachPointComboBox = _attachPointComboBoxList[modelIndex];

                    attachPointComboBox.currentIndex = (int) model.attachPoint;
                    attachPointComboBox.onSelected = (maidCache, index) =>
                    {
                        model.attachPoint = (AttachPoint) index;
                        modelManager.UpdateAttachPoint(model);
                    };

                    attachPointComboBox.DrawButton(view);
                }
            }
            view.EndLayout();

            view.DrawHorizontalLine(Color.gray);
        }

        private int GetPluginIndex(string pluginName)
        {
            int index;
            if (!string.IsNullOrEmpty(pluginName) && _pluginNameToIndex.TryGetValue(pluginName, out index))
            {
                return index;
            }

            return 0;
        }
    }
}