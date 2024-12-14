using System.Collections.Generic;
using System.Linq;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    public enum PostEffectType
    {
        DepthOfField,
        Paraffin,
        DistanceFog,
        Rimlight,
    }

    public static class PostEffectUtils
    {
        private static TimelineData timeline => TimelineManager.instance.timeline;

        public static readonly Dictionary<PostEffectType, string> PostEffectTypeToJpNameMap = new Dictionary<PostEffectType, string>
        {
            { PostEffectType.DepthOfField, "被写界深度" },
            { PostEffectType.Paraffin, "パラフィン" },
            { PostEffectType.DistanceFog, "距離フォグ" },
            { PostEffectType.Rimlight, "リムライト" },
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

        private static Dictionary<string, int> _indexCache = new Dictionary<string, int>(16);

        public static int GetEffectIndex(string name)
        {
            int index;
            if (_indexCache.TryGetValue(name, out index))
            {
                return index;
            }

            index = PluginUtils.ExtractGroup(name);
            _indexCache[name] = index;
            return index;
        }

        private static Dictionary<string, PostEffectType> _effectTypeCache = new Dictionary<string, PostEffectType>(16);

        public static PostEffectType GetEffectType(string name)
        {
            PostEffectType type;
            if (_effectTypeCache.TryGetValue(name, out type))
            {
                return type;
            }

            type = ToEffectType(name);
            _effectTypeCache[name] = type;
            return type;
        }
    }
}