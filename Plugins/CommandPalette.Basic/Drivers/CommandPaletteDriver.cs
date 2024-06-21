using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace CommandPalette.Basic {
    public static class CommandPaletteDriver {
        private static readonly List<CommandEntry> s_CommandEntries = new();
        private static Dictionary<string, MethodInfo> s_parameterValueProviders;

        public static List<CommandEntry> CommandEntries => s_CommandEntries;
        public static Dictionary<string, MethodInfo> ParameterValueProviders => s_parameterValueProviders;

        [InitializeOnLoadMethod]
        private static void InitializeDriver() {
            s_parameterValueProviders = TypeCache.GetMethodsWithAttribute<InlineParameterValuesProviderAttribute>().ToDictionary(info => info.Name);
            Dictionary<string, MethodInfo> validateMethods = TypeCache.GetMethodsWithAttribute<CommandValidateMethodAttribute>().ToDictionary(info => info.Name);
            IEnumerable<MethodInfo> methods = TypeCache.GetMethodsWithAttribute<CommandAttribute>();

            foreach (var method in methods) {
                var attribute = method.GetCustomAttribute<CommandAttribute>();
                var displayName = string.IsNullOrWhiteSpace(attribute.DisplayName) ? ObjectNames.NicifyVariableName(method.Name) : attribute.DisplayName;
                var shortName = string.IsNullOrWhiteSpace(attribute.ShortName) ? GetShortName(ObjectNames.NicifyVariableName(displayName)) : attribute.ShortName;
                MethodInfo validationMethod = null;
                if (!string.IsNullOrEmpty(attribute.ValidationMethod)) {
                    if (!validateMethods.TryGetValue(attribute.ValidationMethod, out validationMethod)) {
                        Debug.LogError($"Could not find validation method {attribute.ValidationMethod} for command {displayName}");
                    }

                    if (validationMethod != null && validationMethod.ReturnType != typeof(bool)) {
                        Debug.LogError($"Validation method {attribute.ValidationMethod} for command {displayName} must return a bool. Located at {validationMethod.DeclaringType!.FullName}.{validationMethod.Name}");
                        validationMethod = null;
                    }
                }
                s_CommandEntries.Add(new CommandEntry(displayName, shortName, attribute.Description, attribute.ShowOnlyWhenSearching, method, validationMethod, attribute.Icon, attribute.Priority));
            }
        }

        private static string GetShortName(string name) {
            string[] parts = name.Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries);
            StringBuilder builder = new();
            foreach (var part in parts) {
                builder.Append(part[0]);
            }
            return builder.ToString();
        }
    }
}