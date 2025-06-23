using System;
using UnityEditor;
using UnityEngine;

namespace Narazaka.VRChat.OverrideValuesOnBuild.Editor.SerializedHandlers
{
    static class SerializedJsonValue
    {
        static string prefix = @"{""value"":";
        static string suffix = @"}";
        static int prefixLength = prefix.Length;
        static int suffixLength = suffix.Length;

        public static string Serialize(SerializedPropertyType type, object value)
        {
            return Trim(SerializeRaw(type, value));
        }

        static string Trim(string value)
        {
            return value.Substring(prefixLength, value.Length - prefixLength - suffixLength);
        }

        static string SerializeRaw(SerializedPropertyType type, object value)
        {
            switch (type)
            {
                case SerializedPropertyType.Integer:
                    return JsonUtility.ToJson(new IntValue { value = (int)value });
                case SerializedPropertyType.Boolean:
                    return JsonUtility.ToJson(new BoolValue { value = (bool)value });
                case SerializedPropertyType.Float:
                    return JsonUtility.ToJson(new FloatValue { value = (float)value });
                case SerializedPropertyType.String:
                    return JsonUtility.ToJson(new StringValue { value = (string)value });
                case SerializedPropertyType.Color:
                    return JsonUtility.ToJson(new ColorValue { value = (Color)value });
                case SerializedPropertyType.ObjectReference:
                    return EditorJsonUtility.ToJson(new ObjectReferenceValue { value = (UnityEngine.Object)value });
                case SerializedPropertyType.LayerMask:
                    return JsonUtility.ToJson(new LayerMaskValue { value = (LayerMask)value });
                case SerializedPropertyType.Enum:
                    return JsonUtility.ToJson(new EnumValue { value = (int)value });
                case SerializedPropertyType.Vector2:
                    return JsonUtility.ToJson(new Vector2Value { value = (Vector2)value });
                case SerializedPropertyType.Vector3:
                    return JsonUtility.ToJson(new Vector3Value { value = (Vector3)value });
                case SerializedPropertyType.Vector4:
                    return JsonUtility.ToJson(new Vector4Value { value = (Vector4)value });
                case SerializedPropertyType.Rect:
                    return JsonUtility.ToJson(new RectValue { value = (Rect)value });
                case SerializedPropertyType.ArraySize:
                    return JsonUtility.ToJson(new ArraySizeValue { value = (int)value });
                case SerializedPropertyType.Character:
                    return JsonUtility.ToJson(new CharacterValue { value = (char)value });
                case SerializedPropertyType.AnimationCurve:
                    return JsonUtility.ToJson(new AnimationCurveValue { value = (AnimationCurve)value });
                case SerializedPropertyType.Bounds:
                    return JsonUtility.ToJson(new BoundsValue { value = (Bounds)value });
                case SerializedPropertyType.Gradient:
                    return JsonUtility.ToJson(new GradientValue { value = (Gradient)value });
                case SerializedPropertyType.Quaternion:
                    return JsonUtility.ToJson(new QuaternionValue { value = (Quaternion)value });
                case SerializedPropertyType.ExposedReference:
                    return JsonUtility.ToJson(new ExposedReferenceValue { value = (UnityEngine.Object)value });
                case SerializedPropertyType.FixedBufferSize:
                    return JsonUtility.ToJson(new FixedBufferSizeValue { value = (int)value });
                case SerializedPropertyType.Vector2Int:
                    return JsonUtility.ToJson(new Vector2IntValue { value = (Vector2Int)value });
                case SerializedPropertyType.Vector3Int:
                    return JsonUtility.ToJson(new Vector3IntValue { value = (Vector3Int)value });
                case SerializedPropertyType.RectInt:
                    return JsonUtility.ToJson(new RectIntValue { value = (RectInt)value });
                case SerializedPropertyType.BoundsInt:
                    return JsonUtility.ToJson(new BoundsIntValue { value = (BoundsInt)value });
                case SerializedPropertyType.ManagedReference:
                    return JsonUtility.ToJson(new ManagedReferenceValue { value = (UnityEngine.Object)value });
                default:
                    throw new System.NotImplementedException();
            }
        }

        public static object Deserialize(SerializedPropertyType type, string value)
        {
            return DeserializeValue(type, value).v;
        }

