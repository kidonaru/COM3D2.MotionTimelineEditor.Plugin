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

        private Dictionary<string, List<BoneData>> _timelineRowsMap = new Dictionary<string, List<BoneData>>();
        private Dictionary<string, MotionPlayData> _playDataMap = new Dictionary<string, MotionPlayData>();
        private List<MotionData> _outputMotions = new List<MotionData>(128);

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

        private void ApplyPlayData()
        {
            var playingFrameNoFloat = this.playingFrameNoFloat;

            foreach (var lightName in _playDataMap.Keys)
            {
                var playData = _playDataMap[lightName];

                var light = lightManager.GetLight(lightName);
                if (light == null || light.light == null)
                {
                    continue;
                }

                playData.Update(playingFrameNoFloat);

                var current = playData.current;
                if (current != null)
                {
                    ApplyMotion(current, light, playData.lerpFrame);
                }

                //PluginUtils.LogDebug("ApplyPlayData: lightName={0} lerpTime={1}, listIndex={2}", lightName, playData.lerpTime, playData.listIndex);
            }
        }

        private void ApplyMotion(MotionData motion, StudioLightStat stat, float lerpTime)
        {
            var light = stat.light;
            var followLight = stat.followLight;
            var transform = stat.transform;
            if (stat == null || light == null || followLight == null || transform == null)
            {
                return;
            }

            var start = motion.start;
            var end = motion.end;

            var stTrans = start.transform;
            var edTrans = end.transform;

            float easingTime = CalcEasingValue(lerpTime, stTrans.easing);

            followLight.maidSlotNo = stTrans["maidSlotNo"].intValue;

            var position = Vector3.Lerp(stTrans.position, edTrans.position, easingTime);
            if (followLight.isFollow)
            {
                followLight.offset = position;
            }
            else
            {
                transform.localPosition = position;
            }

            transform.localEulerAngles = Vector3.Lerp(stTrans.eulerAngles, edTrans.eulerAngles, easingTime);

            if (timeline.isLightColorEasing)
            {
                light.color = Color.Lerp(stTrans.color, edTrans.color, easingTime);
            }
            else
            {
                light.color = stTrans.color;
            }

            light.range = stTrans["range"].value;
            light.intensity = stTrans["intensity"].value;
            light.spotAngle = stTrans["spotAngle"].value;
            light.shadowStrength = stTrans["shadowStrength"].value;
            light.shadowBias = stTrans["shadowBias"].value;

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

                var newBone = keyFrame.GetOrCreateBone(newLightName);
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

                var trans = CreateTransformData(lightName);
                trans.position = position;
                trans.eulerAngles = transform.localEulerAngles;
                trans.color = light.color;
                trans["range"].value = light.range;
                trans["intensity"].value = light.intensity;
                trans["spotAngle"].value = light.spotAngle;
                trans["shadowStrength"].value = light.shadowStrength;
                trans["shadowBias"].value = light.shadowBias;
                trans["maidSlotNo"].value = followLight.maidSlotNo;
                trans.easing = GetEasing(frame.frameNo, lightName);

                var bone = frame.CreateBone(trans);
                frame.UpdateBone(bone);
            }
        }

        public override void ApplyAnm(long id, byte[] anmData)
        {
            ApplyPlayData();
        }

        public override void ApplyCurrentFrame(bool motionUpdate)
        {
            if (anmId != TimelineAnmId || motionUpdate)
            {
                CreateAndApplyAnm();
            }
            else
            {
                ApplyPlayData();
            }
        }

        public override void OutputAnm()
        {
            // do nothing
        }

        private void AppendTimeLineRow(FrameData frame)
        {
            foreach (var name in allBoneNames)
            {
                var bone = frame.GetBone(name);
                if (bone == null)
                {
                    continue;
                }

                List<BoneData> rows;
                if (!_timelineRowsMap.TryGetValue(name, out rows))
                {
                    rows = new List<BoneData>();
                    _timelineRowsMap[name] = rows;
                }

                rows.Add(bone);
            }
        }

        private void BuildPlayData(bool forOutput)
        {
            foreach (var playData in _playDataMap.Values)
            {
                playData.ResetIndex();
                playData.motions.Clear();
            }

            foreach (var pair in _timelineRowsMap)
            {
                var name = pair.Key;
                var rows = pair.Value;

                if (rows.Count == 0)
                {
                    continue;
                }

                var light = lightManager.GetLight(name);
                if (light == null || light.light == null)
                {
                    continue;
                }

                MotionPlayData playData;
                if (!_playDataMap.TryGetValue(name, out playData))
                {
                    playData = new MotionPlayData(rows.Count);
                    _playDataMap[name] = playData;
                }

                for (var i = 0; i < rows.Count - 1; i++)
                {
                    var start = rows[i];
                    var end = rows[i + 1];
                    playData.motions.Add(new MotionData(start, end));
                }

                playData.Setup(timeline.singleFrameType);
            }
        }

        protected override byte[] GetAnmBinaryInternal(bool forOutput, int startFrameNo, int endFrameNo)
        {
            foreach (var rows in _timelineRowsMap.Values)
            {
                rows.Clear();
            }

            foreach (var keyFrame in keyFrames)
            {
                AppendTimeLineRow(keyFrame);
            }

            AppendTimeLineRow(_dummyLastFrame);

            BuildPlayData(forOutput);

            return null;
        }

        public void SaveLightMotion(
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
                var start = motion.start;
                var end = motion.end;

                var stTrans = start.transform;
                var edTrans = end.transform;

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
                builder.Append(stTrans.position.x.ToString("0.000") + ",");
                builder.Append(stTrans.position.y.ToString("0.000") + ",");
                builder.Append(stTrans.position.z.ToString("0.000") + ",");
                builder.Append(stTrans.eulerAngles.x.ToString("0.000") + ",");
                builder.Append(stTrans.eulerAngles.y.ToString("0.000") + ",");
                builder.Append(stTrans.eulerAngles.z.ToString("0.000") + ",");
                builder.Append(stTrans.color.r.ToString("0.000") + ",");
                builder.Append(stTrans.color.g.ToString("0.000") + ",");
                builder.Append(stTrans.color.b.ToString("0.000") + ",");
                builder.Append(edTime.ToString("0.000") + ",");
                builder.Append(edTrans.position.x.ToString("0.000") + ",");
                builder.Append(edTrans.position.y.ToString("0.000") + ",");
                builder.Append(edTrans.position.z.ToString("0.000") + ",");
                builder.Append(edTrans.eulerAngles.x.ToString("0.000") + ",");
                builder.Append(edTrans.eulerAngles.y.ToString("0.000") + ",");
                builder.Append(edTrans.eulerAngles.z.ToString("0.000") + ",");
                builder.Append(edTrans.color.r.ToString("0.000") + ",");
                builder.Append(edTrans.color.g.ToString("0.000") + ",");
                builder.Append(edTrans.color.b.ToString("0.000") + ",");
                builder.Append("" + ","); // option
                builder.Append(stTrans["range"].value.ToString("0.000") + ","); // range
                builder.Append(stTrans["intensity"].value.ToString("0.000") + ","); // intensity
                builder.Append(stTrans["spotAngle"].value.ToString("0.000") + ","); // spotAngle
                builder.Append(stTrans["maidSlotNo"].value + ","); // maidSlotNo
                builder.Append(stTrans["shadowStrength"].value.ToString("0.000") + ","); // shadowStrength
                builder.Append(stTrans["shadowBias"].value.ToString("0.000")); // shadowBias
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
                _outputMotions.Clear();

                foreach (var playData in _playDataMap.Values)
                {
                    _outputMotions.AddRange(playData.motions);
                }

                var outputFileName = "light.csv";
                var outputPath = timeline.GetDcmSongFilePath(outputFileName);
                SaveLightMotion(_outputMotions, outputPath);

                songElement.Add(new XElement("changeLight", outputFileName));
            }
            catch (Exception e)
            {
                PluginUtils.LogException(e);
                PluginUtils.ShowDialog("ライトチェンジの出力に失敗しました");
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

        public override ITransformData CreateTransformData(string name)
        {
            var transform = new TransformDataLight();
            transform.Initialize(name);
            return transform;
        }
    }
}