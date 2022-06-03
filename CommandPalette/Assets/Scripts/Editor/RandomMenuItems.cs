﻿using System;
using CommandPalette;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Editor {
    public static class RandomMenuItems {
        [MenuItem("Tools/Find All Windows", false, 101)]
        private static void FindAllWindows() {
            EditorWindow[] windows = Resources.FindObjectsOfTypeAll<EditorWindow>();
            foreach (EditorWindow window in windows) {
                Debug.Log($"{window.titleContent} ({window.GetType()})");
            }
        }

        [MenuItem("Tools/Recompile Code", false, 101)]
        private static void RecompileCode() {
            CompilationPipeline.RequestScriptCompilation();
        }

        [MenuItem("Tools/Unload Unused Assets", false, 101)]
        private static void UnloadUnusedAssets() {
            GC.Collect();
            Resources.UnloadUnusedAssets();
        }

        [MenuItem("Tools/Destroy All Command Palettes")]
        private static void DestroyAllCommandPalettes() {
            CommandPaletteWindow[] windows = Resources.FindObjectsOfTypeAll<CommandPaletteWindow>();
            foreach (CommandPaletteWindow window in windows) {
                try {
                    window.Close();
                } catch {
                    Object.DestroyImmediate(window);
                }
            }
        }
    }
}