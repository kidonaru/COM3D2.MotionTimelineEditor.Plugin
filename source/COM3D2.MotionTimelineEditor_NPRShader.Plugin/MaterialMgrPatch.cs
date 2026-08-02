using System.Reflection;
using COM3D2.MotionTimelineEditor;
using HarmonyLib;
using UnityEngine;

namespace COM3D2.MotionTimelineEditor_NPRShader.Plugin
{
    /// <summary>
    /// MaterialMgr.FixSkinMaskCutout の NullReferenceException を防ぐパッチ。
    ///
    /// COM3D2 v2.43.1 で追加された MaterialMgr は、スロットロード時の
    /// renderer.materials（インスタンスのコピー配列）を m_materials にキャッシュする。
    /// NPRShader はマテリアルを新規生成して丸ごと差し替え、旧マテリアルを破棄するため、
    /// m_materials[0] が破棄済み参照となり、null チェックのない本体処理が毎フレーム NRE を出し続ける。
    /// </summary>
    public static class MaterialMgrPatch
    {
        private const string HarmonyId = "COM3D2.MotionTimelineEditor_NPRShader.Plugin";

        // 毎フレーム呼ばれるホットパスのため FieldInfo をキャッシュする
        private static FieldInfo _materialsField;

        public static void Apply()
        {
            var materialMgrType = AccessTools.TypeByName("MaterialMgr");
            if (materialMgrType == null)
            {
                // MaterialMgr 未搭載のバージョンでは対象の不具合自体が存在しない
                return;
            }

            var targetMethod = AccessTools.Method(materialMgrType, "FixSkinMaskCutout");
            _materialsField = AccessTools.Field(materialMgrType, "m_materials");
            if (targetMethod == null || _materialsField == null)
            {
                MTEUtils.LogWarning("NPRShader: MaterialMgr のパッチ対象が見つかりませんでした");
                return;
            }

            var prefix = AccessTools.Method(typeof(MaterialMgrPatch), "FixSkinMaskCutoutPrefix");
            new Harmony(HarmonyId).Patch(targetMethod, new HarmonyMethod(prefix));

            MTEUtils.Log("NPRShader: MaterialMgr.FixSkinMaskCutout にパッチを適用しました");
        }

        /// <summary>
        /// m_materials が未初期化、または破棄済みなら本体処理をスキップする。
        /// 破棄済みマテリアルは Unity の擬似 null により null 比較で検出できる。
        /// </summary>
        private static bool FixSkinMaskCutoutPrefix(object __instance, ref bool __result)
        {
            var materials = (Material[])_materialsField.GetValue(__instance);
            if (materials == null || materials.Length == 0 || materials[0] == null)
            {
                __result = false;
                return false;
            }

            return true;
        }
    }
}
