#nullable enable

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using Object = UnityEngine.Object;

namespace Narazaka.VRChat.OverrideValuesOnBuild
{
    // ScriptableObject is not supported
    [Serializable]
    public class SerializableUnityType
    {
        public int ClassID;
        public string ScriptGUID;
        public ulong ScriptFileID;

        public SerializableUnityType(int classID, string scriptGUID, ulong scriptFileID)
        {
            ClassID = classID;
            ScriptGUID = scriptGUID;
            ScriptFileID = scriptFileID;
        }

        public SerializableUnityType() : this(0, string.Empty, 0) {}

#if UNITY_EDITOR
        private const int MonoScriptClassID = 114;

        public Type? ToType()
        {
            // User Script
            if (ClassID == MonoScriptClassID)
            {
                if (!string.IsNullOrEmpty(ScriptGUID))
                {
                    return GetMonoScript(ScriptGUID, ScriptFileID)?.GetClass();
                }
                return null;
            }
            // Native Class
            else
            {
                return UnityClassIdMap.TryGetType(ClassID, out var type) ? type : null;
            }
        }
        
        public static SerializableUnityType Create(Object instance)
        {
            if (instance is MonoBehaviour behaviour)
            {
                var script = MonoScript.FromMonoBehaviour(behaviour);
                var (guid, fileid) = GetGuidAndFileId(script);
                return new SerializableUnityType(MonoScriptClassID, guid, fileid);
            }
            else if (UnityClassIdMap.TryGetClassId(instance.GetType(), out int id))
            {
                return new SerializableUnityType(id, string.Empty, 0);
            }
            else
            {
                throw new ArgumentException($"SerializableUnityType does not support type: {instance.GetType().FullName}");
            }
        }

        private static MonoScript? GetMonoScript(string guid, ulong fileid)
        {
            const int MonoScriptIdentifierType = 1;
            var idString = $"GlobalObjectId_V1-{MonoScriptIdentifierType}-{guid}-{fileid}-0";
            if (!GlobalObjectId.TryParse(idString, out var id)) return null;
            return GlobalObjectId.GlobalObjectIdentifierToObjectSlow(id) as MonoScript;
        }

        private static (string guid, ulong fileid) GetGuidAndFileId(MonoScript script)
        {
            var id = GlobalObjectId.GetGlobalObjectIdSlow(script);
            return (id.assetGUID.ToString(), id.targetObjectId);
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

            var persistentId = s_persistentIdProp.GetValue(unityType);
            if (persistentId is not int persistentIdInt) return false;

            classId = persistentIdInt;
            return true;
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
                var objectTypes = TypeCache.GetTypesDerivedFrom<Object>();  
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