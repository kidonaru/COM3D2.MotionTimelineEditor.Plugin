using System;
using System.IO;
using System.Reflection;
using COM3D2.MotionTimelineEditor.Plugin;
using UnityEngine;

namespace COM3D2.MotionTimelineEditor_PartsEdit.Plugin
{
    public class PartsEditHack : PartsEditHackBase
    {
        private Type _yureUtilType = null;
        private Type _settingType = null;
        private Type _commonUIDataType = null;

        private FieldInfo _boneDisplayInfo = null;

        public override BoneDisplay boneDisplay
        {
            get
            {
                if (_boneDisplayInfo == null)
                {
                    _boneDisplayInfo = _settingType.GetField("boneDisplay", BindingFlags.Static | BindingFlags.Public);
                    PluginUtils.AssertNull(_boneDisplayInfo != null, "boneDisplay field is null");
                }

                return (BoneDisplay)_boneDisplayInfo.GetValue(null);
            }
            set
            {
                if (_boneDisplayInfo == null)
                {
                    _boneDisplayInfo = _settingType.GetField("boneDisplay", BindingFlags.Static | BindingFlags.Public);
                    PluginUtils.AssertNull(_boneDisplayInfo != null, "boneDisplay field is null");
                }

                _boneDisplayInfo.SetValue(null, (int) value);
            }
        }

        private FieldInfo _gizmoTypeInfo = null;

        public override GizmoType gizmoType
        {
            get
            {
                if (_gizmoTypeInfo == null)
                {
                    _gizmoTypeInfo = _settingType.GetField("gizmoType", BindingFlags.Static | BindingFlags.Public);
                    PluginUtils.AssertNull(_gizmoTypeInfo != null, "gizmoType field is null");
                }

                return (GizmoType)_gizmoTypeInfo.GetValue(null);
            }
            set
            {
                if (_gizmoTypeInfo == null)
                {
                    _gizmoTypeInfo = _settingType.GetField("gizmoType", BindingFlags.Static | BindingFlags.Public);
                    PluginUtils.AssertNull(_gizmoTypeInfo != null, "gizmoType field is null");
                }

                _gizmoTypeInfo.SetValue(null, (int) value);
            }
        }

        private FieldInfo _targetSelectModeInfo = null;

        public override int targetSelectMode
        {
            get
            {
                if (_targetSelectModeInfo == null)
                {
                    _targetSelectModeInfo = _settingType.GetField("targetSelectMode", BindingFlags.Static | BindingFlags.Public);
                    PluginUtils.AssertNull(_targetSelectModeInfo != null, "targetSelectMode field is null");
                }

                return (int)_targetSelectModeInfo.GetValue(null);
            }
            set
            {
                if (_targetSelectModeInfo == null)
                {
                    _targetSelectModeInfo = _settingType.GetField("targetSelectMode", BindingFlags.Static | BindingFlags.Public);
                    PluginUtils.AssertNull(_targetSelectModeInfo != null, "targetSelectMode field is null");
                }

                _targetSelectModeInfo.SetValue(null, value);
            }
        }

        public PartsEditHack()
        {
        }

        private readonly static string[] PartsEditPluginFileNames = new string[]
        {
            "COM3D2.PartsEdit.Plugin.dll",
            "COM3D2.PartsEditWithStudio.Plugin.dll",
        };

        private readonly static string[] SettingTypeNames = new string[]
        {
            "CM3D2.PartsEdit.Plugin.Setting",
            "COM3D2.PartsEdit.Plugin.Setting",
        };

        private readonly static string[] CommonUIDataTypeNames = new string[]
        {
            "CM3D2.PartsEdit.Plugin.CommonUIData",
            "COM3D2.PartsEdit.Plugin.CommonUIData",
        };

