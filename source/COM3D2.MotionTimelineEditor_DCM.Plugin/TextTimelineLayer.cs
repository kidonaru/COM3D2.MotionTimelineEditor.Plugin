using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using COM3D2.DanceCameraMotion.Plugin;
using COM3D2.MotionTimelineEditor.Plugin;
using UnityEngine;

namespace COM3D2.MotionTimelineEditor_DCM.Plugin
{
    using TextPlayData = MotionPlayData<TextMotionData>;

    public class TextTimeLineRow
    {
        public int frame;
        public int index;
        public string text;
        public string font;
        public int fontSize;
        public Vector3 position;
        public Vector3 rotation;
        public Vector3 scale;
        public Color color;
        public int easing;
        public int lineSpacing;
		public TextAnchor alignment;
		public Vector2 sizeDelta;
    }

    public class TextMotionData : MotionDataBase
    {
        public TextTimeLineRow start;
        public TextTimeLineRow end;
    }

    [LayerDisplayName("テキスト")]
    public partial class TextTimelineLayer : TimelineLayerBase
    {
        public override int priority
        {
            get
            {
                return 44;
            }
        }

        public override string className
        {
            get
            {
                return typeof(TextTimelineLayer).Name;
            }
        }

        public static string TextBoneName = "Text";
        public static string TextDisplayName = "テキスト";
        public static string DefaultFontName = "Yu Gothic Bold";

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

        private Dictionary<string, List<TextTimeLineRow>> _timelineRowsMap = new Dictionary<string, List<TextTimeLineRow>>();
        private Dictionary<string, TextPlayData> _playDataMap = new Dictionary<string, TextPlayData>();

        private TextManager _textManager = new TextManager(0);

        private TextTimelineLayer(int slotNo) : base(slotNo)
        {
        }

        public static TextTimelineLayer Create(int slotNo)
        {
            return new TextTimelineLayer(0);
        }

        public override void Init()
        {
            InitTextManager();

            base.Init();
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
                InitTextManager();
                InitMenuItems();
                AddFirstBones(allBoneNames);
            }

            if (!studioHack.isPoseEditing)
            {
                ApplyPlayData();
            }
        }

        private void ApplyPlayData()
        {
            var playingFrameNoFloat = this.playingFrameNoFloat;

            foreach (var playData in _playDataMap.Values)
            {
                var indexUpdated = playData.Update(playingFrameNoFloat);

                var current = playData.current;
                if (current != null)
                {
                    ApplyMotion(current, playData.lerpFrame, indexUpdated);
                }
            }
        }

        private void ApplyMotion(TextMotionData motion, float t, bool indexUpdated)
		{
            if (!IsValidIndex(motion.start.index))
            {
                return;
            }

            var start = motion.start;
            var end = motion.end;
			var freeTextSet = GetFreeTextSet(start.index);

            if (indexUpdated)
            {
                var text = freeTextSet.text;
                RectTransform rect = freeTextSet.rect;

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
                rect.eulerAngles = start.rotation;
                rect.localScale = start.scale;
                text.color = start.color;
                text.lineSpacing = (float) start.lineSpacing;
                text.alignment = start.alignment;
                rect.sizeDelta = start.sizeDelta;
            }

			if (start.position != end.position)
            {
                freeTextSet.rect.localPosition = Vector3.Lerp(start.position, end.position, t);
            }
			if (start.rotation != end.rotation)
            {
                freeTextSet.rect.eulerAngles = Vector3.Lerp(start.rotation, end.rotation, t);
            }
			if (start.scale != end.scale)
            {
                freeTextSet.rect.localScale = Vector3.Lerp(start.scale, end.scale, t);
            }
			if (start.color != end.color)
            {
                freeTextSet.text.color = Color.Lerp(start.color, end.color, t);
            }
			UpdateFreeTextSet(start.index, freeTextSet);
		}

        private void InitTextManager()
        {
            if (_textManager.TextData.Length == timeline.textCount)
            {
                return;
            }

            if (_textManager != null)
            {
                _textManager.ReleaseDanceText();
                _textManager = null;
            }

            _textManager = new TextManager(timeline.textCount);

            for (var i = 0; i < timeline.textCount; i++)
            {
                _textManager.InitializeText(i);

                var freeTextSet = GetFreeTextSet(i);
                var text = freeTextSet.text;
                var rect = freeTextSet.rect;

                text.font = _textManager.GetFont(DefaultFontName);
                text.fontSize = 50;
                text.lineSpacing = 50f;
                text.alignment = TextAnchor.MiddleCenter;
                rect.localPosition = Vector3.zero;
                rect.localScale = Vector3.one;
                rect.sizeDelta = new Vector2(1000, 1000);
            }
        }

        private bool IsValidIndex(int index)
        {
            return index >= 0 && index < _textManager.TextData.Length;
        }

        private FreeTextSet GetFreeTextSet(int index)
        {
            return _textManager.TextData[index];
        }

        private void UpdateFreeTextSet(int index, FreeTextSet freeTextSet)
        {
            _textManager.TextData[index] = freeTextSet;
        }

