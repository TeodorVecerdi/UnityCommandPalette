using System;
using CommandPalette.Core;

namespace CommandPalette.Basic {
    [AttributeUsage(AttributeTargets.Method)]
    public class CommandAttribute : Attribute {
        public string DisplayName { get; set; }
        public string ShortName { get; set; }
        public string Description { get; set; }
        public string ValidationMethod { get; set; }
        public string IconPath { get; set; }
        public IconResource Icon => IconResource.Parse(IconPath);
    }
}