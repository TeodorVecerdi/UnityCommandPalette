using CommandPalette.Utils;

namespace CommandPalette.Basic {
    public class CommandParameterValues {
        public readonly Parameter[] Parameters;
        public readonly object[] Values;

        public CommandParameterValues(Parameter[] parameters) {
            Parameters = parameters;
            Values = new object[Parameters.Length];

            for (var i = 0; i < Parameters.Length; i++) {
                if (Parameters[i].HasDefaultValue) {
                    Values[i] = Parameters[i].DefaultValue;
                } else {
                    Values[i] = ReflectionHelper.GetDefaultValue(Parameters[i].Type);
                }
            }
        }
    }
}