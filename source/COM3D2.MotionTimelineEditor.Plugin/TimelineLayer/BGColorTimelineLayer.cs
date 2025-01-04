using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using UnityEngine;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    [TimelineLayerDesc("背景色", 32)]
    public partial class BGColorTimelineLayer : TimelineLayerBase
    {
        public override string className => typeof(BGColorTimelineLayer).Name;

        public BGGround bgGround = null;

        public static string BGColorBoneName = "BGColor";
        public static string BGColorDisplayName = "背景色";

        public static string BGGroundColorBoneName = "BGGroundColor";
        public static string BGGroundColorDisplayName = "地面色";

        private List<string> _allBoneNames = new List<string>
        {
            BGColorBoneName,
            BGGroundColorBoneName,
        };
        public override List<string> allBoneNames => _allBoneNames;

        private static Camera camera
        {
            get => GameMain.Instance.MainCamera.camera;
        }

        private BGColorTimelineLayer(int slotNo) : base(slotNo)
        {
        }

        public static BGColorTimelineLayer Create(int slotNo)
        {
            return new BGColorTimelineLayer(0);
        }

        public override void Init()
        {
            base.Init();

            if (bgGround == null)
            {
                var go = new GameObject("Ground");
                bgGround = go.AddComponent<BGGround>();
                bgGround.visible = false;
            }

            AddFirstBones(allBoneNames);
        }

        protected override void InitMenuItems()
        {
            allMenuItems.Clear();

            {
                var menuItem = new BoneMenuItem(BGColorBoneName, BGColorDisplayName);
                allMenuItems.Add(menuItem);
            }

            {
                var menuItem = new BoneMenuItem(BGGroundColorBoneName, BGGroundColorDisplayName);
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

            if (!studioHack.isPoseEditing)
            {
                ApplyPlayData();
            }
        }

        public override void LateUpdate()
        {
            base.LateUpdate();
        }

        protected override void ApplyMotion(MotionData motion, float t, bool indexUpdated)
        {
            switch (motion.start.type)
            {
                case TransformType.BGColor:
                    if (indexUpdated)
                    {
                        ApplyBGColorMotionInit(motion, t);
                    }
                    break;
                case TransformType.BGGroundColor:
                    if (indexUpdated)
                    {
                        ApplyBGGroundColorMotionInit(motion, t);
                    }
                    break;
            }
        }

        private void ApplyBGColorMotionInit(MotionData motion, float t)
        {
            try
            {
                var start = motion.start;
                camera.backgroundColor = start.color;
            }
            catch (Exception e)
            {
                PluginUtils.LogException(e);
            }
        }

        private void ApplyBGGroundColorMotionInit(MotionData motion, float t)
        {
            try
            {
                var start = motion.start;

                if (bgGround != null)
                {
                    bgGround.visible = start.visible;
                    bgGround.position = start.position;
                    bgGround.scale = start.scale;
                    bgGround.color = start.color;
                }
            }
            catch (Exception e)
            {
                PluginUtils.LogException(e);
            }
        }

        public override void UpdateFrame(FrameData frame)
        {
            {
                var trans = frame.GetOrCreateTransformData<TransformDataBGColor>(BGColorBoneName);
                trans.color = camera.backgroundColor;
            }

            if (bgGround != null)
            {
                var trans = frame.GetOrCreateTransformData<TransformDataBGGroundColor>(BGGroundColorBoneName);
                trans.visible = bgGround.visible;
                trans.position = bgGround.position;
                trans.scale = bgGround.scale;
                trans.color = bgGround.color;
            }
        }

        public void OutputBones(
            List<BoneData> rows,
            string filePath)
        {
            var offsetTime = timeline.startOffsetTime;

            var builder = new StringBuilder();
            builder.Append("bgName,colorR,colorG,colorB\r\n");

            Action<BoneData, bool> appendRow = (row, isFirst) =>
            {
                var time = row.frameNo * timeline.frameDuration;

                if (isFirst)
                {
                    time = 0;
                }
                else
                {
                    time += offsetTime;
                }

                var transform = row.transform;

                builder.Append(time.ToString("0.000") + ",");
                builder.Append(transform.color.IntR() + ",");
                builder.Append(transform.color.IntG() + ",");
                builder.Append(transform.color.IntB());
                builder.Append("\r\n");
            };

            if (rows.Count > 0 && offsetTime > 0f)
            {
                appendRow(rows.First(), true);
            }
            
            foreach (var row in rows)
            {
                appendRow(row, false);
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
                var outputFileName = "bg_color.csv";
                var outputPath = timeline.GetDcmSongFilePath(outputFileName);
                OutputBones(_timelineBonesMap[BGColorBoneName], outputPath);

                songElement.Add(new XElement("changeBgColor", outputFileName));
            }
            catch (Exception e)
            {
                PluginUtils.LogException(e);
                PluginUtils.LogError("背景色チェンジの出力に失敗しました");
            }
        }

        private ColorFieldCache _colorFieldValue = new ColorFieldCache("Color", false);

        public override void DrawWindow(GUIView view)
        {
            if (camera == null)
            {
                return;
            }
            var color = camera.backgroundColor;
            var updateTransform = false;

            view.SetEnabled(!view.IsComboBoxFocused() && studioHack.isPoseEditing);

            view.DrawLabel("背景色", 70, 20);

            updateTransform |= view.DrawColor(
                _colorFieldValue,
                color,
                Color.black,
                c => color = c);

            if (updateTransform)
            {
                camera.backgroundColor = color;
            }

            view.DrawHorizontalLine(Color.gray);

            if (bgGround != null)
            {
                var defaultTrans = TransformDataBGGroundColor.defaultTrans;

                updateTransform |= view.DrawToggle("地面色", bgGround.visible, 80, 20, v =>
                {
                    bgGround.visible = v;
                });

                updateTransform |= view.DrawColor(
                    _colorFieldValue,
                    bgGround.color,
                    defaultTrans.initialColor,
                    c => bgGround.color = c);

                {
                    var initialPosition = defaultTrans.initialPosition;
                    var transformCache = view.GetTransformCache(null);
                    transformCache.position = bgGround.position;

                    view.DrawLabel("位置", 200, 20);

                    updateTransform |= DrawPosition(
                        view,
                        transformCache,
                        TransformEditType.全て,
                        initialPosition);

                    if (updateTransform)
                    {
                        bgGround.position = transformCache.position;
                    }
                }

                {
                    var initialScale = defaultTrans.initialScale;
                    var scale = bgGround.scale;

                    updateTransform |= view.DrawSliderValue(new GUIView.SliderOption
                    {
                        label = "SX",
                        labelWidth = 30,
                        min = 0,
                        max = 1000f,
                        step = 1f,
                        defaultValue = initialScale.x,
                        value = scale.x,
                        onChanged = x => scale.x = x,
                    });

                    updateTransform |= view.DrawSliderValue(new GUIView.SliderOption
                    {
                        label = "SZ",
                        labelWidth = 30,
                        min = 0,
                        max = 1000f,
                        step = 1f,
                        defaultValue = initialScale.z,
                        value = scale.z,
                        onChanged = z => scale.z = z,
                    });

                    if (updateTransform)
                    {
                        bgGround.scale = scale;
                    }
                }
            }
        }

        public override SingleFrameType GetSingleFrameType(TransformType transformType)
        {
            return SingleFrameType.None;
        }

        public override TransformType GetTransformType(string name)
        {
            if (name == BGColorBoneName)
            {
                return TransformType.BGColor;
            }
            else if (name == BGGroundColorBoneName)
            {
                return TransformType.BGGroundColor;
            }

            return TransformType.BGColor;
        }
    }
}