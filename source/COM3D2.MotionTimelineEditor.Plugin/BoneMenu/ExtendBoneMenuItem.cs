namespace COM3D2.MotionTimelineEditor.Plugin
{
    public class ExtendBoneMenuItem : BoneMenuItem
    {
        public override bool isSelectedMenu
        {
            get
            {
                return base.isSelectedMenu;
            }
            set
            {
                base.isSelectedMenu = value;

                if (partsEditHack == null || maidCache == null)
                {
                    return;
                }

                var boneEntity = maidCache.extendBoneCache.GetEntity(name);
                if (boneEntity == null)
                {
                    return;
                }

                if (value)
                {
                    partsEditHack.targetSelectMode = 0;
                    partsEditHack.SetMaid(maidCache.maid);
                    partsEditHack.SetSlot(boneEntity.slotNo);

                    PluginUtils.ExecuteNextFrame(() =>
                    {
                        partsEditHack.SetBone(boneEntity.transform);
                    });
                }
                else
                {
                    partsEditHack.SetBone(null);
                }
            }
        }

        public ExtendBoneMenuItem(string name, string displayName) : base(name, displayName)
        {
        }
    }
}