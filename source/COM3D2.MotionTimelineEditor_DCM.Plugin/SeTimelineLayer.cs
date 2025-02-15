using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using COM3D2.DanceCameraMotion.Plugin;
using COM3D2.MotionTimelineEditor;
using COM3D2.MotionTimelineEditor.Plugin;
using UnityEngine;

namespace COM3D2.MotionTimelineEditor_DCM.Plugin
{
    using TransformType = MotionTimelineEditor.Plugin.TransformType;

    [TimelineLayerDesc("効果音", 51)]
    public partial class SeTimelineLayer : TimelineLayerBase
    {
        public override Type layerType => typeof(SeTimelineLayer);
        public override string layerName => nameof(SeTimelineLayer);

        public static string SeBoneName = "SE";
        public static string SeDisplayName = "効果音";

        private List<string> _allBoneNames = new List<string> { SeBoneName };
        public override List<string> allBoneNames => _allBoneNames;

        private static SoundManager soundManager = new SoundManager(false);
        private List<string> _seNames = new List<string>();

        private SeTimelineLayer(int slotNo) : base(slotNo)
        {
        }

        public static SeTimelineLayer Create(int slotNo)
        {
            return new SeTimelineLayer(0);
        }

        public override void Init()
        {
            base.Init();

            UpdateSeNames();
        }

        private void UpdateSeNames()
        {
            _seNames.Clear();
            _seNames.AddRange(soundManager.SEData.data);
            _seNames.AddRange(config.additionalSeNames);
        }

        protected override void InitMenuItems()
        {
            allMenuItems.Clear();

            var menuItem = new BoneMenuItem(SeBoneName, SeDisplayName);
            allMenuItems.Add(menuItem);
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

            if (!studioHackManager.isPoseEditing)
            {
                ApplyPlayData();
            }

            UpdateSe();
        }

        protected override void ApplyMotion(MotionData motion, float t, bool indexUpdated)
        {
            if (indexUpdated)
            {
                var start = motion.start as TransformDataSe;

                var fileName = start.fileName;
                var interval = start.interval;
                var isLoop = start.isLoop;
                PlaySe(fileName, interval, isLoop);
            }
        }

        private string _currentSeName = "";
        private float _currentInterval = 0f;
        private bool _currentIsLoop = false;
        private float _currentTime = 0f;

        private void PlaySe(string fileName, float interval, bool isLoop)
        {
            if (fileName == "")
            {
                soundManager.StopSe();
            }
            else
            {
                soundManager.PlaySe(fileName, isLoop);
            }

            _currentSeName = fileName;
            _currentInterval = interval;
            _currentIsLoop = isLoop;
            _currentTime = 0f;
        }

        private void UpdateSe()
        {
            if (_currentSeName != "" && _currentInterval > 0f && !_currentIsLoop)
            {
                _currentTime += Time.deltaTime;
                if (_currentTime >= _currentInterval)
                {
                    soundManager.PlaySe(_currentSeName, _currentIsLoop);
                    _currentTime = 0f;
                }
            }
        }

        public override void UpdateFrame(FrameData frame)
        {
            var trans = CreateTransformData<TransformDataSe>(SeBoneName);
            trans.fileName = _currentSeName;
            trans.interval = _currentInterval;
            trans.isLoop = _currentIsLoop;

            var bone = frame.CreateBone(trans);
            frame.UpdateBone(bone);
        }

        public void OutputMotions(
            List<MotionData> motions,
            string filePath)
        {
            var offsetTime = timeline.startOffsetTime;

            var builder = new StringBuilder();
            builder.Append("fileName,sTime,eTime,interval,isLoop" +
                            "\r\n");

            Action<MotionData> appendMotion = motion =>
            {
                var stTime = motion.stFrame * timeline.frameDuration;
                var edTime = motion.edFrame * timeline.frameDuration;

                stTime += offsetTime;
                edTime += offsetTime;

                var start = motion.start as TransformDataSe;

                var interval = start.interval == 0f ? timeline.maxFrameNo * timeline.frameDuration : start.interval;

                builder.Append(start.fileName + ",");
                builder.Append(stTime.ToString("0.000") + ",");
                builder.Append(edTime.ToString("0.000") + ",");
                builder.Append(interval.ToString("0.000") + ",");
                builder.Append(start.isLoop ? "1" : "0");
                builder.Append("\r\n");
            };

            foreach (var motion in motions)
            {
                appendMotion(motion);
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
                var motions = _playDataMap[SeBoneName].motions;

                var outputFileName = "se.csv";
                var outputPath = timeline.GetDcmSongFilePath(outputFileName);
                OutputMotions(motions, outputPath);

                songElement.Add(new XElement("changeSe", outputFileName));
            }
            catch (Exception e)
            {
                MTEUtils.LogException(e);
                MTEUtils.LogError("効果音の出力に失敗しました");
            }
        }

