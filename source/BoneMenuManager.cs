using System.Collections.Generic;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    using MTE = MotionTimelineEditor;

    public class BoneMenuManager
    {
        private List<IBoneMenuItem> easyMenuItems = new List<IBoneMenuItem>();
        private List<IBoneMenuItem> allMenuItems = new List<IBoneMenuItem>();
        private List<IBoneMenuItem> allSetMenuItems = new List<IBoneMenuItem>();

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

        private static Config config
        {
            get
            {
                return MTE.config;
            }
        }

        private BoneMenuManager()
        {
        }

        public void Init()
        {
            easyMenuItems = new List<IBoneMenuItem>(1);
            allMenuItems = new List<IBoneMenuItem>(128);
            allSetMenuItems = new List<IBoneMenuItem>(9);

            easyMenuItems.Add(new EasyMenuItem());

            var menuItemsList = new List<BoneMenuItem>[9]
            {
                new List<BoneMenuItem>(8),
                new List<BoneMenuItem>(8),
                new List<BoneMenuItem>(8),
                new List<BoneMenuItem>(8),
                new List<BoneMenuItem>(8),
                new List<BoneMenuItem>(8),
                new List<BoneMenuItem>(8),
                new List<BoneMenuItem>(8),
                new List<BoneMenuItem>(8),
            };

            foreach (var pair in BoneUtils.boneTypeToSetMenuTypeMap)
            {
                var boneType = pair.Key;
                var boneSetType = pair.Value;
                var menuItem = new BoneMenuItem(boneType);

                menuItemsList[(int) boneSetType].Add(menuItem);
            }

            for (var i = 0; i < menuItemsList.Length; ++i)
            {
                var setMenuType = (BoneSetMenuType) i;
                var menuItems = menuItemsList[i];
                var menuSetItem = new BoneSetMenuItem(setMenuType, menuItems);
                allMenuItems.Add(menuSetItem);
                allSetMenuItems.Add(menuSetItem);

                foreach (var item in menuItems)
                {
                    allMenuItems.Add(item);
                }
            }

            allSetMenuItems[(int)BoneSetMenuType.LeftArmFinger].isOpenMenu = false;
            allSetMenuItems[(int)BoneSetMenuType.RightArmFinger].isOpenMenu = false;
            allSetMenuItems[(int)BoneSetMenuType.LeftLegFinger].isOpenMenu = false;
            allSetMenuItems[(int)BoneSetMenuType.RightLegFinger].isOpenMenu = false;
        }

        public void UnselectAll()
        {
            foreach (var setMenuItem in allSetMenuItems)
            {
                setMenuItem.isSelectedMenu = false;
            }
        }

        public List<IBoneMenuItem> GetVisibleItems()
        {
            if (config.isEasyEdit)
            {
                return easyMenuItems;
            }

            var visibleItems = new List<IBoneMenuItem>(allMenuItems.Count);
            foreach (var menuItem in allMenuItems)
            {
                if (menuItem.isVisibleMenu)
                {
                    visibleItems.Add(menuItem);
                }
            }
            return visibleItems;
        }

        public List<IBoneMenuItem> GetSelectedItems()
        {
            if (config.isEasyEdit)
            {
                return easyMenuItems;
            }

            var selectedItems = new List<IBoneMenuItem>(allMenuItems.Count);
            foreach (var menuItem in allMenuItems)
            {
                if (menuItem.isSelectedMenu)
                {
                    selectedItems.Add(menuItem);
                }
            }
            return selectedItems;
        }
    }
}