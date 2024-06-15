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
    using EyesPlayData = MotionPlayData<EyesMotionData>;

    public class EyesTimeLineRow
    {
        public int frame;
        public string name;
        public float horizon;
        public float vertical;
        public int easing;
    }

    public class EyesMotionData : IMotionData
    {
        public int stFrame { get; set; }
        public int edFrame { get; set; }

        public string name;
        public float startHorizon;
        public float startVertical;
        public float endHorizon;
        public float endVertical;
        public int easing;
    }

    [LayerDisplayName("メイド瞳")]
    public class EyesTimelineLayer : TimelineLayerBase
    {
        public override int priority
        {
            get
            {
                return 12;
            }
        }

        public override string className
        {
            get
            {
                return typeof(EyesTimelineLayer).Name;
            }
        }

        public override bool hasSlotNo
        {
            get
            {
                return true;
            }
        }

        public static readonly Dictionary<string, SongEyesType> EyesTypeMap = new Dictionary<string, SongEyesType>
        {
            { "EyesPosL", SongEyesType.EyesPosL },
            { "EyesPosR", SongEyesType.EyesPosR },
            { "EyesScaL", SongEyesType.EyesScaL },
            { "EyesScaR", SongEyesType.EyesScaR },
        };

        public static readonly Dictionary<string, string> EyesDisplayNameMap = new Dictionary<string, string>
        {
            { "EyesPosL", "左瞳位置" },
            { "EyesPosR", "右瞳位置" },
            { "EyesScaL", "左瞳サイズ" },
            { "EyesScaR", "右瞳サイズ" },
        };

        private static List<string> _saveEyesNames = null;
        public static List<string> saveEyesNames
        {
            get
            {
                if (_saveEyesNames == null)
                {
                    _saveEyesNames = EyesTypeMap.Keys.ToList();
                }
                return _saveEyesNames;
            }
        }

        public override List<string> allBoneNames
        {
            get
            {
                return saveEyesNames;
            }
        }

        public override bool isDragging
        {
            get
            {
                return _isEyesDragging;
            }
        }

        private Dictionary<string, List<EyesTimeLineRow>> _timelineRowsMap = new Dictionary<string, List<EyesTimeLineRow>>();
        private Dictionary<string, EyesPlayData> _playDataMap = new Dictionary<string, EyesPlayData>();
        private List<EyesMotionData> _dcmOutputMotions = new List<EyesMotionData>(128);

        private EyesTimelineLayer(int slotNo) : base(slotNo)
        {
        }

        public static EyesTimelineLayer Create(int slotNo)
        {
            return new EyesTimelineLayer(slotNo);
        }

        protected override void InitMenuItems()
        {
            _allMenuItems.Clear();

            foreach (var pair in EyesDisplayNameMap)
            {
                var eyesName = pair.Key;
                var displayName = pair.Value;

                var menuItem = new BoneMenuItem(eyesName, displayName);
                _allMenuItems.Add(menuItem);
            }
        }

        public override bool IsValidData()
        {
            errorMessage = "";

            var firstFrame = this.firstFrame;
            if (firstFrame == null || firstFrame.frameNo != 0)
            {
                errorMessage = "0フレーム目にキーフレームが必要です";
                return false;
            }

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
            var maid = this.maid;
            if (maid == null || maid.body0 == null || !maid.body0.isLoadedBody)
            {
                return;
            }

            var playingFrameNoFloat = this.playingFrameNoFloat;

            foreach (var pair in _playDataMap)
            {
                var eyesName = pair.Key;
                var playData = pair.Value;

                playData.Update(playingFrameNoFloat);

                var current = playData.current;
                if (current != null)
                {
                    float easingValue = CalcEasingValue(playData.lerpFrame, current.easing);
                    float horizon = Mathf.Lerp(current.startHorizon, current.endHorizon, easingValue);
                    float vertical = Mathf.Lerp(current.startVertical, current.endVertical, easingValue);

                    TransformEyes(eyesName, horizon, vertical);
                }
            }

            //PluginUtils.LogDebug("ApplyPlayData: lerpFrame={0}, listIndex={1}", playData.lerpFrame, playData.listIndex);
        }

        private void TransformEyes(string eyesName, float horizon, float vertical)
        {
            var eyesType = EyesTypeMap[eyesName];
            switch (eyesType)
            {
                case SongEyesType.EyesPosL:
                    maidCache.eyesPosL = new Vector3(0f, vertical / 100f, horizon / 100f);
                    break;
                case SongEyesType.EyesPosR:
                    maidCache.eyesPosR = new Vector3(0f, vertical / 100f, horizon / 100f);
                    break;
                case SongEyesType.EyesScaL:
                    maidCache.eyesScaL = new Vector3(0f, vertical, horizon);
                    break;
                case SongEyesType.EyesScaR:
                    maidCache.eyesScaR = new Vector3(0f, vertical, horizon);
                    break;
            }
        }

        private Vector2 GetEyesValue(string eyesName)
        {
            var maid = this.maid;
            if (maid == null || maid.body0 == null || !maid.body0.isLoadedBody)
            {
                return Vector2.zero;
            }

            var eyesType = EyesTypeMap[eyesName];
            switch (eyesType)
            {
                case SongEyesType.EyesPosL:
                {
                    var pos = maidCache.eyesPosL;
                    return new Vector2(pos.z * 100f, pos.y * 100f);
                }
                case SongEyesType.EyesPosR:
                {
                    var pos = maidCache.eyesPosR;
                    return new Vector2(pos.z * 100f, pos.y * 100f);
                }
                case SongEyesType.EyesScaL:
                {
                    var sca = maidCache.eyesScaL;
                    return new Vector2(sca.z, sca.y);
                }
                case SongEyesType.EyesScaR:
                {
                    var sca = maidCache.eyesScaR;
                    return new Vector2(sca.z, sca.y);
                }
            }

            return Vector2.zero;
        }

        public override void UpdateFrame(FrameData frame)
        {
            foreach (var eyesName in allBoneNames)
            {
                var eyesValue = GetEyesValue(eyesName);

                var trans = CreateTransformData(eyesName);
                trans.easing = GetEasing(frame.frameNo, eyesName);
                trans["horizon"].value = eyesValue.x;
                trans["vertical"].value = eyesValue.y;

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
                AppendTimelineRow(keyFrame);
            }

            AppendTimelineRow(_dummyLastFrame);

            BuildPlayData();
            return null;
        }

        private void AppendTimelineRow(FrameData frame)
        {
            var isLastFrame = frame.frameNo == maxFrameNo;
            foreach (var name in firstFrame.boneNames)
            {
                List<EyesTimeLineRow> rows;
                if (!_timelineRowsMap.TryGetValue(name, out rows))
                {
                    rows = new List<EyesTimeLineRow>(16);
                    _timelineRowsMap[name] = rows;
                }

                var bone = frame.GetBone(name);
                if (bone == null)
                {
                    continue;
                }

                // 最後のフレームは2重に追加しない
                if (isLastFrame && rows.Count > 0 && rows.Last().frame == frame.frameNo)
                {
                    continue;
                }

                var row = new EyesTimeLineRow
                {
                    frame = frame.frameNo,
                    name = name,
                    horizon = bone.transform["horizon"].value,
                    vertical = bone.transform["vertical"].value,
                    easing = bone.transform.easing,
                };

                rows.Add(row);
            }
        }
        
        private void BuildPlayData()
        {
            _playDataMap.Clear();

            foreach (var pair in _timelineRowsMap)
            {
                var name = pair.Key;
                var rows = pair.Value;

                EyesPlayData playData;
                if (!_playDataMap.TryGetValue(name, out playData))
                {
                    playData = new EyesPlayData
                    {
                        motions = new List<EyesMotionData>(rows.Count)
                    };
                    _playDataMap[name] = playData;
                }

                foreach (var row in rows)
                {
                    var motion = new EyesMotionData
                    {
                        stFrame = row.frame,
                        edFrame = row.frame,

                        name = row.name,
                        startHorizon = row.horizon,
                        endHorizon = row.horizon,
                        startVertical = row.vertical,
                        endVertical = row.vertical,
                        easing = row.easing,
                    };
                    playData.motions.Add(motion);

                    if (playData.motions.Count >= 2)
                    {
                        int prevIndex = playData.motions.Count() - 2;
                        var prevMotion = playData.motions[prevIndex];
                        prevMotion.edFrame = row.frame;
                        prevMotion.endHorizon = row.horizon;
                        prevMotion.endVertical = row.vertical;
                    }
                }
            }
        }

        public void SaveMotions(
            List<EyesMotionData> motions,
            string filePath)
        {
            var offsetTime = timeline.startOffsetTime;

            var builder = new StringBuilder();
            builder.Append("type,startTime,startHorizon,startVertical,endTime,endHorizon,endVertical,easing" +
                            "\r\n");

            Action<EyesMotionData, bool> appendMotion = (motion, isFirst) =>
            {
                var type = EyesTypeMap[motion.name];

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

                builder.Append((int) type + ",");
                builder.Append(stTime.ToString("0.000") + ",");
                builder.Append(motion.startHorizon.ToString("0.000") + ",");
                builder.Append(motion.startVertical.ToString("0.000") + ",");
                builder.Append(edTime.ToString("0.000") + ",");
                builder.Append(motion.endHorizon.ToString("0.000") + ",");
                builder.Append(motion.endVertical.ToString("0.000") + ",");
                builder.Append(motion.easing);
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
                _dcmOutputMotions.Clear();

                foreach (var playData in _playDataMap.Values)
                {
                    _dcmOutputMotions.AddRange(playData.motions);
                }

                var outputFileName = string.Format("eyes_{0}.csv", slotNo);
                var outputPath = timeline.GetDcmSongFilePath(outputFileName);
                SaveMotions(_dcmOutputMotions, outputPath);

                var maidElement = GetMeidElement(songElement);
                maidElement.Add(new XElement("eyes", outputFileName));
            }
            catch (Exception e)
            {
                PluginUtils.LogException(e);
                PluginUtils.ShowDialog("瞳モーションの出力に失敗しました");
            }
        }

        public override float CalcEasingValue(float t, int easing)
        {
            return TimelineMotionEasing.MotionEasing(t, (EasingType) easing);
        }

        private Texture2D _eyesPositionTex = null;
        private Texture2D _eyesTex = null;
        private bool _isEyesDragging = false;

        private void InitTexture()
        {
            if (_eyesPositionTex == null)
            {
                _eyesPositionTex = new Texture2D(150, 150);
                TextureUtils.ClearTexture(_eyesPositionTex, config.curveBgColor);

                var color1 = config.curveLineColor;
                var color2 = new Color(color1.r, color1.g, color1.b, color1.a * 0.5f);

                // 10x10のグリッドの描画
                for (var i = 1; i < 10; i++)
                {
                    var x = i * 15;
                    TextureUtils.DrawLineTexture(_eyesPositionTex, x, 0, x, 150, color2);
                    TextureUtils.DrawLineTexture(_eyesPositionTex, 0, x, 150, x, color2);
                }

                // 円の描画
                TextureUtils.DrawCircleLineTexture(
                    _eyesPositionTex,
                    150 / 2 - 0.5f,
                    64,
                    config.curveLineColor);
            }

            if (_eyesTex == null)
            {
                _eyesTex = TextureUtils.CreateCircleTexture(
                    config.frameWidth,
                    Color.white);
            }
        }

        public override void DrawWindow(GUIView view)
        {
            var maid = this.maid;
            if (maid == null)
            {
                return;
            }

            InitTexture();

            view.SetEnabled(view.guiEnabled && studioHack.isPoseEditing);

            var basePos = view.currentPos;

            view.DrawTexture(
                _eyesPositionTex,
                150,
                150,
                studioHack.isPoseEditing ? Color.white : Color.gray,
                EventType.MouseDrag,
                pos =>
            {
                _isEyesDragging = true;

                var x = pos.x - 75;
                var y = pos.y - 75;

                var horizon = x / 75f;
                var vertical = y / 75f;

                TransformEyes("EyesPosL", -horizon, -vertical);
                TransformEyes("EyesPosR", horizon, vertical);
            });

            if (_isEyesDragging && !Input.GetMouseButton(0))
            {
                _isEyesDragging = false;
            }

            var halfEyesSize = _eyesTex.width / 2;

            {
                var eyesValue = GetEyesValue("EyesPosL");
                var horizon = eyesValue.x;
                var vertical = eyesValue.y;

                var pos = new Vector2(75 - horizon * 75, 75 - vertical * 75);
                pos.x = Mathf.Clamp(pos.x, 0, 150);
                pos.y = Mathf.Clamp(pos.y, 0, 150);

                view.currentPos = basePos + pos - new Vector2(halfEyesSize, halfEyesSize);
                view.DrawTexture(_eyesTex);
            }

            {
                var eyesValue = GetEyesValue("EyesPosR");
                var horizon = eyesValue.x;
                var vertical = eyesValue.y;

                var pos = new Vector2(75 + horizon * 75, 75 + vertical * 75);
                pos.x = Mathf.Clamp(pos.x, 0, 150);
                pos.y = Mathf.Clamp(pos.y, 0, 150);

                view.currentPos = basePos + pos - new Vector2(halfEyesSize, halfEyesSize);
                view.DrawTexture(_eyesTex);
            }

            view.currentPos = basePos;
            view.currentPos.x += 150 + 10;

            if (view.DrawButton("初期化", 60, 20))
            {
                TransformEyes("EyesPosL", 0, 0);
                TransformEyes("EyesPosR", 0, 0);
                TransformEyes("EyesScaL", 0, 0);
                TransformEyes("EyesScaR", 0, 0);
            }

            view.currentPos = basePos;
            view.currentPos.y += 150;

            for (var i = 0; i < allBoneNames.Count; i++)
            {
                var eyesName = allBoneNames[i];
                var displayName = EyesDisplayNameMap[eyesName];

                var eyesValue = GetEyesValue(eyesName);
                var horizon = eyesValue.x;
                var vertical = eyesValue.y;
                var updateTransform = false;

                view.DrawLabel(displayName, 100, 20);

                var names = i < 2 ? new string[] { "X", "Y" } : new string[] { "幅", "高さ" };

                updateTransform |= view.DrawSliderValue(
                    view.GetFieldCache(names[0]),
                    new GUIView.SliderOption
                    {
                        min = -1f,
                        max = 1f,
                        step = 0.01f,
                        defaultValue = 0f,
                        value = horizon,
                        onChanged = x => horizon = x,
                        labelWidth = 30,
                    });

                updateTransform |= view.DrawSliderValue(
                    view.GetFieldCache(names[1]),
                    new GUIView.SliderOption
                    {
                        min = -1f,
                        max = 1f,
                        step = 0.01f,
                        defaultValue = 0f,
                        value = vertical,
                        onChanged = y => vertical = y,
                        labelWidth = 30,
                    });

                if (updateTransform)
                {
                    TransformEyes(eyesName, horizon, vertical);
                }
            }
        }

        public override ITransformData CreateTransformData(string name)
        {
            var transform = new TransformDataEyes();
            transform.Initialize(name);
            return transform;
        }
    }
}