        public override bool Init()
        {
            PluginUtils.Log("PartsEditHack: 初期化中...");

            try
            {
                Assembly assembly = null;
                foreach (var fileName in PartsEditPluginFileNames)
                {
                    var assemblyPath = Path.GetFullPath(PluginUtils.CombinePaths(
                        "Sybaris", "UnityInjector", fileName));
                    if (File.Exists(assemblyPath))
                    {
                        assembly = Assembly.LoadFile(assemblyPath);
                        break;
                    }
                }
                PluginUtils.AssertNull(assembly != null, "PartsEdit.Plugin.dll not found");

                _yureUtilType = assembly.GetType("YureUtil");
                PluginUtils.AssertNull(_yureUtilType != null, "YureUtil type is null");

                foreach (var typeName in SettingTypeNames)
                {
                    _settingType = assembly.GetType(typeName);
                    if (_settingType != null) break;
                }
                PluginUtils.AssertNull(_settingType != null, "Setting type is null");

                foreach (var typeName in CommonUIDataTypeNames)
                {
                    _commonUIDataType = assembly.GetType(typeName);
                    if (_commonUIDataType != null) break;
                }
                PluginUtils.AssertNull(_commonUIDataType != null, "CommonUIData type is null");

                if (_yureUtilType == null || _settingType == null || _commonUIDataType == null)
                {
                    return false;
                }

                boneDisplay = BoneDisplay.None;
                gizmoType = GizmoType.None;

                return true;
            }
            catch (Exception e)
            {
                PluginUtils.LogException(e);
                return false;
            }
        }

        private MethodInfo _getYureAbleInfo = null;

        public override bool GetYureAble(Maid maid, int slotNo)
        {
            if (_getYureAbleInfo == null)
            {
                _getYureAbleInfo = _yureUtilType.GetMethod("GetYureAble", BindingFlags.Static | BindingFlags.Public);
                PluginUtils.AssertNull(_getYureAbleInfo != null, "GetYureAble method is null");
            }

            return (bool)_getYureAbleInfo.Invoke(null, new object[] { maid, slotNo });
        }

        private MethodInfo _getYureStateInfo = null;

        public override bool GetYureState(Maid maid, int slotNo)
        {
            if (_getYureStateInfo == null)
            {
                _getYureStateInfo = _yureUtilType.GetMethod("GetYureState", BindingFlags.Static | BindingFlags.Public);
                PluginUtils.AssertNull(_getYureStateInfo != null, "GetYureState method is null");
            }

            return (bool)_getYureStateInfo.Invoke(null, new object[] { maid, slotNo });
        }

        private MethodInfo _setYureStateInfo = null;

        public override void SetYureState(Maid maid, int slotNo, bool state)
        {
            if (_setYureStateInfo == null)
            {
                _setYureStateInfo = _yureUtilType.GetMethod("SetYureState", BindingFlags.Static | BindingFlags.Public);
                PluginUtils.AssertNull(_setYureStateInfo != null, "SetYureState method is null");
            }

            _setYureStateInfo.Invoke(null, new object[] { maid, slotNo, state });
        }

        private MethodInfo _setMaidInfo = null;

        public override void SetMaid(Maid maid)
        {
            if (_setMaidInfo == null)
            {
                _setMaidInfo = _commonUIDataType.GetMethod("SetMaid", BindingFlags.Static | BindingFlags.Public);
                PluginUtils.AssertNull(_setMaidInfo != null, "SetMaid method is null");
            }

            _setMaidInfo.Invoke(null, new object[] { maid });
        }

        private MethodInfo _setSlotInfo = null;

        public override void SetSlot(int slotNo)
        {
            if (_setSlotInfo == null)
            {
                _setSlotInfo = _commonUIDataType.GetMethod("SetSlot", BindingFlags.Static | BindingFlags.Public);
                PluginUtils.AssertNull(_setSlotInfo != null, "SetSlot method is null");
            }

            _setSlotInfo.Invoke(null, new object[] { slotNo });
        }

        private MethodInfo _setObjectInfo = null;

        public override void SetObject(GameObject obj)
        {
            if (_setObjectInfo == null)
            {
                _setObjectInfo = _commonUIDataType.GetMethod("SetObject", BindingFlags.Static | BindingFlags.Public);
                PluginUtils.AssertNull(_setObjectInfo != null, "SetObject method is null");
            }

            _setObjectInfo.Invoke(null, new object[] { obj });
        }

        private MethodInfo _setBoneInfo = null;

        public override void SetBone(Transform bone)
        {
            if (_setBoneInfo == null)
            {
                _setBoneInfo = _commonUIDataType.GetMethod("SetBone", BindingFlags.Static | BindingFlags.Public);
                PluginUtils.AssertNull(_setBoneInfo != null, "SetBone method is null");
            }

            _setBoneInfo.Invoke(null, new object[] { bone });
        }
    }
}