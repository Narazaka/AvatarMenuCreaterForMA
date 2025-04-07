using System;
#if UNITY_EDITOR
using UnityEngine;

namespace net.narazaka.avatarmenucreator.animationcurve
{
    [Serializable]
    public class ColorAnimationCurve : ComplexAnimationCurve<Color, ColorKeyframe>
    {
        static string[] PropertyNames = new string[] { "r", "g", "b", "a" };

        public ColorAnimationCurve() : base() { }
        public ColorAnimationCurve(params ColorKeyframe[] keyframes) : base(keyframes) { }

        protected override int ComponentCount => 4;

        protected override string PropertyName(int index) => PropertyNames[index];
    }
}
#endif
