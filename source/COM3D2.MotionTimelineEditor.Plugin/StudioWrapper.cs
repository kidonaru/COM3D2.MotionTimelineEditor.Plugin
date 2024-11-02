using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    public class StudioWrapper
    {
        public bool active = false;

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

        private FieldInfo _fieldDataDic = null;
        private FieldInfo _fieldBoneDic = null;
        private FieldInfo _fieldIkboxVisibleDic = null;

        public WFCheckBox ikBoxVisibleRoot = null;
        public WFCheckBox ikBoxVisibleBody = null;

        public CreateBGObjectSubWindow createBgObjectWindow
        {
            get
            {
                return objectManagerWindow.createBgObjectWindow;
            }
        }

        public WindowPartsDirectionalLight directionalLightWindow
        {
            get
            {
                return lightWindow.DirectionalLightWindow;
            }
        }

        private MethodInfo _methodSelectMaid = null;
        public void SetSelectMaid(Maid maid)
        {
            if (_methodSelectMaid == null)
            {
                _methodSelectMaid = typeof(PlacementWindow).GetMethod("SetSelectMaid", BindingFlags.Instance | BindingFlags.InvokeMethod | BindingFlags.NonPublic);
                PluginUtils.AssertNull(_methodSelectMaid != null, "methodSelectMaid is null");
            }

            _methodSelectMaid.Invoke(placementWindow, new object[] { maid });
        }

        private MethodInfo _methodAddObject = null;
        public void AddObject(PhotoBGObjectData add_bg_data, string create_time)
        {
            if (_methodAddObject == null)
            {
                _methodAddObject = typeof(CreateBGObjectSubWindow).GetMethod("AddObject", BindingFlags.Instance | BindingFlags.InvokeMethod | BindingFlags.NonPublic);
                PluginUtils.AssertNull(_methodAddObject != null, "methodAddObject is null");
            }

            _methodAddObject.Invoke(createBgObjectWindow, new object[] { add_bg_data, create_time });
        }

        private MethodInfo _methodInstantiateLight = null;
        public GameObject InstantiateLight()
        {
            if (_methodInstantiateLight == null)
            {
                _methodInstantiateLight = typeof(LightWindow).GetMethod("InstantiateLight", BindingFlags.Instance | BindingFlags.InvokeMethod | BindingFlags.NonPublic);
                PluginUtils.AssertNull(_methodInstantiateLight != null, "methodInstantiateLight is null");
            }

            return (GameObject) _methodInstantiateLight.Invoke(lightWindow, null);
        }

        public bool Init()
        {
            BindingFlags bindingAttr = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.InvokeMethod;

            _fieldDataDic = typeof(WindowPartsBoneCheckBox).GetField("data_dic_", bindingAttr);
            PluginUtils.AssertNull(_fieldDataDic != null, "fieldDataDic is null");
            if (_fieldDataDic == null) return false;

            _fieldBoneDic = typeof(IKManager).GetField("bone_dic_", bindingAttr);
            PluginUtils.AssertNull(_fieldBoneDic != null, "fieldBoneDic is null");
            if (_fieldBoneDic == null) return false;

            _fieldIkboxVisibleDic = typeof(PoseEditWindow).GetField("ikbox_visible_dic_", bindingAttr);
            PluginUtils.AssertNull(_fieldIkboxVisibleDic != null, "fieldIkboxVisibleDic is null");
            if (_fieldIkboxVisibleDic == null) return false;

            return true;
        }

        public void OnSceneActive()
        {
            active = false;

            {
                var gameObject = GameObject.Find("PoseEditWindow");
                poseEditWindow = gameObject.GetComponent<PoseEditWindow>();
                PluginUtils.AssertNull(poseEditWindow != null, "poseEditWindow is null");
                if (poseEditWindow == null) return;
            }

            {
                var gameObject = GameObject.Find("PlacementWindow");
                placementWindow = gameObject.GetComponent<PlacementWindow>();
                PluginUtils.AssertNull(placementWindow != null, "placementWindow is null");
                if (placementWindow == null) return;
            }

            {
                var gameObject = GameObject.Find("MotionWindow");
                motionWindow = gameObject.GetComponent<MotionWindow>();
                PluginUtils.AssertNull(motionWindow != null, "motionWindow is null");
                if (motionWindow == null) return;
            }
            
            {
                var gameObject = GameObject.Find("ObjectManagerWindow");
                objectManagerWindow = gameObject.GetComponent<ObjectManagerWindow>();
                PluginUtils.AssertNull(objectManagerWindow != null, "objectManagerWindow is null");
                if (objectManagerWindow == null) return;
            }

            {
                var gameObject = GameObject.Find("LightWindow");
                lightWindow = gameObject.GetComponent<LightWindow>();
                PluginUtils.AssertNull(lightWindow != null, "lightWindow is null");
                if (lightWindow == null) return;
            }

            {
                var gameObject = GameObject.Find("BGWindow");
                bgWindow = gameObject.GetComponent<BGWindow>();
                PluginUtils.AssertNull(bgWindow != null, "bgWindow is null");
                if (bgWindow == null) return;
            }

            photoManager = poseEditWindow.mgr;
            PluginUtils.AssertNull(photoManager != null, "photoManager is null");
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

                var dataDic = (Dictionary<IKManager.BoneType, WFCheckBox>) _fieldDataDic.GetValue(boneCheckBox);
                foreach (var pair in dataDic)
                {
                    boneCheckBoxMap[pair.Key] = pair.Value;
                }
            }

            PluginUtils.AssertNull(bodyBoneCheckBox != null, "bodyBoneCheckBox is null");
            if (bodyBoneCheckBox == null) return;

            {
                var ikboxVisibleDic = (Dictionary<string, WFCheckBox>) _fieldIkboxVisibleDic.GetValue(poseEditWindow);
                PluginUtils.AssertNull(ikboxVisibleDic != null, "ikboxVisibleDic is null");
                if (ikboxVisibleDic == null) return;

                ikBoxVisibleRoot = ikboxVisibleDic["ik_box_visible_Root"];
                PluginUtils.AssertNull(ikBoxVisibleRoot != null, "ikBoxVisibleRoot is null");
                if (ikBoxVisibleRoot == null) return;

                ikBoxVisibleBody = ikboxVisibleDic["ik_box_visible_Body"];
                PluginUtils.AssertNull(ikBoxVisibleBody != null, "ikBoxVisibleBody is null");
                if (ikBoxVisibleBody == null) return;
            }

            active = true;
        }
    }
}