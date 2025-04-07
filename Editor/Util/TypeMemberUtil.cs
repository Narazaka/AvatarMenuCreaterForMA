using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using VRC.SDK3.Dynamics.PhysBone.Components;
using UnityEngine;
using UnityEngine.Animations;
#if HAS_VRC_CONSTRAINT
using VRC.SDK3.Dynamics.Constraint.Components;
#endif
using VRC.Dynamics;

namespace net.narazaka.avatarmenucreator.util
{
    public static class TypeMemberUtil
    {
        class MemberInfoContainer
        {
            public MemberInfo MemberInfo;
            public string Label;
            /// <summary>
            /// animation field name (maybe not C# field name (C name?))
            /// </summary>
            public string Field;
            /// <summary>
            /// path for nested field (must be C# field name)
            /// </summary>
            public string Path;
            public MemberInfoContainer(MemberInfo memberInfo)
            {
                MemberInfo = memberInfo;
            }
        }
        static MemberInfoContainer[] ConstraintCommonMembers(Type type) => new MemberInfoContainer[]
        {
            new MemberInfoContainer(type.GetProperty(nameof(IConstraint.constraintActive))) { Field = "m_Active", Label = "Is Active" },
            new MemberInfoContainer(type.GetProperty(nameof(IConstraint.weight))) { Field = "m_Weight" },
        };
#if HAS_VRC_CONSTRAINT
        static MemberInfoContainer[] VRCConstraintCommonMembers(Type type) => new MemberInfoContainer[]
        {
            new MemberInfoContainer(type.GetField(nameof(VRCConstraintBase.IsActive))),
            new MemberInfoContainer(type.GetField(nameof(VRCConstraintBase.GlobalWeight))) { Label = "Weight" },
            new MemberInfoContainer(type.GetField(nameof(VRCConstraintBase.SolveInLocalSpace))),
            new MemberInfoContainer(type.GetField(nameof(VRCConstraintBase.FreezeToWorld))),
            new MemberInfoContainer(type.GetField(nameof(VRCConstraintBase.RebakeOffsetsWhenUnfrozen))),
        }.Concat(Enumerable.Range(0, VRCConstraintSourceKeyableList.MaxFlatLength).Select(index =>
            new MemberInfoContainer(typeof(VRCConstraintSource).GetField(nameof(VRCConstraintSource.Weight))) { Path = $"Sources.source{index}.Weight", Label = $"Sources[{index}].Weight" }
        )).ToArray();
#endif
        static Dictionary<Type, MemberInfoContainer[]> AvailableMembers = new Dictionary<Type, MemberInfoContainer[]>
        {
            {
                typeof(VRCPhysBone), new MemberInfoContainer[]
                {
                    //new (typeof(VRCPhysBone).GetField(nameof(VRCPhysBone.integrationType))),
                    new MemberInfoContainer(typeof(VRCPhysBone).GetField(nameof(VRCPhysBone.pull))),
                    new MemberInfoContainer(typeof(VRCPhysBone).GetField(nameof(VRCPhysBone.spring))) { Label ="Spring / Momentum" },
                    new MemberInfoContainer(typeof(VRCPhysBone).GetField(nameof(VRCPhysBone.stiffness))),
                    new MemberInfoContainer(typeof(VRCPhysBone).GetField(nameof(VRCPhysBone.gravity))),
                    new MemberInfoContainer(typeof(VRCPhysBone).GetField(nameof(VRCPhysBone.gravityFalloff))),
                    //new (typeof(VRCPhysBone).GetField(nameof(VRCPhysBone.immobileType))),
                    new MemberInfoContainer(typeof(VRCPhysBone).GetField(nameof(VRCPhysBone.immobile))),
                    // new (typeof(VRCPhysBone).GetField(nameof(VRCPhysBone.limitType))),
                    new MemberInfoContainer(typeof(VRCPhysBone).GetField(nameof(VRCPhysBone.maxAngleX))) { Label = "Max Angle / Max Pitch" },
                    new MemberInfoContainer(typeof(VRCPhysBone).GetField(nameof(VRCPhysBone.maxAngleZ))) { Label = "Max Yaw" },
                    new MemberInfoContainer(typeof(VRCPhysBone).GetField(nameof(VRCPhysBone.limitRotation))) { Label ="Rotation (Pitch / Roll / Yaw)" },
                    new MemberInfoContainer(typeof(VRCPhysBone).GetField(nameof(VRCPhysBone.radius))),
                    //new (typeof(VRCPhysBone).GetField(nameof(VRCPhysBone.allowCollision))),
                    new MemberInfoContainer(typeof(VRCPhysBone).GetField(nameof(VRCPhysBone.collisionFilter))) { Label = "Allow Collision (Other)" },
                    new MemberInfoContainer(typeof(VRCPhysBone).GetField(nameof(VRCPhysBone.maxStretch))),
#if !PHYSBONE_ONLY_1_0
                    new MemberInfoContainer(typeof(VRCPhysBone).GetField(nameof(VRCPhysBone.stretchMotion))),
                    new MemberInfoContainer(typeof(VRCPhysBone).GetField(nameof(VRCPhysBone.maxSquish))),
#endif
                    //new (typeof(VRCPhysBone).GetField(nameof(VRCPhysBone.allowGrabbing))),
                    new MemberInfoContainer(typeof(VRCPhysBone).GetField(nameof(VRCPhysBone.grabFilter))) { Label = "Allow Grabbing (Other)" },
                    //new (typeof(VRCPhysBone).GetField(nameof(VRCPhysBone.allowPosing))),
                    new MemberInfoContainer(typeof(VRCPhysBone).GetField(nameof(VRCPhysBone.poseFilter))) { Label = "Allow Posing (Other)" },
                    new MemberInfoContainer(typeof(VRCPhysBone).GetField(nameof(VRCPhysBone.snapToHand))),
                }
            },
            { typeof(AimConstraint), ConstraintCommonMembers(typeof(AimConstraint)) },
            { typeof(LookAtConstraint), ConstraintCommonMembers(typeof(LookAtConstraint)) },
            { typeof(ParentConstraint), ConstraintCommonMembers(typeof(ParentConstraint)) },
            { typeof(PositionConstraint), ConstraintCommonMembers(typeof(PositionConstraint)) },
            { typeof(RotationConstraint), ConstraintCommonMembers(typeof(RotationConstraint)) },
            { typeof(ScaleConstraint), ConstraintCommonMembers(typeof(ScaleConstraint)) },
#if HAS_VRC_CONSTRAINT
            { typeof(VRCAimConstraint), VRCConstraintCommonMembers(typeof(VRCAimConstraint)) },
            { typeof(VRCLookAtConstraint), VRCConstraintCommonMembers(typeof(VRCLookAtConstraint)) },
            { typeof(VRCParentConstraint), VRCConstraintCommonMembers(typeof(VRCParentConstraint)) },
            { typeof(VRCPositionConstraint), VRCConstraintCommonMembers(typeof(VRCPositionConstraint)) },
            { typeof(VRCRotationConstraint), VRCConstraintCommonMembers(typeof(VRCRotationConstraint)) },
            { typeof(VRCScaleConstraint), VRCConstraintCommonMembers(typeof(VRCScaleConstraint)) },
#endif
        };

