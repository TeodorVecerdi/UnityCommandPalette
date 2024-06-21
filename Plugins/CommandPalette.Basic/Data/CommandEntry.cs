using System;
using System.Linq;
using System.Reflection;
using CommandPalette.Core;

namespace CommandPalette.Basic {
    public readonly struct CommandEntry {
        public readonly string DisplayName;
        public readonly string ShortName;
        public readonly string Description;
        public readonly bool ShowOnlyWhenSearching;
        public readonly MethodInfo Method;
        public readonly MethodInfo? ValidationMethod;
        public readonly IconResource Icon;
        public readonly float Priority;

        public readonly bool HasParameters;
        public readonly Parameter[] Parameters;

        public readonly bool HasInlineSupport;

        public CommandEntry(string displayName, string shortName, string description, bool showOnlyWhenSearching, MethodInfo method, MethodInfo? validationMethod, IconResource icon, float priority) {
            DisplayName = displayName;
            ShortName = shortName;
            Description = description;
            ShowOnlyWhenSearching = showOnlyWhenSearching;
            Method = method;
            ValidationMethod = validationMethod;
            Icon = icon;
            Priority = priority;

            var methodParameters = method.GetParameters();
            HasParameters = methodParameters.Length > 0;
            Parameters = HasParameters ? methodParameters.Select(param => new Parameter(param)).ToArray() : Array.Empty<Parameter>();
            HasInlineSupport = HasParameters && Parameters is { Length: >= 1 } && Parameters[0].HasInlineSupport;
        }
    }
}
