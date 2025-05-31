using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using UnityEngine;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    [XmlRoot("TemplateLayer")]
    public class TemplateLayerXml
    {
        [XmlElement("LayerName")]
        public string layerName;

        [XmlElement("Category")]
        public List<TemplateCategoryXml> categories = new List<TemplateCategoryXml>();

        [XmlIgnore]
        public List<string> categoryNames = new List<string>();

        [XmlIgnore]
        private bool _dirty = false;

        [XmlIgnore]
        public bool dirty
        {
            get => _dirty;
            set => _dirty = value;
        }

        public void OnLoad()
        {
            // デフォルトカテゴリが無い場合は追加
            if (categories.FindIndex(c => c.categoryName == "Default") < 0)
            {
                categories.Insert(0, new TemplateCategoryXml { categoryName = "Default" });
            }

            categoryNames.Clear();
            foreach (var category in categories)
            {
                categoryNames.Add(category.categoryName);
            }
        }

        public TemplateCategoryXml GetCategory(string categoryName)
        {
            return categories.FirstOrDefault(c => c.categoryName == categoryName);
        }

        public TemplateCategoryXml AddCategory(string categoryName)
        {
            if (HasCategory(categoryName))
            {
                MTEUtils.ShowDialog("カテゴリ名が重複しています: " + categoryName);
                return null;
            }

            var category = new TemplateCategoryXml
            {
                categoryName = categoryName,
            };

            categories.Add(category);
            categoryNames.Add(categoryName);
            dirty = true;
            MTEUtils.Log("新しいカテゴリが追加されました: " + categoryName);
            return category;
        }

        public bool RemoveCategory(string categoryName)
        {
            if (categoryName == "Default")
            {
                MTEUtils.ShowDialog("デフォルトカテゴリは削除できません。");
                return false;
            }

            var category = GetCategory(categoryName);
            if (category == null)
            {
                MTEUtils.ShowDialog($"カテゴリが見つかりません: {categoryName}");
                return false;
            }

            categories.Remove(category);
            categoryNames.Remove(categoryName);
            dirty = true;
            MTEUtils.Log("カテゴリが削除されました: " + categoryName);
            return true;
        }

        public bool HasCategory(string categoryName)
        {
            return categories.Any(c => c.categoryName == categoryName);
        }

        public bool MoveCategory(string categoryName, int offset)
        {
            if (CanMoveCategory(categoryName, offset) == false)
            {
                MTEUtils.ShowDialog("カテゴリの移動に失敗しました: " + categoryName);
                return false;
            }

            var index = categories.FindIndex(c => c.categoryName == categoryName);
            var category = categories[index];
            categories.RemoveAt(index);
            categories.Insert(index + offset, category);
            dirty = true;
            MTEUtils.Log($"カテゴリを移動しました: {categoryName} (新しい位置: {index + offset})");
            return true;
        }

        public bool CanMoveCategory(string categoryName, int offset)
        {
            var index = categories.FindIndex(c => c.categoryName == categoryName);
            return index >= 0 && index + offset >= 0 && index + offset < categories.Count;
        }

        public bool IsDirtyRecursive()
        {
            if (dirty) return true;

            foreach (var category in categories)
            {
                if (category.dirty) return true;
            }

            return false;
        }

        public void ClearDirtyRecursive()
        {
            dirty = false;

            foreach (var category in categories)
            {
                category.dirty = false;
            }
        }
    }

    public class TemplateCategoryXml
    {
        [XmlElement("CategoryName")]
        public string categoryName;

        [XmlElement("Template")]
        public List<TemplateXml> templates = new List<TemplateXml>();

        [XmlIgnore]
        public bool dirty = false;

        [XmlIgnore]
        private static TimelineManager timelineManager => TimelineManager.instance;

        [XmlIgnore]
        private static TimelineData timeline => timelineManager.timeline;

        [XmlIgnore]
        private static ITimelineLayer currentLayer => timelineManager.currentLayer;

        public TemplateXml GetTemplate(string templateName)
        {
            return templates.FirstOrDefault(t => t.templateName == templateName);
        }

        public bool AddTemplate(string templateName)
        {
            var selectedBones = timelineManager.selectedBones;
            if (selectedBones == null || selectedBones.Count == 0)
            {
                MTEUtils.ShowDialog("ボーンを選択してください。");
                return false;
            }

            var tmpFrames = new Dictionary<int, FrameData>();
            foreach (var bone in selectedBones)
            {
                FrameData tmpFrame;
                if (!tmpFrames.TryGetValue(bone.frameNo, out tmpFrame))
                {
                    tmpFrame = currentLayer.CreateFrame(bone.frameNo);
                    tmpFrames[bone.frameNo] = tmpFrame;
                }

                tmpFrame.UpdateBone(bone);
            }

            var newTemplate = new TemplateXml
            {
                templateName = templateName,
                frames = tmpFrames.Values.Select(frame => frame.ToXml()).ToList(),
            };

            if (HasTemplate(templateName))
            {
                MTEUtils.ShowConfirmDialog($"同名のテンプレートがあります\n上書きしますか?\n「{templateName}」", () =>
                {
                    GameMain.Instance.SysDlg.Close();
                    RemoveTemplate(templateName);
                    templates.Add(newTemplate);
                    SortTemplates();
                    dirty = true;
                    MTEUtils.Log("テンプレートが上書きされました: " + templateName);
                });
                return true;
            }

            templates.Add(newTemplate);
            SortTemplates();
            dirty = true;
            MTEUtils.Log("新しいテンプレートが追加されました: " + templateName);
            return true;
        }

        public bool RemoveTemplate(string templateName)
        {
            var template = GetTemplate(templateName);
            if (template == null)
            {
                MTEUtils.ShowDialog($"テンプレートが見つかりません: {templateName}");
                return false;
            }

            templates.Remove(template);
            dirty = true;
            MTEUtils.Log("テンプレートが削除されました: " + templateName);
            return true;
        }

        public bool HasTemplate(string templateName)
        {
            return templates.Any(t => t.templateName == templateName);
        }

        public void SortTemplates()
        {
            var compare = new NaturalStringComparer();
            templates.Sort((a, b) => compare.Compare(a.templateName, b.templateName));
            dirty = true;
        }

        public void ApplyTemplate(string templateName)
        {
            try
            {
                var template = GetTemplate(templateName);
                if (template == null)
                {
                    MTEUtils.ShowDialog("指定されたテンプレートが存在しません: " + templateName);
                    return;
                }

                if (template.frames.Count == 0)
                {
                    MTEUtils.ShowDialog("空のテンプレートです: " + templateName);
                    return;
                }

                var framesXml = template.frames;
                var minFrameNo = framesXml.Min(frame => frame.frameNo);
                foreach (var frameXml in framesXml)
                {
                    var tmpFrame = currentLayer.CreateFrame(frameXml);
                    var frameNo = timelineManager.currentFrameNo + tmpFrame.frameNo - minFrameNo;
                    currentLayer.UpdateBones(frameNo, tmpFrame.bones);
                }

                timeline.AdjustMaxFrameNo();
                timelineManager.RequestHistory("テンプレ適用: " + templateName);
                MTEUtils.LogDebug("テンプレートが適用されました: " + templateName);

                currentLayer.ApplyCurrentFrame(true);
            }
            catch (Exception e)
            {
                MTEUtils.LogException(e);
            }
        }
    }

    public class TemplateXml
    {
        [XmlElement("TemplateName")]
        public string templateName;
        [XmlElement("Frame")]
        public List<FrameXml> frames = new List<FrameXml>();

        [XmlIgnore]
        public float nameWidth = -1f;
    }

    public class TimelineTemplateManager : ManagerBase
    {
        public Dictionary<string, TemplateLayerXml> templateLayerMap = new Dictionary<string, TemplateLayerXml>();

        private static TimelineTemplateManager _instance = null;
        public static TimelineTemplateManager instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new TimelineTemplateManager();
                }
                return _instance;
            }
        }

        public string layerName => currentLayer?.layerName ?? "MotionTimelineLayer";

        private TimelineTemplateManager()
        {
        }

        public override void Init()
        {
            LoadXml();
        }

        public override void Update()
        {
            SaveXml();
        }

        public void LoadXml()
        {
            templateLayerMap.Clear();

            var templateDirPath = PluginUtils.TemplateDirPath;
            var filePaths = Directory.GetFiles(templateDirPath, "*.xml");

            foreach (var path in filePaths)
            {
                try
                {
                    var serializer = new XmlSerializer(typeof(TemplateLayerXml));
                    using (var stream = new FileStream(path, FileMode.Open))
                    {
                        var templateLayer = (TemplateLayerXml)serializer.Deserialize(stream);
                        templateLayer.OnLoad();
                        templateLayerMap[templateLayer.layerName] = templateLayer;
                    }
                }
                catch (Exception e)
                {
                    MTEUtils.LogException(e);
                }
            }
        }

        public void SaveXml()
        {
            foreach (var templateLayer in templateLayerMap.Values)
            {
                if (!templateLayer.IsDirtyRecursive()) continue;

                try
                {
                    var path = PluginUtils.GetTemplatePath(templateLayer.layerName);
                    var serializer = new XmlSerializer(typeof(TemplateLayerXml));
                    using (var stream = new FileStream(path, FileMode.Create))
                    {
                        serializer.Serialize(stream, templateLayer);
                    }
                    templateLayer.ClearDirtyRecursive();
                    MTEUtils.LogDebug($"テンプレートレイヤーを保存しました: {templateLayer.layerName} ({path})");
                }
                catch (Exception e)
                {
                    MTEUtils.LogException(e);
                }
            }
        }

        public TemplateLayerXml GetTemplateLayer()
        {
            if (templateLayerMap.TryGetValue(layerName, out var templateLayer))
            {
                return templateLayer;
            }

            // 無い場合は作成する
            templateLayer = new TemplateLayerXml
            {
                layerName = layerName,
                dirty = true,
            };

            templateLayerMap[layerName] = templateLayer;
            templateLayer.OnLoad();
            return templateLayer;
        }

        public bool AddTemplateCategory(string categoryName)
        {
            var templateLayer = GetTemplateLayer();
            if (templateLayer == null)
            {
                MTEUtils.ShowDialog("テンプレートレイヤーが見つかりません。");
                return false;
            }

            // 新しいカテゴリを追加
            templateLayer.AddCategory(categoryName);
            return true;
        }
    }
}