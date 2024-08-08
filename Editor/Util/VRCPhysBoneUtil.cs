using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using VRC.SDK3.Dynamics.PhysBone.Components;

namespace net.narazaka.avatarmenucreator.util
{
    public class VRCPhysBoneUtil
    {
        public static PropertyInfo PhysBoneEnabled = typeof(VRCPhysBone).GetProperty("enabled", BindingFlags.Instance | BindingFlags.Public);

        static TypeMember PhysBoneEnabledTypeMember = new TypeMember(PhysBoneEnabled);

        static HashSet<MemberInfo> NotNeedResetPhysBoneFields = new HashSet<MemberInfo>
        {
            PhysBoneEnabled,
            typeof(VRCPhysBone).GetField(nameof(VRCPhysBone.collisionFilter)),
            typeof(VRCPhysBone).GetField(nameof(VRCPhysBone.grabFilter)),
            typeof(VRCPhysBone).GetField(nameof(VRCPhysBone.poseFilter)),
            typeof(VRCPhysBone).GetField(nameof(VRCPhysBone.snapToHand)),
        };

        public static bool IsNeedResetPhysBoneField(MemberInfo memberInfo) => memberInfo.ReflectedType == typeof(VRCPhysBone) && !NotNeedResetPhysBoneFields.Contains(memberInfo);
        public static bool IsNeedResetPhysBoneField(TypeMember[] typeMembers)
        {
            foreach (var typeMember in typeMembers)
            {
                if (IsNeedResetPhysBoneField(typeMember.Member)) return true;
            }
            return false;
        }
        public static bool HasPhysBoneEnabled(TypeMember[] typeMembers) => Array.IndexOf(typeMembers, PhysBoneEnabledTypeMember) != -1;
    }
}
