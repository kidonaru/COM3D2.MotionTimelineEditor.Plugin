using System.Reflection;
using System.Collections.Generic;
using RootMotion.FinalIK;
using UnityEngine;
using System.Linq;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    public static partial class Extensions
    {
        public static void ResizeTexture(
            this Texture2D sourceTexture,
            int targetWidth,
            int targetHeight)
        {
            float sourceAspect = (float)sourceTexture.width / sourceTexture.height;
            float targetAspect = (float)targetWidth / targetHeight;

            int width, height;
            if (sourceAspect > targetAspect)
            {
                width = targetWidth;
                height = (int)(width / sourceAspect);
            }
            else
            {
                height = targetHeight;
                width = (int)(height * sourceAspect);
            }
            TextureScale.Bilinear(sourceTexture, width, height);
        }

        public static Texture2D ResizeAndCropTexture(
            this Texture2D sourceTexture,
            int targetWidth,
            int targetHeight)
        {
            float sourceAspect = (float)sourceTexture.width / sourceTexture.height;
            float targetAspect = (float)targetWidth / targetHeight;

            int width, height;
            if (sourceAspect > targetAspect)
            {
                height = targetHeight;
                width = (int)(sourceTexture.width * ((float)targetHeight / sourceTexture.height));
            }
            else
            {
                width = targetWidth;
                height = (int)(sourceTexture.height * ((float)targetWidth / sourceTexture.width));
            }
            TextureScale.Bilinear(sourceTexture, width, height);

            var pixels = new Color[targetWidth * targetHeight];

            int x = (width - targetWidth) / 2;
            int y = (height - targetHeight) / 2;

            for (int i = 0; i < targetHeight; i++)
            {
                for (int j = 0; j < targetWidth; j++)
                {
                    pixels[i * targetWidth + j] = sourceTexture.GetPixel(x + j, y + i);
                }
            }

            var resultTexture = new Texture2D(targetWidth, targetHeight, TextureFormat.ARGB32, false);
            resultTexture.SetPixels(pixels);
            resultTexture.Apply();

            return resultTexture;
        }

        private static FieldInfo fieldPathDic = null;

        public static Dictionary<string, CacheBoneDataArray.BoneData> GetPathDic(
            this CacheBoneDataArray cacheBoneDataArray)
        {
            if (fieldPathDic == null)
            {
                fieldPathDic = typeof(CacheBoneDataArray).GetField("path_dic_", BindingFlags.Instance | BindingFlags.NonPublic);
                PluginUtils.AssertNull(fieldPathDic != null, "fieldPathDic is null");
            }
            return (Dictionary<string, CacheBoneDataArray.BoneData>) fieldPathDic.GetValue(cacheBoneDataArray);
        }

        public static CacheBoneDataArray.BoneData GetBoneData(
            this CacheBoneDataArray cacheBoneDataArray,
            string path)
        {
            var pathDic = cacheBoneDataArray.GetPathDic();
            CacheBoneDataArray.BoneData boneData;
            if (pathDic.TryGetValue(path, out boneData))
            {
                return boneData;
            }
            return null;
        }

        private static FieldInfo fieldIkFabrik = null;

        public static FABRIK GetIkFabrik(this LimbControl limbControl)
        {
            if (limbControl == null)
            {
                return null;
            }
            if (fieldIkFabrik == null)
            {
                fieldIkFabrik = typeof(LimbControl).GetField("ik_fabrik_", BindingFlags.NonPublic | BindingFlags.Instance);
                PluginUtils.AssertNull(fieldIkFabrik != null, "fieldIkFabrik is null");
            }
            return (FABRIK) fieldIkFabrik.GetValue(limbControl);
        }

        private static FieldInfo fieldJointDragPoint = null;

        public static IKDragPoint GetJointDragPoint(
            this LimbControl limbControl)
        {
            if (fieldJointDragPoint == null)
            {
                fieldJointDragPoint = typeof(LimbControl).GetField("joint_drag_point_", BindingFlags.NonPublic | BindingFlags.Instance);
                PluginUtils.AssertNull(fieldJointDragPoint != null, "fieldJointDragPoint is null");
            }
            return (IKDragPoint) fieldJointDragPoint.GetValue(limbControl);
        }

        private static FieldInfo fieldTipDragPoint = null;

        public static IKDragPoint GetTipDragPoint(
            this LimbControl limbControl)
        {
            if (fieldTipDragPoint == null)
            {
                fieldTipDragPoint = typeof(LimbControl).GetField("tip_drag_point_", BindingFlags.NonPublic | BindingFlags.Instance);
                PluginUtils.AssertNull(fieldTipDragPoint != null, "fieldTipDragPoint is null");
            }
            return (IKDragPoint) fieldTipDragPoint.GetValue(limbControl);
        }

        private static FieldInfo fieldBackupLocalPos = null;

        public static Vector3 GetBackupLocalPos(
            this IKDragPoint ikDragPoint)
        {
            if (fieldBackupLocalPos == null)
            {
                fieldBackupLocalPos = typeof(IKDragPoint).GetField("backup_local_pos_", BindingFlags.NonPublic | BindingFlags.Instance);
                PluginUtils.AssertNull(fieldBackupLocalPos != null, "fieldBackupLocalPos is null");
            }
            return (Vector3) fieldBackupLocalPos.GetValue(ikDragPoint);
        }

        public static void PositonCorrection(this IKDragPoint ikDragPoint)
        {
            if (ikDragPoint.PositonCorrectionEnabled)
            {
                ikDragPoint.target_ik_point_trans.localPosition = ikDragPoint.GetBackupLocalPos();
            }
        }

        private static FieldInfo fieldEyeEulerAngle = null;

        public static Vector3 GetEyeEulerAngle(this TBody body)
        {
            if (fieldEyeEulerAngle == null)
            {
                fieldEyeEulerAngle = typeof(TBody).GetField("EyeEulerAngle", BindingFlags.NonPublic | BindingFlags.Instance);
                PluginUtils.AssertNull(fieldEyeEulerAngle != null, "fieldEyeEulerAngle is null");
            }
            return (Vector3) fieldEyeEulerAngle.GetValue(body);
        }

        public static void SetEyeEulerAngle(this TBody body, Vector3 eulerAngle)
        {
            if (fieldEyeEulerAngle == null)
            {
                fieldEyeEulerAngle = typeof(TBody).GetField("EyeEulerAngle", BindingFlags.NonPublic | BindingFlags.Instance);
                PluginUtils.AssertNull(fieldEyeEulerAngle != null, "fieldEyeEulerAngle is null");
            }
            fieldEyeEulerAngle.SetValue(body, eulerAngle);
        }

        public static T GetCustomAttribute<T>(
            this System.Type type)
            where T : System.Attribute
        {
            var attributes = type.GetCustomAttributes(typeof(T), false);
            if (attributes.Length > 0)
            {
                return (T) attributes[0];
            }
            return default(T);
        }

        public static Vector2 ToVector2(this ValueData[] values)
        {
            if (values.Length != 2)
            {
                PluginUtils.LogError("ToVector2: 不正なValueData配列です length={0}", values.Length);
                return Vector2.zero;
            }

            return new Vector2(values[0].value, values[1].value);
        }

        public static void FromVector2(this ValueData[] values, Vector2 vector)
        {
            if (values.Length != 2)
            {
                PluginUtils.LogError("FromVector2: 不正なValueData配列です length={0}", values.Length);
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
                PluginUtils.LogError("ToVector3: 不正なValueData配列です length={0}", values.Length);
                return Vector3.zero;
            }
            return new Vector3(values[0].value, values[1].value, values[2].value);
        }

        public static void FromVector3(this ValueData[] values, Vector3 vector)
        {
            if (values.Length != 3)
            {
                PluginUtils.LogError("FromVector3: 不正なValueData配列です length={0}", values.Length);
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
                PluginUtils.LogError("ToQuaternion: 不正なValueData配列です length={0}", values.Length);
                return Quaternion.identity;
            }
            return new Quaternion(values[0].value, values[1].value, values[2].value, values[3].value);
        }

        public static void FromQuaternion(this ValueData[] values, Quaternion quaternion)
        {
            if (values.Length != 4)
            {
                PluginUtils.LogError("FromQuaternion: 不正なValueData配列です length={0}", values.Length);
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

            PluginUtils.LogError("ToColor: 不正なValueData配列です length={0}", values.Length);
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
                PluginUtils.LogError("FromColor: 不正なValueData配列です length={0}", values.Length);
            }
        }

        public static Vector3 ToVector3(this Color color)
        {
            return new Vector3(color.r, color.g, color.b);
        }

        public static Color ToColor(this Vector3 vector)
        {
            return new Color(vector.x, vector.y, vector.z);
        }

        public static Vector4 ToHSVA(this Color color)
        {
            Vector4 hsv = Vector4.zero;
            Color.RGBToHSV(color, out hsv.x, out hsv.y, out hsv.z);
            hsv.w = color.a;
            return hsv;
        }

        public static Color FromHSVA(this Vector4 hsv)
        {
            var color = Color.HSVToRGB(hsv.x, hsv.y, hsv.z);
            color.a = hsv.w;
            return color;
        }

        public static int IntR(this Color color)
        {
            return Mathf.Clamp(Mathf.RoundToInt(color.r * 255f), 0, 255);
        }

        public static int IntG(this Color color)
        {
            return Mathf.Clamp(Mathf.RoundToInt(color.g * 255f), 0, 255);
        }

        public static int IntB(this Color color)
        {
            return Mathf.Clamp(Mathf.RoundToInt(color.b * 255f), 0, 255);
        }

        public static int IntA(this Color color)
        {
            return Mathf.Clamp(Mathf.RoundToInt(color.a * 255f), 0, 255);
        }

        public static string ToHexRGB(this Color color)
        {
            return "#" + ColorUtility.ToHtmlStringRGB(color);
        }

        public static string ToHexRGBA(this Color color)
        {
            return "#" + ColorUtility.ToHtmlStringRGBA(color);
        }

        private static FieldInfo _objectTargetListField = null;

        public static List<PhotoTransTargetObject> GetTargetList(this ObjectManagerWindow self)
        {
            if (_objectTargetListField == null)
            {
                _objectTargetListField = typeof(ObjectManagerWindow).GetField("target_list_",
                    BindingFlags.NonPublic | BindingFlags.Instance);
            }

            return (List<PhotoTransTargetObject>) _objectTargetListField.GetValue(self);
        }

        private static FieldInfo _lightTargetListField = null;

        public static List<PhotoTransTargetObject> GetTargetList(this LightWindow self)
        {
            if (_lightTargetListField == null)
            {
                _lightTargetListField = typeof(LightWindow).GetField("targetList",
                    BindingFlags.NonPublic | BindingFlags.Instance);
            }

            return (List<PhotoTransTargetObject>) _lightTargetListField.GetValue(self);
        }

        private static FieldInfo _valueOpenField = null;

        public static void SetValueOpenOnly(this FingerBlend.BaseFinger self, float value)
        {
            if (_valueOpenField == null)
            {
                _valueOpenField = typeof(FingerBlend.BaseFinger).GetField("value_open_",
                    BindingFlags.NonPublic | BindingFlags.Instance);
            }

            _valueOpenField.SetValue(self, value);
        }

        private static FieldInfo _valueFistField = null;

        public static void SetValueFistOnly(this FingerBlend.BaseFinger self, float value)
        {
            if (_valueFistField == null)
            {
                _valueFistField = typeof(FingerBlend.BaseFinger).GetField("value_fist_",
                    BindingFlags.NonPublic | BindingFlags.Instance);
            }

            _valueFistField.SetValue(self, value);
        }

        private static FieldInfo _blendEnabledField = null;

        public static void SetEnabledOnly(this FingerBlend.BaseFinger self, bool value)
        {
            if (_blendEnabledField == null)
            {
                _blendEnabledField = typeof(FingerBlend.BaseFinger).GetField("enabled_",
                    BindingFlags.NonPublic | BindingFlags.Instance);
            }

            _blendEnabledField.SetValue(self, value);
        }

        public static bool IsLock(this FingerBlend.BaseFinger baseFinger, int index)
        {
            var armFinger = baseFinger as FingerBlend.ArmFinger;
            if (armFinger != null)
            {
                switch (index)
                {
                    case 0:
                        return armFinger.lock_enabled0;
                    case 1:
                        return armFinger.lock_enabled1;
                    case 2:
                        return armFinger.lock_enabled2;
                    case 3:
                        return armFinger.lock_enabled3;
                    case 4:
                        return armFinger.lock_enabled4;
                }
            }

            var legFinger = baseFinger as FingerBlend.LegFinger;
            if (legFinger != null)
            {
                switch (index)
                {
                    case 0:
                        return legFinger.lock_enabled0;
                    case 1:
                        return legFinger.lock_enabled1;
                    case 2:
                        return legFinger.lock_enabled2;
                }
            }

            return false;
        }

        public static void LockAllItems(this FingerBlend.BaseFinger baseFinger, bool isLock)
        {
            var armFinger = baseFinger as FingerBlend.ArmFinger;
            if (armFinger != null)
            {
                armFinger.lock_enabled0 = isLock;
                armFinger.lock_enabled1 = isLock;
                armFinger.lock_enabled2 = isLock;
                armFinger.lock_enabled3 = isLock;
                armFinger.lock_enabled4 = isLock;
            }

            var legFinger = baseFinger as FingerBlend.LegFinger;
            if (legFinger != null)
            {
                legFinger.lock_enabled0 = isLock;
                legFinger.lock_enabled1 = isLock;
                legFinger.lock_enabled2 = isLock;
            }
        }

        public static void LockReverse(this FingerBlend.BaseFinger baseFinger)
        {
            var armFinger = baseFinger as FingerBlend.ArmFinger;
            if (armFinger != null)
            {
                armFinger.lock_enabled0 = !armFinger.lock_enabled0;
                armFinger.lock_enabled1 = !armFinger.lock_enabled1;
                armFinger.lock_enabled2 = !armFinger.lock_enabled2;
                armFinger.lock_enabled3 = !armFinger.lock_enabled3;
                armFinger.lock_enabled4 = !armFinger.lock_enabled4;
            }

            var legFinger = baseFinger as FingerBlend.LegFinger;
            if (legFinger != null)
            {
                legFinger.lock_enabled0 = !legFinger.lock_enabled0;
                legFinger.lock_enabled1 = !legFinger.lock_enabled1;
                legFinger.lock_enabled2 = !legFinger.lock_enabled2;
            }
        }

        public static void CopyFrom(
            this FingerBlend.BaseFinger baseFinger,
            FingerBlend.BaseFinger source)
        {
            baseFinger.enabled = source.enabled;

            var armFinger = baseFinger as FingerBlend.ArmFinger;
            var sourceArmFinger = source as FingerBlend.ArmFinger;
            if (armFinger != null && sourceArmFinger != null)
            {
                armFinger.lock_enabled0 = sourceArmFinger.lock_enabled0;
                armFinger.lock_enabled1 = sourceArmFinger.lock_enabled1;
                armFinger.lock_enabled2 = sourceArmFinger.lock_enabled2;
                armFinger.lock_enabled3 = sourceArmFinger.lock_enabled3;
                armFinger.lock_enabled4 = sourceArmFinger.lock_enabled4;

                armFinger.lock_value0 = sourceArmFinger.lock_value0;
                armFinger.lock_value1 = sourceArmFinger.lock_value1;
                armFinger.lock_value2 = sourceArmFinger.lock_value2;
                armFinger.lock_value3 = sourceArmFinger.lock_value3;
                armFinger.lock_value4 = sourceArmFinger.lock_value4;
            }

            var legFinger = baseFinger as FingerBlend.LegFinger;
            var sourceLegFinger = source as FingerBlend.LegFinger;
            if (legFinger != null && sourceLegFinger != null)
            {
                legFinger.lock_enabled0 = sourceLegFinger.lock_enabled0;
                legFinger.lock_enabled1 = sourceLegFinger.lock_enabled1;
                legFinger.lock_enabled2 = sourceLegFinger.lock_enabled2;

                legFinger.lock_value0 = sourceLegFinger.lock_value0;
                legFinger.lock_value1 = sourceLegFinger.lock_value1;
                legFinger.lock_value2 = sourceLegFinger.lock_value2;
            }

            baseFinger.value_open = source.value_open;
            baseFinger.value_fist = source.value_fist;
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

            PluginUtils.LogError("Invalid IKHoldType: " + holdType);
            return IKManager.BoneType.UpperArm_R;
        }

        public static List<string> GetTags(this TMorph morph)
        {
            if (morph != null)
            {
                var tags = new List<string>(morph.hash.Count);
                foreach (string tag in morph.hash.Keys)
                {
                    tags.Add(tag);
                }
                return tags;
            }

            return new List<string>();
        }

        public static string GetFullPath(this Transform transform, Transform root)
        {
            if (transform == null || root == null)
            {
                return "";
            }

            var parent = transform.parent;
            if (parent == null || parent == root)
            {
                return transform.name;
            }

            return parent.GetFullPath(root) + "/" + transform.name;
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

        public static Vector3 ToVector3(this float[] values)
        {
            if (values.Length != 3)
            {
                PluginUtils.LogError("ToVector3: 不正なfloat配列です length={0}", values.Length);
                return Vector3.zero;
            }
            return new Vector3(values[0], values[1], values[2]);
        }

        public static Vector4 ToVector4(this float[] values)
        {
            if (values.Length != 4)
            {
                PluginUtils.LogError("ToVector4: 不正なfloat配列です length={0}", values.Length);
                return Vector4.zero;
            }
            return new Vector4(values[0], values[1], values[2], values[3]);
        }

        public static Quaternion ToQuaternion(this float[] values)
        {
            if (values.Length != 4)
            {
                PluginUtils.LogError("ToQuaternion: 不正なfloat配列です length={0}", values.Length);
                return Quaternion.identity;
            }
            return new Quaternion(values[0], values[1], values[2], values[3]);
        }

        public static Color ToColor(this float[] values)
        {
            if (values.Length == 4)
            {
                return new Color(values[0], values[1], values[2], values[3]);
            }
            if (values.Length == 3)
            {
                return new Color(values[0], values[1], values[2]);
            }

            PluginUtils.LogError("ToColor: 不正なfloat配列です length={0}", values.Length);
            return Color.white;
        }

        public static void RemoveAllButFirst<T>(this List<T> list)
        {
            if (list == null)
            {
                PluginUtils.LogError("RemoveAllButFirst: list is null");
                return;
            }

            if (list.Count <= 1)
            {
                return;
            }

            T first = list[0];
            list.Clear();
            list.Add(first);
        }

        public static float GetRotationZ(this Camera camera)
        {
            return camera.transform.eulerAngles.z;
        }

        public static void SetRotationZ(this Camera camera, float z)
        {
            Vector3 eulerAngles = camera.transform.eulerAngles;
            eulerAngles.z = z;
            camera.transform.eulerAngles = eulerAngles;
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

        public static TValue GetOrCreate<TKey, TValue>(this Dictionary<TKey, TValue> self, TKey key)
            where TValue : new()
        {
            TValue value;
            if (!self.TryGetValue(key, out value))
            {
                value = new TValue();
                self[key] = value;
            }

            return value;
        }

        public static TValue GetOrNull<TKey, TValue>(this Dictionary<TKey, TValue> self, TKey key)
            where TValue : class
        {
            TValue value;
            if (!self.TryGetValue(key, out value))
            {
                return null;
            }

            return value;
        }
    }

}