#if UNITY_EDITOR
using System;
using UnityEngine;
using System.Linq;
using UnityEditor;

namespace net.narazaka.avatarmenucreator.animationcurve
{
    public abstract class ComplexAnimationCurve<T, U> where U : ComplexKeyframe<T>, new()
    {
        protected abstract int ComponentCount { get; }
        protected abstract string PropertyName(int index);
        ComplexKeyframe<T>[] Keyframes;

        public ComplexAnimationCurve(params ComplexKeyframe<T>[] keyframes)
        {
            Keyframes = keyframes;
        }

        public void AddKey(float time, T value)
        {
            Array.Resize(ref Keyframes, Keyframes.Length + 1);
            Keyframes[Keyframes.Length - 1] = new U { Time = time, Value = value };
        }

        public NamedAnimationCurve[] GetCurves(string prefix)
        {
            var curves = new NamedAnimationCurve[ComponentCount];
            for (int i = 0; i < ComponentCount; i++)
            {
                var curve = new AnimationCurve(Keyframes.Select(keyframe => new Keyframe(keyframe.Time, keyframe.ValueComponent(i))).ToArray());
                curves[i] = new NamedAnimationCurve($"{prefix}.{PropertyName(i)}", curve);
            }
            return curves;
        }
    }
}
#endif
