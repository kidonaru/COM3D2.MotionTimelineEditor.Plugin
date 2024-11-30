using UnityEngine;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    public class TransformCache
    {
        public Transform source;
        public Vector3 position;
        public Vector3 scale;

        public Vector3 _eulerAngles;
        private Quaternion _rotation;

        public Vector3 eulerAngles
        {
            get => _eulerAngles;
            set
            {
                _eulerAngles = value;
                _rotation = Quaternion.Euler(value);
            }
        }

        public TransformCache()
        {
        }

        public void Update(Transform source)
        {
            if (source == null)
            {
                this.source = null;
                return;
            }

            if (this.source == source)
            {
                this.position = source.localPosition;
                this.scale = source.localScale;

                if (source.localRotation != _rotation)
                {
                    this.eulerAngles = source.localEulerAngles;
                }
            }
            else
            {
                this.source = source;
                this.position = source.localPosition;
                this.eulerAngles = source.localEulerAngles;
                this.scale = source.localScale;
            }
        }

        public void Apply()
        {
            if (source != null)
            {
                source.localPosition = position;
                source.localRotation = _rotation;
                source.localScale = scale;
            }
        }
    }
}