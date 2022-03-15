using System.Collections.Generic;
using CommandPalette.Core;

namespace CommandPalette.Plugins {
    public interface IPlugin {
        float PriorityMultiplier { get; }

        List<ResultEntry> GetResults(Query query);
        bool IsValid(Query query);
    }
}