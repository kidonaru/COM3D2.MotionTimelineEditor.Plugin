using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.AI;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    using MTE = MotionTimelineEditor;

    public class KeyFrameUI : ISubWindowUI
    {
        public string title
        {
            get
            {
                return "キーフレーム 詳細";
            }
        }

        public static int WINDOW_WIDTH
        {
            get
            {
                return SubWindow.WINDOW_WIDTH;
            }
        }
        public static int WINDOW_HEIGHT
        {
            get
            {
                return SubWindow.WINDOW_HEIGHT;
            }
        }

        private Texture2D curveTex = null;
        private TangentValueType tangentValueType = TangentValueType.All;
        private HashSet<TangentPair> cachedTangents = new HashSet<TangentPair>();
        private Texture2D[] tangentTextures = null;
        private FloatFieldValue[] transFieldValues = null;
        private FloatFieldValue outTangentFieldValue = new FloatFieldValue();
        private FloatFieldValue inTangentFieldValue = new FloatFieldValue();

        private static TimelineManager timelineManager
        {
            get
            {
                return TimelineManager.instance;
            }
        }

        private static TimelineData timeline
        {
            get
            {
                return timelineManager.timeline;
            }
        }

        public static Config config
        {
            get
            {
                return MTE.config;
            }
        }

        public static readonly string[] TransValueLabels = new string[]
        {
            "X", "Y", "Z", "RX", "RY", "RZ"
        };

        public void OnOpen()
        {
            if (tangentTextures == null)
            {
                Extensions.LogDebug("補完曲線プリセット画像を生成します");

                tangentTextures = new Texture2D[(int) TangentType.Smooth];

                for (var i = 0; i < tangentTextures.Length; i++)
                {
                    var tangentType = (TangentType) i;
                    var texture = new Texture2D(40, 40);
                    var tangentPair = TangentPair.GetDefault(tangentType);

                    tangentTextures[i] = texture;
                    TimelineData.ClearTexture(texture, config.curveBgColor);

                    TimelineData.UpdateCurveTexture(
                        texture,
                        tangentPair.outTangent,
                        tangentPair.inTangent,
                        config.curveLineColor,
                        3
                    );
                }
            }

            if (curveTex == null)
            {
                curveTex = new Texture2D(150, 150);

                TimelineData.ClearTexture(curveTex, config.curveBgColor);
            }

            if (transFieldValues == null)
            {
                transFieldValues = new FloatFieldValue[6];

                for (var i = 0; i < transFieldValues.Length; i++)
                {
                    var fieldValue = new FloatFieldValue();
                    transFieldValues[i] = fieldValue;
                }
            }
        }

        public void OnClose()
        {
        }

        public void Update()
        {
        }

        public void DrawWindow(int id)
        {
            {
                var view = new GUIView(0, 20, WINDOW_WIDTH, WINDOW_HEIGHT - 20);

                HashSet<BoneData> selectedBones = timelineManager.selectedBones;

                var values = new float[6]
                {
                    float.NaN, float.NaN, float.NaN,
                    float.NaN, float.NaN, float.NaN
                };
                var includePos = false;
                var includeRot = false;

                var boneIndex = 0;
                foreach (var bone in selectedBones)
                {
                    var transform = bone.transform;
                    int nanCount = 0;

                    if (transform.isBipRoot)
                    {
                        var pos = transform.localPosition;
                        if (!includePos)
                        {
                            includePos = true;
                            values[0] = pos.x;
                            values[1] = pos.y;
                            values[2] = pos.z;
                        }
                        else
                        {
                            if (pos.x != values[0])
                            {
                                values[0] = float.NaN;
                                nanCount++;
                            }
                            if (pos.y != values[1])
                            {
                                values[1] = float.NaN;
                                nanCount++;
                            }
                            if (pos.z != values[2])
                            {
                                values[2] = float.NaN;
                                nanCount++;
                            }
                        }
                    }

                    boneIndex++;

                    if (boneIndex >= config.detailTransformCount || nanCount == 3)
                    {
                        break;
                    }
                }

                boneIndex = 0;
                foreach (var bone in selectedBones)
                {
                    var transform = bone.transform;
                    var rot = transform.localRotation.eulerAngles;
                    int nanCount = 0;

                    if (!includeRot)
                    {
                        includeRot = true;
                        values[3] = rot.x;
                        values[4] = rot.y;
                        values[5] = rot.z;
                    }
                    else
                    {
                        if (rot.x != values[3])
                        {
                            values[3] = float.NaN;
                            nanCount++;
                        }
                        if (rot.y != values[4])
                        {
                            values[4] = float.NaN;
                            nanCount++;
                        }
                        if (rot.z != values[5])
                        {
                            values[5] = float.NaN;
                            nanCount++;
                        }
                    }

                    boneIndex++;

                    if (boneIndex >= config.detailTransformCount || nanCount == 3)
                    {
                        break;
                    }
                }

                var diffValues = new float[6]
                {
                    0f, 0f, 0f,
                    0f, 0f, 0f,
                };
                
                var newValues = new float[6]
                {
                    values[0], values[1], values[2],
                    values[3], values[4], values[5],
                };

                for (var i = 0; i < transFieldValues.Length; i++)
                {
                    bool isPos = i < 3;
                    if (isPos && !includePos)
                    {
                        GUI.enabled = false;
                    }
                    if (!isPos && !includeRot)
                    {
                        GUI.enabled = false;
                    }

                    var value = values[i];
                    var fieldValue = transFieldValues[i];
                    fieldValue.UpdateValue(value, true);

                    view.BeginLayout(GUIView.LayoutDirection.Horizontal);

                    var label = TransValueLabels[i];
                    view.DrawLabel(label, 50, 20);

                    var diffValue = 0f;

                    if (view.DrawButton("<<", 25, 20))
                    {
                        diffValue = isPos ? -0.1f : -10f;
                    }
                    if (view.DrawButton("<", 20, 20))
                    {
                        diffValue = isPos ? -0.01f : -1f;
                    }

                    var newValue = view.DrawFloatFieldValue(fieldValue, 50, 20);

                    if (view.DrawButton(">", 20, 20))
                    {
                        diffValue = isPos ? 0.01f : 1f;
                    }
                    if (view.DrawButton(">>", 25, 20))
                    {
                        diffValue = isPos ? 0.1f : 10f;
                    }
                    if (view.DrawButton("0", 20, 20))
                    {
                        newValue = 0f;
                    }

                    diffValues[i] = diffValue;
                    newValues[i] = newValue;

                    view.EndLayout();

                    GUI.enabled = true;
                }

                if (view.DrawButton("初期化", 80, 20, includePos || includeRot))
                {
                    foreach (var selectedBone in selectedBones)
                    {
                        selectedBone.transform.Reset();
                    }

                    Extensions.LogDebug("初期化します");
                    timelineManager.ApplyCurrentFrame(true);
                }

                // 差分の適用
                for (var i = 0; i < diffValues.Length; i++)
                {
                    var diffValue = diffValues[i];
                    if (diffValue == 0f)
                    {
                        continue;
                    }

                    foreach (var selectedBone in selectedBones)
                    {
                        var transform = selectedBone.transform;

                        if (i < 3)
                        {
                            if (transform.isBipRoot)
                            {
                                var pos = transform.localPosition;
                                pos[i] += diffValue;
                                transform.localPosition = pos;
                            }
                        }
                        else
                        {
                            var rot = transform.localRotation.eulerAngles;
                            rot[i - 3] += diffValue;
                            transform.localRotation = Quaternion.Euler(rot);
                        }
                    }

                    Extensions.LogDebug("差分を適用します：" + diffValue);
                    timelineManager.ApplyCurrentFrame(true);
                }

                // 新値の適用
                for (var i = 0; i < newValues.Length; i++)
                {
                    var newValue = newValues[i];
                    if (float.IsNaN(newValue) || newValue == values[i])
                    {
                        continue;
                    }

                    foreach (var selectedBone in selectedBones)
                    {
                        var transform = selectedBone.transform;

                        if (i < 3)
                        {
                            if (transform.isBipRoot)
                            {
                                var pos = transform.localPosition;
                                pos[i] = newValue;
                                transform.localPosition = pos;
                            }
                        }
                        else
                        {
                            var rot = transform.localRotation.eulerAngles;
                            rot[i - 3] = newValue;
                            transform.localRotation = Quaternion.Euler(rot);
                        }
                    }

                    Extensions.LogDebug("新値を適用します：" + newValue);
                    timelineManager.ApplyCurrentFrame(true);
                }

                var tangents = new HashSet<TangentPair>();

                if (selectedBones.Count > 0)
                {
                    foreach (var bone in selectedBones)
                    {
                        var prevBone = timeline.GetPrevBone(bone);
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
                    Extensions.LogDebug("補完曲線画像を更新します：" + tangents.Count);

                    cachedTangents = tangents;

                    TimelineData.ClearTexture(curveTex, config.curveBgColor);

                    foreach (var tangent in tangents)
                    {
                        var color = tangent.isSmooth ? config.curveLineSmoothColor : config.curveLineColor;
                        TimelineData.UpdateCurveTexture(
                            curveTex,
                            tangent.outTangent,
                            tangent.inTangent,
                            color,
                            1
                        );
                    }
                }

                Action<Action<TangentData>> foreachOutTangent = (callback) =>
                {
                    foreach (var prevBone in timeline.GetPrevBones(selectedBones))
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

                view.DrawLabel("補完曲線", 100, 20);

                {
                    var subView = new GUIView(
                        curveTex.width + 10,
                        view.currentPos.y + 20,
                        WINDOW_WIDTH - curveTex.width,
                        200);

                    tangentValueType = (TangentValueType) subView.DrawSelectList(
                        TangentData.TangentValueTypeNames,
                        100, 20, (int) tangentValueType);

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
                        newOutTangent = timeline.ClampTangent(newOutTangent);

                        foreachOutTangent((outTangentData) =>
                        {
                            outTangentData.normalizedValue = newOutTangent;
                            outTangentData.isSmooth = false;
                        });

                        Extensions.LogDebug("OutTangentを適用します：" + newOutTangent);
                        timelineManager.ApplyCurrentFrame(true);
                    }

                    if (!float.IsNaN(newInTangent) && newInTangent != inTangent)
                    {
                        newInTangent = timeline.ClampTangent(newInTangent);

                        foreachInTangent((inTangentData) =>
                        {
                            inTangentData.normalizedValue = newInTangent;
                            inTangentData.isSmooth = false;
                        });

                        Extensions.LogDebug("InTangentを適用します：" + newInTangent);
                        timelineManager.ApplyCurrentFrame(true);
                    }

                    // 差分の適用
                    if (diffOutTangent != 0f)
                    {
                        foreachOutTangent((outTangentData) =>
                        {
                            var value = outTangentData.normalizedValue;
                            value = timeline.ClampTangent(value + diffOutTangent);
                            outTangentData.normalizedValue = value;
                            outTangentData.isSmooth = false;
                        });

                        Extensions.LogDebug("OutTangentの差分を適用します：" + diffOutTangent);
                        timelineManager.ApplyCurrentFrame(true);
                    }

                    if (diffInTangent != 0f)
                    {
                        foreachInTangent((inTangentData) =>
                        {
                            var value = inTangentData.normalizedValue;
                            value = timeline.ClampTangent(value + diffInTangent);
                            inTangentData.normalizedValue = value;
                            inTangentData.isSmooth = false;
                        });

                        Extensions.LogDebug("InTangentの差分を適用します：" + diffInTangent);
                        timelineManager.ApplyCurrentFrame(true);
                    }

                    var isSmooth = tangents.All(tangent => tangent.isSmooth);
                    var newIsSmooth = subView.DrawToggle("自動補完", isSmooth, 100, 20);
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

                        Extensions.LogDebug("自動補完を適用します：" + newIsSmooth);
                        timelineManager.ApplyCurrentFrame(true);
                    }
                }

                view.DrawTexture(curveTex);

                view.DrawLabel("プリセット反映", 100, 20);

                view.BeginLayout(GUIView.LayoutDirection.Horizontal);

                for (int i = 0; i < tangentTextures.Length; ++i)
                {
                    var texture = tangentTextures[i];
                    var tangentType = (TangentType) i;

                    view.DrawTexture(texture, texture.width, texture.height, Color.white, () =>
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

                        Extensions.LogDebug("プリセットを適用します：" + tangentType);
                        timelineManager.ApplyCurrentFrame(true);
                    });
                }

                view.EndLayout();
            }

            GUI.DragWindow();
        }
    }
}