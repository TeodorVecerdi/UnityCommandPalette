using System.Collections.Generic;
using CommandPalette.Core;

namespace CommandPalette.Plugins {
    public interface IPlugin {
        float PriorityMultiplier { get; }
        CommandPaletteWindow Window { get; set; }

        bool IsValid(Query query);
        IEnumerable<ResultEntry> GetResults(Query query);
    }
}