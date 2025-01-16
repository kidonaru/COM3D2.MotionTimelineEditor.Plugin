using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using COM3D2.DanceCameraMotion.Plugin;
using UnityEngine;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    using DCM = COM3D2.DanceCameraMotion.Plugin.DanceCameraMotion;

    public static class DCMUtils
    {
        private static DCM _danceCameraMotion; 

        public static DCM danceCameraMotion
        {
            get
            {
                if (_danceCameraMotion == null)
                {
                    GameObject gameObject = GameObject.Find("UnityInjector");
                    _danceCameraMotion = gameObject.GetComponent<DCM>();
                    MTEUtils.AssertNull(_danceCameraMotion != null, "_danceCameraMotion is null");
                }
                return _danceCameraMotion;
            }
        }

        private static FieldInfo _stageMgrField = null;

        public static SatgeObjectManager stageObjectManager
        {
            get
            {
                if (_stageMgrField == null)
                {
                    _stageMgrField = typeof(DCM).GetField("stageMgr", BindingFlags.NonPublic | BindingFlags.Instance);
                    MTEUtils.AssertNull(_stageMgrField != null, "_stageMgrField is null");
                }

                return (SatgeObjectManager)_stageMgrField.GetValue(danceCameraMotion);
            }
        }

        private static FieldInfo _cameraDataField = null;

        public static Dictionary<int, TimeLineSet> GetCameraData(
            this TimelineCameraManager self)
        {
            if (_cameraDataField == null)
            {
                _cameraDataField = typeof(TimelineCameraManager).GetField("cameraData",
                    BindingFlags.NonPublic | BindingFlags.Instance);
            }

            return (Dictionary<int, TimeLineSet>) _cameraDataField.GetValue(self);
        }

        private static FieldInfo _cameraPlayDataField = null;

        public static TimeLinePlaySet GetPlayData(this TimelineCameraManager self)
        {
            if (_cameraPlayDataField == null)
            {
                _cameraPlayDataField = typeof(TimelineCameraManager).GetField("playData",
                    BindingFlags.NonPublic | BindingFlags.Instance);
            }

            return (TimeLinePlaySet) _cameraPlayDataField.GetValue(self);
        }

        public static void SetPlayData(
            this TimelineCameraManager self, TimeLinePlaySet playData)
        {
            if (_cameraPlayDataField == null)
            {
                _cameraPlayDataField = typeof(TimelineCameraManager).GetField("playData",
                    BindingFlags.NonPublic | BindingFlags.Instance);
            }

            _cameraPlayDataField.SetValue(self, playData);
        }

        private static FieldInfo playDataField = null;

        public static Dictionary<int, Dictionary<string, TimeLinePlaySet>> GetPlayData(
            this Timeline self)
        {
            if (playDataField == null)
            {
                playDataField = typeof(Timeline).GetField("playData",
                    BindingFlags.NonPublic | BindingFlags.Instance);
            }

            return (Dictionary<int, Dictionary<string, TimeLinePlaySet>>) playDataField.GetValue(self);
        }

        public static void SetPlayData(
            this Timeline self,
            Dictionary<int, Dictionary<string, TimeLinePlaySet>> playData)
        {
            if (playDataField == null)
            {
                playDataField = typeof(Timeline).GetField("playData",
                    BindingFlags.NonPublic | BindingFlags.Instance);
            }

            playDataField.SetValue(self, playData);
        }

        private static FieldInfo _playTimeDataField = null;

        public static Dictionary<int, float> GetPlayTimeData(this Timeline self)
        {
            if (_playTimeDataField == null)
            {
                _playTimeDataField = typeof(Timeline).GetField("playTimeData",
                    BindingFlags.NonPublic | BindingFlags.Instance);
            }

            return (Dictionary<int, float>) _playTimeDataField.GetValue(self);
        }

        private static FieldInfo _motionDataField = null;

        public static Dictionary<int, Dictionary<int, List<TimeLineSet>>> GetMotionData(this Timeline self)
        {
            if (_motionDataField == null)
            {
                _motionDataField = typeof(Timeline).GetField("motionData",
                    BindingFlags.NonPublic | BindingFlags.Instance);
            }

            return (Dictionary<int, Dictionary<int, List<TimeLineSet>>>) _motionDataField.GetValue(self);
        }
    }
}