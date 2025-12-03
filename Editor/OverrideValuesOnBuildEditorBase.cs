#nullable enable

using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Narazaka.VRChat.OverrideValuesOnBuild.Editor.SerializedHandlers;

namespace Narazaka.VRChat.OverrideValuesOnBuild.Editor
{
    public abstract class OverrideValuesOnBuildEditorBase<T> : UnityEditor.Editor where T : OverrideValuesOnBuildBase
    {
        protected T targetComponent = null!;

        protected abstract string overrideValuesPropertyName { get; }
    
        SerializedProperty overrideValuesProperty = null!;
        Dictionary<string, int>? _overrideValuesMap = null;
        SerializedObject? _editingTargetSerializedObject = null;
        List<int> _toDeleteIndexes = new();

        protected virtual void OnEnable()
        {
            targetComponent = (T)target;
            overrideValuesProperty = serializedObject.FindProperty(overrideValuesPropertyName);
        }

        public sealed override void OnInspectorGUI()
        {
            serializedObject.UpdateIfRequiredOrScript();

            _overrideValuesMap ??= GetOverrideValuesMap();

            var editingTarget = DrawTargetSelection();
            DrawEditingTargetProperties(editingTarget, _overrideValuesMap);

            EditorGUILayout.PropertyField(overrideValuesProperty, true);
            if (serializedObject.hasModifiedProperties)
            {
                _overrideValuesMap = null;
            }

            serializedObject.ApplyModifiedProperties();
        }

        protected abstract Object? DrawTargetSelection();

        private void DrawEditingTargetProperties(Object? editingTarget, Dictionary<string, int> overrideValuesMap)
        {
            if (editingTarget == null) return;
            if (_editingTargetSerializedObject == null || _editingTargetSerializedObject.targetObject != editingTarget)
            {
                _editingTargetSerializedObject = new SerializedObject(editingTarget);
            }

            _editingTargetSerializedObject.UpdateIfRequiredOrScript();
            _toDeleteIndexes.Clear();
            DisplayProperties(_editingTargetSerializedObject, overrideValuesMap, _toDeleteIndexes);
            if (_toDeleteIndexes.Count > 0)
            {
                for (int i = _toDeleteIndexes.Count - 1; i >= 0; i--)
                {
                    var index = _toDeleteIndexes[i];
                    if (index < overrideValuesProperty.arraySize)
                    {
                        overrideValuesProperty.DeleteArrayElementAtIndex(index);
                    }
                }
            }
            _editingTargetSerializedObject.ApplyModifiedProperties();
        }

        private Dictionary<string, int> GetOverrideValuesMap()
        {
            var overrideValuesMap = new Dictionary<string, int>();
            for (int i = 0; i < overrideValuesProperty.arraySize; i++)
            {
                var overrideValue = overrideValuesProperty.GetArrayElementAtIndex(i);
                var name = overrideValue.FindPropertyRelative(nameof(OverrideValue.propertyPath)).stringValue;
                if (!overrideValuesMap.ContainsKey(name))
                {
                    overrideValuesMap[name] = i;
                }
            }
            return overrideValuesMap;
        }

        private void DisplayProperties(SerializedObject serializedObject, Dictionary<string, int> overrideValuesMap, List<int> toDeleteIndexes)
        {
            var iterator = serializedObject.GetIterator();
            var enterChildren = true;
            while (iterator.NextVisible(enterChildren))
            {
                if ("m_Script" == iterator.propertyPath)
                {
                    continue;
                }

                DisplayProperty(iterator, overrideValuesMap, toDeleteIndexes);

                enterChildren = false;
            }
        }

        private void DisplayProperty(SerializedProperty property, Dictionary<string, int> overrideValuesMap, List<int> toDeleteIndexes)
        {
            if (property.propertyType == SerializedPropertyType.Generic)
            {
                property.isExpanded = EditorGUILayout.Foldout(property.isExpanded, property.displayName);
                if (!property.isExpanded) return;
                EditorGUI.indentLevel++;
                var enumerator = property.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    DisplayProperty((SerializedProperty)enumerator.Current, overrideValuesMap, toDeleteIndexes);
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
                    if (newHasValue)
                    {
                        overrideValuesProperty.InsertArrayElementAtIndex(overrideValuesProperty.arraySize);
                        var overrideValue = overrideValuesProperty.GetArrayElementAtIndex(overrideValuesProperty.arraySize - 1);
                        overrideValue.FindPropertyRelative(nameof(OverrideValue.propertyPath)).stringValue = property.propertyPath;
                        overrideValue.FindPropertyRelative(nameof(OverrideValue.propertyType)).intValue = (int)property.propertyType;
                        overrideValue.FindPropertyRelative(nameof(OverrideValue.value)).stringValue = property.propertyType == SerializedPropertyType.ObjectReference ? "" : SerializedJsonValue.Serialize(property.propertyType, SerializedValueAccessor.GetValue(property));
                        overrideValue.FindPropertyRelative(nameof(OverrideValue.target)).objectReferenceValue = property.propertyType == SerializedPropertyType.ObjectReference ? property.objectReferenceValue : null;
                    }
                    else
                    {
                        toDeleteIndexes.Add(overrideValueIndex);
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
                    var targetProperty = overrideValue.FindPropertyRelative(nameof(OverrideValue.target));
                    if (property.propertyType == SerializedPropertyType.ObjectReference && string.IsNullOrEmpty(valueProperty.stringValue))
                    {
                        EditorGUI.ObjectField(rect, targetProperty, SerializedPropertyTypeResolver.ObjectType(property, typeof(UnityEngine.Object)));
                    }
                    else
                    {
                        SerializedPropertyField.PropertyField(rect, property, valueProperty);
                    }

                    EditorGUI.indentLevel = indentLevel;
                }
            }
        }
    }
}
