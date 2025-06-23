using System.Runtime.CompilerServices;
using UnityEngine;

[assembly: InternalsVisibleTo("Narazaka.VRChat.OverrideValuesOnBuild.Editor")]

namespace Narazaka.VRChat.OverrideValuesOnBuild
{
    [AddComponentMenu("OverrideValuesOnBuild")]
    public class OverrideValuesOnBuild : MonoBehaviour
    {
        [SerializeField] public Component target;
        [SerializeField] public OverrideValue[] overrideValues = new OverrideValue[0];
    }
}
