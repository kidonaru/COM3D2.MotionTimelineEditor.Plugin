using UnityEngine.SceneManagement;
using System.Collections.Generic;
using UnityEngine.Events;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    public class StudioHackManager
    {
        private List<StudioHackBase> studioHacks = new List<StudioHackBase>();
        private List<StudioHackBase> activeStudioHacks = new List<StudioHackBase>();

        public static StudioHackBase studioHack { get; private set; }

        public static event UnityAction<bool> onPoseEditingChanged;

        private static StudioHackManager _instance;
        public static StudioHackManager instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new StudioHackManager();
                }

                return _instance;
            }
        }

        private static Config config => ConfigManager.config;

        private StudioHackManager()
        {
            SceneManager.sceneLoaded += OnChangedSceneLevel;
        }

        public void Register(StudioHackBase studioHack)
        {
            if (studioHack == null || !studioHack.Init())
            {
                return;
            }

            studioHacks.Add(studioHack);
            studioHacks.Sort((a, b) => b.priority - a.priority);
        }

        private bool _isPoseEditing = false;

        public void Update()
        {
            if (studioHacks.Count == 0)
            {
                return;
            }

            studioHack = null;
            activeStudioHacks.Clear();

            foreach (var hack in studioHacks)
            {
                if (hack.isSceneActive)
                {
                    activeStudioHacks.Add(hack);
                }
            }

            foreach (var hack in activeStudioHacks)
            {
                if (hack.IsValid())
                {
                    studioHack = hack;
                    break;
                }
            }

            if (studioHack == null && activeStudioHacks.Count > 0)
            {
                studioHack = activeStudioHacks[0];
            }

            if (studioHack != null)
            {
                if (studioHack.isPoseEditing != _isPoseEditing)
                {
                    _isPoseEditing = studioHack.isPoseEditing;

                    if (onPoseEditingChanged != null)
                    {
                        onPoseEditingChanged(_isPoseEditing);
                    }
                }
            }
        }

        public void OnChangedSceneLevel(Scene sceneName, LoadSceneMode sceneMode)
        {
            if (!config.pluginEnabled)
            {
                return;
            }

            foreach (var studioHack in studioHacks)
            {
                studioHack.OnChangedSceneLevel(sceneName, sceneMode);
            }
        }
    }
}