using System.IO;
using System.Linq;
using System.Reflection;
using CommandPalette.Core;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using TypeCache = CommandPalette.Utils.TypeCache;

namespace CommandPalette.Basic {
    public static class BasicCommands {
        private static readonly MethodInfo clearConsoleMethod = TypeCache.GetTypesByFullName("UnityEditor.LogEntries").FirstOrDefault()?.GetMethod("Clear", BindingFlags.Static | BindingFlags.Public);

        [CommandValidateMethod] private static bool ValidateEnterPlayMode() => !EditorApplication.isPlaying;
        [CommandValidateMethod] private static bool ValidateExitPlayMode() => EditorApplication.isPlaying;
        [CommandValidateMethod] private static bool ClearConsoleMethodExists() => clearConsoleMethod != null;

        [InlineParameterValuesProvider]
        private static InlineParameterValues<string> OpenScene_GetScenesProvider() {
            return new InlineParameterValues<string>(
                AssetDatabase
                    .GetAllAssetPaths()
                    .Where(path => path.EndsWith(".unity"))
                    .Select(path => new InlineParameterResultEntry<string>(
                                path, new ResultDisplaySettings(Path.GetFileNameWithoutExtension(path), null, path, IconResource.FromBuiltinIcon("d_unitylogo"))
                            )
                    ));
        }

        [InlineParameterValuesProvider]
        private static InlineParameterValues<EditorWindow> CloseWindow_ValuesProvider() {
            return new InlineParameterValues<EditorWindow>(
                Resources.FindObjectsOfTypeAll<EditorWindow>()
                         .Where(window => window is not CommandPaletteWindow)
                         .Select(window => new InlineParameterResultEntry<EditorWindow>(
                                     window, new ResultDisplaySettings(window.titleContent.ToString().Trim(), null, window.GetType().FullName, IconResource.FromTexture(window.titleContent.image))
                                 )
                         ));
        }

        [InlineParameterValuesProvider]
        private static InlineParameterValues<string> OpenWindow_ValuesProvider() {
            return new InlineParameterValues<string> {
                new InlineParameterResultEntry("Window/General/Game", new ResultDisplaySettings("Game View", null, null)),
                new InlineParameterResultEntry("Window/General/Scene", new ResultDisplaySettings("Scene View", null, null)),
                new InlineParameterResultEntry("Window/General/Inspector", new ResultDisplaySettings("Inspector", null, null)),
                new InlineParameterResultEntry("Window/General/Hierarchy", new ResultDisplaySettings("Hierarchy", null, null)),
                new InlineParameterResultEntry("Window/General/Console", new ResultDisplaySettings("Console", null, null)),
                new InlineParameterResultEntry("Window/Package Manager", new ResultDisplaySettings("Package Manager", null, null)),
                new InlineParameterResultEntry("Window/General/Project", new ResultDisplaySettings("Project", null, null)),
                new InlineParameterResultEntry("Edit/Project Settings...", new ResultDisplaySettings("Project Settings", null, null)),
                new InlineParameterResultEntry("Edit/Preferences...", new ResultDisplaySettings("Preferences", null, null)),
                new InlineParameterResultEntry("File/Build Settings...", new ResultDisplaySettings("Build Settings", null, null)),
            };
        }

        [Command(IconPath = "r:d_unitylogo")]
        private static void OpenScene([InlineParameter(nameof(OpenScene_GetScenesProvider))] string scenePath) {
            if (string.IsNullOrEmpty(scenePath)) {
                return;
            }

            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) {
                EditorSceneManager.OpenScene(scenePath);
            }
        }

        [Command(DisplayName = "Open/Focus Window", ShortName = "OPN", Description = "Opens or focuses the selected window if it is already open.", IconPath = "r:d_panelsettings on icon")]
        private static void OpenWindow([InlineParameter(nameof(OpenWindow_ValuesProvider))]string menuPath) {
            EditorApplication.ExecuteMenuItem(menuPath);
        }

        [Command(IconPath = "r:d_guiskin on icon")]
        private static void CloseWindow([InlineParameter(nameof(CloseWindow_ValuesProvider))] EditorWindow window) {
            if (window == null) {
                return;
            }

            window.Close();
        }

        [Command(ValidationMethod = nameof(ValidateEnterPlayMode), IconPath = "CommandPalette.Basic/Textures/play")]
        private static void EnterPlayMode() {
            EditorApplication.isPlaying = true;
        }

        [Command(ValidationMethod = nameof(ValidateExitPlayMode), IconPath = "CommandPalette.Basic/Textures/stop")]
        private static void ExitPlayMode() {
            EditorApplication.isPlaying = false;
        }

        [Command(ShortName = "CLR", ValidationMethod = nameof(ClearConsoleMethodExists))]
        private static void ClearConsoleEntries() {
            clearConsoleMethod.Invoke(null, null);
        }
    }
}