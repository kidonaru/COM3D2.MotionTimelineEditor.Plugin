using System.Collections.Generic;
using System.Linq;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    public class BoneSetMenuItem : IBoneMenuItem
    {
        public readonly BoneSetMenuType setMenuType;
        public readonly List<BoneMenuItem> menuItems;

        public bool isSelectedMenu
        {
            get
            {
                return menuItems.All(item => item.isSelectedMenu);
            }
            set
            {
                foreach (var menuItem in menuItems)
                {
                    menuItem.isSelectedMenu = value;
                }
            }
        }

        private bool _isVisibleMenu = true;
        public bool isVisibleMenu
        {
            get
            {
                return _isVisibleMenu;
            }
            set
            {
                _isVisibleMenu = value;
            }
        }

        private bool _isOpenMenu = true;
        public bool isOpenMenu
        {
            get
            {
                return _isOpenMenu;
            }
            set
            {
                if (isOpenMenu == value)
                {
                    return;
                }
                _isOpenMenu = value;

                foreach (var menuItem in menuItems)
                {
                    menuItem.isVisibleMenu = value;
                }
            }
        }

        public bool isSetMenu
        {
            get
            {
                return true;
            }
        }

        public string diplayName
        {
            get
            {
                return BoneUtils.GetBoneSetMenuJpName(setMenuType);
            }
        }

        private static BoneMenuManager boneMenuManager
        {
            get
            {
                return BoneMenuManager.Instance;
            }
        }

        private static TimelineManager timelineManager
        {
            get
            {
                return TimelineManager.instance;
            }
        }

        public BoneSetMenuItem(
            BoneSetMenuType setMenuType,
            List<BoneMenuItem> menuItems)
        {
            this.setMenuType = setMenuType;
            this.menuItems = menuItems;
        }

        public void SelectMenu(bool isMultiSelect)
        {
            var prevSelected = isSelectedMenu;

            if (!isMultiSelect)
            {
                boneMenuManager.UnselectAll();
            }

            isSelectedMenu = !prevSelected;
        }

        public bool HasBone(FrameData frame)
        {
            if (isOpenMenu)
            {
                return false;
            }

            return menuItems.Any(item => item.HasBone(frame));
        }

        public bool HasFullBone(FrameData frame)
        {
            if (isOpenMenu)
            {
                return false;
            }

            return menuItems.All(item => item.HasBone(frame));
        }

        public bool IsTargetBone(BoneData bone)
        {
            if (isOpenMenu)
            {
                return false;
            }

            return menuItems.Any(item => item.IsTargetBone(bone));
        }

        public bool IsSelectedFrame(FrameData frame)
        {
            if (isOpenMenu)
            {
                return false;
            }

            return menuItems.Any(item => item.IsSelectedFrame(frame));
        }

        public void SelectFrame(FrameData frame, bool isMultiSelect)
        {
            if (isOpenMenu)
            {
                return;
            }

            var bones = new List<BoneData>(menuItems.Count);
            foreach (var menuItem in menuItems)
            {
                var bone = frame.GetBone(menuItem.bonePath);
                if (bone != null)
                {
                    bones.Add(bone);
                }
            }

            timelineManager.SelectBones(frame, bones, isMultiSelect);
        }
    }
}