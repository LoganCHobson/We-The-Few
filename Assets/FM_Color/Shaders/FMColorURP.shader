Shader "Hidden/FMColorURP"
{
    Properties
    {
        _MainTex ("Texture", Any) = "" {}
    }
    SubShader
    {
        ZTest Always Cull Off ZWrite Off
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            #include "FMColorShared.cginc"

            half3 ApplyEffects(half3 c, float2 uv)
            {
                //Cel Shading
                if(GetLuma(c) > 0) c = IF(_CelCuts < 255, normalize(c) * ceil(GetLuma(c) * _CelCuts) * (1/_CelCuts), c);
                c = IF(_Hue > 0, ApplyHue(c, _Hue), c);
                c = IF(_Contrast != 1, ApplyContrast(c, _Contrast), c);
                c = IF(_Brightness != 1, ApplyBrightness(c, _Brightness), c);

                //Lut
                c = IF(_EffectLut > 0, lerp(c, tex3D(_ClutTex, c * _Scale + _Offset).rgb, _EffectLut), c);
                c *= _TintColor;

                c = IF(_EffectScanline > 0, ApplyScanline(c, uv), c);
        
                c = IF(_EffectGrain > 0, ApplyGrain(c, uv), c);
                c = IF(_EffectVignette > 0, ApplyVignette(c, uv), c);
                c = IF(_Saturation != 1, ApplySaturation(c, _Saturation), c);

                return c;
            }
            fixed4 frag(v2f i) : SV_Target
	        {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i); //for single pass vr

                float2 uv = IF(_PixelSize > 1, round(i.uv/(_PixelSize / _ScreenParams.xy)) * _PixelSize / _ScreenParams.xy, i.uv);
                half4 c = UNITY_SAMPLE_SCREENSPACE_TEXTURE(_MainTex, uv);
                c.rgb = IF(_Sharpness > 0, lerp(c.rgb, ApplySharpness(c.rgb, uv), _Sharpness), c.rgb);
                c.rgb = ApplyEffects(c.rgb, uv);
                c.rgb = IF(_PixelSize >= 6, ApplyPixelate(c.rgb, i.uv), c.rgb);
                c.rgb = IF(i.uv.x < _DebugSlider, ApplyDebug(UNITY_SAMPLE_SCREENSPACE_TEXTURE(_MainTex, i.uv).rgb, i.uv.x), c);

                return c;
	        }


            ENDCG
        }
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment fragLinear

            #include "UnityCG.cginc"
            #include "FMColorShared.cginc"

            half3 ApplyEffects(half3 c, float2 uv)
            {
                //Cel Shading
                if(GetLuma(c) > 0) c = IF(_CelCuts < 255, normalize(c) * ceil(GetLuma(c) * _CelCuts) * (1/_CelCuts), c);
                c = IF(_Hue > 0, ApplyHue(c, _Hue), c);
                c = IF(_Contrast != 1, ApplyContrast(c, _Contrast), c);
                c = IF(_Brightness != 1, ApplyBrightness(c, _Brightness), c);

                //Lut
                c = IF(_EffectLut > 0, lerp(c, tex3D(_ClutTex, c * _Scale + _Offset).rgb, _EffectLut), c);
                c *= _TintColor;

                c = IF(_EffectScanline > 0, ApplyScanline(c, uv), c);
        
                c = IF(_EffectGrain > 0, ApplyGrain(c, uv), c);
                c = IF(_EffectVignette > 0, ApplyVignette(c, uv), c);
                c = IF(_Saturation != 1, ApplySaturation(c, _Saturation), c);
                return c;
            }
            fixed4 fragLinear(v2f i) : SV_Target
	        {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i); //for single pass vr

                float2 uv = IF(_PixelSize > 1, round(i.uv/(_PixelSize / _ScreenParams.xy)) * _PixelSize / _ScreenParams.xy, i.uv);
                half4 c = UNITY_SAMPLE_SCREENSPACE_TEXTURE(_MainTex, uv);
                c.rgb = IF(_Sharpness > 0, lerp(c.rgb, ApplySharpness(c.rgb, uv), _Sharpness), c.rgb);

                c.rgb = sqrt(c.rgb);
                c.rgb = ApplyEffects(c.rgb, uv);
                c.rgb *= c.rgb;

                c.rgb = IF(_PixelSize >= 6, ApplyPixelate(c.rgb, i.uv), c.rgb);
                c.rgb = IF(i.uv.x < _DebugSlider, ApplyDebug(UNITY_SAMPLE_SCREENSPACE_TEXTURE(_MainTex, i.uv).rgb, i.uv.x), c);

                return c;
	        }
            ENDCG
        }
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment fragPaint

            #include "UnityCG.cginc"
            #include "FMColorShared.cginc"

            
            uniform half _PaintRadius;
            uniform half _PaintBlurLevel;
            inline half3 ApplyPaintEffect(float2 uv)
            {
                float3 mean[4];
                float3 sigma[4];
                mean[0]=mean[1]=mean[2]=mean[3]=sigma[0]=sigma[1]=sigma[2]=sigma[3]=float3(0,0,0);      
 
                float2 start[4] = {{-_PaintRadius, -_PaintRadius}, {-_PaintRadius, 0}, {0, -_PaintRadius}, {0, 0}};
                float2 pos;
                float3 col;

                for(int i = 0; i <= _PaintRadius; i++) 
                {
                    for(int j = 0; j <= _PaintRadius; j++) 
                    {
                        pos = float2(i, j) + start[0];
                        col = UNITY_SAMPLE_SCREENSPACE_TEXTURE(_MainTex, float4(uv + float2(pos.x * _MainTex_TexelSize.x, pos.y * _MainTex_TexelSize.y), 0, 0)).rgb;
                        mean[0] += col;
                        sigma[0] += col * col;

                        pos = float2(i, j) + start[1];
                        col = UNITY_SAMPLE_SCREENSPACE_TEXTURE(_MainTex, float4(uv + float2(pos.x * _MainTex_TexelSize.x, pos.y * _MainTex_TexelSize.y), 0, 0)).rgb;
                        mean[1] += col;
                        sigma[1] += col * col;

                        pos = float2(i, j) + start[2];
                        col = UNITY_SAMPLE_SCREENSPACE_TEXTURE(_MainTex, float4(uv + float2(pos.x * _MainTex_TexelSize.x, pos.y * _MainTex_TexelSize.y), 0, 0)).rgb;
                        mean[2] += col;
                        sigma[2] += col * col;

                        pos = float2(i, j) + start[3];
                        col = UNITY_SAMPLE_SCREENSPACE_TEXTURE(_MainTex, float4(uv + float2(pos.x * _MainTex_TexelSize.x, pos.y * _MainTex_TexelSize.y), 0, 0)).rgb;
                        mean[3] += col;
                        sigma[3] += col * col;
                    }
                }

                /*
                for (int k = 0; k < 4; k++) 
                {
                    for(int i = 0; i <= _PaintRadius; i++) 
                    {
                        for(int j = 0; j <= _PaintRadius; j++) 
                        {
                            pos = float2(i, j) + start[k];
                            col = UNITY_SAMPLE_SCREENSPACE_TEXTURE(_MainTex, float4(uv + float2(pos.x * _MainTex_TexelSize.x, pos.y * _MainTex_TexelSize.y), 0, 0)).rgb;
                            mean[k] += col;
                            sigma[k] += col * col;
                        }
                    }
                }
                */

                float sigma2;
                float n = pow(_PaintRadius + 1, 2);
                half3 c = float3(1,1,1);
                float min = 1;
 
                for (int l = 0; l < 4; l++) 
                {
                    mean[l] /= n;
                    sigma[l] = abs(sigma[l] / n - mean[l] * mean[l]);
                    sigma2 = sigma[l].r + sigma[l].g + sigma[l].b;
                    c = IF(sigma2 < min, mean[l], c);
                    min = IF(sigma2 < min, sigma2, min);
                }
                return c;
            }   

            half3 ApplyEffects(half3 c, float2 uv)
            {
                //Cel Shading
                if(GetLuma(c) > 0) c = IF(_CelCuts < 255, normalize(c) * ceil(GetLuma(c) * _CelCuts) * (1/_CelCuts), c);
                c = IF(_Hue > 0, ApplyHue(c, _Hue), c);
                c = IF(_Contrast != 1, ApplyContrast(c, _Contrast), c);
                c = IF(_Brightness != 1, ApplyBrightness(c, _Brightness), c);

                //Lut
                c = IF(_EffectLut > 0, lerp(c, tex3D(_ClutTex, c * _Scale + _Offset).rgb, _EffectLut), c);
                c *= _TintColor;

                c = IF(_EffectScanline > 0, ApplyScanline(c, uv), c);
        
                c = IF(_EffectGrain > 0, ApplyGrain(c, uv), c);
                c = IF(_EffectVignette > 0, ApplyVignette(c, uv), c);
                c = IF(_Saturation != 1, ApplySaturation(c, _Saturation), c);
                return c;
            }
            fixed4 fragPaint(v2f i) : SV_Target
	        {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i); //for single pass vr

                float2 uv = IF(_PixelSize > 1, round(i.uv/(_PixelSize / _ScreenParams.xy)) * _PixelSize / _ScreenParams.xy, i.uv);
                half4 c = half4(ApplyPaintEffect(uv),1);
                c.rgb = IF(_Sharpness > 0, lerp(c.rgb, ApplySharpness(c.rgb, uv), _Sharpness), c.rgb);
                c.rgb = ApplyEffects(c.rgb, uv);
                c.rgb = IF(_PixelSize >= 6, ApplyPixelate(c.rgb, i.uv), c.rgb);
                c.rgb = IF(i.uv.x < _DebugSlider, ApplyDebug(UNITY_SAMPLE_SCREENSPACE_TEXTURE(_MainTex, i.uv).rgb, i.uv.x), c);

                return c;
	        }


            ENDCG
        }
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment fragLinearPaint

            #include "UnityCG.cginc"
            #include "FMColorShared.cginc"

            uniform half _PaintRadius;
            uniform half _PaintBlurLevel;
            inline half3 ApplyPaintEffect(float2 uv)
            {
                float3 mean[4];
                float3 sigma[4];
                mean[0]=mean[1]=mean[2]=mean[3]=sigma[0]=sigma[1]=sigma[2]=sigma[3]=float3(0,0,0);      
 
                float2 start[4] = {{-_PaintRadius, -_PaintRadius}, {-_PaintRadius, 0}, {0, -_PaintRadius}, {0, 0}};
                float2 pos;
                float3 col;

                for(int i = 0; i <= _PaintRadius; i++) 
                {
                    for(int j = 0; j <= _PaintRadius; j++) 
                    {
                        pos = float2(i, j) + start[0];
                        col = UNITY_SAMPLE_SCREENSPACE_TEXTURE(_MainTex, float4(uv + float2(pos.x * _MainTex_TexelSize.x, pos.y * _MainTex_TexelSize.y), 0, 0)).rgb;
                        mean[0] += col;
                        sigma[0] += col * col;

                        pos = float2(i, j) + start[1];
                        col = UNITY_SAMPLE_SCREENSPACE_TEXTURE(_MainTex, float4(uv + float2(pos.x * _MainTex_TexelSize.x, pos.y * _MainTex_TexelSize.y), 0, 0)).rgb;
                        mean[1] += col;
                        sigma[1] += col * col;

                        pos = float2(i, j) + start[2];
                        col = UNITY_SAMPLE_SCREENSPACE_TEXTURE(_MainTex, float4(uv + float2(pos.x * _MainTex_TexelSize.x, pos.y * _MainTex_TexelSize.y), 0, 0)).rgb;
                        mean[2] += col;
                        sigma[2] += col * col;

                        pos = float2(i, j) + start[3];
                        col = UNITY_SAMPLE_SCREENSPACE_TEXTURE(_MainTex, float4(uv + float2(pos.x * _MainTex_TexelSize.x, pos.y * _MainTex_TexelSize.y), 0, 0)).rgb;
                        mean[3] += col;
                        sigma[3] += col * col;
                    }
                }

                /*
                for (int k = 0; k < 4; k++) 
                {
                    for(int i = 0; i <= _PaintRadius; i++) 
                    {
                        for(int j = 0; j <= _PaintRadius; j++) 
                        {
                            pos = float2(i, j) + start[k];
                            col = UNITY_SAMPLE_SCREENSPACE_TEXTURE(_MainTex, float4(uv + float2(pos.x * _MainTex_TexelSize.x, pos.y * _MainTex_TexelSize.y), 0, 0)).rgb;
                            mean[k] += col;
                            sigma[k] += col * col;
                        }
                    }
                }
                */
 
                float sigma2;
                float n = pow(_PaintRadius + 1, 2);
                half3 c = float3(1,1,1);
                float min = 1;
 
                for (int l = 0; l < 4; l++) 
                {
                    mean[l] /= n;
                    sigma[l] = abs(sigma[l] / n - mean[l] * mean[l]);
                    sigma2 = sigma[l].r + sigma[l].g + sigma[l].b;
                    c = IF(sigma2 < min, mean[l], c);
                    min = IF(sigma2 < min, sigma2, min);
                }
                return c;
            }

            half3 ApplyEffects(half3 c, float2 uv)
            {
                //Cel Shading
                if(GetLuma(c) > 0) c = IF(_CelCuts < 255, normalize(c) * ceil(GetLuma(c) * _CelCuts) * (1/_CelCuts), c);
                c = IF(_Hue > 0, ApplyHue(c, _Hue), c);
                c = IF(_Contrast != 1, ApplyContrast(c, _Contrast), c);
                c = IF(_Brightness != 1, ApplyBrightness(c, _Brightness), c);

                //Lut
                c = IF(_EffectLut > 0, lerp(c, tex3D(_ClutTex, c * _Scale + _Offset).rgb, _EffectLut), c);
                c *= _TintColor;

                c = IF(_EffectScanline > 0, ApplyScanline(c, uv), c);
        
                c = IF(_EffectGrain > 0, ApplyGrain(c, uv), c);
                c = IF(_EffectVignette > 0, ApplyVignette(c, uv), c);
                c = IF(_Saturation != 1, ApplySaturation(c, _Saturation), c);
                return c;
            }
            fixed4 fragLinearPaint(v2f i) : SV_Target
	        {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i); //for single pass vr

                float2 uv = IF(_PixelSize > 1, round(i.uv/(_PixelSize / _ScreenParams.xy)) * _PixelSize / _ScreenParams.xy, i.uv);
                half4 c = half4(ApplyPaintEffect(uv),1);
                c.rgb = IF(_Sharpness > 0, lerp(c.rgb, ApplySharpness(c.rgb, uv), _Sharpness), c.rgb);

                c.rgb = sqrt(c.rgb);
                c.rgb = ApplyEffects(c.rgb, uv);
                c.rgb *= c.rgb;

                c.rgb = IF(_PixelSize >= 6, ApplyPixelate(c.rgb, i.uv), c.rgb);
                c.rgb = IF(i.uv.x < _DebugSlider, ApplyDebug(UNITY_SAMPLE_SCREENSPACE_TEXTURE(_MainTex, i.uv).rgb, i.uv.x), c);

                return c;
	        }
            ENDCG
        }
    }
}
