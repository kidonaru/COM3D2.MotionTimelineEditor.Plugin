using System;
using COM3D2.MotionTimelineEditor.Plugin;
using UnityEngine;

namespace COM3D2.MotionTimelineEditor_PartsEdit.Plugin
{

    public class PartsEditHack : PartsEditHackBase
    {
        private PartsEditField _field = new PartsEditField();

        public override BoneDisplay boneDisplay
        {
            get => (BoneDisplay)_field.boneDisplay.GetValue(null);
            set => _field.boneDisplay.SetValue(null, (int) value);
        }

        public override GizmoType gizmoType
        {
            get => (GizmoType)_field.gizmoType.GetValue(null);
            set => _field.gizmoType.SetValue(null, (int) value);
        }

        public override int targetSelectMode
        {
            get => (int)_field.targetSelectMode.GetValue(null);
            set => _field.targetSelectMode.SetValue(null, value);
        }

        public PartsEditHack()
        {
        }

        public override bool Init()
        {
            PluginUtils.Log("PartsEditHack: 初期化中...");

            try
            {
                if (!_field.Init())
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

        public override bool GetYureAble(Maid maid, int slotNo)
        {
            return (bool)_field.GetYureAble.Invoke(null, new object[] { maid, slotNo });
        }

        public override bool GetYureState(Maid maid, int slotNo)
        {
            return (bool)_field.GetYureState.Invoke(null, new object[] { maid, slotNo });
        }

        public override void SetYureState(Maid maid, int slotNo, bool state)
        {
            _field.SetYureState.Invoke(null, new object[] { maid, slotNo, state });
        }

        public override void SetMaid(Maid maid)
        {
            _field.SetMaid.Invoke(null, new object[] { maid });
        }

        public override void SetSlot(int slotNo)
        {
            _field.SetSlot.Invoke(null, new object[] { slotNo });
        }

        public override void SetObject(GameObject obj)
        {
            _field.SetObject.Invoke(null, new object[] { obj });
        }

        public override void SetBone(Transform bone)
        {
            _field.SetBone.Invoke(null, new object[] { bone });
        }
    }
}