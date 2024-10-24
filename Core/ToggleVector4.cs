using System;
using UnityEngine;

#if UNITY_EDITOR
using net.narazaka.avatarmenucreator.valuecurve;
#endif

namespace net.narazaka.avatarmenucreator
{
    [Serializable]
    public class ToggleVector4 : ToggleItemBase<Vector4>, IToggleComplexItem
    {
#if UNITY_EDITOR
        public INamedAnimationToggleCurve ComplexAnimationToggleCurve(string prefix) => new Vector4ToggleCurve(prefix, Inactive, Active, TransitionOffsetPercent, TransitionDurationPercent);
#endif
    }
}
