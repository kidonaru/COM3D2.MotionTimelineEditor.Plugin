using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using UnityEngine;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    [TimelineLayerDesc("ライト", 41)]
    public class LightTimelineLayer : LightTimelineLayerBase
    {
        public override string className
        {
            get
            {
                return typeof(LightTimelineLayer).Name;
            }
        }

        public override List<string> allBoneNames
        {
            get
            {
                return lightManager.lightNames;
            }
        }

        private LightTimelineLayer(int slotNo) : base(slotNo)
        {
        }

        public static LightTimelineLayer Create(int slotNo)
        {
            return new LightTimelineLayer(0);
        }

        protected override void InitMenuItems()
        {
            allMenuItems.Clear();

            foreach (var light in lightManager.lights)
            {
                var menuItem = new BoneMenuItem(light.name, light.displayName);
                allMenuItems.Add(menuItem);
            }
        }

        public override bool IsValidData()
        {
            errorMessage = "";
            return true;
        }

        public override void Update()
        {
            base.Update();
        }

        public override void LateUpdate()
        {
            base.LateUpdate();

            if (!studioHack.isPoseEditing)
            {
                ApplyPlayData();
            }
        }

        protected override void ApplyMotion(MotionData motion, float t, bool indexUpdated)
        {
            var stat = lightManager.GetLight(motion.name);
            if (stat == null)
            {
                return;
            }

            var light = stat.light;
            if (light == null)
            {
                return;
            }

            var followLight = stat.followLight;
            var transform = stat.transform;
            if (followLight == null || transform == null)
            {
                return;
            }

            var start = motion.start as TransformDataLight;
            var end = motion.end as TransformDataLight;

            float easingTime = CalcEasingValue(t, start.easing);

            followLight.maidSlotNo = start.maidSlotNo;

            var position = Vector3.Lerp(start.position, end.position, easingTime);
            if (followLight.isFollow)
            {
                followLight.offset = position;
            }
            else
            {
                transform.localPosition = position;
            }

            transform.localEulerAngles = Vector3.Lerp(start.eulerAngles, end.eulerAngles, easingTime);

            if (timeline.isLightColorEasing)
            {
                light.color = Color.Lerp(start.color, end.color, easingTime);
            }
            else
            {
                light.color = start.color;
            }

            light.range = start.range;
            light.intensity = start.intensity;
            light.spotAngle = start.spotAngle;
            light.shadowStrength = start.shadowStrength;
            light.shadowBias = start.shadowBias;

            lightManager.ApplyLight(stat);
        }

        public override void OnLightAdded(StudioLightStat light)
        {
            InitMenuItems();
            AddFirstBones(new List<string> { light.name });
            ApplyCurrentFrame(true);
        }

        public override void OnLightRemoved(StudioLightStat light)
        {
            InitMenuItems();
            RemoveAllBones(new List<string> { light.name });
            ApplyCurrentFrame(true);
        }

        public override void OnLightUpdated(StudioLightStat light)
        {
            InitMenuItems();
            ApplyCurrentFrame(true);
        }

        public override void OnCopyLight(StudioLightStat sourceLight, StudioLightStat newLight)
        {
            var sourceLightName = sourceLight.name;
            var newLightName = newLight.name;
            foreach (var keyFrame in keyFrames)
            {
                var sourceBone = keyFrame.GetBone(sourceLightName);
                if (sourceBone == null)
                {
                    continue;
                }

                var newBone = keyFrame.GetOrCreateBone(sourceBone.transform.type, newLightName);
                newBone.transform.FromTransformData(sourceBone.transform);
            }
        }

        public override void UpdateFrame(FrameData frame)
        {
            foreach (var stat in lightManager.lights)
            {
                if (stat == null || stat.light == null || stat.transform == null)
                {
                    continue;
                }

                var lightName = stat.name;
                var light = stat.light;
                var transform = stat.transform;
                var followLight = stat.followLight;

                var position = transform.localPosition;
                if (followLight.isFollow)
                {
                    position = followLight.offset;
                }

                var trans = CreateTransformData<TransformDataLight>(lightName);
                trans.position = position;
                trans.eulerAngles = transform.localEulerAngles;
                trans.color = light.color;
                trans.range = light.range;
                trans.intensity = light.intensity;
                trans.spotAngle = light.spotAngle;
                trans.shadowStrength = light.shadowStrength;
                trans.shadowBias = light.shadowBias;
                trans.maidSlotNo = followLight.maidSlotNo;
                trans.easing = GetEasing(frame.frameNo, lightName);

                var bone = frame.CreateBone(trans);
                frame.UpdateBone(bone);
            }
        }

        public void OutputMotions(
            List<MotionData> motions,
            string filePath)
        {
            var offsetTime = timeline.startOffsetTime;

            var builder = new StringBuilder();
            builder.Append("type,group,stTime,stPosX,stPosY,stPosZ,stRotX,stRotY,stRotZ,stColR,stColG,stColB," +
                            "edTime,edPosX,edPosY,edPosZ,edRotX,edRotY,edRotZ,edColR,edColG,edColB," +
                            "option,range,intensity,spotAngle,maidSlotNo,shadowStrength,shadowBias" +
                            "\r\n");

            Action<MotionData, bool> appendMotion = (motion, isFirst) =>
            {
                var start = motion.start as TransformDataLight;
                var end = motion.end as TransformDataLight;

                var light = lightManager.GetLight(start.name);
                if (light == null || light.light == null)
                {
                    return;
                }

                var stTime = motion.stFrame * timeline.frameDuration;
                var edTime = motion.edFrame * timeline.frameDuration;

                if (isFirst)
                {
                    stTime = 0;
                    edTime = offsetTime;
                }
                else
                {
                    stTime += offsetTime;
                    edTime += offsetTime;
                }

                builder.Append(light.type + ",");
                builder.Append(light.index + ",");
                builder.Append(stTime.ToString("0.000") + ",");
                builder.Append(start.position.x.ToString("0.000") + ",");
                builder.Append(start.position.y.ToString("0.000") + ",");
                builder.Append(start.position.z.ToString("0.000") + ",");
                builder.Append(start.eulerAngles.x.ToString("0.000") + ",");
                builder.Append(start.eulerAngles.y.ToString("0.000") + ",");
                builder.Append(start.eulerAngles.z.ToString("0.000") + ",");
                builder.Append(start.color.r.ToString("0.000") + ",");
                builder.Append(start.color.g.ToString("0.000") + ",");
                builder.Append(start.color.b.ToString("0.000") + ",");
                builder.Append(edTime.ToString("0.000") + ",");
                builder.Append(end.position.x.ToString("0.000") + ",");
                builder.Append(end.position.y.ToString("0.000") + ",");
                builder.Append(end.position.z.ToString("0.000") + ",");
                builder.Append(end.eulerAngles.x.ToString("0.000") + ",");
                builder.Append(end.eulerAngles.y.ToString("0.000") + ",");
                builder.Append(end.eulerAngles.z.ToString("0.000") + ",");
                builder.Append(end.color.r.ToString("0.000") + ",");
                builder.Append(end.color.g.ToString("0.000") + ",");
                builder.Append(end.color.b.ToString("0.000") + ",");
                builder.Append("" + ","); // option
                builder.Append(start.range.ToString("0.000") + ","); // range
                builder.Append(start.intensity.ToString("0.000") + ","); // intensity
                builder.Append(start.spotAngle.ToString("0.000") + ","); // spotAngle
                builder.Append(start.maidSlotNo + ","); // maidSlotNo
                builder.Append(start.shadowStrength.ToString("0.000") + ","); // shadowStrength
                builder.Append(start.shadowBias.ToString("0.000")); // shadowBias
                builder.Append("\r\n");
            };

            if (motions.Count > 0 && offsetTime > 0f)
            {
                appendMotion(motions.First(), true);
            }

            foreach (var motion in motions)
            {
                appendMotion(motion, false);
            }

            using (var streamWriter = new StreamWriter(filePath, false))
            {
                streamWriter.Write(builder.ToString());
            }
        }

        public override void OutputDCM(XElement songElement)
        {
            try
            {
                var motions = new List<MotionData>(128);

                foreach (var playData in _playDataMap.Values)
                {
                    motions.AddRange(playData.motions);
                }

                var outputFileName = "light.csv";
                var outputPath = timeline.GetDcmSongFilePath(outputFileName);
                OutputMotions(motions, outputPath);

                songElement.Add(new XElement("changeLight", outputFileName));
            }
            catch (Exception e)
            {
                PluginUtils.LogException(e);
                PluginUtils.LogError("ライトチェンジの出力に失敗しました");
            }
        }

        private GUIComboBox<StudioLightStat> _lightComboBox = new GUIComboBox<StudioLightStat>
        {
            getName = (stat, index) => stat.displayName,
        };

        private GUIComboBox<MaidCache> _maidComboBox = new GUIComboBox<MaidCache>
        {
            getName = (maidCache, _) => maidCache == null ? "未選択" : maidCache.fullName,
            buttonSize = new Vector2(100, 20),
            contentSize = new Vector2(150, 300),
        };

        private ColorFieldCache _colorFieldValue = new ColorFieldCache("Color", false);

        private enum TabType
        {
            操作,
            管理,
        }

        private TabType _tabType = TabType.操作;

        public override void DrawWindow(GUIView view)
        {
            _tabType = view.DrawTabs(_tabType, 50, 20);

            switch (_tabType)
            {
                case TabType.操作:
                    DrawLightEdit(view);
                    break;
                case TabType.管理:
                    DrawLightManage(view);
                    break;
            }

            view.DrawComboBox();
        }
        
        public void DrawLightEdit(GUIView view)
        {
            var lights = lightManager.lights;
            if (lights.Count == 0)
            {
                view.DrawLabel("ライトが存在しません", 200, 20);
                return;
            }

            view.SetEnabled(!view.IsComboBoxFocused());

            _lightComboBox.items = lights;
            _lightComboBox.DrawButton("対象", view);

            var stat = _lightComboBox.currentItem;

            if (stat == null || stat.light == null || stat.transform == null)
            {
                view.DrawLabel("ライトを選択してください", 200, 20);
                return;
            }

            view.DrawHorizontalLine(Color.gray);

            view.AddSpace(5);

            view.BeginScrollView();

            view.SetEnabled(!view.IsComboBoxFocused() && studioHack.isPoseEditing);

            {
                view.DrawLabel(stat.displayName, 200, 20);

                var light = stat.light;
                var followLight = stat.followLight;
                var transform = stat.transform;
                var position = transform.localPosition;
                var initialPosition = StudioLightStat.DefaultPosition;
                var initialEulerAngles = StudioLightStat.DefaultRotation;
                var initialScale = Vector3.one;
                var updateTransform = false;
                var editType = TransformEditType.全て;

                int drawMask;
                switch (stat.type)
                {
                    case LightType.Directional:
                        drawMask = DrawMaskRotation;
                        break;
                    case LightType.Spot:
                        drawMask = DrawMaskPositonAndRotation;
                        break;
                    case LightType.Point:
                    default:
                        drawMask = DrawMaskPosition;
                        break;
                }

                if (followLight.isFollow)
                {
                    if (IsDrawTransformType(TransformDrawType.移動, editType, drawMask))
                    {
                        initialPosition = new Vector3(0f, 0f, 0f);
                        position = followLight.offset;

                        updateTransform |= view.DrawSliderValue(
                            new GUIView.SliderOption
                            {
                                label = "X",
                                labelWidth = 30,
                                min = -config.positionRange,
                                max = config.positionRange,
                                step = 0.01f,
                                defaultValue = initialPosition.x,
                                value = position.x,
                                onChanged = x => position.x = x,
                            });

                        updateTransform |= view.DrawSliderValue(
                            new GUIView.SliderOption
                            {
                                label = "Y",
                                labelWidth = 30,
                                min = -config.positionRange,
                                max = config.positionRange,
                                step = 0.01f,
                                defaultValue = initialPosition.y,
                                value = position.y,
                                onChanged = x => position.y = x,
                            });

                        updateTransform |= view.DrawSliderValue(
                            new GUIView.SliderOption
                            {
                                label = "Z",
                                labelWidth = 30,
                                min = -config.positionRange,
                                max = config.positionRange,
                                step = 0.01f,
                                defaultValue = initialPosition.z,
                                value = position.z,
                                onChanged = x => position.z = x,
                            });

                        if (updateTransform)
                        {
                            followLight.offset = position;
                        }

                        drawMask &= DrawMaskRotation;
                    }
                }

                updateTransform |= DrawTransform(
                    view,
                    transform,
                    editType,
                    drawMask,
                    stat.name,
                    initialPosition,
                    initialEulerAngles,
                    initialScale);

                updateTransform |= view.DrawColor(
                    _colorFieldValue,
                    light.color,
                    Color.white,
                    c => light.color = c);

                if (stat.type != LightType.Directional)
                {
                    updateTransform |= view.DrawSliderValue(
                        new GUIView.SliderOption
                        {
                            label = "範囲",
                            labelWidth = 30,
                            min = 0f,
                            max = 30f,
                            step = 0.1f,
                            defaultValue = 3f,
                            value = light.range,
                            onChanged = newValue => light.range = newValue,
                        });
                }

                updateTransform |= view.DrawSliderValue(
                    new GUIView.SliderOption
                    {
                        label = "強度",
                        labelWidth = 30,
                        min = 0f,
                        max = 5f,
                        step = 0.1f,
                        defaultValue = 3f,
                        value = light.intensity,
                        onChanged = newValue => light.intensity = newValue,
                    });

                if (stat.type == LightType.Spot)
                {
                    updateTransform |= view.DrawSliderValue(
                        new GUIView.SliderOption
                        {
                            label = "角度",
                            labelWidth = 30,
                            min = 0f,
                            max = 180f,
                            step = 1f,
                            defaultValue = 50f,
                            value = light.spotAngle,
                            onChanged = newValue => light.spotAngle = newValue,
                        });
                }

                if (stat.type == LightType.Directional)
                {
                    updateTransform |= view.DrawSliderValue(
                        new GUIView.SliderOption
                        {
                            label = "影濃",
                            labelWidth = 30,
                            min = 0f,
                            max = 1f,
                            step = 0.01f,
                            defaultValue = 0.1f,
                            value = light.shadowStrength,
                            onChanged = newValue => light.shadowStrength = newValue,
                        });

                    updateTransform |= view.DrawSliderValue(
                        new GUIView.SliderOption
                        {
                            label = "影距",
                            labelWidth = 30,
                            min = 0f,
                            max = 1f,
                            step = 0.01f,
                            defaultValue = 0.01f,
                            value = light.shadowBias,
                            onChanged = newValue => light.shadowBias = newValue,
                        });
                }

                if (stat.type != LightType.Directional)
                {
                    view.BeginHorizontal();
                    {
                        view.DrawLabel("追従メイド", 70, 20);

                        view.DrawToggle("", followLight.maidSlotNo >= 0, 20, 20, newValue =>
                        {
                            followLight.maidSlotNo = newValue ? _maidComboBox.currentIndex : -1;
                        });

                        _maidComboBox.items = maidManager.maidCaches;
                        _maidComboBox.onSelected = (maidCache, index) =>
                        {
                            followLight.maidSlotNo = index;
                        };
                        _maidComboBox.DrawButton(view);
                    }
                    view.EndLayout();
                }

                if (updateTransform)
                {
                    lightManager.ApplyLight(stat);
                }

                view.DrawHorizontalLine(Color.gray);
            }

            view.SetEnabled(!view.IsComboBoxFocused());
            view.EndScrollView();
        }

        public override TransformType GetTransformType(string name)
        {
            return TransformType.Light;
        }
    }
}