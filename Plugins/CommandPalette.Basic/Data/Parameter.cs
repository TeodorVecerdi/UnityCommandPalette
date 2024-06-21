using System;
using System.Reflection;
using UnityEditor;

namespace CommandPalette.Basic {
    public readonly struct Parameter {
        public readonly ParameterInfo ParameterInfo;
        public readonly bool HasDefaultValue;
        public readonly object? DefaultValue;
        public readonly Type Type;

        public readonly string? DisplayName;
        public readonly string? Description;

        public readonly bool HasInlineSupport;
        public readonly MethodInfo? InlineValuesProvider;

        public Parameter(ParameterInfo parameterInfo) {
            ParameterInfo = parameterInfo;
            HasDefaultValue = parameterInfo.HasDefaultValue;
            DefaultValue = parameterInfo.DefaultValue;
            Type = parameterInfo.ParameterType;

            DisplayName = null;
            Description = null;

            if (ParameterInfo.GetCustomAttribute<ParameterAttribute>() is { } parameterAttribute) {
                if (!string.IsNullOrWhiteSpace(parameterAttribute.Name)) {
                    DisplayName = parameterAttribute.Name;
                }
                if (!string.IsNullOrWhiteSpace(parameterAttribute.Description)) {
                    Description = parameterAttribute.Description;
                }
            }

            DisplayName ??= ObjectNames.NicifyVariableName(ParameterInfo.Name);

            HasInlineSupport = false;
            InlineValuesProvider = null;
            if (ParameterInfo.GetCustomAttribute<InlineParameterAttribute>() is { } inlineParameterAttribute) {
                if (CommandPaletteDriver.ParameterValueProviders.TryGetValue(inlineParameterAttribute.ValuesMethod, out var valuesMethod)) {
                    HasInlineSupport = valuesMethod.ReturnType == typeof(InlineParameterValues<>).MakeGenericType(Type);
                    InlineValuesProvider = valuesMethod;
                }
            }
        }
    }
}