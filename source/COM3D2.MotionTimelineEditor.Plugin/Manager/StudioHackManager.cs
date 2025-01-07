using UnityEngine.SceneManagement;
using System.Collections.Generic;
using UnityEngine.Events;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    public class StudioHackManager : ManagerBase
    {
        private List<StudioHackBase> studioHacks = new List<StudioHackBase>();
        private List<StudioHackBase> activeStudioHacks = new List<StudioHackBase>();

        private StudioHackBase _studioHack = null;
        public override StudioHackBase studioHack => _studioHack;

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

        private StudioHackManager()
        {
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

        public override void PreUpdate()
        {
            if (studioHacks.Count == 0)
            {
                return;
            }

            _studioHack = null;
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
                    _studioHack = hack;
                    break;
                }
            }

            if (_studioHack == null && activeStudioHacks.Count > 0)
            {
                _studioHack = activeStudioHacks[0];
            }

            if (_studioHack != null)
            {
                if (_studioHack.isPoseEditing != _isPoseEditing)
                {
                    _isPoseEditing = _studioHack.isPoseEditing;

                    onPoseEditingChanged?.Invoke(_isPoseEditing);
                }
            }
        }

        public override void Update()
        {
            if (_studioHack != null)
            {
                _studioHack.Update();
            }
        }

        public override void OnChangedSceneLevel(Scene scene, LoadSceneMode sceneMode)
        {
            foreach (var studioHack in studioHacks)
            {
                studioHack.OnChangedSceneLevel(scene, sceneMode);
            }
        }
    }
}