using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    public class TimelineTemplateUI : SubWindowUIBase
    {
        public override string title => "テンプレート";

        private enum TabType
        {
            操作,
            ｶﾃｺﾞﾘ編集,
            ﾃﾝﾌﾟﾚ編集,
        }

        private TabType _tabType = TabType.操作;

        private static TimelineTemplateManager templateManager => TimelineTemplateManager.instance;

        private Dictionary<string, string> _newTemplateNames = new Dictionary<string, string>();
        private string _newCategoryName = "";

        public TimelineTemplateUI(SubWindow subWindow) : base(subWindow)
        {
        }

        public override void DrawContent(GUIView view)
        {
            view.SetEnabled(!view.IsComboBoxFocused());

            if (timeline == null)
            {
                view.DrawLabel("タイムラインをロードしてください。", -1, 20);
                return;
            }

            _tabType = view.DrawTabs(_tabType, 70, 20);

            view.DrawHorizontalLine(Color.gray);

            view.AddSpace(5);

            view.BeginScrollView();

            switch (_tabType)
            {
                case TabType.操作:
                    DrawControl(view);
                    break;
                case TabType.ｶﾃｺﾞﾘ編集:
                    DrawCategory(view);
                    break;
                case TabType.ﾃﾝﾌﾟﾚ編集:
                    DrawTemplate(view);
                    break;
            }

            view.EndScrollView();
        }

        private void DrawControl(GUIView view)
        {
            var info = timelineManager.GetLayerInfo(currentLayer?.layerType);
            view.DrawLabel($"{info?.displayName ?? ""} テンプレ", -1, 20);

            view.DrawHorizontalLine();

            // テンプレート一覧
            var templateLayer = templateManager.GetTemplateLayer();
            if (templateLayer == null)
            {
                view.DrawLabel("テンプレートレイヤーが存在しません。", -1, 20);
                return;
            }

            var categories = templateLayer.categories.ToArray();
            foreach (var category in categories)
            {
                view.DrawLabel($"[{category.categoryName}]", 200, 20);

                view.BeginHorizontal();

                foreach (var template in category.templates)
                {
                    if (template.nameWidth < 0f)
                    {
                        template.nameWidth = GUIView.CalcWidth(GUIView.gsButton, template.templateName);
                    }

                    if (view.currentPos.x + template.nameWidth > view.viewRect.width)
                    {
                        view.EndLayout();
                        view.BeginHorizontal();
                    }

                    if (view.DrawButton(template.templateName, template.nameWidth, 20))
                    {
                        category.ApplyTemplate(template.templateName);
                    }
                }

                view.EndLayout();

                // 新規テンプレート作成
                if (!_newTemplateNames.ContainsKey(category.categoryName))
                {
                    _newTemplateNames[category.categoryName] = "";
                }
                var newTemplateName = _newTemplateNames[category.categoryName];

                view.BeginHorizontal();
                {
                    view.DrawTextField(new GUIView.TextFieldOption
                    {
                        label = "テンプレ名",
                        labelWidth = 70,
                        width = 200,
                        value = newTemplateName,
                        onChanged = value => _newTemplateNames[category.categoryName] = value,
                        hiddenButton = true,
                    });

                    if (view.DrawButton("追加", 45, 20, !string.IsNullOrEmpty(newTemplateName)))
                    {
                        if (category.AddTemplate(newTemplateName))
                        {
                            _newTemplateNames[category.categoryName] = "";
                        }
                    }
                }
                view.EndLayout();

                if (newTemplateName.Length > 0)
                {
                    var selectedBones = timelineManager.selectedBones;
                    if (selectedBones == null || selectedBones.Count == 0)
                    {
                        view.DrawLabel("ボーンが選択されていません。", -1, 20, Color.green);
                    }
                    else if (category.HasTemplate(newTemplateName))
                    {
                        view.DrawLabel("同名のテンプレートが既に存在します。", -1, 20, Color.green);
                    }
                }

                view.DrawHorizontalLine();
            }
        }

        private void DrawCategory(GUIView view)
        {
            // 新規カテゴリ作成
            view.BeginHorizontal();
            {
                view.DrawTextField(new GUIView.TextFieldOption
                {
                    label = "カテゴリ名",
                    labelWidth = 70,
                    width = 200,
                    value = _newCategoryName,
                    onChanged = value => _newCategoryName = value,
                    hiddenButton = true,
                });

                if (view.DrawButton("追加", 45, 20, !string.IsNullOrEmpty(_newCategoryName)))
                {
                    if (templateManager.AddTemplateCategory(_newCategoryName))
                    {
                        _newCategoryName = "";
                    }
                }
            }
            view.EndLayout();

            view.DrawHorizontalLine();

            // カテゴリ一覧
            var templateLayer = templateManager.GetTemplateLayer();
            if (templateLayer == null)
            {
                view.DrawLabel("テンプレートレイヤーが存在しません。", -1, 20);
                return;
            }

            var categories = templateLayer.categories.ToArray();
            foreach (var category in categories)
            {
                view.BeginHorizontal();
                {
                    view.DrawTextField(new GUIView.TextFieldOption
                    {
                        width = 150,
                        value = category.categoryName,
                        onChanged = value =>
                        {
                            category.categoryName = value;
                            category.dirty = true;
                        },
                        disabled = category.categoryName == "Default",
                        hiddenButton = true,
                    });

                    if (view.DrawButton("削除", 45, 20, category.categoryName != "Default"))
                    {
                        templateLayer.RemoveCategory(category.categoryName);
                    }

                    if (view.DrawButton("↑", 20, 20, templateLayer.CanMoveCategory(category.categoryName, -1)))
                    {
                        templateLayer.MoveCategory(category.categoryName, -1);
                    }

                    if (view.DrawButton("↓", 20, 20, templateLayer.CanMoveCategory(category.categoryName, 1)))
                    {
                        templateLayer.MoveCategory(category.categoryName, 1);
                    }
                }
                view.EndLayout();
            }
        }

        private void DrawTemplate(GUIView view)
        {
            var templateLayer = templateManager.GetTemplateLayer();
            if (templateLayer == null)
            {
                view.DrawLabel("テンプレートレイヤーが存在しません。", -1, 20);
                return;
            }

            foreach (var category in templateLayer.categories)
            {
                view.BeginHorizontal();
                {
                    view.DrawLabel($"[{category.categoryName}]", 180, 20);

                    if (view.DrawButton("ソート", 60, 20))
                    {
                        category.SortTemplates();
                    }
                }
                view.EndLayout();

                var templates = category.templates.ToArray();
                foreach (var template in templates)
                {
                    view.BeginHorizontal();
                    {
                        view.DrawTextField(new GUIView.TextFieldOption
                        {
                            width = 150,
                            value = template.templateName,
                            onChanged = value =>
                            {
                                template.templateName = value;
                                template.nameWidth = -1f;
                                category.dirty = true;
                            },
                            hiddenButton = true,
                        });

                        if (view.DrawButton("削除", 45, 20))
                        {
                            category.RemoveTemplate(template.templateName);
                        }
                    }
                    view.EndLayout();
                }

                // 新規テンプレート作成
                if (!_newTemplateNames.ContainsKey(category.categoryName))
                {
                    _newTemplateNames[category.categoryName] = "";
                }
                var newTemplateName = _newTemplateNames[category.categoryName];

                view.BeginHorizontal();
                {
                    view.DrawTextField(new GUIView.TextFieldOption
                    {
                        label = "テンプレ名",
                        labelWidth = 70,
                        width = 200,
                        value = newTemplateName,
                        onChanged = value => _newTemplateNames[category.categoryName] = value,
                        hiddenButton = true,
                    });

                    if (view.DrawButton("追加", 45, 20, !string.IsNullOrEmpty(newTemplateName)))
                    {
                        if (category.AddTemplate(newTemplateName))
                        {
                            _newTemplateNames[category.categoryName] = "";
                        }
                    }
                }
                view.EndLayout();

                if (newTemplateName.Length > 0)
                {
                    var selectedBones = timelineManager.selectedBones;
                    if (selectedBones == null || selectedBones.Count == 0)
                    {
                        view.DrawLabel("ボーンが選択されていません。", -1, 20, Color.green);
                    }
                    else if (category.HasTemplate(newTemplateName))
                    {
                        view.DrawLabel("同名のテンプレートが既に存在します。", -1, 20, Color.green);
                    }
                }

                view.DrawHorizontalLine();
            }
        }
    }
} 