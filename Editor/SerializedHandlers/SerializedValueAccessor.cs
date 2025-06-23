using UnityEditor;

namespace Narazaka.VRChat.OverrideValuesOnBuild.Editor.SerializedHandlers
{
    class SerializedValueAccessor
    {
        public static object GetValue(SerializedProperty property)
        {
            switch (property.propertyType)
            {
                case SerializedPropertyType.Integer:
                    return property.intValue;
                case SerializedPropertyType.Boolean:
                    return property.boolValue;
                case SerializedPropertyType.Float:
                    return property.floatValue;
                case SerializedPropertyType.String:
                    return property.stringValue;
                case SerializedPropertyType.Color:
                    return property.colorValue;
                case SerializedPropertyType.ObjectReference:
                    return property.objectReferenceValue;
                case SerializedPropertyType.LayerMask:
                    return (UnityEngine.LayerMask)property.intValue;
                case SerializedPropertyType.Enum:
                    return property.enumValueIndex;
                case SerializedPropertyType.Vector2:
                    return property.vector2Value;
                case SerializedPropertyType.Vector3:
                    return property.vector3Value;
                case SerializedPropertyType.Vector4:
                    return property.vector4Value;
                case SerializedPropertyType.Rect:
                    return property.rectValue;
                case SerializedPropertyType.ArraySize:
                    return property.arraySize;
                case SerializedPropertyType.Character:
                    return (char)property.intValue;
                case SerializedPropertyType.AnimationCurve:
                    return property.animationCurveValue;
                case SerializedPropertyType.Bounds:
                    return property.boundsValue;
                case SerializedPropertyType.Gradient:
                    return property.gradientValue;
                case SerializedPropertyType.Quaternion:
                    return property.quaternionValue;
                case SerializedPropertyType.ExposedReference:
                    return property.exposedReferenceValue;
                case SerializedPropertyType.FixedBufferSize:
                    return property.fixedBufferSize;
                case SerializedPropertyType.Vector2Int:
                    return property.vector2IntValue;
                case SerializedPropertyType.Vector3Int:
                    return property.vector3IntValue;
                case SerializedPropertyType.RectInt:
                    return property.rectIntValue;
                case SerializedPropertyType.BoundsInt:
                    return property.boundsIntValue;
                case SerializedPropertyType.ManagedReference:
                    return property.managedReferenceValue;
                default:
                    throw new System.NotImplementedException();
            }
        }
        public static void SetValue(SerializedProperty property, object value)
        {
            switch (property.propertyType)
            {
                case SerializedPropertyType.Integer:
                    property.intValue = (int)value;
                    break;
                case SerializedPropertyType.Boolean:
                    property.boolValue = (bool)value;
                    break;
                case SerializedPropertyType.Float:
                    property.floatValue = (float)value;
                    break;
                case SerializedPropertyType.String:
                    property.stringValue = (string)value;
                    break;
                case SerializedPropertyType.Color:
                    property.colorValue = (UnityEngine.Color)value;
                    break;
                case SerializedPropertyType.ObjectReference:
                    property.objectReferenceValue = (UnityEngine.Object)value;
                    break;
                case SerializedPropertyType.LayerMask:
                    property.intValue = ((UnityEngine.LayerMask)value).value;
                    break;
                case SerializedPropertyType.Enum:
                    property.enumValueIndex = (int)value;
                    break;
                case SerializedPropertyType.Vector2:
                    property.vector2Value = (UnityEngine.Vector2)value;
                    break;
                case SerializedPropertyType.Vector3:
                    property.vector3Value = (UnityEngine.Vector3)value;
                    break;
                case SerializedPropertyType.Vector4:
                    property.vector4Value = (UnityEngine.Vector4)value;
                    break;
                case SerializedPropertyType.Rect:
                    property.rectValue = (UnityEngine.Rect)value;
                    break;
                case SerializedPropertyType.ArraySize:
                    property.arraySize = (int)value;
                    break;
                case SerializedPropertyType.Character:
                    property.intValue = (int)(char)value;
                    break;
                case SerializedPropertyType.AnimationCurve:
                    property.animationCurveValue = (UnityEngine.AnimationCurve)value;
                    break;
                case SerializedPropertyType.Bounds:
                    property.boundsValue = (UnityEngine.Bounds)value;
                    break;
                case SerializedPropertyType.Gradient:
                    property.gradientValue = (UnityEngine.Gradient)value;
                    break;
                case SerializedPropertyType.Quaternion:
                    property.quaternionValue = (UnityEngine.Quaternion)value;
                    break;
                case SerializedPropertyType.ExposedReference:
                    property.exposedReferenceValue = (UnityEngine.Object)value;
                    break;
                case SerializedPropertyType.FixedBufferSize:
                    // FixedBufferSize is read-only, cannot set value
                    // nop
                    // throw new System.InvalidOperationException("Cannot set FixedBufferSize as it is read-only.");
                    break;
                case SerializedPropertyType.Vector2Int:
                    property.vector2IntValue = (UnityEngine.Vector2Int)value;
                    break;
                case SerializedPropertyType.Vector3Int:
                    property.vector3IntValue = (UnityEngine.Vector3Int)value;
                    break;
                case SerializedPropertyType.RectInt:
                    property.rectIntValue = (UnityEngine.RectInt)value;
                    break;
                case SerializedPropertyType.BoundsInt:
                    property.boundsIntValue = (UnityEngine.BoundsInt)value;
                    break;
                case SerializedPropertyType.ManagedReference:
                    property.managedReferenceValue = (UnityEngine.Object)value;
                    break;
                default:
                    throw new System.NotImplementedException();
            }
        }

        public SerializedProperty Property { get; private set; }

        public SerializedValueAccessor(SerializedProperty property)
        {
            Property = property;
        }

        public object value
        {
            get => GetValue(Property);
            set => SetValue(Property, value);
        }
    }
}
