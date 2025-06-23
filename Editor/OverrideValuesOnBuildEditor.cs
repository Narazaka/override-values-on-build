using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Narazaka.VRChat.OverrideValuesOnBuild.Editor.SerializedHandlers;

namespace Narazaka.VRChat.OverrideValuesOnBuild.Editor
{
    [CustomEditor(typeof(OverrideValuesOnBuild))]
    public class OverrideValuesOnBuildEditor : UnityEditor.Editor
    {
        SerializedProperty targetProperty;
        SerializedProperty overrideValuesProperty;
        SerializedObject targetSerializedObject;

        Dictionary<string, int> overrideValuesMap;

        void OnEnable()
        {
            targetProperty = serializedObject.FindProperty(nameof(OverrideValuesOnBuild.target));
            overrideValuesProperty = serializedObject.FindProperty(nameof(OverrideValuesOnBuild.overrideValues));
        }
        public override void OnInspectorGUI()
        {
            serializedObject.UpdateIfRequiredOrScript();
            EditorGUILayout.PropertyField(targetProperty);
            EditorGUILayout.PropertyField(overrideValuesProperty, true);

            if (overrideValuesMap == null)
            {
                overrideValuesMap = new Dictionary<string, int>();
                for (int i = 0; i < overrideValuesProperty.arraySize; i++)
                {
                    var overrideValue = overrideValuesProperty.GetArrayElementAtIndex(i);
                    var name = overrideValue.FindPropertyRelative(nameof(OverrideValue.name)).stringValue;
                    if (!overrideValuesMap.ContainsKey(name))
                    {
                        overrideValuesMap[name] = i;
                    }
                }
            }

            var targetObject = targetProperty.objectReferenceValue;
            if (targetSerializedObject == null || targetSerializedObject.targetObject != targetObject)
            {
                targetSerializedObject = new SerializedObject(targetObject);
            }

            targetSerializedObject.UpdateIfRequiredOrScript();
            DisplayProperties(serializedObject);
            targetSerializedObject.ApplyModifiedProperties();

            if (serializedObject.hasModifiedProperties)
            {
                overrideValuesMap = null;
            }
            serializedObject.ApplyModifiedProperties();

            var ov = (OverrideValuesOnBuild)target;
            var fieldInfos = SerializeFieldInfo.GetSerializeFields(ov.target);
            foreach (var fieldInfo in fieldInfos)
            {
                EditorGUILayout.LabelField($"{fieldInfo.Name} : {fieldInfo.Type}", $"{fieldInfo.GetValue()}");
            }
        }

        void DisplayProperties(SerializedObject serializedObject)
        {
            var iterator = targetSerializedObject.GetIterator();
            var enterChildren = true;
            while (iterator.NextVisible(enterChildren))
            {
                if ("m_Script" == iterator.propertyPath)
                {
                    continue;
                }

                DisplayProperty(iterator);

                enterChildren = false;
            }
        }

        void DisplayProperty(SerializedProperty property)
        {
            if (property.propertyType == SerializedPropertyType.Generic)
            {
                property.isExpanded = EditorGUILayout.Foldout(property.isExpanded, property.displayName);
                if (!property.isExpanded) return;
                EditorGUI.indentLevel++;
                var enumerator = property.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    DisplayProperty((SerializedProperty)enumerator.Current);
                }
                EditorGUI.indentLevel--;
            }
            else
            {
                var hasValue = overrideValuesMap.TryGetValue(property.propertyPath, out var overrideValueIndex);
                var heightLayout = GUILayout.Height(EditorGUI.GetPropertyHeight(property));
                var rect = EditorGUILayout.GetControlRect(heightLayout);

                rect = EditorGUI.IndentedRect(rect);
                var indentLevel = EditorGUI.indentLevel;
                EditorGUI.indentLevel = 0;

                var checkRect = rect;
                checkRect.width = 24;
                rect.xMin += checkRect.width + EditorGUIUtility.standardVerticalSpacing;
                EditorGUI.BeginChangeCheck();
                var newHasValue = EditorGUI.Toggle(checkRect, hasValue);
                if (EditorGUI.EndChangeCheck())
                {
                    Debug.Log($"OverrideValuesOnBuildEditor: {property.propertyPath} {(newHasValue ? "enabled" : "disabled")}");
                    if (newHasValue)
                    {
                        overrideValuesProperty.InsertArrayElementAtIndex(overrideValuesProperty.arraySize);
                        var overrideValue = overrideValuesProperty.GetArrayElementAtIndex(overrideValuesProperty.arraySize - 1);
                        overrideValue.FindPropertyRelative(nameof(OverrideValue.name)).stringValue = property.propertyPath;
                        overrideValue.FindPropertyRelative(nameof(OverrideValue.type)).stringValue = property.propertyType.ToString();
                        overrideValue.FindPropertyRelative(nameof(OverrideValue.value)).stringValue = SerializedJsonValue.Serialize(property.propertyType, SerializedValueAccessor.GetValue(property));
                    }
                    else
                    {
                        overrideValuesProperty.DeleteArrayElementAtIndex(overrideValueIndex);
                        overrideValuesMap.Remove(property.propertyPath);
                    }
                }
                EditorGUI.BeginDisabledGroup(true);
                EditorGUI.PropertyField(rect, property);
                EditorGUI.EndDisabledGroup();

                EditorGUI.indentLevel = indentLevel;

                if (hasValue && newHasValue && overrideValueIndex < overrideValuesProperty.arraySize)
                {
                    rect = EditorGUILayout.GetControlRect(heightLayout);

                    rect = EditorGUI.IndentedRect(rect);
                    indentLevel = EditorGUI.indentLevel;
                    EditorGUI.indentLevel = 0;

                    rect.xMin += checkRect.width + EditorGUIUtility.standardVerticalSpacing;

                    var overrideValue = overrideValuesProperty.GetArrayElementAtIndex(overrideValueIndex);
                    var valueProperty = overrideValue.FindPropertyRelative(nameof(OverrideValue.value));
                    SerializedPropertyField.PropertyField(rect, property, valueProperty);

                    EditorGUI.indentLevel = indentLevel;
                }
            }
        }
    }

    [System.Serializable]
    public class JsonValue
    {
        public string value;
    }
}
