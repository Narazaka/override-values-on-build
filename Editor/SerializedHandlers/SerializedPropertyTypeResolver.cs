using UnityEngine;
using UnityEditor;

namespace Narazaka.VRChat.OverrideValuesOnBuild.Editor.SerializedHandlers
{
    static class SerializedPropertyTypeResolver
    {
        static System.Text.RegularExpressions.Regex TypeRegex = new System.Text.RegularExpressions.Regex(@"PPtr<\$?([^>]+)>");

        /// <summary>
        /// Determines the type of the object referenced by the SerializedProperty by traversing the property path from the target object.
        /// </summary>
        /// <param name="sourceProperty">The SerializedProperty to analyze.</param>
        /// <returns>The System.Type of the property at the end of the property path.</returns>
        public static System.Type ObjectType(SerializedProperty sourceProperty)
        {
            // Start with the target object's type
            var targetObject = sourceProperty.serializedObject.targetObject;
            if (targetObject == null)
            {
                Debug.LogWarning($"SerializedPropertyTypeResolver: Target object is null for {sourceProperty.propertyPath}");
                return typeof(UnityEngine.Object);
            }

            System.Type currentType = targetObject.GetType();
            string propertyPath = sourceProperty.propertyPath;

            // Process the property path by consuming segments from the start
            string remainingPath = propertyPath;
            while (!string.IsNullOrEmpty(remainingPath))
            {
                int dotIndex = remainingPath.IndexOf('.');
                string segment;
                if (dotIndex >= 0)
                {
                    segment = remainingPath.Substring(0, dotIndex);
                    remainingPath = remainingPath.Substring(dotIndex + 1);
                }
                else
                {
                    segment = remainingPath;
                    remainingPath = string.Empty;
                }

                // Handle array or list indexing (e.g., "data[1]")
                string fieldName = segment;
                int arrayIndex = -1;
                if (segment.Contains("["))
                {
                    var parts = segment.Split(new char[] { '[', ']' }, System.StringSplitOptions.RemoveEmptyEntries);
                    fieldName = parts[0];
                    if (parts.Length > 1 && int.TryParse(parts[1], out int index))
                    {
                        arrayIndex = index;
                    }
                }

                // Special handling for array-like notation ".Array.data"
                bool isArrayData = false;
                if (remainingPath.StartsWith("Array.data"))
                {
                    isArrayData = true;
                    // Skip "Array.data" in the path
                    int nextDotIndex = remainingPath.IndexOf('.', "Array.data".Length);
                    if (nextDotIndex >= 0)
                    {
                        remainingPath = remainingPath.Substring(nextDotIndex + 1);
                    }
                    else
                    {
                        remainingPath = string.Empty;
                    }
                }

                // Find the field or property in the current type
                var fieldInfo = currentType.GetField(fieldName, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (fieldInfo != null)
                {
                    currentType = fieldInfo.FieldType;
                }
                else
                {
                    var propertyInfo = currentType.GetProperty(fieldName, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    if (propertyInfo != null)
                    {
                        currentType = propertyInfo.PropertyType;
                    }
                    else
                    {
                        Debug.LogWarning($"SerializedPropertyTypeResolver: Could not find field or property {fieldName} in type {currentType.Name} for path {propertyPath}");
                        return typeof(UnityEngine.Object);
                    }
                }

                // If it's an array or list, or if it's part of the ".Array.data" notation, get the element type
                if (arrayIndex >= 0 || isArrayData)
                {
                    if (currentType.IsArray)
                    {
                        currentType = currentType.GetElementType();
                    }
                    else if (currentType.IsGenericType && currentType.GetGenericTypeDefinition() == typeof(System.Collections.Generic.List<>))
                    {
                        currentType = currentType.GetGenericArguments()[0];
                    }
                    else
                    {
                        Debug.LogWarning($"SerializedPropertyTypeResolver: Expected array or list for {fieldName} in path {propertyPath}, but got {currentType.Name}");
                        return typeof(UnityEngine.Object);
                    }
                }
            }

            // For object references, try to extract a more specific type if possible
            if (sourceProperty.propertyType == SerializedPropertyType.ObjectReference && currentType == typeof(UnityEngine.Object))
            {
                string typeHint = TypeRegex.Replace(sourceProperty.type, "$1");
                var hintedType = TypeUtil.GetType(typeHint) ?? TypeUtil.GetType("UnityEngine." + typeHint);
                if (hintedType != null && hintedType.IsSubclassOf(typeof(UnityEngine.Object)))
                {
                    currentType = hintedType;
                }
            }

            Debug.Log($"SerializedPropertyTypeResolver: ObjectType for {sourceProperty.propertyPath} resolved to {currentType.Name}");
            return currentType;
        }
    }
}
