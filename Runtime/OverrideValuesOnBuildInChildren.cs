#nullable enable

using System.Collections.Generic;
using UnityEngine;

namespace Narazaka.VRChat.OverrideValuesOnBuild
{
    [AddComponentMenu("OverrideValuesOnBuildInChildren")]
    public class OverrideValuesOnBuildInChildren : OverrideValuesOnBuildBase
    {
        [SerializeField] public GameObject? rootObject;
        [SerializeField] public SerializableUnityType targetType = new();
        [SerializeField] public OverrideValue[] overrideValues = new OverrideValue[0];

        public override IEnumerable<Component> GetTargets()
        {
#if UNITY_EDITOR
            var type = targetType.ToType();
            if (rootObject == null || type == null || !type.IsSubclassOf(typeof(Component)))
            {
                yield break;
            }
            foreach (var component in rootObject.GetComponentsInChildren(type, true))
            {
                yield return component;
            }
#endif
        }

        public override OverrideValue[] OverrideValues => overrideValues;
    }
}
