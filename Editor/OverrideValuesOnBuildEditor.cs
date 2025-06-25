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

        List<int> ToDeleteIndexes = new List<int>();

        void OnEnable()
        {
            targetProperty = serializedObject.FindProperty(nameof(OverrideValuesOnBuild.target));
            overrideValuesProperty = serializedObject.FindProperty(nameof(OverrideValuesOnBuild.overrideValues));
        }

        public override void OnInspectorGUI()
        {
            serializedObject.UpdateIfRequiredOrScript();
            EditorGUILayout.PropertyField(targetProperty);

            if (overrideValuesMap == null)
            {
                overrideValuesMap = new Dictionary<string, int>();
                for (int i = 0; i < overrideValuesProperty.arraySize; i++)
                {
                    var overrideValue = overrideValuesProperty.GetArrayElementAtIndex(i);
                    var name = overrideValue.FindPropertyRelative(nameof(OverrideValue.propertyPath)).stringValue;
                    if (!overrideValuesMap.ContainsKey(name))
                    {
                        overrideValuesMap[name] = i;
                    }
                }
            }

            var targetObject = targetProperty.objectReferenceValue;
            if (targetObject != null)
            {
                if (targetSerializedObject == null || targetSerializedObject.targetObject != targetObject)
                {
                    targetSerializedObject = new SerializedObject(targetObject);
                }

                targetSerializedObject.UpdateIfRequiredOrScript();
                ToDeleteIndexes.Clear();
                DisplayProperties(serializedObject);
                if (ToDeleteIndexes.Count > 0)
                {
                    for (int i = ToDeleteIndexes.Count - 1; i >= 0; i--)
                    {
                        var index = ToDeleteIndexes[i];
                        if (index < overrideValuesProperty.arraySize)
                        {
                            overrideValuesProperty.DeleteArrayElementAtIndex(index);
                        }
                    }
                }
                targetSerializedObject.ApplyModifiedProperties();
            }

            EditorGUILayout.PropertyField(overrideValuesProperty, true);

            if (serializedObject.hasModifiedProperties)
            {
                overrideValuesMap = null;
            }
            serializedObject.ApplyModifiedProperties();
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
                        overrideValue.FindPropertyRelative(nameof(OverrideValue.propertyPath)).stringValue = property.propertyPath;
                        overrideValue.FindPropertyRelative(nameof(OverrideValue.propertyType)).intValue = (int)property.propertyType;
                        overrideValue.FindPropertyRelative(nameof(OverrideValue.value)).stringValue = SerializedJsonValue.Serialize(property.propertyType, SerializedValueAccessor.GetValue(property));
                    }
                    else
                    {
                        ToDeleteIndexes.Add(overrideValueIndex);
                        return;
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
}
