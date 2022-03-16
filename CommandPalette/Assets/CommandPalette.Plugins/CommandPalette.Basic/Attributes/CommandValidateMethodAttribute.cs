using System;

namespace CommandPalette.CommandsPlugin {
    [AttributeUsage(AttributeTargets.Method)]
    public class CommandValidateMethodAttribute : Attribute {
    }
}