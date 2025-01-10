using System;
using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    public partial class PostEffectTimelineLayer : TimelineLayerBase
    {
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
            view.SetEnabled(!view.IsComboBoxFocused());

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

            view.SetEnabled(!view.IsComboBoxFocused() && studioHackManager.isPoseEditing);

            var updateTransform = false;
            var defaultTrans = TransformDataRimlight.defaultTrans;

            updateTransform = view.DrawToggle("有効化", rimlight.enabled, 80, 20, newValue =>
            {
                rimlight.enabled = newValue;
            });

            if (timeline.usePostEffectExtraColor)
            {
                updateTransform |= view.DrawColor(
                    _color1FieldValue,
                    rimlight.color1,
                    defaultTrans.initialColor,
                    newValue => rimlight.color1 = newValue
                );

                updateTransform |= view.DrawColor(
                    _color2FieldValue,
                    rimlight.color2,
                    defaultTrans.initialSubColor,
                    newValue => rimlight.color2 = newValue
                );
            }
            else
            {
                updateTransform |= view.DrawColor(
                    _color1FieldValue,
                    rimlight.color1,
                    defaultTrans.initialColor,
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

            var initialEulerAngles = defaultTrans.initialEulerAngles;
            var transformCache = view.GetTransformCache(null);
            transformCache.eulerAngles = rimlight.rotation;

            view.DrawLabel("ライト方向", 200, 20);

            updateTransform |= DrawEulerAngles(
                view,
                transformCache,
                TransformEditType.全て,
                rimlightName,
                initialEulerAngles);

            updateTransform |= view.DrawCustomValueBool(
                defaultTrans.isWorldSpaceInfo,
                rimlight.isWorldSpace,
                newValue => rimlight.isWorldSpace = newValue);

            updateTransform |= view.DrawCustomValueFloat(
                defaultTrans.lightAreaInfo,
                rimlight.lightArea,
                newValue => rimlight.lightArea = newValue);

            updateTransform |= view.DrawCustomValueFloat(
                defaultTrans.fadeRangeInfo,
                rimlight.fadeRange,
                newValue => rimlight.fadeRange = newValue);

            updateTransform |= view.DrawCustomValueFloat(
                defaultTrans.fadeExpInfo,
                rimlight.fadeExp,
                newValue => rimlight.fadeExp = newValue);

            updateTransform |= view.DrawCustomValueFloat(
                defaultTrans.depthMinInfo,
                rimlight.depthMin,
                newValue => rimlight.depthMin = newValue);

            updateTransform |= view.DrawCustomValueFloat(
                defaultTrans.depthMaxInfo,
                rimlight.depthMax,
                newValue => rimlight.depthMax = newValue);

            updateTransform |= view.DrawCustomValueFloat(
                defaultTrans.depthFadeInfo,
                rimlight.depthFade,
                newValue => rimlight.depthFade = newValue);

            updateTransform |= view.DrawCustomValueFloat(
                defaultTrans.edgeDepthInfo,
                rimlight.edgeDepth,
                newValue => rimlight.edgeDepth = newValue);

            updateTransform |= view.DrawCustomValueFloat(
                defaultTrans.edgeRangeInfo,
                rimlight.edgeRange,
                newValue => rimlight.edgeRange = newValue);
            
            updateTransform |= view.DrawCustomValueFloat(
                defaultTrans.heightMinInfo,
                rimlight.heightMin,
                newValue => rimlight.heightMin = newValue);

            view.DrawLabel("ブレンドモード", 100, 20);

            if (timeline.usePostEffectExtraBlend)
            {
                updateTransform |= view.DrawCustomValueFloat(
                    defaultTrans.useNormalInfo,
                    rimlight.useNormal,
                    newValue => rimlight.useNormal = newValue);

                updateTransform |= view.DrawCustomValueFloat(
                    defaultTrans.useAddInfo,
                    rimlight.useAdd,
                    newValue => rimlight.useAdd = newValue);

                updateTransform |= view.DrawCustomValueFloat(
                    defaultTrans.useMultiplyInfo,
                    rimlight.useMultiply,
                    newValue => rimlight.useMultiply = newValue);

                updateTransform |= view.DrawCustomValueFloat(
                    defaultTrans.useOverlayInfo,
                    rimlight.useOverlay,
                    newValue => rimlight.useOverlay = newValue);

                updateTransform |= view.DrawCustomValueFloat(
                    defaultTrans.useSubstructInfo,
                    rimlight.useSubstruct,
                    newValue => rimlight.useSubstruct = newValue);
            }
            else
            {
                updateTransform |= view.DrawCustomValueFloat(
                    defaultTrans.useAddInfo,
                    rimlight.useAdd,
                    newValue => rimlight.useAdd = newValue);
            }

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