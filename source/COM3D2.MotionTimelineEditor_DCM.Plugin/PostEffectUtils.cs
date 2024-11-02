using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using COM3D2.DanceCameraMotion.Plugin;

namespace COM3D2.MotionTimelineEditor_DCM.Plugin
{
    public enum PostEffectType
    {
        DepthOfField,
    }

    public static class PostEffectUtils
    {
        public static readonly Dictionary<PostEffectType, string> PostEffectTypeToJpNameMap = new Dictionary<PostEffectType, string>
        {
            { PostEffectType.DepthOfField, "被写界深度" },
        };

        public static string ToJpName(PostEffectType postEffectType)
        {
            if (PostEffectTypeToJpNameMap.ContainsKey(postEffectType))
            {
                return PostEffectTypeToJpNameMap[postEffectType];
            }
            return postEffectType.ToString();
        }

        public static readonly Dictionary<PostEffectType, string> PostEffectTypeToNameMap =
            PostEffectTypeToJpNameMap.ToDictionary(pair => pair.Key, pair => pair.Key.ToString());

        public static string ToEffectName(PostEffectType postEffectType)
        {
            if (PostEffectTypeToNameMap.ContainsKey(postEffectType))
            {
                return PostEffectTypeToNameMap[postEffectType];
            }
            return postEffectType.ToString();
        }

        public static readonly Dictionary<string, string> PostEffectNameToJpNameMap =
            PostEffectTypeToJpNameMap.ToDictionary(pair => pair.Key.ToString(), pair => pair.Value);

        public static string ToJpName(string postEffectName)
        {
            if (PostEffectNameToJpNameMap.ContainsKey(postEffectName))
            {
                return PostEffectNameToJpNameMap[postEffectName];
            }
            return postEffectName;
        }

        public static readonly Dictionary<string, PostEffectType> PostEffectNameToTypeMap =
            PostEffectTypeToNameMap.ToDictionary(pair => pair.Value, pair => pair.Key);

        public static PostEffectType ToEffectType(string postEffectName)
        {
            if (PostEffectNameToTypeMap.ContainsKey(postEffectName))
            {
                return PostEffectNameToTypeMap[postEffectName];
            }
            return PostEffectType.DepthOfField;
        }

        private static List<string> _postEffectNames = null;
        public static List<string> PostEffectNames
        {
            get
            {
                if (_postEffectNames == null)
                {
                    _postEffectNames = PostEffectTypeToNameMap.Values.ToList();
                }
                return _postEffectNames;
            }
        }
    }
}