using System;
using System.Reflection;
using UnityEngine;

namespace CommandPalette.Basic {
    public static class ExternalPluginsCommands {
        private static readonly MethodInfo s_consolePro3Window_CreateWindow = Type.GetType("FlyingWormConsole3.ConsolePro3Window, ConsolePro.Editor")?.GetMethod("CreateWindow", BindingFlags.Static | BindingFlags.Public);

        [CommandValidateMethod] private static bool ShowConsolePro3Commands() => s_consolePro3Window_CreateWindow != null;

        [Command(ValidationMethod = nameof(ShowConsolePro3Commands), IconPath = "r:console.infoicon")]
        private static void OpenConsolePro3() {
            if (s_consolePro3Window_CreateWindow == null) {
                Debug.LogError("ConsolePro3 is not in project or was not found.");
                return;
            }

            s_consolePro3Window_CreateWindow.Invoke(null, null);
        }
    }
}