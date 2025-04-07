using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using VRC.SDK3.Dynamics.PhysBone.Components;
using UnityEngine;
using UnityEngine.Animations;
using VRC.SDK3.Avatars.Components;
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
            {
                typeof(AudioSource),
                new MemberInfoContainer[]
                {
                    new MemberInfoContainer(typeof(AudioSource).GetProperty(nameof(AudioSource.mute))) { Field = "Mute" },
                    new MemberInfoContainer(typeof(AudioSource).GetProperty(nameof(AudioSource.bypassEffects))) { Field = "BypassEffects" },
                    new MemberInfoContainer(typeof(AudioSource).GetProperty(nameof(AudioSource.bypassListenerEffects))) { Field = "BypassListenerEffects" },
                    new MemberInfoContainer(typeof(AudioSource).GetProperty(nameof(AudioSource.bypassReverbZones))) { Field = "BypassReverbZones" },
                    new MemberInfoContainer(typeof(AudioSource).GetProperty(nameof(AudioSource.playOnAwake))) { Field = "m_PlayOnAwake" },
                    new MemberInfoContainer(typeof(AudioSource).GetProperty(nameof(AudioSource.loop))) { Field = "Loop" },
                    new MemberInfoContainer(typeof(AudioSource).GetProperty(nameof(AudioSource.priority))) { Field = "Priority" },
                    new MemberInfoContainer(typeof(AudioSource).GetProperty(nameof(AudioSource.volume))) { Field = "m_Volume" },
                    new MemberInfoContainer(typeof(AudioSource).GetProperty(nameof(AudioSource.pitch))) { Field = "m_Pitch" },
                    new MemberInfoContainer(typeof(AudioSource).GetProperty(nameof(AudioSource.panStereo))) { Field = "Pan2D", Label = "Stereo Pan" },
                    new MemberInfoContainer(typeof(AudioSource).GetProperty(nameof(AudioSource.dopplerLevel))) { Field = "DopplerLevel" },
                    new MemberInfoContainer(typeof(AudioSource).GetProperty(nameof(AudioSource.minDistance))) { Field = "MinDistance" },
                    new MemberInfoContainer(typeof(AudioSource).GetProperty(nameof(AudioSource.maxDistance))) { Field = "MaxDistance" },
                }
            },
            {
                typeof(VRCSpatialAudioSource),
                new MemberInfoContainer[]
                {
                    new MemberInfoContainer(typeof(VRCSpatialAudioSource).GetField(nameof(VRCSpatialAudioSource.Gain))),
                    new MemberInfoContainer(typeof(VRCSpatialAudioSource).GetField(nameof(VRCSpatialAudioSource.Far))),
                    new MemberInfoContainer(typeof(VRCSpatialAudioSource).GetField(nameof(VRCSpatialAudioSource.Near))),
                    new MemberInfoContainer(typeof(VRCSpatialAudioSource).GetField(nameof(VRCSpatialAudioSource.VolumetricRadius))),
                    new MemberInfoContainer(typeof(VRCSpatialAudioSource).GetField(nameof(VRCSpatialAudioSource.EnableSpatialization))),
                    new MemberInfoContainer(typeof(VRCSpatialAudioSource).GetField(nameof(VRCSpatialAudioSource.UseAudioSourceVolumeCurve))),
                }
            },
            {
                typeof(MeshRenderer),
                new MemberInfoContainer[]{ new MemberInfoContainer(typeof(MeshRenderer).GetProperty(nameof(MeshRenderer.receiveShadows))) { Field = "m_ReceiveShadows" } }
            },
            {
                typeof(SkinnedMeshRenderer),
                new MemberInfoContainer[]
                {
                    new MemberInfoContainer(typeof(SkinnedMeshRenderer).GetProperty(nameof(SkinnedMeshRenderer.receiveShadows))) { Field = "m_ReceiveShadows" },
                    new MemberInfoContainer(typeof(SkinnedMeshRenderer).GetProperty(nameof(SkinnedMeshRenderer.skinnedMotionVectors))) { Field = "m_SkinnedMotionVectors" },
                    new MemberInfoContainer(typeof(SkinnedMeshRenderer).GetProperty(nameof(SkinnedMeshRenderer.updateWhenOffscreen))) { Field = "m_UpdateWhenOffscreen" },
                    // new MemberInfoContainer(typeof(Bounds).GetProperty(nameof(Bounds.center))) { Field = "m_AABB.m_Center", Label = "Bounds.Center" },
                    // new MemberInfoContainer(typeof(Bounds).GetProperty(nameof(Bounds.extents))) { Field = "m_AABB.m_Extent", Label = "Bounds.Extents" },
                }
            },
            {
                typeof(Rigidbody),
                new MemberInfoContainer[]
                {
                    new MemberInfoContainer(typeof(Rigidbody).GetProperty(nameof(Rigidbody.mass))) { Field = "m_Mass" },
                    new MemberInfoContainer(typeof(Rigidbody).GetProperty(nameof(Rigidbody.drag))) { Field = "m_Drag" },
                    new MemberInfoContainer(typeof(Rigidbody).GetProperty(nameof(Rigidbody.angularDrag))) { Field = "m_AngularDrag" },
                    new MemberInfoContainer(typeof(Rigidbody).GetProperty(nameof(Rigidbody.inertiaTensor))) { Field = "m_InertiaTensor" },
                    new MemberInfoContainer(typeof(Rigidbody).GetProperty(nameof(Rigidbody.centerOfMass))) { Field = "m_CenterOfMass" },
                    new MemberInfoContainer(typeof(Rigidbody).GetProperty(nameof(Rigidbody.isKinematic))) { Field = "m_IsKinematic" },
                    new MemberInfoContainer(typeof(Rigidbody).GetProperty(nameof(Rigidbody.useGravity))) { Field = "m_UseGravity" },
                }
            },
            {
                typeof(BoxCollider),
                new MemberInfoContainer[]
                {
                    new MemberInfoContainer(typeof(BoxCollider).GetProperty(nameof(BoxCollider.isTrigger))) { Field = "m_IsTrigger" },
                    new MemberInfoContainer(typeof(BoxCollider).GetProperty(nameof(BoxCollider.center))) { Field = "m_Center" },
                    new MemberInfoContainer(typeof(BoxCollider).GetProperty(nameof(BoxCollider.size))) { Field = "m_Size" },
                }
            },
            {
                typeof(SphereCollider),
                new MemberInfoContainer[]
                {
                    new MemberInfoContainer(typeof(SphereCollider).GetProperty(nameof(SphereCollider.isTrigger))) { Field = "m_IsTrigger" },
                    new MemberInfoContainer(typeof(SphereCollider).GetProperty(nameof(SphereCollider.center))) { Field = "m_Center" },
                    new MemberInfoContainer(typeof(SphereCollider).GetProperty(nameof(SphereCollider.radius))) { Field = "m_Radius" },
                }
            },
            {
                typeof(CapsuleCollider),
                new MemberInfoContainer[]
                {
                    new MemberInfoContainer(typeof(CapsuleCollider).GetProperty(nameof(CapsuleCollider.isTrigger))) { Field = "m_IsTrigger" },
                    new MemberInfoContainer(typeof(CapsuleCollider).GetProperty(nameof(CapsuleCollider.center))) { Field = "m_Center" },
                    new MemberInfoContainer(typeof(CapsuleCollider).GetProperty(nameof(CapsuleCollider.radius))) { Field = "m_Radius" },
                    new MemberInfoContainer(typeof(CapsuleCollider).GetProperty(nameof(CapsuleCollider.height))) { Field = "m_Height" },
                }
            },
            {
                typeof(MeshCollider),
                new MemberInfoContainer[]
                {
                    new MemberInfoContainer(typeof(MeshCollider).GetProperty(nameof(MeshCollider.isTrigger))) { Field = "m_IsTrigger" },
                    new MemberInfoContainer(typeof(MeshCollider).GetProperty(nameof(MeshCollider.convex))) { Field = "m_Convex" },
                }
            },
            {
                typeof(WheelCollider),
                new MemberInfoContainer[]
                {
                    new MemberInfoContainer(typeof(WheelCollider).GetProperty(nameof(WheelCollider.isTrigger))) { Field = "m_IsTrigger" },
                    new MemberInfoContainer(typeof(WheelCollider).GetProperty(nameof(WheelCollider.mass))) { Field = "m_Mass" },
                    new MemberInfoContainer(typeof(WheelCollider).GetProperty(nameof(WheelCollider.radius))) { Field = "m_Radius" },
                    new MemberInfoContainer(typeof(WheelCollider).GetProperty(nameof(WheelCollider.wheelDampingRate))) { Field = "m_WheelDampingRate" },
                    new MemberInfoContainer(typeof(WheelCollider).GetProperty(nameof(WheelCollider.suspensionDistance))) { Field = "m_SuspensionDistance" },
                    new MemberInfoContainer(typeof(WheelCollider).GetProperty(nameof(WheelCollider.forceAppPointDistance))) { Field = "m_ForceAppPointDistance" },
                    new MemberInfoContainer(typeof(WheelCollider).GetProperty(nameof(WheelCollider.center))) { Field = "m_Center" },
                    // new MemberInfoContainer(typeof(WheelCollider).GetProperty(nameof(WheelCollider.suspensionSpring))) { Field = "m_SuspensionSpring" },
                    // new MemberInfoContainer(typeof(WheelCollider).GetProperty(nameof(WheelCollider.forwardFriction))) { Field = "m_ForwardFriction" },
                    // new MemberInfoContainer(typeof(WheelCollider).GetProperty(nameof(WheelCollider.sidewaysFriction))) { Field = "m_SidewaysFriction" },
                }
            },
            {
                typeof(VRCPhysBoneCollider),
                new MemberInfoContainer[]
                {
                    new MemberInfoContainer(typeof(VRCPhysBoneCollider).GetField(nameof(VRCPhysBoneCollider.shapeType))),
                    new MemberInfoContainer(typeof(VRCPhysBoneCollider).GetField(nameof(VRCPhysBoneCollider.radius))),
                    new MemberInfoContainer(typeof(VRCPhysBoneCollider).GetField(nameof(VRCPhysBoneCollider.height))),
                    new MemberInfoContainer(typeof(VRCPhysBoneCollider).GetField(nameof(VRCPhysBoneCollider.position))),
                    new MemberInfoContainer(typeof(VRCPhysBoneCollider).GetField(nameof(VRCPhysBoneCollider.rotation))),
                    new MemberInfoContainer(typeof(VRCPhysBoneCollider).GetField(nameof(VRCPhysBoneCollider.insideBounds))),
                    new MemberInfoContainer(typeof(VRCPhysBoneCollider).GetField(nameof(VRCPhysBoneCollider.bonesAsSpheres))),
                }
            },
