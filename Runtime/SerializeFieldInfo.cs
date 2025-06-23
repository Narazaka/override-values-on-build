using System.Collections.Generic;
using System.Reflection;
using System;
using UnityEngine;

namespace Narazaka.VRChat.OverrideValuesOnBuild
{
    internal class SerializeFieldInfo
    {
        public static IList<SerializeFieldInfo> GetSerializeFields(Component target)
        {
            if (target == null)
            {
                return new SerializeFieldInfo[0];
            }
            var members = target.GetType().GetMembers(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (members.Length == 0)
            {
                return new SerializeFieldInfo[0];
            }
            var serializedFields = new List<SerializeFieldInfo>();
            foreach (var member in members)
            {
                if (IgnoreParents.Contains(member.DeclaringType))
                {
                    continue; // Skip members from MonoBehaviour or Component
                }
                if (member is FieldInfo field && IsSerialize(field))
                {
                    serializedFields.Add(new SerializeFieldInfo(target, field));
                }
                else if (member is PropertyInfo property && IsSerialize(property))
                {
                    serializedFields.Add(new SerializeFieldInfo(target, property));
                }
            }
            return serializedFields;
        }

        static HashSet<Type> IgnoreParents = new HashSet<Type>
            {
                typeof(MonoBehaviour),
                typeof(Behaviour),
                typeof(Component),
                typeof(UnityEngine.Object),
            };

        static bool IsSerialize(FieldInfo field)
        {
            return field.IsDefined(typeof(SerializeField), true) || (field.IsPublic && !field.IsDefined(typeof(NonSerializedAttribute)));
        }

        static bool IsSerialize(PropertyInfo property)
        {
            if (property.GetIndexParameters().Length != 0) return false;
            if (property.IsDefined(typeof(SerializeField), true)) return true;
            if (property.IsDefined(typeof(NonSerializedAttribute), true)) return false;
            return property.GetAccessors().Length == 2;
        }

        Component target;
        bool isField;
        FieldInfo field;
        PropertyInfo property;

        public SerializeFieldInfo(Component target, FieldInfo field)
        {
            this.target = target;
            this.field = field;
            isField = true;
        }

        public SerializeFieldInfo(Component target, PropertyInfo property)
        {
            this.target = target;
            this.property = property;
            isField = false;
        }

        public string Name => isField ? field.Name : property.Name;
        public Type Type => isField ? field.FieldType : property.PropertyType;
        public object GetValue() => isField ? field.GetValue(target) : property.GetValue(target, null);
        public void SetValue(object value)
        {
            if (isField)
            {
                field.SetValue(target, value);
            }
            else
            {
                property.SetValue(target, value, null);
            }
        }
    }
}
