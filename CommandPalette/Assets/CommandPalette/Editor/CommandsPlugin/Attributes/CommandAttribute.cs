using System;

namespace CommandPalette.CommandsPlugin {
    [AttributeUsage(AttributeTargets.Method)]
    public class CommandAttribute : Attribute {
        public string DisplayName { get; set; }
        public string ShortName { get; set; }
        public string Description { get; set; }
        public string ValidationMethod { get; set; }
    }
}