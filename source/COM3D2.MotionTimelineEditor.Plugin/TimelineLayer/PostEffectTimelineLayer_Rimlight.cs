using System;
using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    public partial class PostEffectTimelineLayer : TimelineLayerBase
    {
        private void InitRimlightEffect()
        {
            while (postEffectManager.GetRimlightCount() < timeline.rimlightCount)
            {
                postEffectManager.AddRimlightData();
            }
            while (postEffectManager.GetRimlightCount() > timeline.rimlightCount)
            {
                postEffectManager.RemoveRimlightData();
            }
        }

        private void ApplyRimlight(MotionData motion, float t)
        {
            var start = motion.start as TransformDataRimlight;
            var end = motion.end as TransformDataRimlight;

            float easingTime = CalcEasingValue(t, start.easing);
            var rimlight = RimlightData.Lerp(start.rimlight, end.rimlight, easingTime);

            var index = start.index;
            postEffectManager.ApplyRimlight(index, rimlight);
        }

        private List<string> _rimlightNames = new List<string>();
        private List<string> rimlightNames
        {
            get
            {
                var rimlightCount = timeline.rimlightCount;
                if (_rimlightNames.Count != rimlightCount)
                {
                    _rimlightNames.Clear();
                    for (var i = 0; i < rimlightCount; i++)
                    {
                        _rimlightNames.Add(GetRimlightName(i));
                    }
                }

                return _rimlightNames;
            }
        }

        private List<string> _rimlightJpNames = new List<string>();
        private List<string> rimlightJpNames
        {
            get
            {
                var rimlightCount = timeline.rimlightCount;
                if (_rimlightJpNames.Count != rimlightCount)
                {
                    _rimlightJpNames.Clear();
                    for (var i = 0; i < rimlightCount; i++)
                    {
                        _rimlightJpNames.Add(GetRimlightJpName(i));
                    }
                }

                return _rimlightJpNames;
            }
        }

        private GUIComboBox<string> _rimlightNameComboBox = new GUIComboBox<string>
        {
            getName = (name, index) => name,
        };

        private GUIComboBox<string> _copyToRimlightComboBox = new GUIComboBox<string>
        {
            getName = (name, index) => name,
        };

        public void DrawRimlight(GUIView view)
        {
            view.SetEnabled(!view.IsComboBoxFocused() && studioHack.isPoseEditing);

            view.BeginHorizontal();
            {
                view.margin = 0;

                view.DrawLabel("エフェクト数", view.labelWidth, 20);

                view.DrawIntField(new GUIView.IntFieldOption
                {
                    value = timeline.rimlightCount,
                    width = view.viewRect.width - (view.labelWidth + 40 + view.padding.x * 2),
                    height = 20,
                    onChanged = x => timeline.rimlightCount = x,
                });

                if (view.DrawButton("-", 20, 20))
                {
                    timeline.rimlightCount--;
                }
                if (view.DrawButton("+", 20, 20))
                {
                    timeline.rimlightCount++;
                }

                timeline.rimlightCount = Mathf.Clamp(timeline.rimlightCount, 0, 8);

                view.margin = GUIView.defaultMargin;
            }
            view.EndLayout();

            if (timeline.rimlightCount == 0)
            {
                view.DrawLabel("エフェクトを追加してください", 200, 20);
                return;
            }

            _rimlightNameComboBox.items = rimlightJpNames;
            _rimlightNameComboBox.DrawButton("対象", view);

            var rimlightName = _rimlightNameComboBox.currentItem;
            var index = _rimlightNameComboBox.currentIndex;

            var rimlight = postEffectManager.GetRimlightData(index);
            if (rimlight == null)
            {
                view.DrawLabel("エフェクトを選択してください", 200, 20);
                return;
            }

            view.SetEnabled(!view.IsComboBoxFocused());
            view.DrawHorizontalLine(Color.gray);
            view.AddSpace(5);
            view.BeginScrollView();

            view.SetEnabled(!view.IsComboBoxFocused() && studioHack.isPoseEditing);

            var updateTransform = false;

            updateTransform = view.DrawToggle("有効化", rimlight.enabled, 80, 20, newValue =>
            {
                rimlight.enabled = newValue;
            });

            if (timeline.usePostEffectExtra)
            {
                updateTransform |= view.DrawColor(
                    _color1FieldValue,
                    rimlight.color1,
                    Color.white,
                    newValue => rimlight.color1 = newValue
                );

                updateTransform |= view.DrawColor(
                    _color2FieldValue,
                    rimlight.color2,
                    new Color(1f, 1f, 1f, 0f),
                    newValue => rimlight.color2 = newValue
                );
            }
            else
            {
                updateTransform |= view.DrawColor(
                    _color1FieldValue,
                    rimlight.color1,
                    Color.white,
                    newValue =>
                    {
                        var alpha2 = rimlight.color2.a;
                        rimlight.color1 = newValue;
                        rimlight.color2 = newValue;
                        rimlight.color2.a = alpha2;
                    }
                );

                updateTransform |= view.DrawSliderValue(new GUIView.SliderOption
                {
                    label = "A2",
                    labelWidth = 30,
                    min = 0f,
                    max = 1f,
                    step = 0.01f,
                    defaultValue = 0f,
                    value = rimlight.color2.a,
                    onChanged = newValue => rimlight.color2.a = newValue,
                });
            }

            var initialEulerAngles = new Vector3(0f, 0f, 0f);
            var transformCache = view.GetTransformCache(null);
            transformCache.eulerAngles = rimlight.rotation;

            view.DrawLabel("ライト方向", 200, 20);

            updateTransform |= DrawEulerAngles(
                view,
                transformCache,
                TransformEditType.全て,
                rimlightName,
                initialEulerAngles);

            updateTransform |= view.DrawToggle("ワールド空間", rimlight.isWorldSpace, 120, 20, newValue =>
            {
                rimlight.isWorldSpace = newValue;
            });

            updateTransform |= view.DrawSliderValue(new GUIView.SliderOption
            {
                label = "影響",
                labelWidth = 30,
                min = 0f,
                max = 2f,
                step = 0.01f,
                defaultValue = 1f,
                value = rimlight.lightArea,
                onChanged = newValue => rimlight.lightArea = newValue,
            });

            updateTransform |= view.DrawSliderValue(new GUIView.SliderOption
            {
                label = "幅",
                labelWidth = 30,
                min = 0f,
                max = 2f,
                step = 0.01f,
                defaultValue = 0.05f,
                value = rimlight.fadeRange,
                onChanged = newValue => rimlight.fadeRange = newValue,
            });

            updateTransform |= view.DrawSliderValue(new GUIView.SliderOption
            {
                label = "指数",
                labelWidth = 30,
                min = 0f,
                max = 10f,
                step = 0.01f,
                defaultValue = 1f,
                value = rimlight.fadeExp,
                onChanged = newValue => rimlight.fadeExp = newValue,
            });

            updateTransform |= view.DrawSliderValue(new GUIView.SliderOption
            {
                label = "最小深度",
                labelWidth = 30,
                min = 0f,
                max = rimlight.depthMax,
                step = 0.1f,
                defaultValue = 0f,
                value = rimlight.depthMin,
                onChanged = newValue => rimlight.depthMin = newValue,
            });

            updateTransform |= view.DrawSliderValue(new GUIView.SliderOption
            {
                label = "最大深度",
                labelWidth = 30,
                min = rimlight.depthMin,
                max = 100f,
                step = 0.1f,
                defaultValue = 5f,
                value = rimlight.depthMax,
                onChanged = newValue => rimlight.depthMax = newValue,
            });

            updateTransform |= view.DrawSliderValue(new GUIView.SliderOption
            {
                label = "深度幅",
                labelWidth = 30,
                min = 0f,
                max = 10f,
                step = 0.01f,
                defaultValue = 1f,
                value = rimlight.depthFade,
                onChanged = newValue => rimlight.depthFade = newValue,
            });

            view.DrawLabel("ブレンドモード", 100, 20);

            updateTransform |= view.DrawSliderValue(new GUIView.SliderOption
            {
                label = "通常",
                labelWidth = 30,
                min = 0f,
                max = 2f,
                step = 0.01f,
                defaultValue = 0f,
                value = rimlight.useNormal,
                onChanged = newValue => rimlight.useNormal = newValue,
            });

            updateTransform |= view.DrawSliderValue(new GUIView.SliderOption
            {
                label = "加算",
                labelWidth = 30,
                min = 0f,
                max = 2f,
                step = 0.01f,
                defaultValue = 0f,
                value = rimlight.useAdd,
                onChanged = newValue => rimlight.useAdd = newValue,
            });

            updateTransform |= view.DrawSliderValue(new GUIView.SliderOption
            {
                label = "乗算",
                labelWidth = 30,
                min = 0f,
                max = 2f,
                step = 0.01f,
                defaultValue = 0f,
                value = rimlight.useMultiply,
                onChanged = newValue => rimlight.useMultiply = newValue,
            });

            updateTransform |= view.DrawSliderValue(new GUIView.SliderOption
            {
                label = "Overlay",
                labelWidth = 30,
                min = 0f,
                max = 2f,
                step = 0.01f,
                defaultValue = 0f,
                value = rimlight.useOverlay,
                onChanged = newValue => rimlight.useOverlay = newValue,
            });

            updateTransform |= view.DrawSliderValue(new GUIView.SliderOption
            {
                label = "減算",
                labelWidth = 30,
                min = 0f,
                max = 2f,
                step = 0.01f,
                defaultValue = 0f,
                value = rimlight.useSubstruct,
                onChanged = newValue => rimlight.useSubstruct = newValue,
            });

            updateTransform |= view.DrawSliderValue(new GUIView.SliderOption
            {
                label = "E深度",
                labelWidth = 30,
                min = 0f,
                max = 10f,
                step = 0.01f,
                defaultValue = 0.2f,
                value = rimlight.edgeDepth,
                onChanged = newValue => rimlight.edgeDepth = newValue,
            });

            updateTransform |= view.DrawSliderValue(new GUIView.SliderOption
            {
                label = "E幅",
                labelWidth = 30,
                min = 0f,
                max = 10f,
                step = 0.01f,
                defaultValue = 1f,
                value = rimlight.edgeRange,
                onChanged = newValue => rimlight.edgeRange = newValue,
            });

            updateTransform |= view.DrawSliderValue(new GUIView.SliderOption
            {
                label = "最小高さ",
                labelWidth = 30,
                min = -config.positionRange,
                max = config.positionRange,
                step = 0.01f,
                defaultValue = 0.01f,
                value = rimlight.heightMin,
                onChanged = newValue => rimlight.heightMin = newValue,
            });

            view.DrawHorizontalLine(Color.gray);

            _copyToRimlightComboBox.items = _rimlightJpNames;
            _copyToRimlightComboBox.DrawButton("コピー先", view);

            if (view.DrawButton("コピー", 60, 20))
            {
                var copyToIndex = _copyToRimlightComboBox.currentIndex;
                if (copyToIndex != -1 && copyToIndex != index)
                {
                    postEffectManager.ApplyRimlight(copyToIndex, rimlight);
                }
            }

            view.SetEnabled(!view.IsComboBoxFocused());

            view.DrawHorizontalLine(Color.gray);

            view.DrawLabel("共通設定", 100, 20);

            updateTransform |= view.DrawToggle("デバッグ表示", config.rimlightDebug, 150, 20, newValue =>
            {
                config.rimlightDebug = newValue;
                config.dirty = true;
            });

            if (updateTransform)
            {
                rimlight.rotation = transformCache.eulerAngles;
                postEffectManager.ApplyRimlight(index, rimlight);
            }

            view.SetEnabled(!view.IsComboBoxFocused());
            view.EndScrollView();
        }

        public static bool IsValidRimlightIndex(int index)
        {
            if (index < 0 || index >= timeline.rimlightCount)
            {
                return false;
            }

            return true;
        }

        public static string GetRimlightName(int index)
        {
            if (!IsValidRimlightIndex(index))
            {
                return "";
            }

            var suffix = PluginUtils.GetGroupSuffix(index);
            return PostEffectUtils.ToEffectName(PostEffectType.Rimlight) + suffix;
        }

        public static string GetRimlightJpName(int index)
        {
            if (!IsValidRimlightIndex(index))
            {
                return "";
            }

            var suffix = PluginUtils.GetGroupSuffix(index);
            return PostEffectUtils.ToJpName(PostEffectType.Rimlight) + suffix;
        }
    }
}