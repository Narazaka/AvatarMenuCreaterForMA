using System;
using UnityEngine;
using net.narazaka.avatarmenucreator.value;
using net.narazaka.avatarmenucreator.valuecurve;
using VRC.Dynamics;

namespace net.narazaka.avatarmenucreator
{
    [Serializable]
    public class ToggleValue : ToggleItemBase<Value>, IToggleTypedSingleItem, IToggleTypedComplexItem
    {
#if UNITY_EDITOR
        public IAnimationToggleCurve AnimationToggleCurve(Type type)
        {

            if (type == typeof(float)) return FloatToggleCurve();
            if (type == typeof(int) || type.IsSubclassOf(typeof(Enum))) return IntToggleCurve();
            if (type == typeof(bool)) return BoolToggleCurve();
            return null;
        }
        public IAnimationToggleCurve AnimationToggleCurve<T>() => AnimationToggleCurve(typeof(T));
        public FloatToggleCurve FloatToggleCurve() => new FloatToggleCurve((float)Inactive, (float)Active, TransitionOffsetPercent, TransitionDurationPercent);
        public IntToggleCurve IntToggleCurve() => new IntToggleCurve((int)Inactive, (int)Active, TransitionOffsetPercent);
        public BoolToggleCurve BoolToggleCurve() => new BoolToggleCurve((bool)Inactive, (bool)Active, TransitionOffsetPercent);
        public INamedAnimationToggleCurve ComplexAnimationToggleCurve(Type type, string prefix)
        {
            if (type == typeof(Vector3)) return Vector3ToggleCurve(prefix);
            if (type == typeof(Quaternion)) return QuaternionToggleCurve(prefix);
            if (type == typeof(Color)) return ColorToggleCurve(prefix);
            if (type == typeof(VRCPhysBoneBase.PermissionFilter)) return PermissionFilterToggleCurve(prefix);
            throw new NotImplementedException();
        }
        public Vector3ToggleCurve Vector3ToggleCurve(string prefix) => new Vector3ToggleCurve(prefix, (Vector3)Inactive, (Vector3)Active, TransitionOffsetPercent, TransitionDurationPercent);
        public QuaternionToggleCurve QuaternionToggleCurve(string prefix) => new QuaternionToggleCurve(prefix, (Quaternion)Inactive, (Quaternion)Active, TransitionOffsetPercent, TransitionDurationPercent);
        public ColorToggleCurve ColorToggleCurve(string prefix) => new ColorToggleCurve(prefix, (Color)Inactive, (Color)Active, TransitionOffsetPercent, TransitionDurationPercent);
        public PermissionFilterToggleCurve PermissionFilterToggleCurve(string prefix) => new PermissionFilterToggleCurve(prefix, (VRCPhysBoneBase.PermissionFilter)Inactive, (VRCPhysBoneBase.PermissionFilter)Active, TransitionOffsetPercent);
#endif
    }
}
