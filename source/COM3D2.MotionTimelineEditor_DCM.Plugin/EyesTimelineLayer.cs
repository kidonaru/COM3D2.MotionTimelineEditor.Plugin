using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using COM3D2.MotionTimelineEditor.Plugin;
using UnityEngine;

namespace COM3D2.MotionTimelineEditor_DCM.Plugin
{
    using TimelineMotionEasing = DanceCameraMotion.Plugin.TimelineMotionEasing;
    using EasingType = DanceCameraMotion.Plugin.EasingType;
    using EyesPlayData = MotionPlayData<EyesMotionData>;

    public enum MotionEyesType
    {
        EyesPosL,
        EyesPosR,
        EyesScaL,
        EyesScaR,
        EyesRot,
        LookAtTarget,
    }

    public class EyesTimeLineRow
    {
        public int frame;
        public string name;
        public float horizon;
        public float vertical;
        public int easing;
        public LookAtTargetType targetType;
        public int targetIndex;
        public MaidPointType maidPointType;
    }

    public class EyesMotionData : MotionDataBase
    {
        public string name;
        public float startHorizon;
        public float startVertical;
        public float endHorizon;
        public float endVertical;
        public int easing;
        public LookAtTargetType targetType;
        public int targetIndex;
        public MaidPointType maidPointType;
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

        public static readonly Dictionary<string, MotionEyesType> EyesTypeMap = new Dictionary<string, MotionEyesType>
        {
            { "EyesPosL", MotionEyesType.EyesPosL },
            { "EyesPosR", MotionEyesType.EyesPosR },
            { "EyesScaL", MotionEyesType.EyesScaL },
            { "EyesScaR", MotionEyesType.EyesScaR },
            { "EyesRot", MotionEyesType.EyesRot },
            { "LookAtTarget", MotionEyesType.LookAtTarget },
        };

        public static readonly Dictionary<string, string> EyesDisplayNameMap = new Dictionary<string, string>
        {
            { "EyesPosL", "左瞳位置" },
            { "EyesPosR", "右瞳位置" },
            { "EyesScaL", "左瞳サイズ" },
            { "EyesScaR", "右瞳サイズ" },
            { "EyesRot", "視線" },
            { "LookAtTarget", "注視" },
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
        private DanceCameraMotion.Plugin.MaidManager _dcmMaidManager = new DanceCameraMotion.Plugin.MaidManager();

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
                var playData = pair.Value;

                playData.Update(playingFrameNoFloat);

                var current = playData.current;
                if (current != null)
                {
                    ApplyMotion(current, playData.lerpFrame);
                }
            }

            //PluginUtils.LogDebug("ApplyPlayData: lerpFrame={0}, listIndex={1}", playData.lerpFrame, playData.listIndex);
        }

        private void ApplyMotion(EyesMotionData motion, float lerpFrame)
        {
            if (motion.name == "LookAtTarget")
            {
                var targetType = motion.targetType;
                var targetIndex = motion.targetIndex;
                var maidPointType = motion.maidPointType;

                ApplyLookAtTarget(targetType, targetIndex, maidPointType);
            }
            else
            {
                float easingValue = CalcEasingValue(lerpFrame, motion.easing);
                float horizon = Mathf.Lerp(motion.startHorizon, motion.endHorizon, easingValue);
                float vertical = Mathf.Lerp(motion.startVertical, motion.endVertical, easingValue);

                var eyesType = EyesTypeMap[motion.name];
                ApplyEyes(eyesType, horizon, vertical);
            }
        }

        private void ApplyEyes(
            MotionEyesType eyesType,
            float horizon,
            float vertical)
        {
            
            switch (eyesType)
            {
                case MotionEyesType.EyesPosL:
                    maidCache.eyesPosL = new Vector3(0f, vertical / 100f, horizon / 100f);
                    break;
                case MotionEyesType.EyesPosR:
                    maidCache.eyesPosR = new Vector3(0f, vertical / 100f, horizon / 100f);
                    break;
                case MotionEyesType.EyesScaL:
                    maidCache.eyesScaL = new Vector3(0f, vertical, horizon);
                    break;
                case MotionEyesType.EyesScaR:
                    maidCache.eyesScaR = new Vector3(0f, vertical, horizon);
                    break;
                case MotionEyesType.EyesRot:
                    maidCache.eyeEulerAngle = new Vector3(horizon * 90, 0f, vertical * 90);
                    break;
            }
        }

