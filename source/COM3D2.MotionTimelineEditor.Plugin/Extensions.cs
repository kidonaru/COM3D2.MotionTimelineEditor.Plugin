using System.Reflection;
using System.Collections.Generic;
using RootMotion.FinalIK;
using UnityEngine;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    public static class Extensions
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

        public static CacheBoneDataArray.BoneData GetBoneData(
            this CacheBoneDataArray cacheBoneDataArray,
            IKManager.BoneType boneType)
        {
            return cacheBoneDataArray.GetBoneData(BoneUtils.GetBonePath(boneType));
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
    }

}