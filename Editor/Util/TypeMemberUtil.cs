using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using VRC.SDK3.Dynamics.PhysBone.Components;
using UnityEngine.Animations;

namespace net.narazaka.avatarmenucreator.util
{
    public static class TypeMemberUtil
    {
        class MemberInfoContainer
        {
            public MemberInfo MemberInfo;
            public string Label;
            public string Field;
            public MemberInfoContainer(MemberInfo memberInfo)
            {
                MemberInfo = memberInfo;
            }
        }
        static MemberInfoContainer[] ConstraintCommonMembers(Type type) => new MemberInfoContainer[]
        {
            new (type.GetProperty(nameof(IConstraint.constraintActive))) { Field = "m_Active" },
            new (type.GetProperty(nameof(IConstraint.weight))) { Field = "m_Weight" },
        };
        static Dictionary<Type, MemberInfoContainer[]> AvailableMembers = new Dictionary<Type, MemberInfoContainer[]>
        {
            {
                typeof(VRCPhysBone), new MemberInfoContainer[]
                {
                    //new (typeof(VRCPhysBone).GetField(nameof(VRCPhysBone.integrationType))),
                    new (typeof(VRCPhysBone).GetField(nameof(VRCPhysBone.pull))),
                    new (typeof(VRCPhysBone).GetField(nameof(VRCPhysBone.spring))) { Label ="Spring / Momentum" },
                    new (typeof(VRCPhysBone).GetField(nameof(VRCPhysBone.stiffness))),
                    new (typeof(VRCPhysBone).GetField(nameof(VRCPhysBone.gravity))),
                    new (typeof(VRCPhysBone).GetField(nameof(VRCPhysBone.gravityFalloff))),
                    //new (typeof(VRCPhysBone).GetField(nameof(VRCPhysBone.immobileType))),
                    new (typeof(VRCPhysBone).GetField(nameof(VRCPhysBone.immobile))),
                    // new (typeof(VRCPhysBone).GetField(nameof(VRCPhysBone.limitType))),
                    new (typeof(VRCPhysBone).GetField(nameof(VRCPhysBone.maxAngleX))) { Label = "Max Angle / Max Pitch" },
                    new (typeof(VRCPhysBone).GetField(nameof(VRCPhysBone.maxAngleZ))) { Label = "Max Yaw" },
                    new (typeof(VRCPhysBone).GetField(nameof(VRCPhysBone.limitRotation))) { Label ="Rotation (Pitch / Roll / Yaw)" },
                    new (typeof(VRCPhysBone).GetField(nameof(VRCPhysBone.radius))),
                    //new (typeof(VRCPhysBone).GetField(nameof(VRCPhysBone.allowCollision))),
                    new (typeof(VRCPhysBone).GetField(nameof(VRCPhysBone.collisionFilter))),
                    new (typeof(VRCPhysBone).GetField(nameof(VRCPhysBone.maxStretch))),
                    new (typeof(VRCPhysBone).GetField(nameof(VRCPhysBone.stretchMotion))),
                    new (typeof(VRCPhysBone).GetField(nameof(VRCPhysBone.maxSquish))),
                    //new (typeof(VRCPhysBone).GetField(nameof(VRCPhysBone.allowGrabbing))),
                    new (typeof(VRCPhysBone).GetField(nameof(VRCPhysBone.grabFilter))),
                    //new (typeof(VRCPhysBone).GetField(nameof(VRCPhysBone.allowPosing))),
                    new (typeof(VRCPhysBone).GetField(nameof(VRCPhysBone.poseFilter))),
                    new (typeof(VRCPhysBone).GetField(nameof(VRCPhysBone.snapToHand))),
                }
            },
            { typeof(AimConstraint), ConstraintCommonMembers(typeof(AimConstraint)) },
            { typeof(LookAtConstraint), ConstraintCommonMembers(typeof(LookAtConstraint)) },
            { typeof(ParentConstraint), ConstraintCommonMembers(typeof(ParentConstraint)) },
            { typeof(PositionConstraint), ConstraintCommonMembers(typeof(PositionConstraint)) },
            { typeof(RotationConstraint), ConstraintCommonMembers(typeof(RotationConstraint)) },
            { typeof(ScaleConstraint), ConstraintCommonMembers(typeof(ScaleConstraint)) },
        };

        static Dictionary<Type, TypeMember[]> AvailableMembersCache = new Dictionary<Type, TypeMember[]>();

        public static IEnumerable<TypeMember> GetAvailableMembers(this Type type)
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
                    array[i + offset] = new TypeMember(members[i].MemberInfo);
                }
                return AvailableMembersCache[type] = array;
            }

            if (hasEnabled)
            {
                return AvailableMembersCache[type] = new TypeMember[] { new TypeMember(type, "enabled") };
            }

            return AvailableMembersCache[type] = new TypeMember[0];
        }

        static Dictionary<(Type, string), MemberInfo> MemberInfoCache = new Dictionary<(Type, string), MemberInfo>();

        public static MemberInfo GetMember(Type type, string memberName)
        {
            if (MemberInfoCache.TryGetValue((type, memberName), out var memberInfo))
            {
                return memberInfo;
            }
            var field = type.GetField(memberName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (field != null) return MemberInfoCache[(type, memberName)] = field;
            var property = type.GetProperty(memberName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (property != null) return MemberInfoCache[(type, memberName)] = property;
            return null;
        }

        public static string GetMemberField(MemberInfo memberInfo)
        {
            if (memberInfo.Name == "enabled") return "m_Enabled";
            return AvailableMembers.TryGetValue(memberInfo.ReflectedType, out var members) ? members.FirstOrDefault(m => m.MemberInfo == memberInfo)?.Field ?? memberInfo.Name : memberInfo.Name;
        }

        public static string GetMemberLabel(MemberInfo memberInfo) => AvailableMembers.TryGetValue(memberInfo.ReflectedType, out var members) ? members.FirstOrDefault(m => m.MemberInfo == memberInfo)?.Label ?? memberInfo.Name : memberInfo.Name;
    }
}
