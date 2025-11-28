#nullable enable

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System;
using System.Linq;
using Object = UnityEngine.Object;

namespace Narazaka.VRChat.OverrideValuesOnBuild.Editor
{
    [CustomEditor(typeof(OverrideValuesOnBuildInChildren))]
    public class OverrideValuesOnBuildInChildrenEditor : OverrideValuesOnBuildEditorBase<OverrideValuesOnBuildInChildren>
    {
        protected override string overrideValuesPropertyName => nameof(OverrideValuesOnBuildInChildren.overrideValues);
        
        SerializedProperty rootObjectProperty = null!;

        (Type, List<Component>)[] childrenComponents = new (Type, List<Component>)[0];
        string[] componentTypeNames = new string[0];

        int selectedTypeIndex = -1;
        Component? currentEditingTarget = null;

        protected override void OnEnable()
        {
            base.OnEnable();
            rootObjectProperty = serializedObject.FindProperty(nameof(OverrideValuesOnBuildInChildren.rootObject));

            UpdateComponentInformation(rootObjectProperty.objectReferenceValue as GameObject);
            var type = targetComponent.targetType.ToType();
            if (type == null) return;
            var index = childrenComponents.ToList().FindIndex(pair => pair.Item1 == type);
            if (index != -1) {selectedTypeIndex = index; currentEditingTarget = childrenComponents[index].Item2.FirstOrDefault();}
        }
        
        protected override Object? DrawTargetSelection()
        {
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(rootObjectProperty);
            if (EditorGUI.EndChangeCheck())
            {
                OnRootObjectChanged(rootObjectProperty.objectReferenceValue as GameObject);
            }

            using (new EditorGUI.DisabledScope(rootObjectProperty.objectReferenceValue == null))
            {
                var newSelectedTypeIndex = EditorGUILayout.Popup(new GUIContent("Type"), selectedTypeIndex, componentTypeNames);
                if (newSelectedTypeIndex != selectedTypeIndex)
                {
                    selectedTypeIndex = newSelectedTypeIndex;
                    OnTypeSelected(newSelectedTypeIndex);
                }
                EditorGUILayout.ObjectField("For Inspector display", currentEditingTarget, typeof(Component), true);
            }

            return currentEditingTarget;
        }

        private void OnRootObjectChanged(GameObject? rootObject)
        {
            UpdateComponentInformation(rootObject);
            selectedTypeIndex = -1;
            currentEditingTarget = null;
        }

        private void UpdateComponentInformation(GameObject? rootObject)
        {
            if (rootObject == null)
            {
                childrenComponents = new (Type, List<Component>)[0];
                componentTypeNames = new string[0];
            }
            else
            {
                var mapping = new Dictionary<Type, List<Component>>();
                foreach (var component in rootObject.GetComponentsInChildren<Component>(true))
                {
                    var type = component.GetType();
                    if (!mapping.ContainsKey(type))
                    {
                        mapping[type] = new();
                    }
                    mapping[type].Add(component);
                }
                childrenComponents = mapping.Select(pair => (pair.Key, pair.Value)).ToArray();
                componentTypeNames = childrenComponents.Select(pair => pair.Item1.FullName ?? pair.Item1.Name).ToArray();
            }
        }

        private void OnTypeSelected(int newSelectedTypeIndex)
        {
            if (newSelectedTypeIndex < 0 || newSelectedTypeIndex >= childrenComponents.Length)
            {
                currentEditingTarget = null;
                return;
            }

            var newType = childrenComponents[newSelectedTypeIndex].Item1;
            UpdateSerializableType(newType);
            
            // 最初のコンポーネントを適当に代表に選ぶ。
            currentEditingTarget = childrenComponents[newSelectedTypeIndex].Item2.FirstOrDefault();
        }

        private void UpdateSerializableType(Type newType)
        {
            var serializableType = SerializableUnityType.Create(newType);
            Undo.RecordObject(targetComponent, "Change target type");
            targetComponent.targetType = serializableType;
        }
    }
}
