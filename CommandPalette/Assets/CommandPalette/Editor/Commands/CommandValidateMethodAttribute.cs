using System;

namespace CommandPalette.Commands {
    [AttributeUsage(AttributeTargets.Method)]
    public class CommandValidateMethodAttribute : Attribute {
    }
}