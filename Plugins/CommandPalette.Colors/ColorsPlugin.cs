using System;
using System.Collections.Generic;
using System.Linq;
using CommandPalette.Core;
using CommandPalette.Plugins;
using UnityEditor;
using UnityEngine;

namespace CommandPalette.Colors {
    public class ColorsPlugin : IPlugin {
        [InitializeOnLoadMethod]
        private static void InitializePlugin() {
            CommandPalette.RegisterPlugin(new ColorsPlugin());
        }

        public string Name { get; } = "Color Converter";
        public float PriorityMultiplier => 2.0f;
        public CommandPaletteWindow Window { get; set; }

        public bool IsValid(Query query) {
            return ColorHelper.IsValid(query.Text);
        }

        public IEnumerable<ResultEntry> GetResults(Query query) {
            if (!ColorHelper.IsValid(query.Text)) {
                return Enumerable.Empty<ResultEntry>();
            }

            Color? color = ColorHelper.Extract(query.Text);
            if (!color.HasValue) {
                return Enumerable.Empty<ResultEntry>();
            }

            if (Math.Abs(color.Value.a - 1.0f) > 0.001f) {
                return GenerateAlphaResults(color.Value);
            }

            return GenerateColorResults(color.Value);
        }

        private static IEnumerable<ResultEntry> GenerateColorResults(Color color) {
            byte r = (byte)Math.Round(color.r * 255.0f);
            byte g = (byte)Math.Round(color.g * 255.0f);
            byte b = (byte)Math.Round(color.b * 255.0f);
            Color.RGBToHSV(color, out float hsvH, out float hsvS, out float hsvV);
            ColorHelper.RgbToHsl(color, out float hslH, out float hslS, out float hslL);

            string hex = $"#{r:X2}{g:X2}{b:X2}";
            string rgb = $"rgb({r}, {g}, {b})";
            string hsv = $"hsv({hsvH*360.0f}, {hsvS}, {hsvV})";
            string hsl = $"hsl({hslH*360.0f}, {hslS}, {hslL})";

            yield return new ColorResultEntry(color, new ResultDisplaySettings(hex, null, "Copy to clipboard", IconResource.FromResource("CommandPalette.Colors/Textures/Square")), 100, _ => {
                GUIUtility.systemCopyBuffer = hex;
                return true;
            });

            yield return new ColorResultEntry(color, new ResultDisplaySettings(rgb, null, "Copy to clipboard", IconResource.FromResource("CommandPalette.Colors/Textures/Square")), 100, _ => {
                GUIUtility.systemCopyBuffer = rgb;
                return true;
            });

            yield return new ColorResultEntry(color, new ResultDisplaySettings(hsv, null, "Copy to clipboard", IconResource.FromResource("CommandPalette.Colors/Textures/Square")), 100, _ => {
                GUIUtility.systemCopyBuffer = hsv;
                return true;
            });

            yield return new ColorResultEntry(color, new ResultDisplaySettings(hsl, null, "Copy to clipboard", IconResource.FromResource("CommandPalette.Colors/Textures/Square")), 100, _ => {
                GUIUtility.systemCopyBuffer = hsl;
                return true;
            });
       }

        private static IEnumerable<ResultEntry> GenerateAlphaResults(Color color) {
            byte r = (byte)Math.Round(color.r * 255.0f);
            byte g = (byte)Math.Round(color.g * 255.0f);
            byte b = (byte)Math.Round(color.b * 255.0f);
            byte a = (byte)Math.Round(color.a * 255.0f);
            Color.RGBToHSV(color, out float hsvH, out float hsvS, out float hsvV);
            ColorHelper.RgbToHsl(color, out float hslH, out float hslS, out float hslL);

            string hex = $"#{r:X2}{g:X2}{b:X2}{a:X2}";
            string rgba = $"rgba({r}, {g}, {b}, {color.a})";
            string hsva = $"hsv({hsvH*360.0f}, {hsvS}, {hsvV}, {color.a})";
            string hsla = $"hsl({hslH*360.0f}, {hslS}, {hslL}, {color.a})";

            yield return new ColorResultEntry(color, new ResultDisplaySettings(hex, null, "Copy to clipboard", IconResource.FromResource("CommandPalette.Colors/Textures/Square")), 100, _ => {
                GUIUtility.systemCopyBuffer = hex;
                return true;
            });

            yield return new ColorResultEntry(color, new ResultDisplaySettings(rgba, null, "Copy to clipboard", IconResource.FromResource("CommandPalette.Colors/Textures/Square")), 100, _ => {
                GUIUtility.systemCopyBuffer = rgba;
                return true;
            });

            yield return new ColorResultEntry(color, new ResultDisplaySettings(hsva, null, "Copy to clipboard", IconResource.FromResource("CommandPalette.Colors/Textures/Square")), 100, _ => {
                GUIUtility.systemCopyBuffer = hsva;
                return true;
            });

            yield return new ColorResultEntry(color, new ResultDisplaySettings(hsla, null, "Copy to clipboard", IconResource.FromResource("CommandPalette.Colors/Textures/Square")), 100, _ => {
                GUIUtility.systemCopyBuffer = hsla;
                return true;
            });
        }
    }
}