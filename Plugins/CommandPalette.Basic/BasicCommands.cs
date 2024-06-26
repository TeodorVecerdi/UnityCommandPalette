﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using CommandPalette.Core;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CommandPalette.Basic {
    public static class BasicCommands {
        private static readonly MethodInfo? s_ClearConsoleMethod = Type.GetType("UnityEditor.LogEntries, UnityEditor")?.GetMethod("Clear", BindingFlags.Static | BindingFlags.Public);
        private static readonly MethodInfo? s_GetActiveFolderPath = Type.GetType("UnityEditor.ProjectWindowUtil, UnityEditor")?.GetMethod("GetActiveFolderPath", BindingFlags.Static | BindingFlags.NonPublic);

        [CommandValidateMethod] private static bool ValidateEnterPlayMode() => !EditorApplication.isPlaying;
        [CommandValidateMethod] private static bool ValidateExitPlayMode() => EditorApplication.isPlaying;
        [CommandValidateMethod] private static bool ClearConsoleMethodExists() => s_ClearConsoleMethod != null;
        [CommandValidateMethod] private static bool GetActiveFolderPathExists() => s_GetActiveFolderPath != null;

        [InlineParameterValuesProvider]
        private static InlineParameterValues<string> OpenScene_GetScenesProvider() {
            return new InlineParameterValues<string>(
                EditorBuildSettings.scenes
                                   .Select(scene => GetSceneInlineParameter(scene.path))
                                   .Concat(GetExtraScenes())
            );
        }

        private static InlineParameterResultEntry<string> GetSceneInlineParameter(string path) {
            return new InlineParameterResultEntry<string>(
                path, new ResultDisplaySettings(Path.GetFileNameWithoutExtension(path), null, path, IconResource.FromBuiltinIcon("d_unitylogo"))
            );
        }

        private static IEnumerable<InlineParameterResultEntry<string>> GetExtraScenes() {
            yield break;
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
                new("Window/General/Game", new ResultDisplaySettings("Game View")),
                new("Window/General/Scene", new ResultDisplaySettings("Scene View")),
                new("Window/General/Inspector", new ResultDisplaySettings("Inspector")),
                new("Window/General/Hierarchy", new ResultDisplaySettings("Hierarchy")),
                new("Window/General/Console", new ResultDisplaySettings("Console")),
                new("Window/Package Manager", new ResultDisplaySettings("Package Manager")),
                new("Window/General/Project", new ResultDisplaySettings("Project")),
                new("Edit/Project Settings...", new ResultDisplaySettings("Project Settings")),
                new("Edit/Preferences...", new ResultDisplaySettings("Preferences")),
                new("File/Build Settings...", new ResultDisplaySettings("Build Settings")),
            };
        }

        [Command(IconPath = "r:d_unitylogo", Priority = 100.0f)]
        private static void OpenScene([InlineParameter(nameof(OpenScene_GetScenesProvider))] string scenePath) {
            if (string.IsNullOrEmpty(scenePath)) {
                return;
            }

            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) {
                EditorSceneManager.OpenScene(scenePath);
            }
        }

        [Command(IconPath = "r:d_unitylogo", Priority = 90.0f)]
        private static void ReloadScene() {
            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) {
                EditorSceneManager.OpenScene(SceneManager.GetActiveScene().path);
            }
        }

        [Command(IconPath = "r:crossicon", Priority = 40.0f)]
        private static void ClearSelection() {
            Selection.activeObject = null;
        }

        [Command(DisplayName = "Open/Focus Window", ShortName = "OPN", Description = "Opens or focuses the selected window if it is already open.", IconPath = "r:d_panelsettings on icon", Priority = 30.0f)]
        private static void OpenWindow([InlineParameter(nameof(OpenWindow_ValuesProvider))]string menuPath) {
            EditorApplication.ExecuteMenuItem(menuPath);
        }

        [Command(IconPath = "r:d_guiskin on icon", Priority = 29.5f)]
        private static void CloseWindow([InlineParameter(nameof(CloseWindow_ValuesProvider))] EditorWindow window) {
            if (window == null) {
                return;
            }

            window.Close();
        }

        [Command(IconPath="r:d_panelsettings on icon", ShowOnlyWhenSearching=true)]
        private static void OpenGameViewWindow() {
            EditorApplication.ExecuteMenuItem("Window/General/Game");
        }

        [Command(IconPath="r:d_panelsettings on icon", ShowOnlyWhenSearching=true)]
        private static void OpenSceneViewWindow() {
            EditorApplication.ExecuteMenuItem("Window/General/Scene");
        }

        [Command(IconPath="r:d_panelsettings on icon", ShowOnlyWhenSearching=true)]
        private static void OpenInspectorWindow() {
            EditorApplication.ExecuteMenuItem("Window/General/Inspector");
        }

        [Command(IconPath="r:d_panelsettings on icon", ShowOnlyWhenSearching=true)]
        private static void OpenHierarchyWindow() {
            EditorApplication.ExecuteMenuItem("Window/General/Hierarchy");
        }

        [Command(IconPath="r:d_panelsettings on icon", ShowOnlyWhenSearching=true)]
        private static void OpenConsoleWindow() {
            EditorApplication.ExecuteMenuItem("Window/General/Console");
        }

        [Command(IconPath="r:d_panelsettings on icon", ShowOnlyWhenSearching=true)]
        private static void OpenPackageManagerWindow() {
            EditorApplication.ExecuteMenuItem("Window/Package Manager");
        }

        [Command(IconPath="r:d_panelsettings on icon", ShowOnlyWhenSearching=true)]
        private static void OpenProjectWindow() {
            EditorApplication.ExecuteMenuItem("Window/General/Project");
        }

        [Command(IconPath="r:d_panelsettings on icon", ShowOnlyWhenSearching=true)]
        private static void OpenProjectSettingsWindow() {
            EditorApplication.ExecuteMenuItem("Edit/Project Settings...");
        }

        [Command(IconPath="r:d_panelsettings on icon", ShowOnlyWhenSearching=true)]
        private static void OpenPreferencesWindow() {
            EditorApplication.ExecuteMenuItem("Edit/Preferences...");
        }

        [Command(IconPath="r:d_panelsettings on icon", ShowOnlyWhenSearching=true)]
        private static void OpenBuildSettingsWindow() {
            EditorApplication.ExecuteMenuItem("File/Build Settings...");
        }

        [Command(ValidationMethod = nameof(GetActiveFolderPathExists), IconPath = "r:folder on icon")]
        private static void CreateFolder() {
            var path = s_GetActiveFolderPath!.Invoke(null, null) as string;
            if (string.IsNullOrEmpty(path)) {
                return;
            }

            Selection.activeObject = AssetDatabase.LoadMainAssetAtPath(path);
            ProjectWindowUtil.CreateFolder();
        }

        [Command(ValidationMethod = nameof(ValidateEnterPlayMode), IconPath = "Textures/play.png")]
        private static void EnterPlayMode() {
            EditorApplication.isPlaying = true;
        }

        [Command(ValidationMethod = nameof(ValidateExitPlayMode), IconPath = "Textures/stop.png")]
        private static void ExitPlayMode() {
            EditorApplication.isPlaying = false;
        }

        [Command(ShortName = "CLR", ValidationMethod = nameof(ClearConsoleMethodExists), IconPath = "r:crossicon")]
        private static void ClearConsoleEntries() {
            s_ClearConsoleMethod!.Invoke(null, null);
        }
    }
}
