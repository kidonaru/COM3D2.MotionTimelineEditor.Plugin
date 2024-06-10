using System.Collections.Generic;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    public class MaidBoneMenuItem : BoneMenuItem
    {
        public readonly IKManager.BoneType boneType;

        public override bool isSelectedMenu
        {
            get
            {
                if (!studioHack.HasBoneRotateVisible(boneType))
                {
                    return base.isSelectedMenu;
                }
                return studioHack.IsBoneRotateVisible(boneType);
            }
            set
            {
                if (!studioHack.HasBoneRotateVisible(boneType))
                {
                    base.isSelectedMenu = value;
                    return;
                }
                studioHack.SetBoneRotateVisible(boneType, value);
            }
        }

        public MaidBoneMenuItem(string name, string displayName) : base(name, displayName)
        {
            this.boneType = BoneUtils.GetBoneTypeByName(name);
        }
    }
}