using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using COM3D2.MotionTimelineEditor.Plugin;
using COM3D2.NPRShader.Plugin;
using System.Text.RegularExpressions;
using System;
using COM3D2.MotionTimelineEditor;

namespace COM3D2.MotionTimelineEditor_NPRShader.Plugin
{
    public class NPRShaderHack : INPRShaderHack
    {
        public NPRShaderWrapper wrapper = new NPRShaderWrapper();

        public bool Init()
        {
            if (!wrapper.Init())
            {
                return false;
            }

            return true;
        }

        public void Reload()
        {
            wrapper.Reload();
        }

        public void UpdateMaterial(GameObject gameObject, string menuFileName)
        {
            try
            {
                if (gameObject == null || string.IsNullOrEmpty(menuFileName))
                {
                    return;
                }

                if (!menuFileName.EndsWith(".menu", StringComparison.OrdinalIgnoreCase))
                {
                    menuFileName += ".menu";
                }

                if (!GameUty.IsExistFile(menuFileName))
                {
                    return;
                }

                PBRModelInfo pbrModelInfo = AssetLoader.LoadMenuPBR(menuFileName);

                if (string.IsNullOrEmpty(pbrModelInfo.modelName))
                {
                    return;
                }

                ApplyPBRModelToGameObject(gameObject, pbrModelInfo);
            }
            catch (Exception ex)
            {
                MTEUtils.LogException(ex);
            }
        }

        private void ApplyPBRModelToGameObject(GameObject gameObject, PBRModelInfo pbrModelInfo)
        {
            foreach (ModelInfo modelInfo in pbrModelInfo.models)
            {
                try
                {
                    int slotIndex = (int)TBody.hashSlotName[modelInfo.slotName];
                    SkinnedMeshRenderer[] renderers = gameObject.transform.GetComponentsInChildren<SkinnedMeshRenderer>(true);
                    
                    foreach (SkinnedMeshRenderer renderer in renderers)
                    {
                        if (renderer == null) continue;
                        
                        try
                        {
                            // 新しいマテリアル配列を作成
                            Material[] baseMaterials = renderer.sharedMaterials;
                            Material[] newMaterials = new Material[baseMaterials.Length];
                            for (int i = 0; i < newMaterials.Length; i++)
                            {
                                newMaterials[i] = new Material(baseMaterials[i]);
                            }

                            // マテリアル変更を適用
                            ApplyMaterialChanges(newMaterials, pbrModelInfo);
                            
                            // テクスチャ変更を適用
                            foreach (TextureChangeInfo textureChange in pbrModelInfo.textureChanges)
                            {
                                newMaterials[textureChange.materialNo].SetTexture(
                                    textureChange.propName, 
                                    AssetLoader.LoadTexture(textureChange.filename)
                                );
                            }
                            
                            renderer.sharedMaterials = newMaterials;
                            renderer.sharedMesh.RecalculateTangents();
                        }
                        catch (Exception ex)
                        {
                            MTEUtils.LogException(ex);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MTEUtils.LogException(ex);
                }
            }
        }

        private void ApplyMaterialChanges(Material[] materials, PBRModelInfo pbrModelInfo)
        {
            string nprMatPrefix = "_NPRMAT_";
            Regex nprRegex = new Regex(nprMatPrefix.ToLower(), RegexOptions.Compiled);
            
            foreach (MaterialChangeInfo materialChange in pbrModelInfo.materialChanges)
            {
                if (materialChange.filename.ToLower().Contains(nprMatPrefix.ToLower()))
                {
                    // NPRシェーダーマテリアルの処理
                    string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(materialChange.filename);
                    string[] parts = nprRegex.Split(fileNameWithoutExtension.ToLower());
                    string shaderName = parts.Last();
                    
                    try
                    {
                        using (AFileBase afileBase = GameUty.FileOpen(materialChange.filename, null))
                        {
                            if (afileBase.IsValid())
                            {
                                MTEUtils.Log($"NPRShader: シェーダー変更 {pbrModelInfo.menuFileName}");
                                materials[materialChange.materialNo] = LoadMaterialWithSetShader(materialChange.filename, shaderName, null);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MTEUtils.LogException(ex);
                        materials[materialChange.materialNo] = AssetLoader.LoadMaterial(materialChange.filename, null);
                    }
                }
                else
                {
                    // 通常のマテリアル処理
                    materials[materialChange.materialNo] = AssetLoader.LoadMaterial(materialChange.filename, null);
                }
            }
        }

        private static Material LoadMaterialWithSetShader(string materialName, string shaderName, Material existmat = null)
        {
            try
            {
                return AssetLoader.LoadMaterialWithSetShader(materialName, shaderName, existmat);
            }
            catch (Exception)
            {
                try
                {
                    MTEUtils.Log($"NPRShader: 2回目のロードを試行します: {materialName}, {shaderName}");
                    return AssetLoader.LoadMaterialWithSetShader(materialName, shaderName, existmat);
                }
                catch (Exception ex2)
                {
                    MTEUtils.LogException(ex2);
                    return null;
                }
            }
        }
    }
}