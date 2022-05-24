using System;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

namespace CommandPalette.Utils {
    public class Blur {
        private readonly Material m_BlurMaterial;

        public Blur() {
            m_BlurMaterial = new Material(Shader.Find("Hidden/Blur"));
        }

        public Texture BlurTexture(Texture sourceTexture, int downSample = 1, float blurSize = 4.0f, int passes = 8, Color tint = default(Color), float tinting = 0.4f) {
            m_BlurMaterial.SetColor("_Tint", tint);
            m_BlurMaterial.SetFloat("_Tinting", 0.0f);
            m_BlurMaterial.SetFloat("_BlurSize", blurSize);

            RenderTexture active = RenderTexture.active; // Save original RenderTexture so we can restore when we're done.

            RenderTexture destTexture = RenderTexture.GetTemporary(sourceTexture.width, sourceTexture.height, 0, sourceTexture.graphicsFormat);
            destTexture.filterMode = FilterMode.Bilinear;
            downSample = Mathf.Clamp(downSample, 0, 3);
            int downSampleWidth = sourceTexture.width >> downSample;
            int downSampleHeight = sourceTexture.height >> downSample;
            RenderTexture downSampleTexture = RenderTexture.GetTemporary(downSampleWidth, downSampleHeight, 0, sourceTexture.graphicsFormat);
            downSampleTexture.filterMode = FilterMode.Bilinear;
            Graphics.Blit(sourceTexture, downSampleTexture);

            m_BlurMaterial.SetFloat("_Tinting", tinting);
            try {
                RenderTexture tempB = RenderTexture.GetTemporary(downSampleTexture.width, downSampleTexture.height);

                for (int i = 0; i < passes; i++) {
                    if (i == 0) {
                        Graphics.Blit(downSampleTexture, tempB, m_BlurMaterial, 0);
                    } else {
                        Graphics.Blit(tempB, downSampleTexture, m_BlurMaterial, 0);
                    }

                    Graphics.Blit(downSampleTexture, tempB, m_BlurMaterial, 1);
                }

                Graphics.Blit(tempB, destTexture, m_BlurMaterial, 2);

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