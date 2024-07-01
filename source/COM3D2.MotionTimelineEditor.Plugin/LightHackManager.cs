using System.Collections.Generic;
using UnityEngine.SceneManagement;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    public interface ILightHack
    {
        List<StudioLightStat> lightList { get; }

        void DeleteAllLights();
        void DeleteLight(StudioLightStat stat);
        void CreateLight(StudioLightStat stat);
        void ApplyLight(StudioLightStat stat);
    }

    public class LightHackManager
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

        private static StudioHackBase studioHack
        {
            get
            {
                return StudioHackManager.studioHack;
            }
        }

        private LightHackManager()
        {
        }

        public void DeleteAllLights()
        {
            studioHack.DeleteAllLights();
        }

        public void DeleteLight(StudioLightStat stat)
        {
            studioHack.DeleteLight(stat);
        }

        public void CreateLight(StudioLightStat stat)
        {
            studioHack.CreateLight(stat);
        }

        public void ApplyLight(StudioLightStat stat)
        {
            studioHack.ApplyLight(stat);
        }

        public void Update()
        {
        }
    }
}