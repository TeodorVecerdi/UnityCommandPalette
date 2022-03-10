using System.Reflection;

namespace CommandPalette.Commands {
    public readonly struct CommandEntry {
        public readonly string DisplayName;
        public readonly string ShortName;
        public readonly string Description;
        public readonly MethodInfo Method;
        public readonly MethodInfo ValidationMethod;
        public readonly bool HasParameters;

        public CommandEntry(string displayName, string shortName, string description, MethodInfo method, MethodInfo validationMethod) {
            DisplayName = displayName;
            ShortName = shortName;
            Description = description;
            Method = method;
            ValidationMethod = validationMethod;
            HasParameters = method.GetParameters().Length > 0;
        }
    }
}