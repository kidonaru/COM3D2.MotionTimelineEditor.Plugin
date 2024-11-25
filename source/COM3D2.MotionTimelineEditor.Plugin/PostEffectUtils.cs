using System.Collections.Generic;
using System.Linq;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    public enum PostEffectType
    {
        DepthOfField,
        Paraffin,
    }

    public static class PostEffectUtils
    {
        private static TimelineData timeline
        {
            get
            {
                return TimelineManager.instance.timeline;
            }
        }

        public static readonly Dictionary<PostEffectType, string> PostEffectTypeToJpNameMap = new Dictionary<PostEffectType, string>
        {
            { PostEffectType.DepthOfField, "被写界深度" },
            { PostEffectType.Paraffin, "パラフィン" },
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
            var groupIndex = PluginUtils.ExtractGroup(postEffectName);
            var groupSuffix = "";
            if (groupIndex > 0)
            {
                groupSuffix = PluginUtils.GetGroupSuffix(groupIndex);
                postEffectName = PluginUtils.RemoveGroupSuffix(postEffectName);
            }

            if (PostEffectNameToJpNameMap.ContainsKey(postEffectName))
            {
                postEffectName = PostEffectNameToJpNameMap[postEffectName];
            }

            return postEffectName + groupSuffix;
        }

        public static readonly Dictionary<string, PostEffectType> PostEffectNameToTypeMap =
            PostEffectTypeToNameMap.ToDictionary(pair => pair.Value, pair => pair.Key);

        public static PostEffectType ToEffectType(string postEffectName)
        {
            postEffectName = PluginUtils.RemoveGroupSuffix(postEffectName);
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

        public static bool IsValidParaffinIndex(int index)
        {
            if (index < 0 || index >= timeline.paraffinCount)
            {
                return false;
            }

            return true;
        }

        public static string GetParaffinName(int index)
        {
            if (!IsValidParaffinIndex(index))
            {
                return "";
            }

            var suffix = PluginUtils.GetGroupSuffix(index);
            return ToEffectName(PostEffectType.Paraffin) + suffix;
        }

        public static string GetParaffinJpName(int index)
        {
            if (!IsValidParaffinIndex(index))
            {
                return "";
            }

            var suffix = PluginUtils.GetGroupSuffix(index);
            return ToJpName(PostEffectType.Paraffin) + suffix;
        }
    }
}