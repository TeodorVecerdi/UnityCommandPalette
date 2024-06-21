using System;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

namespace CommandPalette.Utils {
    public static class Blur {
        private static readonly int s_BlurSizeShaderPropertyId = Shader.PropertyToID("_BlurSize");
        private static readonly int s_EnableTintShaderPropertyId = Shader.PropertyToID("_EnableTint");
        private static readonly int s_TintAmountShaderPropertyId = Shader.PropertyToID("_TintAmount");
        private static readonly int s_TintShaderPropertyId = Shader.PropertyToID("_Tint");
        private static readonly int s_EnableVibrancyShaderPropertyId = Shader.PropertyToID("_EnableVibrancy");
        private static readonly int s_VibrancyShaderPropertyId = Shader.PropertyToID("_Vibrancy");
        private static readonly int s_EnableNoiseShaderPropertyId = Shader.PropertyToID("_EnableNoise");
        private static readonly int s_NoiseTexShaderPropertyId = Shader.PropertyToID("_NoiseTex");

        private static Material? s_BlurMaterial;

        public static Texture2D BlurTexture(Texture sourceTexture, int downSamplePasses, int passes, float blurSize, bool enableTint, float tintAmount, Color tint, bool enableVibrancy, float vibrancy, bool enableNoise, Texture2D? noiseTexture) {
            PrepareMaterial(blurSize, enableTint, tintAmount, tint, enableVibrancy, vibrancy, enableNoise, noiseTexture);
            return BlurImpl(sourceTexture, downSamplePasses, passes);
        }

        private static void PrepareMaterial(float blurSize, bool enableTint, float tintAmount, Color tint, bool enableVibrancy, float vibrancy, bool enableNoise, Texture2D? noiseTexture) {
            if (s_BlurMaterial == null) {
                s_BlurMaterial = new Material(Shader.Find("Hidden/TeodorVecerdi/Blur-Default"));
            }

            s_BlurMaterial.SetFloat(s_BlurSizeShaderPropertyId, blurSize);
            s_BlurMaterial.SetFloat(s_EnableTintShaderPropertyId, enableTint ? 1.0f : 0.0f);
            s_BlurMaterial.SetFloat(s_TintAmountShaderPropertyId, tintAmount);
            s_BlurMaterial.SetColor(s_TintShaderPropertyId, tint);
            s_BlurMaterial.SetFloat(s_EnableVibrancyShaderPropertyId, enableVibrancy ? 1.0f : 0.0f);
            s_BlurMaterial.SetFloat(s_VibrancyShaderPropertyId, vibrancy);
            s_BlurMaterial.SetFloat(s_EnableNoiseShaderPropertyId, enableNoise ? 1.0f : 0.0f);
            s_BlurMaterial.SetTexture(s_NoiseTexShaderPropertyId, noiseTexture);
        }

