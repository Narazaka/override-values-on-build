using UnityEngine;
using UnityEditor;

namespace Narazaka.VRChat.OverrideValuesOnBuild.Editor.SerializedHandlers
{
    static class SerializedPropertyField
    {
        public static void PropertyField(Rect rect, SerializedProperty sourceProperty, SerializedProperty property)
        {
            object value;
            try
            {
                value = SerializedJsonValue.Deserialize(sourceProperty.propertyType, property.stringValue);
            }
            catch (System.Exception ex)
            {
                EditorGUI.HelpBox(rect, "Error Value", MessageType.Error);
                return;
            }
            object newValue = null;
            EditorGUI.BeginChangeCheck();
            switch (sourceProperty.propertyType)
            {
                case SerializedPropertyType.Integer:
                    newValue = EditorGUI.IntField(rect, sourceProperty.displayName, (int)value);
                    break;
                case SerializedPropertyType.Boolean:
                    newValue = EditorGUI.Toggle(rect, sourceProperty.displayName, (bool)value);
                    break;
                case SerializedPropertyType.Float:
                    newValue = EditorGUI.FloatField(rect, sourceProperty.displayName, (float)value);
                    break;
                case SerializedPropertyType.String:
                    newValue = EditorGUI.TextField(rect, sourceProperty.displayName, (string)value);
                    break;
                case SerializedPropertyType.Color:
                    newValue = EditorGUI.ColorField(rect, sourceProperty.displayName, (UnityEngine.Color)value);
                    break;
                case SerializedPropertyType.ObjectReference:
                    newValue = EditorGUI.ObjectField(rect, sourceProperty.displayName, (UnityEngine.Object)value, SerializedPropertyTypeResolver.ObjectType(sourceProperty, typeof(UnityEngine.Object)), true);
                    break;
                case SerializedPropertyType.LayerMask:
                    newValue = EditorGUI.LayerField(rect, sourceProperty.displayName, (UnityEngine.LayerMask)value);
                    break;
                case SerializedPropertyType.Enum:
                    var enumType = SerializedPropertyTypeResolver.ObjectType(sourceProperty, typeof(System.Enum));
                    var enumValue = System.Enum.ToObject(enumType, value);
                    newValue = EditorGUI.EnumPopup(rect, sourceProperty.displayName, (System.Enum)enumValue);
                    break;
                case SerializedPropertyType.Vector2:
                    newValue = EditorGUI.Vector2Field(rect, sourceProperty.displayName, (UnityEngine.Vector2)value);
                    break;
                case SerializedPropertyType.Vector3:
                    newValue = EditorGUI.Vector3Field(rect, sourceProperty.displayName, (UnityEngine.Vector3)value);
                    break;
                case SerializedPropertyType.Vector4:
                    newValue = EditorGUI.Vector4Field(rect, sourceProperty.displayName, (UnityEngine.Vector4)value);
                    break;
                case SerializedPropertyType.Rect:
                    newValue = EditorGUI.RectField(rect, sourceProperty.displayName, (UnityEngine.Rect)value);
                    break;
                case SerializedPropertyType.ArraySize:
                    newValue = EditorGUI.IntField(rect, sourceProperty.displayName, (int)value);
                    break;
                case SerializedPropertyType.Character:
                    string charStr = new string((char)value, 1);
                    charStr = EditorGUI.TextField(rect, sourceProperty.displayName, charStr);
                    newValue = charStr.Length > 0 ? charStr[0] : (char)0;
                    break;
                case SerializedPropertyType.AnimationCurve:
                    newValue = EditorGUI.CurveField(rect, sourceProperty.displayName, (UnityEngine.AnimationCurve)value);
                    break;
                case SerializedPropertyType.Bounds:
                    newValue = EditorGUI.BoundsField(rect, sourceProperty.displayName, (UnityEngine.Bounds)value);
                    break;
                case SerializedPropertyType.Gradient:
                    newValue = EditorGUI.GradientField(rect, sourceProperty.displayName, (UnityEngine.Gradient)value);
                    break;
                case SerializedPropertyType.Quaternion:
                    UnityEngine.Vector4 vec4 = EditorGUI.Vector4Field(rect, sourceProperty.displayName, new UnityEngine.Vector4(((UnityEngine.Quaternion)value).x, ((UnityEngine.Quaternion)value).y, ((UnityEngine.Quaternion)value).z, ((UnityEngine.Quaternion)value).w));
                    newValue = new UnityEngine.Quaternion(vec4.x, vec4.y, vec4.z, vec4.w);
                    break;
                case SerializedPropertyType.ExposedReference:
                    newValue = EditorGUI.ObjectField(rect, sourceProperty.displayName, (UnityEngine.Object)value, typeof(UnityEngine.Object), true);
                    break;
                case SerializedPropertyType.FixedBufferSize:
                    newValue = EditorGUI.IntField(rect, sourceProperty.displayName, (int)value);
                    break;
                case SerializedPropertyType.Vector2Int:
                    newValue = EditorGUI.Vector2IntField(rect, sourceProperty.displayName, (UnityEngine.Vector2Int)value);
                    break;
                case SerializedPropertyType.Vector3Int:
                    newValue = EditorGUI.Vector3IntField(rect, sourceProperty.displayName, (UnityEngine.Vector3Int)value);
                    break;
                case SerializedPropertyType.RectInt:
                    newValue = EditorGUI.RectIntField(rect, sourceProperty.displayName, (UnityEngine.RectInt)value);
                    break;
                case SerializedPropertyType.BoundsInt:
                    newValue = EditorGUI.BoundsIntField(rect, sourceProperty.displayName, (UnityEngine.BoundsInt)value);
                    break;
                case SerializedPropertyType.ManagedReference:
                    newValue = EditorGUI.ObjectField(rect, sourceProperty.displayName, (UnityEngine.Object)value, typeof(UnityEngine.Object), true);
                    break;
                default:
                    throw new System.NotImplementedException();
            }
            if (EditorGUI.EndChangeCheck())
            {
                property.stringValue = SerializedJsonValue.Serialize(sourceProperty.propertyType, newValue);
            }
        }
    }
}
