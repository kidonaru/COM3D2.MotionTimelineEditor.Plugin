using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace COM3D2.MotionTimelineEditor.Plugin
{

    public class StudioWrapper
    {
        public bool active = false;

        private StudioField _field = new StudioField();
        private BgObjectField _bgObjectfield = new BgObjectField();

        public PoseEditWindow poseEditWindow = null;
        public PlacementWindow placementWindow = null;
        public MotionWindow motionWindow = null;
        public ObjectManagerWindow objectManagerWindow = null;
        public LightWindow lightWindow = null;
        public BGWindow bgWindow = null;
        public WindowPartsEffectDepthBlur effectDepthBlur = null;
        public PhotoWindowManager photoManager = null;
        public WindowPartsBoneCheckBox bodyBoneCheckBox = null;
        public Dictionary<IKManager.BoneType, WFCheckBox> boneCheckBoxMap = new Dictionary<IKManager.BoneType, WFCheckBox>(78);

        public WFCheckBox ikBoxVisibleRoot = null;
        public WFCheckBox ikBoxVisibleBody = null;

        public CreateBGObjectSubWindow createBgObjectWindow
        {
            get => objectManagerWindow.createBgObjectWindow;
        }

        public WindowPartsDirectionalLight directionalLightWindow
        {
            get => lightWindow.DirectionalLightWindow;
        }

        public void SetSelectMaid(Maid maid)
        {
            _field.SetSelectMaid.Invoke(placementWindow, new object[] { maid });
        }

        public BgObjectWrapper AddObject(PhotoBGObjectData add_bg_data, string create_time)
        {
            var obj = _field.AddObject.Invoke(createBgObjectWindow, new object[] { add_bg_data, create_time });
            var wrapper = _bgObjectfield.ConvertToWrapper(obj);
            return wrapper;
        }

        public GameObject InstantiateLight()
        {
            return (GameObject) _field.InstantiateLight.Invoke(lightWindow, null);
        }

        private Dictionary<string, BgObjectWrapper> _bgObjectMap = new Dictionary<string, BgObjectWrapper>(16);

        public Dictionary<string, BgObjectWrapper> GetBgObjectMap()
        {
            _bgObjectMap.Clear();

            var objList = (IList) _field.create_obj_list_.GetValue(createBgObjectWindow);
            foreach (var obj in objList)
            {
                var wrapper = _bgObjectfield.ConvertToWrapper(obj);
                _bgObjectMap[wrapper.create_time] = wrapper;
            }

            return _bgObjectMap;
        }

        public bool Init()
        {
            if (!_field.Init())
            {
                return false;
            }

            if (!_bgObjectfield.Init())
            {
                return false;
            }

            return true;
        }

        public void OnSceneActive()
        {
            active = false;

            {
                var gameObject = GameObject.Find("PoseEditWindow");
                poseEditWindow = gameObject.GetComponent<PoseEditWindow>();
                MTEUtils.AssertNull(poseEditWindow != null, "poseEditWindow is null");
                if (poseEditWindow == null) return;
            }

            {
                var gameObject = GameObject.Find("PlacementWindow");
                placementWindow = gameObject.GetComponent<PlacementWindow>();
                MTEUtils.AssertNull(placementWindow != null, "placementWindow is null");
                if (placementWindow == null) return;
            }

            {
                var gameObject = GameObject.Find("MotionWindow");
                motionWindow = gameObject.GetComponent<MotionWindow>();
                MTEUtils.AssertNull(motionWindow != null, "motionWindow is null");
                if (motionWindow == null) return;
            }
            
            {
                var gameObject = GameObject.Find("ObjectManagerWindow");
                objectManagerWindow = gameObject.GetComponent<ObjectManagerWindow>();
                MTEUtils.AssertNull(objectManagerWindow != null, "objectManagerWindow is null");
                if (objectManagerWindow == null) return;
            }

            {
                var gameObject = GameObject.Find("LightWindow");
                lightWindow = gameObject.GetComponent<LightWindow>();
                MTEUtils.AssertNull(lightWindow != null, "lightWindow is null");
                if (lightWindow == null) return;
            }

            {
                var gameObject = GameObject.Find("BGWindow");
                bgWindow = gameObject.GetComponent<BGWindow>();
                MTEUtils.AssertNull(bgWindow != null, "bgWindow is null");
                if (bgWindow == null) return;
            }

            photoManager = poseEditWindow.mgr;
            MTEUtils.AssertNull(photoManager != null, "photoManager is null");
            if (photoManager == null) return;

            foreach (var boneCheckBox in poseEditWindow.RotateCheckBoxArray)
            {
                if (boneCheckBox == null)
                {
                    continue;
                }

                if (boneCheckBox.BoneSetType == IKManager.BoneSetType.Body)
                {
                    bodyBoneCheckBox = boneCheckBox;
                }

                var dataDic = (Dictionary<IKManager.BoneType, WFCheckBox>) _field.data_dic_.GetValue(boneCheckBox);
                foreach (var pair in dataDic)
                {
                    boneCheckBoxMap[pair.Key] = pair.Value;
                }
            }

            MTEUtils.AssertNull(bodyBoneCheckBox != null, "bodyBoneCheckBox is null");
            if (bodyBoneCheckBox == null) return;

            {
                var ikboxVisibleDic = (Dictionary<string, WFCheckBox>) _field.ikbox_visible_dic_.GetValue(poseEditWindow);
                MTEUtils.AssertNull(ikboxVisibleDic != null, "ikboxVisibleDic is null");
                if (ikboxVisibleDic == null) return;

                ikBoxVisibleRoot = ikboxVisibleDic["ik_box_visible_Root"];
                MTEUtils.AssertNull(ikBoxVisibleRoot != null, "ikBoxVisibleRoot is null");
                if (ikBoxVisibleRoot == null) return;

                ikBoxVisibleBody = ikboxVisibleDic["ik_box_visible_Body"];
                MTEUtils.AssertNull(ikBoxVisibleBody != null, "ikBoxVisibleBody is null");
                if (ikBoxVisibleBody == null) return;
            }

            active = true;
        }
    }
}