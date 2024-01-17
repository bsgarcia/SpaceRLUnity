Shader "Custom/RandomDotsShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" { }
        _Proportion ("Proportion", Range(0, 1)) = 0.5
        _DefaultColor ("Default Color", Color) = (0, 0, 1, 1) // Default blue color
        _DotColor ("Dot Color", Color) = (1, 0, 0, 1)
        _DotSize ("Dot Size", Range(0, 1)) = 0.05
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        CGPROGRAM
        #pragma surface surf Lambert

        struct Input
        {
            float2 uv_MainTex;
        };

        uniform float _Proportion;
        uniform float4 _DefaultColor;
        uniform float4 _DotColor;
        uniform float _DotSize;

        void surf(Input IN, inout SurfaceOutput o)
        {
            float randValue = frac(sin(dot(IN.uv_MainTex, float2(12.9898, 78.233))) * 43758.5453);
            float distance = length(IN.uv_MainTex - 0.5);

            if (randValue > _Proportion && distance < _DotSize)
            {
                o.Albedo = _DotColor.rgb;
            }
            else
            {
                o.Albedo = _DefaultColor.rgb;
            }
        }
        ENDCG
    }
    FallBack "Diffuse"
}
