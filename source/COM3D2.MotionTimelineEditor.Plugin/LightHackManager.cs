using System.Collections.Generic;
using UnityEngine.SceneManagement;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    public interface ILightHack
    {
        List<StudioLightStat> lightList { get; }

        bool CanCreateLight();
        void DeleteAllLights();
        void DeleteLight(StudioLightStat stat);
        void CreateLight(StudioLightStat stat);
        void ChangeLight(StudioLightStat stat);
        void ApplyLight(StudioLightStat stat);
    }

    public class LightHackManager : ManagerBase
    {
        private List<StudioLightStat> _lightList = new List<StudioLightStat>();
        public List<StudioLightStat> lightList
        {
            get
            {
                _lightList.Clear();
                _lightList.AddRange(studioHack.lightList);
                return _lightList;
            }
        }

        private static LightHackManager _instance;
        public static LightHackManager instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new LightHackManager();
                }

                return _instance;
            }
        }

        private LightHackManager()
        {
        }

        public bool IsValid()
        {
            return studioHack != null && studioHack.IsValid();
        }

        public bool CanCreateLight()
        {
            if (IsValid())
            {
                return studioHack.CanCreateLight();
            }
            return false;
        }

        public void DeleteAllLights()
        {
            if (IsValid())
            {
                studioHack.DeleteAllLights();
            }
        }

        public void DeleteLight(StudioLightStat stat)
        {
            if (IsValid())
            {
                studioHack.DeleteLight(stat);
            }
        }

        public void CreateLight(StudioLightStat stat)
        {
            if (IsValid())
            {
                studioHack.CreateLight(stat);
            }
        }

        public void ChangeLight(StudioLightStat stat)
        {
            if (IsValid())
            {
                studioHack.ChangeLight(stat);
            }
        }

        public void ApplyLight(StudioLightStat stat)
        {
            if (IsValid())
            {
                studioHack.ApplyLight(stat);
            }
        }

        public override void Update()
        {
        }
    }
}