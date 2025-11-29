#nullable enable

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Narazaka.VRChat.OverrideValuesOnBuild
{
    // ScriptableObject is not supported
    [Serializable]
    public class SerializableUnityType
    {
        public int ClassID;
        public MonoScript? Script;

        public SerializableUnityType(int classID, MonoScript? script)
        {
            ClassID = classID;
            Script = script;
        }

        public SerializableUnityType() : this(0, null) {}

    #if UNITY_EDITOR
        private const int MonoScriptClassID = 114;

        public Type? ToType()
        {
            // User Script
            if (ClassID == MonoScriptClassID)
            {
                if (Script != null)
                {
                    return Script.GetClass();
                }
                return null;
            }
            // Native Class
            else
            {
                return UnityClassIdMap.TryGetType(ClassID, out var type) ? type : null;
            }
        }
        
        public static SerializableUnityType Create(Type type)
        {
            if (type.IsSubclassOf(typeof(MonoBehaviour)))
            {
                return new SerializableUnityType(MonoScriptClassID, FindMonoScript(type));
            }
            else if (UnityClassIdMap.TryGetClassId(type, out int id))
            {
                return new SerializableUnityType(id, null);
            }
            else
            {
                throw new ArgumentException($"SerializableUnityType does not support type: {type.FullName}");
            }
        }

        private static Dictionary<Type, MonoScript>? monoScriptCache = null;
        private static MonoScript? FindMonoScript(Type type)
        {
            if (monoScriptCache == null)
            {
                monoScriptCache = new();
                var guids = AssetDatabase.FindAssets($"t:MonoScript {type.Name}");
                foreach (var guid in guids)
                {
                    var path = AssetDatabase.GUIDToAssetPath(guid);
                    var script = AssetDatabase.LoadAssetAtPath<MonoScript>(path);
                    if (script != null)
                    {
                        monoScriptCache[script.GetClass()] = script;
                    }
                }
            }
            return monoScriptCache.TryGetValue(type, out var value) ? value : null;
        }
    #endif
    }

    #if UNITY_EDITOR
    internal static class UnityClassIdMap
    {
        private static readonly Type? s_unityTypeType;
        private const string UnityTypeQualifiedName = "UnityEditor.UnityType, UnityEditor.CoreModule";

        private const BindingFlags StaticFlags =
            BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
        private const BindingFlags InstanceFlags =
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        private static readonly MethodInfo? s_findByName;
        private static readonly MethodInfo? s_findById;
        private static readonly PropertyInfo? s_persistentIdProp;
        private static readonly PropertyInfo? s_nameProp;

        private static Dictionary<string, Type>? s_nameToType;

        static UnityClassIdMap()
        {
            s_unityTypeType = Type.GetType(UnityTypeQualifiedName);
            if (s_unityTypeType == null) return;

            s_findByName = s_unityTypeType.GetMethod("FindTypeByName", StaticFlags);
            s_findById = s_unityTypeType.GetMethod("FindTypeByPersistentTypeID", StaticFlags);

            s_persistentIdProp = s_unityTypeType.GetProperty("persistentTypeID", InstanceFlags);
            s_nameProp = s_unityTypeType.GetProperty("name", InstanceFlags);
        }

        public static bool TryGetClassId(Type type, out int classId)
        {
            classId = default;

            if (s_findByName == null || s_persistentIdProp == null) return false;
            
            var unityType = s_findByName.Invoke(null, new object[] { type.Name });
            if (unityType == null) return false;

            classId = (int)(s_persistentIdProp.GetValue(unityType) ?? 0);
            return classId != 0;
        }
        
        public static bool TryGetType(int classId, [NotNullWhen(true)] out Type? type)
        {
            type = null;

            if (s_findById == null || s_nameProp == null) return false;

            var unityType = s_findById.Invoke(null, new object[] { classId });
            if (unityType == null) return false;

            var name = s_nameProp.GetValue(unityType);
            if (name is not string nameString) return false;

            if (s_nameToType == null)
            {
                s_nameToType = new();
                var objectTypes = TypeCache.GetTypesDerivedFrom<UnityEngine.Object>();  
                foreach (var objectType in objectTypes)  
                {
                    s_nameToType[objectType.Name] = objectType;
                }
            }

            return s_nameToType.TryGetValue(nameString, out type);
        }
    }
    #endif
}