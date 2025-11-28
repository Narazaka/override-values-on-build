#nullable enable

using System;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

// ScriptableObject is not supported
[Serializable]
public class SerializableUnityType
{
    public int ClassID;
    public string ScriptGuid;
    public ulong ScriptFileID;

    public SerializableUnityType()
    {
        ClassID = 0;
        ScriptGuid = string.Empty;
        ScriptFileID = 0;
    }

    const int MonoScriptClassID = 114;
    const int MonoScriptIdentifierType = 1;

#if UNITY_EDITOR
    public Type? ToType()
    {
        // User Script
        if (ClassID == MonoScriptClassID)
        {
            if (!string.IsNullOrEmpty(ScriptGuid))
            {
                return GetMonoScriptFromGuid(ScriptGuid, ScriptFileID)?.GetClass();
            }
            return null;
        }
        // Native Class
        else
        {
            return UnityClassIdMap.TryGetType(ClassID, out var type) ? type : null;
        }
    }

    private static MonoScript? GetMonoScriptFromGuid(string guid, ulong fileid)
    {
        var idString = $"GlobalObjectId_V1-{MonoScriptIdentifierType}-{guid}-{fileid}-0";
        if (GlobalObjectId.TryParse(idString, out var id))
        {
            var obj = GlobalObjectId.GlobalObjectIdentifierToObjectSlow(id) as MonoScript;
            return obj;
        }
        return null;
    }

    public static SerializableUnityType Create(Type type)
    {
        var data = new SerializableUnityType();
                
        if (type.IsSubclassOf(typeof(MonoBehaviour)))
        {
            data.ClassID = MonoScriptClassID;
            var monoScript = FindMonoScript(type);
            if (monoScript != null)
            {
                var globalId = GlobalObjectId.GetGlobalObjectIdSlow(monoScript);
                if (globalId.identifierType == MonoScriptIdentifierType)
                {
                    data.ScriptGuid = globalId.assetGUID.ToString();
                    data.ScriptFileID = globalId.targetObjectId;
                }
            }
            else
            {
                data.ScriptGuid = string.Empty;
                data.ScriptFileID = 0;
            }
            return data;    
        }
        else if (UnityClassIdMap.TryGetClassId(type, out int id))
        {
            data.ClassID = id;
            return data;
        }
        else
        {
            throw new ArgumentException($"SerializableUnityType does not support type: {type.FullName}");
        }
    }

    private static MonoScript? FindMonoScript(Type type)
    {
        var guids = AssetDatabase.FindAssets($"t:MonoScript {type.Name}");
        foreach (var guid in guids)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var script = AssetDatabase.LoadAssetAtPath<MonoScript>(path);
            if (script != null && script.GetClass() == type)
            {
                return script;
            }
        }
        return null;
    }
#endif
}

