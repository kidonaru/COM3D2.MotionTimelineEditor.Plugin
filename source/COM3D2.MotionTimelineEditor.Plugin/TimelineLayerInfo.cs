using System;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    [AttributeUsage(AttributeTargets.Class)]
    public class LayerDisplayNameAttribute : Attribute
    {
        public string DisplayName { get; set; }

        public LayerDisplayNameAttribute(string displayName)
        {
            DisplayName = displayName;
        }
    }

    public class TimelineLayerInfo
    {
        public readonly int index;
        public readonly Type layerType;
        public readonly string className;
        public readonly string displayName;
        public readonly Func<int, ITimelineLayer> createLayer;

        public TimelineLayerInfo(
            int index,
            Type layerType,
            Func<int, ITimelineLayer> createLayer)
        {
            this.index = index;
            this.layerType = layerType;
            this.createLayer = createLayer;

            className = layerType.Name;
            displayName = layerType.Name;

            var displayNameAttr = layerType.GetCustomAttribute<LayerDisplayNameAttribute>();
            if (displayNameAttr != null)
            {
                displayName = displayNameAttr.DisplayName;
            }
        }
    }
}