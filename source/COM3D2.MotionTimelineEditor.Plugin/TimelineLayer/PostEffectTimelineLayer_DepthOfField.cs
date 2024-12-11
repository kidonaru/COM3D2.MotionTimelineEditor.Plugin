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
            var defaultTrans = TransformDataDepthOfField.defaultTrans;

            view.DrawToggle("有効化", depthOfField.enabled, 80, 20, newValue =>
            {
                depthOfField.enabled = newValue;
                updateTransform = true;
            });

            updateTransform |= view.DrawCustomValueFloat(
                defaultTrans.focalLengthInfo,
                depthOfField.focalLength,
                newValue => depthOfField.focalLength = newValue);

            updateTransform |= view.DrawCustomValueFloat(
                defaultTrans.focalSizeInfo,
                depthOfField.focalSize,
                newValue => depthOfField.focalSize = newValue);
            
            updateTransform |= view.DrawCustomValueFloat(
                defaultTrans.apertureInfo,
                depthOfField.aperture,
                newValue => depthOfField.aperture = newValue);
            
            updateTransform |= view.DrawCustomValueFloat(
                defaultTrans.maxBlurSizeInfo,
                depthOfField.maxBlurSize,
                newValue => depthOfField.maxBlurSize = newValue);

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