#if UNITY_EDITOR
internal static class UnityClassIdMap
{
    // ref https://docs.unity3d.com/2022.3/Documentation/Manual/ClassIDReference.html
    private static readonly IReadOnlyDictionary<int, string> ClassIdToName = new Dictionary<int, string>
    {
        { 0, "Object" },
        { 1, "GameObject" },
        { 2, "Component" },
        { 3, "LevelGameManager" },
        { 4, "Transform" },
        { 5, "TimeManager" },
        { 6, "GlobalGameManager" },
        { 8, "Behaviour" },
        { 9, "GameManager" },
        { 11, "AudioManager" },
        { 13, "InputManager" },
        { 18, "EditorExtension" },
        { 19, "Physics2DSettings" },
        { 20, "Camera" },
        { 21, "Material" },
        { 23, "MeshRenderer" },
        { 25, "Renderer" },
        { 27, "Texture" },
        { 28, "Texture2D" },
        { 29, "OcclusionCullingSettings" },
        { 30, "GraphicsSettings" },
        { 33, "MeshFilter" },
        { 41, "OcclusionPortal" },
        { 43, "Mesh" },
        { 45, "Skybox" },
        { 47, "QualitySettings" },
        { 48, "Shader" },
        { 49, "TextAsset" },
        { 50, "Rigidbody2D" },
        { 53, "Collider2D" },
        { 54, "Rigidbody" },
        { 55, "PhysicsManager" },
        { 56, "Collider" },
        { 57, "Joint" },
        { 58, "CircleCollider2D" },
        { 59, "HingeJoint" },
        { 60, "PolygonCollider2D" },
        { 61, "BoxCollider2D" },
        { 62, "PhysicsMaterial2D" },
        { 64, "MeshCollider" },
        { 65, "BoxCollider" },
        { 66, "CompositeCollider2D" },
        { 68, "EdgeCollider2D" },
        { 70, "CapsuleCollider2D" },
        { 72, "ComputeShader" },
        { 74, "AnimationClip" },
        { 75, "ConstantForce" },
        { 78, "TagManager" },
        { 81, "AudioListener" },
        { 82, "AudioSource" },
        { 83, "AudioClip" },
        { 84, "RenderTexture" },
        { 86, "CustomRenderTexture" },
        { 89, "Cubemap" },
        { 90, "Avatar" },
        { 91, "AnimatorController" },
        { 93, "RuntimeAnimatorController" },
        { 94, "ShaderNameRegistry" },
        { 95, "Animator" },
        { 96, "TrailRenderer" },
        { 98, "DelayedCallManager" },
        { 102, "TextMesh" },
        { 104, "RenderSettings" },
        { 108, "Light" },
        { 109, "ShaderInclude" },
        { 110, "BaseAnimationTrack" },
        { 111, "Animation" },
        { 114, "MonoBehaviour" },
        { 115, "MonoScript" },
        { 116, "MonoManager" },
        { 117, "Texture3D" },
        { 118, "NewAnimationTrack" },
        { 119, "Projector" },
        { 120, "LineRenderer" },
        { 121, "Flare" },
        { 122, "Halo" },
        { 123, "LensFlare" },
        { 124, "FlareLayer" },
        { 126, "NavMeshProjectSettings" },
        { 128, "Font" },
        { 129, "PlayerSettings" },
        { 130, "NamedObject" },
        { 134, "PhysicMaterial" },
        { 135, "SphereCollider" },
        { 136, "CapsuleCollider" },
        { 137, "SkinnedMeshRenderer" },
        { 138, "FixedJoint" },
        { 141, "BuildSettings" },
        { 142, "AssetBundle" },
        { 143, "CharacterController" },
        { 144, "CharacterJoint" },
        { 145, "SpringJoint" },
        { 146, "WheelCollider" },
        { 147, "ResourceManager" },
        { 150, "PreloadData" },
        { 152, "MovieTexture" },
        { 153, "ConfigurableJoint" },
        { 154, "TerrainCollider" },
        { 156, "TerrainData" },
        { 157, "LightmapSettings" },
        { 158, "WebCamTexture" },
        { 159, "EditorSettings" },
        { 162, "EditorUserSettings" },
        { 164, "AudioReverbFilter" },
        { 165, "AudioHighPassFilter" },
        { 166, "AudioChorusFilter" },
        { 167, "AudioReverbZone" },
        { 168, "AudioEchoFilter" },
        { 169, "AudioLowPassFilter" },
        { 170, "AudioDistortionFilter" },
        { 171, "SparseTexture" },
        { 180, "AudioBehaviour" },
        { 181, "AudioFilter" },
        { 182, "WindZone" },
        { 183, "Cloth" },
        { 184, "SubstanceArchive" },
        { 185, "ProceduralMaterial" },
        { 186, "ProceduralTexture" },
        { 187, "Texture2DArray" },
        { 188, "CubemapArray" },
        { 191, "OffMeshLink" },
        { 192, "OcclusionArea" },
        { 193, "Tree" },
        { 195, "NavMeshAgent" },
        { 196, "NavMeshSettings" },
        { 198, "ParticleSystem" },
        { 199, "ParticleSystemRenderer" },
        { 200, "ShaderVariantCollection" },
        { 205, "LODGroup" },
        { 206, "BlendTree" },
        { 207, "Motion" },
        { 208, "NavMeshObstacle" },
        { 210, "SortingGroup" },
        { 212, "SpriteRenderer" },
        { 213, "Sprite" },
        { 214, "CachedSpriteAtlas" },
        { 215, "ReflectionProbe" },
        { 218, "Terrain" },
        { 220, "LightProbeGroup" },
        { 221, "AnimatorOverrideController" },
        { 222, "CanvasRenderer" },
        { 223, "Canvas" },
        { 224, "RectTransform" },
        { 225, "CanvasGroup" },
        { 226, "BillboardAsset" },
        { 227, "BillboardRenderer" },
        { 228, "SpeedTreeWindAsset" },
        { 229, "AnchoredJoint2D" },
        { 230, "Joint2D" },
        { 231, "SpringJoint2D" },
        { 232, "DistanceJoint2D" },
        { 233, "HingeJoint2D" },
        { 234, "SliderJoint2D" },
        { 235, "WheelJoint2D" },
        { 236, "ClusterInputManager" },
        { 237, "BaseVideoTexture" },
        { 238, "NavMeshData" },
        { 240, "AudioMixer" },
        { 241, "AudioMixerController" },
        { 243, "AudioMixerGroupController" },
        { 244, "AudioMixerEffectController" },
        { 245, "AudioMixerSnapshotController" },
        { 246, "PhysicsUpdateBehaviour2D" },
        { 247, "ConstantForce2D" },
        { 248, "Effector2D" },
        { 249, "AreaEffector2D" },
        { 250, "PointEffector2D" },
        { 251, "PlatformEffector2D" },
        { 252, "SurfaceEffector2D" },
        { 253, "BuoyancyEffector2D" },
        { 254, "RelativeJoint2D" },
        { 255, "FixedJoint2D" },
        { 256, "FrictionJoint2D" },
        { 257, "TargetJoint2D" },
        { 258, "LightProbes" },
        { 259, "LightProbeProxyVolume" },
        { 271, "SampleClip" },
        { 272, "AudioMixerSnapshot" },
        { 273, "AudioMixerGroup" },
        { 290, "AssetBundleManifest" },
        { 300, "RuntimeInitializeOnLoadManager" },
        { 310, "UnityConnectSettings" },
        { 319, "AvatarMask" },
        { 320, "PlayableDirector" },
        { 328, "VideoPlayer" },
        { 329, "VideoClip" },
        { 330, "ParticleSystemForceField" },
        { 331, "SpriteMask" },
        { 363, "OcclusionCullingData" },
        { 1001, "PrefabInstance" },
        { 1002, "EditorExtensionImpl" },
        { 1003, "AssetImporter" },
        { 1005, "Mesh3DSImporter" },
        { 1006, "TextureImporter" },
        { 1007, "ShaderImporter" },
        { 1008, "ComputeShaderImporter" },
        { 1020, "AudioImporter" },
        { 1026, "HierarchyState" },
        { 1028, "AssetMetaData" },
        { 1029, "DefaultAsset" },
        { 1030, "DefaultImporter" },
        { 1031, "TextScriptImporter" },
        { 1032, "SceneAsset" },
        { 1034, "NativeFormatImporter" },
        { 1035, "MonoImporter" },
        { 1038, "LibraryAssetImporter" },
        { 1040, "ModelImporter" },
        { 1041, "FBXImporter" },
        { 1042, "TrueTypeFontImporter" },
        { 1045, "EditorBuildSettings" },
        { 1048, "InspectorExpandedState" },
        { 1049, "AnnotationManager" },
        { 1050, "PluginImporter" },
        { 1051, "EditorUserBuildSettings" },
        { 1055, "IHVImageFormatImporter" },
        { 1101, "AnimatorStateTransition" },
        { 1102, "AnimatorState" },
        { 1105, "HumanTemplate" },
        { 1107, "AnimatorStateMachine" },
        { 1108, "PreviewAnimationClip" },
        { 1109, "AnimatorTransition" },
        { 1110, "SpeedTreeImporter" },
        { 1111, "AnimatorTransitionBase" },
        { 1112, "SubstanceImporter" },
        { 1113, "LightmapParameters" },
        { 1120, "LightingDataAsset" },
        { 1124, "SketchUpImporter" },
        { 1125, "BuildReport" },
        { 1126, "PackedAssets" },
        { 1127, "VideoClipImporter" },
        { 100000, "int" },
        { 100001, "bool" },
        { 100002, "float" },
        { 100003, "MonoObject" },
        { 100004, "Collision" },
        { 100005, "Vector3f" },
        { 100006, "RootMotionData" },
        { 100007, "Collision2D" },
        { 100008, "AudioMixerLiveUpdateFloat" },
        { 100009, "AudioMixerLiveUpdateBool" },
        { 100010, "Polygon2D" },
        { 100011, "void" },
        { 19719996, "TilemapCollider2D" },
        { 41386430, "ImportLog" },
        { 73398921, "VFXRenderer" },
        { 156049354, "Grid" },
        { 156483287, "ScenesUsingAssets" },
        { 171741748, "ArticulationBody" },
        { 181963792, "Preset" },
        { 285090594, "IConstraint" },
        { 294290339, "AssemblyDefinitionReferenceImporter" },
        { 369655926, "AssetImportInProgressProxy" },
        { 382020655, "PluginBuildInfo" },
        { 387306366, "MemorySettings" },
        { 426301858, "EditorProjectAccess" },
        { 468431735, "PrefabImporter" },
        { 483693784, "TilemapRenderer" },
        { 612988286, "SpriteAtlasAsset" },
        { 638013454, "SpriteAtlasDatabase" },
        { 641289076, "AudioBuildInfo" },
        { 644342135, "CachedSpriteAtlasRuntimeData" },
        { 662584278, "AssemblyDefinitionReferenceAsset" },
        { 668709126, "BuiltAssetBundleInfoSet" },
        { 687078895, "SpriteAtlas" },
        { 747330370, "RayTracingShaderImporter" },
        { 815301076, "PreviewImporter" },
        { 825902497, "RayTracingShader" },
        { 850595691, "LightingSettings" },
        { 877146078, "PlatformModuleSetup" },
        { 890905787, "VersionControlSettings" },
        { 893571522, "CustomCollider2D" },
        { 895512359, "AimConstraint" },
        { 937362698, "VFXManager" },
        { 947337230, "RoslynAnalyzerConfigAsset" },
        { 954905827, "RuleSetFileAsset" },
        { 994735392, "VisualEffectSubgraph" },
        { 994735403, "VisualEffectSubgraphOperator" },
        { 994735404, "VisualEffectSubgraphBlock" },
        { 1001480554, "Prefab" },
        { 1027052791, "LocalizationImporter" },
        { 1114811875, "ReferencesArtifactGenerator" },
        { 1152215463, "AssemblyDefinitionAsset" },
        { 1154873562, "SceneVisibilityState" },
        { 1183024399, "LookAtConstraint" },
        { 1210832254, "SpriteAtlasImporter" },
        { 1223240404, "MultiArtifactTestImporter" },
        { 1268269756, "GameObjectRecorder" },
        { 1325145578, "LightingDataAssetParent" },
        { 1386491679, "PresetManager" },
        { 1403656975, "StreamingManager" },
        { 1480428607, "LowerResBlitTexture" },
        { 1521398425, "VideoBuildInfo" },
        { 1541671625, "C4DImporter" },
        { 1542919678, "StreamingController" },
        { 1557264870, "ShaderContainer" },
        { 1597193336, "RoslynAdditionalFileAsset" },
        { 1642787288, "RoslynAdditionalFileImporter" },
        { 1660057539, "SceneRoots" },
        { 1731078267, "BrokenPrefabAsset" },
        { 1736697216, "AndroidAssetPackImporter" },
        { 1742807556, "GridLayout" },
        { 1766753193, "AssemblyDefinitionImporter" },
        { 1773428102, "ParentConstraint" },
        { 1777034230, "RuleSetFileImporter" },
        { 1818360608, "PositionConstraint" },
        { 1818360609, "RotationConstraint" },
        { 1818360610, "ScaleConstraint" },
        { 1839735485, "Tilemap" },
        { 1896753125, "PackageManifest" },
        { 1896753126, "PackageManifestImporter" },
        { 1903396204, "RoslynAnalyzerConfigImporter" },
        { 1953259897, "TerrainLayer" },
        { 1971053207, "SpriteShapeRenderer" },
        { 2058629509, "VisualEffectAsset" },
        { 2058629510, "VisualEffectImporter" },
        { 2058629511, "VisualEffectResource" },
        { 2059678085, "VisualEffectObject" },
        { 2083052967, "VisualEffect" },
        { 2083778819, "LocalizationAsset" },
        { 2089858483, "ScriptedImporter" },
        { 2103361453, "ShaderIncludeImporter" },
    };

