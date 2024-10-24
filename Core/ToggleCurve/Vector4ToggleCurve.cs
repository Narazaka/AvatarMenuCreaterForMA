#if UNITY_EDITOR
using UnityEngine;
using net.narazaka.avatarmenucreator.animationcurve;

namespace net.narazaka.avatarmenucreator.valuecurve
{
    public class Vector4ToggleCurve : ContinuousComplexToggleCurve<Vector4, Vector4AnimationCurve, Vector4Keyframe>
    {
        public Vector4ToggleCurve(string prefix, Vector4 inactive, Vector4 active, float transitionOffsetPercent, float transitionDurationPercent) : base(prefix, inactive, active, transitionOffsetPercent, transitionDurationPercent) { }
    }
}
#endif
