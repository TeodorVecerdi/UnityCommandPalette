using System;
using JetBrains.Annotations;

namespace CommandPalette.Basic {
    [AttributeUsage(AttributeTargets.Method), MeansImplicitUse]
    public class InlineParameterValuesProviderAttribute : Attribute {
    }
}