    private static Dictionary<Type, int>? _typeToId;
    private static Dictionary<int, Type>? _idToType;
    private static bool _initialized;

    private static void EnsureInitialized()
    {
        if (_initialized) return;

        var typeToId = new Dictionary<Type, int>();
        var idToType = new Dictionary<int, Type>();

        foreach (var pair in ClassIdToName)
        {
            var id = pair.Key;
            var typeName = pair.Value;

            var type = ResolveUnityType(typeName);
            if (type == null)
            {
                continue;
            }

            if (!idToType.ContainsKey(id))
            {
                idToType[id] = type;
            }

            if (!typeToId.ContainsKey(type))
            {
                typeToId[type] = id;
            }
        }

        _typeToId = typeToId;
        _idToType = idToType;
        _initialized = true;
    }

    private static Type? ResolveUnityType(string typeName)
    {
        // UnityEngine 名前空間
        var engineAsm = typeof(UnityEngine.Object).Assembly;
        var type = engineAsm.GetType("UnityEngine." + typeName);
        if (type != null)
        {
            return type;
        }

        var editorAsm = typeof(UnityEditor.Editor).Assembly;
        type = editorAsm.GetType("UnityEditor." + typeName);
        if (type != null)
        {
            return type;
        }

        return null;
    }

    public static bool TryGetClassId(Type type, out int classId)
    {
        EnsureInitialized();
        if (_typeToId != null)
        {
            return _typeToId.TryGetValue(type, out classId);
        }

        classId = 0;
        return false;
    }

    public static bool TryGetType(int classId, out Type? type)
    {
        EnsureInitialized();
        if (_idToType != null)
        {
            return _idToType.TryGetValue(classId, out type);
        }

        type = null;
        return false;
    }
}
#endif