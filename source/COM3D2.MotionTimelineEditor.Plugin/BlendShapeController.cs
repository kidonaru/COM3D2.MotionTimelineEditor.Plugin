using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    public class ModelBlendShape
    {
        public BlendShapeController controller { get; set; }
        public string shapeKeyName { get; private set; }
        public float weight { get; set; }

        public StudioModelStat model
        {
            get
            {
                return controller.model;
            }
        }

        public string name
        {
            get
            {
                return string.Format("{0}/{1}", model.name, shapeKeyName);
            }
        }

        public ModelBlendShape(
            BlendShapeController controller,
            string shapeKeyName)
        {
            this.controller = controller;
            this.shapeKeyName = shapeKeyName;
        }
    }

    public class BlendShapeController : MonoBehaviour
    {
        public StudioModelStat model;
        private Mesh mesh;
        private BlendShapeCacheData blendData;
        public List<ModelBlendShape> blendShapes = new List<ModelBlendShape>();

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

            blendShapes.Clear();

            for (int i = 0; i < blendData.blendShapes.Count; i++)
            {
                var shepeKey = blendData.blendShapes[i].name;
                var blendShape = new ModelBlendShape(this, shepeKey);
                blendShapes.Add(blendShape);
            }
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
                var blendShapeData = blendData.blendShapes[i];
                var weight = blendShapes[i].weight;

                if (weight == 0f)
                {
                    continue;
                }

                for (int j = 0; j < blendShapeData.vertexCount; j++)
                {
                    var index = blendShapeData.vertexIndices[j];
                    tmpVertices[index] += blendShapeData.vertices[j] * weight;
                    tmpNormals[index] += blendShapeData.normals[j] * weight;
                }
            }

            mesh.vertices = tmpVertices;
            mesh.normals = tmpNormals;
        }
    }
}