#nullable enable

namespace CommandPalette.Resource {
    public interface IResourcePathProvider {
        public string GetResourcePath(string path);
    }
}