Shader "Unlit/ZWriteOnly"
{
    Properties
    {
        
    }
    SubShader
    {
        Tags { "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent" }
        LOD 100

        Pass
        {
           ZWrite On
           ColorMask 0
        }
    }
}