        private GUIComboBox<string> _seNameComboBox = new GUIComboBox<string>
        {
            getName = (seName, index) =>
            {
                return seName;
            },
        };

        private enum TabType
        {
            操作,
            管理,
        }

        private TabType _tabType = TabType.操作;

        public override void DrawWindow(GUIView view)
        {
            view.SetEnabled(!view.IsComboBoxFocused());

            _tabType = view.DrawTabs(_tabType, 50, 20);

            switch (_tabType)
            {
                case TabType.操作:
                    DrawSeControl(view);
                    break;
                case TabType.管理:
                    DrawSeManage(view);
                    break;
            }
        }

        public void DrawSeControl(GUIView view)
        {
            view.SetEnabled(!view.IsComboBoxFocused() && studioHackManager.isPoseEditing);

            bool updated = false;

            _seNameComboBox.items = _seNames;
            if (_seNameComboBox.currentItem != _currentSeName)
            {
                _seNameComboBox.currentIndex = _seNameComboBox.items.IndexOf(_currentSeName);
            }
            _seNameComboBox.onSelected = (seName, _) =>
            {
                _currentSeName = seName;
                updated = true;
            };

            _seNameComboBox.DrawButton("SE名", view);

            updated |= view.DrawSliderValue(
                new GUIView.SliderOption
                {
                    label = "再生間隔",
                    labelWidth = 60,
                    min = 0f,
                    max = config.voiceMaxLength,
                    step = 0.01f,
                    defaultValue = 0f,
                    value = _currentInterval,
                    onChanged = value => _currentInterval = value,
                });

            view.DrawToggle("ループ", _currentIsLoop, 80, 20, newValue =>
            {
                _currentIsLoop = newValue;
                updated = true;
            });

            view.BeginHorizontal();
            {
                if (view.DrawButton("再生", 100, 20))
                {
                    updated = true;
                }

                if (view.DrawButton("初期化", 100, 20))
                {
                    PlaySe("", 0f, false);
                }
            }
            view.EndLayout();

            if (updated)
            {
                soundManager.StopSe();
                PlaySe(_currentSeName, _currentInterval, _currentIsLoop);
            }
        }

        private string _additionalSeName = "";

        public void DrawSeManage(GUIView view)
        {
            view.SetEnabled(!view.IsComboBoxFocused());

            view.DrawTextField(new GUIView.TextFieldOption
            {
                label = "SE名",
                labelWidth = 50,
                value = _additionalSeName,
                onChanged = value => _additionalSeName = value,
            });

            if (view.DrawButton("追加", 100, 20))
            {
                if (_additionalSeName != "")
                {
                    if (!_additionalSeName.EndsWith(".ogg"))
                    {
                        _additionalSeName += ".ogg";
                    }

                    if (!config.additionalSeNames.Contains(_additionalSeName))
                    {
                        config.additionalSeNames.Add(_additionalSeName);
                        config.dirty = true;
                        UpdateSeNames();
                    }

                    _additionalSeName = "";
                }
            }

            view.DrawHorizontalLine(Color.gray);

            view.BeginScrollView();
            {
                for (var i = 0; i < config.additionalSeNames.Count; i++)
                {
                    var seName = config.additionalSeNames[i];

                    view.BeginHorizontal();
                    {
                        view.DrawLabel(seName, view.viewRect.width - 50 - 10, 20);

                        if (view.DrawButton("削除", 50, 20))
                        {
                            config.additionalSeNames.Remove(seName);
                            config.dirty = true;
                            UpdateSeNames();
                            break;
                        }
                    }
                    view.EndLayout();
                }
            }
            view.EndScrollView();
        }

        public override SingleFrameType GetSingleFrameType(TransformType transformType)
        {
            return SingleFrameType.None;
        }

        public override TransformType GetTransformType(string name)
        {
            return TransformType.Se;
        }
    }
}