using System;
using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    public partial class PostEffectTimelineLayer : TimelineLayerBase
    {
        private void ApplyDistanceFog(MotionData motion, float t)
        {
            var start = motion.start as TransformDataDistanceFog;
            var end = motion.end as TransformDataDistanceFog;

            float easingTime = CalcEasingValue(t, motion.easing);
            var distanceFog = DistanceFogData.Lerp(start.distanceFog, end.distanceFog, easingTime);

            var index = start.index;
            postEffectManager.ApplyDistanceFog(index, distanceFog);
        }

        private List<string> _distanceFogNames = new List<string>();
        private List<string> distanceFogNames
        {
            get
            {
                var distanceFogCount = timeline.distanceFogCount;
                if (_distanceFogNames.Count != distanceFogCount)
                {
                    _distanceFogNames.Clear();
                    for (var i = 0; i < distanceFogCount; i++)
                    {
                        _distanceFogNames.Add(GetDistanceFogName(i));
                    }
                }

                return _distanceFogNames;
            }
        }

        private List<string> _distanceFogJpNames = new List<string>();
        private List<string> distanceFogJpNames
        {
            get
            {
                var distanceFogCount = timeline.distanceFogCount;
                if (_distanceFogJpNames.Count != distanceFogCount)
                {
                    _distanceFogJpNames.Clear();
                    for (var i = 0; i < distanceFogCount; i++)
                    {
                        _distanceFogJpNames.Add(GetDistanceFogJpName(i));
                    }
                }

                return _distanceFogJpNames;
            }
        }

        private GUIComboBox<string> _distanceFogNameComboBox = new GUIComboBox<string>
        {
            getName = (name, index) => name,
        };

        private GUIComboBox<string> _copyToDistanceFogComboBox = new GUIComboBox<string>
        {
            getName = (name, index) => name,
        };

        public void DrawDistanceFog(GUIView view)
        {
            view.SetEnabled(!view.IsComboBoxFocused());

            view.BeginHorizontal();
            {
                view.margin = 0;

                view.DrawLabel("エフェクト数", view.labelWidth, 20);

                view.DrawIntField(new GUIView.IntFieldOption
                {
                    value = timeline.distanceFogCount,
                    width = view.viewRect.width - (view.labelWidth + 40 + view.padding.x * 2),
                    height = 20,
                    onChanged = x => timeline.distanceFogCount = x,
                });

                if (view.DrawButton("-", 20, 20))
                {
                    timeline.distanceFogCount--;
                }
                if (view.DrawButton("+", 20, 20))
                {
                    timeline.distanceFogCount++;
                }

                timeline.distanceFogCount = Mathf.Clamp(timeline.distanceFogCount, 0, 4);

                view.margin = GUIView.defaultMargin;
            }
            view.EndLayout();

            if (timeline.distanceFogCount == 0)
            {
                view.DrawLabel("エフェクトを追加してください", 200, 20);
                return;
            }

            _distanceFogNameComboBox.items = distanceFogJpNames;
            _distanceFogNameComboBox.DrawButton("対象", view);

            var index = _distanceFogNameComboBox.currentIndex;

            var distanceFog = postEffectManager.GetDistanceFogData(index);
            if (distanceFog == null)
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
            var defaultTrans = TransformDataDistanceFog.defaultTrans;

            updateTransform = view.DrawToggle("有効化", distanceFog.enabled, 80, 20, newValue =>
            {
                distanceFog.enabled = newValue;
            });

            if (timeline.usePostEffectExtraColor)
            {
                updateTransform |= view.DrawColor(
                    _color1FieldValue,
                    distanceFog.color1,
                    defaultTrans.initialColor,
                    newValue => distanceFog.color1 = newValue
                );

                updateTransform |= view.DrawColor(
                    _color2FieldValue,
                    distanceFog.color2,
                    defaultTrans.initialSubColor,
                    newValue => distanceFog.color2 = newValue
                );
            }
            else
            {
                updateTransform |= view.DrawColor(
                    _color1FieldValue,
                    distanceFog.color1,
                    defaultTrans.initialColor,
                    newValue =>
                    {
                        var alpha2 = distanceFog.color2.a;
                        distanceFog.color1 = newValue;
                        distanceFog.color2 = newValue;
                        distanceFog.color2.a = alpha2;
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
                    value = distanceFog.color2.a,
                    onChanged = newValue => distanceFog.color2.a = newValue,
                });
            }

            updateTransform |= view.DrawCustomValueFloat(
                defaultTrans.fogStartInfo,
                distanceFog.fogStart,
                newValue => distanceFog.fogStart = newValue);
            
            updateTransform |= view.DrawCustomValueFloat(
                defaultTrans.fogEndInfo,
                distanceFog.fogEnd,
                newValue => distanceFog.fogEnd = newValue);
            
            updateTransform |= view.DrawCustomValueFloat(
                defaultTrans.fogExpInfo,
                distanceFog.fogExp,
                newValue => distanceFog.fogExp = newValue);

            view.DrawLabel("ブレンドモード", 100, 20);

            if (timeline.usePostEffectExtraBlend)
            {
                updateTransform |= view.DrawCustomValueFloat(
                    defaultTrans.useNormalInfo,
                    distanceFog.useNormal,
                    newValue => distanceFog.useNormal = newValue);

                updateTransform |= view.DrawCustomValueFloat(
                    defaultTrans.useAddInfo,
                    distanceFog.useAdd,
                    newValue => distanceFog.useAdd = newValue);

                updateTransform |= view.DrawCustomValueFloat(
                    defaultTrans.useMultiplyInfo,
                    distanceFog.useMultiply,
                    newValue => distanceFog.useMultiply = newValue);

                updateTransform |= view.DrawCustomValueFloat(
                    defaultTrans.useOverlayInfo,
                    distanceFog.useOverlay,
                    newValue => distanceFog.useOverlay = newValue);

                updateTransform |= view.DrawCustomValueFloat(
                    defaultTrans.useSubstructInfo,
                    distanceFog.useSubstruct,
                    newValue => distanceFog.useSubstruct = newValue);
            }
            else
            {
                updateTransform |= view.DrawCustomValueFloat(
                    defaultTrans.useNormalInfo,
                    distanceFog.useNormal,
                    newValue => distanceFog.useNormal = newValue);
            }

            view.DrawHorizontalLine(Color.gray);

            _copyToDistanceFogComboBox.items = _distanceFogJpNames;
            _copyToDistanceFogComboBox.DrawButton("コピー先", view);

            if (view.DrawButton("コピー", 60, 20))
            {
                var copyToIndex = _copyToDistanceFogComboBox.currentIndex;
                if (copyToIndex != -1 && copyToIndex != index)
                {
                    postEffectManager.ApplyDistanceFog(copyToIndex, distanceFog);
                }
            }

            view.SetEnabled(!view.IsComboBoxFocused());

            view.DrawHorizontalLine(Color.gray);

            view.DrawLabel("共通設定", 100, 20);

            updateTransform |= view.DrawToggle("デバッグ表示", config.distanceFogDebug, 150, 20, newValue =>
            {
                config.distanceFogDebug = newValue;
                config.dirty = true;
            });

            if (updateTransform)
            {
                postEffectManager.ApplyDistanceFog(index, distanceFog);
            }

            view.SetEnabled(!view.IsComboBoxFocused());
            view.EndScrollView();
        }

        public static bool IsValidDistanceFogIndex(int index)
        {
            if (index < 0 || index >= timeline.distanceFogCount)
            {
                return false;
            }

            return true;
        }

        public static string GetDistanceFogName(int index)
        {
            if (!IsValidDistanceFogIndex(index))
            {
                return "";
            }

            var suffix = PluginUtils.GetGroupSuffix(index);
            return PostEffectUtils.ToEffectName(PostEffectType.DistanceFog) + suffix;
        }

        public static string GetDistanceFogJpName(int index)
        {
            if (!IsValidDistanceFogIndex(index))
            {
                return "";
            }

            var suffix = PluginUtils.GetGroupSuffix(index);
            return PostEffectUtils.ToJpName(PostEffectType.DistanceFog) + suffix;
        }
    }
}