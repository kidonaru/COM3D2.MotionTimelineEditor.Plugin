using System;
using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    public partial class PostEffectTimelineLayer : TimelineLayerBase
    {
        private void ApplyDepthOfField(MotionData motion, float t)
        {
            var start = motion.start as TransformDataDepthOfField;
            var end = motion.end as TransformDataDepthOfField;

            float easingTime = CalcEasingValue(t, start.easing);
            var depthOfField = DepthOfFieldData.Lerp(start.depthOfField, end.depthOfField, easingTime);

            postEffectManager.ApplyDepthOfField(depthOfField);
        }

        public void DrawDepthOfField(GUIView view)
        {
            view.SetEnabled(!view.IsComboBoxFocused());
            view.DrawHorizontalLine(Color.gray);
            view.AddSpace(5);

            view.SetEnabled(!view.IsComboBoxFocused() && studioHack.isPoseEditing);

            var depthOfField = postEffectManager.GetDepthOfFieldData();
            var updateTransform = false;

            view.DrawToggle("有効化", depthOfField.enabled, 80, 20, newValue =>
            {
                depthOfField.enabled = newValue;
                updateTransform = true;
            });

            updateTransform |= view.DrawSliderValue(new GUIView.SliderOption
            {
                label = "焦点距離",
                labelWidth = 30,
                min = 0f,
                max = config.positionRange,
                step = 0.1f,
                defaultValue = 10f,
                value = depthOfField.focalLength,
                onChanged = newValue => depthOfField.focalLength = newValue,
            });
            
            updateTransform |= view.DrawSliderValue(new GUIView.SliderOption
            {
                label = "焦点サイズ",
                labelWidth = 30,
                min = 0f,
                max = 2f,
                step = 0.01f,
                defaultValue = 0.05f,
                value = depthOfField.focalSize,
                onChanged = newValue => depthOfField.focalSize = newValue,
            });

            updateTransform |= view.DrawSliderValue(new GUIView.SliderOption
            {
                label = "絞り値",
                labelWidth = 30,
                min = 0f,
                max = 60f,
                step = 0.1f,
                defaultValue = 11.5f,
                value = depthOfField.aperture,
                onChanged = newValue => depthOfField.aperture = newValue,
            });

            updateTransform |= view.DrawSliderValue(new GUIView.SliderOption
            {
                label = "最大ブラー",
                labelWidth = 30,
                min = 0f,
                max = 10f,
                step = 0.1f,
                defaultValue = 2f,
                value = depthOfField.maxBlurSize,
                onChanged = newValue => depthOfField.maxBlurSize = newValue,
            });

            view.BeginHorizontal();
            {
                view.DrawLabel("追従メイド", 70, 20);

                view.DrawToggle("", depthOfField.maidSlotNo >= 0, 20, 20, newValue =>
                {
                    depthOfField.maidSlotNo = newValue ? _maidComboBox.currentIndex : -1;
                    updateTransform = true;
                });

                _maidComboBox.items = maidManager.maidCaches;
                _maidComboBox.onSelected = (maidCache, index) =>
                {
                    depthOfField.maidSlotNo = index;
                    updateTransform = true;
                };
                _maidComboBox.DrawButton(view);
            }
            view.EndLayout();

            view.SetEnabled(!view.IsComboBoxFocused());

            view.DrawHorizontalLine(Color.gray);

            view.DrawLabel("共通設定", 100, 20);

            view.BeginHorizontal();
            {
                view.DrawToggle("高解像度", config.dofHighResolution, 100, 20, newValue =>
                {
                    config.dofHighResolution = newValue;
                    config.dirty = true;
                    updateTransform = true;
                });

                view.DrawToggle("近距離ブラー", config.dofNearBlur, 100, 20, newValue =>
                {
                    config.dofNearBlur = newValue;
                    config.dirty = true;
                    updateTransform = true;
                });
            }
            view.EndLayout();

            view.DrawToggle("フォーカスの可視化", config.dofVisualizeFocus, 150, 20, newValue =>
            {
                config.dofVisualizeFocus = newValue;
                config.dirty = true;
                updateTransform = true;
            });

            if (updateTransform)
            {
                postEffectManager.ApplyDepthOfField(depthOfField);
            }
        }
    }
}