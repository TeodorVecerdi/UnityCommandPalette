using System;

namespace CommandPalette.Basic {
    [AttributeUsage(AttributeTargets.Parameter)]
    public class ParameterAttribute : Attribute {
        public string Name { get; set; }
        public string Description { get; set; }
    }

    [AttributeUsage(AttributeTargets.Parameter)]
    public class InlineParameterAttribute : Attribute {
        public string ValuesMethod { get; }

        public InlineParameterAttribute(string valuesMethod) {
            ValuesMethod = valuesMethod;
        }
    }

}