Shader "Custom/TerrainShader"
{
    Properties
    {
        _RockColor ("Rock Color", Color) = (1,1,1,1)
        _GrassColor ("Grass Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        sampler2D _MainTex;
        sampler2D _CameraDepthTexture;

        struct Input
        {
            float2 uv_MainTex;
            float3 worldPos;
            fixed3 worldNormal;
            float4 screenPos;
        };

        half _Glossiness;
        half _Metallic;
        fixed4 _RockColor;
        fixed4 _GrassColor;
        int _IsPlayerUnderwater;

        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

        float InverseLerp (float a, float b, float v)
        {
            if (v > b)
                v = b;
            else if (v < a)
                v = a;
            return (v-a)/(b-a);
        }

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            // Albedo comes from a texture tinted by color
            // c = tex2D (_MainTex, IN.uv_MainTex) * _RockColor;
            fixed4 c;
            float lerpAmount = abs(IN.worldNormal.y);
            c = lerp(_RockColor, _GrassColor, lerpAmount);
            if (_IsPlayerUnderwater == 1)
                c *= half4 (0.75, 0.75, 1, 1);
            // Metallic and smoothness come from slider variables
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            c = half4(c.r, c.g, c.b, 1) * saturate(IN.worldPos.y / 128);
            c.a = 1;
            o.Albedo = c.rgb;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
