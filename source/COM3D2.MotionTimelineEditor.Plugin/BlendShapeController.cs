using System.Linq;
using UnityEngine;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    public class BlendShapeController : MonoBehaviour
    {
        private Mesh mesh;
        private BlendShapeCacheData blendData;
        private float[] blendWeights;
        
        private Vector3[] orgVertices;
        private Vector3[] orgNormals;
        private Vector3[] tmpVertices;
        private Vector3[] tmpNormals;

        public int blendShapeCount
        {
            get
            {
                return blendData.blendShapes.Count;
            }
        }

        public void Init(Mesh mesh, BlendShapeCacheData blendData)
        {
            this.mesh = mesh;
            this.blendData = blendData;

            if (blendData.blendShapes.Count == 0)
            {
                return;
            }

            orgVertices = mesh.vertices.Clone() as Vector3[];
            orgNormals = mesh.normals.Clone() as Vector3[];
            tmpVertices = new Vector3[mesh.vertexCount];
            tmpNormals = new Vector3[mesh.vertexCount];

            blendWeights = new float[blendData.blendShapes.Count];

            for (int i = 0; i < blendWeights.Length; i++)
            {
                blendWeights[i] = 0f;
            }
        }

        public int GetBlendShapeIndex(string name)
        {
            for (int i = 0; i < blendData.blendShapes.Count; i++)
            {
                if (blendData.blendShapes[i].name == name)
                {
                    return i;
                }
            }
            return -1;
        }

        public string GetBlendShapeName(int index)
        {
            if (index >= 0 && index < blendData.blendShapes.Count)
            {
                return blendData.blendShapes[index].name;
            }
            PluginUtils.LogWarning("BlendShapeIndex out of range: {0}", index);
            return null;
        }

        public float GetBlendShapeWeight(int index)
        {
            if (index >= 0 && index < blendWeights.Length)
            {
                return blendWeights[index];
            }
            PluginUtils.LogWarning("BlendShapeIndex out of range: {0}", index);
            return 0f;
        }

        public void SetBlendShapeWeight(int index, float weight)
        {
            if (index >= 0 && index < blendWeights.Length)
            {
                blendWeights[index] = weight;
                return;
            }
            PluginUtils.LogWarning("BlendShapeIndex out of range: {0}", index);
        }

        public void FixBlendValues()
        {
            if (blendData.blendShapes.Count == 0)
            {
                return;
            }

            orgVertices.CopyTo(tmpVertices, 0);
            orgNormals.CopyTo(tmpNormals, 0);

            for (int i = 0; i < blendData.blendShapes.Count; i++)
            {
                var blendShape = blendData.blendShapes[i];
                var weight = blendWeights[i];

                if (weight == 0f)
                {
                    continue;
                }

                for (int j = 0; j < blendShape.vertexCount; j++)
                {
                    var index = blendShape.vertexIndices[j];
                    tmpVertices[index] += blendShape.vertices[j] * weight;
                    tmpNormals[index] += blendShape.normals[j] * weight;
                }
            }

            mesh.vertices = tmpVertices;
            mesh.normals = tmpNormals;
        }
    }
}