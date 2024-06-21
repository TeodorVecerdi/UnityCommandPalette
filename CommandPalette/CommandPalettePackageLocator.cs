using System.IO;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace CommandPalette {
    internal static class CommandPalettePackageLocator {
        public static string GetCommandPaletteAssetPath() {
            return GetCommandPaletteFullPath().Replace("\\", "/").Replace(Application.dataPath, "Assets");
        }

        private static string GetCommandPaletteFullPath() {
            return Path.GetDirectoryName(Path.GetDirectoryName(PathHelper()));
        }

        private static string PathHelper([CallerFilePath] string path = "") => path;
    }
}