using System.Collections.Generic;
using UnityEngine;
using System;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    public static partial class Extensions
    {
        public static Vector2 ToVector2(this ValueData[] values)
        {
            if (values.Length != 2)
            {
                MTEUtils.LogError("ToVector2: 不正なValueData配列です length={0}", values.Length);
                return Vector2.zero;
            }

            return new Vector2(values[0].value, values[1].value);
        }

        public static void FromVector2(this ValueData[] values, Vector2 vector)
        {
            if (values.Length != 2)
            {
                MTEUtils.LogError("FromVector2: 不正なValueData配列です length={0}", values.Length);
            }
            else
            {
                values[0].value = vector.x;
                values[1].value = vector.y;
            }
        }

        public static Vector3 ToVector3(this ValueData[] values)
        {
            if (values.Length != 3)
            {
                MTEUtils.LogError("ToVector3: 不正なValueData配列です length={0}", values.Length);
                return Vector3.zero;
            }
            return new Vector3(values[0].value, values[1].value, values[2].value);
        }

        public static void FromVector3(this ValueData[] values, Vector3 vector)
        {
            if (values.Length != 3)
            {
                MTEUtils.LogError("FromVector3: 不正なValueData配列です length={0}", values.Length);
                return;
            }
            values[0].value = vector.x;
            values[1].value = vector.y;
            values[2].value = vector.z;
        }

        public static Quaternion ToQuaternion(this ValueData[] values)
        {
            if (values.Length != 4)
            {
                MTEUtils.LogError("ToQuaternion: 不正なValueData配列です length={0}", values.Length);
                return Quaternion.identity;
            }
            return new Quaternion(values[0].value, values[1].value, values[2].value, values[3].value);
        }

        public static void FromQuaternion(this ValueData[] values, Quaternion quaternion)
        {
            if (values.Length != 4)
            {
                MTEUtils.LogError("FromQuaternion: 不正なValueData配列です length={0}", values.Length);
                return;
            }
            values[0].value = quaternion.x;
            values[1].value = quaternion.y;
            values[2].value = quaternion.z;
            values[3].value = quaternion.w;
        }

        public static Color ToColor(this ValueData[] values)
        {
            if (values.Length == 3)
            {
                return new Color(values[0].value, values[1].value, values[2].value);
            }
            if (values.Length == 4)
            {
                return new Color(values[0].value, values[1].value, values[2].value, values[3].value);
            }

            MTEUtils.LogError("ToColor: 不正なValueData配列です length={0}", values.Length);
            return Color.white;
        }

        public static void FromColor(this ValueData[] values, Color color)
        {
            if (values.Length == 3)
            {
                values[0].value = color.r;
                values[1].value = color.g;
                values[2].value = color.b;
            }
            else if (values.Length == 4)
            {
                values[0].value = color.r;
                values[1].value = color.g;
                values[2].value = color.b;
                values[3].value = color.a;
            }
            else
            {
                MTEUtils.LogError("FromColor: 不正なValueData配列です length={0}", values.Length);
            }
        }

        public static IKManager.BoneType ConvertBoneType(this IKHoldType holdType)
        {
            switch (holdType)
            {
                case IKHoldType.Arm_R_Joint:
                    return IKManager.BoneType.UpperArm_R;
                case IKHoldType.Arm_R_Tip:
                    return IKManager.BoneType.Forearm_R;
                case IKHoldType.Arm_L_Joint:
                    return IKManager.BoneType.UpperArm_L;
                case IKHoldType.Arm_L_Tip:
                    return IKManager.BoneType.Forearm_L;
                case IKHoldType.Foot_R_Joint:
                    return IKManager.BoneType.Thigh_R;
                case IKHoldType.Foot_R_Tip:
                    return IKManager.BoneType.Calf_R;
                case IKHoldType.Foot_L_Joint:
                    return IKManager.BoneType.Thigh_L;
                case IKHoldType.Foot_L_Tip:
                    return IKManager.BoneType.Calf_L;
            }

            MTEUtils.LogError("Invalid IKHoldType: " + holdType);
            return IKManager.BoneType.UpperArm_R;
        }

        public static float[] GetInTangents(this ValueData[] values)
        {
            var ret = new float[values.Length];
            for (int i = 0; i < values.Length; i++)
            {
                ret[i] = values[i].inTangent.value;
            }

            return ret;
        }

        public static float[] GetOutTangents(this ValueData[] values)
        {
            var ret = new float[values.Length];
            for (int i = 0; i < values.Length; i++)
            {
                ret[i] = values[i].outTangent.value;
            }

            return ret;
        }

        public static void ClearBones(
            this Dictionary<string, List<BoneData>> bonesMap)
        {
            foreach (var bones in bonesMap.Values)
            {
                bones.Clear();
            }
        }

        public static void ClearPlayData(
            this Dictionary<string, MotionPlayData> playDataMap)
        {
            foreach (var data in playDataMap.Values)
            {
                data.Clear();
            }
        }

        public static void AppendBone(
            this Dictionary<string, List<BoneData>> bonesMap,
            BoneData bone,
            bool isLastFrame)
        {
            if (bone == null)
            {
                return;
            }

            var boneName = bone.name;

            List<BoneData> rows;
            if (!bonesMap.TryGetValue(boneName, out rows))
            {
                rows = new List<BoneData>(16);
                bonesMap[boneName] = rows;
            }

            // 最後のフレームは2重に追加しない
            if (isLastFrame &&
                rows.Count > 0 &&
                rows[rows.Count - 1].frameNo == bone.frameNo)
            {
                return;
            }

            rows.Add(bone);
        }

        public static bool DrawCustomValueFloat(
            this GUIView view,
            CustomValueInfo info,
            float value,
            Action<float> onChanged)
        {
            if (info.type == CustomValueType.FloatSlider)
            {
                return view.DrawSliderValue(new GUIView.SliderOption
                {
                    label = info.name,
                    labelWidth = 40,
                    fieldType = FloatFieldType.Float,
                    min = info.min,
                    max = info.max,
                    step = info.step,
                    defaultValue = info.defaultValue,
                    value = value,
                    onChanged = onChanged,
                });
            }
            else
            {
                return view.DrawFloatField(new GUIView.FloatFieldOption
                {
                    label = info.name,
                    labelWidth = 40,
                    minValue = info.min,
                    maxValue = info.max,
                    value = value,
                    width = 90,
                    height = 20,
                    onChanged = onChanged,
                });
            }
        }

        public static bool DrawCustomValueInt(
            this GUIView view,
            CustomValueInfo info,
            int value,
            Action<int> onChanged)
        {
            return view.DrawSliderValue(new GUIView.SliderOption
            {
                label = info.name,
                labelWidth = 40,
                fieldType = FloatFieldType.Int,
                min = info.min,
                max = info.max,
                step = info.step,
                defaultValue = info.defaultValue,
                value = value,
                onChanged = x => onChanged((int) x),
            });
        }

        public static bool DrawCustomValueIntRandom(
            this GUIView view,
            CustomValueInfo info,
            int value,
            Action<int> onChanged)
        {
            return view.DrawIntField(new GUIView.IntFieldOption
            {
                label = info.name,
                labelWidth = 40,
                minValue = 1,
                maxValue = int.MaxValue,
                value = value,
                width = 150,
                height = 20,
                onChanged = onChanged,
                onReset = () => onChanged(UnityEngine.Random.Range(1, int.MaxValue)),
            });
        }

        public static bool DrawCustomValueBool(
            this GUIView view,
            CustomValueInfo info,
            bool value,
            Action<bool> onChanged)
        {
            return view.DrawToggle(info.name, value, -1, 20, onChanged);
        }
    }
}