using System.IO;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace CommandPalette {
    internal static class CommandPalettePackageLocator {
        public static string GetCommandPaletteFullPath() {
            // Go up two levels from the current file path
            return Path.GetDirectoryName(Path.GetDirectoryName(PathHelper()));
        }

        public static string GetCommandPaletteAssetPath() {
            return GetCommandPaletteFullPath().Replace("\\", "/").Replace(Application.dataPath, "Assets");
        }

        private static string PathHelper([CallerFilePath] string path = "") => path;
    }
}