using System;
using UnityEngine;
using net.narazaka.avatarmenucreator.animationcurve;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace net.narazaka.avatarmenucreator
{
    [Serializable]
    public class ToggleVector3 : System.IEquatable<ToggleVector3>
    {
        public Vector3 Inactive;
        public Vector3 Active;
        public float TransitionOffsetPercent;
        public float TransitionDurationPercent = 100;
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
        public ToggleVector3 ResetAdvanced()
        {
            OmitInactive = OmitActive = false;
            return this;
        }

        public bool Equals(ToggleVector3 other)
        {
            return Inactive == other.Inactive && Active == other.Active && TransitionOffsetPercent == other.TransitionOffsetPercent && TransitionDurationPercent == other.TransitionDurationPercent && UseInactive == other.UseInactive && UseActive == other.UseActive;
        }

        public string ChangedProp(ToggleVector3 other)
        {
            if (Inactive != other.Inactive) return nameof(Inactive);
            if (Active != other.Active) return nameof(Active);
            if (TransitionOffsetPercent != other.TransitionOffsetPercent) return nameof(TransitionOffsetPercent);
            if (TransitionDurationPercent != other.TransitionDurationPercent) return nameof(TransitionDurationPercent);
            if (UseInactive != other.UseInactive) return nameof(UseInactive);
            if (UseActive != other.UseActive) return nameof(UseActive);
            return "";
        }

        public object GetProp(string name)
        {
            if (name == nameof(Inactive)) return Inactive;
            if (name == nameof(Active)) return Active;
            if (name == nameof(TransitionOffsetPercent)) return TransitionOffsetPercent;
            if (name == nameof(TransitionDurationPercent)) return TransitionDurationPercent;
            if (name == nameof(UseInactive)) return UseInactive;
            if (name == nameof(UseActive)) return UseActive;
            return 0;
        }

        public ToggleVector3 SetProp(string name, object value)
        {
            if (name == nameof(Inactive)) Inactive = (Vector3)value;
            if (name == nameof(Active)) Active = (Vector3)value;
            if (name == nameof(TransitionOffsetPercent)) TransitionOffsetPercent = (float)value;
            if (name == nameof(TransitionDurationPercent)) TransitionDurationPercent = (float)value;
            if (name == nameof(UseInactive)) UseInactive = (bool)value;
            if (name == nameof(UseActive)) UseActive = (bool)value;
            return this;
        }

        public float TransitionOffsetRate { get => TransitionOffsetPercent / 100f; }
        public float TransitionDurationRate { get => TransitionDurationPercent / 100f; }
        public float ActivateStartRate { get => TransitionOffsetRate; }
        public float ActivateEndRate { get => TransitionOffsetRate + TransitionDurationRate; }
        public float InactivateStartRate { get => 1f - ActivateEndRate; }
        public float InactivateEndRate { get => 1f - ActivateStartRate; }
        public bool NeedActivateEndKey { get => 1f - ActivateEndRate >= 1f / 60; }
        public bool NeedInactivateEndKey { get => 1f - InactivateEndRate >= 1f / 60; }

#if UNITY_EDITOR
        public NamedAnimationCurve[] ActiveCurve(string prefix) => new Vector3AnimationCurve(new Vector3Keyframe(0 / 60.0f, Active)).GetCurves(prefix);
        public NamedAnimationCurve[] InactiveCurve(string prefix) => new Vector3AnimationCurve(new Vector3Keyframe(0 / 60.0f, Inactive)).GetCurves(prefix);
        public NamedAnimationCurve[] ActivateCurve(string prefix, float transitionSeconds)
        {
            var curve = new Vector3AnimationCurve(new Vector3Keyframe(transitionSeconds * ActivateStartRate, Inactive), new Vector3Keyframe(transitionSeconds * ActivateEndRate, Active));
            if (NeedActivateEndKey) curve.AddKey(transitionSeconds, Active);
            return NamedAnimationCurve.SetTangentModes(curve.GetCurves(prefix));
        }
        public NamedAnimationCurve[] InactivateCurve(string prefix, float transitionSeconds)
        {
            var curve = new Vector3AnimationCurve(new Vector3Keyframe(transitionSeconds * InactivateStartRate, Active), new Vector3Keyframe(transitionSeconds * InactivateEndRate, Inactive));
            if (NeedInactivateEndKey) curve.AddKey(transitionSeconds, Inactive);
            return NamedAnimationCurve.SetTangentModes(curve.GetCurves(prefix));
        }
#endif
    }
}
