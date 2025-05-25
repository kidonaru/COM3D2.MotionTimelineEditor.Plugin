using System;
using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    // GT Tonemap
    // Copyright(c) 2017 by Hajime UCHIMURA @ Polyphony Digital Inc.
    // https://www.desmos.com/calculator/gslcdxvipg?lang=ja
    // https://www.slideshare.net/nikuque/hdr-theory-and-practicce-jp
    public static class GTToneMap
    {
        private static float W(float x, float e0, float e1)
        {
            if (x <= e0) return 0;
            if (x >= e1) return 1;

            float a = (x - e0) / (e1 - e0);
            return a * a * (3 - 2 * a);
        }

        private static float H(float x, float e0, float e1)
        {
            if (x <= e0) return 0;
            if (x >= e1) return 1;
            return (x - e0) / (e1 - e0);
        }

        public static float Calc(
            float x,
            float maxBrightness = 1.0f,
            float contrast = 1.0f,
            float linearStart = 0.22f,
            float linearLength = 0.4f,
            float blackTightness = 1.33f,
            float blackOffset = 0.0f)
        {
            float P = maxBrightness;
            float a = contrast;
            float m = linearStart;
            float l = linearLength;
            float c = blackTightness;
            float b = blackOffset;

            float l0 = (P - m) * l / a;
            float L0 = m - m / a;
            float L1 = m + (1 - m) / a;
            float L_x = m + a * (x - m);
            float T_x = m * Mathf.Pow(x / m, c) + b;
            float S0 = m + l0;
            float S1 = m + a * l0;
            float C2 = a * P / (P - S1);
            float e = 2.71828f;
            float S_x = P - (P - S1) * Mathf.Pow(e, -(C2 * (x - S0) / P));
            float w0_x = 1 - W(x, 0, m);
            float w2_x = H(x, m + l0, m + l0);
            float w1_x = 1 - w0_x - w2_x;
            float f_x = T_x * w0_x + L_x * w1_x + S_x * w2_x;
            return f_x;
        }

        public static void ApplyTexture(
            Texture2D texture,
            Color lineColor,
            int lineWidth,
            float maxBrightness = 1.0f,
            float contrast = 1.0f,
            float linearStart = 0.22f,
            float linearLength = 0.4f,
            float blackTightness = 1.33f,
            float blackOffset = 0.0f)
        {
            if (lineColor == default)
            {
                lineColor = Color.white;
            }

            var width = texture.width;
            var height = texture.height;
            var halfLineWidth = lineWidth / 2;

            // トーンマッピング曲線を描画
            for (int x = 0; x < width; x++)
            {
                float t = x / (float)width;
                float value = Calc(t, maxBrightness, contrast, linearStart, linearLength, blackTightness, blackOffset);
                int y = (int)(value * height);

                y -= halfLineWidth;
                for (int i = 0; i < lineWidth; i++)
                {
                    var yy = Mathf.Clamp(y + i, 0, height - 1);
                    texture.SetPixel(x, yy, lineColor);
                }
            }

            texture.Apply();
        }
    }

    public partial class PostEffectTimelineLayer : TimelineLayerBase
    {
        private Texture2D _gtToneMapTexture;
        private GTToneMapData _gtToneMapTextureData;

        private void ApplyGTToneMap(MotionData motion, float t)
        {
            var start = motion.start as TransformDataGTToneMap;
            var end = motion.end as TransformDataGTToneMap;

            float easingTime = CalcEasingValue(t, motion.easing);
            var data = GTToneMapData.Lerp(start.data, end.data, easingTime);

            postEffectManager.ApplyGTToneMap(data);
        }

        public void DrawGTToneMap(GUIView view)
        {
            view.SetEnabled(!view.IsComboBoxFocused());
            view.DrawHorizontalLine(Color.gray);
            view.AddSpace(5);

            view.SetEnabled(!view.IsComboBoxFocused() && studioHackManager.isPoseEditing);

            var data = postEffectManager.GetGTToneMapData();
            var updateTransform = false;
            var defaultTrans = TransformDataGTToneMap.defaultTrans;

            view.DrawToggle("有効化", data.enabled, 80, 20, newValue =>
            {
                data.enabled = newValue;
                updateTransform = true;
            });

            updateTransform |= view.DrawCustomValueFloat(
                defaultTrans.maxBrightnessInfo,
                data.maxBrightness,
                newValue => data.maxBrightness = newValue);

            updateTransform |= view.DrawCustomValueFloat(
                defaultTrans.contrastInfo,
                data.contrast,
                newValue => data.contrast = newValue);

            updateTransform |= view.DrawCustomValueFloat(
                defaultTrans.linearStartInfo,
                data.linearStart,
                newValue => data.linearStart = newValue);

            updateTransform |= view.DrawCustomValueFloat(
                defaultTrans.linearLengthInfo,
                data.linearLength,
                newValue => data.linearLength = newValue);

            updateTransform |= view.DrawCustomValueFloat(
                defaultTrans.blackTightnessInfo,
                data.blackTightness,
                newValue => data.blackTightness = newValue);

            updateTransform |= view.DrawCustomValueFloat(
                defaultTrans.blackOffsetInfo,
                data.blackOffset,
                newValue => data.blackOffset = newValue);

            if (updateTransform)
            {
                postEffectManager.ApplyGTToneMap(data);
            }

            view.SetEnabled(!view.IsComboBoxFocused());
            view.DrawHorizontalLine(Color.gray);

            if (_gtToneMapTexture == null)
            {
                _gtToneMapTexture = new Texture2D(150, 150);
                TextureUtils.ClearTexture(_gtToneMapTexture, config.curveBgColor);
            }

            if (!_gtToneMapTextureData.Equals(data))
            {
                _gtToneMapTextureData = data;

                TextureUtils.ClearTexture(_gtToneMapTexture, config.curveBgColor);

                GTToneMap.ApplyTexture(
                    _gtToneMapTexture,
                    config.curveLineColor,
                    1,
                    data.maxBrightness,
                    data.contrast,
                    data.linearStart,
                    data.linearLength,
                    data.blackTightness,
                    data.blackOffset);
            }

            view.DrawTexture(_gtToneMapTexture);
        }
    }
}