#if HAS_VRC_HEADCHOP
            {
                typeof(VRCHeadChop),
                new MemberInfoContainer[]
                {
                    new MemberInfoContainer(typeof(VRCHeadChop).GetField(nameof(VRCHeadChop.globalScaleFactor))),
                }
            },
#endif
            {
                typeof(Camera),
                new MemberInfoContainer[]
                {
                    new MemberInfoContainer(typeof(Camera).GetProperty(nameof(Camera.backgroundColor))) { Field = "m_BackGroundColor", Label = "BackGround" },
                    new MemberInfoContainer(typeof(Camera).GetProperty(nameof(Camera.orthographic))) { Field = "orthographic" },
                    new MemberInfoContainer(typeof(Camera).GetProperty(nameof(Camera.orthographicSize))) { Field = "orthographic size" },
                    new MemberInfoContainer(typeof(Camera).GetProperty(nameof(Camera.fieldOfView))) { Field = "field of view" },
#if UNITY_2022_2_OR_NEWER
                    new MemberInfoContainer(typeof(Camera).GetProperty(nameof(Camera.iso))) { Field = "m_Iso" },
                    new MemberInfoContainer(typeof(Camera).GetProperty(nameof(Camera.shutterSpeed))) { Field = "m_ShutterSpeed" },
                    new MemberInfoContainer(typeof(Camera).GetProperty(nameof(Camera.aperture))) { Field = "m_Aperture" },
                    new MemberInfoContainer(typeof(Camera).GetProperty(nameof(Camera.focusDistance))) { Field = "m_FocusDistance" },
                    // new MemberInfoContainer(typeof(Camera).GetProperty(nameof(Camera.curvature))) { Field = "m_Curvature" },
                    new MemberInfoContainer(typeof(Camera).GetProperty(nameof(Camera.barrelClipping))) { Field = "m_BarrelClipping" },
                    new MemberInfoContainer(typeof(Camera).GetProperty(nameof(Camera.anamorphism))) { Field = "m_Anamorphism" },
#endif
                    new MemberInfoContainer(typeof(Camera).GetProperty(nameof(Camera.focalLength))) { Field = "m_FocalLength" },
                    // new MemberInfoContainer(typeof(Camera).GetProperty(nameof(Camera.sensorSize))) { Field = "m_SensorSize" },
                    // new MemberInfoContainer(typeof(Camera).GetProperty(nameof(Camera.lensShift))) { Field = "m_LensShift" },
                    new MemberInfoContainer(typeof(Camera).GetProperty(nameof(Camera.nearClipPlane))) { Field = "near clip plane" },
                    new MemberInfoContainer(typeof(Camera).GetProperty(nameof(Camera.farClipPlane))) { Field = "far clip plane" },
                    // new MemberInfoContainer(typeof(Camera).GetProperty(nameof(Camera.rect))) { Field = "m_NormalizedViewPortRect", Label = "Viewport Rect" },
                    new MemberInfoContainer(typeof(Camera).GetProperty(nameof(Camera.depth))) { Field = "m_Depth" },
                    new MemberInfoContainer(typeof(Camera).GetProperty(nameof(Camera.useOcclusionCulling))) { Field = "m_OcclusionCulling" },
                    new MemberInfoContainer(typeof(Camera).GetProperty(nameof(Camera.allowHDR))) { Field = "m_HDR" },
                    new MemberInfoContainer(typeof(Camera).GetProperty(nameof(Camera.allowMSAA))) { Field = "m_AllowMSAA" },
                    new MemberInfoContainer(typeof(Camera).GetProperty(nameof(Camera.stereoSeparation))) { Field = "m_StereoSeparation" },
                    new MemberInfoContainer(typeof(Camera).GetProperty(nameof(Camera.stereoConvergence))) { Field = "m_StereoConvergence" },
                }
            },
            {
                typeof(Cloth),
                new MemberInfoContainer[]
                {
                    new MemberInfoContainer(typeof(Cloth).GetProperty(nameof(Cloth.bendingStiffness))) { Field = "m_BendingStiffness" },
                    new MemberInfoContainer(typeof(Cloth).GetProperty(nameof(Cloth.stretchingStiffness))) { Field = "m_StretchingStiffness" },
                    new MemberInfoContainer(typeof(Cloth).GetProperty(nameof(Cloth.useTethers))) { Field = "m_UseTethers" },
                    new MemberInfoContainer(typeof(Cloth).GetProperty(nameof(Cloth.useGravity))) { Field = "m_UseGravity" },
                    new MemberInfoContainer(typeof(Cloth).GetProperty(nameof(Cloth.damping))) { Field = "m_Damping" },
                    new MemberInfoContainer(typeof(Cloth).GetProperty(nameof(Cloth.externalAcceleration))) { Field = "m_ExternalAcceleration" },
                    new MemberInfoContainer(typeof(Cloth).GetProperty(nameof(Cloth.randomAcceleration))) { Field = "m_RandomAcceleration" },
                    new MemberInfoContainer(typeof(Cloth).GetProperty(nameof(Cloth.worldVelocityScale))) { Field = "m_WorldVelocityScale" },
                    new MemberInfoContainer(typeof(Cloth).GetProperty(nameof(Cloth.worldAccelerationScale))) { Field = "m_WorldAccelerationScale" },
                    new MemberInfoContainer(typeof(Cloth).GetProperty(nameof(Cloth.friction))) { Field = "m_Friction" },
                    new MemberInfoContainer(typeof(Cloth).GetProperty(nameof(Cloth.collisionMassScale))) { Field = "m_CollisionMassScale" },
                    new MemberInfoContainer(typeof(Cloth).GetProperty(nameof(Cloth.enableContinuousCollision))) { Field = "m_UseContinuousCollision" },
                    new MemberInfoContainer(typeof(Cloth).GetProperty(nameof(Cloth.sleepThreshold))) { Field = "m_SleepThreshold" },
                }
            },
            {
                typeof(Light),
                new MemberInfoContainer[]
                {
                    new MemberInfoContainer(typeof(Light).GetProperty(nameof(Light.range))) { Field = "m_Range" },
                    new MemberInfoContainer(typeof(Light).GetProperty(nameof(Light.spotAngle))) { Field = "m_SpotAngle" },
                    new MemberInfoContainer(typeof(Light).GetProperty(nameof(Light.colorTemperature))) { Field = "m_ColorTemperature" },
                    new MemberInfoContainer(typeof(Light).GetProperty(nameof(Light.color))) { Field = "m_Color" },
                    new MemberInfoContainer(typeof(Light).GetProperty(nameof(Light.bounceIntensity))) { Field = "m_BounceIntensity" },
                    new MemberInfoContainer(typeof(Light).GetProperty(nameof(Light.intensity))) { Field = "m_Intensity" },
                    // new MemberInfoContainer(typeof(Light).GetProperty(nameof(Light.shadows))) { Field = "m_Shadows" },
                    // m_DrawHalo private?
                }
            },
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
