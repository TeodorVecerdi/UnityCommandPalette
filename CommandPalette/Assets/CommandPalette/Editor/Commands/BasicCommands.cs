using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace CommandPalette.Commands {
    public static class BasicCommands {
        private static readonly MethodInfo clearConsoleMethod = Type.GetType("UnityEditor.LogEntries,UnityEditor.dll")?.GetMethod("Clear", BindingFlags.Static | BindingFlags.Public);

        [CommandValidateMethod] private static bool ValidateEnterPlayMode() => !EditorApplication.isPlaying;
        [CommandValidateMethod] private static bool ValidateExitPlayMode() => EditorApplication.isPlaying;

        [Command]
        private static void AddTwoNumbers(int i, GameObject gameObject, Matrix4x4 matrix, Vector3 vector3, float a = 1.234f, float b = 5.678f) {
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

        [Command(ShortName = "CLR")]
        private static void ClearConsoleEntries() {
            clearConsoleMethod?.Invoke(null, null);
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