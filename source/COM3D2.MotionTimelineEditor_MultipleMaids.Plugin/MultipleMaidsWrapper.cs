using CM3D2.MultipleMaids.Plugin;
using UnityEngine;
using COM3D2.MotionTimelineEditor.Plugin;
using System.Collections.Generic;

namespace COM3D2.MotionTimelineEditor_MultipleMaids.Plugin
{
    public class MultipleMaidsWrapper
    {
        private MultipleMaids multipleMaids = null;
        private MultipleMaidsField field = new MultipleMaidsField();

        public Maid[] maidArray
        {
            get
            {
                return (Maid[])field.maidArray.GetValue(multipleMaids);
            }
        }

        public int selectMaidIndex
        {
            get
            {
                return (int)field.selectMaidIndex.GetValue(multipleMaids);
            }
            set
            {
                field.selectMaidIndex.SetValue(multipleMaids, value);
            }
        }

        public bool[] isIK
        {
            get
            {
                return (bool[])field.isIK.GetValue(multipleMaids);
            }
        }

        public bool[] isLock
        {
            get
            {
                return (bool[])field.isLock.GetValue(multipleMaids);
            }
        }

        public bool unLockFlg
        {
            get
            {
                return (bool)field.unLockFlg.GetValue(multipleMaids);
            }
            set
            {
                field.unLockFlg.SetValue(multipleMaids, value);
            }
        }

        public bool[] isStop
        {
            get
            {
                return (bool[])field.isStop.GetValue(multipleMaids);
            }
        }

        public bool[] isBone
        {
            get
            {
                return (bool[])field.isBone.GetValue(multipleMaids);
            }
        }

        public bool okFlg
        {
            get
            {
                return (bool)field.okFlg.GetValue(multipleMaids);
            }
        }

        public List<GameObject> doguBObject
        {
            get
            {
                return (List<GameObject>)field.doguBObject.GetValue(multipleMaids);
            }
        }

        public int doguCnt
        {
            get
            {
                return (int)field.doguCnt.GetValue(multipleMaids);
            }
            set
            {
                field.doguCnt.SetValue(multipleMaids, value);
            }
        }

        public MouseDrag6[] mDogu
        {
            get
            {
                return (MouseDrag6[])field.mDogu.GetValue(multipleMaids);
            }
        }

        public GameObject[] gDogu
        {
            get
            {
                return (GameObject[])field.gDogu.GetValue(multipleMaids);
            }
        }

        public Material m_material
        {
            get
            {
                return (Material)field.m_material.GetValue(multipleMaids);
            }
        }

        public float cubeSize
        {
            get
            {
                return (float)field.cubeSize.GetValue(multipleMaids);
            }
        }

        public List<GameObject> lightList
        {
            get
            {
                return (List<GameObject>)field.lightList.GetValue(multipleMaids);
            }
        }

        public List<int> lightIndex
        {
            get
            {
                return (List<int>)field.lightIndex.GetValue(multipleMaids);
            }
        }

        public List<float> lightColorR
        {
            get
            {
                return (List<float>)field.lightColorR.GetValue(multipleMaids);
            }
        }

        public List<float> lightColorG
        {
            get
            {
                return (List<float>)field.lightColorG.GetValue(multipleMaids);
            }
        }

        public List<float> lightColorB
        {
            get
            {
                return (List<float>)field.lightColorB.GetValue(multipleMaids);
            }
        }

        public List<float> lightX
        {
            get
            {
                return (List<float>)field.lightX.GetValue(multipleMaids);
            }
        }

        public List<float> lightY
        {
            get
            {
                return (List<float>)field.lightY.GetValue(multipleMaids);
            }
        }

        public List<float> lightAkarusa
        {
            get
            {
                return (List<float>)field.lightAkarusa.GetValue(multipleMaids);
            }
        }

        public List<float> lightKage
        {
            get
            {
                return (List<float>)field.lightKage.GetValue(multipleMaids);
            }
        }

        public List<float> lightRange
        {
            get
            {
                return (List<float>)field.lightRange.GetValue(multipleMaids);
            }
        }

        public int selectLightIndex
        {
            get
            {
                return (int)field.selectLightIndex.GetValue(multipleMaids);
            }
            set
            {
                field.selectLightIndex.SetValue(multipleMaids, value);
            }
        }

        public MouseDrag6[] mLight
        {
            get
            {
                return (MouseDrag6[])field.mLight.GetValue(multipleMaids);
            }
        }

        public GameObject[] gLight
        {
            get
            {
                return (GameObject[])field.gLight.GetValue(multipleMaids);
            }
        }

        public ComboBox2 lightCombo
        {
            get
            {
                return (ComboBox2)field.lightCombo.GetValue(multipleMaids);
            }
        }

        public GUIContent[] lightComboList
        {
            get
            {
                return (GUIContent[])field.lightComboList.GetValue(multipleMaids);
            }
            set
            {
                field.lightComboList.SetValue(multipleMaids, value);
            }
        }

        public bool Init()
        {
            {
                GameObject gameObject = GameObject.Find("UnityInjector");
                multipleMaids = gameObject.GetComponent<MultipleMaids>();
                PluginUtils.AssertNull(multipleMaids != null, "multipleMaids is null");
            }

            if (multipleMaids == null)
            {
                PluginUtils.LogError("複数メイドプラグインが見つかりませんでした");
                return false;
            }

            if (!field.Init())
            {
                return false;
            }

            return true;
        }
    }
}