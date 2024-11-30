using System.Collections.Generic;
using System.Linq;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    public class MaidBlendShape
    {
        public class Entity
        {
            public TMorph morph;
            public int shapeKeyIndex;

            public float weight
            {
                get => morph.GetBlendValues(shapeKeyIndex);
                set => morph.SetBlendValues(shapeKeyIndex, value);
            }
        }

        public List<Entity> entities = new List<Entity>();

        public float weight
        {
            get
            {
                if (entities.Count > 0)
                {
                    return entities.First().weight;
                }
                return 0f;
            }
            set
            {
                foreach (var entity in entities)
                {
                    entity.weight = value;
                }
            }
        }
    }
}