using nadena.dev.modular_avatar.core;
using UnityEditor.Animations;
using UnityEngine;
using UnityEditor;
using VRC.SDK3.Avatars.ScriptableObjects;
using System.Linq;
using System.Collections.Generic;
using System;

namespace net.narazaka.avatarmenucreator.editor
{
    public abstract class RestoreAvatarMenuBase<T> where T : AvatarMenuBase
    {
        public static AvatarMenuBase RestoreAssets(GameObject go)
        {
            var installer = go.GetComponent<ModularAvatarMenuInstaller>();
            var mergeAnimator = go.GetComponent<ModularAvatarMergeAnimator>();
            var parameters = go.GetComponent<ModularAvatarParameters>();
            if (parameters == null || parameters.parameters.Count != 1 || installer == null || installer.menuToAppend == null || mergeAnimator == null || mergeAnimator.animator as AnimatorController == null)
            {
                return null;
            }
            var parameterConfig = parameters.parameters[0];
            switch (parameterConfig.syncType)
            {
                case ParameterSyncType.Bool:
                    return new RestoreAvatarToggleMenu(go).RestoreAssets();
                case ParameterSyncType.Int:
                    return new RestoreAvatarChooseMenu(go).RestoreAssets();
                case ParameterSyncType.Float:
                    return new RestoreAvatarRadialMenu(go).RestoreAssets();
                default:
                    return null;
            }
        }

        protected ParameterConfig ParameterConfig;
        protected AnimatorController Animator;
        protected VRCExpressionsMenu Menu;

        public RestoreAvatarMenuBase(GameObject go) : this(
            go.GetComponent<ModularAvatarParameters>().parameters[0],
            go.GetComponent<ModularAvatarMergeAnimator>().animator as AnimatorController,
            go.GetComponent<ModularAvatarMenuInstaller>().menuToAppend
            )
        {
        }

        public RestoreAvatarMenuBase(ParameterConfig parameterConfig, AnimatorController animator, VRCExpressionsMenu menu)
        {
            ParameterConfig = parameterConfig;
            Animator = animator;
            Menu = menu;
        }

        public abstract T RestoreAssets();

        public virtual void CheckAssets()
        {
            Assert(Animator.parameters[0].name == ParameterName, "Animatorのパラメーター名とMA Parametersのパラメーター名が一致するべきです");
            Assert(Menu.controls.Count == 1, "VRCExpressionsMenuのコントロールが1つであるべきです");
            Assert(Animator.parameters.Length >= 1, "Animatorのパラメーターがあるべきです");
            Assert(Animator.layers.Length >= 1, "Animatorのレイヤーがあるべきです");
        }

        public virtual void StrictCheckAssets()
        {
            CheckAssets();
            Assert(Animator.parameters.Length == 1, "Animatorのパラメーターが1つであるべきです");
            Assert(Animator.layers.Length == 1, "Animatorのレイヤーが1つであるべきです");
        }

        public bool SafeCheckAssets()
        {
            try
            {
                CheckAssets();
            }
            catch (System.ArgumentException)
            {
                return false;
            }
            return true;
        }

        public bool SafeStrictCheckAssets()
        {
            try
            {
                StrictCheckAssets();
            }
            catch (System.ArgumentException)
            {
                return false;
            }
            return true;
        }

        protected void Assert(bool condition, string message)
        {
            if (!condition) throw new AssertException(message);
        }

        protected string ParameterName => ParameterConfig.nameOrPrefix;
        protected string ObjectName => Animator.layers.Length == 0 ? "__Object_name_not_found__" : Animator.layers[0].name;
        protected string StoreParameterName => ParameterName == ObjectName ? null : ParameterName;
        protected string WithName(object postfix = null) => postfix == null ? ObjectName : $"{ObjectName}_{postfix}";
        protected VRCExpressionsMenu.Control MenuControl => Menu.controls[0];
        protected AnimatorStateMachine StateMachine => Animator.layers[0].stateMachine;
        protected IEnumerable<AnimatorState> States => StateMachine.states.Select(s => s.state);
        protected AnimatorState State(object postfix = null)
        {
            var name = WithName(postfix);
            return States.FirstOrDefault(s => s.name == name);
        }
        protected AnimationClip Clip(object postfix = null) => State(postfix)?.motion as AnimationClip;
        protected AnimatorStateTransition AnyTransition(object postfix = null)
        {
            var name = WithName(postfix);
            return StateMachine.anyStateTransitions.FirstOrDefault(t => t.destinationState.name == name);
        }
        protected AnimatorStateTransition[] Transitions(object postfix = null) => State(postfix).transitions;
        protected AnimatorStateTransition Transition(object postfix = null, object destinationPostfix = null)
        {
            var destination = WithName(destinationPostfix);
            return Transitions(postfix).FirstOrDefault(t => t.destinationState.name == destination);
        }
        protected IEnumerable<AnimatorStateTransition> Transitions(object postfix, object destinationPostfix)
        {
            var destination = WithName(destinationPostfix);
            return Transitions(postfix).Where(t => t.destinationState.name == destination);
        }
        protected enum Postfix
        {
            active,
            inactive,
            activate,
            inactivate,
        }

