using System.Collections.Generic;
using UnityEngine;
using VRC.SDKBase;

namespace Narazaka.VRChat.OverrideValuesOnBuild
{
    public abstract class OverrideValuesOnBuildBase : MonoBehaviour, IEditorOnly
    {
        public abstract IEnumerable<Component> GetTargets();
        public abstract OverrideValue[] OverrideValues { get; }
    }
}