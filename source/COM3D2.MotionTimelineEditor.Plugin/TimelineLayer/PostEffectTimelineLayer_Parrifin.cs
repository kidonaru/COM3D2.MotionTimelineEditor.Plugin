using System;
using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    public partial class PostEffectTimelineLayer : TimelineLayerBase
    {
        private void InitParrifinEffect()
        {
            while (postEffectManager.GetParaffinCount() < timeline.paraffinCount)
            {
                postEffectManager.AddParaffinData();
            }
            while (postEffectManager.GetParaffinCount() > timeline.paraffinCount)
            {
                postEffectManager.RemoveParaffinData();
            }
        }

        private void ApplyParaffin(MotionData motion, float t)
        {
            var start = motion.start as TransformDataParaffin;
            var end = motion.end as TransformDataParaffin;

            float easingTime = CalcEasingValue(t, start.easing);
            var paraffin = ColorParaffinData.Lerp(start.paraffin, end.paraffin, easingTime);

            var index = start.index;
            postEffectManager.ApplyParaffin(index, paraffin);
        }

        private List<string> _paraffinNames = new List<string>();
        private List<string> paraffinNames
        {
            get
            {
                var paraffinCount = timeline.paraffinCount;
                if (_paraffinNames.Count != paraffinCount)
                {
                    _paraffinNames.Clear();
                    for (var i = 0; i < paraffinCount; i++)
                    {
                        _paraffinNames.Add(GetParaffinName(i));
                    }
                }

                return _paraffinNames;
            }
        }

        private List<string> _paraffinJpNames = new List<string>();
        private List<string> paraffinJpNames
        {
            get
            {
                var paraffinCount = timeline.paraffinCount;
                if (_paraffinJpNames.Count != paraffinCount)
                {
                    _paraffinJpNames.Clear();
                    for (var i = 0; i < paraffinCount; i++)
                    {
                        _paraffinJpNames.Add(GetParaffinJpName(i));
                    }
                }

                return _paraffinJpNames;
            }
        }

        private GUIComboBox<string> _paraffinNameComboBox = new GUIComboBox<string>
        {
            getName = (name, index) => name,
        };

        private GUIComboBox<string> _copyToParaffinComboBox = new GUIComboBox<string>
        {
            getName = (name, index) => name,
        };

        public void DrawParaffin(GUIView view)
        {
            view.SetEnabled(!view.IsComboBoxFocused() && studioHack.isPoseEditing);

            view.BeginHorizontal();
            {
                view.margin = 0;

                view.DrawLabel("エフェクト数", view.labelWidth, 20);

                view.DrawIntField(new GUIView.IntFieldOption
                {
                    value = timeline.paraffinCount,
                    width = view.viewRect.width - (view.labelWidth + 40 + view.padding.x * 2),
                    height = 20,
                    onChanged = x => timeline.paraffinCount = x,
                });

                if (view.DrawButton("-", 20, 20))
                {
                    timeline.paraffinCount--;
                }
                if (view.DrawButton("+", 20, 20))
                {
                    timeline.paraffinCount++;
                }

                timeline.paraffinCount = Mathf.Clamp(timeline.paraffinCount, 0, 8);

                view.margin = GUIView.defaultMargin;
            }
            view.EndLayout();

            if (timeline.paraffinCount == 0)
            {
                view.DrawLabel("エフェクトを追加してください", 200, 20);
                return;
            }

            _paraffinNameComboBox.items = paraffinJpNames;
            _paraffinNameComboBox.DrawButton("対象", view);

            var index = _paraffinNameComboBox.currentIndex;

            var paraffin = postEffectManager.GetParaffinData(index);
            if (paraffin == null)
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

            updateTransform = view.DrawToggle("有効化", paraffin.enabled, 80, 20, newValue =>
            {
                paraffin.enabled = newValue;
            });

            if (timeline.usePostEffectExtra)
            {
                updateTransform |= view.DrawColor(
                    _color1FieldValue,
                    paraffin.color1,
                    Color.white,
                    newValue => paraffin.color1 = newValue
                );

                updateTransform |= view.DrawColor(
                    _color2FieldValue,
                    paraffin.color2,
                    new Color(1f, 1f, 1f, 0f),
                    newValue => paraffin.color2 = newValue
                );
            }
            else
            {
                updateTransform |= view.DrawColor(
                    _color1FieldValue,
                    paraffin.color1,
                    Color.white,
                    newValue =>
                    {
                        var alpha2 = paraffin.color2.a;
                        paraffin.color1 = newValue;
                        paraffin.color2 = newValue;
                        paraffin.color2.a = alpha2;
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
                    value = paraffin.color2.a,
                    onChanged = newValue => paraffin.color2.a = newValue,
                });
            }

            updateTransform |= view.DrawSliderValue(new GUIView.SliderOption
            {
                label = "X",
                labelWidth = 30,
                min = -1f,
                max = 2f,
                step = 0.01f,
                defaultValue = 0.5f,
                value = paraffin.centerPosition.x,
                onChanged = newValue => paraffin.centerPosition.x = newValue,
            });

            updateTransform |= view.DrawSliderValue(new GUIView.SliderOption
            {
                label = "Y",
                labelWidth = 30,
                min = -1f,
                max = 2f,
                step = 0.01f,
                defaultValue = 0.5f,
                value = paraffin.centerPosition.y,
                onChanged = newValue => paraffin.centerPosition.y = newValue,
            });
            
            updateTransform |= view.DrawSliderValue(new GUIView.SliderOption
            {
                label = "外側半径",
                labelWidth = 30,
                min = 0f,
                max = 1f,
                step = 0.01f,
                defaultValue = 1f,
                value = paraffin.radiusFar,
                onChanged = newValue => paraffin.radiusFar = newValue,
            });

            updateTransform |= view.DrawSliderValue(new GUIView.SliderOption
            {
                label = "内側半径",
                labelWidth = 30,
                min = 0f,
                max = 1f,
                step = 0.01f,
                defaultValue = 0f,
                value = paraffin.radiusNear,
                onChanged = newValue => paraffin.radiusNear = newValue,
            });

            updateTransform |= view.DrawSliderValue(new GUIView.SliderOption
            {
                label = "SX",
                labelWidth = 30,
                min = 0f,
                max = 5f,
                step = 0.01f,
                defaultValue = 1f,
                value = paraffin.radiusScale.x,
                onChanged = newValue => paraffin.radiusScale.x = newValue,
            });

            updateTransform |= view.DrawSliderValue(new GUIView.SliderOption
            {
                label = "SY",
                labelWidth = 30,
                min = 0f,
                max = 5f,
                step = 0.01f,
                defaultValue = 1f,
                value = paraffin.radiusScale.y,
                onChanged = newValue => paraffin.radiusScale.y = newValue,
            });

            updateTransform |= view.DrawSliderValue(new GUIView.SliderOption
            {
                label = "最小深度",
                labelWidth = 30,
                min = 0f,
                max = paraffin.depthMax,
                step = 0.1f,
                defaultValue = 0f,
                value = paraffin.depthMin,
                onChanged = newValue => paraffin.depthMin = newValue,
            });

            updateTransform |= view.DrawSliderValue(new GUIView.SliderOption
            {
                label = "最大深度",
                labelWidth = 30,
                min = paraffin.depthMin,
                max = 100f,
                step = 0.1f,
                defaultValue = 0f,
                value = paraffin.depthMax,
                onChanged = newValue => paraffin.depthMax = newValue,
            });

            updateTransform |= view.DrawSliderValue(new GUIView.SliderOption
            {
                label = "深度幅",
                labelWidth = 30,
                min = 0f,
                max = 10f,
                step = 0.01f,
                defaultValue = 0f,
                value = paraffin.depthFade,
                onChanged = newValue => paraffin.depthFade = newValue,
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
                value = paraffin.useNormal,
                onChanged = newValue => paraffin.useNormal = newValue,
            });

            updateTransform |= view.DrawSliderValue(new GUIView.SliderOption
            {
                label = "加算",
                labelWidth = 30,
                min = 0f,
                max = 2f,
                step = 0.01f,
                defaultValue = 0f,
                value = paraffin.useAdd,
                onChanged = newValue => paraffin.useAdd = newValue,
            });

            updateTransform |= view.DrawSliderValue(new GUIView.SliderOption
            {
                label = "乗算",
                labelWidth = 30,
                min = 0f,
                max = 2f,
                step = 0.01f,
                defaultValue = 0f,
                value = paraffin.useMultiply,
                onChanged = newValue => paraffin.useMultiply = newValue,
            });

            updateTransform |= view.DrawSliderValue(new GUIView.SliderOption
            {
                label = "Overlay",
                labelWidth = 30,
                min = 0f,
                max = 2f,
                step = 0.01f,
                defaultValue = 0f,
                value = paraffin.useOverlay,
                onChanged = newValue => paraffin.useOverlay = newValue,
            });

            updateTransform |= view.DrawSliderValue(new GUIView.SliderOption
            {
                label = "減算",
                labelWidth = 30,
                min = 0f,
                max = 2f,
                step = 0.01f,
                defaultValue = 0f,
                value = paraffin.useSubstruct,
                onChanged = newValue => paraffin.useSubstruct = newValue,
            });

            view.DrawHorizontalLine(Color.gray);

            _copyToParaffinComboBox.items = _paraffinJpNames;
            _copyToParaffinComboBox.DrawButton("コピー先", view);

            if (view.DrawButton("コピー", 60, 20))
            {
                var copyToIndex = _copyToParaffinComboBox.currentIndex;
                if (copyToIndex != -1 && copyToIndex != index)
                {
                    postEffectManager.ApplyParaffin(copyToIndex, paraffin);
                }
            }

            view.SetEnabled(!view.IsComboBoxFocused());

            view.DrawHorizontalLine(Color.gray);

            view.DrawLabel("共通設定", 100, 20);

            updateTransform |= view.DrawToggle("デバッグ表示", config.paraffinDebug, 150, 20, newValue =>
            {
                config.paraffinDebug = newValue;
                config.dirty = true;
            });

            if (updateTransform)
            {
                postEffectManager.ApplyParaffin(index, paraffin);
            }

            view.SetEnabled(!view.IsComboBoxFocused());
            view.EndScrollView();
        }

        public static bool IsValidParaffinIndex(int index)
        {
            if (index < 0 || index >= timeline.paraffinCount)
            {
                return false;
            }

            return true;
        }

        public static string GetParaffinName(int index)
        {
            if (!IsValidParaffinIndex(index))
            {
                return "";
            }

            var suffix = PluginUtils.GetGroupSuffix(index);
            return PostEffectUtils.ToEffectName(PostEffectType.Paraffin) + suffix;
        }

        public static string GetParaffinJpName(int index)
        {
            if (!IsValidParaffinIndex(index))
            {
                return "";
            }

            var suffix = PluginUtils.GetGroupSuffix(index);
            return PostEffectUtils.ToJpName(PostEffectType.Paraffin) + suffix;
        }
    }
}