using System;
using UnityEngine;
using net.narazaka.avatarmenucreator.value;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace net.narazaka.avatarmenucreator
{
    [Serializable]
    public class ToggleValue : IEquatable<ToggleValue>
    {
        public Value Inactive { get => InactiveValue; set => InactiveValue = value; }
        public Value Active { get => ActiveValue; set => ActiveValue = value; }
        public Value InactiveValue;
        public Value ActiveValue;
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

        public string ChangedProp(ToggleValue other)
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
            if (name == nameof(UseInactive)) return UseInactive ? 1 : 0;
            if (name == nameof(UseActive)) return UseActive ? 1 : 0;
            return 0;
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

        public float TransitionOffsetRate { get => TransitionOffsetPercent / 100f; }
        public float TransitionDurationRate { get => TransitionDurationPercent / 100f; }
        public float ActivateStartRate { get => TransitionOffsetRate; }
        public float ActivateEndRate { get => TransitionOffsetRate + TransitionDurationRate; }
        public float InactivateStartRate { get => 1f - ActivateEndRate; }
        public float InactivateEndRate { get => 1f - ActivateStartRate; }
        public bool NeedActivateEndKey { get => 1f - ActivateEndRate >= 1f / 60; }
        public bool NeedInactivateEndKey { get => 1f - InactivateEndRate >= 1f / 60; }

#if UNITY_EDITOR
        /*
        public AnimationCurve ActiveCurve() => new AnimationCurve(new Keyframe(0 / 60.0f, Active));
        public AnimationCurve InactiveCurve() => new AnimationCurve(new Keyframe(0 / 60.0f, Inactive));
        public AnimationCurve ActivateCurve(float transitionSeconds)
        {
            var curve = new AnimationCurve(new Keyframe(transitionSeconds * ActivateStartRate, Inactive), new Keyframe(transitionSeconds * ActivateEndRate, Active));
            if (NeedActivateEndKey) curve.AddKey(transitionSeconds, Active);
            AnimationUtility.SetKeyLeftTangentMode(curve, 0, AnimationUtility.TangentMode.Constant);
            AnimationUtility.SetKeyRightTangentMode(curve, 0, AnimationUtility.TangentMode.Linear);
            AnimationUtility.SetKeyLeftTangentMode(curve, 1, AnimationUtility.TangentMode.Linear);
            AnimationUtility.SetKeyRightTangentMode(curve, 1, AnimationUtility.TangentMode.Constant);
            return curve;
        }
        public AnimationCurve InactivateCurve(float transitionSeconds)
        {
            var curve = new AnimationCurve(new Keyframe(transitionSeconds * InactivateStartRate, Active), new Keyframe(transitionSeconds * InactivateEndRate, Inactive));
            if (NeedInactivateEndKey) curve.AddKey(transitionSeconds, Inactive);
            AnimationUtility.SetKeyLeftTangentMode(curve, 0, AnimationUtility.TangentMode.Constant);
            AnimationUtility.SetKeyRightTangentMode(curve, 0, AnimationUtility.TangentMode.Linear);
            AnimationUtility.SetKeyLeftTangentMode(curve, 1, AnimationUtility.TangentMode.Linear);
            AnimationUtility.SetKeyRightTangentMode(curve, 1, AnimationUtility.TangentMode.Constant);
            return curve;
        }
        */
#endif
    }
}
