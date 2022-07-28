Shader "Unlit/TerrainUnlitShader"
{
    Properties
    {
        _RockTexture ("Rock Texture", 2D) = "black"
        _BiomesTextureArray ("Biomes textures", 2DArray) = "" {}
        _TriplanarBlendSharpness ("Blend Sharpness",float) = 1
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float3 worldPos : TEXCOORD1;
                float4 screenPos : TEXCOORD2;
                float3 normal : TEXCOORD3;
                UNITY_FOG_COORDS(4)
                float4 vertex : SV_POSITION;
            };

            sampler2D _RockTexture;
            UNITY_DECLARE_TEX2DARRAY(_BiomesTextureArray);

            float _TriplanarBlendSharpness;
            int _IsPlayerUnderwater;

            float InverseLerp (float a, float b, float v)
            {
                if (v > b)
                    v = b;
                else if (v < a)
                    v = a;
                return (v-a)/(b-a);
            }

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.screenPos = ComputeScreenPos(o.vertex);
                o.uv = v.uv;
                // o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                o.normal = mul(v.normal, (float3x3)unity_WorldToObject);
                o.normal = normalize(o.normal);
                o.worldPos = mul (unity_ObjectToWorld, v.vertex).xyz;

                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 c;
                
                fixed4 rock1 = tex2D(_RockTexture, i.worldPos.xy/100);
                fixed4 rock2 = tex2D(_RockTexture, i.worldPos.yz/100);

                fixed4 biomeTex = UNITY_SAMPLE_TEX2DARRAY(_BiomesTextureArray, float3 (i.worldPos.xz/10, round(i.uv.x)));

                // c = fixed4 (i.worldPos/100, 1);
                // c = grass;
                // return c;

                half3 blendWeights = pow (abs(i.normal), _TriplanarBlendSharpness);

                float height = saturate(i.worldPos.y / 120);
                // blendWeights.y = saturate(blendWeights.y - 1 + depth);

                blendWeights = blendWeights / (blendWeights.x + blendWeights.y + blendWeights.z);

                c.rgb = biomeTex * blendWeights.y + rock1 * blendWeights.z + rock2 * blendWeights.x;
                // float lerpAmount = saturate(abs(i.normal.y) - 0.1f + (i.worldPos.y - 64) / 128);
                // c = lerp(_RockColor, _GrassColor, lerpAmount);

                if (_IsPlayerUnderwater == 1)
                    c *= half4 (0.75, 0.75, 1, 1);

                c *= saturate(i.worldPos.y / 128 + 0.2f);
                c.a = 1;

                UNITY_APPLY_FOG(i.fogCoord, c);
                return c;
            }
            ENDCG
        }
    }
}
