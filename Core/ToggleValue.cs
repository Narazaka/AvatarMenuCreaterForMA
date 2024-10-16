using System;
using System.Collections.Generic;
using UnityEngine;
using net.narazaka.avatarmenucreator.value;
using net.narazaka.avatarmenucreator.valuecurve;
using VRC.Dynamics;

namespace net.narazaka.avatarmenucreator
{
    [Serializable]
    public struct ToggleValue : IEquatable<ToggleValue>
    {
        public Value Inactive;
        public Value Active;
        public float TransitionOffsetPercent;
        public float TransitionDurationPercent;
        [SerializeField]
        bool OmitInactive;
        [SerializeField]
        bool OmitActive;

        public bool UseInactive
        {
            get => !OmitInactive;
            set => OmitInactive = !value;
        }
        public bool UseActive
        {
            get => !OmitActive;
            set => OmitActive = !value;
        }
        public bool HasAdvanced => OmitInactive || OmitActive;
        public ToggleValue ResetAdvanced()
        {
            OmitInactive = OmitActive = false;
            return this;
        }

        public bool Equals(ToggleValue other)
        {
            return Inactive == other.Inactive && Active == other.Active && TransitionOffsetPercent == other.TransitionOffsetPercent && TransitionDurationPercent == other.TransitionDurationPercent && UseInactive == other.UseInactive && UseActive == other.UseActive;
        }

        public IEnumerable<string> ChangedProps(ToggleValue other)
        {
            var changed = new List<string>();
            if (Inactive != other.Inactive) changed.Add(nameof(Inactive));
            if (Active != other.Active) changed.Add(nameof(Active));
            if (TransitionOffsetPercent != other.TransitionOffsetPercent) changed.Add(nameof(TransitionOffsetPercent));
            if (TransitionDurationPercent != other.TransitionDurationPercent) changed.Add(nameof(TransitionDurationPercent));
            if (UseInactive != other.UseInactive) changed.Add(nameof(UseInactive));
            if (UseActive != other.UseActive) changed.Add(nameof(UseActive));
            return changed;
        }

        public object GetProp(string name)
        {
            if (name == nameof(Inactive)) return Inactive;
            if (name == nameof(Active)) return Active;
            if (name == nameof(TransitionOffsetPercent)) return TransitionOffsetPercent;
            if (name == nameof(TransitionDurationPercent)) return TransitionDurationPercent;
            if (name == nameof(UseInactive)) return UseInactive;
            if (name == nameof(UseActive)) return UseActive;
            return null;
        }

        public ToggleValue SetProp(string name, object value)
        {
            if (name == nameof(Inactive)) Inactive = (Value)value;
            if (name == nameof(Active)) Active = (Value)value;
            if (name == nameof(TransitionOffsetPercent)) TransitionOffsetPercent = (float)value;
            if (name == nameof(TransitionDurationPercent)) TransitionDurationPercent = (float)value;
            if (name == nameof(UseInactive)) UseInactive = (bool)value;
            if (name == nameof(UseActive)) UseActive = (bool)value;
            return this;
        }

        public void AdjustTransitionValues()
        {
            if (TransitionOffsetPercent < 0) TransitionOffsetPercent = 0;
            if (TransitionOffsetPercent > 100) TransitionOffsetPercent = 100;
            if (TransitionDurationPercent < 0) TransitionDurationPercent = 0;
            if (TransitionDurationPercent > 100) TransitionDurationPercent = 100;
            if (TransitionOffsetPercent + TransitionDurationPercent > 100) TransitionDurationPercent = 100 - TransitionOffsetPercent;
        }

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
            if (type == typeof(VRCPhysBoneBase.PermissionFilter)) return PermissionFilterToggleCurve(prefix);
            return null;
        }
        public Vector3ToggleCurve Vector3ToggleCurve(string prefix) => new Vector3ToggleCurve(prefix, (Vector3)Inactive, (Vector3)Active, TransitionOffsetPercent, TransitionDurationPercent);
        public PermissionFilterToggleCurve PermissionFilterToggleCurve(string prefix) => new PermissionFilterToggleCurve(prefix, (VRCPhysBoneBase.PermissionFilter)Inactive, (VRCPhysBoneBase.PermissionFilter)Active, TransitionOffsetPercent);
#endif
    }
}
