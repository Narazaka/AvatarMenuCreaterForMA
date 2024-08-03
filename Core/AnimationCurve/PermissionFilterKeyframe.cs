using UnityEngine;
using VRC.Dynamics;

namespace net.narazaka.avatarmenucreator.animationcurve
{
    public class PermissionFilterKeyframe : ComplexKeyframe<VRCPhysBoneBase.PermissionFilter>
    {
        public PermissionFilterKeyframe() : base() { }
        public PermissionFilterKeyframe(float time, VRCPhysBoneBase.PermissionFilter value) : base(time, value) { }

        public override float ValueComponent(int index) => index == 0 ? Value.allowSelf ? 1 : 0 : Value.allowOthers ? 1 : 0;
    }
}