        private void ApplyLookAtTarget(
            LookAtTargetType targetType,
            int targetIndex,
            MaidPointType maidPointType)
        {
            var maidCache = this.maidCache;
            if (maidCache == null)
            {
                return;
            }

            maidCache.lookAtTargetType = targetType;
            maidCache.lookAtTargetIndex = targetIndex;
            maidCache.lookAtMaidPointType = maidPointType;
        }

        private Transform GetLookAtTarget(
            LookAtTargetType targetType,
            int targetIndex,
            MaidPointType maidPointType)
        {
            var maid = this.maid;
            if (maid == null)
            {
                return null;
            }

            switch (targetType)
            {
                case LookAtTargetType.Camera:
                    return GameMain.Instance.MainCamera.transform;
                case LookAtTargetType.Maid:
                {
                    var maidCache = maidManager.GetMaidCache(targetIndex);
                    if (maidCache != null)
                    {
                        return maidCache.GetPointTransform(maidPointType);
                    }
                    break;
                }
                case LookAtTargetType.Model:
                {
                    var model = modelManager.GetModel(targetIndex);
                    if (model != null)
                    {
                        return model.transform;
                    }
                    break;
                }
            }

            return null;
        }

        private Vector2 GetEyesValue(MotionEyesType eyesType)
        {
            var maid = this.maid;
            if (maid == null || maid.body0 == null || !maid.body0.isLoadedBody)
            {
                return Vector2.zero;
            }

            switch (eyesType)
            {
                case MotionEyesType.EyesPosL:
                {
                    var pos = maidCache.eyesPosL;
                    return new Vector2(pos.z * 100f, pos.y * 100f);
                }
                case MotionEyesType.EyesPosR:
                {
                    var pos = maidCache.eyesPosR;
                    return new Vector2(pos.z * 100f, pos.y * 100f);
                }
                case MotionEyesType.EyesScaL:
                {
                    var sca = maidCache.eyesScaL;
                    return new Vector2(sca.z, sca.y);
                }
                case MotionEyesType.EyesScaR:
                {
                    var sca = maidCache.eyesScaR;
                    return new Vector2(sca.z, sca.y);
                }
                case MotionEyesType.EyesRot:
                {
                    var rot = maidCache.eyeEulerAngle;
                    return new Vector2(rot.x / 90f, rot.z / 90f);
                }
            }

            return Vector2.zero;
        }

