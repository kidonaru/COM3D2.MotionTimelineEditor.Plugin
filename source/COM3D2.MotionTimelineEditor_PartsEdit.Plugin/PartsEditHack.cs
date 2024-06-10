using System;
using System.IO;
using System.Reflection;
using COM3D2.MotionTimelineEditor.Plugin;

namespace COM3D2.MotionTimelineEditor_PartsEdit.Plugin
{
    public class PartsEditHack : PartsEditHackBase
    {
        private Type _yureUtilType = null;

        public PartsEditHack()
        {
        }

        private readonly static string[] PartsEditPluginFileNames = new string[]
        {
            "COM3D2.PartsEdit.Plugin.dll",
            "COM3D2.PartsEditWithStudio.Plugin.dll"
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

                return _yureUtilType != null;
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
            }

            return (bool)_getYureAbleInfo.Invoke(null, new object[] { maid, slotNo });
        }

        private MethodInfo _getYureStateInfo = null;

        public override bool GetYureState(Maid maid, int slotNo)
        {
            if (_getYureStateInfo == null)
            {
                _getYureStateInfo = _yureUtilType.GetMethod("GetYureState", BindingFlags.Static | BindingFlags.Public);
            }

            return (bool)_getYureStateInfo.Invoke(null, new object[] { maid, slotNo });
        }

        private MethodInfo _setYureStateInfo = null;

        public override void SetYureState(Maid maid, int slotNo, bool state)
        {
            if (_setYureStateInfo == null)
            {
                _setYureStateInfo = _yureUtilType.GetMethod("SetYureState", BindingFlags.Static | BindingFlags.Public);
            }

            _setYureStateInfo.Invoke(null, new object[] { maid, slotNo, state });
        }
    }
}