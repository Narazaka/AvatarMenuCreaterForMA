using System;
using UnityEngine;
using VRC.Dynamics;

namespace net.narazaka.avatarmenucreator.value
{
    [Serializable]
    public sealed class PermissionFilterValue : Value
    {
        public PermissionFilterValue(VRCPhysBoneBase.PermissionFilter value) : base(new float[] { value.allowSelf ? 1 : 0, value.allowOthers ? 1 : 0 }) { }
        public static implicit operator VRCPhysBoneBase.PermissionFilter(PermissionFilterValue value) => value != null && value.value.Length >= 2 ? new VRCPhysBoneBase.PermissionFilter { allowSelf = value.value[0] != 0, allowOthers = value.value[1] != 0 } : new VRCPhysBoneBase.PermissionFilter();
    }
}
