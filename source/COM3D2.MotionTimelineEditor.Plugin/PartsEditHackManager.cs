using System.Collections.Generic;
using UnityEngine.SceneManagement;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    public class PartsEditHackManager
    {
        public IPartsEditHack partsEditHack = null;

        private static PartsEditHackManager _instance;
        public static PartsEditHackManager instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new PartsEditHackManager();
                }

                return _instance;
            }
        }

        private PartsEditHackManager()
        {
            SceneManager.sceneLoaded += OnChangedSceneLevel;
            StudioHackManager.onPoseEditingChanged += OnPoseEditingChanged;
        }

        public void Register(IPartsEditHack partsEditHack)
        {
            if (partsEditHack == null || !partsEditHack.Init())
            {
                return;
            }

            this.partsEditHack = partsEditHack;
        }

        private int GetSlotNo(Maid maid, string slotName)
        {
            if (maid.body0 == null || maid.body0.goSlot == null)
            {
                return -1;
            }

            var count = maid.body0.goSlot.Count;
            for (int i = 0; i < count; i++)
            {
                var slot = maid.body0.goSlot[i];
                if (slot != null && slot.Category == slotName)
                {
                    return i;
                }
            }

            return -1;
        }

        public bool GetYureAble(Maid maid, string slotName)
        {
            if (partsEditHack == null)
            {
                return false;
            }

            var slotNo = GetSlotNo(maid, slotName);
            if (slotNo == -1)
            {
                return false;
            }

            return partsEditHack.GetYureAble(maid, slotNo);
        }

        public bool GetYureState(Maid maid, string slotName)
        {
            if (partsEditHack == null)
            {
                return false;
            }

            var slotNo = GetSlotNo(maid, slotName);
            if (slotNo == -1)
            {
                return false;
            }

            return partsEditHack.GetYureState(maid, slotNo);
        }

        public void SetYureState(Maid maid, string slotName, bool state)
        {
            if (partsEditHack == null)
            {
                return;
            }

            var slotNo = GetSlotNo(maid, slotName);
            if (slotNo == -1)
            {
                return;
            }

            partsEditHack.SetYureState(maid, slotNo, state);
        }

        private GizmoType _savedGizmoType = GizmoType.Rotation;

        private void OnPoseEditingChanged(bool isPoseEditing)
        {
            if (partsEditHack == null)
            {
                return;
            }

            if (isPoseEditing)
            {
                partsEditHack.boneDisplay = BoneDisplay.Visible;
                partsEditHack.gizmoType = _savedGizmoType;
            }
            else
            {
                _savedGizmoType = partsEditHack.gizmoType;

                partsEditHack.boneDisplay = BoneDisplay.None;
                partsEditHack.gizmoType = GizmoType.None;
            }
        }

        private void OnChangedSceneLevel(Scene sceneName, LoadSceneMode sceneMode)
        {
            if (partsEditHack == null)
            {
                return;
            }

            partsEditHack.SetBone(null);
        }
    }
}