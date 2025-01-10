using System.Collections.Generic;
using System.Reflection;
using System;

namespace COM3D2.MotionTimelineEditor.Plugin
{
    public abstract class CustomFieldBase
    {
        public Assembly assembly;
        public abstract Type assemblyType { get; set; }

        public virtual Dictionary<string, string> typeNames { get; } = new Dictionary<string, string>
        {
        };

        public virtual Type defaultParentType { get; set; } = null;

        public virtual Dictionary<string, Type> parentTypes { get; } = new Dictionary<string, System.Type>
        {
        };

        public virtual Dictionary<string, string> overrideFieldName { get; } = new Dictionary<string, string>
        {
        };

        public virtual Dictionary<string, Type[]> methodParameters { get; } = new Dictionary<string, System.Type[]>
        {
        };

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
            foreach (var pair in typeNames)
            {
                var fieldName = pair.Key;
                var typeName = pair.Value;

                var type = assembly.GetType(typeName);
                if (type == null && typeName.Contains("CM3D2"))
                {
                    type = assembly.GetType(typeName.Replace("CM3D2", "COM3D2"));
                }
                PluginUtils.AssertNull(type != null, fieldName + " is null");

                var targetField = this.GetType().GetField(fieldName);
                PluginUtils.AssertNull(targetField != null, fieldName + " field is null");

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