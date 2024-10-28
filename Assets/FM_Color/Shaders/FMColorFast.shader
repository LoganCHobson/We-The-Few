Shader "Hidden/FMColorFast"
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
