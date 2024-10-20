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
    using SePlayData = MotionPlayData<SoundMotionData>;

    public class SeTimeLineRow
    {
        public int frame;
        public string fileName;
        public float interval;
        public bool isLoop;
    }

    public class SoundMotionData : MotionDataBase
    {
        public string fileName;
        public float interval;
        public bool isLoop;
    }

    [LayerDisplayName("効果音")]
    public partial class SeTimelineLayer : TimelineLayerBase
    {
        public override int priority
        {
            get
            {
                return 42;
            }
        }

        public override string className
        {
            get
            {
                return typeof(SeTimelineLayer).Name;
            }
        }

        public static string SeBoneName = "SE";
        public static string SeDisplayName = "効果音";

        private List<string> _allBoneNames = new List<string> { SeBoneName };
        public override List<string> allBoneNames
        {
            get
            {
                return _allBoneNames;
            }
        }

        private static SoundManager soundManager = new SoundManager(false);

        private List<SeTimeLineRow> _timelineRows = new List<SeTimeLineRow>();
        private SePlayData _playData = new SePlayData();

        private SeTimelineLayer(int slotNo) : base(slotNo)
        {
        }

        public static SeTimelineLayer Create(int slotNo)
        {
            return new SeTimelineLayer(0);
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

            if (!studioHack.isPoseEditing)
            {
                ApplyPlayData();
            }

            UpdateSe();
        }

        private void ApplyPlayData()
        {
            var playingFrameNoFloat = this.playingFrameNoFloat;

            var updated = _playData.Update(playingFrameNoFloat);

            if (updated)
            {
                var current = _playData.current;
                ApplyMotion(current);
            }
        }

        private void ApplyMotion(SoundMotionData motion)
        {
            PlaySe(motion.fileName, motion.interval, motion.isLoop);
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

        public override void OnEndPoseEdit()
        {
            base.OnEndPoseEdit();
            ApplyPlayData();
        }

        public override void UpdateFrame(FrameData frame)
        {
            var trans = CreateTransformData(SeBoneName);
            trans.SetStrValue("fileName", _currentSeName);
            trans["interval"].value = _currentInterval;
            trans["isLoop"].boolValue = _currentIsLoop;

            var bone = frame.CreateBone(trans);
            frame.UpdateBone(bone);
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
            _timelineRows.Clear();

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
            var bone = frame.GetBone(SeBoneName);
            if (bone == null)
            {
                return;
            }

            var trans = bone.transform;

            var row = new SeTimeLineRow
            {
                frame = frame.frameNo,
                fileName = trans.GetStrValue("fileName"),
                interval = trans["interval"].value,
                isLoop = trans["isLoop"].boolValue,
            };

            _timelineRows.Add(row);
        }

        private void BuildPlayData(bool forOutput)
        {
            PluginUtils.LogDebug("BuildPlayData");

            _playData.ResetIndex();
            _playData.motions.Clear();

            for (var i = 0; i < _timelineRows.Count - 1; i++)
            {
                var start = _timelineRows[i];
                var end = _timelineRows[i + 1];

                var stFrame = start.frame;
                var edFrame = end.frame;

                var motion = new SoundMotionData
                {
                    stFrame = stFrame,
                    edFrame = edFrame,
                    fileName = start.fileName,
                    interval = start.interval,
                    isLoop = start.isLoop,
                };

                _playData.motions.Add(motion);
            }

            _playData.Setup(SingleFrameType.None);
        }

        public void SaveMotions(
            List<SoundMotionData> motions,
            string filePath)
        {
            var offsetTime = timeline.startOffsetTime;

            var builder = new StringBuilder();
            builder.Append("fileName,sTime,eTime,interval,isLoop" +
                            "\r\n");

            Action<SoundMotionData> appendMotion = motion =>
            {
                var stTime = motion.stFrame * timeline.frameDuration;
                var edTime = motion.edFrame * timeline.frameDuration;

                stTime += offsetTime;
                edTime += offsetTime;

                var interval = motion.interval == 0f ? timeline.maxFrameNo * timeline.frameDuration : motion.interval;

                builder.Append(motion.fileName + ",");
                builder.Append(stTime.ToString("0.000") + ",");
                builder.Append(edTime.ToString("0.000") + ",");
                builder.Append(interval.ToString("0.000") + ",");
                builder.Append(motion.isLoop ? "1" : "0");
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
                var motions = _playData.motions;

                var outputFileName = "se.csv";
                var outputPath = timeline.GetDcmSongFilePath(outputFileName);
                SaveMotions(motions, outputPath);

                songElement.Add(new XElement("changeSe", outputFileName));
            }
            catch (Exception e)
            {
                PluginUtils.LogException(e);
                PluginUtils.ShowDialog("効果音の出力に失敗しました");
            }
        }

        public override float CalcEasingValue(float t, int easing)
        {
            return TimelineMotionEasing.MotionEasing(t, (EasingType) easing);
        }

        private GUIComboBox<string> _seNameComboBox = new GUIComboBox<string>
        {
            items = soundManager.SEData.data,
            getName = (seName, index) =>
            {
                return seName;
            },
        };

        public override void DrawWindow(GUIView view)
        {
            view.SetEnabled(!view.IsComboBoxFocused() && studioHack.isPoseEditing);

            bool updated = false;

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

        public override ITransformData CreateTransformData(string name)
        {
            var transform = new TransformDataSe();
            transform.Initialize(name);
            return transform;
        }
    }
}