using System;

namespace CommandPalette.CommandsPlugin {
    [AttributeUsage(AttributeTargets.Parameter)]
    public class ParameterAttribute : Attribute {
        public string Name { get; set; }
        public string Description { get; set; }
    }
}