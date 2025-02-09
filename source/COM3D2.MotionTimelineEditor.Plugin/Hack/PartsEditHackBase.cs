using UnityEngine;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    public enum BoneDisplay {
        None,
        Visible,
        Choisable,
    }

    public enum GizmoType {
        None,
        Position,
        Rotation,
        Scale,
    }

    public interface IPartsEditHack
    {
        BoneDisplay boneDisplay { get; set; }
        GizmoType gizmoType { get; set; }
        int targetSelectMode { get; set; }

        bool Init();
        bool GetYureAble(Maid maid, int slotNo);
        bool GetYureState(Maid maid, int slotNo);
        void SetYureState(Maid maid, int slotNo, bool state);
        void SetMaid(Maid maid);
        void SetSlot(int slotNo);
        void SetObject(GameObject obj);
        void SetBone(Transform bone);
    }

    public abstract class PartsEditHackBase : IPartsEditHack
    {
        public abstract BoneDisplay boneDisplay { get; set; }
        public abstract GizmoType gizmoType { get; set; }
        public abstract int targetSelectMode { get; set; }

        public abstract bool Init();

        public abstract bool GetYureAble(Maid maid, int slotNo);

        public abstract bool GetYureState(Maid maid, int slotNo);

        public abstract void SetYureState(Maid maid, int slotNo, bool state);

        public abstract void SetMaid(Maid maid);

        public abstract void SetSlot(int slotNo);

        public abstract void SetObject(GameObject obj);

        public abstract void SetBone(Transform bone);
    }
}