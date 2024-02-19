using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace net.narazaka.avatarmenucreator
{
    [Serializable]
    public struct ToggleMaterial : System.IEquatable<ToggleMaterial>
    {
        public Material Inactive;
        public Material Active;
        public float TransitionOffsetPercent;

        public bool Equals(ToggleMaterial other)
        {
            return Inactive == other.Inactive && Active == other.Active && TransitionOffsetPercent == other.TransitionOffsetPercent;
        }

        public string ChangedProp(ToggleMaterial other)
        {
            if (Inactive != other.Inactive) return nameof(Inactive);
            if (Active != other.Active) return nameof(Active);
            if (TransitionOffsetPercent != other.TransitionOffsetPercent) return nameof(TransitionOffsetPercent);
            return "";
        }

        public object GetProp(string name)
        {
            if (name == nameof(Inactive)) return Inactive;
            if (name == nameof(Active)) return Active;
            if (name == nameof(TransitionOffsetPercent)) return TransitionOffsetPercent;
            return null;
        }

        public ToggleMaterial SetProp(string name, object value)
        {
            if (name == nameof(Inactive)) Inactive = value as Material;
            if (name == nameof(Active)) Active = value as Material;
            if (name == nameof(TransitionOffsetPercent)) TransitionOffsetPercent = (float)value;
            return this;
        }

        public float TransitionOffsetRate { get => TransitionOffsetPercent / 100f; }
        public float ActivateChangeRate { get => TransitionOffsetRate; }
        public float InactivateChangeRate { get => 1f - TransitionOffsetRate; }
        public bool NeedActivateStartKey { get => ActivateChangeRate > 0; }
        public bool NeedActivateEndKey { get => 1f - ActivateChangeRate > 0; }
        public bool NeedInactivateStartKey { get => InactivateChangeRate > 0; }
        public bool NeedInactivateEndKey { get => 1f - InactivateChangeRate > 0; }

#if UNITY_EDITOR
        public ObjectReferenceKeyframe[] ActiveCurve() => new ObjectReferenceKeyframe[] { new ObjectReferenceKeyframe { time = 0, value = Active } };
        public ObjectReferenceKeyframe[] InactiveCurve() => new ObjectReferenceKeyframe[] { new ObjectReferenceKeyframe { time = 0, value = Inactive } };
        public ObjectReferenceKeyframe[] ActivateCurve(float transitionSeconds)
        {
            var curve = new List<ObjectReferenceKeyframe>();
            if (NeedActivateStartKey) curve.Add(new ObjectReferenceKeyframe { time = 0, value = Inactive });
            curve.Add(new ObjectReferenceKeyframe { time = transitionSeconds * ActivateChangeRate, value = Active });
            if (NeedActivateEndKey) curve.Add(new ObjectReferenceKeyframe { time = transitionSeconds, value = Active });
            return curve.ToArray();
        }
        public ObjectReferenceKeyframe[] InactivateCurve(float transitionSeconds)
        {
            var curve = new List<ObjectReferenceKeyframe>();
            if (NeedInactivateStartKey) curve.Add(new ObjectReferenceKeyframe { time = 0, value = Active });
            curve.Add(new ObjectReferenceKeyframe { time = transitionSeconds * InactivateChangeRate, value = Inactive });
            if (NeedInactivateEndKey) curve.Add(new ObjectReferenceKeyframe { time = transitionSeconds, value = Inactive });
            return curve.ToArray();
        }
#endif
    }
}
