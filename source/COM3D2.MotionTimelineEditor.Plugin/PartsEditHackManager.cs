using System.Collections.Generic;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    public interface IPartsEditHack
    {
        bool Init();
        bool GetYureAble(Maid maid, int slotNo);
        bool GetYureState(Maid maid, int slotNo);
        void SetYureState(Maid maid, int slotNo, bool state);
    }

    public abstract class PartsEditHackBase : IPartsEditHack
    {
        public abstract bool Init();

        public abstract bool GetYureAble(Maid maid, int slotNo);

        public abstract bool GetYureState(Maid maid, int slotNo);

        public abstract void SetYureState(Maid maid, int slotNo, bool state);
    }

    public class PartsEditHackManager
    {
        private IPartsEditHack partsEditHack = null;

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
    }
}