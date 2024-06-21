using System.IO;

namespace CommandPalette.Resource {
    internal class DefaultResourcePathProvider : IResourcePathProvider {
        public string GetResourcePath(string path) {
            return Path.Combine(CommandPalettePackageLocator.GetCommandPaletteAssetPath(), "CommandPalette", "EditorResources", path).Replace("\\", "/");
        }
    }
}