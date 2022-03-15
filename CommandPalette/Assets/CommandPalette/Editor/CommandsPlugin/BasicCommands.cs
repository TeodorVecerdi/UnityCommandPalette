using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using TypeCache = CommandPalette.Utils.TypeCache;

namespace CommandPalette.CommandsPlugin {
    public static class BasicCommands {
        private static readonly MethodInfo clearConsoleMethod = TypeCache.GetTypesByFullName("UnityEditor.LogEntries").FirstOrDefault()?.GetMethod("Clear", BindingFlags.Static | BindingFlags.Public);

        [CommandValidateMethod] private static bool ValidateEnterPlayMode() => !EditorApplication.isPlaying;
        [CommandValidateMethod] private static bool ValidateExitPlayMode() => EditorApplication.isPlaying;
        [CommandValidateMethod] private static bool ClearConsoleMethodExists() => clearConsoleMethod != null;

        [Command(Description = "Adds two numbers and prints the result to the console")]
        private static void AddTwoNumbers(
            [Parameter(Name = "Parameter", Description = "Does absolutely nothing!")]
            int i,
            [Parameter(Description = "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua.")]
            GameObject gameObject,
            Matrix4x4 matrix, Vector3 vector3, float a = 1.234f, float b = 5.678f
        ) {
            Debug.Log($"{a} + {b} = {a + b}");
        }

        [Command(ValidationMethod = nameof(ValidateEnterPlayMode))]
        private static void EnterPlayMode() {
            EditorApplication.isPlaying = true;
        }

        [Command(ValidationMethod = nameof(ValidateExitPlayMode))]
        private static void ExitPlayMode() {
            EditorApplication.isPlaying = false;
        }

        [Command(ShortName = "CLR", ValidationMethod = nameof(ClearConsoleMethodExists))]
        private static void ClearConsoleEntries() {
            clearConsoleMethod.Invoke(null, null);
        }

        [Command]
        private static void OpenPreferences() {
            EditorApplication.ExecuteMenuItem("Edit/Preferences...");
        }

        [Command]
        private static void OpenProjectSettings() {
            EditorApplication.ExecuteMenuItem("Edit/Project Settings...");
        }

        [Command]
        private static void OpenPackageManager() {
            EditorApplication.ExecuteMenuItem("Window/Package Manager");
        }

        [Command]
        private static void FocusGameView() {
            EditorApplication.ExecuteMenuItem("Window/General/Game");
        }

        [Command]
        private static void FocusSceneView() {
            EditorApplication.ExecuteMenuItem("Window/General/Scene");
        }

        [Command]
        private static void FocusInspector() {
            EditorApplication.ExecuteMenuItem("Window/General/Inspector");
        }

        [Command]
        private static void FocusHierarchy() {
            EditorApplication.ExecuteMenuItem("Window/General/Hierarchy");
        }

        [Command]
        private static void FocusConsole() {
            EditorApplication.ExecuteMenuItem("Window/General/Console");
        }

        [Command]
        private static void FocusProject() {
            EditorApplication.ExecuteMenuItem("Window/General/Project");
        }

        [Command]
        private static void OpenBuildSettings() {
            EditorApplication.ExecuteMenuItem("File/Build Settings...");
        }
    }
}