        public override void UpdateFrame(FrameData frame)
        {
            for (var index = 0; index < timeline.textCount; index++)
            {
                if (!IsValidIndex(index))
                {
                    continue;
                }

                var boneName = TextBoneName + index;
                var freeTextSet = GetFreeTextSet(index);
                var text = freeTextSet.text;
                var rect = freeTextSet.rect;

                var trans = CreateTransformData(boneName);
                trans.SetStrValue("text", text.text);
                trans.SetStrValue("font", text.font != null ? text.font.name : "");
                trans.position = rect.localPosition;
                trans.eulerAngles = rect.localEulerAngles;
                trans.scale = rect.localScale;
                trans.color = text.color;
                trans.easing = GetEasing(frame.frameNo, boneName);
                trans["index"].value = index;
                trans["fontSize"].value = text.fontSize;
                trans["lineSpacing"].value = text.lineSpacing;
                trans["alignment"].value = (int) text.alignment;
                trans["sizeDeltaX"].value = rect.sizeDelta.x;
                trans["sizeDeltaY"].value = rect.sizeDelta.y;

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

        protected override byte[] GetAnmBinaryInternal(bool forOutput, int startFrameNo, int endFrameNo)
        {
            _timelineRowsMap.Clear();

            foreach (var keyFrame in keyFrames)
            {
                AppendTimeLineRow(keyFrame);
            }

            AppendTimeLineRow(_dummyLastFrame);

            BuildPlayData(forOutput);

            return null;
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

                List<TextTimeLineRow> rows;
                if (!_timelineRowsMap.TryGetValue(name, out rows))
                {
                    rows = new List<TextTimeLineRow>();
                    _timelineRowsMap[name] = rows;
                }

                var trans = bone.transform;

                var row = new TextTimeLineRow
                {
                    frame = frame.frameNo,
                    index = trans["index"].intValue,
                    text = trans.GetStrValue("text"),
                    font = trans.GetStrValue("font"),
                    fontSize = trans["fontSize"].intValue,
                    position = trans.position,
                    rotation = trans.eulerAngles,
                    scale = trans.scale,
                    color = trans.color,
                    easing = trans.easing,
                    lineSpacing = trans["lineSpacing"].intValue,
                    alignment = (TextAnchor) trans["alignment"].intValue,
                    sizeDelta = new Vector2(trans["sizeDeltaX"].value, trans["sizeDeltaY"].value),
                };

                rows.Add(row);
            }
        }

        private void BuildPlayData(bool forOutput)
        {
            PluginUtils.LogDebug("BuildPlayData");
            _playDataMap.Clear();

            foreach (var pair in _timelineRowsMap)
            {
                var name = pair.Key;
                var rows = pair.Value;

                TextPlayData playData;
                if (!_playDataMap.TryGetValue(name, out playData))
                {
                    playData = new TextPlayData
                    {
                        motions = new List<TextMotionData>(rows.Count),
                    };
                    _playDataMap[name] = playData;
                }

                playData.ResetIndex();
                playData.motions.Clear();

                for (var i = 0; i < rows.Count - 1; i++)
                {
                    var start = rows[i];
                    var end = rows[i + 1];

                    var stFrame = start.frame;
                    var edFrame = end.frame;

                    var motion = new TextMotionData
                    {
                        stFrame = stFrame,
                        edFrame = edFrame,
                        start = start,
                        end = end,
                    };

                    playData.motions.Add(motion);
                }
            }

            foreach (var pair in _playDataMap)
            {
                var name = pair.Key;
                var playData = pair.Value;
                playData.Setup(timeline.singleFrameType);
                //PluginUtils.LogDebug("PlayData: name={0}, count={1}", name, playData.motions.Count);
            }
        }

        public override void OutputDCM(XElement songElement)
        {
            // TODO:
        }

        public override float CalcEasingValue(float t, int easing)
        {
            return TimelineMotionEasing.MotionEasing(t, (EasingType) easing);
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

        private static List<string> _fontNames = new List<string>
        {
            DefaultFontName,
        };

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

                if (view.DrawRepeatButton("<", 20, 20))
                {
                    timeline.textCount--;
                }
                if (view.DrawRepeatButton(">", 20, 20))
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

            if (!IsValidIndex(index))
            {
                return;
            }

            var freeTextSet = GetFreeTextSet(index);
            var text = freeTextSet.text;
            var rect = freeTextSet.rect;

            view.DrawHorizontalLine(Color.gray);
            view.AddSpace(5);
            view.BeginScrollView();

            view.SetEnabled(!view.IsComboBoxFocused() && studioHack.isPoseEditing);

            view.DrawLabel("テキスト", -1, 20);

            view.DrawTextField(new GUIView.TextFieldOption
            {
                value = text.text,
                onChanged = value => text.text = value,
                maxLines = 3,
            });

            _fontNameComboBox.items = _fontNames;
            _fontNameComboBox.currentIndex = _fontNames.IndexOf(text.font != null ? text.font.name : "");
            _fontNameComboBox.onSelected = (fontName, _) =>
            {
                text.font = _textManager.GetFont(fontName);
            };

            _fontNameComboBox.DrawButton("フォント", view);

            if (view.DrawButton("フォント一覧取得", 120, 20))
            {
                _textManager.GetFontNames();
                _fontNames = _textManager.FontNames;
            }

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

        public override ITransformData CreateTransformData(string name)
        {
            var transform = new TransformDataText();
            transform.Initialize(name);
            return transform;
        }
    }
}