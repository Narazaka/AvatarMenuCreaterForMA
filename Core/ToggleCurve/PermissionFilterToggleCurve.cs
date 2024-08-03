#if UNITY_EDITOR
using net.narazaka.avatarmenucreator.animationcurve;
using VRC.Dynamics;

namespace net.narazaka.avatarmenucreator.valuecurve
{
    public class PermissionFilterToggleCurve : DiscreteComplexToggleCurve<VRCPhysBoneBase.PermissionFilter, PermissionFilterAnimationCurve, PermissionFilterKeyframe>
    {
        public PermissionFilterToggleCurve(string prefix, VRCPhysBoneBase.PermissionFilter inactive, VRCPhysBoneBase.PermissionFilter active, float transitionOffsetPercent) : base(prefix, inactive, active, transitionOffsetPercent) { }
    }
}
#endif
