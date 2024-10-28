#define IF(a, b, c) lerp(b, c, step((half) (a), 0));

struct appdata
{
    float4 vertex : POSITION;
    float2 uv : TEXCOORD0;

    UNITY_VERTEX_INPUT_INSTANCE_ID //for single pass vr
};

struct v2f
{
    float4 vertex : SV_POSITION;
    float2 uv : TEXCOORD0;

    UNITY_VERTEX_OUTPUT_STEREO //for single pass vr
};
            
UNITY_DECLARE_SCREENSPACE_TEXTURE(_MainTex); //for single pass vr

//sampler2D _MainTex;
float4 _MainTex_ST;
half4 _MainTex_TexelSize;

half _Scale;
half _Offset;

sampler3D _ClutTex;
half _EffectLut;

inline half GetLuma(half3 c) { return sqrt(dot(c, half3(0.299, 0.587, 0.114))); }

inline half3 ApplyHue(half3 c, half hue)
{
	half angle = radians(hue);
	half3 k = half3(0.57735, 0.57735, 0.57735);
	half cosAngle = cos(angle);      
	return c * cosAngle + cross(k, c) * sin(angle) + k * dot(k, c) * (1 - cosAngle);
}

half _Hue;
half _Saturation;
half _Brightness;
half _Contrast;
half _Sharpness;
inline half3 ApplyContrast(half3 c, half contrast) { return (((c - 0.5f) * contrast) + 0.5f); }
inline half3 ApplyBrightness(half3 c, half brightness) { return c * brightness; }
inline half3 ApplySaturation(half3 c, half saturation) { return lerp(dot(c, half3(0.299, 0.587, 0.114)), c, saturation); }

inline float2 EdgeUV(float2 uv, half OffX, half OffY) { return uv + float2(_MainTex_TexelSize.x * OffX, _MainTex_TexelSize.y * OffY); }

inline half3 ApplySharpness(half3 c, float2 uv)
{
    half3 sum_col = c;
    sum_col += UNITY_SAMPLE_SCREENSPACE_TEXTURE(_MainTex, EdgeUV(uv, -1,0)).rgb;
    sum_col += UNITY_SAMPLE_SCREENSPACE_TEXTURE(_MainTex, EdgeUV(uv, 1,0)).rgb;
    sum_col += UNITY_SAMPLE_SCREENSPACE_TEXTURE(_MainTex, EdgeUV(uv, 0,-1)).rgb;
    sum_col += UNITY_SAMPLE_SCREENSPACE_TEXTURE(_MainTex, EdgeUV(uv, 0,1)).rgb;
    return saturate((c * 2) - (sum_col/5));
}

half3 _ScanlineColor;
half _EffectScanline;
half _ScanlineX;
half _ScanlineY;
inline half3 ApplyScanline(half3 c, float2 uv)
{
    if(_ScanlineX == 0 && _ScanlineY == 0) return c;
    half3 ScanlineColor = _ScanlineColor;
    float2 texel = _MainTex_TexelSize.xy;
        
    ScanlineColor = IF(uv.x % (texel.x * _ScanlineX * 2) < texel.x * _ScanlineX, c, ScanlineColor);
    ScanlineColor = IF(uv.y % (texel.y * _ScanlineY * 2) < texel.y * _ScanlineY, c, ScanlineColor);
    return lerp(c, ScanlineColor, _EffectScanline);
}

inline half random (float2 p) { return frac(sin(dot(p.xy, float2(_Time.y % 10 * 0.1, 65.115))) * 2773.8856); }


half _EffectGrain;
half _GrainSize;
inline half3 ApplyGrain(half3 c, float2 uv)
{
    half Luminance = GetLuma(c);
    float2 normUV = uv; 
    normUV.x *= (_MainTex_TexelSize.z) / _GrainSize;
    normUV.y *= (_MainTex_TexelSize.w) / _GrainSize;

    float2 ipos = floor(normUV);  // get the integer coords
    half rand = random(ipos);
    half3 mv = half3(1,0,0);
    mv = IF(c.g > c.r && c.g > c.b, half3(0,1,0), mv);
    mv = IF(c.b > c.r && c.b > c.g, half3(0,0,1), mv);

    half3 Noise = ApplyHue(mv* c, rand * 360);
    return lerp(c, Noise * (0.05 + 0.2 * (0.5-abs(0.5-Luminance))) + c * 0.9, _EffectGrain);
}

half _EffectVignette;
half3 _VignetteColor;
inline half3 ApplyVignette(half3 c, float2 uv)
{
    float2 coord = (uv - 0.5) * 2;
    half rf = sqrt(dot(coord, coord)) * _EffectVignette;
    half rf2_1 = rf * rf + 1.0;
    half e = 1.0 / (rf2_1 * rf2_1);
    return lerp(c, _VignetteColor, (1-e));
}

half _PixelSize;
half _CelCuts;
half _HorizontalFlip;
half _VerticalFlip;

inline half3 ApplyPixelate(half3 c, float2 uv)
{
    float2 PS = (_PixelSize / _ScreenParams.xy);
    return IF(( ((uv.x+PS.x*0.5) % PS.x < PS.x *0.1) || ((uv.y+PS.y*0.5) % PS.y < PS.y * 0.1) ), c * GetLuma(c), c);
}

half _DebugSlider;
inline half3 ApplyDebug(half3 c, half x) { return IF(x > _DebugSlider, half3(0.9,0.9,0.9), c); }

half4 _TintColor;
/*
inline half3 ApplyEffects(half3 c, float2 uv)
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
*/


v2f vert (appdata v)
{
    v2f o;

    UNITY_SETUP_INSTANCE_ID(v); //for single pass vr
    UNITY_INITIALIZE_OUTPUT(v2f, o); //for single pass vr
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o); //for single pass vr
    
    o.vertex = UnityObjectToClipPos(v.vertex);
    o.uv = v.uv;

    o.uv.x = IF(_HorizontalFlip > 0, 1.0 - o.uv.x, o.uv.x);
    o.uv.y = IF(_VerticalFlip > 0, 1.0 - o.uv.y, o.uv.y);

    return o;
}
