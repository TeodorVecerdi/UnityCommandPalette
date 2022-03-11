using System;
using System.Linq;
using System.Reflection;
using CommandPalette.Utils;

namespace CommandPalette.Commands {
    public readonly struct CommandEntry {
        public readonly string DisplayName;
        public readonly string ShortName;
        public readonly string Description;
        public readonly MethodInfo Method;
        public readonly MethodInfo ValidationMethod;

        public readonly bool HasParameters;
        public readonly CommandParameters Parameters;

        public CommandEntry(string displayName, string shortName, string description, MethodInfo method, MethodInfo validationMethod) {
            DisplayName = displayName;
            ShortName = shortName;
            Description = description;
            Method = method;
            ValidationMethod = validationMethod;

            ParameterInfo[] methodParameters = method.GetParameters();
            HasParameters = methodParameters.Length > 0;
            Parameters = HasParameters ? new CommandParameters(methodParameters) : default;
        }
    }

    public readonly struct CommandParameters {
        public readonly ParameterInfo[] Parameters;
        public readonly Type[] ParameterTypes;
        public readonly object[] DefaultValues;
        public readonly bool[] HasDefaultValues;

        public CommandParameters(ParameterInfo[] parameters) {
            Parameters = parameters;
            ParameterTypes = new Type[parameters.Length];
            DefaultValues = new object[Parameters.Length];
            HasDefaultValues = new bool[Parameters.Length];

            for (int i = 0; i < Parameters.Length; i++) {
                ParameterTypes[i] = Parameters[i].ParameterType;
                DefaultValues[i] = Parameters[i].DefaultValue;
                HasDefaultValues[i] = Parameters[i].HasDefaultValue;
            }
        }

        public string Dump(string pre = "", string join = ", ") {
            string[] parameterStrings = new string[Parameters.Length];
            for (int i = 0; i < Parameters.Length; i++) {
                parameterStrings[i] = $"{pre}{ParameterTypes[i].Name} {Parameters[i].Name}";
                if (HasDefaultValues[i]) {
                    parameterStrings[i] += $" = {DefaultValues[i] ?? "null"}";
                }
            }
            return string.Join(join, parameterStrings);
        }
    }

    public class CommandParameterValues {
        public readonly CommandParameters CommandParameters;
        public readonly object[] Values;

        public CommandParameterValues(CommandParameters commandParameters) {
            CommandParameters = commandParameters;
            Values = new object[CommandParameters.Parameters.Length];

            for (int i = 0; i < CommandParameters.Parameters.Length; i++) {
                if (CommandParameters.HasDefaultValues[i]) {
                    Values[i] = CommandParameters.DefaultValues[i];
                } else {
                    Values[i] = ReflectionHelper.GetDefaultValue(CommandParameters.ParameterTypes[i]);
                }
            }
        }
    }
}