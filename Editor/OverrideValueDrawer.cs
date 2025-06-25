using UnityEngine;
using UnityEditor;

namespace Narazaka.VRChat.OverrideValuesOnBuild.Editor
{
    [CustomPropertyDrawer(typeof(OverrideValue))]
    class OverrideValueDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            position.height = EditorGUIUtility.singleLineHeight;
            property.isExpanded = EditorGUI.Foldout(position, property.isExpanded, label, true);
            position.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            if (property.isExpanded)
            {
                EditorGUI.indentLevel++;
                EditorGUI.PropertyField(position, property.FindPropertyRelative(nameof(OverrideValue.propertyPath)));
                position.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                var propertyType = property.FindPropertyRelative(nameof(OverrideValue.propertyType));
                propertyType.intValue = (int)(SerializedPropertyType)EditorGUI.EnumPopup(position, new GUIContent(propertyType.displayName), (SerializedPropertyType)propertyType.intValue);
                position.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                EditorGUI.PropertyField(position, property.FindPropertyRelative(nameof(OverrideValue.value)));
                position.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                EditorGUI.PropertyField(position, property.FindPropertyRelative(nameof(OverrideValue.target)));
                EditorGUI.indentLevel--;
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property);
        }
    }
}
