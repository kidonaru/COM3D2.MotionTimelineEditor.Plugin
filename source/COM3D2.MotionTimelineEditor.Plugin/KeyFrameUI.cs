using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    public class KeyFrameUI : SubWindowUIBase
    {
        public override string title
        {
            get
            {
                return "キーフレーム 詳細";
            }
        }

        private GUIView mainView = new GUIView(0, 20, WINDOW_WIDTH, WINDOW_HEIGHT);
        private Texture2D tangentTex = null;
        private TangentValueType tangentValueType = TangentValueType.All;
        private HashSet<TangentPair> cachedTangents = new HashSet<TangentPair>();
        private Texture2D[] tangentPresetTextures = null;

        private Texture2D easingTex = null;
        private HashSet<int> cachedEasings = new HashSet<int>();
        private ComboBoxValue<int> easingComboBox = null;

        private FloatFieldValue[] posFieldValues = FloatFieldValue.CreateArray(
            new string[] { "X", "Y", "Z" }
        );
        private FloatFieldValue[] angleFieldValues = FloatFieldValue.CreateArray(
            new string[] { "RX", "RY", "RZ" }
        );
        private FloatFieldValue[] scaleFieldValues = FloatFieldValue.CreateArray(
            new string[] { "SX", "SY", "SZ" }
        );
        private FloatFieldValue[] colorFieldValues = FloatFieldValue.CreateArray(
            new string[] { "R", "G", "B", "A" }
        );
        private FloatFieldValue outTangentFieldValue = new FloatFieldValue();
        private FloatFieldValue inTangentFieldValue = new FloatFieldValue();
        private FloatFieldValue[] customFieldValues = FloatFieldValue.CreateArray(8);

        private static HashSet<BoneData> selectedBones
        {
            get
            {
                return TimelineManager.instance.selectedBones;
            }
        }

        public override void OnOpen()
        {
            if (tangentPresetTextures == null)
            {
                PluginUtils.LogDebug("補間曲線プリセット画像を生成します");

                tangentPresetTextures = new Texture2D[(int) TangentType.Smooth];

                for (var i = 0; i < tangentPresetTextures.Length; i++)
                {
                    var tangentType = (TangentType) i;
                    var texture = new Texture2D(40, 40);
                    var tangentPair = TangentPair.GetDefault(tangentType);

                    tangentPresetTextures[i] = texture;
                    TextureUtils.ClearTexture(texture, config.curveBgColor);

                    UpdateTangentTexture(
                        texture,
                        tangentPair.outTangent,
                        tangentPair.inTangent,
                        config.curveLineColor,
                        3
                    );
                }
            }

            if (tangentTex == null)
            {
                tangentTex = new Texture2D(150, 150);

                TextureUtils.ClearTexture(tangentTex, config.curveBgColor);
            }

            if (easingTex == null)
            {
                easingTex = new Texture2D(150, 150);

                TextureUtils.ClearTexture(easingTex, config.curveBgColor);
            }

            if (easingComboBox == null)
            {
                easingComboBox = new ComboBoxValue<int>
                {
                    items = Enumerable.Range(0, (int)MoveEasingType.Max).ToList(),
                    getName = (type, index) => ((MoveEasingType)type).ToString()
                };
            }
        }

        public override void DrawWindow(int id)
        {
            var view = mainView;
            view.SetEnabled(!easingComboBox.focused);
            view.ResetLayout();

            if (selectedBones.Count == 0)
            {
                view.DrawLabel("キーフレームが選択されていません", -1, 20);
            }
            else
            {
                DrawTransform(view);
                DrawCustomValue(view);

                if (view.DrawButton("初期化", 80, 20))
                {
                    foreach (var selectedBone in selectedBones)
                    {
                        selectedBone.transform.Reset();
                    }

                    PluginUtils.LogDebug("初期化します");
                    currentLayer.ApplyCurrentFrame(true);
                }

                view.DrawHorizontalLine(Color.gray);

                DrawTangent(view);
                DrawEasing(view);
                DrawComboBox(view);
            }

            GUI.DragWindow();
        }

        private void DrawTransform(GUIView view)
        {
            DrawVector3(
                view,
                posFieldValues,
                0.01f,
                0.1f,
                transform => transform.initialPosition,
                transform => transform.hasPosition,
                transform => transform.position,
                (transform, pos) => transform.position = pos
            );
            DrawVector3(
                view,
                angleFieldValues,
                1f,
                10f,
                transform => transform.initialEulerAngles,
                transform => transform.hasRotation || transform.hasEulerAngles,
                transform => transform.eulerAngles,
                (transform, angle) => transform.eulerAngles = angle
            );
            DrawVector3(
                view,
                scaleFieldValues,
                0.01f,
                0.1f,
                transform => transform.initialScale,
                transform => transform.hasScale,
                transform => transform.scale,
                (transform, scale) => transform.scale = scale
            );
            DrawVector3(
                view,
                colorFieldValues,
                0.01f,
                0.1f,
                transform => transform.initialColor.ToVector3(),
                transform => transform.hasColor,
                transform => transform.color.ToVector3(),
                (transform, color) => transform.color = color.ToColor()
            );
        }

        private void DrawCustomValue(GUIView view)
        {
            var firstBone = selectedBones.First();
            var customNames = firstBone.transform.GetCustomValueIndexMap().Keys;

            int index = 0;
            foreach (var customName in customNames)
            {
                if (index >= customFieldValues.Length)
                {
                    break;
                }
                var fieldValue = customFieldValues[index];
                fieldValue.label = customName;

                var value = firstBone.transform.GetCustomValue(customName);

                DrawValue(
                    view,
                    fieldValue,
                    0.01f,
                    0.1f,
                    transform => transform.GetInitialCustomValue(customName),
                    transform => transform.HasCustomValue(customName),
                    transform => transform.GetCustomValue(customName).value,
                    (transform, newValue) => transform.GetCustomValue(customName).value = newValue
                );

                index++;
            }
        }

        private void DrawVector3(
            GUIView view,
            FloatFieldValue[] fieldValues,
            float addedValue1,
            float addedValue2,
            Func<ITransformData, Vector3> getResetValue,
            Func<ITransformData, bool> hasValue,
            Func<ITransformData, Vector3> getValue,
            Action<ITransformData, Vector3> setValue)
        {
            var values = Vector3.zero;
            var boneCount = 0;

            foreach (var bone in selectedBones)
            {
                var transform = bone.transform;
                int nanCount = 0;

                if (hasValue(transform))
                {
                    var pos = getValue(transform);
                    if (boneCount == 0)
                    {
                        values = pos;
                    }
                    else
                    {
                        for (var i = 0; i < 3; i++)
                        {
                            if (pos[i] != values[i])
                            {
                                values[i] = float.NaN;
                                nanCount++;
                            }
                        }
                    }

                    boneCount++;
                    if (boneCount >= config.detailTransformCount || nanCount == 3)
                    {
                        break;
                    }
                }
            }

            if (boneCount == 0)
            {
                return;
            }

            var diffValues = Vector3.zero;
            var newValues = values;
            int resetIndex = -1;

            for (var i = 0; i < 3; i++)
            {
                view.DrawValue(
                    fieldValues[i],
                    addedValue1,
                    addedValue2,
                    () => resetIndex = i,
                    values[i],
                    newValue => newValues[i] = newValue,
                    diffValue => diffValues[i] = diffValue
                );
            }

            // リセット
            if (resetIndex >= 0)
            {
                foreach (var selectedBone in selectedBones)
                {
                    var transform = selectedBone.transform;
                    if (hasValue(transform))
                    {
                        var pos = getValue(transform);
                        pos[resetIndex] = getResetValue(transform)[resetIndex];
                        setValue(transform, pos);
                    }
                }

                PluginUtils.LogDebug("リセットします");
                currentLayer.ApplyCurrentFrame(true);
            }

            // 差分の適用
            for (var i = 0; i < 3; i++)
            {
                var diffValue = diffValues[i];
                if (diffValue == 0f)
                {
                    continue;
                }

                foreach (var selectedBone in selectedBones)
                {
                    var transform = selectedBone.transform;
                    if (hasValue(transform))
                    {
                        var pos = getValue(transform);
                        pos[i] += diffValue;
                        setValue(transform, pos);;
                    }
                }

                PluginUtils.LogDebug("差分を適用します：" + diffValue);
                currentLayer.ApplyCurrentFrame(true);
            }

            // 新値の適用
            for (var i = 0; i < 3; i++)
            {
                var newValue = newValues[i];
                if (float.IsNaN(newValue) || newValue == values[i])
                {
                    continue;
                }

                foreach (var selectedBone in selectedBones)
                {
                    var transform = selectedBone.transform;
                    if (hasValue(transform))
                    {
                        var pos = getValue(transform);
                        pos[i] = newValue;
                        setValue(transform, pos);
                    }
                }

                PluginUtils.LogDebug("新値を適用します：" + newValue);
                currentLayer.ApplyCurrentFrame(true);
            }
        }

        private void DrawValue(
            GUIView view,
            FloatFieldValue fieldValue,
            float addedValue1,
            float addedValue2,
            Func<ITransformData, float> getResetValue,
            Func<ITransformData, bool> hasValue,
            Func<ITransformData, float> getValue,
            Action<ITransformData, float> setValue)
        {
            var value = 0f;
            var boneCount = 0;

            foreach (var bone in selectedBones)
            {
                var transform = bone.transform;
                var isNan = false;

                if (hasValue(transform))
                {
                    var _value = getValue(transform);
                    if (boneCount == 0)
                    {
                        value = _value;
                    }
                    else
                    {
                        if (_value != value)
                        {
                            value = float.NaN;
                            isNan = true;
                        }
                    }

                    boneCount++;
                    if (boneCount >= config.detailTransformCount || isNan)
                    {
                        break;
                    }
                }
            }

            if (boneCount == 0)
            {
                return;
            }

            var diffValue = 0f;
            var newValue = value;
            bool isReset = false;

            view.DrawValue(
                fieldValue,
                addedValue1,
                addedValue2,
                () => isReset = true,
                value,
                _newValue => newValue = _newValue,
                _diffValue => diffValue = _diffValue
            );

            // リセット
            if (isReset)
            {
                foreach (var selectedBone in selectedBones)
                {
                    var transform = selectedBone.transform;
                    if (hasValue(transform))
                    {
                        setValue(transform, getResetValue(transform));
                    }
                }

                PluginUtils.LogDebug("リセットします");
                currentLayer.ApplyCurrentFrame(true);
            }

            // 差分の適用
            if (diffValue != 0f)
            {
                foreach (var selectedBone in selectedBones)
                {
                    var transform = selectedBone.transform;
                    if (hasValue(transform))
                    {
                        var _value = getValue(transform);
                        _value += diffValue;
                        setValue(transform, _value);
                    }
                }

                PluginUtils.LogDebug("差分を適用します：" + diffValue);
                currentLayer.ApplyCurrentFrame(true);
            }

            // 新値の適用
            if (!float.IsNaN(newValue) && newValue != value)
            {
                foreach (var selectedBone in selectedBones)
                {
                    var transform = selectedBone.transform;
                    if (hasValue(transform))
                    {
                        setValue(transform, newValue);
                    }
                }

                PluginUtils.LogDebug("新値を適用します：" + newValue);
                currentLayer.ApplyCurrentFrame(true);
            }
        }

        private void DrawTangent(GUIView view)
        {
            var tangents = new HashSet<TangentPair>();

            bool hasTangent = false;

            foreach (var bone in selectedBones)
            {
                if (!bone.transform.hasTangent)
                {
                    continue;
                }

                hasTangent = true;

                var prevBone = currentLayer.GetPrevBone(bone);
                if (prevBone == null)
                {
                    continue;
                }
                var outTangentDataList = prevBone.transform.GetOutTangentDataList(tangentValueType);
                var inTangentDataList = bone.transform.GetInTangentDataList(tangentValueType);

                for (var i = 0; i < outTangentDataList.Length && i < inTangentDataList.Length; i++)
                {
                    var outTangentData = outTangentDataList[i];
                    var inTangentData = inTangentDataList[i];

                    tangents.Add(new TangentPair
                    {
                        outTangent = outTangentData.normalizedValue,
                        inTangent = inTangentData.normalizedValue,
                        isSmooth = outTangentData.isSmooth && inTangentData.isSmooth,
                    });

                    if (tangents.Count >= config.detailTangentCount)
                    {
                        break;
                    }
                }

                if (tangents.Count >= config.detailTangentCount)
                {
                    break;
                }
            }

            if (!hasTangent)
            {
                return;
            }

            bool isUpdated = false;

            if (cachedTangents.Count != tangents.Count)
            {
                isUpdated = true;
            }
            else
            {
                foreach (var tangent in cachedTangents)
                {
                    if (!tangents.Contains(tangent))
                    {
                        isUpdated = true;
                        break;
                    }
                }
            }

            if (isUpdated)
            {
                PluginUtils.LogDebug("補間曲線画像を更新します：" + tangents.Count);

                cachedTangents = tangents;

                TextureUtils.ClearTexture(tangentTex, config.curveBgColor);

                foreach (var tangent in tangents)
                {
                    var color = tangent.isSmooth ? config.curveLineSmoothColor : config.curveLineColor;
                    UpdateTangentTexture(
                        tangentTex,
                        tangent.outTangent,
                        tangent.inTangent,
                        color,
                        1
                    );
                }
            }

            Action<Action<TangentData>> foreachOutTangent = (callback) =>
            {
                foreach (var prevBone in currentLayer.GetPrevBones(selectedBones))
                {
                    var outTangentDataList = prevBone.transform.GetOutTangentDataList(tangentValueType);
                    foreach (var outTangentData in outTangentDataList)
                    {
                        callback(outTangentData);
                    }
                }
            };

            Action<Action<TangentData>> foreachInTangent = (callback) =>
            {
                foreach (var selectedBone in selectedBones)
                {
                    var inTangentDataList = selectedBone.transform.GetInTangentDataList(tangentValueType);
                    foreach (var inTangentData in inTangentDataList)
                    {
                        callback(inTangentData);
                    }
                }
            };

            view.AddSpace(10);

            view.DrawLabel("補間曲線", 100, 20);

            {
                var subView = new GUIView(
                    tangentTex.width + 10,
                    view.currentPos.y + 20,
                    WINDOW_WIDTH - tangentTex.width,
                    200);

                tangentValueType = (TangentValueType)subView.DrawSelectList(
                    TangentData.TangentValueTypeNames,
                    (name, index) => name,
                    100, 20, (int)tangentValueType);

                float outTangent = float.NaN;
                float inTangent = float.NaN;
                bool includeOutTangent = false;
                bool includeInTangent = false;

                foreach (var tangent in tangents)
                {
                    if (!includeOutTangent)
                    {
                        outTangent = tangent.outTangent;
                        includeOutTangent = true;
                    }
                    else if (outTangent != tangent.outTangent)
                    {
                        outTangent = float.NaN;
                    }

                    if (!includeInTangent)
                    {
                        inTangent = tangent.inTangent;
                        includeInTangent = true;
                    }
                    else if (inTangent != tangent.inTangent)
                    {
                        inTangent = float.NaN;
                    }
                }

                outTangentFieldValue.UpdateValue(outTangent, true);
                inTangentFieldValue.UpdateValue(inTangent, true);

                subView.DrawLabel("OutTangent", 100, 20);

                subView.BeginLayout(GUIView.LayoutDirection.Horizontal);

                if (!includeOutTangent)
                {
                    GUI.enabled = false;
                }

                var diffOutTangent = 0f;

                if (subView.DrawButton("<", 20, 20))
                {
                    diffOutTangent = -0.1f;
                }

                var newOutTangent = subView.DrawFloatFieldValue(outTangentFieldValue, 50, 20);

                if (subView.DrawButton(">", 20, 20))
                {
                    diffOutTangent = 0.1f;
                }

                GUI.enabled = true;

                subView.EndLayout();

                subView.DrawLabel("InTangent", 100, 20);

                subView.BeginLayout(GUIView.LayoutDirection.Horizontal);

                if (!includeInTangent)
                {
                    GUI.enabled = false;
                }

                var diffInTangent = 0f;

                if (subView.DrawButton("<", 20, 20))
                {
                    diffInTangent = -0.1f;
                }

                var newInTangent = subView.DrawFloatFieldValue(inTangentFieldValue, 50, 20);

                if (subView.DrawButton(">", 20, 20))
                {
                    diffInTangent = 0.1f;
                }

                GUI.enabled = true;

                subView.EndLayout();

                // 新値の適用
                if (!float.IsNaN(newOutTangent) && newOutTangent != outTangent)
                {
                    foreachOutTangent((outTangentData) =>
                    {
                        outTangentData.normalizedValue = newOutTangent;
                        outTangentData.isSmooth = false;
                    });

                    PluginUtils.LogDebug("OutTangentを適用します：" + newOutTangent);
                    currentLayer.ApplyCurrentFrame(true);
                }

                if (!float.IsNaN(newInTangent) && newInTangent != inTangent)
                {
                    foreachInTangent((inTangentData) =>
                    {
                        inTangentData.normalizedValue = newInTangent;
                        inTangentData.isSmooth = false;
                    });

                    PluginUtils.LogDebug("InTangentを適用します：" + newInTangent);
                    currentLayer.ApplyCurrentFrame(true);
                }

                // 差分の適用
                if (diffOutTangent != 0f)
                {
                    foreachOutTangent((outTangentData) =>
                    {
                        var value = outTangentData.normalizedValue;
                        value = value + diffOutTangent;
                        outTangentData.normalizedValue = value;
                        outTangentData.isSmooth = false;
                    });

                    PluginUtils.LogDebug("OutTangentの差分を適用します：" + diffOutTangent);
                    currentLayer.ApplyCurrentFrame(true);
                }

                if (diffInTangent != 0f)
                {
                    foreachInTangent((inTangentData) =>
                    {
                        var value = inTangentData.normalizedValue;
                        value = value + diffInTangent;
                        inTangentData.normalizedValue = value;
                        inTangentData.isSmooth = false;
                    });

                    PluginUtils.LogDebug("InTangentの差分を適用します：" + diffInTangent);
                    currentLayer.ApplyCurrentFrame(true);
                }

                var isSmooth = tangents.All(tangent => tangent.isSmooth);
                var newIsSmooth = subView.DrawToggle("自動補間", isSmooth, 100, 20);
                if (newIsSmooth != isSmooth)
                {
                    foreachOutTangent((outTangentData) =>
                    {
                        outTangentData.isSmooth = newIsSmooth;
                    });

                    foreachInTangent((inTangentData) =>
                    {
                        inTangentData.isSmooth = newIsSmooth;
                    });

                    PluginUtils.LogDebug("自動補間を適用します：" + newIsSmooth);
                    currentLayer.ApplyCurrentFrame(true);
                }
            }

            view.DrawTexture(tangentTex);

            view.DrawLabel("プリセット反映", 100, 20);

            view.BeginLayout(GUIView.LayoutDirection.Horizontal);

            for (int i = 0; i < tangentPresetTextures.Length; ++i)
            {
                var texture = tangentPresetTextures[i];
                var tangentType = (TangentType)i;

                view.DrawTexture(
                    texture,
                    texture.width,
                    texture.height,
                    Color.white,
                    EventType.MouseDown,
                    _ =>
                {
                    if (selectedBones.Count == 0)
                    {
                        return;
                    }

                    var tangentPair = TangentPair.GetDefault(tangentType);

                    foreachOutTangent((outTangentData) =>
                    {
                        outTangentData.normalizedValue = tangentPair.outTangent;
                        outTangentData.isSmooth = tangentPair.isSmooth;
                    });

                    foreachInTangent((inTangentData) =>
                    {
                        inTangentData.normalizedValue = tangentPair.inTangent;
                        inTangentData.isSmooth = tangentPair.isSmooth;
                    });

                    PluginUtils.LogDebug("プリセットを適用します：" + tangentType);
                    currentLayer.ApplyCurrentFrame(true);
                });
            }

            view.EndLayout();
        }

        private void DrawEasing(GUIView view)
        {
            var easings = new HashSet<int>();

            foreach (var bone in selectedBones)
            {
                if (!bone.transform.hasEasing)
                {
                    continue;
                }

                var easing = bone.transform.easing;
                easings.Add(easing);
            }

            bool isUpdated = false;

            if (cachedEasings.Count != easings.Count)
            {
                isUpdated = true;
            }
            else
            {
                foreach (var cachedEasing in cachedEasings)
                {
                    if (!easings.Contains(cachedEasing))
                    {
                        isUpdated = true;
                        break;
                    }
                }
            }

            if (isUpdated)
            {
                PluginUtils.LogDebug("補間曲線画像を更新します：" + easings.Count);

                cachedEasings = easings;

                TextureUtils.ClearTexture(easingTex, config.curveBgColor);

                foreach (var easing in easings)
                {
                    var color = config.curveLineColor;
                    UpdateEasingTexture(
                        easingTex,
                        easing,
                        color,
                        1
                    );
                }
            }

            if (cachedEasings.Count == 0)
            {
                easingComboBox.focused = false;
                return;
            }

            view.AddSpace(10);

            view.DrawLabel("補間曲線", 100, 20);

            Action<int> updateEasing = easing =>
            {
                var max = (int) MoveEasingType.Max;
                easing = (easing + max) % max;
 
                foreach (var bone in selectedBones)
                {
                    bone.transform.easing = easing;
                }

                PluginUtils.LogDebug("Easingを適用します: " + easing);
                currentLayer.ApplyCurrentFrame(true);
            };

            easingComboBox.onSelected = (easing, index) =>
            {
                updateEasing(easing);
            };

            {
                var subView = new GUIView(
                    easingTex.width + 10,
                    view.currentPos.y + 20,
                    WINDOW_WIDTH - easingTex.width,
                    200);

                int easing = -1;
                bool includeEasing = false;

                foreach (var _easing in easings)
                {
                    if (!includeEasing)
                    {
                        easing = _easing;
                        includeEasing = true;
                    }
                    else if (easing != _easing)
                    {
                        easing = -1;
                    }
                }

                int newEasing = easing;
                easingComboBox.currentIndex = easing;

                subView.BeginLayout(GUIView.LayoutDirection.Horizontal);
                {
                    subView.DrawLabel("Easing", 50, 20);

                    if (subView.DrawButton("<", 20, 20))
                    {
                        newEasing--;
                    }

                    if (subView.DrawButton(">", 20, 20))
                    {
                        newEasing++;
                    }
                }
                subView.EndLayout();

                // 新値の適用
                if (newEasing != easing)
                {
                    updateEasing(newEasing);
                }

                subView.DrawComboBoxButton(easingComboBox, 80, 20);
            }

            view.DrawTexture(easingTex);
        }

        private void DrawComboBox(GUIView view)
        {
            view.SetEnabled(true);

            view.DrawComboBoxContent(
                easingComboBox,
                100, 100,
                rc_stgw.width, rc_stgw.height,
                20);
        }

        // ヘルミート曲線の計算
        private static float Hermite(
            float t,
            float outTangent,
            float inTangent)
        {
            float t2 = t * t;
            float t3 = t2 * t;
            return (t3 - 2 * t2 + t) * outTangent + (-2 * t3 + 3 * t2) * 1 + (t3 - t2) * inTangent;
        }

        private static void UpdateTangentTexture(
            Texture2D texture,
            float outTangent,
            float inTangent,
            Color lineColor,
            int lineWidth)
        {
            var width = texture.width;
            var height = texture.height;
            var halfLineWidth = lineWidth / 2;

            for (int x = 0; x < width; x++)
            {
                float t = x / (float)width;
                int y = (int)(Hermite(t, outTangent, inTangent) * height);

                y -= halfLineWidth;
                for (int i = 0; i < lineWidth; i++)
                {
                    var yy = Mathf.Clamp(y + i, 0, height - 1);
                    texture.SetPixel(x, yy, lineColor);
                }
            }

            texture.Apply();
        }

        private static void UpdateEasingTexture(
            Texture2D texture,
            int easing,
            Color lineColor,
            int lineWidth)
        {
            var width = texture.width;
            var height = texture.height;
            var halfLineWidth = lineWidth / 2;

            for (int x = 0; x < width; x++)
            {
                float t = x / (float)width;
                int y = (int)(currentLayer.CalcEasingValue(t, easing) * height);

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
}