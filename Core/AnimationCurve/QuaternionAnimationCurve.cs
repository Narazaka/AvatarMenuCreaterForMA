using System;
#if UNITY_EDITOR
using UnityEngine;

namespace net.narazaka.avatarmenucreator.animationcurve
{
    [Serializable]
    public class QuaternionAnimationCurve : ComplexAnimationCurve<Quaternion, QuaternionKeyframe>
    {
        static string[] PropertyNames = new string[] { "x", "y", "z", "w" };

        public QuaternionAnimationCurve() : base() { }
        public QuaternionAnimationCurve(params QuaternionKeyframe[] keyframes) : base(keyframes) { }

        protected override int ComponentCount => 4;

        protected override string PropertyName(int index) => PropertyNames[index];
    }
}
#endif
