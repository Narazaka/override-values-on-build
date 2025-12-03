#nullable enable

using UnityEngine;
using UnityEditor;

namespace Narazaka.VRChat.OverrideValuesOnBuild.Editor
{
    [CustomEditor(typeof(OverrideValuesOnBuild))]
    public class OverrideValuesOnBuildEditor : OverrideValuesOnBuildEditorBase<OverrideValuesOnBuild>
    {
        protected override string overrideValuesPropertyName => nameof(OverrideValuesOnBuild.overrideValues);
        
        SerializedProperty targetProperty = null!;
        protected override void OnEnable()
        {
            base.OnEnable();
            targetProperty = serializedObject.FindProperty(nameof(OverrideValuesOnBuild.target));
        }
        protected override Object? DrawTargetSelection()
        {
            EditorGUILayout.PropertyField(targetProperty);
            return targetProperty.objectReferenceValue;
        }
    }
}
