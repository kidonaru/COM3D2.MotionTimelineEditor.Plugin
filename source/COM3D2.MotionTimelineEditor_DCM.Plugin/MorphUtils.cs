using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using COM3D2.DanceCameraMotion.Plugin;

namespace COM3D2.MotionTimelineEditor_DCM.Plugin
{
    public static class MorphUtils
    {
        private static Dictionary<string, string> _morphNameJpNameMap = null;

        public static Dictionary<string, string> MorphNameJpNameMap
        {
            get
            {
                if (_morphNameJpNameMap == null)
                {
                    _morphNameJpNameMap = new Dictionary<string, string>(64);
                    foreach (var pair in MyConst.EYE_MORPH)
                    {
                        _morphNameJpNameMap[pair.Key] = pair.Value;
                    }
                    foreach (var pair in MyConst.MAYU_MORPH)
                    {
                        _morphNameJpNameMap[pair.Key] = pair.Value;
                    }
                    foreach (var pair in MyConst.MOUTH_MORPH)
                    {
                        _morphNameJpNameMap[pair.Key] = pair.Value;
                    }
                    foreach (var pair in MyConst.FACE_OPTION_MORPH)
                    {
                        _morphNameJpNameMap[pair.Key] = pair.Value;
                    }
                }
                return _morphNameJpNameMap;
            }
        }

        public static string GetMorphJpName(string morphName)
        {
            if (MorphNameJpNameMap.ContainsKey(morphName))
            {
                return MorphNameJpNameMap[morphName];
            }
            return morphName;
        }

        private static List<string> _saveMorphNames = null;
        public static List<string> saveMorphNames
        {
            get
            {
                if (_saveMorphNames == null)
                {
                    _saveMorphNames = MorphNameJpNameMap.Keys.ToList();
                }
                return _saveMorphNames;
            }
        }

        public static Dictionary<string, string> MorphSetNameJpNameMap = new Dictionary<string, string>
        {
            { "eye", "目" },
            { "mayu", "眉" },
            { "mouth", "口" },
            { "faceOption", "オプション" }
        };

        public static string GetMorphSetJpName(string morphSetName)
        {
            if (MorphSetNameJpNameMap.ContainsKey(morphSetName))
            {
                return MorphSetNameJpNameMap[morphSetName];
            }
            return morphSetName;
        }

        public static Dictionary<string, string> _morphNameToSetNameMap = null;

        public static Dictionary<string, string> MorphNameToSetNameMap
        {
            get
            {
                if (_morphNameToSetNameMap == null)
                {
                    _morphNameToSetNameMap = new Dictionary<string, string>(64);
                    foreach (var pair in MyConst.EYE_MORPH)
                    {
                        _morphNameToSetNameMap[pair.Key] = "eye";
                    }
                    foreach (var pair in MyConst.MAYU_MORPH)
                    {
                        _morphNameToSetNameMap[pair.Key] = "mayu";
                    }
                    foreach (var pair in MyConst.MOUTH_MORPH)
                    {
                        _morphNameToSetNameMap[pair.Key] = "mouth";
                    }
                    foreach (var pair in MyConst.FACE_OPTION_MORPH)
                    {
                        _morphNameToSetNameMap[pair.Key] = "faceOption";
                    }
                }
                return _morphNameToSetNameMap;
            }
        }
    }
}