        private static Texture2D BlurImpl(Texture sourceTexture, int downSamplePasses, int passes) {
            var currentWidth = sourceTexture.width;
            var currentHeight = sourceTexture.height;

            var nonDownSamplePasses = Math.Max(0, passes - downSamplePasses);

            var src = RenderTexture.GetTemporary(currentWidth, currentHeight);
            src.filterMode = FilterMode.Bilinear;

            Graphics.Blit(sourceTexture, src);

            // Downsample passes
            for (var i = 0; i < downSamplePasses; i++) {
                currentWidth >>= 1;
                currentHeight >>= 1;

                var tempA = RenderTexture.GetTemporary(currentWidth, currentHeight);
                var tempB = RenderTexture.GetTemporary(currentWidth, currentHeight);
                tempA.filterMode = FilterMode.Bilinear;
                tempB.filterMode = FilterMode.Bilinear;

                // Src -> A, Horizontal blur pass
                Graphics.Blit(src, tempA, s_BlurMaterial, 0);

                // A -> B, Vertical blur pass
                Graphics.Blit(tempA, tempB, s_BlurMaterial, 1);

                RenderTexture.ReleaseTemporary(tempA);
                RenderTexture.ReleaseTemporary(src);

                // Src = B
                src = tempB;
            }

            // Non-downsample passes
            for (var i = 0; i < nonDownSamplePasses; i++) {
                var tempA = RenderTexture.GetTemporary(currentWidth, currentHeight);
                var tempB = RenderTexture.GetTemporary(currentWidth, currentHeight);
                tempA.filterMode = FilterMode.Bilinear;
                tempB.filterMode = FilterMode.Bilinear;

                // Src -> A, Horizontal blur pass
                Graphics.Blit(src, tempA, s_BlurMaterial, 0);

                // A -> B, Vertical blur pass
                Graphics.Blit(tempA, tempB, s_BlurMaterial, 1);

                RenderTexture.ReleaseTemporary(tempA);
                RenderTexture.ReleaseTemporary(src);

                // Src = B
                src = tempB;
            }

            // Src -> Temp, Tint (pass 2), vibrancy
            var temp = RenderTexture.GetTemporary(currentWidth, currentHeight);
            Graphics.Blit(src, temp, s_BlurMaterial, 2);

            // Create target texture
            var targetTexture = new Texture2D(currentWidth, currentHeight, sourceTexture.graphicsFormat, TextureCreationFlags.None);
            var active = RenderTexture.active;
            RenderTexture.active = temp;
            targetTexture.ReadPixels(new Rect(0, 0, currentWidth, currentHeight), 0, 0);
            targetTexture.Apply(false, true);
            RenderTexture.active = active;

            RenderTexture.ReleaseTemporary(temp);
            RenderTexture.ReleaseTemporary(src);

            return targetTexture;
        }

        /*
        public static void BlurTexture(Texture sourceTexture, Texture targetTexture, int downSample = 1, float blurSize = 4.0f, int passes = 8, Color tint = default(Color), float tinting = 0.4f, float vibrancy = 0.0f) {
            if (s_BlurMaterial == null) {
                s_BlurMaterial = new Material(Shader.Find("Hidden/Command Palette/Blur"));
            }

            s_BlurMaterial.SetColor(s_TintShaderPropertyId, tint);
            s_BlurMaterial.SetFloat(s_BlurSizeShaderPropertyId, blurSize);
            s_BlurMaterial.SetFloat(s_TintingShaderPropertyId, tinting);
            s_BlurMaterial.SetFloat(s_VibrancyShaderPropertyId, vibrancy);

            RenderTexture active = RenderTexture.active; // Save original RenderTexture so we can restore when we're done.

            RenderTexture destTexture = RenderTexture.GetTemporary(sourceTexture.width, sourceTexture.height, 0, sourceTexture.graphicsFormat);
            destTexture.filterMode = FilterMode.Bilinear;
            downSample = Mathf.Clamp(downSample, 0, 3);
            int downSampleWidth = sourceTexture.width >> downSample;
            int downSampleHeight = sourceTexture.height >> downSample;
            RenderTexture downSampleTexture = RenderTexture.GetTemporary(downSampleWidth, downSampleHeight, 0, sourceTexture.graphicsFormat);
            downSampleTexture.filterMode = FilterMode.Bilinear;
            Graphics.Blit(sourceTexture, downSampleTexture);
            try {
                RenderTexture tempB = RenderTexture.GetTemporary(downSampleTexture.width, downSampleTexture.height);

                for (int i = 0; i < passes; i++) {
                    if (i == 0) {
                        Graphics.Blit(downSampleTexture, tempB, s_BlurMaterial, 0);
                    } else {
                        Graphics.Blit(tempB, downSampleTexture, s_BlurMaterial, 0);
                    }

                    Graphics.Blit(downSampleTexture, tempB, s_BlurMaterial, 1);
                }

                Graphics.Blit(tempB, destTexture, s_BlurMaterial, 2);

                RenderTexture.ReleaseTemporary(tempB);
                RenderTexture.ReleaseTemporary(downSampleTexture);
            } catch (Exception e) {
                Debug.LogException(e);
            } finally {
                RenderTexture.active = active; // Restore
            }

            Graphics.CopyTexture(destTexture, targetTexture);
            RenderTexture.ReleaseTemporary(destTexture);
        }
    */
    }
}
