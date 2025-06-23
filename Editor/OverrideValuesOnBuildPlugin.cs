using nadena.dev.ndmf;
using UnityEditor;
using Narazaka.VRChat.OverrideValuesOnBuild.Editor.SerializedHandlers;

[assembly: ExportsPlugin(typeof(Narazaka.VRChat.OverrideValuesOnBuild.Editor.OverrideValuesOnBuildPlugin))]

namespace Narazaka.VRChat.OverrideValuesOnBuild.Editor
{

    class OverrideValuesOnBuildPlugin : Plugin<OverrideValuesOnBuildPlugin>
    {
        public override string QualifiedName => "net.narazaka.vrchat.override-values-on-build";

        protected override void Configure()
        {
            InPhase(BuildPhase.Resolving).BeforePlugin("nadena.dev.modular-avatar").Run(DisplayName, Run);
        }

        void Run(BuildContext ctx)
        {
            var ovs = ctx.AvatarRootObject.GetComponentsInChildren<OverrideValuesOnBuild>(true);
            foreach (var ov in ovs)
            {
                if (ov.isActiveAndEnabled)
                {
                    if (ov.target == null || ov.overrideValues == null || ov.overrideValues.Length == 0)
                    {
                        continue;
                    }
                    var so = new SerializedObject(ov.target);
                    so.Update();
                    foreach (var overrideValue in ov.overrideValues)
                    {
                        SerializedValueAccessor.SetValue(so.FindProperty(overrideValue.propertyPath), SerializedJsonValue.Deserialize((SerializedPropertyType)System.Enum.ToObject(typeof(SerializedPropertyType), overrideValue.propertyType), overrideValue.value));
                    }
                    so.ApplyModifiedPropertiesWithoutUndo();
                }
                UnityEngine.Object.DestroyImmediate(ov);
            }

        }
    }
}
