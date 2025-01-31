using System.Collections.Generic;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    public class BoneMenuManager : ManagerBase
    {
        private List<IBoneMenuItem> easyMenuItems = null;

        private static BoneMenuManager _instance = null;
        public static BoneMenuManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new BoneMenuManager();
                }
                return _instance;
            }
        }

        private List<IBoneMenuItem> allMenuItems => currentLayer.allMenuItems;

        private BoneMenuManager()
        {
        }

        public override void Init()
        {
            if (easyMenuItems == null)
            {
                easyMenuItems = new List<IBoneMenuItem>
                {
                    new EasyMenuItem()
                };
            }
        }

        public void UnselectAll()
        {
            foreach (var setMenuItem in allMenuItems)
            {
                setMenuItem.isSelectedMenu = false;
            }
        }

        private List<IBoneMenuItem> _visibleItems = new List<IBoneMenuItem>(128);

        public List<IBoneMenuItem> GetVisibleItems()
        {
            if (config.isEasyEdit)
            {
                return easyMenuItems;
            }

            _visibleItems.Clear();

            foreach (var setMenuItem in allMenuItems)
            {
                if (setMenuItem.isVisibleMenu)
                {
                    _visibleItems.Add(setMenuItem);
                }

                if (setMenuItem.children == null)
                {
                    continue;
                }

                foreach (var menuItem in setMenuItem.children)
                {
                    if (menuItem.isVisibleMenu)
                    {
                        _visibleItems.Add(menuItem);
                    }
                }
            }
            return _visibleItems;
        }

        private List<IBoneMenuItem> _selectedItems = new List<IBoneMenuItem>(128);

        public List<IBoneMenuItem> GetSelectedItems()
        {
            if (config.isEasyEdit)
            {
                return easyMenuItems;
            }

            _selectedItems.Clear();

            foreach (var setMenuItem in allMenuItems)
            {
                if (setMenuItem.isSelectedMenu)
                {
                    _selectedItems.Add(setMenuItem);
                }

                if (setMenuItem.children == null)
                {
                    continue;
                }

                foreach (var menuItem in setMenuItem.children)
                {
                    if (menuItem.isSelectedMenu)
                    {
                        _selectedItems.Add(menuItem);
                    }
                }
            }
            return _selectedItems;
        }
    }
}