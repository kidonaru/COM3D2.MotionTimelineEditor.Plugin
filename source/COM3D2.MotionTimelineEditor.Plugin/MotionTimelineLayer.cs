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

        public override List<string> allBoneNames
        {
            get
            {
                return BoneUtils.saveBoneNames;
            }
        }

        private List<PoseTimeLineRow> _dcmPoseRows = new List<PoseTimeLineRow>();

        public MotionTimelineLayer(int slotNo)
        {
            this.slotNo = slotNo;
        }

        public static MotionTimelineLayer Create(int slotNo)
        {
            PluginUtils.LogDebug("MotionTimelineLayer.Create slotNo={0}", slotNo);
            return new MotionTimelineLayer(slotNo);
        }

        public override void Init()
        {
            base.Init();

            boneComboBox.getName = (boneType, index) =>
            {
                return BoneUtils.GetBoneJpName(boneType);
            };
            boneComboBox.onSelected = (boneType, index) =>
            {
                _targetBoneIndex = index;
            };
            boneComboBox.items = BoneUtils.saveBoneTypes;
        }

        protected override void InitMenuItems()
        {
            var setMenuItemMap = new Dictionary<BoneSetMenuType, BoneSetMenuItem>(10);

            foreach (var pair in BoneUtils.BoneTypeToSetMenuTypeMap)
            {
                var boneType = pair.Key;
                var boneSetType = pair.Value;
                
                BoneSetMenuItem setMenuItem;
                if (!setMenuItemMap.TryGetValue(boneSetType, out setMenuItem))
                {
                    var boneSetName = boneSetType.ToString();
                    var displaySetName = BoneUtils.GetBoneSetMenuJpName(boneSetType);
                    setMenuItem = new BoneSetMenuItem(boneSetName, displaySetName);
                    setMenuItemMap[boneSetType] = setMenuItem;
                }

                var boneName = BoneUtils.GetBoneName(boneType);
                var displayName = BoneUtils.GetBoneJpName(boneType);

                var menuItem = new BoneMotionMenuItem(boneName, displayName);
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
        }

        public override void UpdateFrameWithCurrentStat(FrameData frame)
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

            var pathDic = cacheBoneData.GetPathDic();
            foreach (var name in allBoneNames)
            {
                var path = BoneUtils.ConvertBonePath(name);
                CacheBoneDataArray.BoneData sourceBone;
                if (pathDic.TryGetValue(path, out sourceBone))
                {
                    if (sourceBone == null || sourceBone.transform == null)
                    {
                        PluginUtils.LogError("SetCacheBoneDataArray：ボーンがnullです Maidを読み込み直してください：" + name);
                        break;
                    }

                    var trans = CreateTransformData(name);
                    if (trans.hasPosition)
                    {
                        trans.position = sourceBone.transform.localPosition;
                    }
                    trans.rotation = sourceBone.transform.localRotation;

                    var bone = frame.CreateBone(trans);
                    frame.UpdateBone(bone);
                }
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
                var path = BoneUtils.ConvertBonePath(name);
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
                    _dcmPoseRows.Clear();

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
                    _dcmPoseRows.Add(row);

                    var outputFileName = string.Format("pose_{0}.csv", slotNo);
                    var outputPath = timeline.GetDcmSongFilePath(outputFileName);
                    SavePoseTimeLine(_dcmPoseRows, outputPath);

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

        private int _targetBoneIndex = 0;
        private FloatFieldValue[] _fieldValues = FloatFieldValue.CreateArray(
            new string[] { "X", "Y", "Z", "RX", "RY", "RZ" }
        );
        private ComboBoxValue<IKManager.BoneType> boneComboBox = new ComboBoxValue<IKManager.BoneType>();
        private Rect _contentRect = new Rect(0, 0, SubWindow.WINDOW_WIDTH, SubWindow.WINDOW_HEIGHT);
        private Vector2 _scrollPosition = Vector2.zero;

        public override void DrawWindow(GUIView view)
        {
            view.SetEnabled(!boneComboBox.focused);

            if (maidCache == null)
            {
                view.DrawLabel("メイドが配置されていません", 200, 20);
                return;
            }

            _contentRect.width = view.viewRect.width - 20;

            _scrollPosition = view.BeginScrollView(
                view.viewRect.width,
                view.viewRect.height,
                _contentRect,
                _scrollPosition,
                false,
                true);

            DrawTransform(view);
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

            _contentRect.height = view.currentPos.y + 20;

            view.EndScrollView();

            DrawComboBox(view);
        }

        private void DrawTransform(GUIView view)
        {
            var saveBoneTypes = BoneUtils.saveBoneTypes;

            view.BeginLayout(GUIView.LayoutDirection.Horizontal);
            {
                view.DrawLabel("対象ボーン", 80, 20);

                boneComboBox.currentIndex = _targetBoneIndex;
                view.DrawComboBoxButton(boneComboBox, 140, 20, true);
            }
            view.EndLayout();

            if (_targetBoneIndex < 0 || _targetBoneIndex >= saveBoneTypes.Count)
            {
                return;
            }

            var boneType = saveBoneTypes[_targetBoneIndex];
            var bonePath = BoneUtils.GetBonePath(boneType);
            var cacheBoneData = maidCache.cacheBoneData;
            var bone = cacheBoneData.GetBoneData(bonePath);
            if (bone == null)
            {
                return;
            }

            var position = bone.transform.localPosition;
            var angle = bone.transform.localEulerAngles;
            var updateTransform = false;

            view.SetEnabled(!boneComboBox.focused && studioHack.isPoseEditing);

            if (boneType == IKManager.BoneType.Root)
            {
                updateTransform |= view.DrawValue(_fieldValues[0], 0.01f, 0.1f,
                    () => position.x = BoneUtils.GetInitialPosition(boneType).x,
                    position.x,
                    x => position.x = x,
                    x => position.x += x);

                updateTransform |= view.DrawValue(_fieldValues[1], 0.01f, 0.1f,
                    () => position.y = BoneUtils.GetInitialPosition(boneType).y,
                    position.y,
                    y => position.y = y,
                    y => position.y += y);

                updateTransform |= view.DrawValue(_fieldValues[2], 0.01f, 0.1f,
                    () => position.z = BoneUtils.GetInitialPosition(boneType).z,
                    position.z,
                    z => position.z = z,
                    z => position.z += z);
            }
            {
                updateTransform |= view.DrawValue(_fieldValues[3], 1f, 10f,
                    () => angle.x = BoneUtils.GetInitialEulerAngles(boneType).x,
                    angle.x,
                    x => angle.x = x,
                    x => angle.x += x);

                updateTransform |= view.DrawValue(_fieldValues[4], 1f, 10f,
                    () => angle.y = BoneUtils.GetInitialEulerAngles(boneType).y,
                    angle.y,
                    y => angle.y = y,
                    y => angle.y += y);

                updateTransform |= view.DrawValue(_fieldValues[5], 1f, 10f,
                    () => angle.z = BoneUtils.GetInitialEulerAngles(boneType).z,
                    angle.z,
                    z => angle.z = z,
                    z => angle.z += z);
            }

            if (updateTransform)
            {
                bone.transform.localPosition = position;
                bone.transform.localEulerAngles = angle;
            }

            view.DrawHorizontalLine(Color.gray);
        }

        private void DrawComboBox(GUIView view)
        {
            view.SetEnabled(true);

            view.DrawComboBoxContent(
                boneComboBox,
                120, 300,
                SubWindow.rc_stgw.width, SubWindow.rc_stgw.height,
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

        public FingerBlend.BaseFinger GetBaseFingerClass(WindowPartsFingerBlend.Type type)
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