        public override void UpdateFrame(FrameData frame)
        {
            foreach (var eyesName in allBoneNames)
            {
                var eyesType = EyesTypeMap[eyesName];
                ITransformData trans = CreateTransformData(eyesName);

                if (eyesType == MotionEyesType.LookAtTarget)
                {
                    var targetType = maidCache.lookAtTargetType;
                    trans["targetType"].value = (int) targetType;
                    trans["targetIndex"].value = 0;
                    trans["maidPointType"].value = 0;

                    switch (targetType)
                    {
                        case LookAtTargetType.Camera:
                            break;
                        case LookAtTargetType.Maid:
                            trans["targetIndex"].value = maidCache.lookAtTargetIndex;
                            trans["maidPointType"].value = (int) maidCache.lookAtMaidPointType;
                            break;
                        case LookAtTargetType.Model:
                            trans["targetIndex"].value = maidCache.lookAtTargetIndex;
                            break;
                    }
                }
                else
                {
                    var eyesValue = GetEyesValue(eyesType);
                    trans["horizon"].value = eyesValue.x;
                    trans["vertical"].value = eyesValue.y;
                    trans.easing = GetEasing(frame.frameNo, eyesName);
                }

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

                if (name == "LookAtTarget")
                {
                    var targetType = (LookAtTargetType) (int) bone.transform["targetType"].value;
                    var targetIndex = (int) bone.transform["targetIndex"].value;
                    var maidPointType = (MaidPointType) (int) bone.transform["maidPointType"].value;

                    var row = new EyesTimeLineRow
                    {
                        frame = frame.frameNo,
                        name = name,
                        targetType = targetType,
                        targetIndex = targetIndex,
                        maidPointType = maidPointType,
                    };
                    rows.Add(row);
                }
                else
                {
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
                        targetType = row.targetType,
                        targetIndex = row.targetIndex,
                        maidPointType = row.maidPointType,
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

            foreach (var playData in _playDataMap.Values)
            {
                playData.Setup(SingleFrameType.None);
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

                if (type == MotionEyesType.LookAtTarget)
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

        private enum TabType
        {
            視線,
            位置,
        }

        private TabType _tabType = TabType.視線;

        private GUIComboBox<LookAtTargetType> _targetTypeComboBox = new GUIComboBox<LookAtTargetType>
        {
            items = Enum.GetValues(typeof(LookAtTargetType)).Cast<LookAtTargetType>().ToList(),
            getName = (type, index) => TransformDataLookAtTarget.TargetTypeNames[index],
        };

        private GUIComboBox<MaidCache> _maidComboBox = new GUIComboBox<MaidCache>
        {
            getName = (maidCache, _) => maidCache == null ? "未選択" : maidCache.fullName,
            contentSize = new Vector2(150, 300),
        };

        private GUIComboBox<MaidPointType> _maidPointComboBox = new GUIComboBox<MaidPointType>
        {
            items = Enum.GetValues(typeof(MaidPointType)).Cast<MaidPointType>().ToList(),
            getName = (type, index) => MaidCache.GetMaidPointTypeName(type),
        };

        private GUIComboBox<StudioModelStat> _modelComboBox = new GUIComboBox<StudioModelStat>
        {
            getName = (model, index) => model.displayName,
            contentSize = new Vector2(200, 300),
        };

        public override void DrawWindow(GUIView view)
        {
            _tabType = view.DrawTabs(_tabType, 50, 20);

            switch (_tabType)
            {
                case TabType.視線:
                    DrawEyesLookAt(view);
                    break;
                case TabType.位置:
                    DrawEyesPos(view);
                    break;
            }

            view.DrawComboBox();
        }

        private void DrawEyesLookAt(GUIView view)
        {
            var maid = this.maid;
            if (maid == null)
            {
                return;
            }

            if (!timeline.useHeadKey)
            {
                view.DrawLabel("顔/瞳の固定化を有効にしてください", -1, 20);
                return;
            }

            InitTexture();

            view.SetEnabled(!view.IsComboBoxFocused() && studioHack.isPoseEditing);

            _targetTypeComboBox.currentIndex = (int) maidCache.lookAtTargetType;
            _targetTypeComboBox.onSelected = (type, index) => maidCache.lookAtTargetType = type;
            _targetTypeComboBox.DrawButton("注視先", view);

            var targetType = _targetTypeComboBox.currentItem;
            switch (targetType)
            {
                case LookAtTargetType.Maid:
                    _maidComboBox.items = maidManager.maidCaches;
                    _maidComboBox.currentIndex = maidCache.lookAtTargetIndex;
                    _maidComboBox.onSelected = (_, index) => maidCache.lookAtTargetIndex = index;
                    _maidComboBox.DrawButton("メイド", view);

                    _maidPointComboBox.currentIndex = (int) maidCache.lookAtMaidPointType;
                    _maidPointComboBox.onSelected = (type, index) => maidCache.lookAtMaidPointType = type;
                    _maidPointComboBox.DrawButton("ポイント", view);
                    break;
                case LookAtTargetType.Model:
                    _modelComboBox.items = modelManager.models;
                    _modelComboBox.currentIndex = maidCache.lookAtTargetIndex;
                    _modelComboBox.onSelected = (model, index) => maidCache.lookAtTargetIndex = index;
                    _modelComboBox.DrawButton("モデル", view);
                    break;
            }

            view.DrawHorizontalLine(Color.gray);

            view.AddSpace(5);

            view.BeginScrollView();

            var basePos = view.currentPos;

            view.SetEnabled(!view.IsComboBoxFocused() && studioHack.isPoseEditing && targetType == LookAtTargetType.None);

            DrawEyesImage(view, basePos, new MotionEyesType[]
            {
                MotionEyesType.EyesRot,
            });

            view.currentPos = basePos;
            view.currentPos.x += 150 + 10;

            if (view.DrawButton("初期化", 60, 20))
            {
                ApplyEyes(MotionEyesType.EyesRot, 0, 0);
                ApplyLookAtTarget(LookAtTargetType.None, 0, 0);
            }

            view.currentPos = basePos;
            view.currentPos.y += 150;

            var eyesTypes = new MotionEyesType[]
            {
                MotionEyesType.EyesRot,
            };

            foreach (var eyesType in eyesTypes)
            {
                DrawEyesSlider(view, eyesType);
            }

            view.SetEnabled(!view.IsComboBoxFocused());
            view.EndScrollView();
        }

        private void DrawEyesPos(GUIView view)
        {
            var maid = this.maid;
            if (maid == null)
            {
                return;
            }

            InitTexture();

            view.BeginScrollView();

            view.SetEnabled(!view.IsComboBoxFocused() && studioHack.isPoseEditing);

            var basePos = view.currentPos;

            DrawEyesImage(view, basePos, new MotionEyesType[]
            {
                MotionEyesType.EyesPosL,
                MotionEyesType.EyesPosR,
            });

            view.currentPos = basePos;
            view.currentPos.x += 150 + 10;

            if (view.DrawButton("初期化", 60, 20))
            {
                ApplyEyes(MotionEyesType.EyesPosL, 0, 0);
                ApplyEyes(MotionEyesType.EyesPosR, 0, 0);
                ApplyEyes(MotionEyesType.EyesScaL, 0, 0);
                ApplyEyes(MotionEyesType.EyesScaR, 0, 0);
            }

            view.currentPos = basePos;
            view.currentPos.y += 150;

            var eyesTypes = new MotionEyesType[]
            {
                MotionEyesType.EyesPosL,
                MotionEyesType.EyesPosR,
                MotionEyesType.EyesScaL,
                MotionEyesType.EyesScaR,
            };

            foreach (var eyesType in eyesTypes)
            {
                DrawEyesSlider(view, eyesType);
            }

            view.SetEnabled(!view.IsComboBoxFocused());
            view.EndScrollView();
        }

        private void DrawEyesSlider(GUIView view, MotionEyesType eyesType)
        {
            var eyesName = eyesType.ToString();
            var displayName = EyesDisplayNameMap[eyesName];

            var eyesValue = GetEyesValue(eyesType);
            var horizon = eyesValue.x;
            var vertical = eyesValue.y;
            var updateTransform = false;

            view.DrawLabel(displayName, 100, 20);

            string[] names;
            switch (eyesType)
            {
                case MotionEyesType.EyesPosL:
                case MotionEyesType.EyesPosR:
                case MotionEyesType.EyesRot:
                    names = new string[] { "水平", "垂直" };
                    break;
                case MotionEyesType.EyesScaL:
                case MotionEyesType.EyesScaR:
                    names = new string[] { "幅", "高さ" };
                    break;
                default:
                    return;
            }

            updateTransform |= view.DrawSliderValue(
                new GUIView.SliderOption
                {
                    label = names[0],
                    labelWidth = 30,
                    min = -1f,
                    max = 1f,
                    step = 0.01f,
                    defaultValue = 0f,
                    value = horizon,
                    onChanged = x => horizon = x,
                });

            updateTransform |= view.DrawSliderValue(
                new GUIView.SliderOption
                {
                    label = names[1],
                    labelWidth = 30,
                    min = -1f,
                    max = 1f,
                    step = 0.01f,
                    defaultValue = 0f,
                    value = vertical,
                    onChanged = y => vertical = y,
                });

            if (updateTransform)
            {
                ApplyEyes(eyesType, horizon, vertical);
            }
        }

        private void DrawEyesImage(GUIView view, Vector2 basePos, MotionEyesType[] eyesTypes)
        {
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

                    foreach (var eyesType in eyesTypes)
                    {
                        if (eyesType == MotionEyesType.EyesPosL)
                        {
                            ApplyEyes(eyesType, -horizon, -vertical);
                        }
                        else if (eyesType == MotionEyesType.EyesRot)
                        {
                            ApplyEyes(eyesType, horizon, -vertical);
                        }
                        else
                        {
                            ApplyEyes(eyesType, horizon, vertical);
                        }
                    }
                });

            if (_isEyesDragging && !Input.GetMouseButton(0))
            {
                _isEyesDragging = false;
            }

            var halfEyesSize = _eyesTex.width / 2;

            Func<MotionEyesType, Vector2> getImagePosition = eyesType =>
            {
                var eyesValue = GetEyesValue(eyesType);
                var horizon = eyesValue.x;
                var vertical = eyesValue.y;

                if (eyesType == MotionEyesType.EyesPosL)
                {
                    horizon = -horizon;
                    vertical = -vertical;
                }
                else if (eyesType == MotionEyesType.EyesRot)
                {
                    vertical = -vertical;
                }

                var pos = new Vector2(75 + horizon * 75, 75 + vertical * 75);
                pos.x = Mathf.Clamp(pos.x, 0, 150);
                pos.y = Mathf.Clamp(pos.y, 0, 150);

                return basePos + pos - new Vector2(halfEyesSize, halfEyesSize);
            };

            foreach (var eyesType in eyesTypes)
            {
                view.currentPos = getImagePosition(eyesType);
                view.DrawTexture(_eyesTex);
            }
        }

        public override ITransformData CreateTransformData(string name)
        {
            if (name == "LookAtTarget")
            {
                var transform = new TransformDataLookAtTarget();
                transform.Initialize(name);
                return transform;
            }
            else
            {
                var transform = new TransformDataEyes();
                transform.Initialize(name);
                return transform;
            }
        }
    }
}