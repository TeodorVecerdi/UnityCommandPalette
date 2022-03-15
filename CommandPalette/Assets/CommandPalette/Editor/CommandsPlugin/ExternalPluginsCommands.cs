using System;
using System.Linq;
using System.Reflection;
using CommandPalette.Utils;
using UnityEngine;

namespace CommandPalette.CommandsPlugin {
    public static class ExternalPluginsCommands {
        private static readonly Type consolePro3WindowType = TypeCache.GetTypesByFullName("FlyingWormConsole3.ConsolePro3Window").FirstOrDefault();
        private static readonly MethodInfo consolePro3Window_CreateWindow = consolePro3WindowType?.GetMethod("CreateWindow", BindingFlags.Static | BindingFlags.Public);

        [CommandValidateMethod] private static bool ShowConsolePro3Commands() => consolePro3WindowType != null;

        [Command(ValidationMethod = nameof(ShowConsolePro3Commands))]
        private static void OpenConsolePro3() {
            if (consolePro3WindowType == null || consolePro3Window_CreateWindow == null) {
                Debug.LogError("ConsolePro3 is not in project or was not found.");
                return;
            }

            consolePro3Window_CreateWindow.Invoke(null, null);
        }
    }
}