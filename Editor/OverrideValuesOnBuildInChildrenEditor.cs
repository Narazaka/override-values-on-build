#nullable enable

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System;
using System.Linq;
using Object = UnityEngine.Object;
using UnityEditor.IMGUI.Controls;

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
        AdvancedDropdownState componentTypeDropdownState = new();

        protected override void OnEnable()
        {
            base.OnEnable();
            rootObjectProperty = serializedObject.FindProperty(nameof(OverrideValuesOnBuildInChildren.rootObject));

            UpdateComponentInformation(rootObjectProperty.objectReferenceValue as GameObject);
            var type = targetComponent.targetType.ToType();
            if (type == null) return;
            selectedTypeIndex = childrenComponents.ToList().FindIndex(pair => pair.Item1 == type);
            currentEditingTarget = selectedTypeIndex == -1 ? null : childrenComponents[selectedTypeIndex].Item2.FirstOrDefault();
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
                var rect = EditorGUILayout.GetControlRect();
                rect = EditorGUI.PrefixLabel(rect, new GUIContent("Type"));
                var label = selectedTypeIndex >= 0 && selectedTypeIndex < componentTypeNames.Length
                    ? componentTypeNames[selectedTypeIndex]
                    : "Select Type";
                if (GUI.Button(rect, label, EditorStyles.popup))
                {
                    var dropdown = new ComponentTypeDropdown(
                        componentTypeDropdownState,
                        componentTypeNames,
                        selectedTypeIndex,
                        newSelectedTypeIndex =>
                        {
                            if (newSelectedTypeIndex == selectedTypeIndex) return;
                            selectedTypeIndex = newSelectedTypeIndex;
                            OnTypeSelected(newSelectedTypeIndex);
                        });
                    dropdown.Show(rect);
                }
                currentEditingTarget = EditorGUILayout.ObjectField("For Inspector display", currentEditingTarget, typeof(Component), true) as Component;
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
                componentTypeNames = childrenComponents.Select(pair => $"{pair.Item1.Name} ({pair.Item1.FullName})").ToArray();
            }
        }

        private void OnTypeSelected(int newSelectedTypeIndex)
        {
            if (newSelectedTypeIndex < 0 || newSelectedTypeIndex >= childrenComponents.Length)
            {
                currentEditingTarget = null;
                return;
            }

            (var newType, var components) = childrenComponents[newSelectedTypeIndex];

            UpdateSerializableType(components.FirstOrDefault());
            
            // 最初のコンポーネントを適当に代表に選ぶ。
            currentEditingTarget = components.FirstOrDefault();
        }

        private void UpdateSerializableType(Object instance)
        {
            var serializableType = SerializableUnityType.Create(instance);
            Undo.RecordObject(targetComponent, "Change target type");
            targetComponent.targetType = serializableType;
        }

        class ComponentTypeDropdown : AdvancedDropdown
        {
            readonly string[] componentTypeNames;
            readonly int currentIndex;
            readonly Action<int> onSelected;

            class ComponentTypeDropdownItem : AdvancedDropdownItem
            {
                public int Index { get; }

                public ComponentTypeDropdownItem(string name, int index) : base(name)
                {
                    Index = index;
                }
            }

            public ComponentTypeDropdown(
                AdvancedDropdownState state,
                string[] componentTypeNames,
                int currentIndex,
                Action<int> onSelected) : base(state)
            {
                this.componentTypeNames = componentTypeNames;
                this.currentIndex = currentIndex;
                this.onSelected = onSelected;
            }

            protected override AdvancedDropdownItem BuildRoot()
            {
                var root = new AdvancedDropdownItem("Type");
                for (var i = 0; i < componentTypeNames.Length; i++)
                {
                    root.AddChild(new ComponentTypeDropdownItem(componentTypeNames[i], i));
                }

                return root;
            }

            protected override void ItemSelected(AdvancedDropdownItem item)
            {
                if (item is ComponentTypeDropdownItem typeItem)
                {
                    onSelected?.Invoke(typeItem.Index);
                }
            }
        }
    }
}