        static Dictionary<Type, TypeMember[]> AvailableMembersCache = new Dictionary<Type, TypeMember[]>();

        public static TypeMember[] GetAvailableMembers(this Type type)
        {
            if (AvailableMembersCache.TryGetValue(type, out var availableMembers))
            {
                return availableMembers;
            }
            var hasEnabled = TypeUtil.GetHasEnabled(type);

            if (AvailableMembers.TryGetValue(type, out var members))
            {
                var array = new TypeMember[members.Length + (hasEnabled ? 1 : 0)];
                var offset = 0;
                if (hasEnabled)
                {
                    array[0] = new TypeMember(type, "enabled");
                    offset = 1;
                }
                for (var i = 0; i < members.Length; ++i)
                {
                    array[i + offset] = members[i].Path == null ? new TypeMember(members[i].MemberInfo) : new TypeMember(type, members[i].Path, members[i].MemberInfo);
                }
                return AvailableMembersCache[type] = array;
            }

            if (hasEnabled)
            {
                return AvailableMembersCache[type] = new TypeMember[] { new TypeMember(type, "enabled") };
            }

            return AvailableMembersCache[type] = new TypeMember[0];
        }

        public static List<TypeMember> GetAvailableMembers(this IEnumerable<Type> types)
        {
            var list = new List<TypeMember>();
            foreach (var type in types)
            {
                list.AddRange(type.GetAvailableMembers());
            }
            return list;
        }

