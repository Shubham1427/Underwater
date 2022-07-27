Shader "Custom/OceanShader"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _NoiseTex1 ("First Noise Texture", 2D) = "black" {}
        _Noise1Scale ("First Noise Scale", Range(0, 0.1)) = 0.05
        _Noise1Speed ("First Noise Speed", Range(0, 0.1)) = 0.05
        _NoiseTex2 ("Second Noise Texture", 2D) = "black" {}
        _Noise2Scale ("Second Noise Scale", Range(0, 0.1)) = 0.05
        _Noise2Speed ("Second Noise Speed", Range(0, 0.1)) = 0.05
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
        _NormalPrecision ("Normal Recalculation Precision", Range(0,3)) = 1
        _WavesHeightMultiplier ("Waves Height Multiplier", Range(-2,2)) = 1
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent"}
        LOD 200
        Blend One OneMinusSrcAlpha
        ZWrite Off

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows vertex:vert alpha:fade

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        sampler2D _NoiseTex1;
        sampler2D _NoiseTex2;
        sampler2D _CameraDepthTexture;

        struct Input
        {
            float2 uv_MainTex;
            float4 screenPos;
        };

        half _Glossiness;
        half _Metallic;
        float _NormalPrecision;
        half _Noise1Scale;
        half _Noise1Speed;
        half _Noise2Scale;
        half _Noise2Speed;
        float _WavesHeightMultiplier;
        fixed4 _Color;

        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

        void vert (inout appdata_full v)
        {
            float4 v0 = v.vertex;
            float4 v1 = v0 + float4(_NormalPrecision, 0.0, 0.0, 0.0);
            float4 v2 = v0 + float4(0.0, 0.0, _NormalPrecision, 0.0);

            float2 v0Sample = mul(unity_ObjectToWorld, v0).xz;
            float2 v1Sample = mul(unity_ObjectToWorld, v1).xz;
            float2 v2Sample = mul(unity_ObjectToWorld, v2).xz;            

            v0.y += _WavesHeightMultiplier * tex2Dlod(_NoiseTex1, float4(v0Sample * _Noise1Scale + _Time.y * _Noise1Speed, 0.0, 0.0)) * tex2Dlod(_NoiseTex2, float4(v0Sample * _Noise2Scale + _Time.y * _Noise2Speed, 0.0, 0.0));
            v1.y += _WavesHeightMultiplier * tex2Dlod(_NoiseTex1, float4(v1Sample * _Noise1Scale + _Time.y * _Noise1Speed, 0.0, 0.0)) * tex2Dlod(_NoiseTex2, float4(v1Sample * _Noise2Scale + _Time.y * _Noise2Speed, 0.0, 0.0));
            v2.y += _WavesHeightMultiplier * tex2Dlod(_NoiseTex1, float4(v2Sample * _Noise1Scale + _Time.y * _Noise1Speed, 0.0, 0.0)) * tex2Dlod(_NoiseTex2, float4(v2Sample * _Noise2Scale + _Time.y * _Noise2Speed, 0.0, 0.0));
            
            float3 vn = cross(v2.xyz - v0.xyz, v1.xyz - v0.xyz);
            v.normal = normalize(vn);

            v.vertex = v0;
        }

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            // Albedo comes from a texture tinted by color
            fixed4 c = _Color;
            o.Albedo = c.rgb;
            // Metallic and smoothness come from slider variables
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            // o.Alpha = LinearEyeDepth(tex2Dproj(_CameraDepthTexture, UNITY_PROJ_COORD(IN.screenPos)).r);
            o.Alpha = 0.75;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
