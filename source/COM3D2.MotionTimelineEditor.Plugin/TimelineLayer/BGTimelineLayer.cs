using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using UnityEngine;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    [TimelineLayerDesc("背景", 31)]
    public partial class BGTimelineLayer : TimelineLayerBase
    {
        public override Type layerType => typeof(BGTimelineLayer);
        public override string layerName => nameof(BGTimelineLayer);

        private List<string> _allBoneNames = new List<string>();
        public override List<string> allBoneNames => _allBoneNames;

        private static BgMgr bgMgr => GameMain.Instance.BgMgr;

        private static GameObject bgObject => bgMgr.current_bg_object;

        private BGTimelineLayer(int slotNo) : base(slotNo)
        {
        }

        public static BGTimelineLayer Create(int slotNo)
        {
            return new BGTimelineLayer(0);
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
                    var displayName = photoBGManager.GetDisplayName(boneName);
                    var menuItem = new BoneMenuItem(boneName, displayName);
                    allMenuItems.Add(menuItem);
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

            if (studioHackManager.isPoseEditing)
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

            if (!studioHackManager.isPoseEditing)
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
            if (!indexUpdated)
            {
                return;
            }

            //MTEUtils.LogDebug("ApplyMotion: bgName={0} stFrame={1}, stPos={2}, stRot={3}",
            //    motion.name, motion.stFrame, motion.myTm.stPos, motion.myTm.stRot);

            try
            {
                if (motion.name != bgMgr.GetBGName())
                {
                    studioHack.ChangeBackground(motion.name);
                }

                studioHack.SetBackgroundVisible(timeline.isBackgroundVisible);

                var start = motion.start;

                if (bgObject != null)
                {
                    bgObject.transform.localPosition = start.position;
                    bgObject.transform.localEulerAngles = start.eulerAngles;
                    bgObject.transform.localScale = start.scale;
                }
            }
            catch (Exception e)
            {
                MTEUtils.LogException(e);
                MTEUtils.LogError("選択された背景が導入されていません: " + motion.name);
            }
        }

        public override void UpdateFrame(FrameData frame, bool initialEdit, bool force)
        {
            var bgName = bgMgr.GetBGName();

            var trans = CreateTransformData<TransformDataBG>(bgName);
            if (bgObject != null)
            {
                trans.position = bgObject.transform.localPosition;
                trans.eulerAngles = bgObject.transform.localEulerAngles;
                trans.scale = bgObject.transform.localScale;
            }

            var bone = frame.CreateBone(trans);
            frame.SetBone(bone);
        }

        public void OutputBones(
            List<BoneData> rows,
            string filePath)
        {
            var offsetTime = timeline.startOffsetTime;

            var builder = new StringBuilder();
            builder.Append("bgName,group,time,posX,posY,posZ,rotX,rotY,rotZ,scale\r\n");

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

                builder.Append(row.name + ",");
                builder.Append(0 + ","); // group
                builder.Append(time.ToString("0.000") + ",");
                builder.Append(transform.position.x.ToString("0.000") + ",");
                builder.Append(transform.position.y.ToString("0.000") + ",");
                builder.Append(transform.position.z.ToString("0.000") + ",");
                builder.Append(transform.eulerAngles.x.ToString("0.000") + ",");
                builder.Append(transform.eulerAngles.y.ToString("0.000") + ",");
                builder.Append(transform.eulerAngles.z.ToString("0.000") + ",");
                builder.Append(transform.scale.x.ToString("0.000"));
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
                var outputRows = new List<BoneData>(64);

                foreach (var rows in _timelineBonesMap.Values)
                {
                    outputRows.AddRange(rows);
                }

                var outputFileName = "bg.csv";
                var outputPath = timeline.GetDcmSongFilePath(outputFileName);
                OutputBones(outputRows, outputPath);

                if (timeline.isBackgroundVisible)
                {
                    songElement.Add(new XElement("changeBg", outputFileName));
                }
            }
            catch (Exception e)
            {
                MTEUtils.LogException(e);
                MTEUtils.LogError("背景チェンジの出力に失敗しました");
            }
        }

        private GUIComboBox<PhotoBGData> _bgComboBox = new GUIComboBox<PhotoBGData>
        {
            items = photoBGManager.bgList,
            getName = (data, index) => data.name,
            onSelected = (data, index) =>
            {
                studioHack.ChangeBackground(data.create_prefab_name);
            },
            contentSize = new Vector2(200, 300),
        };

        public override void DrawWindow(GUIView view)
        {
            if (bgObject == null || bgObject.transform == null)
            {
                return;
            }

            var transform = bgObject.transform;
            var boneName = bgMgr.GetBGName();

            var initialPosition = Vector3.zero;
            var initialEulerAngles = Vector3.zero;
            var initialScale = Vector3.one;

            view.SetEnabled(!view.IsComboBoxFocused() && studioHackManager.isPoseEditing);

            _bgComboBox.currentIndex = photoBGManager.GetBGIndex(boneName);
            _bgComboBox.DrawButton("背景", view);

            view.DrawLabel(boneName, -1, 20);

            view.DrawHorizontalLine(Color.gray);

            DrawTransform(
                view,
                transform,
                TransformEditType.全て,
                DrawMaskAll,
                boneName,
                initialPosition,
                initialEulerAngles,
                initialScale);
        }

        public override SingleFrameType GetSingleFrameType(TransformType transformType)
        {
            return SingleFrameType.None;
        }

        public override TransformType GetTransformType(string name)
        {
            return TransformType.BG;
        }

        public override void UpdateBones(int frameNo, IEnumerable<BoneData> bones)
        {
            // 背景は常に前のフレームをクリアしてから更新する
            var frame = GetOrCreateFrame(frameNo);
            frame.ClearBones();
            frame.UpdateBones(bones);
        }

        private void OnBGChanged()
        {
            InitMenuItems();
        }
    }
}