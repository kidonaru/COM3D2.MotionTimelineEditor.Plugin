using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    using AttachPoint = PhotoTransTargetObject.AttachPoint;

    public class StudioModelUI : SubWindowUIBase
    {
        public override string title
        {
            get
            {
                return "モデル設定";
            }
        }

        private List<ComboBoxValue<string>> _pluginComboBoxList = new List<ComboBoxValue<string>>();
        private List<ComboBoxValue<MaidCache>> _maidComboBoxList = new List<ComboBoxValue<MaidCache>>();
        private List<ComboBoxValue<string>> _attachPointComboBoxList = new List<ComboBoxValue<string>>();
        private List<string> _pluginNames = new List<string>();
        private Dictionary<string, int> _pluginNameToIndex = new Dictionary<string, int>();
        private List<MaidCache> _maidCaches = new List<MaidCache>();
        private Vector2 _scrollPosition = Vector2.zero;

        public StudioModelUI(SubWindow subWindow) : base(subWindow)
        {
        }

        public override void DrawContent(GUIView view)
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

            {
                DrawModels(view);
            }
        }

        public void DrawModels(GUIView view)
        {
            var models = modelManager.models;
            if (models.Count == 0)
            {
                view.DrawLabel("モデルがありません", -1, 20);
                return;
            }

            while (_pluginComboBoxList.Count < models.Count)
            {
                _pluginComboBoxList.Add(new ComboBoxValue<string>
                {
                    getName = (name, _) =>
                    {
                        return string.IsNullOrEmpty(name) ? "Default" : name;
                    },
                });
            }

            while (_maidComboBoxList.Count < models.Count)
            {
                _maidComboBoxList.Add(new ComboBoxValue<MaidCache>
                {
                    getName = (maidCache, _) =>
                    {
                        return maidCache == null ? "未選択" : maidCache.fullName;
                    },
                });
            }

            while (_attachPointComboBoxList.Count < models.Count)
            {
                _attachPointComboBoxList.Add(new ComboBoxValue<string>
                {
                    getName = (name, _) =>
                    {
                        return name;
                    },
                    items = BoneUtils.AttachPointNames,
                });
            }

            view.SetEnabled(!IsComboBoxFocused());

            view.padding = Vector2.zero;
            var currentIndex = timeline.activeTrackIndex;

            view.DrawContentListView(
                models,
                DrawModel,
                -1,
                -1,
                ref _scrollPosition,
                80);
        }

        public void DrawModel(
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

                    view.DrawComboBoxButton(pluginComboBox, 150, 20, true);
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

                    view.DrawComboBoxButton(maidComboBox, 150, 20, true);
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

                    view.DrawComboBoxButton(attachPointComboBox, 100, 20, true);
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

        public override void DrawComboBox(GUIView view)
        {
            base.DrawComboBox(view);

            var models = modelManager.models;
            for (var i = 0; i < models.Count; i++)
            {
                if (i >= _pluginComboBoxList.Count || i >= _maidComboBoxList.Count || i >= _attachPointComboBoxList.Count)
                {
                    break;
                }

                var pluginComboBox = _pluginComboBoxList[i];
                var maidComboBox = _maidComboBoxList[i];
                var attachPointComboBox = _attachPointComboBoxList[i];

                view.DrawComboBoxContent(
                    pluginComboBox,
                    130, 120,
                    view.viewRect.width, view.viewRect.height,
                    20);

                view.DrawComboBoxContent(
                    maidComboBox,
                    130, 120,
                    view.viewRect.width, view.viewRect.height,
                    20);

                view.DrawComboBoxContent(
                    attachPointComboBox,
                    80, 200,
                    view.viewRect.width, view.viewRect.height,
                    20);
            }
        }

        public override bool IsComboBoxFocused()
        {
            if (base.IsComboBoxFocused())
            {
                return true;
            }

            var models = modelManager.models;
            for (var i = 0; i < models.Count; i++)
            {
                if (i >= _pluginComboBoxList.Count || i >= _maidComboBoxList.Count || i >= _attachPointComboBoxList.Count)
                {
                    break;
                }

                var pluginComboBox = _pluginComboBoxList[i];
                var maidComboBox = _maidComboBoxList[i];
                var attachPointComboBox = _attachPointComboBoxList[i];

                if (pluginComboBox.focused || maidComboBox.focused || attachPointComboBox.focused)
                {
                    return true;
                }
            }

            return false;
        }
    }
}