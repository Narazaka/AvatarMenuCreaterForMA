#if UNITY_EDITOR
using System;
using UnityEngine;

namespace net.narazaka.avatarmenucreator.valuecurve
{
    public class ToggleAnimationClipSet
    {
        public AnimationClip Active;
        public AnimationClip Inactive;
        public AnimationClip Activate;
        public AnimationClip Inactivate;
        public float TransitionSeconds;

        public ToggleAnimationClipSet(AnimationClip active, AnimationClip inactive, AnimationClip activate, AnimationClip inactivate, float transitionSeconds)
        {
            Active = active;
            Inactive = inactive;
            Activate = activate;
            Inactivate = inactivate;
            TransitionSeconds = transitionSeconds;
        }

        public void SetupAnimationToggleCurve<T>(T value,
            string path,
            Type type,
            string propertyName
            ) where T : IToggleSingleItem, IUseActive
        {
            new AnimationToggleCurveSetup(
                curve: value.AnimationToggleCurve(),
                clipSet: this,
                path: path,
                propertyName: propertyName,
                type: type,
                useActive: value.UseActive,
                useInactive: value.UseInactive,
                transitionSeconds: TransitionSeconds
                ).Setup();
        }

        public void SetupAnimationToggleCurve<T>(T value,
            Type valueType,
            string path,
            Type type,
            string propertyName
            ) where T : IToggleTypedSingleItem, IUseActive
        {
            new AnimationToggleCurveSetup(
                curve: value.AnimationToggleCurve(valueType),
                clipSet: this,
                path: path,
                propertyName: propertyName,
                type: type,
                useActive: value.UseActive,
                useInactive: value.UseInactive,
                transitionSeconds: TransitionSeconds
                ).Setup();
        }

        public void SetupComplexAnimationToggleCurve<T>(T value,
            string path,
            Type type,
            string prefix
            ) where T : IToggleComplexItem, IUseActive
        {
            new NamedAnimationToggleCurveSetup(
                curve: value.ComplexAnimationToggleCurve(prefix),
                clipSet: this,
                path: path,
                type: type,
                useActive: value.UseActive,
                useInactive: value.UseInactive,
                transitionSeconds: TransitionSeconds
                ).Setup();
        }

        public void SetupComplexAnimationToggleCurve<T>(T value,
            Type valueType,
            string path,
            Type type,
            string prefix
            ) where T : IToggleTypedComplexItem, IUseActive
        {
            new NamedAnimationToggleCurveSetup(
                curve: value.ComplexAnimationToggleCurve(valueType, prefix),
                clipSet: this,
                path: path,
                type: type,
                useActive: value.UseActive,
                useInactive: value.UseInactive,
                transitionSeconds: TransitionSeconds
                ).Setup();
        }
    }
}
#endif
