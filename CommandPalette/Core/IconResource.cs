﻿using CommandPalette.Resource;
using CommandPalette.Utils;
using UnityEditor;
using UnityEngine;

namespace CommandPalette.Core {
    public struct IconResource {
        private readonly string? m_ResourceName;
        private readonly string? m_ResourcePath;
        private Texture? m_Texture;

        private IconResource(string? resourceName, string? resourcePath, Texture? texture) {
            m_ResourceName = resourceName;
            m_ResourcePath = resourcePath;
            m_Texture = texture;
        }

        public Texture? GetTexture(IResourcePathProvider? resourcePathProvider = null) {
            if (m_Texture != null) {
                return m_Texture;
            }

            if (!string.IsNullOrEmpty(m_ResourceName)) {
                return m_Texture = EditorGUIUtility.IconContent(m_ResourceName).image;
            }

            if (!string.IsNullOrEmpty(m_ResourcePath)) {
                return m_Texture = ResourceLoader.Load<Texture>(m_ResourcePath, resourcePathProvider);
            }

            return null;
        }

        public static IconResource FromBuiltinIcon(string resourceName)
            => new(resourceName, null, null);

        public static IconResource FromResource(string resourcePath)
            => new(null, resourcePath, null);

        public static IconResource FromTexture(Texture texture)
            => new(null, null, texture);

        public static IconResource Parse(string resource) {
            if (string.IsNullOrWhiteSpace(resource)) return default;

            if (resource.StartsWith("r:")) return FromBuiltinIcon(resource.Substring(2));
            return FromResource(resource);
        }

        public static implicit operator IconResource(string resource) => Parse(resource);
        public static implicit operator IconResource(Texture texture) => FromTexture(texture);
    }
}
