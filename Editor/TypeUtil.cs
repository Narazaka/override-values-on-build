using System.Collections.Generic;
using System;

namespace Narazaka.VRChat.OverrideValuesOnBuild.Editor
{
    internal static class TypeUtil
    {
        static Dictionary<string, Type> TypeCache = new Dictionary<string, Type>();

        public static Type GetType(string typeName)
        {
            if (TypeCache.ContainsKey(typeName))
            {
                return TypeCache[typeName];
            }

            Type type = Type.GetType(typeName);
            if (type != null)
            {
                TypeCache[typeName] = type;
                return type;
            }

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                type = assembly.GetType(typeName);
                if (type != null)
                {
                    TypeCache[typeName] = type;
                    return type;
                }
            }

            return null;
        }

        public static string GetTypeName(Type type)
        {
            if (type == null)
            {
                return null;
            }

            return type.FullName;
        }
    }
}
