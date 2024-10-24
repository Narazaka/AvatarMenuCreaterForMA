using System;
using UnityEngine;
using net.narazaka.avatarmenucreator.animationcurve;
using net.narazaka.avatarmenucreator.valuecurve;


#if UNITY_EDITOR
using UnityEditor;
#endif

namespace net.narazaka.avatarmenucreator
{
    [Serializable]
    public class ToggleVector3 : ToggleItemBase<Vector3>, IToggleComplexItem
    {
#if UNITY_EDITOR
        public INamedAnimationToggleCurve ComplexAnimationToggleCurve(string prefix) => new Vector3ToggleCurve(prefix, Inactive, Active, TransitionOffsetPercent, TransitionDurationPercent);
#endif
    }
}
