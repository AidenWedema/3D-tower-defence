Shader "Custom/HeightBasedColorShader"
{
    Properties
    {
        _Threshold1 ("Threshold 1", Float) = 0.25
        _Threshold2 ("Threshold 2", Float) = 0.5
        _Threshold3 ("Threshold 3", Float) = 0.75
        _Threshold4 ("Threshold 4", Float) = 1.0
        _Color1 ("Color 1", Color) = (0, 0, 1, 1)
        _Color2 ("Color 2", Color) = (0, 1, 0, 1)
        _Color3 ("Color 3", Color) = (1, 0, 0, 1)
        _Color4 ("Color 4", Color) = (1, 1, 0, 1)
        _Color5 ("Color 5", Color) = (1, 0, 1, 1)
        _GradientThreshold1 ("Gradient Threshold 1", Float) = 1
        _GradientThreshold2 ("Gradient Threshold 2", Float) = 1
        _GradientThreshold3 ("Gradient Threshold 3", Float) = 1
        _GradientThreshold4 ("Gradient Threshold 4", Float) = 1
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        #pragma surface surf Lambert

        sampler2D _MainTex;
        float _Threshold1;
        float _Threshold2;
        float _Threshold3;
        float _Threshold4;
        fixed4 _Color1;
        fixed4 _Color2;
        fixed4 _Color3;
        fixed4 _Color4;
        fixed4 _Color5;
        float _GradientThreshold1;
        float _GradientThreshold2;
        float _GradientThreshold3;
        float _GradientThreshold4;

        struct Input
        {
            float2 uv_MainTex;
            float3 worldPos;
        };

        void surf (Input IN, inout SurfaceOutput o)
        {
            fixed4 c = tex2D(_MainTex, IN.uv_MainTex);
            float height = IN.worldPos.y;
            float t;
            
            if (height < _Threshold1)
            {
                o.Albedo = _Color1;
            }
            else if (height < _Threshold2)
            {
                o.Albedo = _Color2;
            }
            else if (height < _Threshold3)
            {
                o.Albedo = _Color3;
            }
            else if (height < _Threshold4)
            {
                o.Albedo = _Color4;
            }
            else
            {
                o.Albedo = _Color5;
            }

            if (Around(height, _Threshold1, _GradientThreshold1))
            {
                t = height - _GradientThreshold1;
                o.Albedo = lerp(_Color1, _Color2, t).rgb;
            }
            if (Around(height, _Threshold2, _GradientThreshold2))
            {
                t = height - _GradientThreshold2;
                o.Albedo = lerp(_Color2, _Color3, t).rgb;
            }
            if (Around(height, _Threshold3, _GradientThreshold3))
            {
                t = height - _GradientThreshold3;
                o.Albedo = lerp(_Color3, _Color4, t).rgb;
            }
            if (Around(height, _Threshold4, _GradientThreshold4))
            {
                t = height - _GradientThreshold4;
                o.Albedo = lerp(_Color4, _Color5, t).rgb;
            }
            o.Alpha = c.a;
        }

        bool Around(float value, float threshold, float maxDifferance)
        {
            maxValue = threshold + maxDifferance;
            minValue = threshold - maxDifferance;
            return (value < maxDifferance && value >minValue);
        }

        ENDCG
    }
    FallBack "Diffuse"
}
