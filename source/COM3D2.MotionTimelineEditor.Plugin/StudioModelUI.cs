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

        private List<ComboBoxValue<MaidCache>> _maidComboBoxList = new List<ComboBoxValue<MaidCache>>();
        private List<ComboBoxValue<string>> _attachPointComboBoxList = new List<ComboBoxValue<string>>();
        private List<MaidCache> _maidCaches = new List<MaidCache>();
        private Vector2 _scrollPosition = Vector2.zero;

        public override void OnOpen()
        {
        }

        public override void DrawWindow(int id)
        {
            {
                var view = new GUIView(0, 0, WINDOW_WIDTH, 20);
                view.padding = Vector2.zero;
            }

            _maidCaches.Clear();
            _maidCaches.Add(null);
            _maidCaches.AddRange(maidManager.maidCaches);

            {
                var view = new GUIView(0, 20, WINDOW_WIDTH, WINDOW_HEIGHT - 20);

                DrawModels(view);
                DrawComboBox(view);
            }

            GUI.DragWindow();
        }

        public void DrawModels(GUIView view)
        {
            var models = modelManager.models;
            if (models.Count == 0)
            {
                view.DrawLabel("モデルがありません", -1, 20);
                return;
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
                        studioHack.UpdateAttachPoint(model);
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
                        studioHack.UpdateAttachPoint(model);
                    };

                    view.DrawComboBoxButton(attachPointComboBox, 100, 20, true);
                }
            }
            view.EndLayout();

            view.DrawHorizontalLine(Color.gray);
        }

        private bool IsComboBoxFocused()
        {
            var models = modelManager.models;
            for (var i = 0; i < models.Count; i++)
            {
                var maidComboBox = _maidComboBoxList[i];
                var attachPointComboBox = _attachPointComboBoxList[i];

                if (maidComboBox.focused || attachPointComboBox.focused)
                {
                    return true;
                }
            }

            return false;
        }

        private void DrawComboBox(GUIView view)
        {
            view.SetEnabled(true);

            var models = modelManager.models;
            for (var i = 0; i < models.Count; i++)
            {
                if (i >= _maidComboBoxList.Count || i >= _attachPointComboBoxList.Count)
                {
                    break;
                }

                var maidComboBox = _maidComboBoxList[i];
                var attachPointComboBox = _attachPointComboBoxList[i];

                view.DrawComboBoxContent(
                    maidComboBox,
                    130, 120,
                    SubWindow.rc_stgw.width, SubWindow.rc_stgw.height,
                    20);

                view.DrawComboBoxContent(
                    attachPointComboBox,
                    80, 200,
                    SubWindow.rc_stgw.width, SubWindow.rc_stgw.height,
                    20);
            }
        }
    }
}