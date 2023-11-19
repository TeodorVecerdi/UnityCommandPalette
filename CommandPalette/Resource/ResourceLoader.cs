#nullable enable

using CommandPalette.Resource;
using UnityEditor;
using UnityEngine;

namespace CommandPalette.Utils {
    public static class ResourceLoader {
        public static T? Load<T>(string resourcePath, IResourcePathProvider? resourcePathProvider = null) where T : Object {
            resourcePathProvider ??= new DefaultResourcePathProvider();
            return AssetDatabase.LoadAssetAtPath<T>(resourcePathProvider.GetResourcePath(resourcePath));
        }
    }
}