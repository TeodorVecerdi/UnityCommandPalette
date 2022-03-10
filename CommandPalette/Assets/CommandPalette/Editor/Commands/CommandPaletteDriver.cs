using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace CommandPalette.Commands {
    public static class CommandPaletteDriver {
        private static readonly List<CommandEntry> s_CommandEntries = new List<CommandEntry>();
        public static List<CommandEntry> CommandEntries => s_CommandEntries;

        [InitializeOnLoadMethod]
        private static void InitializeDriver() {
            List<MethodInfo> allMethods = AppDomain.CurrentDomain
                                                          .GetAssemblies()
                                                          .SelectMany(assembly => assembly.GetTypes())
                                                          .SelectMany(type => type.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)).ToList();
            Dictionary<string, MethodInfo> validateMethods = allMethods.Where(method => method.GetCustomAttribute<CommandValidateMethodAttribute>() != null).ToDictionary(info => info.Name);
            IEnumerable<MethodInfo> methods = allMethods.Where(info => info.GetCustomAttribute<CommandAttribute>() != null);

            foreach (MethodInfo method in methods) {
                CommandAttribute attribute = method.GetCustomAttribute<CommandAttribute>();
                string displayName = string.IsNullOrWhiteSpace(attribute.DisplayName) ? ObjectNames.NicifyVariableName(method.Name) : attribute.DisplayName;
                string shortName = string.IsNullOrWhiteSpace(attribute.ShortName) ? GetShortName(ObjectNames.NicifyVariableName(displayName)) : attribute.ShortName;
                MethodInfo validationMethod = null;
                if (!string.IsNullOrEmpty(attribute.ValidationMethod)) {
                    if (!validateMethods.TryGetValue(attribute.ValidationMethod, out validationMethod)) {
                        Debug.LogError($"Could not find validation method {attribute.ValidationMethod} for command {displayName}");
                    }

                    if (validationMethod != null && validationMethod.ReturnType != typeof(bool)) {
                        Debug.LogError($"Validation method {attribute.ValidationMethod} for command {displayName} must return a bool. Located at {validationMethod.DeclaringType.FullName}.{validationMethod.Name}");
                        validationMethod = null;
                    }
                }
                s_CommandEntries.Add(new CommandEntry(displayName, shortName, attribute.Description, method, validationMethod));
            }
        }

        private static string GetShortName(string name) {
            string[] parts = name.Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries);
            StringBuilder builder = new StringBuilder();
            foreach (string part in parts) {
                builder.Append(part[0]);
            }
            return builder.ToString();
        }
    }
}