        static Value DeserializeValue(SerializedPropertyType type, string value)
        {
            var json = prefix + value + suffix;
            switch (type)
            {
                case SerializedPropertyType.Integer:
                    return JsonUtility.FromJson<IntValue>(json);
                case SerializedPropertyType.Boolean:
                    return JsonUtility.FromJson<BoolValue>(json);
                case SerializedPropertyType.Float:
                    return JsonUtility.FromJson<FloatValue>(json);
                case SerializedPropertyType.String:
                    return JsonUtility.FromJson<StringValue>(json);
                case SerializedPropertyType.Color:
                    return JsonUtility.FromJson<ColorValue>(json);
                case SerializedPropertyType.ObjectReference:
                    var obj = new ObjectReferenceValue();
                    EditorJsonUtility.FromJsonOverwrite(json, obj);
                    return obj;
                case SerializedPropertyType.LayerMask:
                    return JsonUtility.FromJson<LayerMaskValue>(json);
                case SerializedPropertyType.Enum:
                    return JsonUtility.FromJson<EnumValue>(json);
                case SerializedPropertyType.Vector2:
                    return JsonUtility.FromJson<Vector2Value>(json);
                case SerializedPropertyType.Vector3:
                    return JsonUtility.FromJson<Vector3Value>(json);
                case SerializedPropertyType.Vector4:
                    return JsonUtility.FromJson<Vector4Value>(json);
                case SerializedPropertyType.Rect:
                    return JsonUtility.FromJson<RectValue>(json);
                case SerializedPropertyType.ArraySize:
                    return JsonUtility.FromJson<ArraySizeValue>(json);
                case SerializedPropertyType.Character:
                    return JsonUtility.FromJson<CharacterValue>(json);
                case SerializedPropertyType.AnimationCurve:
                    return JsonUtility.FromJson<AnimationCurveValue>(json);
                case SerializedPropertyType.Bounds:
                    return JsonUtility.FromJson<BoundsValue>(json);
                case SerializedPropertyType.Gradient:
                    return JsonUtility.FromJson<GradientValue>(json);
                case SerializedPropertyType.Quaternion:
                    return JsonUtility.FromJson<QuaternionValue>(json);
                case SerializedPropertyType.ExposedReference:
                    return JsonUtility.FromJson<ExposedReferenceValue>(json);
                case SerializedPropertyType.FixedBufferSize:
                    return JsonUtility.FromJson<FixedBufferSizeValue>(json);
                case SerializedPropertyType.Vector2Int:
                    return JsonUtility.FromJson<Vector2IntValue>(json);
                case SerializedPropertyType.Vector3Int:
                    return JsonUtility.FromJson<Vector3IntValue>(json);
                case SerializedPropertyType.RectInt:
                    return JsonUtility.FromJson<RectIntValue>(json);
                case SerializedPropertyType.BoundsInt:
                    return JsonUtility.FromJson<BoundsIntValue>(json);
                case SerializedPropertyType.ManagedReference:
                    return JsonUtility.FromJson<ManagedReferenceValue>(json);
                default:
                    throw new System.NotImplementedException();
            }
        }

        interface Value
        {
            public object v { get; }
        }

        [Serializable] class IntValue : Value { public object v => value; public int value; }
        [Serializable] class BoolValue : Value { public object v => value; public bool value; }
        [Serializable] class FloatValue : Value { public object v => value; public float value; }
        [Serializable] class StringValue : Value { public object v => value; public string value; }
        [Serializable] class ColorValue : Value { public object v => value; public Color value; }
        [Serializable] class ObjectReferenceValue : Value { public object v => value; public UnityEngine.Object value; }
        [Serializable] class LayerMaskValue : Value { public object v => value; public LayerMask value; }
        [Serializable] class EnumValue : Value { public object v => value; public int value; }
        [Serializable] class Vector2Value : Value { public object v => value; public Vector2 value; }
        [Serializable] class Vector3Value : Value { public object v => value; public Vector3 value; }
        [Serializable] class Vector4Value : Value { public object v => value; public Vector4 value; }
        [Serializable] class RectValue : Value { public object v => value; public Rect value; }
        [Serializable] class ArraySizeValue : Value { public object v => value; public int value; }
        [Serializable] class CharacterValue : Value { public object v => value; public char value; }
        [Serializable] class AnimationCurveValue : Value { public object v => value; public AnimationCurve value; }
        [Serializable] class BoundsValue : Value { public object v => value; public Bounds value; }
        [Serializable] class GradientValue : Value { public object v => value; public Gradient value; }
        [Serializable] class QuaternionValue : Value { public object v => value; public Quaternion value; }
        [Serializable] class ExposedReferenceValue : Value { public object v => value; public UnityEngine.Object value; }
        [Serializable] class FixedBufferSizeValue : Value { public object v => value; public int value; }
        [Serializable] class Vector2IntValue : Value { public object v => value; public Vector2Int value; }
        [Serializable] class Vector3IntValue : Value { public object v => value; public Vector3Int value; }
        [Serializable] class RectIntValue : Value { public object v => value; public RectInt value; }
        [Serializable] class BoundsIntValue : Value { public object v => value; public BoundsInt value; }
        [Serializable] class ManagedReferenceValue : Value { public object v => value; public UnityEngine.Object value; }
    }
}
