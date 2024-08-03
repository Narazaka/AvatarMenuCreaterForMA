using System;
#if UNITY_EDITOR
using UnityEngine;

namespace net.narazaka.avatarmenucreator.animationcurve
{
    [Serializable]
    public class Vector3AnimationCurve : ComplexAnimationCurve<Vector3, Vector3Keyframe>
    {
        static string[] PropertyNames = new string[] { "x", "y", "z" };

        public Vector3AnimationCurve() : base() { }
        public Vector3AnimationCurve(params Vector3Keyframe[] keyframes) : base(keyframes) { }

        protected override int ComponentCount => 3;

        protected override string PropertyName(int index) => PropertyNames[index];
    }
}
#endif
