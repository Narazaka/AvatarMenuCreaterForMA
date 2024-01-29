using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace net.narazaka.avatarmenucreator
{
    public static class Util
    {
        /// <summary>
        /// 子GameObjectの相対パスを返す (親がnullなら絶対パス)
        /// </summary>
        /// <param name="baseObject">親 (nullなら絶対パス)</param>
        /// <param name="targetObject">子</param>
        /// <returns>パス</returns>
        public static string ChildPath(GameObject baseObject, GameObject targetObject)
        {
            var paths = new List<string>();
            var transform = targetObject.transform;
            var baseObjectTransform = baseObject == null ? null : baseObject.transform;
            while (baseObjectTransform != transform && transform != null)
            {
                paths.Add(transform.gameObject.name);
                transform = transform.parent;
            }
            paths.Reverse();
            return string.Join("/", paths.ToArray());
        }

        public static Material[] GetMaterialSlots(GameObject gameObject)
        {
            var renderer = gameObject.GetComponent<Renderer>();
            if (renderer == null) return new Material[0];
            return renderer.sharedMaterials;
        }

        public static List<string> GetBlendShapeNames(GameObject gameObject)
        {
            var shapekeyNames = new List<string>();
            var mesh = gameObject.GetComponent<SkinnedMeshRenderer>();
            if (mesh == null) return shapekeyNames;
            for (var i = 0; i < mesh.sharedMesh.blendShapeCount; ++i)
            {
                var name = mesh.sharedMesh.GetBlendShapeName(i);
                if (shapekeyNames.Contains(name)) continue;
                shapekeyNames.Add(name);
            }
            return shapekeyNames;
        }

        public interface INameAndDescription
        {
            string Name { get; }
            string Description { get; }
        }

        public class NameWithDescription : INameAndDescription
        {
            public string Name { get; set; }
            public string Description { get => Name; }
        }

        public static IEnumerable<NameWithDescription> ToNames(this IEnumerable<string> names) =>
            names.Select(name => new NameWithDescription { Name = name });

        public class MaterialShaderDescription
        {
            public int Index;
            public Material Material;
            public Shader Shader;
            public List<ShaderParameter> ShaderParameters;
        }

        public class ShaderParametersCache
        {
            Dictionary<GameObject, List<MaterialShaderDescription>> All = new Dictionary<GameObject, List<MaterialShaderDescription>>();
            Dictionary<GameObject, List<ShaderParameter>> Filtered = new Dictionary<GameObject, List<ShaderParameter>>();

            public void Clear()
            {
                All.Clear();
                Filtered.Clear();
            }

            public List<MaterialShaderDescription> GetShaderParameters(GameObject gameObject)
            {
                var renderer = gameObject.GetComponent<Renderer>();
                if (renderer == null) return new List<MaterialShaderDescription>();
                if (All.TryGetValue(gameObject, out var descriptions))
                {
                    if (Valid(renderer, descriptions))
                    {
                        return descriptions;
                    }
                }
                descriptions = Util.GetShaderParameters(gameObject).ToList();
                All[gameObject] = descriptions;
                return descriptions;
            }

            public List<ShaderParameter> GetFilteredShaderParameters(GameObject gameObject)
            {
                var renderer = gameObject.GetComponent<Renderer>();
                if (renderer == null) return new List<ShaderParameter>();
                if (All.TryGetValue(gameObject, out var descriptions))
                {
                    if (Valid(renderer, descriptions))
                    {
                        if (Filtered.TryGetValue(gameObject, out var filtered))
                        {
                            return filtered;
                        }
                    }
                }
                return (Filtered[gameObject] = GetShaderParameters(gameObject).ToFlatUniqueShaderParameterValues().OnlyFloatLike().OrderDefault().ToList());
            }

            bool Valid(Renderer renderer, List<MaterialShaderDescription> descriptions)
            {
                var materials = renderer.sharedMaterials;
                if (descriptions.Count != materials.Length) return false;
                for (var i = 0; i < descriptions.Count; ++i)
                {
                    if (descriptions[i].Material != materials[i]) return false;
                    if (descriptions[i].Shader != materials[i].shader) return false;
                }
                return true;
            }
        }

        public static IEnumerable<MaterialShaderDescription> GetShaderParameters(GameObject gameObject)
        {
            var renderer = gameObject.GetComponent<Renderer>();
            if (renderer == null) return Enumerable.Empty<MaterialShaderDescription>();
            return renderer.sharedMaterials.Select((mat, i) => new MaterialShaderDescription
            {
                Index = i,
                Material = mat,
                Shader = mat.shader,
                ShaderParameters = GetShaderParameters(mat.shader).ToList(),
            });
        }

        public class ShaderParameter : ShaderParameterValue
        {
            public int Index { get; set; }
        }

        public class ShaderParameterValue : INameAndDescription, System.IEquatable<ShaderParameterValue>
        {
            static Dictionary<string, string> CommonNames = new Dictionary<string, string>
            {
                { "_Tweak_transparency", "Transparency Level(UTS)" },
                { "_AlphaMaskValue", "Transparency(lilToon)" },
            };

            public UnityEngine.Rendering.ShaderPropertyType Type { get; set; }
            public string Name { get; set; }
            public string Description {
                get {
                    if (CommonNames.TryGetValue(Name, out var description)) return $"{Name} [{description}]";
                    if (string.IsNullOrEmpty(PropertyDescription) || Name == PropertyDescription) return Name;
                    return $"{Name} [{PropertyDescription}]";
                }
            }
            public string PropertyDescription { get; set; }
            public bool IsCommon { get => CommonNames.ContainsKey(Name); }

            public override bool Equals(object obj)
            {
                return Equals(obj as ShaderParameterValue);
            }

            public bool Equals(ShaderParameterValue other)
            {
                return Name == other.Name && Type == other.Type;
            }

            public override int GetHashCode()
            {
                return Name.GetHashCode() ^ Type.GetHashCode() * 17;
            }
        }

        public class ShaderParameterComparator : IEqualityComparer<ShaderParameter>
        {
            public bool Equals(ShaderParameter x, ShaderParameter y)
            {
                return (x as ShaderParameterValue).Equals(y);
            }

            public int GetHashCode(ShaderParameter obj)
            {
                return (obj as ShaderParameterValue).GetHashCode();
            }
        }

        public static IEnumerable<ShaderParameter> GetShaderParameters(Shader shader)
        {
            return Enumerable.Range(0, shader.GetPropertyCount()).Select(i => new ShaderParameter
            {
                Index = i,
                Type = shader.GetPropertyType(i),
                Name = shader.GetPropertyName(i),
                PropertyDescription = shader.GetPropertyDescription(i),
            });
        }

        static HashSet<UnityEngine.Rendering.ShaderPropertyType> FloatLikeShaderPropertyType = new HashSet<UnityEngine.Rendering.ShaderPropertyType> {
            UnityEngine.Rendering.ShaderPropertyType.Range,
            UnityEngine.Rendering.ShaderPropertyType.Float,
        };

        public static IEnumerable<ShaderParameter> OnlyFloatLike(this IEnumerable<ShaderParameter> values) =>
                values.Where(p => FloatLikeShaderPropertyType.Contains(p.Type));

        public static IEnumerable<ShaderParameter> OrderDefault(this IEnumerable<ShaderParameter> values) =>
            values.OrderBy(p => !p.IsCommon).ThenBy(p => p.Name);

        public static IEnumerable<ShaderParameter> ToFlatUniqueShaderParameterValues(this IEnumerable<MaterialShaderDescription> descriptions) =>
            descriptions
                .SelectMany(desc => desc.ShaderParameters)
                .Distinct(new ShaderParameterComparator());
    }
}
