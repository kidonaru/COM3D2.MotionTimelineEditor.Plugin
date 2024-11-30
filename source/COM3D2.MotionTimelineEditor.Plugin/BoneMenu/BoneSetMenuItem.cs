using System.Collections.Generic;
using System.Linq;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    public class BoneSetMenuItem : IBoneMenuItem
    {
        public string name { get; private set; }
        public string displayName { get; private set; }

        public bool isSelectedMenu
        {
            get => children.All(item => item.isSelectedMenu);
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
            get => _isVisibleMenu;
            set => _isVisibleMenu = value;
        }

        private bool _isOpenMenu = true;
        public bool isOpenMenu
        {
            get => _isOpenMenu;
            set
            {
                if (isOpenMenu == value)
                {
                    return;
                }
                _isOpenMenu = value;

                config.SetBoneSetMenuOpen(name, value);
            }
        }

        public bool isSetMenu => true;

        public IBoneMenuItem parent { get; set; }

        public List<IBoneMenuItem> children { get; private set; }

        private static BoneMenuManager boneMenuManager => BoneMenuManager.Instance;

        private static TimelineManager timelineManager => TimelineManager.instance;

        protected static ITimelineLayer currentLayer => timelineManager.currentLayer;

        private static Config config => ConfigManager.config;

        public BoneSetMenuItem(string name, string displayName)
        {
            this.name = name;
            this.displayName = displayName;
            this.children = new List<IBoneMenuItem>(8);
            this._isOpenMenu = config.IsBoneSetMenuOpen(name);
        }

        public void AddChild(BoneMenuItem menuItem)
        {
            menuItem.parent = this;
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

        public bool HasVisibleBone(FrameData frame)
        {
            if (isOpenMenu)
            {
                return false;
            }

            return children.Any(item => item.HasVisibleBone(frame));
        }

        public bool IsFullBones(FrameData frame)
        {
            return children.All(item => item.HasVisibleBone(frame));
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

        public void AddKey()
        {
            var boneNames = children.Select(item => item.name);
            currentLayer.AddKeyFrames(boneNames);
        }

        public void RemoveKey()
        {
            var boneNames = children.Select(item => item.name);
            currentLayer.RemoveKeyFrames(boneNames);
        }
    }
}