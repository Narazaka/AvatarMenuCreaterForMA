#if UNITY_EDITOR
using UnityEngine;
using net.narazaka.avatarmenucreator.animationcurve;

namespace net.narazaka.avatarmenucreator.valuecurve
{
    public class Vector3ToggleCurve : ContinuousComplexToggleCurve<Vector3, Vector3AnimationCurve, Vector3Keyframe>
    {
        public Vector3ToggleCurve(string prefix, Vector3 inactive, Vector3 active, float transitionOffsetPercent, float transitionDurationPercent) : base(prefix, inactive, active, transitionOffsetPercent, transitionDurationPercent) { }
    }
}
#endif
