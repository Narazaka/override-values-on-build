using nadena.dev.ndmf;
using UnityEditor;
using Narazaka.VRChat.OverrideValuesOnBuild.Editor.SerializedHandlers;
using System.Linq;

[assembly: ExportsPlugin(typeof(Narazaka.VRChat.OverrideValuesOnBuild.Editor.OverrideValuesOnBuildPlugin))]

namespace Narazaka.VRChat.OverrideValuesOnBuild.Editor
{

    class OverrideValuesOnBuildPlugin : Plugin<OverrideValuesOnBuildPlugin>
    {
        public override string QualifiedName => "net.narazaka.vrchat.override-values-on-build";

        protected override void Configure()
        {
            InPhase(BuildPhase.Resolving)
                .BeforePlugin("nadena.dev.modular-avatar")
                .BeforePlugin("net.narazaka.vrchat.breast_pb_adjuster")
                .BeforePlugin("net.narazaka.vrchat.animator-layer-filter")
                .BeforePlugin("net.narazaka.vrchat.ma-sync-parameter-sequence-by-id")
                .BeforePlugin("net.narazaka.vrchat.add-material-slots")
                .BeforePlugin("dev.hai-vr.prefabulous.vrc.ReplaceAnimators")
                .BeforePlugin("com.github.kurotu.vrc-quest-tools")
                .BeforePlugin("com.anatawa12.avatar-optimizer")
                .BeforePlugin("HhotateA.AvatarModifyTools.MagicalDresserInventorySystem.ModularAvatarExtension.MagicalDresserInventoryModularAvatarProcessor")
                .BeforePlugin("org.kb10uy.zatools")
                .Run(DisplayName, Run);
        }

        void Run(BuildContext ctx)
        {
            var ovs = ctx.AvatarRootObject.GetComponentsInChildren<OverrideValuesOnBuildBase>(true);
            foreach (var ov in ovs)
            {
                RunComponent(ctx, ov);
                UnityEngine.Object.DestroyImmediate(ov);
            }

        }

        void RunComponent(BuildContext ctx, OverrideValuesOnBuildBase ov)
        {
            if (!ov.isActiveAndEnabled)
            {
                return;
            }
            var targets = ov.GetTargets().ToArray();
            var overrideValues = ov.OverrideValues;
            if (targets.Length == 0 || overrideValues.Length == 0)
            {
                return;
            }
#if HAS_VQT
            var vqtGameObjectRemovers = ov.GetComponentsInParent<KRT.VRCQuestTools.Components.PlatformGameObjectRemover>();
            foreach (var vqtGameObjectRemover in vqtGameObjectRemovers)
            {
                var remove =
#if UNITY_EDITOR_WIN
                    vqtGameObjectRemover.removeOnPC;
#else
                    vqtGameObjectRemover.removeOnAndroid;
#endif
                if (remove)
                {
                    return;
                }
            }
            var vqtComponentRemover = ov.GetComponent<KRT.VRCQuestTools.Components.PlatformComponentRemover>();
            if (vqtComponentRemover != null)
            {
                var setting = vqtComponentRemover.componentSettings.FirstOrDefault(s => s.component == ov);
                var remove =
#if UNITY_EDITOR_WIN
                    setting.removeOnPC;
#else
                    setting.removeOnAndroid;
#endif
                if (remove)
                {
                    return;
                }
            }
#endif      
            foreach (var target in targets)
            {
                var so = new SerializedObject(target);
                so.Update();
                foreach (var overrideValue in overrideValues)
                {
                    if (overrideValue.propertyType == (int)SerializedPropertyType.ObjectReference && string.IsNullOrEmpty(overrideValue.value))
                    {
                        so.FindProperty(overrideValue.propertyPath).objectReferenceValue = overrideValue.target;
                    }
                    else
                    {
                        SerializedValueAccessor.SetValue(so.FindProperty(overrideValue.propertyPath), SerializedJsonValue.Deserialize((SerializedPropertyType)System.Enum.ToObject(typeof(SerializedPropertyType), overrideValue.propertyType), overrideValue.value));
                    }
                }
                so.ApplyModifiedPropertiesWithoutUndo();
            }
        }
    }
}
