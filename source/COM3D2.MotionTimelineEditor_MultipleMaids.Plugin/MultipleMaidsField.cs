using System.Reflection;
using CM3D2.MultipleMaids.Plugin;
using COM3D2.MotionTimelineEditor;

namespace COM3D2.MotionTimelineEditor_MultipleMaids.Plugin
{
    public class MultipleMaidsField : CustomFieldBase
    {
        public override System.Type assemblyType { get; set; } = typeof(MultipleMaids);
        public override System.Type defaultParentType { get; set; } = typeof(MultipleMaids);

        public FieldInfo maidArray;
        public FieldInfo selectMaidIndex;
        public FieldInfo bGui;
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
        public FieldInfo subcamera;
        public FieldInfo depth_field_;
        
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
        public FieldInfo isDepth;
        public FieldInfo isDepthA;
        public FieldInfo depth1;
        public FieldInfo depth2;
        public FieldInfo depth3;
        public FieldInfo depth4;

        public FieldInfo mHandL;
        public FieldInfo mHandR;
        public FieldInfo mArmL;
        public FieldInfo mArmR;
        public FieldInfo mFootL;
        public FieldInfo mFootR;
        public FieldInfo mHizaL;
        public FieldInfo mHizaR;
    }
}