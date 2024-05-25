using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    public class BlendShapeLoader
    {
        private static Dictionary<string, BlendShapeCacheData> blendShapeCacheMap = new Dictionary<string, BlendShapeCacheData>();
        private static byte[] fileBuffer;

        public static BlendShapeController LoadController(StudioModelStat model)
        {
            if (model == null || model.transform == null || model.info == null || string.IsNullOrEmpty(model.info.fileName))
            {
                return null;
            }

			var controller = model.transform.GetComponent<BlendShapeController>();
			if (controller != null)
			{
				return controller;
			}

            var blendShapeCache = LoadCache(model.info.fileName);
            if (blendShapeCache == null)
            {
                return null;
            }

            var meshRenderer = model.transform.GetComponentInChildren<SkinnedMeshRenderer>();
            if (meshRenderer == null)
            {
                return null;
            }

            controller = model.transform.gameObject.AddComponent<BlendShapeController>();
			controller.Init(meshRenderer.sharedMesh, blendShapeCache);
            return controller;
        }

        public static BlendShapeCacheData LoadCache(string menuFileName)
        {
            PluginUtils.LogDebug("Menuからブレンドシェイプのロード中: {0}", menuFileName);

            BlendShapeCacheData blendShapeCache;
            if (blendShapeCacheMap.TryGetValue(menuFileName, out blendShapeCache))
            {
                return blendShapeCache;
            }

            var modelFileName = ReadModelFileName(menuFileName);
            if (modelFileName == null)
            {
                return null;
            }

            blendShapeCache = LoadCacheFromModel(modelFileName);
            if (blendShapeCache == null)
            {
                return null;
            }

            blendShapeCacheMap[menuFileName] = blendShapeCache;
            return blendShapeCache;
        }

        private static string ReadModelFileName(string menuFileName)
		{
			byte[] buffer;
			try
			{
				buffer = ReadAFileBase(menuFileName);
			}
			catch (Exception ex)
			{
				PluginUtils.LogError("Could not read menu file '" + menuFileName + "' because " + ex.Message);
				return null;
			}
			try
			{
				using (var reader = new BinaryReader(new MemoryStream(buffer), Encoding.UTF8))
				{
					if (!(reader.ReadString() == "CM3D2_MENU"))
					{
						return null;
					}
					reader.ReadInt32();
					reader.ReadString();
					reader.ReadString();
					reader.ReadString();
					reader.ReadString();
					reader.ReadInt32();
					for (;;)
					{
						byte b = reader.ReadByte();
						string text = string.Empty;
						if (b == 0)
						{
							break;
						}
						for (int i = 0; i < (int)b; i++)
						{
							text = text + "\"" + reader.ReadString() + "\"";
						}
						if (!string.IsNullOrEmpty(text))
						{
							string stringCom = UTY.GetStringCom(text);
							string[] stringList = UTY.GetStringList(text);
							if (stringCom == "end")
							{
								break;
							}
							if (stringCom == "additem")
							{
								return stringList[1];
							}
						}
					}
				}
			}
			catch (Exception ex2)
			{
				PluginUtils.LogWarning("Could not parse menu file '" + menuFileName + "' because " + ex2.Message);
				return null;
			}
			return null;
		}

        public static byte[] ReadAFileBase(string filename)
		{
			byte[] result;
			using (AFileBase afileBase = GameUty.FileOpen(filename, null))
			{
				if (!afileBase.IsValid() || afileBase.GetSize() == 0)
				{
					PluginUtils.LogError("AFileBase '" + filename + "' is invalid");
					result = null;
				}
				else
				{
                    if (fileBuffer == null)
                    {
                        fileBuffer = new byte[Math.Max(500000L, afileBase.GetSize())];
                    }
                    else if ((long)fileBuffer.Length < afileBase.GetSize())
                    {
                        fileBuffer = new byte[afileBase.GetSize()];
                    }
					afileBase.Read(ref fileBuffer, afileBase.GetSize());
					result = fileBuffer;
				}
			}
			return result;
		}

        private static BlendShapeCacheData LoadCacheFromModel(string modelFileName)
		{
            BlendShapeCacheData blendShapeData = null;

			byte[] buffer;
			try
			{
				buffer = ReadAFileBase(modelFileName);
			}
			catch
			{
				PluginUtils.LogError("Could not load model file '" + modelFileName + "'");
				return null;
			}

			using (var reader = new BinaryReader(new MemoryStream(buffer), Encoding.UTF8))
			{
				if (!(reader.ReadString() == "CM3D2_MESH"))
				{
					PluginUtils.LogError(modelFileName + " is not a model file");
				}
				else
				{
					int version = reader.ReadInt32();
					string modelName = reader.ReadString();
					string rootBoneName = reader.ReadString();
					int boneCount = reader.ReadInt32();
					try
					{
						for (int i = 0; i < boneCount; i++)
						{
							var boneName = reader.ReadString();
							var flag = reader.ReadByte() != 0;
						}
						for (int i = 0; i < boneCount; i++)
						{
							var t = reader.ReadInt32();
						}
						for (int i = 0; i < boneCount; i++)
						{
							var localPosition = reader.ReadVector3();
							var localRotation = reader.ReadQuaternion();
							if (version >= 2001 && reader.ReadBoolean())
							{
								var t = reader.ReadVector3();
							}
						}

						int vertexCount = reader.ReadInt32();
						int subMeshCount = reader.ReadInt32();
						int boneArrayCount = reader.ReadInt32();
						for (int i = 0; i < boneArrayCount; i++)
						{
							var boneName = reader.ReadString();
						}

						for (int i = 0; i < boneArrayCount; i++)
						{
							var bindPose = reader.ReadMatrix4x4();
						}

						for (int i = 0; i < vertexCount; i++)
						{
							var vertex = reader.ReadVector3();
							var normal = reader.ReadVector3();
							var uvs = reader.ReadVector2();
						}

						int tangentCount = reader.ReadInt32();
						if (tangentCount > 0)
						{
							for (int i = 0; i < tangentCount; i++)
							{
								var tangent = reader.ReadVector4();
							}
						}

						for (int i = 0; i < vertexCount; i++)
						{
							var boneIndex0 = (int)reader.ReadUInt16();
                            var boneIndex1 = (int)reader.ReadUInt16();
                            var boneIndex2 = (int)reader.ReadUInt16();
                            var boneIndex3 = (int)reader.ReadUInt16();
                            var weight0 = reader.ReadSingle();
                            var weight1 = reader.ReadSingle();
                            var weight2 = reader.ReadSingle();
                            var weight3 = reader.ReadSingle();
						}

						for (int i = 0; i < subMeshCount; i++)
						{
							int trianglesCount = reader.ReadInt32();
							for (int j = 0; j < trianglesCount; j++)
							{
								var triangle = (int)reader.ReadUInt16();
							}
						}

						int materialCount = reader.ReadInt32();
						for (int i = 0; i < materialCount; i++)
						{
							var material = ImportCM.ReadMaterial(reader, null, null);
                            UnityEngine.Object.Destroy(material);
						}

                        blendShapeData = new BlendShapeCacheData();
                        blendShapeData.Load(reader);
					}
					catch (Exception ex)
					{
						PluginUtils.LogError(string.Concat(new string[]
						{
							"Could not load mesh for '",
							modelFileName,
							"' because ",
							ex.Message,
							"\n",
							ex.StackTrace
						}));

						blendShapeData = null;
					}
				}
			}
			return blendShapeData;
		}
    }
}