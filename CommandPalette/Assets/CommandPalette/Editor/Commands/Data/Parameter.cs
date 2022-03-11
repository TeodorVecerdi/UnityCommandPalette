using System;
using System.Reflection;
using UnityEditor;

namespace CommandPalette.Commands {
    public readonly struct Parameter {
        public readonly ParameterInfo ParameterInfo;
        public readonly bool HasDefaultValue;
        public readonly object DefaultValue;
        public readonly Type Type;

        public readonly string DisplayName;
        public readonly string Description;

        public Parameter(ParameterInfo parameterInfo) {
            ParameterInfo = parameterInfo;
            HasDefaultValue = parameterInfo.HasDefaultValue;
            DefaultValue = parameterInfo.DefaultValue;
            Type = parameterInfo.ParameterType;

            DisplayName = null;
            Description = null;

            if (ParameterInfo.GetCustomAttribute<ParameterAttribute>() is { } parameterAttribute) {
                if (!string.IsNullOrWhiteSpace(parameterAttribute.DisplayName)) {
                    DisplayName = parameterAttribute.DisplayName;
                }
                if (!string.IsNullOrWhiteSpace(parameterAttribute.Description)) {
                    Description = parameterAttribute.Description;
                }
            }

            DisplayName ??= ObjectNames.NicifyVariableName(ParameterInfo.Name);
        }
    }
}