using System.Collections.Generic;
using System.Reflection;
using COM3D2.MotionTimelineEditor.Plugin;
using System;

namespace COM3D2.MotionTimelineEditor_PngPlacement.Plugin
{
    public abstract class CustomFieldBase
    {
        public Assembly assembly;
        public abstract Type assemblyType { get; }
        public abstract Dictionary<string, string> typeNames { get; }
        public abstract Type defaultParentType { get; set; }
        public abstract Dictionary<string, Type> parentTypes { get; }
        public abstract Dictionary<string, string> overrideFieldName { get; }
        public abstract Dictionary<string, Type[]> methodParameters { get; }

        public virtual bool Init()
        {
            if (!LoadAssembly())
            {
                return false;
            }

            if (!LoadTypes())
            {
                return false;
            }

            if (!PrepareLoadFields())
            {
                return false;
            }

            if (!LoadFields())
            {
                return false;
            }

            return true;
        }

        public virtual bool LoadAssembly()
        {
            assembly = Assembly.GetAssembly(assemblyType);
            PluginUtils.AssertNull(assembly != null, assemblyType.Name + " not found");
            return assembly != null;
        }

        public virtual bool LoadTypes()
        {
            foreach (var typeName in typeNames)
            {
                var type = assembly.GetType(typeName.Value);
                PluginUtils.AssertNull(type != null, typeName.Key + " is null");

                var targetField = this.GetType().GetField(typeName.Key);
                PluginUtils.AssertNull(targetField != null, typeName.Key + " field is null");

                if (type == null || targetField == null)
                {
                    return false;
                }

                targetField.SetValue(this, type);
            }

            return true;
        }

        public virtual bool PrepareLoadFields()
        {   
            return true;
        }

        public virtual bool LoadFields()
        {
            var bindingAttr = BindingFlags.Instance | BindingFlags.Static |
                    BindingFlags.Public | BindingFlags.NonPublic |
                    BindingFlags.GetProperty | BindingFlags.InvokeMethod;

            foreach (var fieldInfo in this.GetType().GetFields())
            {
                try
                {
                    if (!overrideFieldName.TryGetValue(fieldInfo.Name, out var fieldName))
                    {
                        fieldName = fieldInfo.Name;
                    }

                    if (!parentTypes.TryGetValue(fieldInfo.Name, out var parentType))
                    {
                        parentType = defaultParentType;
                    }

                    if (fieldInfo.FieldType == typeof(FieldInfo))
                    {
                        var targetField = parentType.GetField(fieldName, bindingAttr);
                        PluginUtils.AssertNull(targetField != null, "field " + fieldName + " is null");
                        fieldInfo.SetValue(this, targetField);

                        if (targetField == null)
                        {
                            return false;
                        }
                    }
                    else if (fieldInfo.FieldType == typeof(PropertyInfo))
                    {
                        var targetProperty = parentType.GetProperty(fieldName, bindingAttr);
                        PluginUtils.AssertNull(targetProperty != null, "property " + fieldName + " is null");
                        fieldInfo.SetValue(this, targetProperty);

                        if (targetProperty == null)
                        {
                            return false;
                        }
                    }
                    else if (fieldInfo.FieldType == typeof(MethodInfo))
                    {
                        if (!methodParameters.TryGetValue(fieldInfo.Name, out var parameters))
                        {
                            parameters = null;
                        }

                        MethodInfo targetMethod = null;
                        if (parameters == null)
                        {
                            targetMethod = parentType.GetMethod(fieldName, bindingAttr);
                        }
                        else
                        {
                            targetMethod = parentType.GetMethod(fieldName, bindingAttr, null, parameters, null);
                        }
                        
                        PluginUtils.AssertNull(targetMethod != null, "method " + fieldName + " is null");
                        fieldInfo.SetValue(this, targetMethod);

                        if (targetMethod == null)
                        {
                            return false;
                        }
                    }
                }
                catch (Exception e)
                {
                    PluginUtils.LogError("Error loading field " + fieldInfo.Name);
                    PluginUtils.LogException(e);
                    return false;
                }
            }

            return true;
        }
    }
}