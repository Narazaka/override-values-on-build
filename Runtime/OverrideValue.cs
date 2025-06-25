using System;

namespace Narazaka.VRChat.OverrideValuesOnBuild
{
    [Serializable]
    public class OverrideValue
    {
        public string propertyPath;
        public int propertyType;
        public string value;
        public UnityEngine.Object target;
    }
}
