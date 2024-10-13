using System.Reflection;
using CM3D2.MultipleMaids.Plugin;
using COM3D2.MotionTimelineEditor.Plugin;

namespace COM3D2.MotionTimelineEditor_MultipleMaids.Plugin
{
    public class MultipleMaidsField
    {
        public FieldInfo maidArray;
        public FieldInfo selectMaidIndex;
        public FieldInfo isIK;
        public FieldInfo isLock;
        public FieldInfo unLockFlg;
        public FieldInfo isStop;
        public FieldInfo isBone;
        public FieldInfo okFlg;
        public FieldInfo doguBObject;
        public FieldInfo doguCnt;
        public FieldInfo mDogu;
        public FieldInfo gDogu;
        public FieldInfo m_material;
        public FieldInfo cubeSize;
        public FieldInfo lightList;
        public FieldInfo lightIndex;
        public FieldInfo lightColorR;
        public FieldInfo lightColorG;
        public FieldInfo lightColorB;
        public FieldInfo lightX;
        public FieldInfo lightY;
        public FieldInfo lightAkarusa;
        public FieldInfo lightKage;
        public FieldInfo lightRange;
        public FieldInfo selectLightIndex;
        public FieldInfo mLight;
        public FieldInfo gLight;
        public FieldInfo lightCombo;
        public FieldInfo lightComboList;
        
        public FieldInfo isWear;
        public FieldInfo isSkirt;
        public FieldInfo isBra;
        public FieldInfo isPanz;
        public FieldInfo isMaid;
        public FieldInfo isMekure1;
        public FieldInfo isMekure2;
        public FieldInfo isZurasi;
        public FieldInfo isMekure1a;
        public FieldInfo isMekure2a;
        public FieldInfo isZurasia;
        public FieldInfo isHeadset;
        public FieldInfo isAccUde;
        public FieldInfo isStkg;
        public FieldInfo isShoes;
        public FieldInfo isGlove;
        public FieldInfo isMegane;
        public FieldInfo isAccSenaka;
        public FieldInfo mekure1;
        public FieldInfo mekure2;
        public FieldInfo zurasi;

        public bool Init()
        {
            var bindingAttr = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.InvokeMethod;
            var multipleMaidsType = typeof(MultipleMaids);

            foreach (var fieldInfo in typeof(MultipleMaidsField).GetFields())
            {
                string fieldName = fieldInfo.Name;
                var targetField = multipleMaidsType.GetField(fieldName, bindingAttr);
                PluginUtils.AssertNull(targetField != null, "field " + fieldName + " is null");
                fieldInfo.SetValue(this, targetField);

                if (targetField == null)
                {
                    return false;
                }
            }

            return true;
        }
    }
}