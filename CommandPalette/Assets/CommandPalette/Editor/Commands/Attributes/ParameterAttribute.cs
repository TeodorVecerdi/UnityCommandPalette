using System;

namespace CommandPalette.Commands {
    [AttributeUsage(AttributeTargets.Parameter)]
    public class ParameterAttribute : Attribute{
        public string DisplayName { get; set; }
        public string Description { get; set; }
    }
}