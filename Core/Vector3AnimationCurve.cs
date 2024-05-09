using System;
using UnityEngine;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;

namespace net.narazaka.avatarmenucreator
{
    [Serializable]
    public class Vector3AnimationCurve
    {
        static string[] PropertyNames = new string[] { "x", "y", "z" };
        public Vector3Keyframe[] Keyframes;

        public Vector3AnimationCurve(params Vector3Keyframe[] keyframes)
        {
            Keyframes = keyframes;
        }

        public void AddKey(float time, Vector3 value)
        {
            Array.Resize(ref Keyframes, Keyframes.Length + 1);
            Keyframes[Keyframes.Length - 1] = new Vector3Keyframe(time, value);
        }

        public NamedAnimationCurve[] GetCurves(string prefix)
        {
            var curves = new NamedAnimationCurve[3];
            for (int i = 0; i < 3; i++)
            {
                var curve = new AnimationCurve(Keyframes.Select(keyframe => new Keyframe(keyframe.Time, keyframe.Value[i])).ToArray());
                curves[i] = new NamedAnimationCurve($"{prefix}.{PropertyNames[i]}", curve);
            }
            return curves;
        }

        public static NamedAnimationCurve[] SetTangentModes(NamedAnimationCurve[] curves)
        {
            foreach (var c in curves)
            {
                AnimationUtility.SetKeyLeftTangentMode(c.curve, 0, AnimationUtility.TangentMode.Constant);
                AnimationUtility.SetKeyRightTangentMode(c.curve, 0, AnimationUtility.TangentMode.Linear);
                AnimationUtility.SetKeyLeftTangentMode(c.curve, 1, AnimationUtility.TangentMode.Linear);
                AnimationUtility.SetKeyRightTangentMode(c.curve, 1, AnimationUtility.TangentMode.Constant);
            }
            return curves;
        }
    }
}
#endif
