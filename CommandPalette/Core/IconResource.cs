using UnityEditor;
using UnityEngine;

namespace CommandPalette.Core {
    public struct IconResource {
        private readonly string m_ResourceName;
        private readonly string m_ResourcePath;
        private Texture m_Texture;

        private IconResource(string resourceName, string resourcePath, Texture texture) {
            this.m_ResourceName = resourceName;
            this.m_ResourcePath = resourcePath;
            this.m_Texture = texture;
        }

        public Texture GetTexture() {
            if (m_Texture != null) {
                return m_Texture;
            }

            if (!string.IsNullOrEmpty(m_ResourceName)) {
                return m_Texture = EditorGUIUtility.IconContent(m_ResourceName).image;
            }

            if (!string.IsNullOrEmpty(m_ResourcePath)) {
                return m_Texture = Resources.Load<Texture>(m_ResourcePath);
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