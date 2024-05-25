using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    public class BlendShapeCacheData
    {
        public class BlendShapeData
        {
            public string name;
            public int vertexCount;
            public int[] vertexIndices;
            public Vector3[] vertices;
            public Vector3[] normals;

            public BlendShapeData()
            {
            }
        }

        public List<BlendShapeData> blendShapes = new List<BlendShapeData>();

        public BlendShapeCacheData()
        {
        }

        public void Load(BinaryReader reader)
        {
            blendShapes.Clear();

            for (;;)
            {
                string a = reader.ReadString();
                if (a == "end")
                {
                    break;
                }
                if (a == "morph")
                {
                    LoadShapeData(reader);
                }
            }
        }

        public void LoadShapeData(BinaryReader reader)
        {
            var blendShape = new BlendShapeData();
            blendShape.name = reader.ReadString();
            blendShape.vertexCount = reader.ReadInt32();
            blendShape.vertexIndices = new int[blendShape.vertexCount];
            blendShape.vertices = new Vector3[blendShape.vertexCount];
            blendShape.normals = new Vector3[blendShape.vertexCount];
            for (int i = 0; i < blendShape.vertexCount; i++)
            {
                blendShape.vertexIndices[i] = reader.ReadUInt16();
                blendShape.vertices[i] = reader.ReadVector3();
                blendShape.normals[i] = reader.ReadVector3();
            }
            blendShapes.Add(blendShape);

            PluginUtils.LogDebug("Loaded BlendShapeData: {0} count: {1}", blendShape.name, blendShape.vertexCount);
        }

        public void SaveBinary(BinaryWriter writer)
        {
            foreach (var blendShape in blendShapes)
            {
                writer.Write("morph");
                writer.Write(blendShape.name);
                writer.Write(blendShape.vertexCount);
                for (int i = 0; i < blendShape.vertexCount; i++)
                {
                    writer.Write((ushort)blendShape.vertexIndices[i]);
                    writer.Write(blendShape.vertices[i]);
                    writer.Write(blendShape.normals[i]);
                }
            }
            writer.Write("end");
        }
    }
}