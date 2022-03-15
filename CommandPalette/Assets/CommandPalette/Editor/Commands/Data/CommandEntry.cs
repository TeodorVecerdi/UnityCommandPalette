using System.Linq;
using System.Reflection;

namespace CommandPalette.Commands {
    public readonly struct CommandEntry {
        public readonly string DisplayName;
        public readonly string ShortName;
        public readonly string Description;
        public readonly MethodInfo Method;
        public readonly MethodInfo ValidationMethod;

        public readonly bool HasParameters;
        public readonly Parameter[] Parameters;

        public CommandEntry(string displayName, string shortName, string description, MethodInfo method, MethodInfo validationMethod) {
            DisplayName = displayName;
            ShortName = shortName;
            Description = description;
            Method = method;
            ValidationMethod = validationMethod;

            ParameterInfo[] methodParameters = method.GetParameters();
            HasParameters = methodParameters.Length > 0;
            Parameters = HasParameters ? methodParameters.Select(param => new Parameter(param)).ToArray() : default;
        }
    }
}