using System;
using System.Reflection;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    [AttributeUsage(AttributeTargets.Class)]
    public class TimelineLayerDescAttribute : Attribute
    {
        public string DisplayName { get; set; }
        public int Priority { get; set; }

        public TimelineLayerDescAttribute(string displayName, int priority)
        {
            DisplayName = displayName;
            Priority = priority;
        }
    }

    public class TimelineLayerInfo
    {
        public int index;
        public readonly int priority;
        public readonly Type layerType;
        public readonly string className;
        public readonly string displayName;
        public readonly Func<int, ITimelineLayer> createLayer;

        public TimelineLayerInfo(
            Type layerType,
            Func<int, ITimelineLayer> createLayer)
        {
            this.layerType = layerType;
            this.createLayer = createLayer;

            className = layerType.Name;
            displayName = layerType.Name;

            var displayNameAttr = layerType.GetCustomAttribute<TimelineLayerDescAttribute>();
            if (displayNameAttr != null)
            {
                displayName = displayNameAttr.DisplayName;
                priority = displayNameAttr.Priority;
            }
        }

        public bool ValidateLayer()
        {
            var validateMethod = layerType.GetMethod("ValidateLayer",
                BindingFlags.Public | BindingFlags.Static);
            if (validateMethod == null)
            {
                return true;
            }

            return (bool)validateMethod.Invoke(null, null);
        }
    }
}