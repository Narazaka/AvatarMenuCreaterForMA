using System;
#if UNITY_EDITOR
using UnityEngine;
using VRC.Dynamics;

namespace net.narazaka.avatarmenucreator.animationcurve
{
    [Serializable]
    public class PermissionFilterAnimationCurve : ComplexAnimationCurve<VRCPhysBoneBase.PermissionFilter, PermissionFilterKeyframe>
    {
        static string[] PropertyNames = new string[] { "allowSelf", "allowOthers" };

        public PermissionFilterAnimationCurve() : base() { }
        public PermissionFilterAnimationCurve(params PermissionFilterKeyframe[] keyframes) : base(keyframes) { }

        protected override int ComponentCount => 2;

        protected override string PropertyName(int index) => PropertyNames[index];
    }
}
#endif
