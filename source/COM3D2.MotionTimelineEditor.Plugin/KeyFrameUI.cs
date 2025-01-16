using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    public class KeyFrameUI : SubWindowUIBase
    {
        public override string title => "キーフレーム 詳細";

        private Texture2D tangentTex = null;
        private TangentValueType tangentValueType = TangentValueType.すべて;
        private HashSet<TangentPair> cachedTangents = new HashSet<TangentPair>();
        private Texture2D[] tangentPresetTextures = null;

        private Texture2D easingTex = null;
        private HashSet<int> cachedEasings = new HashSet<int>();
        private GUIComboBox<int> easingComboBox = new GUIComboBox<int>
        {
            items = Enumerable.Range(0, (int)MoveEasingType.Max).ToList(),
            getName = (type, index) => ((MoveEasingType)type).ToString(),
        };

        private static HashSet<BoneData> selectedBones
        {
            get
            {
                return TimelineManager.instance.selectedBones;
            }
        }

        public KeyFrameUI(SubWindow subWindow) : base(subWindow)
        {
        }

        public override void InitWindow()
        {
            if (tangentPresetTextures == null)
            {
                MTEUtils.LogDebug("補間曲線プリセット画像を生成します");

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

            base.InitWindow();
        }

        public override void DrawContent(GUIView view)
        {
            if (timeline == null)
            {
                return;
            }

            if (selectedBones.Count == 0)
            {
                view.DrawLabel("キーフレームが選択されていません", -1, 20);
            }
            else
            {
                view.BeginScrollView();

                DrawTransform(view);
                DrawCustomValues(view);
                DrawStrValues(view);

                if (view.DrawButton("初期化", 80, 20))
                {
                    foreach (var selectedBone in selectedBones)
                    {
                        selectedBone.transform.Reset();
                    }

                    MTEUtils.LogDebug("初期化します");
                    currentLayer.ApplyCurrentFrame(true);
                }

                view.DrawHorizontalLine(Color.gray);

                DrawTangent(view);
                DrawEasing(view);

                view.EndScrollView();
            }
        }

        private void DrawTransform(GUIView view)
        {
            DrawVector3(
                view,
                new string[] { "X", "Y", "Z" },
                0.01f,
                0.1f,
                transform => transform.initialPosition,
                transform => transform.hasPosition,
                transform => transform.position,
                (transform, pos) => transform.position = pos
            );
            DrawVector3(
                view,
                new string[] { "RX", "RY", "RZ" },
                1f,
                10f,
                transform => transform.initialEulerAngles,
                transform => transform.hasRotation || transform.hasEulerAngles,
                transform => transform.normalizedEulerAngles,
                (transform, angle) => transform.eulerAngles = angle
            );
            DrawVector3(
                view,
                new string[] { "SX", "SY", "SZ" },
                0.01f,
                0.1f,
                transform => transform.initialScale,
                transform => transform.hasScale,
                transform => transform.scale,
                (transform, scale) => transform.scale = scale
            );
            DrawVector3(
                view,
                new string[] { "R", "G", "B" },
                0.01f,
                0.1f,
                transform => transform.initialColor.ToVector3(),
                transform => transform.hasColor,
                transform => transform.color.ToVector3(),
                (transform, color) => transform.color = color.ToColor()
            );
            DrawBoolValue(
                view,
                "表示",
                transform => transform.hasVisible,
                transform => transform.visible,
                (transform, visible) => transform.visible = visible
            );
        }

        private void DrawCustomValues(GUIView view)
        {
            var firstBone = selectedBones.First();
            var customKeys = firstBone.transform.GetCustomValueInfoMap().Keys;

            int index = 0;
            foreach (var customKey in customKeys)
            {
                DrawValue(
                    view,
                    firstBone.transform.GetCustomValueName(customKey),
                    0.01f,
                    0.1f,
                    transform => transform.GetDefaultCustomValue(customKey),
                    transform => transform.HasCustomValue(customKey),
                    transform => transform.GetCustomValue(customKey).value,
                    (transform, newValue) => transform.GetCustomValue(customKey).value = newValue
                );

                index++;
            }
        }

        private void DrawStrValues(GUIView view)
        {
            var firstBone = selectedBones.First();
            var customKeys = firstBone.transform.GetStrValueInfoMap().Keys;

            int index = 0;
            foreach (var customKey in customKeys)
            {
                DrawStrValue(
                    view,
                    firstBone.transform.GetStrValueName(customKey),
                    transform => transform.HasStrValue(customKey),
                    transform => transform.GetStrValue(customKey),
                    (transform, newValue) => transform.SetStrValue(customKey, newValue)
                );

                index++;
            }
        }

        private void DrawVector3(
            GUIView view,
            string[] labels,
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
                view.DrawFloatSelect(
                    labels[i],
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

                MTEUtils.LogDebug("リセットします");
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

                MTEUtils.LogDebug("差分を適用します：" + diffValue);
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

                MTEUtils.LogDebug("新値を適用します：" + newValue);
                currentLayer.ApplyCurrentFrame(true);
            }
        }

        private void DrawValue(
            GUIView view,
            string label,
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

            view.DrawFloatSelect(
                label,
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

                MTEUtils.LogDebug("リセットします");
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

                MTEUtils.LogDebug("差分を適用します：" + diffValue);
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

                MTEUtils.LogDebug("新値を適用します：" + newValue);
                currentLayer.ApplyCurrentFrame(true);
            }
        }

        private void DrawBoolValue(
            GUIView view,
            string label,
            Func<ITransformData, bool> hasValue,
            Func<ITransformData, bool> getValue,
            Action<ITransformData, bool> setValue)
        {
            bool value = false;
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
                            value = false;
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

            var newValue = value;

            view.DrawToggle(
                label,
                value,
                100,
                20,
                _newValue => newValue = _newValue);

            // 新値の適用
            if (newValue != value)
            {
                foreach (var selectedBone in selectedBones)
                {
                    var transform = selectedBone.transform;
                    if (hasValue(transform))
                    {
                        setValue(transform, newValue);
                    }
                }

                MTEUtils.LogDebug("新値を適用します：" + newValue);
                currentLayer.ApplyCurrentFrame(true);
            }
        }

        private void DrawStrValue(
            GUIView view,
            string label,
            Func<ITransformData, bool> hasValue,
            Func<ITransformData, string> getValue,
            Action<ITransformData, string> setValue)
        {
            var value = "";
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
                            value = "";
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

            var newValue = value;

            view.DrawTextField(
                label,
                0,
                value,
                -1,
                20,
                 _newValue => newValue = _newValue);

            // 新値の適用
            if (newValue != value)
            {
                foreach (var selectedBone in selectedBones)
                {
                    var transform = selectedBone.transform;
                    if (hasValue(transform))
                    {
                        setValue(transform, newValue);
                    }
                }

                MTEUtils.LogDebug("新値を適用します：" + newValue);
                currentLayer.ApplyCurrentFrame(true);
            }
        }

        private GUIComboBox<TangentValueType> _tangentValueTypeComboBox = new GUIComboBox<TangentValueType>
        {
            items = Enum.GetValues(typeof(TangentValueType)).Cast<TangentValueType>().ToList(),
            getName = (type, index) => type.ToString(),
            buttonSize = new Vector2(50, 20),
        }; 

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
                MTEUtils.LogDebug("補間曲線画像を更新します：" + tangents.Count);

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
                    view.currentPos.y,
                    WINDOW_WIDTH - tangentTex.width,
                    200);
                subView.parent = view;

                _tangentValueTypeComboBox.onSelected = (type, index) =>
                {
                    tangentValueType = type;
                };
                _tangentValueTypeComboBox.currentIndex = (int)tangentValueType;
                _tangentValueTypeComboBox.DrawButton(subView);

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

                var diffOutTangent = 0f;
                var diffInTangent = 0f;
                var newOutTangent = outTangent;
                var newInTangent = inTangent;

                subView.DrawLabel("OutTangent", 100, 20);

                subView.SetEnabled(!subView.IsComboBoxFocused() && includeOutTangent);

                subView.DrawFloatSelect(
                    "",
                    0.1f,
                    0f,
                    null,
                    outTangent,
                    value => newOutTangent = value,
                    value => diffOutTangent = value
                );

                subView.SetEnabled(!subView.IsComboBoxFocused());

                subView.DrawLabel("InTangent", 100, 20);

                subView.SetEnabled(!subView.IsComboBoxFocused() && includeInTangent);

                subView.DrawFloatSelect(
                    "",
                    0.1f,
                    0f,
                    null,
                    inTangent,
                    value => newInTangent = value,
                    value => diffInTangent = value
                );

                subView.SetEnabled(!subView.IsComboBoxFocused());

                // 新値の適用
                if (!float.IsNaN(newOutTangent) && newOutTangent != outTangent)
                {
                    foreachOutTangent((outTangentData) =>
                    {
                        outTangentData.normalizedValue = newOutTangent;
                        outTangentData.isSmooth = false;
                    });

                    MTEUtils.LogDebug("OutTangentを適用します：" + newOutTangent);
                    currentLayer.ApplyCurrentFrame(true);
                }

                if (!float.IsNaN(newInTangent) && newInTangent != inTangent)
                {
                    foreachInTangent((inTangentData) =>
                    {
                        inTangentData.normalizedValue = newInTangent;
                        inTangentData.isSmooth = false;
                    });

                    MTEUtils.LogDebug("InTangentを適用します：" + newInTangent);
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

                    MTEUtils.LogDebug("OutTangentの差分を適用します：" + diffOutTangent);
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

                    MTEUtils.LogDebug("InTangentの差分を適用します：" + diffInTangent);
                    currentLayer.ApplyCurrentFrame(true);
                }

                var isSmooth = tangents.All(tangent => tangent.isSmooth);
                subView.DrawToggle("自動補間", isSmooth, 100, 20, newIsSmooth =>
                {
                    foreachOutTangent((outTangentData) =>
                    {
                        outTangentData.isSmooth = newIsSmooth;
                    });

                    foreachInTangent((inTangentData) =>
                    {
                        inTangentData.isSmooth = newIsSmooth;
                    });

                    MTEUtils.LogDebug("自動補間を適用します：" + newIsSmooth);
                    currentLayer.ApplyCurrentFrame(true);
                });
            }

            view.DrawTexture(tangentTex);

            view.DrawLabel("プリセット反映", 100, 20);

            view.BeginHorizontal();

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

                    MTEUtils.LogDebug("プリセットを適用します：" + tangentType);
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
                MTEUtils.LogDebug("補間曲線画像を更新します：" + easings.Count);

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
                return;
            }

            view.AddSpace(10);

            Action<int> updateEasing = easing =>
            {
                var max = (int) MoveEasingType.Max;
                easing = (easing + max) % max;
 
                foreach (var bone in selectedBones)
                {
                    bone.transform.easing = easing;
                }

                MTEUtils.LogDebug("Easingを適用します: " + easing);
                currentLayer.ApplyCurrentFrame(true);
            };

            {
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

                easingComboBox.currentIndex = easing;
                easingComboBox.onSelected = (_easing, index) =>
                {
                    updateEasing(_easing);
                };
                easingComboBox.DrawButton("補間曲線", view);
            }

            view.DrawTexture(easingTex);
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
                int y = (int)(PluginUtils.HermiteSimplified(outTangent, inTangent, t) * height);

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