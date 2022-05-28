using System;
using UnityEngine;

namespace CommandPalette.Utils {
    public static class Blur {
        private static readonly int s_tintShaderPropertyId = Shader.PropertyToID("_Tint");
        private static readonly int s_tintingShaderPropertyId = Shader.PropertyToID("_Tinting");
        private static readonly int s_blurSizeShaderPropertyId = Shader.PropertyToID("_BlurSize");

        private static Material s_blurMaterial;

        public static void BlurTexture(Texture sourceTexture, Texture targetTexture, int downSample = 1, float blurSize = 4.0f, int passes = 8, Color tint = default(Color), float tinting = 0.4f) {
            if (s_blurMaterial == null) {
                s_blurMaterial = new Material(Shader.Find("Hidden/Blur"));
            }

            s_blurMaterial.SetColor(s_tintShaderPropertyId, tint);
            s_blurMaterial.SetFloat(s_tintingShaderPropertyId, 0.0f);
            s_blurMaterial.SetFloat(s_blurSizeShaderPropertyId, blurSize);

            RenderTexture active = RenderTexture.active; // Save original RenderTexture so we can restore when we're done.

            RenderTexture destTexture = RenderTexture.GetTemporary(sourceTexture.width, sourceTexture.height, 0, sourceTexture.graphicsFormat);
            destTexture.filterMode = FilterMode.Bilinear;
            downSample = Mathf.Clamp(downSample, 0, 3);
            int downSampleWidth = sourceTexture.width >> downSample;
            int downSampleHeight = sourceTexture.height >> downSample;
            RenderTexture downSampleTexture = RenderTexture.GetTemporary(downSampleWidth, downSampleHeight, 0, sourceTexture.graphicsFormat);
            downSampleTexture.filterMode = FilterMode.Bilinear;
            Graphics.Blit(sourceTexture, downSampleTexture);

            s_blurMaterial.SetFloat(s_tintingShaderPropertyId, tinting);
            try {
                RenderTexture tempB = RenderTexture.GetTemporary(downSampleTexture.width, downSampleTexture.height);

                for (int i = 0; i < passes; i++) {
                    if (i == 0) {
                        Graphics.Blit(downSampleTexture, tempB, s_blurMaterial, 0);
                    } else {
                        Graphics.Blit(tempB, downSampleTexture, s_blurMaterial, 0);
                    }

                    Graphics.Blit(downSampleTexture, tempB, s_blurMaterial, 1);
                }

                Graphics.Blit(tempB, destTexture, s_blurMaterial, 2);

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
    }
}