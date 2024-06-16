using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using COM3D2.DanceCameraMotion.Plugin;
using COM3D2.MotionTimelineEditor.Plugin;
using UnityEngine;

namespace COM3D2.MotionTimelineEditor_DCM.Plugin
{
    using AttachPoint = PhotoTransTargetObject.AttachPoint;

    public abstract class ModelTimelineLayerBase : TimelineLayerBase
    {
        protected ModelTimelineLayerBase(int slotNo) : base(slotNo)
        {
        }

        public override float CalcEasingValue(float t, int easing)
        {
            return TimelineMotionEasing.MotionEasing(t, (EasingType) easing);
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
            var currentIndex = timeline.activeTrackIndex;

            view.DrawContentListView(
                models,
                DrawModelContent,
                -1,
                -1,
                80);
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

            view.BeginLayout(GUIView.LayoutDirection.Horizontal);
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