using System.Collections.Generic;
using System.Linq;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    using MTE = MotionTimelineEditor;

    public class BoneSetMenuItem : IBoneMenuItem
    {
        public string name { get; private set; }
        public string displayName { get; private set; }

        public bool isSelectedMenu
        {
            get
            {
                return children.All(item => item.isSelectedMenu);
            }
            set
            {
                foreach (var menuItem in children)
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

                config.SetBoneSetMenuOpen(name, value);

                foreach (var menuItem in children)
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

        public List<IBoneMenuItem> children { get; private set; }

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

        private static Config config
        {
            get
            {
                return MTE.config;
            }
        }

        public BoneSetMenuItem(string name, string displayName)
        {
            this.name = name;
            this.displayName = displayName;
            this.children = new List<IBoneMenuItem>(8);
        }

        public void AddChild(BoneMenuItem menuItem)
        {
            children.Add(menuItem);
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

            return children.Any(item => item.HasBone(frame));
        }

        public bool HasFullBone(FrameData frame)
        {
            if (isOpenMenu)
            {
                return false;
            }

            return children.All(item => item.HasBone(frame));
        }

        public bool IsTargetBone(BoneData bone)
        {
            if (isOpenMenu)
            {
                return false;
            }

            return children.Any(item => item.IsTargetBone(bone));
        }

        public bool IsSelectedFrame(FrameData frame)
        {
            if (isOpenMenu)
            {
                return false;
            }

            return children.Any(item => item.IsSelectedFrame(frame));
        }

        public void SelectFrame(FrameData frame, bool isMultiSelect)
        {
            if (isOpenMenu)
            {
                return;
            }

            var bones = new List<BoneData>(children.Count);
            foreach (var menuItem in children)
            {
                var bone = frame.GetBone(menuItem.name);
                if (bone != null)
                {
                    bones.Add(bone);
                }
            }

            timelineManager.SelectBones(bones, isMultiSelect);
        }
    }
}