        public static List<TypeMember> GetAvailableMembersOnlySuitableForTransition(this IEnumerable<Type> types)
        {
            var list = new List<TypeMember>();
            foreach (var type in types)
            {
                var members = type.GetAvailableMembers();
                foreach (var member in members)
                {
                    if (member.MemberType.IsSuitableForTransition())
                    {
                        list.Add(member);
                    }
                }
            }
            return list;
        }

        public static bool IsSuitableForTransition(this Type type) => type == typeof(float) || type == typeof(Vector3) || type == typeof(Quaternion) || type == typeof(Color);

        static Dictionary<(Type, string), MemberInfo> MemberInfoCache = new Dictionary<(Type, string), MemberInfo>();

        public static MemberInfo GetMember(Type type, string memberName)
        {
            if (MemberInfoCache.TryGetValue((type, memberName), out var memberInfo))
            {
                return memberInfo;
            }
            if (memberName.Contains('.'))
            {
                var separaterIndex = memberName.IndexOf('.');
                var firstMemberName = memberName.Substring(0, separaterIndex);
                var firstMember = GetMember(type, firstMemberName);
                if (firstMember == null) return null;
                MemberInfoCache[(type, firstMemberName)] = firstMember;
                var firstMemberType = firstMember.MemberType == MemberTypes.Field ? ((FieldInfo)firstMember).FieldType : ((PropertyInfo)firstMember).PropertyType;
                return MemberInfoCache[(type, memberName)] = GetMember(firstMemberType, memberName.Substring(separaterIndex + 1));
            }
            var field = type.GetField(memberName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (field != null) return MemberInfoCache[(type, memberName)] = field;
            var property = type.GetProperty(memberName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (property != null) return MemberInfoCache[(type, memberName)] = property;
            return null;
        }

        public static string GetMemberField(Type type, string memberName)
        {
            if (memberName == "enabled") return "m_Enabled";
            if (AvailableMembers.TryGetValue(type, out var members))
            {
                var memberInfoContainer = members.FirstOrDefault(m => m.Path == memberName);
                if (memberInfoContainer != null) return memberInfoContainer.Field ?? memberInfoContainer.Path;
                var memberInfo = GetMember(type, memberName);
                return members.FirstOrDefault(m => m.MemberInfo == memberInfo)?.Field ?? memberInfo.Name;
            }
            else
            {
                var memberInfo = GetMember(type, memberName);
                return memberInfo.Name;
            }
        }

        public static string GetMemberLabel(Type type, string memberName)
        {
            if (AvailableMembers.TryGetValue(type, out var members))
            {
                var memberInfoContainer = members.FirstOrDefault(m => m.Path == memberName);
                if (memberInfoContainer != null) return memberInfoContainer.Label ?? memberInfoContainer.Path;
                var memberInfo = GetMember(type, memberName);
                return members.FirstOrDefault(m => m.MemberInfo == memberInfo)?.Label ?? memberInfo.Name;
            }
            else
            {
                var memberInfo = GetMember(type, memberName);
                return memberInfo.Name;
            }
        }
    }
}
