// MIT License
//
// Copyright (c) 2024 Teodor Vecerdi
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

Shader "Hidden/Command Palette/Blur"
{
    Properties
    {
        _MainTex ("", 2D) = "white" {}
        _BlurSize ("", Range(0.0, 1.0)) = 1.0
        _Tint ("", Color) = (0.0, 0.0, 0.0, 0.0)
        _Tinting ("", Range(0.0, 1.0)) = 0.64
        _Vibrancy ("", Range(0.0, 1.0)) = 0.0
    }

    SubShader
    {
        Pass
        {
            // Pass 0 - Horizontal
            Name "Horizontal"
            ZTest Always Cull Off ZWrite Off
            Fog
            {
                Mode off
            }
            Blend Off

            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag
            #pragma fragmentoption ARB_precision_hint_fastest

            #include "UnityCG.cginc"

            uniform sampler2D _MainTex;
            uniform half4 _MainTex_TexelSize;
            uniform half _BlurSize;

            half4 frag(v2f_img i) : COLOR {
                half4 color = 0.16 * tex2D(_MainTex, i.uv);
                color += 0.15 * tex2D(_MainTex, i.uv + _MainTex_TexelSize.xy * half2(1.0 * _BlurSize, 0.0));
                color += 0.15 * tex2D(_MainTex, i.uv - _MainTex_TexelSize.xy * half2(1.0 * _BlurSize, 0.0));
                color += 0.12 * tex2D(_MainTex, i.uv + _MainTex_TexelSize.xy * half2(2.0 * _BlurSize, 0.0));
                color += 0.12 * tex2D(_MainTex, i.uv - _MainTex_TexelSize.xy * half2(2.0 * _BlurSize, 0.0));
                color += 0.09 * tex2D(_MainTex, i.uv + _MainTex_TexelSize.xy * half2(3.0 * _BlurSize, 0.0));
                color += 0.09 * tex2D(_MainTex, i.uv - _MainTex_TexelSize.xy * half2(3.0 * _BlurSize, 0.0));
                color += 0.06 * tex2D(_MainTex, i.uv + _MainTex_TexelSize.xy * half2(4.0 * _BlurSize, 0.0));
                color += 0.06 * tex2D(_MainTex, i.uv - _MainTex_TexelSize.xy * half2(4.0 * _BlurSize, 0.0));
                return color;
            }
            ENDCG
        }

        Pass
        {
            // Pass 1 - Vertical
            Name "Vertical"
            ZTest Always Cull Off ZWrite Off
            Fog
            {
                Mode off
            }
            Blend Off

            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag
            #pragma fragmentoption ARB_precision_hint_fastest

            #include "UnityCG.cginc"

            uniform sampler2D _MainTex;
            uniform half4 _MainTex_TexelSize;
            uniform half _BlurSize;

            half4 frag(v2f_img i) : COLOR {
                half4 color = 0.16 * tex2D(_MainTex, i.uv);
                color += 0.15 * tex2D(_MainTex, i.uv + _MainTex_TexelSize.xy * half2(0.0, 1.0 * _BlurSize));
                color += 0.15 * tex2D(_MainTex, i.uv - _MainTex_TexelSize.xy * half2(0.0, 1.0 * _BlurSize));
                color += 0.12 * tex2D(_MainTex, i.uv + _MainTex_TexelSize.xy * half2(0.0, 2.0 * _BlurSize));
                color += 0.12 * tex2D(_MainTex, i.uv - _MainTex_TexelSize.xy * half2(0.0, 2.0 * _BlurSize));
                color += 0.09 * tex2D(_MainTex, i.uv + _MainTex_TexelSize.xy * half2(0.0, 3.0 * _BlurSize));
                color += 0.09 * tex2D(_MainTex, i.uv - _MainTex_TexelSize.xy * half2(0.0, 3.0 * _BlurSize));
                color += 0.06 * tex2D(_MainTex, i.uv + _MainTex_TexelSize.xy * half2(0.0, 4.0 * _BlurSize));
                color += 0.06 * tex2D(_MainTex, i.uv - _MainTex_TexelSize.xy * half2(0.0, 4.0 * _BlurSize));
                return color;
            }
            ENDCG
        }

        Pass
        {
            // Pass 2 - Tint
            Name "Tint"
            ZTest Always Cull Off ZWrite Off
            Fog
            {
                Mode off
            }
            Blend Off

            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag
            #pragma fragmentoption ARB_precision_hint_fastest

            #include "UnityCG.cginc"

            uniform sampler2D _MainTex;
            uniform half4 _Tint;
            uniform half _Tinting;
            uniform half _Vibrancy;

            half4 frag(v2f_img i) : COLOR {
                half4 color = tex2D(_MainTex, i.uv);

                float grey = dot(color.rgb, half3(0.299, 0.587, 0.114));
                color.rgb = lerp(half3(grey, grey, grey), color.rgb, _Vibrancy + 1.0);

                return lerp(color, _Tint, _Tinting);
            }
            ENDCG
        }
    }

    Fallback off
}
