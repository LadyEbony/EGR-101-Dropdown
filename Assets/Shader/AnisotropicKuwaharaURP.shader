Shader "AnisotropicKuwaharaURP"
{
    Properties
    {
        _KernelSize ("Kernel Size (even; radius = /2)", Int) = 8
        _N          ("Sector Count (<=8)", Int) = 8
        _Hardness   ("Hardness", Float) = 1.0
        _Q          ("Q (shape)", Float) = 1.0
        _Alpha      ("Alpha", Float) = 1.0
        _ZeroCrossing ("Zero Crossing (radians)", Float) = 1.2
        _Zeta       ("Zeta", Float) = 0.5
        _TFM        ("TFM (internally set)", 2D) = "black" {}
    }

    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Opaque" }
        ZTest Always
        Cull Off
        ZWrite Off

        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

        // Blitter feeds this
        TEXTURE2D_X(_BlitTexture);
        SAMPLER(sampler_BlitTexture);

        // Orientation/Anisotropy texture (pass 2 output) used by final pass
        TEXTURE2D_X(_TFM);
        SAMPLER(sampler_TFM);

        float4 _MainTex_TexelSize; // set from C#
        int _KernelSize, _N;
        float _Hardness, _Q, _Alpha, _ZeroCrossing, _Zeta;

        struct Varyings
        {
            float4 positionCS : SV_POSITION;
            float2 uv : TEXCOORD0;
        };

        // Fullscreen triangle without a vertex buffer
        Varyings Vert(uint id : SV_VertexID) {
            Varyings o;
            o.positionCS = GetFullScreenTriangleVertexPosition(id);
            o.uv         = GetFullScreenTriangleTexCoord(id); // scaled & flipped for you
            return o;
        }

        float gaussian(float sigma, float pos)
        {
            float s2 = sigma * sigma;
            return (1.0 / sqrt(2.0 * 3.14159265358979323846 * s2)) * exp(-(pos * pos) / (2.0 * s2));
        }
        ENDHLSL

        // --- PASS 0: Eigen (structure tensor components) ---
        Pass
        {
            Name "Eigen"

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            float4 Frag(Varyings i) : SV_Target
            {
                float2 d = _MainTex_TexelSize.xy;

                float3 Sx =
                    (1.0 * SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_BlitTexture, i.uv + float2(-d.x, -d.y)).rgb +
                      2.0 * SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_BlitTexture, i.uv + float2(-d.x, 0.0)).rgb +
                      1.0 * SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_BlitTexture, i.uv + float2(-d.x, d.y)).rgb +
                     -1.0 * SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_BlitTexture, i.uv + float2(d.x, -d.y)).rgb +
                     -2.0 * SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_BlitTexture, i.uv + float2(d.x, 0.0)).rgb +
                     -1.0 * SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_BlitTexture, i.uv + float2(d.x, d.y)).rgb) / 4.0;

                float3 Sy =
                    (1.0 * SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_BlitTexture, i.uv + float2(-d.x, -d.y)).rgb +
                      2.0 * SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_BlitTexture, i.uv + float2(0.0, -d.y)).rgb +
                      1.0 * SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_BlitTexture, i.uv + float2(d.x, -d.y)).rgb +
                     -1.0 * SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_BlitTexture, i.uv + float2(-d.x, d.y)).rgb +
                     -2.0 * SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_BlitTexture, i.uv + float2(0.0, d.y)).rgb +
                     -1.0 * SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_BlitTexture, i.uv + float2(d.x, d.y)).rgb) / 4.0;

                // Return tensor components (xx, yy, xy, 1)
                return float4(dot(Sx, Sx), dot(Sy, Sy), dot(Sx, Sy), 1.0);
            }
            ENDHLSL
        }

        // --- PASS 1: Blur X ---
        Pass
        {
            Name"BlurX"

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            float4 Frag(Varyings i) : SV_Target
            {
                const int kernelRadius = 5;
                float4 col = 0;
                float kernelSum = 0;

                            [loop]
                for (int x = -kernelRadius; x <= kernelRadius; ++x)
                {
                    float4 c = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_BlitTexture, i.uv + float2(x, 0) * _MainTex_TexelSize.xy);
                    float g = gaussian(2.0, x);
                    col += c * g;
                    kernelSum += g;
                }
                return col / max(kernelSum, 1e-6);
            }
            ENDHLSL
        }

        // --- PASS 2: Blur Y + Orientation/Anisotropy (TFM) ---
        Pass
        {
            Name"BlurY_TFM"

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            float4 Frag(Varyings i) : SV_Target
            {
                const int kernelRadius = 5;
                float4 col = 0;
                float kernelSum = 0;

                            [loop]
                for (int y = -kernelRadius; y <= kernelRadius; ++y)
                {
                    float4 c = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_BlitTexture, i.uv + float2(0, y) * _MainTex_TexelSize.xy);
                    float g = gaussian(2.0, y);
                    col += c * g;
                    kernelSum += g;
                }

                float3 g = col.rgb / max(kernelSum, 1e-6);

                float disc = g.y * g.y - 2.0 * g.x * g.y + g.x * g.x + 4.0 * g.z * g.z;
                float root = sqrt(max(disc, 0.0));

                float lambda1 = 0.5 * (g.y + g.x + root);
                float lambda2 = 0.5 * (g.y + g.x - root);

                float2 v = float2(lambda1 - g.x, -g.z);
                float2 t = (length(v) > 0.0) ? normalize(v) : float2(0.0, 1.0);
                float phi = -atan2(t.y, t.x);

                float A = (lambda1 + lambda2 > 0.0) ? (lambda1 - lambda2) / (lambda1 + lambda2) : 0.0;

                            // Pack t.x, t.y, phi, A
                return float4(t, phi, A);
            }
            ENDHLSL
        }

        // --- PASS 3: Kuwahara (final) ---
        Pass
        {
            Name"Kuwahara"

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            float4 Frag(Varyings i) : SV_Target
            {
                float4 t = SAMPLE_TEXTURE2D_X(_TFM, sampler_TFM, i.uv);
                float alpha = _Alpha;

                int kernelRadius = _KernelSize / 2;
                float a = (float) kernelRadius * clamp((alpha + t.w) / alpha, 0.1, 2.0);
                float b = (float) kernelRadius * clamp(alpha / (alpha + t.w), 0.1, 2.0);

                float cos_phi = cos(t.z);
                float sin_phi = sin(t.z);

                float2x2 R = float2x2(cos_phi, -sin_phi,
                                                  sin_phi, cos_phi);
                float2x2 S = float2x2(0.5 / a, 0.0,
                                                  0.0, 0.5 / b);
                float2x2 SR = mul(S, R);

                int max_x = (int) sqrt(a * a * cos_phi * cos_phi + b * b * sin_phi * sin_phi);
                int max_y = (int) sqrt(a * a * sin_phi * sin_phi + b * b * cos_phi * cos_phi);

                float zeta = _Zeta;
                float zeroCross = _ZeroCrossing;
                float sinZeroCross = sin(zeroCross);
                float eta = (zeta + cos(zeroCross)) / max(sinZeroCross * sinZeroCross, 1e-6);

                float4 m[8];
                float3 s[8];
                            [unroll]
                for (int k = 0; k < 8; ++k)
                {
                    m[k] = 0.0;
                    s[k] = 0.0;
                }

                            [loop]
                for (int y = -max_y; y <= max_y; ++y)
                {
                                [loop]
                    for (int x = -max_x; x <= max_x; ++x)
                    {
                        float2 v = mul(SR, float2(x, y));
                        if (dot(v, v) <= 0.25)
                        {
                            float3 c = saturate(SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_BlitTexture, i.uv + float2(x, y) * _MainTex_TexelSize.xy).rgb);

                            float sum = 0;
                            float w[8];
                            float z, vxx, vyy;

                                        // Axis-aligned
                            vxx = zeta - eta * v.x * v.x;
                            vyy = zeta - eta * v.y * v.y;
                            z = max(0, v.y + vxx);
                            w[0] = z * z;
                            sum += w[0];
                            z = max(0, -v.x + vyy);
                            w[2] = z * z;
                            sum += w[2];
                            z = max(0, -v.y + vxx);
                            w[4] = z * z;
                            sum += w[4];
                            z = max(0, v.x + vyy);
                            w[6] = z * z;
                            sum += w[6];

                                        // 45-degree
                            float s2 = 0.70710678118; // sqrt(2)/2
                            float2 v2 = s2 * float2(v.x - v.y, v.x + v.y);
                            vxx = zeta - eta * v2.x * v2.x;
                            vyy = zeta - eta * v2.y * v2.y;
                            z = max(0, v2.y + vxx);
                            w[1] = z * z;
                            sum += w[1];
                            z = max(0, -v2.x + vyy);
                            w[3] = z * z;
                            sum += w[3];
                            z = max(0, -v2.y + vxx);
                            w[5] = z * z;
                            sum += w[5];
                            z = max(0, v2.x + vyy);
                            w[7] = z * z;
                            sum += w[7];

                            float g = exp(-3.125 * dot(v, v)) / max(sum, 1e-6);

                                        [unroll]
                            for (int k = 0; k < 8; ++k)
                            {
                                float wk = w[k] * g;
                                m[k] += float4(c * wk, wk);
                                s[k] += c * c * wk;
                            }
                        }
                    }
                }

                float4 outC = 0;
                            [loop]
                for (int k = 0; k < _N; ++k)
                {
                    float invW = 1.0 / max(m[k].w, 1e-6);
                    float3 mean = m[k].rgb * invW;
                    float3 var = abs(s[k] * invW - mean * mean);
                    float sigma2 = var.r + var.g + var.b;
                    float w = 1.0 / (1.0 + pow(_Hardness * 1000.0 * sigma2, 0.5 * _Q));
                    outC += float4(mean * w, w);
                }

                return saturate(outC / max(outC.w, 1e-6));
            }
            ENDHLSL
        }
    }
}