        protected struct BindingInfo : IEquatable<BindingInfo>
        {
            public string path;
            public System.Type type;
            public string propertyName;

            bool IEquatable<BindingInfo>.Equals(BindingInfo other) => path == other.path && type == other.type && propertyName == other.propertyName;
            
            public override int GetHashCode() => path.GetHashCode() ^ type.GetHashCode() ^ propertyName.GetHashCode();

            public bool IsGameObjectActive => type == typeof(GameObject) && propertyName == "m_IsActive";
            public bool IsBlendShape => type == typeof(SkinnedMeshRenderer) && propertyName.StartsWith("blendShape.");
            public string BlendShapeName => propertyName.Substring("blendShape.".Length);
            public bool IsShaderParameter => (type == typeof(Renderer) || type == typeof(SkinnedMeshRenderer)) && propertyName.StartsWith("material.");
            public string ShaderParameterName => propertyName.Substring("material.".Length);
            public bool IsMaterial => (type == typeof(Renderer) || type == typeof(SkinnedMeshRenderer)) && MaterialIndexRegex.IsMatch(propertyName);
            public int MaterialIndex => int.Parse(MaterialIndexRegex.Match(propertyName).Groups[1].Value);

            static System.Text.RegularExpressions.Regex MaterialIndexRegex = new System.Text.RegularExpressions.Regex(@"m_Materials.Array.data\[(\d+)\]");
        }

        protected class BindingGroup
        {
            public string ObjectName;
            public Dictionary<string, AnimationCurve> curves = new Dictionary<string, AnimationCurve>();
            public Dictionary<string, ObjectReferenceKeyframe[]> objectReferenceCurves = new Dictionary<string, ObjectReferenceKeyframe[]>();

            public void AddCurve(string name, AnimationCurve curve)
            {
                curves.Add(name, curve);
            }

            public void AddObjectReferenceCurve(string name, ObjectReferenceKeyframe[] curve)
            {
                objectReferenceCurves.Add(name, curve);
            }

            public AnimationCurve GetCurve(object postfix)
            {
                return curves[WithName(postfix)];
            }

            public ObjectReferenceKeyframe[] GetObjectReferenceCurve(object postfix)
            {
                return objectReferenceCurves[WithName(postfix)];
            }

            protected string WithName(object postfix = null) => postfix == null ? ObjectName : $"{ObjectName}_{postfix}";
        }

        protected Dictionary<BindingInfo, BindingGroup> MakeBindingGroups(params object[] postfixes)
        {
            var bindingGroups = new Dictionary<BindingInfo, BindingGroup>();
            foreach (var postfix in postfixes)
            {
                var name = WithName(postfix);
                var bindings = AnimationUtility.GetCurveBindings(Clip(postfix));
                var objectReferenceBindings = AnimationUtility.GetObjectReferenceCurveBindings(Clip(postfix));
                foreach (var binding in bindings)
                {
                    var info = new BindingInfo { path = binding.path, type = binding.type, propertyName = binding.propertyName };
                    if (!bindingGroups.TryGetValue(info, out var bindingGroup))
                    {
                        bindingGroups[info] = bindingGroup = new BindingGroup { ObjectName = ObjectName };
                    }
                    bindingGroup.AddCurve(name, AnimationUtility.GetEditorCurve(Clip(postfix), binding));
                }
                foreach (var binding in objectReferenceBindings)
                {
                    var info = new BindingInfo { path = binding.path, type = binding.type, propertyName = binding.propertyName };
                    if (!bindingGroups.TryGetValue(info, out var bindingGroup))
                    {
                        bindingGroups[info] = bindingGroup = new BindingGroup { ObjectName = ObjectName };
                    }
                    bindingGroup.AddObjectReferenceCurve(name, AnimationUtility.GetObjectReferenceCurve(Clip(postfix), binding));
                }
            }
            return bindingGroups;
        }
    }
}
