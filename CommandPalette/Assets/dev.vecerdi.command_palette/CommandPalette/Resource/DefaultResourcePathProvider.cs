#nullable enable

using System.IO;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace CommandPalette.Resource {
    internal class DefaultResourcePathProvider : IResourcePathProvider {
        public string GetResourcePath(string path) {
            return Path.Combine(CommandPalettePackageLocator.GetCommandPaletteAssetPath(), "EditorResources", path).Replace("\\", "/");
        }
    }
}