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
    using TransformType = MotionTimelineEditor.Plugin.TransformType;

    [TimelineLayerDesc("メイド表情", 10)]
    public class MorphTimelineLayer : TimelineLayerBase
    {
        public override string className
        {
            get
            {
                return typeof(MorphTimelineLayer).Name;
            }
        }

        public override bool hasSlotNo => true;

        public override List<string> allBoneNames
        {
            get
            {
                return MorphUtils.saveMorphNames;
            }
        }

        private MaidFaceManager _faceManager = new MaidFaceManager();
        private Dictionary<string, float> _applyMorphMap = new Dictionary<string, float>();

        private bool _isForceUpdate = false;

        private MorphTimelineLayer(int slotNo) : base(slotNo)
        {
        }

        public static MorphTimelineLayer Create(int slotNo)
        {
            return new MorphTimelineLayer(slotNo);
        }

        protected override void InitMenuItems()
        {
            var setMenuItemMap = new Dictionary<string, BoneSetMenuItem>(10);

            foreach (var pair in MorphUtils.MorphNameToSetNameMap)
            {
                var morphName = pair.Key;
                var morphSetName = pair.Value;

                BoneSetMenuItem setMenuItem;
                if (!setMenuItemMap.TryGetValue(morphSetName, out setMenuItem))
                {
                    var displaySetName = MorphUtils.GetMorphSetJpName(morphSetName);
                    setMenuItem = new BoneSetMenuItem(morphSetName, displaySetName);
                    setMenuItemMap[morphSetName] = setMenuItem;
                }

                var displayName = MorphUtils.GetMorphJpName(morphName);

                var menuItem = new BoneMenuItem(morphName, displayName);
                setMenuItem.AddChild(menuItem);
            }

            _allMenuItems = new List<IBoneMenuItem>(
                setMenuItemMap.Values.Cast<IBoneMenuItem>());
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

        protected override void ApplyPlayData()
        {
            var maid = this.maid;
            if (maid == null || maid.body0 == null || !maid.body0.isLoadedBody)
            {
                return;
            }

            _applyMorphMap.Clear();

            base.ApplyPlayData();

            _faceManager.SetMabatakiOff(maid);
            _faceManager.SetMorphValue(maid, _applyMorphMap);

            //PluginUtils.LogDebug("ApplyCamera: lerpFrame={0}, listIndex={1}", playData.lerpFrame, playData.listIndex);
        }

        protected override void ApplyMotion(MotionData motion, float t, bool indexUpdated)
        {
            var start = motion.start as TransformDataMorph;
            var end = motion.end as TransformDataMorph;
            var morphName = motion.name;

            if (indexUpdated)
            {
                _applyMorphMap[morphName] = start.morphValue;
            }

            if (start.morphValue != end.morphValue)
            {
                _applyMorphMap[morphName] = this.Lerp(
                    start.morphValue,
                    end.morphValue,
                    t,
                    morphName);
            }
        }

        private float Lerp(float startValue, float endValue, float lerpFrame, string morphName)
        {
            if (MyConst.FACE_OPTION_MORPH.ContainsKey(morphName) && lerpFrame < 0.99f)
            {
                lerpFrame = 0f;
            }
            return Mathf.Lerp(startValue, endValue, lerpFrame);
        }

        private float GetMorphValue(string morphName)
        {
            var morphValue = 0f;
            if (_isForceUpdate)
            {
                if (_applyMorphMap.TryGetValue(morphName, out morphValue))
                {
                    return morphValue;
                }
                return 0f;
            }

            morphValue = _faceManager.GetMorphValue(this.maid, morphName);

            // 目閉じ補正を戻す
            if (morphName == "eyeclose")
            {
                TMorph morph = maid.body0.Face.morph;
                if (morph != null)
                {
                    var eyeCloseRate = morph.m_fEyeCloseRate;
                    if (eyeCloseRate != 0f && eyeCloseRate != 1f)
                    {
                        morphValue = (morphValue - eyeCloseRate) / (1f - eyeCloseRate);
                        morphValue = Mathf.Clamp01(morphValue);
                    }
                }
            }

            return morphValue;
        }

        private void SetMorphValue(string morphName, float value)
        {
            if (_isForceUpdate)
            {
                _applyMorphMap[morphName] = value;
                return;
            }

            var tmpMorphMap = new Dictionary<string, float>();
            tmpMorphMap[morphName] = value;
            _faceManager.SetMorphValue(maid, tmpMorphMap);
        }

        public override void UpdateFrame(FrameData frame)
        {
            foreach (var name in allBoneNames)
            {
                var morphValue = GetMorphValue(name);

                var trans = CreateTransformData<TransformDataMorph>(name);
                trans.values[0].value = morphValue;

                var bone = frame.CreateBone(trans);
                frame.UpdateBone(bone);
            }
        }

        public void OutputBones(
            List<BoneData> rows,
            string filePath)
        {
            var offsetTime = timeline.startOffsetTime;
            var offsetFrame = (int) Mathf.Round(offsetTime * 30f);
            var frameFactor = 30f / timeline.frameRate;

            var builder = new StringBuilder();
            builder.Append("frame,morphName,morphValue\r\n");

            Action<BoneData, bool> appendRow = (row, isFirst) =>
            {
                int frame = (int) Mathf.Round(row.frameNo * frameFactor);
                if (!isFirst)
                {
                    frame += offsetFrame;
                }

                var trans = row.transform as TransformDataMorph;

                builder.Append(frame + ",");
                builder.Append(row.name + ",");
                builder.Append(trans.morphValue.ToString("0.000"));
                builder.Append("\r\n");
            };

            if (offsetFrame > 0 && rows.Count > 0)
            {
                foreach (var row in rows)
                {
                    if (row.frameNo == 0)
                    {
                        appendRow(row, true);
                    }
                }
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
                var output = new List<BoneData>(64);

                foreach (var rows in _timelineBonesMap.Values)
                {
                    output.AddRange(rows);
                }

                var outputFileName = string.Format("morph_{0}.csv", slotNo);
                var outputPath = timeline.GetDcmSongFilePath(outputFileName);
                OutputBones(output, outputPath);

                var maidElement = GetMeidElement(songElement);
                maidElement.Add(new XElement("morph", outputFileName));
            }
            catch (Exception e)
            {
                PluginUtils.LogException(e);
                PluginUtils.LogError("表情モーションの出力に失敗しました");
            }
        }

        private enum TabType
        {
            目,
            眉,
            口,
            他,
        }

        private TabType _tabType = TabType.目;

        public override void DrawWindow(GUIView view)
        {
            if (maid == null)
            {
                return;
            }

            _tabType = view.DrawTabs(_tabType, 50, 20);

            view.DrawToggle("強制上書き", _isForceUpdate, 150, 20, newValue =>
            {
                _isForceUpdate = newValue;
            });

            view.DrawHorizontalLine(Color.gray);

            view.SetEnabled(!view.IsComboBoxFocused() && studioHack.isPoseEditing);

            DrawMorph(view);
        }

        private void DrawMorph(GUIView view)
        {
            view.DrawLabel(_tabType.ToString(), 80, 20);

            switch (_tabType)
            {
                case TabType.目:
                    foreach (var morphName in MyConst.EYE_MORPH.Keys)
                    {
                        DrawMorphSlider(view, morphName);
                    }
                    break;
                case TabType.眉:
                    foreach (var morphName in MyConst.MAYU_MORPH.Keys)
                    {
                        DrawMorphSlider(view, morphName);
                    }
                    break;
                case TabType.口:
                    foreach (var morphName in MyConst.MOUTH_MORPH.Keys)
                    {
                        DrawMorphSlider(view, morphName);
                    }
                    break;
                case TabType.他:
                    foreach (var morphName in MyConst.FACE_OPTION_MORPH.Keys)
                    {
                        DrawMorphToggle(view, morphName);
                    }
                    break;
            }

            if (studioHack.isPoseEditing && _isForceUpdate)
            {
                _faceManager.SetMorphValue(maid, _applyMorphMap);
            }
        }

        private void DrawMorphSlider(GUIView view, string morphName)
        {
            var displayName = MorphUtils.GetMorphJpName(morphName);
            var morphValue = GetMorphValue(morphName);

            view.DrawSliderValue(
                new GUIView.SliderOption
                {
                    label = displayName,
                    labelWidth = 80,
                    min = 0f,
                    max = 1f,
                    step = 0f,
                    defaultValue = 0f,
                    value = morphValue,
                    onChanged = newValue => SetMorphValue(morphName, newValue),
                });
        }

        private void DrawMorphToggle(GUIView view, string morphName)
        {
            var displayName = MorphUtils.GetMorphJpName(morphName);
            var morphValue = GetMorphValue(morphName);
            var isOn = morphValue >= 1f;

            view.DrawToggle(displayName, isOn, 150, 20, newValue =>
            {
                SetMorphValue(morphName, newValue ? 1f : 0f);
            });
        }

        public override SingleFrameType GetSingleFrameType(TransformType transformType)
        {
            return SingleFrameType.None;
        }

        public override TransformType GetTransformType(string name)
        {
            return TransformType.Morph;
        }
    }
}