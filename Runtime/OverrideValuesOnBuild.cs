using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

[assembly: InternalsVisibleTo("Narazaka.VRChat.OverrideValuesOnBuild.Editor")]

namespace Narazaka.VRChat.OverrideValuesOnBuild
{
    [AddComponentMenu("OverrideValuesOnBuild")]
    public class OverrideValuesOnBuild : OverrideValuesOnBuildBase
    {
        [SerializeField] public Component target;
        [SerializeField] public OverrideValue[] overrideValues = new OverrideValue[0];

        public override IEnumerable<Component> GetTargets() { yield return target; }
        public override OverrideValue[] OverrideValues => overrideValues;
    }
}
