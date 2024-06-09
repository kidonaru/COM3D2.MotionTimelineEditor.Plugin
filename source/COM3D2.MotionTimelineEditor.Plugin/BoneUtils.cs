using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    using AttachPoint = PhotoTransTargetObject.AttachPoint;

    public enum BoneSetMenuType
    {
        None,
        Body,
        Head,
        LeftArm,
        RightArm,
        LeftLeg,
        RightLeg,
        LeftArmFinger,
        RightArmFinger,
        LeftLegFinger,
        RightLegFinger,
        Extended,
    }

    public static class BoneUtils
    {
        private static string[] _saveBonePaths = null;
        public static string[] saveBonePaths
        {
            get
            {
                if (_saveBonePaths == null)
                {
                    var methodInfo = typeof(CacheBoneDataArray).GetMethod("GetSaveBonePathArray", BindingFlags.NonPublic | BindingFlags.Static);
                    _saveBonePaths = (string[]) methodInfo.Invoke(null, null);
                    PluginUtils.AssertNull(saveBonePaths != null, "saveBonePaths is null");
                }
                return _saveBonePaths;
            }
        }

        private static List<string> _saveBoneNames = null;
        public static List<string> saveBoneNames
        {
            get
            {
                if (_saveBoneNames == null)
                {
                    _saveBoneNames = new List<string>(saveBonePaths.Length);
                    for (int i = 0; i < saveBonePaths.Length; i++)
                    {
                        _saveBoneNames.Add(ConvertToBoneName(saveBonePaths[i]));
                    }
                }
                return _saveBoneNames;
            }
        }

        private static List<IKManager.BoneType> _saveBoneTypes = null;
        public static List<IKManager.BoneType> saveBoneTypes
        {
            get
            {
                if (_saveBoneTypes == null)
                {
                    _saveBoneTypes = new List<IKManager.BoneType>(saveBoneNames.Count);
                    for (int i = 0; i < saveBoneNames.Count; i++)
                    {
                        _saveBoneTypes.Add(GetBoneTypeByName(saveBoneNames[i]));
                    }
                }
                return _saveBoneTypes;
            }
        }

        private static Dictionary<string, string> _boneNameToPathMap = null;

        public static Dictionary<string, string> boneNameToPathMap
        {
            get
            {
                if (_boneNameToPathMap == null)
                {
                    _boneNameToPathMap = new Dictionary<string, string>(saveBonePaths.Length + 2);
                    foreach (var bonePath in saveBonePaths)
                    {
                        _boneNameToPathMap[ConvertToBoneName(bonePath)] = bonePath;
                    }

                    _boneNameToPathMap["Hip_L"] = "Bip01/Bip01 Pelvis/Hip_L";
                    _boneNameToPathMap["Hip_R"] = "Bip01/Bip01 Pelvis/Hip_R";
                }
                return _boneNameToPathMap;
            }
        }

        public static string ConvertToBoneName(string bonePath)
        {
            return bonePath.Substring(bonePath.LastIndexOf('/') + 1);
        }

        public static string ConvertToBonePath(string boneName)
        {
            string bonePath;
            if (boneNameToPathMap.TryGetValue(boneName, out bonePath))
            {
                return bonePath;
            }
            return "";
        }

        public static readonly Dictionary<IKManager.BoneType, string> BoneTypeToJpNameMap = new Dictionary<IKManager.BoneType, string>
        {
            {IKManager.BoneType.Root, "中心"},
            {IKManager.BoneType.TopFixed, "足元"},
            {IKManager.BoneType.Pelvis, "骨盤"},
            {IKManager.BoneType.Thigh_L, "足付根(左)"},
            {IKManager.BoneType.Calf_L, "膝(左)"},
            {IKManager.BoneType.Foot_L, "足首(左)"},
            {IKManager.BoneType.Toe0_Root_L, "足親指2(左)"},
            {IKManager.BoneType.Toe0_0_L, "足親指1(左)"},
            {IKManager.BoneType.Toe1_Root_L, "足中指2(左)"},
            {IKManager.BoneType.Toe1_0_L, "足中指1(左)"},
            {IKManager.BoneType.Toe2_Root_L, "足子指2(左)"},
            {IKManager.BoneType.Toe2_0_L, "足子指1(左)"},
            {IKManager.BoneType.Thigh_R, "足付根(右)"},
            {IKManager.BoneType.Calf_R, "膝(右)"},
            {IKManager.BoneType.Foot_R, "足首(右)"},
            {IKManager.BoneType.Toe0_Root_R, "足親指2(右)"},
            {IKManager.BoneType.Toe0_0_R, "足親指1(右)"},
            {IKManager.BoneType.Toe1_Root_R, "足中指2(右)"},
            {IKManager.BoneType.Toe1_0_R, "足中指1(右)"},
            {IKManager.BoneType.Toe2_Root_R, "足子指2(右)"},
            {IKManager.BoneType.Toe2_0_R, "足子指1(右)"},
            {IKManager.BoneType.Spine0, "脊椎4"},
            {IKManager.BoneType.Spine1, "脊椎3"},
            {IKManager.BoneType.Spine2, "脊椎2"},
            {IKManager.BoneType.Spine3, "脊椎1"},
            {IKManager.BoneType.Neck, "首"},
            {IKManager.BoneType.Head, "頭"},
            {IKManager.BoneType.Clavicle_L, "鎖骨(左)"},
            {IKManager.BoneType.UpperArm_L, "肩(左)"},
            {IKManager.BoneType.Forearm_L, "肘(左)"},
            {IKManager.BoneType.Hand_L, "手首(左)"},
            {IKManager.BoneType.Finger0_Root_L, "親指3(左)"},
            {IKManager.BoneType.Finger0_0_L, "親指2(左)"},
            {IKManager.BoneType.Finger0_1_L, "親指1(左)"},
            {IKManager.BoneType.Finger1_Root_L, "人差し指3(左)"},
            {IKManager.BoneType.Finger1_0_L, "人差し指2(左)"},
            {IKManager.BoneType.Finger1_1_L, "人差し指1(左)"},
            {IKManager.BoneType.Finger2_Root_L, "中指3(左)"},
            {IKManager.BoneType.Finger2_0_L, "中指2(左)"},
            {IKManager.BoneType.Finger2_1_L, "中指1(左)"},
            {IKManager.BoneType.Finger3_Root_L, "薬指3(左)"},
            {IKManager.BoneType.Finger3_0_L, "薬指2(左)"},
            {IKManager.BoneType.Finger3_1_L, "薬指1(左)"},
            {IKManager.BoneType.Finger4_Root_L, "小指3(左)"},
            {IKManager.BoneType.Finger4_0_L, "小指2(左)"},
            {IKManager.BoneType.Finger4_1_L, "小指1(左)"},
            {IKManager.BoneType.Clavicle_R, "鎖骨(右)"},
            {IKManager.BoneType.UpperArm_R, "肩(右)"},
            {IKManager.BoneType.Forearm_R, "肘(右)"},
            {IKManager.BoneType.Hand_R, "手首(右)"},
            {IKManager.BoneType.Finger0_Root_R, "親指3(右)"},
            {IKManager.BoneType.Finger0_0_R, "親指2(右)"},
            {IKManager.BoneType.Finger0_1_R, "親指1(右)"},
            {IKManager.BoneType.Finger1_Root_R, "人差し指3(右)"},
            {IKManager.BoneType.Finger1_0_R, "人差し指2(右)"},
            {IKManager.BoneType.Finger1_1_R, "人差し指1(右)"},
            {IKManager.BoneType.Finger2_Root_R, "中指3(右)"},
            {IKManager.BoneType.Finger2_0_R, "中指2(右)"},
            {IKManager.BoneType.Finger2_1_R, "中指1(右)"},
            {IKManager.BoneType.Finger3_Root_R, "薬指3(右)"},
            {IKManager.BoneType.Finger3_0_R, "薬指2(右)"},
            {IKManager.BoneType.Finger3_1_R, "薬指1(右)"},
            {IKManager.BoneType.Finger4_Root_R, "小指3(右)"},
            {IKManager.BoneType.Finger4_0_R, "小指2(右)"},
            {IKManager.BoneType.Finger4_1_R, "小指1(右)"},
            {IKManager.BoneType.Bust_L, "胸(左)"},
            {IKManager.BoneType.Bust_R, "胸(右)"}
        };

        public static string GetBoneJpName(IKManager.BoneType boneType)
        {
            string japaneseName;
            if (BoneTypeToJpNameMap.TryGetValue(boneType, out japaneseName))
            {
                return japaneseName;
            }
            PluginUtils.LogError("無効なBoneType：" + boneType);
            return "";
        }

        public static string GetBoneJpName(string boneName)
        {
            if (IsDefaultBoneName(boneName))
            {
                return GetBoneJpName(GetBoneTypeByName(boneName));
            }

            return boneName;
        }

        public static readonly Dictionary<IKManager.BoneType, string> BoneTypeToNameMap = new Dictionary<IKManager.BoneType, string>
        {
            {IKManager.BoneType.Root, "Bip01"},
            {IKManager.BoneType.TopFixed, "Bip01 Footsteps"}, // 違うけど一旦アサインしておく
            {IKManager.BoneType.Pelvis, "Bip01 Pelvis"},
            {IKManager.BoneType.Thigh_L, "Bip01 L Thigh"},
            {IKManager.BoneType.Calf_L, "Bip01 L Calf"},
            {IKManager.BoneType.Foot_L, "Bip01 L Foot"},
            {IKManager.BoneType.Toe0_Root_L, "Bip01 L Toe0"},
            {IKManager.BoneType.Toe0_0_L, "Bip01 L Toe01"},
            {IKManager.BoneType.Toe1_Root_L, "Bip01 L Toe1"},
            {IKManager.BoneType.Toe1_0_L, "Bip01 L Toe11"},
            {IKManager.BoneType.Toe2_Root_L, "Bip01 L Toe2"},
            {IKManager.BoneType.Toe2_0_L, "Bip01 L Toe21"},
            {IKManager.BoneType.Thigh_R, "Bip01 R Thigh"},
            {IKManager.BoneType.Calf_R, "Bip01 R Calf"},
            {IKManager.BoneType.Foot_R, "Bip01 R Foot"},
            {IKManager.BoneType.Toe0_Root_R, "Bip01 R Toe0"},
            {IKManager.BoneType.Toe0_0_R, "Bip01 R Toe01"},
            {IKManager.BoneType.Toe1_Root_R, "Bip01 R Toe1"},
            {IKManager.BoneType.Toe1_0_R, "Bip01 R Toe11"},
            {IKManager.BoneType.Toe2_Root_R, "Bip01 R Toe2"},
            {IKManager.BoneType.Toe2_0_R, "Bip01 R Toe21"},
            {IKManager.BoneType.Spine0, "Bip01 Spine"},
            {IKManager.BoneType.Spine1, "Bip01 Spine0a"},
            {IKManager.BoneType.Spine2, "Bip01 Spine1"},
            {IKManager.BoneType.Spine3, "Bip01 Spine1a"},
            {IKManager.BoneType.Neck, "Bip01 Neck"},
            {IKManager.BoneType.Head, "Bip01 Head"},
            {IKManager.BoneType.Clavicle_L, "Bip01 L Clavicle"},
            {IKManager.BoneType.UpperArm_L, "Bip01 L UpperArm"},
            {IKManager.BoneType.Forearm_L, "Bip01 L Forearm"},
            {IKManager.BoneType.Hand_L, "Bip01 L Hand"},
            {IKManager.BoneType.Finger0_Root_L, "Bip01 L Finger0"},
            {IKManager.BoneType.Finger0_0_L, "Bip01 L Finger01"},
            {IKManager.BoneType.Finger0_1_L, "Bip01 L Finger02"},
            {IKManager.BoneType.Finger1_Root_L, "Bip01 L Finger1"},
            {IKManager.BoneType.Finger1_0_L, "Bip01 L Finger11"},
            {IKManager.BoneType.Finger1_1_L, "Bip01 L Finger12"},
            {IKManager.BoneType.Finger2_Root_L, "Bip01 L Finger2"},
            {IKManager.BoneType.Finger2_0_L, "Bip01 L Finger21"},
            {IKManager.BoneType.Finger2_1_L, "Bip01 L Finger22"},
            {IKManager.BoneType.Finger3_Root_L, "Bip01 L Finger3"},
            {IKManager.BoneType.Finger3_0_L, "Bip01 L Finger31"},
            {IKManager.BoneType.Finger3_1_L, "Bip01 L Finger32"},
            {IKManager.BoneType.Finger4_Root_L, "Bip01 L Finger4"},
            {IKManager.BoneType.Finger4_0_L, "Bip01 L Finger41"},
            {IKManager.BoneType.Finger4_1_L, "Bip01 L Finger42"},
            {IKManager.BoneType.Clavicle_R, "Bip01 R Clavicle"},
            {IKManager.BoneType.UpperArm_R, "Bip01 R UpperArm"},
            {IKManager.BoneType.Forearm_R, "Bip01 R Forearm"},
            {IKManager.BoneType.Hand_R, "Bip01 R Hand"},
            {IKManager.BoneType.Finger0_Root_R, "Bip01 R Finger0"},
            {IKManager.BoneType.Finger0_0_R, "Bip01 R Finger01"},
            {IKManager.BoneType.Finger0_1_R, "Bip01 R Finger02"},
            {IKManager.BoneType.Finger1_Root_R, "Bip01 R Finger1"},
            {IKManager.BoneType.Finger1_0_R, "Bip01 R Finger11"},
            {IKManager.BoneType.Finger1_1_R, "Bip01 R Finger12"},
            {IKManager.BoneType.Finger2_Root_R, "Bip01 R Finger2"},
            {IKManager.BoneType.Finger2_0_R, "Bip01 R Finger21"},
            {IKManager.BoneType.Finger2_1_R, "Bip01 R Finger22"},
            {IKManager.BoneType.Finger3_Root_R, "Bip01 R Finger3"},
            {IKManager.BoneType.Finger3_0_R, "Bip01 R Finger31"},
            {IKManager.BoneType.Finger3_1_R, "Bip01 R Finger32"},
            {IKManager.BoneType.Finger4_Root_R, "Bip01 R Finger4"},
            {IKManager.BoneType.Finger4_0_R, "Bip01 R Finger41"},
            {IKManager.BoneType.Finger4_1_R, "Bip01 R Finger42"},
            {IKManager.BoneType.Bust_L, "Mune_L"},
            {IKManager.BoneType.Bust_R, "Mune_R"},

            {IKManager.BoneType.Hip_L, "Hip_L"},
            {IKManager.BoneType.Hip_R, "Hip_R"},
        };

        public static string GetBoneName(IKManager.BoneType boneType)
        {
            string boneName;
            if (BoneTypeToNameMap.TryGetValue(boneType, out boneName))
            {
                return boneName;
            }
            PluginUtils.LogError("無効なBoneType：" + boneType);
            return "";
        }

        private static Dictionary<string, IKManager.BoneType> _boneNameToTypeMap = null;
        public static Dictionary<string, IKManager.BoneType> BoneNameToTypeMap
        {
            get
            {
                if (_boneNameToTypeMap == null)
                {
                    _boneNameToTypeMap = BoneTypeToNameMap.ToDictionary(kv => kv.Value, kv => kv.Key);
                }
                return _boneNameToTypeMap;
            }
        }

        public static IKManager.BoneType GetBoneTypeByName(string boneName)
        {
            IKManager.BoneType boneType;
            if (BoneNameToTypeMap.TryGetValue(boneName, out boneType))
            {
                return boneType;
            }
            //PluginUtils.LogError("無効なBoneName：" + boneName);
            return IKManager.BoneType.TopFixed;
        }

        public static bool IsDefaultBoneName(string boneName)
        {
            return BoneNameToTypeMap.ContainsKey(boneName);
        }

        public static bool IsVisibleBoneName(string boneName)
        {
            if (boneName.EndsWith("nub", System.StringComparison.Ordinal))
            {
                return false;
            }
            if (boneName.EndsWith("_SCL_", System.StringComparison.Ordinal))
            {
                return false;
            }

            return true;
        }

        public static readonly Dictionary<IKManager.BoneType, BoneSetMenuType> BoneTypeToSetMenuTypeMap = new Dictionary<IKManager.BoneType, BoneSetMenuType>
        {
            {IKManager.BoneType.Root, BoneSetMenuType.None},

            {IKManager.BoneType.TopFixed, BoneSetMenuType.Body},
            {IKManager.BoneType.Pelvis, BoneSetMenuType.Body},
            {IKManager.BoneType.Spine3, BoneSetMenuType.Body},
            {IKManager.BoneType.Spine2, BoneSetMenuType.Body},
            {IKManager.BoneType.Spine1, BoneSetMenuType.Body},
            {IKManager.BoneType.Spine0, BoneSetMenuType.Body},
            {IKManager.BoneType.Bust_L, BoneSetMenuType.Body},
            {IKManager.BoneType.Bust_R, BoneSetMenuType.Body},

            {IKManager.BoneType.Neck, BoneSetMenuType.Head},
            {IKManager.BoneType.Head, BoneSetMenuType.Head},

            {IKManager.BoneType.Clavicle_L, BoneSetMenuType.LeftArm},
            {IKManager.BoneType.UpperArm_L, BoneSetMenuType.LeftArm},
            {IKManager.BoneType.Forearm_L, BoneSetMenuType.LeftArm},
            {IKManager.BoneType.Hand_L, BoneSetMenuType.LeftArm},

            {IKManager.BoneType.Clavicle_R, BoneSetMenuType.RightArm},
            {IKManager.BoneType.UpperArm_R, BoneSetMenuType.RightArm},
            {IKManager.BoneType.Forearm_R, BoneSetMenuType.RightArm},
            {IKManager.BoneType.Hand_R, BoneSetMenuType.RightArm},

            {IKManager.BoneType.Thigh_L, BoneSetMenuType.LeftLeg},
            {IKManager.BoneType.Calf_L, BoneSetMenuType.LeftLeg},
            {IKManager.BoneType.Foot_L, BoneSetMenuType.LeftLeg},

            {IKManager.BoneType.Thigh_R, BoneSetMenuType.RightLeg},
            {IKManager.BoneType.Calf_R, BoneSetMenuType.RightLeg},
            {IKManager.BoneType.Foot_R, BoneSetMenuType.RightLeg},

            {IKManager.BoneType.Finger0_Root_L, BoneSetMenuType.LeftArmFinger},
            {IKManager.BoneType.Finger0_0_L, BoneSetMenuType.LeftArmFinger},
            {IKManager.BoneType.Finger0_1_L, BoneSetMenuType.LeftArmFinger},
            {IKManager.BoneType.Finger1_Root_L, BoneSetMenuType.LeftArmFinger},
            {IKManager.BoneType.Finger1_0_L, BoneSetMenuType.LeftArmFinger},
            {IKManager.BoneType.Finger1_1_L, BoneSetMenuType.LeftArmFinger},
            {IKManager.BoneType.Finger2_Root_L, BoneSetMenuType.LeftArmFinger},
            {IKManager.BoneType.Finger2_0_L, BoneSetMenuType.LeftArmFinger},
            {IKManager.BoneType.Finger2_1_L, BoneSetMenuType.LeftArmFinger},
            {IKManager.BoneType.Finger3_Root_L, BoneSetMenuType.LeftArmFinger},
            {IKManager.BoneType.Finger3_0_L, BoneSetMenuType.LeftArmFinger},
            {IKManager.BoneType.Finger3_1_L, BoneSetMenuType.LeftArmFinger},
            {IKManager.BoneType.Finger4_Root_L, BoneSetMenuType.LeftArmFinger},
            {IKManager.BoneType.Finger4_0_L, BoneSetMenuType.LeftArmFinger},
            {IKManager.BoneType.Finger4_1_L, BoneSetMenuType.LeftArmFinger},

            {IKManager.BoneType.Finger0_Root_R, BoneSetMenuType.RightArmFinger},
            {IKManager.BoneType.Finger0_0_R, BoneSetMenuType.RightArmFinger},
            {IKManager.BoneType.Finger0_1_R, BoneSetMenuType.RightArmFinger},
            {IKManager.BoneType.Finger1_Root_R, BoneSetMenuType.RightArmFinger},
            {IKManager.BoneType.Finger1_0_R, BoneSetMenuType.RightArmFinger},
            {IKManager.BoneType.Finger1_1_R, BoneSetMenuType.RightArmFinger},
            {IKManager.BoneType.Finger2_Root_R, BoneSetMenuType.RightArmFinger},
            {IKManager.BoneType.Finger2_0_R, BoneSetMenuType.RightArmFinger},
            {IKManager.BoneType.Finger2_1_R, BoneSetMenuType.RightArmFinger},
            {IKManager.BoneType.Finger3_Root_R, BoneSetMenuType.RightArmFinger},
            {IKManager.BoneType.Finger3_0_R, BoneSetMenuType.RightArmFinger},
            {IKManager.BoneType.Finger3_1_R, BoneSetMenuType.RightArmFinger},
            {IKManager.BoneType.Finger4_Root_R, BoneSetMenuType.RightArmFinger},
            {IKManager.BoneType.Finger4_0_R, BoneSetMenuType.RightArmFinger},
            {IKManager.BoneType.Finger4_1_R, BoneSetMenuType.RightArmFinger},

            {IKManager.BoneType.Toe0_Root_L, BoneSetMenuType.LeftLegFinger},
            {IKManager.BoneType.Toe0_0_L, BoneSetMenuType.LeftLegFinger},
            {IKManager.BoneType.Toe1_Root_L, BoneSetMenuType.LeftLegFinger},
            {IKManager.BoneType.Toe1_0_L, BoneSetMenuType.LeftLegFinger},
            {IKManager.BoneType.Toe2_Root_L, BoneSetMenuType.LeftLegFinger},
            {IKManager.BoneType.Toe2_0_L, BoneSetMenuType.LeftLegFinger},

            {IKManager.BoneType.Toe0_Root_R, BoneSetMenuType.RightLegFinger},
            {IKManager.BoneType.Toe0_0_R, BoneSetMenuType.RightLegFinger},
            {IKManager.BoneType.Toe1_Root_R, BoneSetMenuType.RightLegFinger},
            {IKManager.BoneType.Toe1_0_R, BoneSetMenuType.RightLegFinger},
            {IKManager.BoneType.Toe2_Root_R, BoneSetMenuType.RightLegFinger},
            {IKManager.BoneType.Toe2_0_R, BoneSetMenuType.RightLegFinger},
        };

        private static BoneSetMenuType _GetBoneSetMenuType(IKManager.BoneType boneType)
        {
            BoneSetMenuType boneSetMenuType;
            if (BoneTypeToSetMenuTypeMap.TryGetValue(boneType, out boneSetMenuType))
            {
                return boneSetMenuType;
            }
            PluginUtils.LogError("無効なBoneType：" + boneType);
            return BoneSetMenuType.Body;
        }

        public static BoneSetMenuType GetBoneSetMenuType(string boneName)
        {
            if (IsDefaultBoneName(boneName))
            {
                return _GetBoneSetMenuType(GetBoneTypeByName(boneName));
            }
            return BoneSetMenuType.Extended;
        }

        public static readonly Dictionary<BoneSetMenuType, string> BoneSetMenuTypeToJpNameMap = new Dictionary<BoneSetMenuType, string>
        {
            {BoneSetMenuType.None, ""},
            {BoneSetMenuType.Body, "体"},
            {BoneSetMenuType.Head, "頭"},
            {BoneSetMenuType.LeftArm, "左腕"},
            {BoneSetMenuType.RightArm, "右腕"},
            {BoneSetMenuType.LeftLeg, "左足"},
            {BoneSetMenuType.RightLeg, "右足"},
            {BoneSetMenuType.LeftArmFinger, "左手指"},
            {BoneSetMenuType.RightArmFinger, "右手指"},
            {BoneSetMenuType.LeftLegFinger, "左足指"},
            {BoneSetMenuType.RightLegFinger, "右足指"},
            {BoneSetMenuType.Extended, "追加"},
        };

        public static string GetBoneSetMenuJpName(BoneSetMenuType boneSetMenuType)
        {
            string japaneseName;
            if (BoneSetMenuTypeToJpNameMap.TryGetValue(boneSetMenuType, out japaneseName))
            {
                return japaneseName;
            }
            PluginUtils.LogError("無効なBoneSetMenuType：" + boneSetMenuType);
            return "";
        }

        public static readonly Dictionary<IKManager.BoneType, Vector3> InitialEulerAnglesMap = new Dictionary<IKManager.BoneType, Vector3>
        {
            {IKManager.BoneType.Root, new Vector3(270f, 180f, 270f)},
            {IKManager.BoneType.Spine3, new Vector3(0f, 0f, 0f)},
            {IKManager.BoneType.Spine2, new Vector3(0f, 0f, 0f)},
            {IKManager.BoneType.Spine1, new Vector3(0f, 0f, 0f)},
            {IKManager.BoneType.Spine0, new Vector3(270f, 90f, 0f)},
            {IKManager.BoneType.Neck, new Vector3(0f, 0f, 0f)},
            {IKManager.BoneType.Head, new Vector3(0f, 0f, 0f)},
            {IKManager.BoneType.Clavicle_L, new Vector3(0f, 270f, 180f)},
            {IKManager.BoneType.UpperArm_L, new Vector3(90f, 280f, 0f)},
            {IKManager.BoneType.Forearm_L, new Vector3(0f, 0f, 0f)},
            {IKManager.BoneType.Hand_L, new Vector3(180f, 0f, 0f)},
            {IKManager.BoneType.Clavicle_R, new Vector3(0f, 90f, 180f)},
            {IKManager.BoneType.UpperArm_R, new Vector3(270f, 80f, 0f)},
            {IKManager.BoneType.Forearm_R, new Vector3(0f, 0f, 0f)},
            {IKManager.BoneType.Hand_R, new Vector3(180f, 0f, 0f)},
            {IKManager.BoneType.Pelvis, new Vector3(270f, 90f, 0f)},
            {IKManager.BoneType.Thigh_L, new Vector3(0f, 180f, 0f)},
            {IKManager.BoneType.Calf_L, new Vector3(0f, 0f, 0f)},
            {IKManager.BoneType.Foot_L, new Vector3(0f, 0f, 0f)},
            {IKManager.BoneType.Thigh_R, new Vector3(0f, 180f, 0f)},
            {IKManager.BoneType.Calf_R, new Vector3(0f, 0f, 0f)},
            {IKManager.BoneType.Foot_R, new Vector3(0f, 0f, 0f)},
            {IKManager.BoneType.Bust_L, new Vector3(0f, 180f, 270f)},
            {IKManager.BoneType.Bust_R, new Vector3(0f, 0f, 270f)},
            {IKManager.BoneType.Toe0_Root_L, new Vector3(0f, 0f, 280f)},
            {IKManager.BoneType.Toe1_Root_L, new Vector3(0f, 0f, 280f)},
            {IKManager.BoneType.Toe2_Root_L, new Vector3(0f, 0f, 280f)},
            {IKManager.BoneType.Toe0_Root_R, new Vector3(0f, 0f, 280f)},
            {IKManager.BoneType.Toe1_Root_R, new Vector3(0f, 0f, 280f)},
            {IKManager.BoneType.Toe2_Root_R, new Vector3(0f, 0f, 280f)},
        };

        public static Vector3 GetInitialEulerAngles(IKManager.BoneType boneType)
        {
            Vector3 initialRotation;
            if (InitialEulerAnglesMap.TryGetValue(boneType, out initialRotation))
            {
                return initialRotation;
            }
            return Vector3.zero;
        }

        public static Vector3 GetInitialPosition(IKManager.BoneType boneType)
        {
            if (boneType == IKManager.BoneType.Root)
            {
                return new Vector3(0f, 0.9f, 0f);
            }
            return Vector3.zero;
        }

        public static readonly Dictionary<AttachPoint, IKManager.BoneType> AttachPointToBoneTypeMap = new Dictionary<AttachPoint, IKManager.BoneType>
        {
            {AttachPoint.Fix, IKManager.BoneType.TopFixed},
            {AttachPoint.Root, IKManager.BoneType.Root},
            {AttachPoint.Head, IKManager.BoneType.Head},
            {AttachPoint.Neck, IKManager.BoneType.Neck},
            {AttachPoint.Mune_R, IKManager.BoneType.Bust_R},
            {AttachPoint.UpperArm_R, IKManager.BoneType.UpperArm_R},
            {AttachPoint.Forearm_R, IKManager.BoneType.Forearm_R},
            {AttachPoint.Hand_R, IKManager.BoneType.Hand_R},
            {AttachPoint.Thigh_R, IKManager.BoneType.Thigh_R},
            {AttachPoint.Calf_R, IKManager.BoneType.Calf_R},
            {AttachPoint.Foot_R, IKManager.BoneType.Foot_R},
            {AttachPoint.Mune_L, IKManager.BoneType.Bust_L},
            {AttachPoint.UpperArm_L, IKManager.BoneType.UpperArm_L},
            {AttachPoint.Forearm_L, IKManager.BoneType.Forearm_L},
            {AttachPoint.Hand_L, IKManager.BoneType.Hand_L},
            {AttachPoint.Thigh_L, IKManager.BoneType.Thigh_L},
            {AttachPoint.Calf_L, IKManager.BoneType.Calf_L},
            {AttachPoint.Foot_L, IKManager.BoneType.Foot_L},
        };

        public static readonly Dictionary<IKManager.BoneType, AttachPoint> BoneTypeToAttachPointMap =
            AttachPointToBoneTypeMap.ToDictionary(kv => kv.Value, kv => kv.Key);

        public static AttachPoint GetAttachPoint(IKManager.BoneType boneType)
        {
            AttachPoint attachPoint;
            if (BoneTypeToAttachPointMap.TryGetValue(boneType, out attachPoint))
            {
                return attachPoint;
            }
            return AttachPoint.Null;
        }

        public static IKManager.BoneType GetBoneType(AttachPoint attachPoint)
        {
            IKManager.BoneType boneType;
            if (AttachPointToBoneTypeMap.TryGetValue(attachPoint, out boneType))
            {
                return boneType;
            }
            return IKManager.BoneType.TopFixed;
        }

        public static readonly List<string> AttachPointNames = new List<string>
        {
            "設定なし",
            "固定",
            "原点",
            "頭",
            "首元",
            "右胸",
            "右肩",
            "右肘",
            "右手首",
            "右足付け根",
            "右膝",
            "右足首",
            "左胸",
            "左肩",
            "左肘",
            "左手首",
            "左足付け根",
            "左膝",
            "左足首",  
        };

        public static string GetAttachPointName(AttachPoint attachPoint)
        {
            var index = (int)attachPoint;
            if (index < 0 || index >= AttachPointNames.Count)
            {
                return "";
            }
            return AttachPointNames[(int)attachPoint];
        }
    }
}