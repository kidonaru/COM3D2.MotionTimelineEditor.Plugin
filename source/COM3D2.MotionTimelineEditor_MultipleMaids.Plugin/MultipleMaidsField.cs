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