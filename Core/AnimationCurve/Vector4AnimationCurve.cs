using System;
#if UNITY_EDITOR
using UnityEngine;

namespace net.narazaka.avatarmenucreator.animationcurve
{
    [Serializable]
    public class Vector4AnimationCurve : ComplexAnimationCurve<Vector4, Vector4Keyframe>
    {
        static string[] PropertyNames = new string[] { "x", "y", "z", "w" };

        public Vector4AnimationCurve() : base() { }
        public Vector4AnimationCurve(params Vector4Keyframe[] keyframes) : base(keyframes) { }

        protected override int ComponentCount => 4;

        protected override string PropertyName(int index) => PropertyNames[index];
    }
}
#endif
