using System;
using CommandPalette.Core;

namespace CommandPalette.Basic {
    public class InlineParameterResultEntry : ResultEntry {
        public object Value { get; }
        public new Action<InlineParameterResultEntry> OnSelect { get; set; }

        public InlineParameterResultEntry(object value, ResultDisplaySettings displaySettings) : base(displaySettings, 0, null, CommandsPlugin.ResourcePathProvider) {
            Value = value;
        }
    }
    public class InlineParameterResultEntry<T> : InlineParameterResultEntry {
        public InlineParameterResultEntry(T value, ResultDisplaySettings displaySettings) : base(value, displaySettings) {
        }
    }
}