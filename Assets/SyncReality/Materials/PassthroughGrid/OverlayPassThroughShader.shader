Shader "Unlit/OverlayPassThroughShader"
{
    Properties
    {
        _GridSize("GridSize", Float) = 80
        _LineStrength("LineStrength", Float) = 0.01

        _LineColor("LineColor", Color) = (1,0,0,1)
        _FillTint("PassthroughTint", Color) = (0,1,0,.1)
    }
        SubShader
    {
        Tags { "RenderType" = "Opaque" }
        LOD 100
        Cull Off

        Pass
        {
            // BlendOp RevSub
            // Blend Zero One, One One

             ZTest LEqual

             CGPROGRAM
             #pragma vertex vert
             #pragma fragment frag
        // make fog work
        #pragma multi_compile_fog

        #include "UnityCG.cginc"


        //Properties
        float _GridSize;
        float _LineStrength;

        float4 _FillTint;
        float4 _LineColor;

        struct appdata
        {
            float4 vertex : POSITION;
            half3 normal : NORMAL;
        };

        struct v2f
        {
            float3 worldPos : TEXCOORD0;
            half3 normal : TEXCOORD1;
            UNITY_FOG_COORDS(1)
            float4 vertex : SV_POSITION;
        };



        v2f vert(appdata v)
        {
            v2f o;
            o.worldPos = mul(unity_ObjectToWorld, v.vertex);
            o.vertex = UnityObjectToClipPos(v.vertex);
            o.normal = UnityObjectToWorldNormal(v.normal);
            UNITY_TRANSFER_FOG(o,o.vertex);
            return o;
        }

        float GridFactor(float axisCoord, float axisNormal)
        {
            float lineFac =step( 1 - _LineStrength, sin( axisCoord * _GridSize));
            float normalClip = 1 - step((0.706), (abs(axisNormal)));

            return lineFac * normalClip;
        }


        fixed4 frag(v2f i) : SV_Target
        {
            
            float grid = clamp(GridFactor(i.worldPos.x, i.normal.x) + GridFactor(i.worldPos.y, i.normal.y) + GridFactor(i.worldPos.z, i.normal.z), 0, 1);

            float4 col = lerp(_FillTint, _LineColor, grid);

            return col;
        }
        ENDCG
    }
    }
}
