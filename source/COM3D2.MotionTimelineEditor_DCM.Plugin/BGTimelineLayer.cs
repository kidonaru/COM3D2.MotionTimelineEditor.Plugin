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
    using BGPlayData = MotionPlayData<BGMotionData>;

    public class BGTimeLineRow
    {
        public int frame;
        public string name;
        public int group;
        public Vector3 position;
        public Vector3 rotation;
        public Vector3 scale;
    }

    public class BGMotionData : IMotionData
    {
        public int stFrame { get; set; }
        public int edFrame { get; set; }

        public string name;
        public MyTransform myTm;
    }

    [LayerDisplayName("背景")]
    public partial class BGTimelineLayer : TimelineLayerBase
    {
        public override int priority
        {
            get
            {
                return 22;
            }
        }

        public override string className
        {
            get
            {
                return typeof(BGTimelineLayer).Name;
            }
        }

        public override bool hasSlotNo
        {
            get
            {
                return false;
            }
        }

        public override bool isCameraLayer
        {
            get
            {
                return false;
            }
        }

        private List<string> _allBoneNames = new List<string>();
        public override List<string> allBoneNames
        {
            get
            {
                return _allBoneNames;
            }
        }

        private Dictionary<string, List<BGTimeLineRow>> _timelineRowsMap = new Dictionary<string, List<BGTimeLineRow>>();
        private Dictionary<string, BGPlayData> _playDataMap = new Dictionary<string, BGPlayData>();
        private List<BGTimeLineRow> _outputRows = new List<BGTimeLineRow>(128);

        private static BgMgr bgMgr
        {
            get
            {
                return GameMain.Instance.BgMgr;
            }
        }

        private static GameObject bgObject
        {
            get
            {
                return bgMgr.current_bg_object;
            }
        }

        public BGTimelineLayer()
        {
        }

        public static BGTimelineLayer Create(int slotNo)
        {
            PluginUtils.LogDebug("BGTimelineLayer.Create");
            return new BGTimelineLayer();
        }

        public override void Init()
        {
            base.Init();
        }

        public override void Dispose()
        {
            base.Dispose();
        }

        protected override void InitMenuItems()
        {
            {
                var boneNameSet = new HashSet<string>(GetExistBoneNames());

                var currentBgName = bgMgr.GetBGName();
                if (!string.IsNullOrEmpty(currentBgName))
                {
                    boneNameSet.Add(currentBgName);
                }

                _allBoneNames.Clear();
                _allBoneNames.AddRange(boneNameSet);
            }

            {
                allMenuItems.Clear();

                foreach (var boneName in allBoneNames)
                {
                    var menuItem = new BoneMenuItem(boneName, boneName);
                    allMenuItems.Add(menuItem);
                }
            }
        }

        private void ApplyPlayData()
        {
            var playingFrameNoFloat = this.playingFrameNoFloat;

            foreach (var bgName in _playDataMap.Keys)
            {
                var playData = _playDataMap[bgName];

                var indexUpdated = playData.Update(playingFrameNoFloat);

                var current = playData.current;
                if (current != null && indexUpdated)
                {
                    ApplyBG(current);
                }
            }
        }

        public override bool IsValidData()
        {
            errorMessage = "";
            return true;
        }

        private string _prevBgName = null;

        public override void Update()
        {
            base.Update();

            if (studioHack.isPoseEditing)
            {
                var bgName = bgMgr.GetBGName();
                if (bgName != _prevBgName)
                {
                    OnBGChanged();
                    _prevBgName = bgName;
                }
            }
            else
            {
                _prevBgName = null;
            }

            if (!studioHack.isPoseEditing)
            {
                ApplyPlayData();
            }
        }

        public override void LateUpdate()
        {
            base.LateUpdate();
        }

        private void ApplyBG(BGMotionData motion)
        {
            if (motion == null)
            {
                return;
            }

            PluginUtils.LogDebug("ApplyBG: bgName={0} stFrame={1}, stPos={2}, stRot={3}",
                motion.name, motion.stFrame, motion.myTm.stPos, motion.myTm.stRot);

            try
            {
                if (motion.name != bgMgr.GetBGName())
                {
                    studioHack.ChangeBackground(motion.name);
                }

                studioHack.SetBackgroundVisible(timeline.isBackgroundVisible);

                if (bgObject != null)
                {
                    bgObject.transform.position = motion.myTm.stPos;
                    bgObject.transform.eulerAngles = motion.myTm.stRot;
                    bgObject.transform.localScale = motion.myTm.stSca;
                }
            }
            catch (Exception e)
            {
                PluginUtils.LogException(e);
                PluginUtils.LogError("選択された背景が導入されていません: " + motion.name);
            }
        }

        public override void UpdateFrameWithCurrentStat(FrameData frame)
        {
            var bgName = bgMgr.GetBGName();

            var trans = CreateTransformData(bgName);
            if (bgObject != null)
            {
                trans.position = bgObject.transform.position;
                trans.eulerAngles = bgObject.transform.eulerAngles;
                trans.scale = bgObject.transform.localScale;
            }

            var bone = frame.CreateBone(trans);
            frame.SetBone(bone);
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

        private void AddMotion(FrameData frame)
        {
            foreach (var name in allBoneNames)
            {
                var bone = frame.GetBone(name);
                if (bone == null)
                {
                    continue;
                }

                List<BGTimeLineRow> rows;
                if (!_timelineRowsMap.TryGetValue(name, out rows))
                {
                    rows = new List<BGTimeLineRow>();
                    _timelineRowsMap[name] = rows;
                }

                var trans = bone.transform;

                var row = new BGTimeLineRow
                {
                    frame = frame.frameNo,
                    name = bone.name,
                    position = trans.position,
                    rotation = trans.eulerAngles,
                    scale = trans.scale,
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

                BGPlayData playData;
                if (!_playDataMap.TryGetValue(name, out playData))
                {
                    playData = new BGPlayData
                    {
                        motions = new List<BGMotionData>(rows.Count),
                    };
                    _playDataMap[name] = playData;
                }

                playData.ResetIndex();
                playData.motions.Clear();

                for (var i = 0; i < rows.Count; i++)
                {
                    var start = rows[i];
                    var stFrame = start.frame;

                    var motion = new BGMotionData
                    {
                        name = name,
                        stFrame = stFrame,
                        edFrame = stFrame,
                        myTm = new MyTransform
                        {
                            stPos = start.position,
                            stRot = start.rotation,
                            stSca = start.scale,
                        },
                    };

                    playData.motions.Add(motion);
                }
            }

            foreach (var pair in _playDataMap)
            {
                var name = pair.Key;
                var playData = pair.Value;
                PluginUtils.LogDebug("PlayData: name={0}, count={1}", name, playData.motions.Count);
            }
        }

        protected override byte[] GetAnmBinaryInternal(bool forOutput, int startFrameNo, int endFrameNo)
        {
            _timelineRowsMap.Clear();

            foreach (var keyFrame in keyFrames)
            {
                AddMotion(keyFrame);
            }

            BuildPlayData(forOutput);
            return null;
        }

        public void SaveBGTimeLine(
            List<BGTimeLineRow> rows,
            string filePath)
        {
            var offsetTime = timeline.startOffsetTime;

            var builder = new StringBuilder();
            builder.Append("bgName,group,time,posX,posY,posZ,rotX,rotY,rotZ,scale\r\n");

            Action<BGTimeLineRow, bool> appendRow = (row, isFirst) =>
            {
                var time = row.frame * timeline.frameDuration;

                if (isFirst)
                {
                    time = 0;
                }
                else
                {
                    time += offsetTime;
                }

                builder.Append(row.name + ",");
                builder.Append(row.group + ",");
                builder.Append(time.ToString("0.000") + ",");
                builder.Append(row.position.x.ToString("0.000") + ",");
                builder.Append(row.position.y.ToString("0.000") + ",");
                builder.Append(row.position.z.ToString("0.000") + ",");
                builder.Append(row.rotation.x.ToString("0.000") + ",");
                builder.Append(row.rotation.y.ToString("0.000") + ",");
                builder.Append(row.rotation.z.ToString("0.000") + ",");
                builder.Append(row.scale.x.ToString("0.000"));
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
                _outputRows.Clear();

                foreach (var rows in _timelineRowsMap.Values)
                {
                    _outputRows.AddRange(rows);
                }

                var outputFileName = "bg.csv";
                var outputPath = timeline.GetDcmSongFilePath(outputFileName);
                SaveBGTimeLine(_outputRows, outputPath);

                if (timeline.isBackgroundVisible)
                {
                    songElement.Add(new XElement("changeBg", outputFileName));
                }
            }
            catch (Exception e)
            {
                PluginUtils.LogException(e);
                PluginUtils.ShowDialog("背景チェンジの出力に失敗しました");
            }
        }

        public override float CalcEasingValue(float t, int easing)
        {
            return TimelineMotionEasing.MotionEasing(t, (EasingType) easing);
        }

        private FloatFieldValue[] _fieldValues = FloatFieldValue.CreateArray(
            new string[] { "X", "Y", "Z", "RX", "RY", "RZ", "Scale" }
        );

        public override void DrawWindow(GUIView view)
        {
            if (bgObject == null)
            {
                return;
            }

            var position = bgObject.transform.position;
            var angle = bgObject.transform.eulerAngles;
            var scale = bgObject.transform.localScale.x;
            var updateTransform = false;

            GUI.enabled = studioHack.isPoseEditing;

            updateTransform |= view.DrawValue(_fieldValues[0], 0.01f, 0.1f, 0f,
                position.x,
                x => position.x = x,
                x => position.x += x);

            updateTransform |= view.DrawValue(_fieldValues[1], 0.01f, 0.1f, 0f,
                position.y,
                y => position.y = y,
                y => position.y += y);

            updateTransform |= view.DrawValue(_fieldValues[2], 0.01f, 0.1f, 0f,
                position.z,
                z => position.z = z,
                z => position.z += z);

            updateTransform |= view.DrawValue(_fieldValues[3], 1f, 10f, 0f,
                angle.x,
                x => angle.x = x,
                x => angle.x += x);

            updateTransform |= view.DrawValue(_fieldValues[4], 1f, 10f, 0f,
                angle.y,
                y => angle.y = y,
                y => angle.y += y);

            updateTransform |= view.DrawValue(_fieldValues[5], 1f, 10f, 0f,
                angle.z,
                z => angle.z = z,
                z => angle.z += z);

            updateTransform |= view.DrawValue(_fieldValues[6], 0.01f, 0.1f, 1f,
                scale,
                x => scale = x,
                x => scale += x);

            view.DrawHorizontalLine(Color.gray);

            /*var isBgVisible = studioHack.IsBackgroundVidible();
            var newIsBgVisible = view.DrawToggle("背景表示", isBgVisible, 80, 20);
            if (newIsBgVisible != isBgVisible)
            {
                studioHack.SetBackgroundVidible(newIsBgVisible);
            }*/

            GUI.enabled = true;

            if (updateTransform)
            {
                bgObject.transform.position = position;
                bgObject.transform.eulerAngles = angle;
                bgObject.transform.localScale = new Vector3(scale, scale, scale);
            }
        }

        public override ITransformData CreateTransformData(string name)
        {
            var transform = new TransformDataBG();
            transform.Initialize(name);
            return transform;
        }

        private void OnBGChanged()
        {
            InitMenuItems();
        }
    }
}