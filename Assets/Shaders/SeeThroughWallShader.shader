Shader "Custom/Dissolve"{
    Properties
    {
        _Color("Color", Color) = (1,1,1,1)
        _MainTex("Albedo (RGB)", 2D) = "white" {}
        _NoiseTex("Noise (Dissolving)", 2D) = "white" {}
        _Metallic("Metallic", 2D) = "black" {}
        _NormalMap("Normal", 2D) = "bump" {}
        _Occlusion("Occlusion", 2D) = "white" {}
        _Emission("Emission ", 2D) = "white" {}
        _EmissionIntensity("Emission Intensity", Range(0,10)) = 0.19
        _Glossiness("Smoothness", Range(0,1)) = 0.5
        _DissolvePercentage("DissolvePercentage", Range(0,3)) = 0.0
        _DistaceMultiplier("DistaceMultiplier", Float) = 1
        [Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend("Src Blend", Float) = 1
        [Enum(UnityEngine.Rendering.BlendMode)] _DstBlend("Dst Blend", Float) = 0
        _AlphaCutout("Alpha Cutout", Range(0,1)) = 0.0
    }
        SubShader
        {
            Tags { "RenderType" = "Transparent" "Queue" = "Transparent"}
            LOD 200
        CGPROGRAM
            // #pragma surface surf Tree fullforwardshadows addshadow   
            #pragma surface surf Standard fullforwardshadows addshadow
            #pragma target 3.0  

            fixed4 _Color;
            sampler2D _MainTex;
            sampler2D _NoiseTex;
            sampler2D _Metallic;
            sampler2D _NormalMap;
            sampler2D _ScreenGradient;
            sampler2D _Occlusion;
            sampler2D _Emission;
            half _Glossiness;

            half _DissolvePercentage;
            float _DistaceMultiplier;
            float _DistanceToCamera;
            float _AlphaCutout;

            struct Input
            {
                float2 uv_MainTex;
                float2 uv_NormalMap; INTERNAL_DATA
                float3 worldPos;
                float3 worldNormal;
                float4 screenPos;
            };

            void surf(Input IN, inout SurfaceOutputStandard o)
            {
                float pixelDepth = UNITY_Z_0_FAR_FROM_CLIPSPACE(IN.screenPos.z);

                float aspectRatio = _ScreenParams.x / _ScreenParams.y;
                float2 screenPos = IN.screenPos.xy / IN.screenPos.w;
                screenPos.x *= aspectRatio;

                half distanceToCenter = 1 - distance(screenPos, float2(0.5 * aspectRatio,0.5)) * _DistaceMultiplier;

                distanceToCenter = saturate(distanceToCenter);


                half grad01 = tex2D(_NoiseTex, IN.worldPos.xy).r;
                half grad02 = tex2D(_NoiseTex, IN.worldPos.xz).r;
                half grad03 = tex2D(_NoiseTex, IN.worldPos.yz).r;

                half gradient = grad01;
                gradient = lerp(gradient, grad02, IN.worldNormal.y);
                gradient = lerp(gradient, grad03, IN.worldNormal.x);

                float distanceMask = step(pixelDepth, _DistanceToCamera);

                clip(gradient - _DissolvePercentage * distanceMask * distanceToCenter);

                fixed4 c = tex2D(_MainTex, IN.uv_MainTex);
                o.Albedo = c.rgb * _Color;
                o.Alpha = 1;
                clip(c.a - _AlphaCutout);
                fixed4 m = tex2D(_Metallic, IN.uv_MainTex);
                o.Metallic = m.rgb;
                fixed4 g = tex2D(_Metallic, IN.uv_MainTex);
                o.Smoothness = g.rgb * _Glossiness;
                o.Occlusion = tex2D(_Occlusion, IN.uv_MainTex);
                fixed3 n = UnpackNormal(tex2D(_NormalMap, IN.uv_MainTex));
                o.Normal = n;
            }
            ENDCG
        }
            FallBack "Diffuse"
}