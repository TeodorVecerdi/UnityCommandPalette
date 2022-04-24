using System;
using CommandPalette.Core;
using JetBrains.Annotations;

namespace CommandPalette.Basic {
    [AttributeUsage(AttributeTargets.Method), MeansImplicitUse]
    public class CommandAttribute : Attribute {
        public string DisplayName { get; set; }
        public string ShortName { get; set; }
        public string Description { get; set; }
        public string ValidationMethod { get; set; }
        public string IconPath { get; set; }
        public bool ShowOnlyWhenSearching { get; set; }
        public IconResource Icon => IconResource.Parse(IconPath);
    }
}