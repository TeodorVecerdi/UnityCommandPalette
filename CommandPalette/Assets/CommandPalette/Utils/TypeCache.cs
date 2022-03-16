using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;

namespace CommandPalette.Utils {
    [InitializeOnLoad]
    public static class TypeCache {
        private static readonly List<Type> allTypes;
        private static readonly Dictionary<string, List<int>> allTypesName;
        private static readonly Dictionary<string, List<int>> allTypesFullName;
        private static readonly Dictionary<string, int> allTypesAssemblyQualifiedName;

        static TypeCache() {
            allTypes = AppDomain.CurrentDomain.GetAssemblies().SelectMany(assembly => assembly.GetTypes()).ToList();
            allTypesName = new Dictionary<string, List<int>>();
            allTypesFullName = new Dictionary<string, List<int>>();
            allTypesAssemblyQualifiedName = new Dictionary<string, int>();

            for (int i = 0; i < allTypes.Count; i++) {
                Type type = allTypes[i];

                if (type.AssemblyQualifiedName != null) {
                    allTypesAssemblyQualifiedName[type.AssemblyQualifiedName] = i;
                }

                if (!allTypesName.ContainsKey(type.Name)) {
                    allTypesName[type.Name] = new List<int>();
                }
                allTypesName[type.Name].Add(i);

                if (type.FullName != null) {
                    if (!allTypesFullName.ContainsKey(type.FullName)) {
                        allTypesFullName[type.FullName] = new List<int>();
                    }
                    allTypesFullName[type.FullName].Add(i);
                }
            }
        }

        public static List<Type> AllTypes => allTypes;
        public static Dictionary<string, List<int>> AllTypeIndicesByName => allTypesName;
        public static Dictionary<string, List<int>> AllTypeIndicesByFullName => allTypesFullName;
        public static Dictionary<string, int> AllTypeIndicesByAssemblyQualifiedName => allTypesAssemblyQualifiedName;

        public static Type GetTypeByAssemblyQualifiedName(string assemblyQualifiedName) {
            return allTypesAssemblyQualifiedName.TryGetValue(assemblyQualifiedName, out int index) ? allTypes[index] : null;
        }

        public static IEnumerable<Type> GetTypesByName(string name) {
            return allTypesName.TryGetValue(name, out List<int> indices) ? indices.Select(index => allTypes[index]) : Enumerable.Empty<Type>();
        }

        public static IEnumerable<Type> GetTypesByFullName(string fullName) {
            return allTypesFullName.TryGetValue(fullName, out List<int> indices) ? indices.Select(index => allTypes[index]) : Enumerable.Empty<Type>();
        }

        public static IEnumerable<Type> TypesWithAttribute<T>() where T : Attribute {
            return allTypes.Where(type => type.GetCustomAttributes(typeof(T), false).Length > 0);
        }

        private const BindingFlags k_DefaultMethodBindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;
        public static IEnumerable<MethodInfo> GetMethodsWithAttribute<T>(BindingFlags bindingFlags = k_DefaultMethodBindingFlags) where T : Attribute {
            return allTypes.SelectMany(type => type.GetMethods(bindingFlags)).Where(method => method.GetCustomAttributes(typeof(T), false).Length > 0);
        }
    }
}