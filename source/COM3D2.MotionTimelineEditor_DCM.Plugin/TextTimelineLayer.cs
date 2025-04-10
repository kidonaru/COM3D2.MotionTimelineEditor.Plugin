using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using COM3D2.MotionTimelineEditor;
using COM3D2.MotionTimelineEditor.Plugin;
using UnityEngine;

namespace COM3D2.MotionTimelineEditor_DCM.Plugin
{
    [TimelineLayerDesc("テキスト", 53)]
    public partial class TextTimelineLayer : TimelineLayerBase
    {
        public override Type layerType => typeof(TextTimelineLayer);
        public override string layerName => nameof(TextTimelineLayer);

        public static string TextBoneName = "Text";
        public static string TextDisplayName = "テキスト";

        private List<string> _allBoneNames = new List<string>();
        public override List<string> allBoneNames
        {
            get
            {
                if (_allBoneNames.Count != timeline.textCount)
                {
                    _allBoneNames.Clear();
                    for (var i = 0; i < timeline.textCount; i++)
                    {
                        _allBoneNames.Add(TextBoneName + i);
                    }
                }
                return _allBoneNames;
            }
        }

        private static MTETextManager _textManager = MTETextManager.instance;

        private TextTimelineLayer(int slotNo) : base(slotNo)
        {
        }

        public static TextTimelineLayer Create(int slotNo)
        {
            return new TextTimelineLayer(0);
        }

