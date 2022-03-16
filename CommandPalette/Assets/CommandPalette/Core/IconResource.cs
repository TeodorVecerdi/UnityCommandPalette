using UnityEditor;
using UnityEngine;

namespace CommandPalette.Core {
    public struct IconResource {
        private readonly string resourceName;
        private readonly string resourcePath;
        private Texture texture;

        private IconResource(string resourceName, string resourcePath, Texture texture) {
            this.resourceName = resourceName;
            this.resourcePath = resourcePath;
            this.texture = texture;
        }

        public Texture GetTexture() {
            if (texture != null) {
                return texture;
            }

            if (!string.IsNullOrEmpty(resourceName)) {
                return texture = EditorGUIUtility.IconContent(resourceName).image;
            }

            if (!string.IsNullOrEmpty(resourcePath)) {
                return texture = Resources.Load<Texture>(resourcePath);
            }

            return null;
        }

        public static IconResource FromBuiltinIcon(string resourceName) {
            return new IconResource(resourceName, null, null);
        }

        public static IconResource FromResource(string resourcePath) {
            return new IconResource(null, resourcePath, null);
        }

        public static IconResource FromTexture(Texture texture) {
            return new IconResource(null, null, texture);
        }

        public static IconResource Parse(string resource) {
            if (string.IsNullOrWhiteSpace(resource)) return default;

            if (resource.StartsWith("r:")) return FromBuiltinIcon(resource.Substring(2));
            return FromResource(resource);
        }

        public static implicit operator IconResource(string resource) {
            return Parse(resource);
        }

        public static implicit operator IconResource (Texture texture) {
            return FromTexture(texture);
        }
    }
}