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

        public Camera subcamera
        {
            get
            {
                return (Camera)field.subcamera.GetValue(multipleMaids);
            }
        }

        public bool isWear
        {
            get { return (bool)field.isWear.GetValue(multipleMaids); }
            set { field.isWear.SetValue(multipleMaids, value); }
        }

        public bool isSkirt
        {
            get { return (bool)field.isSkirt.GetValue(multipleMaids); }
            set { field.isSkirt.SetValue(multipleMaids, value); }
        }

        public bool isBra
        {
            get { return (bool)field.isBra.GetValue(multipleMaids); }
            set { field.isBra.SetValue(multipleMaids, value); }
        }

        public bool isPanz
        {
            get { return (bool)field.isPanz.GetValue(multipleMaids); }
            set { field.isPanz.SetValue(multipleMaids, value); }
        }

        public bool isMaid
        {
            get { return (bool)field.isMaid.GetValue(multipleMaids); }
            set { field.isMaid.SetValue(multipleMaids, value); }
        }

        public bool isMekure1
        {
            get { return (bool)field.isMekure1.GetValue(multipleMaids); }
            set { field.isMekure1.SetValue(multipleMaids, value); }
        }

        public bool isMekure2
        {
            get { return (bool)field.isMekure2.GetValue(multipleMaids); }
            set { field.isMekure2.SetValue(multipleMaids, value); }
        }

        public bool isZurasi
        {
            get { return (bool)field.isZurasi.GetValue(multipleMaids); }
            set { field.isZurasi.SetValue(multipleMaids, value); }
        }

        public bool isMekure1a
        {
            get { return (bool)field.isMekure1a.GetValue(multipleMaids); }
            set { field.isMekure1a.SetValue(multipleMaids, value); }
        }

        public bool isMekure2a
        {
            get { return (bool)field.isMekure2a.GetValue(multipleMaids); }
            set { field.isMekure2a.SetValue(multipleMaids, value); }
        }

        public bool isZurasia
        {
            get { return (bool)field.isZurasia.GetValue(multipleMaids); }
            set { field.isZurasia.SetValue(multipleMaids, value); }
        }

        public bool isHeadset
        {
            get { return (bool)field.isHeadset.GetValue(multipleMaids); }
            set { field.isHeadset.SetValue(multipleMaids, value); }
        }

        public bool isAccUde
        {
            get { return (bool)field.isAccUde.GetValue(multipleMaids); }
            set { field.isAccUde.SetValue(multipleMaids, value); }
        }

        public bool isStkg
        {
            get { return (bool)field.isStkg.GetValue(multipleMaids); }
            set { field.isStkg.SetValue(multipleMaids, value); }
        }

        public bool isShoes
        {
            get { return (bool)field.isShoes.GetValue(multipleMaids); }
            set { field.isShoes.SetValue(multipleMaids, value); }
        }

        public bool isGlove
        {
            get { return (bool)field.isGlove.GetValue(multipleMaids); }
            set { field.isGlove.SetValue(multipleMaids, value); }
        }

        public bool isMegane
        {
            get { return (bool)field.isMegane.GetValue(multipleMaids); }
            set { field.isMegane.SetValue(multipleMaids, value); }
        }

        public bool isAccSenaka
        {
            get { return (bool)field.isAccSenaka.GetValue(multipleMaids); }
            set { field.isAccSenaka.SetValue(multipleMaids, value); }
        }

        public bool[] mekure1
        {
            get { return (bool[])field.mekure1.GetValue(multipleMaids); }
            set { field.mekure1.SetValue(multipleMaids, value); }
        }

        public bool[] mekure2
        {
            get { return (bool[])field.mekure2.GetValue(multipleMaids); }
            set { field.mekure2.SetValue(multipleMaids, value); }
        }

        public bool[] zurasi
        {
            get { return (bool[])field.zurasi.GetValue(multipleMaids); }
            set { field.zurasi.SetValue(multipleMaids, value); }
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