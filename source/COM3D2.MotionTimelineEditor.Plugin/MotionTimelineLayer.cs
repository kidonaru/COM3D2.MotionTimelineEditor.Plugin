using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using UnityEngine;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    public class PoseTimeLineRow
    {
        public float time;
        public int poseType;
        public string animation;
        public float fadeTime;
        public float speed;
        public Vector3 position;
        public Vector3 rotation;
        public Maid.EyeMoveType eyeMoveType;
        public string option;
    }

    [LayerDisplayName("メイドアニメ")]
    public class MotionTimelineLayer : TimelineLayerBase
    {
        public override int priority
        {
            get
            {
                return 0;
            }
        }

        public override string className
        {
            get
            {
                return typeof(MotionTimelineLayer).Name;
            }
        }

        public override bool hasSlotNo
        {
            get
            {
                return true;
            }
        }

        private List<string> _allBoneNames = null;
        public override List<string> allBoneNames
        {
            get
            {
                if (_allBoneNames == null)
                {
                    _allBoneNames = new List<string>(BoneUtils.saveBoneNames);
                    _allBoneNames.AddRange(timeline.GetExtendBoneNames(slotNo));
                }
                return _allBoneNames;
            }
        }

        private List<PoseTimeLineRow> _dcmOutputRows = new List<PoseTimeLineRow>();

        private MotionTimelineLayer(int slotNo) : base(slotNo)
        {
        }

        public static MotionTimelineLayer Create(int slotNo)
        {
            return new MotionTimelineLayer(slotNo);
        }

        protected override void InitMenuItems()
        {
            _allMenuItems.Clear();
            _allBoneNames = null;

            var setMenuItemMap = new Dictionary<BoneSetMenuType, BoneSetMenuItem>(12);

            foreach (var pair in BoneUtils.BoneTypeToSetMenuTypeMap)
            {
                var boneType = pair.Key;
                var boneSetType = pair.Value;

                var boneName = BoneUtils.GetBoneName(boneType);
                var displayName = BoneUtils.GetBoneJpName(boneType);
                var menuItem = new BoneMotionMenuItem(boneName, displayName);

                if (boneSetType == BoneSetMenuType.None)
                {
                    _allMenuItems.Add(menuItem);
                    continue;
                }

                BoneSetMenuItem setMenuItem;
                if (!setMenuItemMap.TryGetValue(boneSetType, out setMenuItem))
                {
                    var boneSetName = boneSetType.ToString();
                    var displaySetName = BoneUtils.GetBoneSetMenuJpName(boneSetType);
                    setMenuItem = new BoneSetMenuItem(boneSetName, displaySetName);
                    setMenuItemMap[boneSetType] = setMenuItem;
                    _allMenuItems.Add(setMenuItem);
                }

                setMenuItem.AddChild(menuItem);
            }

            var slotMenuItemMap = new Dictionary<string, BoneSetMenuItem>(12);

            foreach (var extendBoneName in timeline.GetExtendBoneNames(slotNo))
            {
                var entity = maidCache.extendBoneCache.GetEntity(extendBoneName);
                if (entity == null)
                {
                    continue;
                }

                var slotName = entity.slotName;
                var boneName = entity.boneName;

                var menuItem = new BoneMotionMenuItem(extendBoneName, boneName);

                BoneSetMenuItem setMenuItem;
                if (!slotMenuItemMap.TryGetValue(slotName, out setMenuItem))
                {
                    setMenuItem = new BoneSetMenuItem(slotName, slotName);
                    slotMenuItemMap[slotName] = setMenuItem;
                    _allMenuItems.Add(setMenuItem);
                }

                setMenuItem.AddChild(menuItem);
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
        }

        public override void OnBoneNameAdded(string extendBoneName)
        {
            InitMenuItems();

            var boneNames = new List<string> { extendBoneName };
            AddFirstBones(boneNames);
            ApplyCurrentFrame(true);
        }

        public override void OnBoneNameRemoved(string extendBoneName)
        {
            InitMenuItems();

            var boneNames = new List<string> { extendBoneName };
            RemoveAllBones(boneNames);
            ApplyCurrentFrame(true);
        }

        public override void UpdateFrame(FrameData frame)
        {
            var cacheBoneData = maidManager.cacheBoneData;
            if (cacheBoneData == null)
            {
                PluginUtils.LogError("ボーンデータが取得できませんでした");
                return;
            }

            var rootBone = cacheBoneData.GetBoneData("Bip01");
            if (rootBone == null)
            {
                PluginUtils.LogError("中心ボーンが取得できませんでした");
                return;
            }

            // ポーズ編集中の移動は中心ボーンに反映  
            if (timelineManager.initialEditFrame != null)
            {
                var targetPosition = rootBone.transform.position;
                var targetRotation = rootBone.transform.rotation;

                maid.transform.position = timelineManager.initialEditPosition;
                maid.transform.rotation = timelineManager.initialEditRotation;

                rootBone.transform.position = targetPosition;
                rootBone.transform.rotation = targetRotation;
            }

            foreach (var name in allBoneNames)
            {
                var transform = maidCache.GetBoneTransform(name);
                if (transform == null)
                {
                    PluginUtils.LogError("SetCacheBoneDataArray：ボーンがnullです Maidを読み込み直してください：" + name);
                    continue;
                }

                var trans = CreateTransformData(name);
                if (trans.hasPosition)
                {
                    trans.position = transform.localPosition;
                }
                trans.rotation = transform.localRotation;

                var bone = frame.CreateBone(trans);
                frame.UpdateBone(bone);
            }
        }

        public override void ApplyAnm(long id, byte[] anmData)
        {
            if (anmData == null)
            {
                return;
            }

            var maid = this.maid;
            if (maid == null)
            {
                PluginUtils.LogError("メイドが配置されていません");
                return;
            }

            float playingFrameNoFloat = defaultLayer.playingFrameNoFloat;
            var isMotionPlaying = this.isMotionPlaying;
            if (isMotionPlaying)
            {
                playingFrameNoFloat += 0.01f; // モーション再生中は再生位置に差分がないと反映されない
            }

            PluginUtils.LogDebug("playingFrameNoFloat={0}", playingFrameNoFloat);

            maidCache.PlayAnm(id, anmData);
            studioHack.OnMotionUpdated(maid);
            maidManager.OnMotionUpdated(maid);

            this.isMotionPlaying = isMotionPlaying;
            maidCache.playingFrameNoFloat = playingFrameNoFloat;
        }

        public override void ApplyCurrentFrame(bool motionUpdate)
        {
            if (anmId != TimelineAnmId || motionUpdate)
            {
                CreateAndApplyAnm();
            }
            else
            {
                maidCache.playingFrameNo = timelineManager.currentFrameNo;
            }
        }

        public override void OutputAnm()
        {
            try
            {
                var anmData = GetAnmBinary(true);
                if (anmData == null)
                {
                    PluginUtils.ShowDialog(errorMessage);
                    return;
                }
                var anmPath = this.anmPath;
                var anmFileName = this.anmFileName;

                bool isExist = File.Exists(anmPath);
                File.WriteAllBytes(anmPath, anmData);

                studioHack.OnUpdateMyPose(anmPath, isExist);
            }
            catch (Exception e)
            {
                PluginUtils.LogException(e);
                PluginUtils.ShowDialog("モーションの出力に失敗しました");
            }
        }

        protected override byte[] GetAnmBinaryInternal(bool forOutput, int startFrameNo, int endFrameNo)
        {
            if (maidCache == null)
            {
                return null;
            }

            var startSecond = timeline.GetFrameTimeSeconds(startFrameNo);
            var endSecond = timeline.GetFrameTimeSeconds(endFrameNo);

            var times = new List<float>(_keyFrames.Count);
            var valuesList = new List<ValueData[]>(_keyFrames.Count);

            int _startFrameNo = startFrameNo;
            int _endFrameNo = endFrameNo;
            Action<BinaryWriter, BoneData> write_bone_data = delegate (
                BinaryWriter w,
                BoneData firstBone)
            {
                var name = firstBone.name;
                var path = maidCache.GetBonePath(name);
                w.Write((byte)1);
                w.Write(path);

                times.Clear();
                valuesList.Clear();

                bool hasLastKey = false;
                foreach (var frame in _keyFrames)
                {
                    if (frame.frameNo < _startFrameNo || frame.frameNo > _endFrameNo)
                    {
                        continue;
                    }

                    var bone = frame.GetBone(name);
                    if (bone == null && frame.frameNo == _startFrameNo)
                    {
                        bone = GetPrevBone(frame.frameNo, name, false);
                    }

                    if (bone == null && frame.frameNo == _endFrameNo)
                    {
                        bone = GetNextBone(frame.frameNo, name, false);
                    }

                    if (bone != null)
                    {
                        times.Add(timeline.GetFrameTimeSeconds(frame.frameNo) - startSecond);
                        valuesList.Add(bone.transform.values);
                        hasLastKey = frame.frameNo == _endFrameNo;
                    }
                }

                if (!hasLastKey)
                {
                    var bone = _dummyLastFrame.GetBone(name);
                    if (bone != null)
                    {
                        times.Add(endSecond - startSecond);
                        valuesList.Add(bone.transform.values);
                    }
                }

                for (int i = 0; i < firstBone.transform.valueCount; i++)
                {
                    w.Write((byte)(100 + i));
                    w.Write(times.Count);
                    for (int j = 0; j < times.Count; j++)
                    {
                        w.Write(times[j]);
                        w.Write(valuesList[j][i].value);
                        w.Write(valuesList[j][i].inTangent.value);
                        w.Write(valuesList[j][i].outTangent.value);
                    }
                }
            };
            MemoryStream memoryStream = new MemoryStream();
            BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
            binaryWriter.Write("CM3D2_ANIM");
            binaryWriter.Write(1001);
            foreach (var name in allBoneNames)
            {
                var bone = firstFrame.GetBone(name);
                if (bone == null)
                {
                    PluginUtils.Log("0フレーム目にキーフレームがないのでスキップしました boneName={0}", name);
                    continue;
                }
                write_bone_data(binaryWriter, bone);
            }
            binaryWriter.Write((byte)0);
            binaryWriter.Write((byte)(useMuneKeyL ? 1u : 0u));
            binaryWriter.Write((byte)(useMuneKeyR ? 1u : 0u));
            binaryWriter.Close();
            memoryStream.Close();
            byte[] result = memoryStream.ToArray();
            memoryStream.Dispose();

            if (!forOutput && maidCache != null)
            {
                maidCache.anmStartFrameNo = startFrameNo;
                maidCache.anmEndFrameNo = endFrameNo;
            }

            return result;
        }

        public void SavePoseTimeLine(
            List<PoseTimeLineRow> rows,
            string filePath)
        {
            var offsetTime = timeline.startOffsetTime;

            var builder = new StringBuilder();
            builder.Append("time,poseType,animation,fadeTime,speed,posX,posY,posZ,rotX,rotY,rotZ,eyeMoveType,option\r\n");

            Action<PoseTimeLineRow, bool> appendRow = (row, isFirst) =>
            {
                var time = row.time;

                if (!isFirst)
                {
                    time += offsetTime;
                }

                builder.Append(time.ToString("0.000") + ",");
                builder.Append(row.poseType + ",");
                builder.Append(row.animation + ",");
                builder.Append(row.fadeTime.ToString("0.000") + ",");
                builder.Append(row.speed.ToString("0.000") + ",");
                builder.Append(row.position.x.ToString("0.000") + ",");
                builder.Append(row.position.y.ToString("0.000") + ",");
                builder.Append(row.position.z.ToString("0.000") + ",");
                builder.Append(row.rotation.x.ToString("0.000") + ",");
                builder.Append(row.rotation.y.ToString("0.000") + ",");
                builder.Append(row.rotation.z.ToString("0.000") + ",");
                builder.Append((int) row.eyeMoveType + ",");
                builder.Append(row.option);
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
                var anmFileName = this.anmFileName;

                {
                    var anmData = GetAnmBinary(true);
                    if (anmData == null)
                    {
                        PluginUtils.ShowDialog(errorMessage);
                        return;
                    }

                    var anmPath = timeline.GetDcmSongFilePath(anmFileName);

                    bool isExist = File.Exists(anmPath);
                    File.WriteAllBytes(anmPath, anmData);

                    var maidElement = GetMeidElement(songElement);
                    //maidElement.Add(new XElement("customAnimation", anmFileName));
                }

                {
                    _dcmOutputRows.Clear();

                    var row = new PoseTimeLineRow
                    {
                        time = 0f,
                        poseType = 0,
                        animation = anmFileName,
                        fadeTime = 0f,
                        speed = 1f,
                        position = Vector3.zero,
                        rotation = Vector3.zero,
                        eyeMoveType = timeline.eyeMoveType,
                        option = string.Empty
                    };
                    _dcmOutputRows.Add(row);

                    var outputFileName = string.Format("pose_{0}.csv", slotNo);
                    var outputPath = timeline.GetDcmSongFilePath(outputFileName);
                    SavePoseTimeLine(_dcmOutputRows, outputPath);

                    var maidElement = GetMeidElement(songElement);
                    maidElement.Add(new XElement("pose", outputFileName));
                }
            }
            catch (Exception e)
            {
                PluginUtils.LogException(e);
                PluginUtils.ShowDialog("モーションの出力に失敗しました");
            }
        }

        public override float CalcEasingValue(float t, int easing)
        {
            return t;
        }

        private ComboBoxValue<TransformEditType> _transComboBox = new ComboBoxValue<TransformEditType>
        {
            items = new List<TransformEditType>
            {
                TransformEditType.全て,
                TransformEditType.移動,
                TransformEditType.回転,
                TransformEditType.X,
                TransformEditType.Y,
                TransformEditType.Z,
                TransformEditType.RX,
                TransformEditType.RY,
                TransformEditType.RZ,
            },
            getName = (type, index) =>
            {
                return type.ToString();
            }
        };

        private ComboBoxValue<string> _slotNameComboBox = new ComboBoxValue<string>
        {
            getName = (slotName, index) =>
            {
                return slotName;
            }
        };

        private ComboBoxValue<IBoneMenuItem> _menuItemComboBox = new ComboBoxValue<IBoneMenuItem>
        {
            getName = (menuItem, index) =>
            {
                return menuItem.displayName;
            }
        };

        private Rect _contentRect = new Rect(0, 0, SubWindow.WINDOW_WIDTH, SubWindow.WINDOW_HEIGHT);
        private Vector2 _scrollPosition = Vector2.zero;

        private enum TabType
        {
            Edit,
            Extend,
            ArmFinger,
            LegFinger,
            Max,
        }

        private TabType tabType = TabType.Edit;

        public static readonly List<string> TabTypeNames = new List<string>
        {
            "編集",
            "追加",
            "手指",
            "足指",
        };

        public override void DrawWindow(GUIView view)
        {
            view.SetEnabled(view.guiEnabled && !_transComboBox.focused && !_slotNameComboBox.focused && !_menuItemComboBox.focused);

            if (maidCache == null)
            {
                view.DrawLabel("メイドが配置されていません", 200, 20);
                return;
            }

            view.BeginLayout(GUIView.LayoutDirection.Horizontal);
            for (var i = 0; i < (int)TabType.Max; i++)
            {
                var type = (TabType)i;
                var color = tabType == type ? Color.green : Color.white;
                if (view.DrawButton(TabTypeNames[i], 50, 20, true, color))
                {
                    tabType = type;
                }
            }
            view.EndLayout();

            view.DrawHorizontalLine(Color.gray);

            _contentRect.width = view.viewRect.width - 20;

            _scrollPosition = view.BeginScrollView(
                view.viewRect.width,
                view.viewRect.height - view.currentPos.y,
                _contentRect,
                _scrollPosition,
                false,
                true);

            switch (tabType)
            {
                case TabType.Edit:
                    DrawTransformEdit(view);
                    break;
                case TabType.Extend:
                    DrawExtendBone(view);
                    break;
                case TabType.ArmFinger:
                    DrawFingerBlend(
                        view,
                        FingerSlotNames,
                        WindowPartsFingerBlend.Type.RightArm,
                        WindowPartsFingerBlend.Type.LeftArm);
                    DrawFingerBlend(
                        view,
                        FingerSlotNames,
                        WindowPartsFingerBlend.Type.LeftArm,
                        WindowPartsFingerBlend.Type.RightArm);
                    break;
                case TabType.LegFinger:
                    DrawFingerBlend(
                        view,
                        LegSlotNames,
                        WindowPartsFingerBlend.Type.RightLeg,
                        WindowPartsFingerBlend.Type.LeftLeg);
                    DrawFingerBlend(
                        view,
                        LegSlotNames,
                        WindowPartsFingerBlend.Type.LeftLeg,
                        WindowPartsFingerBlend.Type.RightLeg);
                    break;
            }

            _contentRect.height = view.currentPos.y + 20;

            view.EndScrollView();

            DrawComboBox(view);
        }

        private void DrawTransformEdit(GUIView view)
        {
            view.BeginLayout(GUIView.LayoutDirection.Horizontal);
            {
                view.DrawLabel("対象カテゴリ", 80, 20);

                _menuItemComboBox.items = allMenuItems;
                view.DrawComboBoxButton(_menuItemComboBox, 150, 20, true);
            }
            view.EndLayout();

            var menuItem = _menuItemComboBox.currentItem;
            if (menuItem == null)
            {
                view.DrawLabel("カテゴリを選択してください", -1, 20);
                return;
            }

            view.BeginLayout(GUIView.LayoutDirection.Horizontal);
            {
                view.DrawLabel("操作種類", 80, 20);
                view.DrawComboBoxButton(_transComboBox, 140, 20, true);
            }
            view.EndLayout();

            var editType = _transComboBox.currentItem;

            view.SetEnabled(view.guiEnabled && studioHack.isPoseEditing);

            if (menuItem.children == null)
            {
                DrawMenuItem(view, editType, menuItem);
            }
            else
            {
                foreach (var child in menuItem.children)
                {
                    DrawMenuItem(view, editType, child);
                }
            }
        }

        private void DrawMenuItem(
            GUIView view,
            TransformEditType editType,
            IBoneMenuItem menuItem)
        {
            var boneName = menuItem.name;
            var displayName = menuItem.displayName;
            var isDefaultBoneName = BoneUtils.IsDefaultBoneName(boneName);
            var boneType = BoneUtils.GetBoneTypeByName(boneName);
            var transform = maidCache.GetBoneTransform(boneName);
            var initialPosition = maidCache.GetInitialPosition(boneName);
            var initialRotation = maidCache.GetInitialEulerAngles(boneName);
            var initialScale = Vector3.one;
            var drawMask = (boneType == IKManager.BoneType.Root || !isDefaultBoneName) ? DrawMaskPositonAndRotation : DrawMaskRotation;
            if (transform == null)
            {
                return;
            }

            var position = transform.localPosition;
            var angle = transform.localEulerAngles;
            var updateTransform = false;

            view.DrawHorizontalLine(Color.gray);

            view.DrawLabel(displayName, 200, 20);

            DrawTransform(
                view,
                transform,
                editType,
                drawMask,
                boneName,
                initialPosition,
                initialRotation,
                initialScale);

            if (updateTransform)
            {
                transform.localPosition = position;
                transform.localEulerAngles = angle;
            }
        }

        private void DrawComboBox(GUIView view)
        {
            view.SetEnabled(true);

            view.DrawComboBoxContent(
                _transComboBox,
                130, 300,
                view.viewRect.width, view.viewRect.height,
                20);

            view.DrawComboBoxContent(
                _slotNameComboBox,
                130, 300,
                view.viewRect.width, view.viewRect.height,
                20);

            view.DrawComboBoxContent(
                _menuItemComboBox,
                130, 300,
                view.viewRect.width, view.viewRect.height,
                20);
        }

        private static readonly string[] FingerBrendNames = new string[]
        {
            "右手",
            "左手",
            "右足",
            "左足",
        };

        private static readonly string[] FingerSlotNames = new string[]
        {
            "親",
            "人",
            "中",
            "薬",
            "小",
        };

        private static readonly string[] LegSlotNames = new string[]
        {
            "親",
            "中",
            "小",
        };

        private FingerBlend.BaseFinger GetBaseFingerClass(WindowPartsFingerBlend.Type type)
        {
            var finger_blend = maidManager.ikManager.finger_blend;
            FingerBlend.BaseFinger result = null;
            if (type == WindowPartsFingerBlend.Type.RightArm)
            {
                result = finger_blend.right_arm_finger;
            }
            else if (type == WindowPartsFingerBlend.Type.LeftArm)
            {
                result = finger_blend.left_arm_finger;
            }
            else if (type == WindowPartsFingerBlend.Type.RightLeg)
            {
                result = finger_blend.right_leg_finger;
            }
            else if (type == WindowPartsFingerBlend.Type.LeftLeg)
            {
                result = finger_blend.left_leg_finger;
            }
            return result;
        }

        public void DrawFingerBlend(
            GUIView view,
            string[] slotNames,
            WindowPartsFingerBlend.Type blendType,
            WindowPartsFingerBlend.Type otherBlendType)
        {
            var baseFinger = GetBaseFingerClass(blendType);

            view.BeginLayout(GUIView.LayoutDirection.Horizontal);
            {
                view.DrawLabel(FingerBrendNames[(int)blendType], 40, 20);

                var newEnabled = view.DrawToggle("有効", baseFinger.enabled, 60, 20);
                if (newEnabled != baseFinger.enabled)
                {
                    baseFinger.enabled = newEnabled;
                    baseFinger.Apply();
                }

                if (view.DrawButton("更新", 40, 20))
                {
                    baseFinger.Apply();
                }
            }
            view.EndLayout();

            view.BeginLayout(GUIView.LayoutDirection.Horizontal);
            {
                view.DrawLabel("ロック", 40, 20);

                bool isAllLock = true;
                for (int i = 0; i < slotNames.Length; i++)
                {
                    var isLock = baseFinger.IsLock(i);
                    isAllLock &= isLock;
                    if (view.DrawButton(slotNames[i], 25, 20, true, isLock ? Color.green : Color.white))
                    {
                        baseFinger.LockSingleItem(!isLock, i);
                        baseFinger.Apply();
                    }
                }

                if (view.DrawButton("全", 25, 20, true, isAllLock ? Color.green : Color.white))
                {
                    baseFinger.LockAllItems(!isAllLock);
                    baseFinger.Apply();
                }

                if (view.DrawButton("反", 25, 20))
                {
                    baseFinger.LockReverse();
                    baseFinger.Apply();
                }
            }
            view.EndLayout();

            view.BeginLayout(GUIView.LayoutDirection.Horizontal);
            {
                view.DrawLabel("開き具合", 60, 20);

                var value = baseFinger.value_open;
                var newValue = view.DrawFloatField(value, 50, 20);

                newValue = view.DrawSlider(newValue, 0f, 1.0f, 100, 20);

                if (view.DrawButton("R", 20, 20))
                {
                    newValue = 0;
                }

                if (newValue != value)
                {
                    baseFinger.value_open = newValue;
                    baseFinger.Apply();
                }
            }
            view.EndLayout();

            view.BeginLayout(GUIView.LayoutDirection.Horizontal);
            {
                view.DrawLabel("閉じ具合", 60, 20);

                var value = baseFinger.value_fist;
                var newValue = view.DrawFloatField(value, 50, 20);

                newValue = view.DrawSlider(newValue, 0f, 1.0f, 100, 20);

                if (view.DrawButton("R", 20, 20))
                {
                    newValue = 0;
                }

                if (newValue != value)
                {
                    baseFinger.value_fist = newValue;
                    baseFinger.Apply();
                }
            }
            view.EndLayout();

            var otherName = FingerBrendNames[(int)otherBlendType];
            if (view.DrawButton(otherName + "にコピー", 100, 20))
            {
                var otherBaseFinger = GetBaseFingerClass(otherBlendType);
                otherBaseFinger.CopyFrom(baseFinger);
                baseFinger.Apply();
            }

            view.DrawHorizontalLine(Color.gray);
        }

        public void DrawExtendBone(GUIView view)
        {
            var extendedBoneCache = this.maidCache.extendBoneCache;

            view.BeginLayout(GUIView.LayoutDirection.Horizontal);
            {
                view.DrawLabel("対象スロット", 80, 20);

                _slotNameComboBox.items = extendedBoneCache.slotNames;
                view.DrawComboBoxButton(_slotNameComboBox, 150, 20, true);
            }
            view.EndLayout();

            var slotName = _slotNameComboBox.currentItem;

            if (string.IsNullOrEmpty(slotName))
            {
                view.DrawLabel("対象スロットがありません", -1, 20);
                return;
            }

            foreach (var entity in extendedBoneCache.entities.Values)
            {
                if (entity.slotName != slotName)
                {
                    continue;
                }

                var extendBoneName = entity.extendBoneName;

                var enable = timeline.HasExtendBoneName(slotNo, extendBoneName);

                var newEnable = view.DrawToggle(
                    entity.boneName,
                    enable,
                    _contentRect.width - 20,
                    20);

                if (newEnable != enable)
                {
                    if (newEnable)
                    {
                        timeline.AddExtendBoneName(slotNo, extendBoneName);
                    }
                    else
                    {
                        timeline.RemoveExtendBoneName(slotNo, extendBoneName);
                    }
                }
            }
        }

        public override ITransformData CreateTransformData(string name)
        {
            ITransformData transform;
            if (name == "Bip01")
            {
                transform = new TransformDataRoot();
            }
            else
            {
                transform = new TransformDataRotation();
            }
            transform.Initialize(name);
            return transform;
        }
    }
}