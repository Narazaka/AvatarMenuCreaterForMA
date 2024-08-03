using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace net.narazaka.avatarmenucreator.animationcurve
{
    public class NamedAnimationCurve
    {
        public string propertyName;
        public AnimationCurve curve;

        public NamedAnimationCurve(string propertyName, AnimationCurve curve)
        {
            this.propertyName = propertyName;
            this.curve = curve;
        }

#if UNITY_EDITOR
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
#endif
    }
}
