using System;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

namespace CommandPalette.Utils {
    public class Blur {
        private readonly Material blurMaterial;

        public Blur() {
            blurMaterial = new Material(Shader.Find("Hidden/Blur"));
        }

        public Texture BlurTexture(Texture sourceTexture, int downSample = 1, float blurSize = 4.0f, int passes = 8, Color tint = default(Color), float tinting = 0.4f) {
            blurMaterial.SetColor("_Tint", tint);
            blurMaterial.SetFloat("_Tinting", 0.0f);
            blurMaterial.SetFloat("_BlurSize", blurSize);

            RenderTexture active = RenderTexture.active; // Save original RenderTexture so we can restore when we're done.

            RenderTexture destTexture = RenderTexture.GetTemporary(sourceTexture.width, sourceTexture.height, 0, sourceTexture.graphicsFormat);
            destTexture.filterMode = FilterMode.Bilinear;
            downSample = Mathf.Clamp(downSample, 0, 3);
            int downSampleWidth = sourceTexture.width >> downSample;
            int downSampleHeight = sourceTexture.height >> downSample;
            RenderTexture downSampleTexture = RenderTexture.GetTemporary(downSampleWidth, downSampleHeight, 0, sourceTexture.graphicsFormat);
            downSampleTexture.filterMode = FilterMode.Bilinear;
            Graphics.Blit(sourceTexture, downSampleTexture);

            blurMaterial.SetFloat("_Tinting", tinting);
            try {
                RenderTexture tempB = RenderTexture.GetTemporary(downSampleTexture.width, downSampleTexture.height);

                for (int i = 0; i < passes; i++) {
                    if (i == 0) {
                        Graphics.Blit(downSampleTexture, tempB, blurMaterial, 0);
                    } else {
                        Graphics.Blit(tempB, downSampleTexture, blurMaterial, 0);
                    }

                    Graphics.Blit(downSampleTexture, tempB, blurMaterial, 1);
                }

                Graphics.Blit(tempB, destTexture, blurMaterial, 2);

                RenderTexture.ReleaseTemporary(tempB);
                RenderTexture.ReleaseTemporary(downSampleTexture);
            } catch (Exception e) {
                Debug.LogException(e);
            } finally {
                RenderTexture.active = active; // Restore
            }

            Texture2D texture2D = new Texture2D(destTexture.width, destTexture.height, sourceTexture.graphicsFormat, TextureCreationFlags.None);
            Graphics.CopyTexture(destTexture, texture2D);
            RenderTexture.ReleaseTemporary(destTexture);

            return texture2D;
        }
    }
}