        protected override void InitMenuItems()
        {
            allMenuItems.Clear();

            foreach (var boneName in allBoneNames)
            {
                var menuItem = new BoneMenuItem(boneName, boneName);
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

            if (_textManager.TextData.Length != timeline.textCount)
            {
                _textManager.InitTextManager();
                InitMenuItems();
                AddFirstBones(allBoneNames);
            }

            if (!studioHackManager.isPoseEditing)
            {
                ApplyPlayData();
            }
        }

        protected override void ApplyMotion(MotionData motion, float t, bool indexUpdated, MotionPlayData playData)
		{
            if (indexUpdated)
            {
                ApplyMotionInit(motion, t);
            }

            ApplyMotionUpdate(motion, t);
        }

        private void ApplyMotionInit(MotionData motion, float t)
		{
            var start = motion.start as TransformDataText;

            if (!_textManager.IsValidIndex(start.index))
            {
                return;
            }

			var freeTextSet = _textManager.GetFreeTextSet(start.index);

            var text = freeTextSet.text;
            var rect = freeTextSet.rect;

            bool flag = false;
            if (text.text != start.text)
            {
                text.text = start.text;
                flag = true;
            }
            if (!string.IsNullOrEmpty(start.text) && (flag || (text.font != null && text.font.name != start.font)))
            {
                text.font = _textManager.GetFont(start.font);
            }
            text.fontSize = start.fontSize;
            rect.localPosition = start.position;
            rect.eulerAngles = start.eulerAngles;
            rect.localScale = start.scale;
            text.color = start.color;
            text.lineSpacing = (float) start.lineSpacing;
            text.alignment = start.alignment;
            rect.sizeDelta = start.sizeDelta;

			_textManager.UpdateFreeTextSet(start.index, freeTextSet);
		}

        private void ApplyMotionUpdate(MotionData motion, float t)
		{
            var start = motion.start as TransformDataText;
            var end = motion.end as TransformDataText;

            if (!_textManager.IsValidIndex(start.index))
            {
                return;
            }

			var freeTextSet = _textManager.GetFreeTextSet(start.index);

			if (start.position != end.position)
            {
                freeTextSet.rect.localPosition = Vector3.Lerp(start.position, end.position, t);
            }
			if (start.eulerAngles != end.eulerAngles)
            {
                freeTextSet.rect.eulerAngles = Vector3.Lerp(start.eulerAngles, end.eulerAngles, t);
            }
			if (start.scale != end.scale)
            {
                freeTextSet.rect.localScale = Vector3.Lerp(start.scale, end.scale, t);
            }
			if (start.color != end.color)
            {
                freeTextSet.text.color = Color.Lerp(start.color, end.color, t);
            }
			_textManager.UpdateFreeTextSet(start.index, freeTextSet);
		}

        public override void UpdateFrame(FrameData frame, bool initialEdit, bool force)
        {
            for (var index = 0; index < timeline.textCount; index++)
            {
                if (!_textManager.IsValidIndex(index))
                {
                    continue;
                }

                var boneName = TextBoneName + index;
                var freeTextSet = _textManager.GetFreeTextSet(index);
                var text = freeTextSet.text;
                var rect = freeTextSet.rect;

                var trans = CreateTransformData<TransformDataText>(boneName);
                trans.text = text.text;
                trans.font = text.font != null ? text.font.name : "";
                trans.position = rect.localPosition;
                trans.eulerAngles = rect.localEulerAngles;
                trans.scale = rect.localScale;
                trans.color = text.color;
                trans.easing = GetEasing(frame.frameNo, boneName);
                trans.index = index;
                trans.fontSize = text.fontSize;
                trans.lineSpacing = (int) text.lineSpacing;
                trans.alignment = text.alignment;
                trans.sizeDelta = rect.sizeDelta;

                var bone = frame.CreateBone(trans);
                frame.UpdateBone(bone);
            }
        }

        public void OutputPlayData(string filePath)
        {
            var offsetTime = timeline.startOffsetTime;

            var builder = new StringBuilder();
            builder.Append("index,text,font,fontSize,stTime,stPosX,stPosY,stRotZ,stScaX,stScaY,stColR,stColG,stColB,stColA,edTime,edPosX,edPosY,edRotZ,edScaX,edScaY,edColR,edColG,edColB,edColA,easing,lineSpacing,alignment,sizeDeltaX,sizeDeltaY\r\n");

            Action<MotionData> appendMotion = motion =>
            {
                var stTime = motion.stFrame * timeline.frameDuration;
                var edTime = motion.edFrame * timeline.frameDuration;

                stTime += offsetTime;
                edTime += offsetTime;

                var start = motion.start as TransformDataText;
                var end = motion.end as TransformDataText;
                var stColor = (Color32) start.color;
                var edColor = (Color32) end.color;

                builder.Append(start.index + ",");
                builder.Append(start.text + ",");
                builder.Append(start.font + ",");
                builder.Append(start.fontSize + ",");
                builder.Append(stTime.ToString("0.000") + ",");
                builder.Append(start.position.x.ToString("0.000") + ",");
                builder.Append(start.position.y.ToString("0.000") + ",");
                builder.Append(start.eulerAngles.z.ToString("0.000") + ",");
                builder.Append(start.scale.x.ToString("0.000") + ",");
                builder.Append(start.scale.y.ToString("0.000") + ",");
                builder.Append(stColor.r + ",");
                builder.Append(stColor.g + ",");
                builder.Append(stColor.b + ",");
                builder.Append(stColor.a + ",");
                builder.Append(edTime.ToString("0.000") + ",");
                builder.Append(end.position.x.ToString("0.000") + ",");
                builder.Append(end.position.y.ToString("0.000") + ",");
                builder.Append(end.eulerAngles.z.ToString("0.000") + ",");
                builder.Append(end.scale.x.ToString("0.000") + ",");
                builder.Append(end.scale.y.ToString("0.000") + ",");
                builder.Append(edColor.r + ",");
                builder.Append(edColor.g + ",");
                builder.Append(edColor.b + ",");
                builder.Append(edColor.a + ",");
                builder.Append(start.easing + ",");
                builder.Append(start.lineSpacing + ",");
                builder.Append((int) start.alignment + ",");
                builder.Append(start.sizeDelta.x.ToString("0.000") + ",");
                builder.Append(start.sizeDelta.y.ToString("0.000"));
                builder.Append("\r\n");
            };

            foreach (var playData in _playDataMap)
            {
                foreach (var motion in playData.Value.motions)
                {
                    appendMotion(motion);
                }
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
                var outputFileName = "text.csv";
                var outputPath = timeline.GetDcmSongFilePath(outputFileName);
                OutputPlayData(outputPath);

                songElement.Add(new XElement("changeText", outputFileName));
            }
            catch (Exception e)
            {
                MTEUtils.LogException(e);
                MTEUtils.LogError("テキストチェンジの出力に失敗しました");
            }
        }

        private GUIComboBox<string> _boneNameComboBox = new GUIComboBox<string>
        {
            getName = (boneName, index) =>
            {
                return boneName;
            },
        };

        private GUIComboBox<string> _fontNameComboBox = new GUIComboBox<string>
        {
            getName = (fontName, index) =>
            {
                return fontName;
            },
        };

        private GUIComboBox<TextAnchor> _textAlignmentComboBox = new GUIComboBox<TextAnchor>
        {
            items = Enum.GetValues(typeof(TextAnchor)).Cast<TextAnchor>().ToList(),
            getName = (type, index) =>
            {
                return type.ToString();
            },
        };

        private ColorFieldCache _colorFieldValue = new ColorFieldCache("Color", true);

        public override void DrawWindow(GUIView view)
        {
            view.SetEnabled(!view.IsComboBoxFocused());

            view.BeginHorizontal();
            {
                view.margin = 0;

                view.DrawLabel("テキスト表示数", view.labelWidth, 20);

                view.DrawIntField(new GUIView.IntFieldOption
                {
                    value = timeline.textCount,
                    width = view.viewRect.width - (view.labelWidth + 40 + view.padding.x * 2),
                    height = 20,
                    onChanged = x =>
                    {
                        timeline.textCount = x;
                    }
                });

                if (view.DrawButton("-", 20, 20))
                {
                    timeline.textCount--;
                }
                if (view.DrawButton("+", 20, 20))
                {
                    timeline.textCount++;
                }

                timeline.textCount = Mathf.Clamp(timeline.textCount, 1, 16);

                view.margin = GUIView.defaultMargin;
            }
            view.EndLayout();

            _boneNameComboBox.items = allBoneNames;
            _boneNameComboBox.DrawButton("対象", view);

            var boneName = _boneNameComboBox.currentItem;
            var index = _boneNameComboBox.currentIndex;

            if (!_textManager.IsValidIndex(index))
            {
                return;
            }

            var freeTextSet = _textManager.GetFreeTextSet(index);
            var text = freeTextSet.text;
            var rect = freeTextSet.rect;

            view.DrawHorizontalLine(Color.gray);
            view.AddSpace(5);
            view.BeginScrollView();

            view.SetEnabled(!view.IsComboBoxFocused() && studioHackManager.isPoseEditing);

            view.DrawLabel("テキスト", -1, 20);

            view.DrawTextField(new GUIView.TextFieldOption
            {
                value = text.text,
                onChanged = value => text.text = value,
                maxLines = 3,
            });

            _fontNameComboBox.items = MTETextManager.fontNames;
            _fontNameComboBox.currentIndex = MTETextManager.fontNames.IndexOf(text.font != null ? text.font.name : "");
            _fontNameComboBox.onSelected = (fontName, _) =>
            {
                text.font = _textManager.GetFont(fontName);
            };

            _fontNameComboBox.DrawButton("フォント", view);

            view.DrawSliderValue(new GUIView.SliderOption
            {
                label = "サイズ",
                labelWidth = 30,
                fieldType = FloatFieldType.Int,
                min = 0,
                max = 200,
                step = 1,
                defaultValue = 50,
                value = text.fontSize,
                onChanged = value => text.fontSize = (int) value,
            });

            view.DrawSliderValue(new GUIView.SliderOption
            {
                label = "行間",
                labelWidth = 30,
                fieldType = FloatFieldType.Int,
                min = 0,
                max = 200,
                step = 1,
                defaultValue = 50,
                value = text.lineSpacing,
                onChanged = value => text.lineSpacing = value,
            });

            _textAlignmentComboBox.currentIndex = (int) text.alignment;
            _textAlignmentComboBox.onSelected = (alignment, _) =>
            {
                text.alignment = alignment;
            };

            _textAlignmentComboBox.DrawButton("整列", view);

            view.DrawSliderValue(new GUIView.SliderOption
            {
                label = "幅",
                labelWidth = 30,
                fieldType = FloatFieldType.Int,
                min = 0,
                max = 2000,
                step = 1,
                defaultValue = 1000,
                value = rect.sizeDelta.x,
                onChanged = value =>
                {
                    var sizeDelta = rect.sizeDelta;
                    sizeDelta.x = value;
                    rect.sizeDelta = sizeDelta;
                }
            });

            view.DrawSliderValue(new GUIView.SliderOption
            {
                label = "高さ",
                labelWidth = 30,
                fieldType = FloatFieldType.Int,
                min = 0,
                max = 2000,
                step = 1,
                defaultValue = 1000,
                value = rect.sizeDelta.y,
                onChanged = value =>
                {
                    var sizeDelta = rect.sizeDelta;
                    sizeDelta.y = value;
                    rect.sizeDelta = sizeDelta;
                }
            });

            view.DrawColor(
                _colorFieldValue,
                text.color,
                Color.white,
                c => text.color = c);

            var initialPosition = Vector3.zero;
            var initialEulerAngles = Vector3.zero;
            var initialScale = Vector3.one;

            DrawTransformRect(
                view,
                rect,
                TransformEditType.全て,
                DrawMaskAll,
                boneName,
                initialPosition,
                initialEulerAngles,
                initialScale);

            view.SetEnabled(!view.IsComboBoxFocused());

            view.EndScrollView();
        }

        public override TransformType GetTransformType(string name)
        {
            return TransformType.Text;
        }
    }
}