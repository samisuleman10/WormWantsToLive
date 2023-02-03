Shader "URP_GroundProjectionEdgeDitheringZWrite"
{
    Properties
    {
        [NoScaleOffset] Texture2D_818cca92a18946a3b27ffbaf1b40a062("BaseColor", 2D) = "white" {}
        [NoScaleOffset]Texture2D_3f2131083fc44d9b989206ab02babd86("AO_Gloss_Metallic", 2D) = "white" {}
        [NoScaleOffset]Texture2D_b01d9e81c8e04473b8880e3878694e5e("Normal", 2D) = "white" {}
        Vector1_accfafba726746c885b8e27280080a86("NormalMapStrength", Float) = 1
        Vector1_c5686f27abfe4d5081a5b5eee32afda8("TilingScale", Float) = 1
        Vector1_9ec3ae0c6d7a4e1f8b784c14876e5174("UVRotation", Float) = 0
        Color_dc69a74bd5704ea796654354ea94b8d9("Color", Color) = (1, 1, 1, 0)
        Vector1_4f1d2b1f25a248f3923ab327466f2485("EdbgeBlend", Range(0, 1)) = 0.2
        [HideInInspector][NoScaleOffset]unity_Lightmaps("unity_Lightmaps", 2DArray) = "" {}
        [HideInInspector][NoScaleOffset]unity_LightmapsInd("unity_LightmapsInd", 2DArray) = "" {}
        [HideInInspector][NoScaleOffset]unity_ShadowMasks("unity_ShadowMasks", 2DArray) = "" {}
    }
        SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
            "RenderType" = "Transparent"
            "UniversalMaterialType" = "Lit"
            "Queue" = "Transparent"
        }
        Pass
        {
            Name "Universal Forward"
            Tags
            {
                "LightMode" = "UniversalForward"
            }

        // Render State
        Cull Back
    Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha
    ZTest LEqual
    ZWrite On

        // Debug
        // <None>

        // --------------------------------------------------
        // Pass

        HLSLPROGRAM

        // Pragmas
        #pragma target 4.5
    #pragma exclude_renderers gles gles3 glcore
    #pragma multi_compile_instancing
    #pragma multi_compile_fog
    #pragma multi_compile _ DOTS_INSTANCING_ON
    #pragma vertex vert
    #pragma fragment frag

        // DotsInstancingOptions: <None>
        // HybridV1InjectedBuiltinProperties: <None>

        // Keywords
        #pragma multi_compile _ _SCREEN_SPACE_OCCLUSION
    #pragma multi_compile _ LIGHTMAP_ON
    #pragma multi_compile _ DIRLIGHTMAP_COMBINED
    #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
    #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
    #pragma multi_compile _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS _ADDITIONAL_OFF
    #pragma multi_compile _ _ADDITIONAL_LIGHT_SHADOWS
    #pragma multi_compile _ _SHADOWS_SOFT
    #pragma multi_compile _ LIGHTMAP_SHADOW_MIXING
    #pragma multi_compile _ SHADOWS_SHADOWMASK
        // GraphKeywords: <None>

        // Defines
        #define _SURFACE_TYPE_TRANSPARENT 1
        #define _AlphaClip 1
        #define _NORMALMAP 1
        #define _NORMAL_DROPOFF_TS 1
        #define ATTRIBUTES_NEED_NORMAL
        #define ATTRIBUTES_NEED_TANGENT
        #define ATTRIBUTES_NEED_TEXCOORD1
        #define VARYINGS_NEED_POSITION_WS
        #define VARYINGS_NEED_NORMAL_WS
        #define VARYINGS_NEED_TANGENT_WS
        #define VARYINGS_NEED_VIEWDIRECTION_WS
        #define VARYINGS_NEED_FOG_AND_VERTEX_LIGHT
        #define FEATURES_GRAPH_VERTEX
        /* WARNING: $splice Could not find named fragment 'PassInstancing' */
        #define SHADERPASS SHADERPASS_FORWARD
    #define REQUIRE_DEPTH_TEXTURE
        /* WARNING: $splice Could not find named fragment 'DotsInstancingVars' */

        // Includes
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
    #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
    #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"

        // --------------------------------------------------
        // Structs and Packing

        struct Attributes
    {
        float3 positionOS : POSITION;
        float3 normalOS : NORMAL;
        float4 tangentOS : TANGENT;
        float4 uv1 : TEXCOORD1;
        #if UNITY_ANY_INSTANCING_ENABLED
        uint instanceID : INSTANCEID_SEMANTIC;
        #endif
    };
    struct Varyings
    {
        float4 positionCS : SV_POSITION;
        float3 positionWS;
        float3 normalWS;
        float4 tangentWS;
        float3 viewDirectionWS;
        #if defined(LIGHTMAP_ON)
        float2 lightmapUV;
        #endif
        #if !defined(LIGHTMAP_ON)
        float3 sh;
        #endif
        float4 fogFactorAndVertexLight;
        float4 shadowCoord;
        #if UNITY_ANY_INSTANCING_ENABLED
        uint instanceID : CUSTOM_INSTANCE_ID;
        #endif
        #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
        uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
        #endif
        #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
        uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
        #endif
        #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
        FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
        #endif
    };
    struct SurfaceDescriptionInputs
    {
        float3 TangentSpaceNormal;
        float3 WorldSpacePosition;
        float3 AbsoluteWorldSpacePosition;
        float4 ScreenPosition;
    };
    struct VertexDescriptionInputs
    {
        float3 ObjectSpaceNormal;
        float3 ObjectSpaceTangent;
        float3 ObjectSpacePosition;
    };
    struct PackedVaryings
    {
        float4 positionCS : SV_POSITION;
        float3 interp0 : TEXCOORD0;
        float3 interp1 : TEXCOORD1;
        float4 interp2 : TEXCOORD2;
        float3 interp3 : TEXCOORD3;
        #if defined(LIGHTMAP_ON)
        float2 interp4 : TEXCOORD4;
        #endif
        #if !defined(LIGHTMAP_ON)
        float3 interp5 : TEXCOORD5;
        #endif
        float4 interp6 : TEXCOORD6;
        float4 interp7 : TEXCOORD7;
        #if UNITY_ANY_INSTANCING_ENABLED
        uint instanceID : CUSTOM_INSTANCE_ID;
        #endif
        #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
        uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
        #endif
        #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
        uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
        #endif
        #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
        FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
        #endif
    };

        PackedVaryings PackVaryings(Varyings input)
    {
        PackedVaryings output;
        output.positionCS = input.positionCS;
        output.interp0.xyz = input.positionWS;
        output.interp1.xyz = input.normalWS;
        output.interp2.xyzw = input.tangentWS;
        output.interp3.xyz = input.viewDirectionWS;
        #if defined(LIGHTMAP_ON)
        output.interp4.xy = input.lightmapUV;
        #endif
        #if !defined(LIGHTMAP_ON)
        output.interp5.xyz = input.sh;
        #endif
        output.interp6.xyzw = input.fogFactorAndVertexLight;
        output.interp7.xyzw = input.shadowCoord;
        #if UNITY_ANY_INSTANCING_ENABLED
        output.instanceID = input.instanceID;
        #endif
        #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
        output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
        #endif
        #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
        output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
        #endif
        #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
        output.cullFace = input.cullFace;
        #endif
        return output;
    }
    Varyings UnpackVaryings(PackedVaryings input)
    {
        Varyings output;
        output.positionCS = input.positionCS;
        output.positionWS = input.interp0.xyz;
        output.normalWS = input.interp1.xyz;
        output.tangentWS = input.interp2.xyzw;
        output.viewDirectionWS = input.interp3.xyz;
        #if defined(LIGHTMAP_ON)
        output.lightmapUV = input.interp4.xy;
        #endif
        #if !defined(LIGHTMAP_ON)
        output.sh = input.interp5.xyz;
        #endif
        output.fogFactorAndVertexLight = input.interp6.xyzw;
        output.shadowCoord = input.interp7.xyzw;
        #if UNITY_ANY_INSTANCING_ENABLED
        output.instanceID = input.instanceID;
        #endif
        #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
        output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
        #endif
        #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
        output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
        #endif
        #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
        output.cullFace = input.cullFace;
        #endif
        return output;
    }

    // --------------------------------------------------
    // Graph

    // Graph Properties
    CBUFFER_START(UnityPerMaterial)
float4 Texture2D_818cca92a18946a3b27ffbaf1b40a062_TexelSize;
float4 Texture2D_3f2131083fc44d9b989206ab02babd86_TexelSize;
float4 Texture2D_b01d9e81c8e04473b8880e3878694e5e_TexelSize;
float Vector1_accfafba726746c885b8e27280080a86;
float Vector1_c5686f27abfe4d5081a5b5eee32afda8;
float Vector1_9ec3ae0c6d7a4e1f8b784c14876e5174;
float4 Color_dc69a74bd5704ea796654354ea94b8d9;
float Vector1_4f1d2b1f25a248f3923ab327466f2485;
CBUFFER_END

// Object and Global properties
SAMPLER(SamplerState_Linear_Repeat);
TEXTURE2D(Texture2D_818cca92a18946a3b27ffbaf1b40a062);
SAMPLER(samplerTexture2D_818cca92a18946a3b27ffbaf1b40a062);
TEXTURE2D(Texture2D_3f2131083fc44d9b989206ab02babd86);
SAMPLER(samplerTexture2D_3f2131083fc44d9b989206ab02babd86);
TEXTURE2D(Texture2D_b01d9e81c8e04473b8880e3878694e5e);
SAMPLER(samplerTexture2D_b01d9e81c8e04473b8880e3878694e5e);

// Graph Functions

void Unity_Multiply_float(float2 A, float2 B, out float2 Out)
{
    Out = A * B;
}

void Unity_Rotate_Radians_float(float2 UV, float2 Center, float Rotation, out float2 Out)
{
    //rotation matrix
    UV -= Center;
    float s = sin(Rotation);
    float c = cos(Rotation);

    //center rotation matrix
    float2x2 rMatrix = float2x2(c, -s, s, c);
    rMatrix *= 0.5;
    rMatrix += 0.5;
    rMatrix = rMatrix * 2 - 1;

    //multiply the UVs by the rotation matrix
    UV.xy = mul(UV.xy, rMatrix);
    UV += Center;

    Out = UV;
}

void Unity_Multiply_float(float4 A, float4 B, out float4 Out)
{
    Out = A * B;
}

void Unity_NormalStrength_float(float3 In, float Strength, out float3 Out)
{
    Out = float3(In.rg * Strength, lerp(1, In.b, saturate(Strength)));
}

void Unity_SceneDepth_Eye_float(float4 UV, out float Out)
{
    Out = LinearEyeDepth(SHADERGRAPH_SAMPLE_SCENE_DEPTH(UV.xy), _ZBufferParams);
}

void Unity_Subtract_float(float A, float B, out float Out)
{
    Out = A - B;
}

void Unity_Divide_float(float A, float B, out float Out)
{
    Out = A / B;
}

void Unity_Saturate_float(float In, out float Out)
{
    Out = saturate(In);
}

// Graph Vertex
struct VertexDescription
{
    float3 Position;
    float3 Normal;
    float3 Tangent;
};

VertexDescription VertexDescriptionFunction(VertexDescriptionInputs IN)
{
    VertexDescription description = (VertexDescription)0;
    description.Position = IN.ObjectSpacePosition;
    description.Normal = IN.ObjectSpaceNormal;
    description.Tangent = IN.ObjectSpaceTangent;
    return description;
}

// Graph Pixel
struct SurfaceDescription
{
    float3 BaseColor;
    float3 NormalTS;
    float3 Emission;
    float Metallic;
    float Smoothness;
    float Occlusion;
    float Alpha;
    float AlphaClipThreshold;
};

SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
{
    SurfaceDescription surface = (SurfaceDescription)0;
    UnityTexture2D _Property_c13bab846ff3488187905dd48c26755a_Out_0 = UnityBuildTexture2DStructNoScale(Texture2D_818cca92a18946a3b27ffbaf1b40a062);
    float _Split_a0d1d32f695a4ceabaf3057615b92541_R_1 = IN.AbsoluteWorldSpacePosition[0];
    float _Split_a0d1d32f695a4ceabaf3057615b92541_G_2 = IN.AbsoluteWorldSpacePosition[1];
    float _Split_a0d1d32f695a4ceabaf3057615b92541_B_3 = IN.AbsoluteWorldSpacePosition[2];
    float _Split_a0d1d32f695a4ceabaf3057615b92541_A_4 = 0;
    float2 _Vector2_f84baf3fcaab45edb30625c920fb91bd_Out_0 = float2(_Split_a0d1d32f695a4ceabaf3057615b92541_R_1, _Split_a0d1d32f695a4ceabaf3057615b92541_B_3);
    float _Property_505300b000564f4fbe41a0d1fb1dd9bb_Out_0 = Vector1_c5686f27abfe4d5081a5b5eee32afda8;
    float2 _Multiply_adf8c2949a394a748dcbaf9bbecc58b5_Out_2;
    Unity_Multiply_float(_Vector2_f84baf3fcaab45edb30625c920fb91bd_Out_0, (_Property_505300b000564f4fbe41a0d1fb1dd9bb_Out_0.xx), _Multiply_adf8c2949a394a748dcbaf9bbecc58b5_Out_2);
    float2 _Vector2_6df102a229904a54a01b4e43c9924cce_Out_0 = float2(0.5, 0.5);
    float _Property_25c7c338c2a340f99397839c68ce01bf_Out_0 = Vector1_9ec3ae0c6d7a4e1f8b784c14876e5174;
    float2 _Rotate_e74144c0506c44c79764e30df1dcc15c_Out_3;
    Unity_Rotate_Radians_float(_Multiply_adf8c2949a394a748dcbaf9bbecc58b5_Out_2, _Vector2_6df102a229904a54a01b4e43c9924cce_Out_0, _Property_25c7c338c2a340f99397839c68ce01bf_Out_0, _Rotate_e74144c0506c44c79764e30df1dcc15c_Out_3);
    float4 _SampleTexture2D_6958146740004e6383c135b2c28476d4_RGBA_0 = SAMPLE_TEXTURE2D(_Property_c13bab846ff3488187905dd48c26755a_Out_0.tex, _Property_c13bab846ff3488187905dd48c26755a_Out_0.samplerstate, _Rotate_e74144c0506c44c79764e30df1dcc15c_Out_3);
    float _SampleTexture2D_6958146740004e6383c135b2c28476d4_R_4 = _SampleTexture2D_6958146740004e6383c135b2c28476d4_RGBA_0.r;
    float _SampleTexture2D_6958146740004e6383c135b2c28476d4_G_5 = _SampleTexture2D_6958146740004e6383c135b2c28476d4_RGBA_0.g;
    float _SampleTexture2D_6958146740004e6383c135b2c28476d4_B_6 = _SampleTexture2D_6958146740004e6383c135b2c28476d4_RGBA_0.b;
    float _SampleTexture2D_6958146740004e6383c135b2c28476d4_A_7 = _SampleTexture2D_6958146740004e6383c135b2c28476d4_RGBA_0.a;
    float4 _Property_13e483cd51b8490b9a44512c4f249ccb_Out_0 = Color_dc69a74bd5704ea796654354ea94b8d9;
    float4 _Multiply_cb02d0721341452692ce3355b85e3b79_Out_2;
    Unity_Multiply_float(_SampleTexture2D_6958146740004e6383c135b2c28476d4_RGBA_0, _Property_13e483cd51b8490b9a44512c4f249ccb_Out_0, _Multiply_cb02d0721341452692ce3355b85e3b79_Out_2);
    UnityTexture2D _Property_a6c0ed483ac044e08dbc27cbaab9842b_Out_0 = UnityBuildTexture2DStructNoScale(Texture2D_b01d9e81c8e04473b8880e3878694e5e);
    float4 _SampleTexture2D_b451781d16ed4f80b11c1edbdc0145a1_RGBA_0 = SAMPLE_TEXTURE2D(_Property_a6c0ed483ac044e08dbc27cbaab9842b_Out_0.tex, _Property_a6c0ed483ac044e08dbc27cbaab9842b_Out_0.samplerstate, _Rotate_e74144c0506c44c79764e30df1dcc15c_Out_3);
    _SampleTexture2D_b451781d16ed4f80b11c1edbdc0145a1_RGBA_0.rgb = UnpackNormal(_SampleTexture2D_b451781d16ed4f80b11c1edbdc0145a1_RGBA_0);
    float _SampleTexture2D_b451781d16ed4f80b11c1edbdc0145a1_R_4 = _SampleTexture2D_b451781d16ed4f80b11c1edbdc0145a1_RGBA_0.r;
    float _SampleTexture2D_b451781d16ed4f80b11c1edbdc0145a1_G_5 = _SampleTexture2D_b451781d16ed4f80b11c1edbdc0145a1_RGBA_0.g;
    float _SampleTexture2D_b451781d16ed4f80b11c1edbdc0145a1_B_6 = _SampleTexture2D_b451781d16ed4f80b11c1edbdc0145a1_RGBA_0.b;
    float _SampleTexture2D_b451781d16ed4f80b11c1edbdc0145a1_A_7 = _SampleTexture2D_b451781d16ed4f80b11c1edbdc0145a1_RGBA_0.a;
    float _Property_99d8f29d83c44a4ba32c6eabbbc92ab2_Out_0 = Vector1_accfafba726746c885b8e27280080a86;
    float3 _NormalStrength_1323c37b2b684b1193977b46a393a553_Out_2;
    Unity_NormalStrength_float((_SampleTexture2D_b451781d16ed4f80b11c1edbdc0145a1_RGBA_0.xyz), _Property_99d8f29d83c44a4ba32c6eabbbc92ab2_Out_0, _NormalStrength_1323c37b2b684b1193977b46a393a553_Out_2);
    UnityTexture2D _Property_8d4bdc48c17846329687d49c0a59d87d_Out_0 = UnityBuildTexture2DStructNoScale(Texture2D_3f2131083fc44d9b989206ab02babd86);
    float4 _SampleTexture2D_056131d47da244f48c8ea7a72bb86401_RGBA_0 = SAMPLE_TEXTURE2D(_Property_8d4bdc48c17846329687d49c0a59d87d_Out_0.tex, _Property_8d4bdc48c17846329687d49c0a59d87d_Out_0.samplerstate, _Rotate_e74144c0506c44c79764e30df1dcc15c_Out_3);
    float _SampleTexture2D_056131d47da244f48c8ea7a72bb86401_R_4 = _SampleTexture2D_056131d47da244f48c8ea7a72bb86401_RGBA_0.r;
    float _SampleTexture2D_056131d47da244f48c8ea7a72bb86401_G_5 = _SampleTexture2D_056131d47da244f48c8ea7a72bb86401_RGBA_0.g;
    float _SampleTexture2D_056131d47da244f48c8ea7a72bb86401_B_6 = _SampleTexture2D_056131d47da244f48c8ea7a72bb86401_RGBA_0.b;
    float _SampleTexture2D_056131d47da244f48c8ea7a72bb86401_A_7 = _SampleTexture2D_056131d47da244f48c8ea7a72bb86401_RGBA_0.a;
    float _SceneDepth_4b924296a5444c71987b488d105a7c45_Out_1;
    Unity_SceneDepth_Eye_float(float4(IN.ScreenPosition.xy / IN.ScreenPosition.w, 0, 0), _SceneDepth_4b924296a5444c71987b488d105a7c45_Out_1);
    float4 _ScreenPosition_088c919562d7448b8f64e5e7c85bef8f_Out_0 = IN.ScreenPosition;
    float _Split_715c1865a284495abb42c53b6db4bb77_R_1 = _ScreenPosition_088c919562d7448b8f64e5e7c85bef8f_Out_0[0];
    float _Split_715c1865a284495abb42c53b6db4bb77_G_2 = _ScreenPosition_088c919562d7448b8f64e5e7c85bef8f_Out_0[1];
    float _Split_715c1865a284495abb42c53b6db4bb77_B_3 = _ScreenPosition_088c919562d7448b8f64e5e7c85bef8f_Out_0[2];
    float _Split_715c1865a284495abb42c53b6db4bb77_A_4 = _ScreenPosition_088c919562d7448b8f64e5e7c85bef8f_Out_0[3];
    float _Subtract_94c5a0cdecd3465c8ebce61a76126ae9_Out_2;
    Unity_Subtract_float(_SceneDepth_4b924296a5444c71987b488d105a7c45_Out_1, _Split_715c1865a284495abb42c53b6db4bb77_A_4, _Subtract_94c5a0cdecd3465c8ebce61a76126ae9_Out_2);
    float _Property_c2fb2ab4753f4a67b952b67243833c4a_Out_0 = Vector1_4f1d2b1f25a248f3923ab327466f2485;
    float _Divide_6b505606b9094bd6890b7e45cb0eb021_Out_2;
    Unity_Divide_float(_Subtract_94c5a0cdecd3465c8ebce61a76126ae9_Out_2, _Property_c2fb2ab4753f4a67b952b67243833c4a_Out_0, _Divide_6b505606b9094bd6890b7e45cb0eb021_Out_2);
    float _Saturate_39416b919e4d4793892c252123310234_Out_1;
    Unity_Saturate_float(_Divide_6b505606b9094bd6890b7e45cb0eb021_Out_2, _Saturate_39416b919e4d4793892c252123310234_Out_1);
    surface.BaseColor = (_Multiply_cb02d0721341452692ce3355b85e3b79_Out_2.xyz);
    surface.NormalTS = _NormalStrength_1323c37b2b684b1193977b46a393a553_Out_2;
    surface.Emission = float3(0, 0, 0);
    surface.Metallic = _SampleTexture2D_056131d47da244f48c8ea7a72bb86401_B_6;
    surface.Smoothness = _SampleTexture2D_056131d47da244f48c8ea7a72bb86401_G_5;
    surface.Occlusion = _SampleTexture2D_056131d47da244f48c8ea7a72bb86401_R_4;
    surface.Alpha = _Saturate_39416b919e4d4793892c252123310234_Out_1;
    surface.AlphaClipThreshold = 0.1;
    return surface;
}

// --------------------------------------------------
// Build Graph Inputs

VertexDescriptionInputs BuildVertexDescriptionInputs(Attributes input)
{
    VertexDescriptionInputs output;
    ZERO_INITIALIZE(VertexDescriptionInputs, output);

    output.ObjectSpaceNormal = input.normalOS;
    output.ObjectSpaceTangent = input.tangentOS.xyz;
    output.ObjectSpacePosition = input.positionOS;

    return output;
}
    SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
{
    SurfaceDescriptionInputs output;
    ZERO_INITIALIZE(SurfaceDescriptionInputs, output);



    output.TangentSpaceNormal = float3(0.0f, 0.0f, 1.0f);


    output.WorldSpacePosition = input.positionWS;
    output.AbsoluteWorldSpacePosition = GetAbsolutePositionWS(input.positionWS);
    output.ScreenPosition = ComputeScreenPos(TransformWorldToHClip(input.positionWS), _ProjectionParams.x);
#if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
#define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN output.FaceSign =                    IS_FRONT_VFACE(input.cullFace, true, false);
#else
#define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
#endif
#undef BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN

    return output;
}

    // --------------------------------------------------
    // Main

    #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"
#include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/Varyings.hlsl"
#include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/PBRForwardPass.hlsl"

    ENDHLSL
}
Pass
{
    Name "GBuffer"
    Tags
    {
        "LightMode" = "UniversalGBuffer"
    }

        // Render State
        Cull Back
    Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha
    ZTest LEqual
    ZWrite Off

        // Debug
        // <None>

        // --------------------------------------------------
        // Pass

        HLSLPROGRAM

        // Pragmas
        #pragma target 4.5
    #pragma exclude_renderers gles gles3 glcore
    #pragma multi_compile_instancing
    #pragma multi_compile_fog
    #pragma multi_compile _ DOTS_INSTANCING_ON
    #pragma vertex vert
    #pragma fragment frag

        // DotsInstancingOptions: <None>
        // HybridV1InjectedBuiltinProperties: <None>

        // Keywords
        #pragma multi_compile _ LIGHTMAP_ON
    #pragma multi_compile _ DIRLIGHTMAP_COMBINED
    #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
    #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
    #pragma multi_compile _ _SHADOWS_SOFT
    #pragma multi_compile _ _MIXED_LIGHTING_SUBTRACTIVE
    #pragma multi_compile _ _GBUFFER_NORMALS_OCT
        // GraphKeywords: <None>

        // Defines
        #define _SURFACE_TYPE_TRANSPARENT 1
        #define _AlphaClip 1
        #define _NORMALMAP 1
        #define _NORMAL_DROPOFF_TS 1
        #define ATTRIBUTES_NEED_NORMAL
        #define ATTRIBUTES_NEED_TANGENT
        #define ATTRIBUTES_NEED_TEXCOORD1
        #define VARYINGS_NEED_POSITION_WS
        #define VARYINGS_NEED_NORMAL_WS
        #define VARYINGS_NEED_TANGENT_WS
        #define VARYINGS_NEED_VIEWDIRECTION_WS
        #define VARYINGS_NEED_FOG_AND_VERTEX_LIGHT
        #define FEATURES_GRAPH_VERTEX
        /* WARNING: $splice Could not find named fragment 'PassInstancing' */
        #define SHADERPASS SHADERPASS_GBUFFER
    #define REQUIRE_DEPTH_TEXTURE
        /* WARNING: $splice Could not find named fragment 'DotsInstancingVars' */

        // Includes
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
    #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
    #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"

        // --------------------------------------------------
        // Structs and Packing

        struct Attributes
    {
        float3 positionOS : POSITION;
        float3 normalOS : NORMAL;
        float4 tangentOS : TANGENT;
        float4 uv1 : TEXCOORD1;
        #if UNITY_ANY_INSTANCING_ENABLED
        uint instanceID : INSTANCEID_SEMANTIC;
        #endif
    };
    struct Varyings
    {
        float4 positionCS : SV_POSITION;
        float3 positionWS;
        float3 normalWS;
        float4 tangentWS;
        float3 viewDirectionWS;
        #if defined(LIGHTMAP_ON)
        float2 lightmapUV;
        #endif
        #if !defined(LIGHTMAP_ON)
        float3 sh;
        #endif
        float4 fogFactorAndVertexLight;
        float4 shadowCoord;
        #if UNITY_ANY_INSTANCING_ENABLED
        uint instanceID : CUSTOM_INSTANCE_ID;
        #endif
        #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
        uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
        #endif
        #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
        uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
        #endif
        #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
        FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
        #endif
    };
    struct SurfaceDescriptionInputs
    {
        float3 TangentSpaceNormal;
        float3 WorldSpacePosition;
        float3 AbsoluteWorldSpacePosition;
        float4 ScreenPosition;
    };
    struct VertexDescriptionInputs
    {
        float3 ObjectSpaceNormal;
        float3 ObjectSpaceTangent;
        float3 ObjectSpacePosition;
    };
    struct PackedVaryings
    {
        float4 positionCS : SV_POSITION;
        float3 interp0 : TEXCOORD0;
        float3 interp1 : TEXCOORD1;
        float4 interp2 : TEXCOORD2;
        float3 interp3 : TEXCOORD3;
        #if defined(LIGHTMAP_ON)
        float2 interp4 : TEXCOORD4;
        #endif
        #if !defined(LIGHTMAP_ON)
        float3 interp5 : TEXCOORD5;
        #endif
        float4 interp6 : TEXCOORD6;
        float4 interp7 : TEXCOORD7;
        #if UNITY_ANY_INSTANCING_ENABLED
        uint instanceID : CUSTOM_INSTANCE_ID;
        #endif
        #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
        uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
        #endif
        #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
        uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
        #endif
        #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
        FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
        #endif
    };

        PackedVaryings PackVaryings(Varyings input)
    {
        PackedVaryings output;
        output.positionCS = input.positionCS;
        output.interp0.xyz = input.positionWS;
        output.interp1.xyz = input.normalWS;
        output.interp2.xyzw = input.tangentWS;
        output.interp3.xyz = input.viewDirectionWS;
        #if defined(LIGHTMAP_ON)
        output.interp4.xy = input.lightmapUV;
        #endif
        #if !defined(LIGHTMAP_ON)
        output.interp5.xyz = input.sh;
        #endif
        output.interp6.xyzw = input.fogFactorAndVertexLight;
        output.interp7.xyzw = input.shadowCoord;
        #if UNITY_ANY_INSTANCING_ENABLED
        output.instanceID = input.instanceID;
        #endif
        #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
        output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
        #endif
        #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
        output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
        #endif
        #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
        output.cullFace = input.cullFace;
        #endif
        return output;
    }
    Varyings UnpackVaryings(PackedVaryings input)
    {
        Varyings output;
        output.positionCS = input.positionCS;
        output.positionWS = input.interp0.xyz;
        output.normalWS = input.interp1.xyz;
        output.tangentWS = input.interp2.xyzw;
        output.viewDirectionWS = input.interp3.xyz;
        #if defined(LIGHTMAP_ON)
        output.lightmapUV = input.interp4.xy;
        #endif
        #if !defined(LIGHTMAP_ON)
        output.sh = input.interp5.xyz;
        #endif
        output.fogFactorAndVertexLight = input.interp6.xyzw;
        output.shadowCoord = input.interp7.xyzw;
        #if UNITY_ANY_INSTANCING_ENABLED
        output.instanceID = input.instanceID;
        #endif
        #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
        output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
        #endif
        #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
        output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
        #endif
        #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
        output.cullFace = input.cullFace;
        #endif
        return output;
    }

    // --------------------------------------------------
    // Graph

    // Graph Properties
    CBUFFER_START(UnityPerMaterial)
float4 Texture2D_818cca92a18946a3b27ffbaf1b40a062_TexelSize;
float4 Texture2D_3f2131083fc44d9b989206ab02babd86_TexelSize;
float4 Texture2D_b01d9e81c8e04473b8880e3878694e5e_TexelSize;
float Vector1_accfafba726746c885b8e27280080a86;
float Vector1_c5686f27abfe4d5081a5b5eee32afda8;
float Vector1_9ec3ae0c6d7a4e1f8b784c14876e5174;
float4 Color_dc69a74bd5704ea796654354ea94b8d9;
float Vector1_4f1d2b1f25a248f3923ab327466f2485;
CBUFFER_END

// Object and Global properties
SAMPLER(SamplerState_Linear_Repeat);
TEXTURE2D(Texture2D_818cca92a18946a3b27ffbaf1b40a062);
SAMPLER(samplerTexture2D_818cca92a18946a3b27ffbaf1b40a062);
TEXTURE2D(Texture2D_3f2131083fc44d9b989206ab02babd86);
SAMPLER(samplerTexture2D_3f2131083fc44d9b989206ab02babd86);
TEXTURE2D(Texture2D_b01d9e81c8e04473b8880e3878694e5e);
SAMPLER(samplerTexture2D_b01d9e81c8e04473b8880e3878694e5e);

// Graph Functions

void Unity_Multiply_float(float2 A, float2 B, out float2 Out)
{
    Out = A * B;
}

void Unity_Rotate_Radians_float(float2 UV, float2 Center, float Rotation, out float2 Out)
{
    //rotation matrix
    UV -= Center;
    float s = sin(Rotation);
    float c = cos(Rotation);

    //center rotation matrix
    float2x2 rMatrix = float2x2(c, -s, s, c);
    rMatrix *= 0.5;
    rMatrix += 0.5;
    rMatrix = rMatrix * 2 - 1;

    //multiply the UVs by the rotation matrix
    UV.xy = mul(UV.xy, rMatrix);
    UV += Center;

    Out = UV;
}

void Unity_Multiply_float(float4 A, float4 B, out float4 Out)
{
    Out = A * B;
}

void Unity_NormalStrength_float(float3 In, float Strength, out float3 Out)
{
    Out = float3(In.rg * Strength, lerp(1, In.b, saturate(Strength)));
}

void Unity_SceneDepth_Eye_float(float4 UV, out float Out)
{
    Out = LinearEyeDepth(SHADERGRAPH_SAMPLE_SCENE_DEPTH(UV.xy), _ZBufferParams);
}

void Unity_Subtract_float(float A, float B, out float Out)
{
    Out = A - B;
}

void Unity_Divide_float(float A, float B, out float Out)
{
    Out = A / B;
}

void Unity_Saturate_float(float In, out float Out)
{
    Out = saturate(In);
}

// Graph Vertex
struct VertexDescription
{
    float3 Position;
    float3 Normal;
    float3 Tangent;
};

VertexDescription VertexDescriptionFunction(VertexDescriptionInputs IN)
{
    VertexDescription description = (VertexDescription)0;
    description.Position = IN.ObjectSpacePosition;
    description.Normal = IN.ObjectSpaceNormal;
    description.Tangent = IN.ObjectSpaceTangent;
    return description;
}

// Graph Pixel
struct SurfaceDescription
{
    float3 BaseColor;
    float3 NormalTS;
    float3 Emission;
    float Metallic;
    float Smoothness;
    float Occlusion;
    float Alpha;
    float AlphaClipThreshold;
};

SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
{
    SurfaceDescription surface = (SurfaceDescription)0;
    UnityTexture2D _Property_c13bab846ff3488187905dd48c26755a_Out_0 = UnityBuildTexture2DStructNoScale(Texture2D_818cca92a18946a3b27ffbaf1b40a062);
    float _Split_a0d1d32f695a4ceabaf3057615b92541_R_1 = IN.AbsoluteWorldSpacePosition[0];
    float _Split_a0d1d32f695a4ceabaf3057615b92541_G_2 = IN.AbsoluteWorldSpacePosition[1];
    float _Split_a0d1d32f695a4ceabaf3057615b92541_B_3 = IN.AbsoluteWorldSpacePosition[2];
    float _Split_a0d1d32f695a4ceabaf3057615b92541_A_4 = 0;
    float2 _Vector2_f84baf3fcaab45edb30625c920fb91bd_Out_0 = float2(_Split_a0d1d32f695a4ceabaf3057615b92541_R_1, _Split_a0d1d32f695a4ceabaf3057615b92541_B_3);
    float _Property_505300b000564f4fbe41a0d1fb1dd9bb_Out_0 = Vector1_c5686f27abfe4d5081a5b5eee32afda8;
    float2 _Multiply_adf8c2949a394a748dcbaf9bbecc58b5_Out_2;
    Unity_Multiply_float(_Vector2_f84baf3fcaab45edb30625c920fb91bd_Out_0, (_Property_505300b000564f4fbe41a0d1fb1dd9bb_Out_0.xx), _Multiply_adf8c2949a394a748dcbaf9bbecc58b5_Out_2);
    float2 _Vector2_6df102a229904a54a01b4e43c9924cce_Out_0 = float2(0.5, 0.5);
    float _Property_25c7c338c2a340f99397839c68ce01bf_Out_0 = Vector1_9ec3ae0c6d7a4e1f8b784c14876e5174;
    float2 _Rotate_e74144c0506c44c79764e30df1dcc15c_Out_3;
    Unity_Rotate_Radians_float(_Multiply_adf8c2949a394a748dcbaf9bbecc58b5_Out_2, _Vector2_6df102a229904a54a01b4e43c9924cce_Out_0, _Property_25c7c338c2a340f99397839c68ce01bf_Out_0, _Rotate_e74144c0506c44c79764e30df1dcc15c_Out_3);
    float4 _SampleTexture2D_6958146740004e6383c135b2c28476d4_RGBA_0 = SAMPLE_TEXTURE2D(_Property_c13bab846ff3488187905dd48c26755a_Out_0.tex, _Property_c13bab846ff3488187905dd48c26755a_Out_0.samplerstate, _Rotate_e74144c0506c44c79764e30df1dcc15c_Out_3);
    float _SampleTexture2D_6958146740004e6383c135b2c28476d4_R_4 = _SampleTexture2D_6958146740004e6383c135b2c28476d4_RGBA_0.r;
    float _SampleTexture2D_6958146740004e6383c135b2c28476d4_G_5 = _SampleTexture2D_6958146740004e6383c135b2c28476d4_RGBA_0.g;
    float _SampleTexture2D_6958146740004e6383c135b2c28476d4_B_6 = _SampleTexture2D_6958146740004e6383c135b2c28476d4_RGBA_0.b;
    float _SampleTexture2D_6958146740004e6383c135b2c28476d4_A_7 = _SampleTexture2D_6958146740004e6383c135b2c28476d4_RGBA_0.a;
    float4 _Property_13e483cd51b8490b9a44512c4f249ccb_Out_0 = Color_dc69a74bd5704ea796654354ea94b8d9;
    float4 _Multiply_cb02d0721341452692ce3355b85e3b79_Out_2;
    Unity_Multiply_float(_SampleTexture2D_6958146740004e6383c135b2c28476d4_RGBA_0, _Property_13e483cd51b8490b9a44512c4f249ccb_Out_0, _Multiply_cb02d0721341452692ce3355b85e3b79_Out_2);
    UnityTexture2D _Property_a6c0ed483ac044e08dbc27cbaab9842b_Out_0 = UnityBuildTexture2DStructNoScale(Texture2D_b01d9e81c8e04473b8880e3878694e5e);
    float4 _SampleTexture2D_b451781d16ed4f80b11c1edbdc0145a1_RGBA_0 = SAMPLE_TEXTURE2D(_Property_a6c0ed483ac044e08dbc27cbaab9842b_Out_0.tex, _Property_a6c0ed483ac044e08dbc27cbaab9842b_Out_0.samplerstate, _Rotate_e74144c0506c44c79764e30df1dcc15c_Out_3);
    _SampleTexture2D_b451781d16ed4f80b11c1edbdc0145a1_RGBA_0.rgb = UnpackNormal(_SampleTexture2D_b451781d16ed4f80b11c1edbdc0145a1_RGBA_0);
    float _SampleTexture2D_b451781d16ed4f80b11c1edbdc0145a1_R_4 = _SampleTexture2D_b451781d16ed4f80b11c1edbdc0145a1_RGBA_0.r;
    float _SampleTexture2D_b451781d16ed4f80b11c1edbdc0145a1_G_5 = _SampleTexture2D_b451781d16ed4f80b11c1edbdc0145a1_RGBA_0.g;
    float _SampleTexture2D_b451781d16ed4f80b11c1edbdc0145a1_B_6 = _SampleTexture2D_b451781d16ed4f80b11c1edbdc0145a1_RGBA_0.b;
    float _SampleTexture2D_b451781d16ed4f80b11c1edbdc0145a1_A_7 = _SampleTexture2D_b451781d16ed4f80b11c1edbdc0145a1_RGBA_0.a;
    float _Property_99d8f29d83c44a4ba32c6eabbbc92ab2_Out_0 = Vector1_accfafba726746c885b8e27280080a86;
    float3 _NormalStrength_1323c37b2b684b1193977b46a393a553_Out_2;
    Unity_NormalStrength_float((_SampleTexture2D_b451781d16ed4f80b11c1edbdc0145a1_RGBA_0.xyz), _Property_99d8f29d83c44a4ba32c6eabbbc92ab2_Out_0, _NormalStrength_1323c37b2b684b1193977b46a393a553_Out_2);
    UnityTexture2D _Property_8d4bdc48c17846329687d49c0a59d87d_Out_0 = UnityBuildTexture2DStructNoScale(Texture2D_3f2131083fc44d9b989206ab02babd86);
    float4 _SampleTexture2D_056131d47da244f48c8ea7a72bb86401_RGBA_0 = SAMPLE_TEXTURE2D(_Property_8d4bdc48c17846329687d49c0a59d87d_Out_0.tex, _Property_8d4bdc48c17846329687d49c0a59d87d_Out_0.samplerstate, _Rotate_e74144c0506c44c79764e30df1dcc15c_Out_3);
    float _SampleTexture2D_056131d47da244f48c8ea7a72bb86401_R_4 = _SampleTexture2D_056131d47da244f48c8ea7a72bb86401_RGBA_0.r;
    float _SampleTexture2D_056131d47da244f48c8ea7a72bb86401_G_5 = _SampleTexture2D_056131d47da244f48c8ea7a72bb86401_RGBA_0.g;
    float _SampleTexture2D_056131d47da244f48c8ea7a72bb86401_B_6 = _SampleTexture2D_056131d47da244f48c8ea7a72bb86401_RGBA_0.b;
    float _SampleTexture2D_056131d47da244f48c8ea7a72bb86401_A_7 = _SampleTexture2D_056131d47da244f48c8ea7a72bb86401_RGBA_0.a;
    float _SceneDepth_4b924296a5444c71987b488d105a7c45_Out_1;
    Unity_SceneDepth_Eye_float(float4(IN.ScreenPosition.xy / IN.ScreenPosition.w, 0, 0), _SceneDepth_4b924296a5444c71987b488d105a7c45_Out_1);
    float4 _ScreenPosition_088c919562d7448b8f64e5e7c85bef8f_Out_0 = IN.ScreenPosition;
    float _Split_715c1865a284495abb42c53b6db4bb77_R_1 = _ScreenPosition_088c919562d7448b8f64e5e7c85bef8f_Out_0[0];
    float _Split_715c1865a284495abb42c53b6db4bb77_G_2 = _ScreenPosition_088c919562d7448b8f64e5e7c85bef8f_Out_0[1];
    float _Split_715c1865a284495abb42c53b6db4bb77_B_3 = _ScreenPosition_088c919562d7448b8f64e5e7c85bef8f_Out_0[2];
    float _Split_715c1865a284495abb42c53b6db4bb77_A_4 = _ScreenPosition_088c919562d7448b8f64e5e7c85bef8f_Out_0[3];
    float _Subtract_94c5a0cdecd3465c8ebce61a76126ae9_Out_2;
    Unity_Subtract_float(_SceneDepth_4b924296a5444c71987b488d105a7c45_Out_1, _Split_715c1865a284495abb42c53b6db4bb77_A_4, _Subtract_94c5a0cdecd3465c8ebce61a76126ae9_Out_2);
    float _Property_c2fb2ab4753f4a67b952b67243833c4a_Out_0 = Vector1_4f1d2b1f25a248f3923ab327466f2485;
    float _Divide_6b505606b9094bd6890b7e45cb0eb021_Out_2;
    Unity_Divide_float(_Subtract_94c5a0cdecd3465c8ebce61a76126ae9_Out_2, _Property_c2fb2ab4753f4a67b952b67243833c4a_Out_0, _Divide_6b505606b9094bd6890b7e45cb0eb021_Out_2);
    float _Saturate_39416b919e4d4793892c252123310234_Out_1;
    Unity_Saturate_float(_Divide_6b505606b9094bd6890b7e45cb0eb021_Out_2, _Saturate_39416b919e4d4793892c252123310234_Out_1);
    surface.BaseColor = (_Multiply_cb02d0721341452692ce3355b85e3b79_Out_2.xyz);
    surface.NormalTS = _NormalStrength_1323c37b2b684b1193977b46a393a553_Out_2;
    surface.Emission = float3(0, 0, 0);
    surface.Metallic = _SampleTexture2D_056131d47da244f48c8ea7a72bb86401_B_6;
    surface.Smoothness = _SampleTexture2D_056131d47da244f48c8ea7a72bb86401_G_5;
    surface.Occlusion = _SampleTexture2D_056131d47da244f48c8ea7a72bb86401_R_4;
    surface.Alpha = _Saturate_39416b919e4d4793892c252123310234_Out_1;
    surface.AlphaClipThreshold = 0.1;
    return surface;
}

// --------------------------------------------------
// Build Graph Inputs

VertexDescriptionInputs BuildVertexDescriptionInputs(Attributes input)
{
    VertexDescriptionInputs output;
    ZERO_INITIALIZE(VertexDescriptionInputs, output);

    output.ObjectSpaceNormal = input.normalOS;
    output.ObjectSpaceTangent = input.tangentOS.xyz;
    output.ObjectSpacePosition = input.positionOS;

    return output;
}
    SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
{
    SurfaceDescriptionInputs output;
    ZERO_INITIALIZE(SurfaceDescriptionInputs, output);



    output.TangentSpaceNormal = float3(0.0f, 0.0f, 1.0f);


    output.WorldSpacePosition = input.positionWS;
    output.AbsoluteWorldSpacePosition = GetAbsolutePositionWS(input.positionWS);
    output.ScreenPosition = ComputeScreenPos(TransformWorldToHClip(input.positionWS), _ProjectionParams.x);
#if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
#define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN output.FaceSign =                    IS_FRONT_VFACE(input.cullFace, true, false);
#else
#define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
#endif
#undef BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN

    return output;
}

    // --------------------------------------------------
    // Main

    #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"
#include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/Varyings.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/UnityGBuffer.hlsl"
#include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/PBRGBufferPass.hlsl"

    ENDHLSL
}
Pass
{
    Name "ShadowCaster"
    Tags
    {
        "LightMode" = "ShadowCaster"
    }

        // Render State
        Cull Back
    Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha
    ZTest LEqual
    ZWrite On
    ColorMask 0

        // Debug
        // <None>

        // --------------------------------------------------
        // Pass

        HLSLPROGRAM

        // Pragmas
        #pragma target 4.5
    #pragma exclude_renderers gles gles3 glcore
    #pragma multi_compile_instancing
    #pragma multi_compile _ DOTS_INSTANCING_ON
    #pragma vertex vert
    #pragma fragment frag

        // DotsInstancingOptions: <None>
        // HybridV1InjectedBuiltinProperties: <None>

        // Keywords
        // PassKeywords: <None>
        // GraphKeywords: <None>

        // Defines
        #define _SURFACE_TYPE_TRANSPARENT 1
        #define _AlphaClip 1
        #define _NORMALMAP 1
        #define _NORMAL_DROPOFF_TS 1
        #define ATTRIBUTES_NEED_NORMAL
        #define ATTRIBUTES_NEED_TANGENT
        #define VARYINGS_NEED_POSITION_WS
        #define FEATURES_GRAPH_VERTEX
        /* WARNING: $splice Could not find named fragment 'PassInstancing' */
        #define SHADERPASS SHADERPASS_SHADOWCASTER
    #define REQUIRE_DEPTH_TEXTURE
        /* WARNING: $splice Could not find named fragment 'DotsInstancingVars' */

        // Includes
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
    #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
    #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"

        // --------------------------------------------------
        // Structs and Packing

        struct Attributes
    {
        float3 positionOS : POSITION;
        float3 normalOS : NORMAL;
        float4 tangentOS : TANGENT;
        #if UNITY_ANY_INSTANCING_ENABLED
        uint instanceID : INSTANCEID_SEMANTIC;
        #endif
    };
    struct Varyings
    {
        float4 positionCS : SV_POSITION;
        float3 positionWS;
        #if UNITY_ANY_INSTANCING_ENABLED
        uint instanceID : CUSTOM_INSTANCE_ID;
        #endif
        #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
        uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
        #endif
        #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
        uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
        #endif
        #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
        FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
        #endif
    };
    struct SurfaceDescriptionInputs
    {
        float3 WorldSpacePosition;
        float4 ScreenPosition;
    };
    struct VertexDescriptionInputs
    {
        float3 ObjectSpaceNormal;
        float3 ObjectSpaceTangent;
        float3 ObjectSpacePosition;
    };
    struct PackedVaryings
    {
        float4 positionCS : SV_POSITION;
        float3 interp0 : TEXCOORD0;
        #if UNITY_ANY_INSTANCING_ENABLED
        uint instanceID : CUSTOM_INSTANCE_ID;
        #endif
        #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
        uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
        #endif
        #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
        uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
        #endif
        #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
        FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
        #endif
    };

        PackedVaryings PackVaryings(Varyings input)
    {
        PackedVaryings output;
        output.positionCS = input.positionCS;
        output.interp0.xyz = input.positionWS;
        #if UNITY_ANY_INSTANCING_ENABLED
        output.instanceID = input.instanceID;
        #endif
        #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
        output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
        #endif
        #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
        output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
        #endif
        #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
        output.cullFace = input.cullFace;
        #endif
        return output;
    }
    Varyings UnpackVaryings(PackedVaryings input)
    {
        Varyings output;
        output.positionCS = input.positionCS;
        output.positionWS = input.interp0.xyz;
        #if UNITY_ANY_INSTANCING_ENABLED
        output.instanceID = input.instanceID;
        #endif
        #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
        output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
        #endif
        #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
        output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
        #endif
        #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
        output.cullFace = input.cullFace;
        #endif
        return output;
    }

    // --------------------------------------------------
    // Graph

    // Graph Properties
    CBUFFER_START(UnityPerMaterial)
float4 Texture2D_818cca92a18946a3b27ffbaf1b40a062_TexelSize;
float4 Texture2D_3f2131083fc44d9b989206ab02babd86_TexelSize;
float4 Texture2D_b01d9e81c8e04473b8880e3878694e5e_TexelSize;
float Vector1_accfafba726746c885b8e27280080a86;
float Vector1_c5686f27abfe4d5081a5b5eee32afda8;
float Vector1_9ec3ae0c6d7a4e1f8b784c14876e5174;
float4 Color_dc69a74bd5704ea796654354ea94b8d9;
float Vector1_4f1d2b1f25a248f3923ab327466f2485;
CBUFFER_END

// Object and Global properties
SAMPLER(SamplerState_Linear_Repeat);
TEXTURE2D(Texture2D_818cca92a18946a3b27ffbaf1b40a062);
SAMPLER(samplerTexture2D_818cca92a18946a3b27ffbaf1b40a062);
TEXTURE2D(Texture2D_3f2131083fc44d9b989206ab02babd86);
SAMPLER(samplerTexture2D_3f2131083fc44d9b989206ab02babd86);
TEXTURE2D(Texture2D_b01d9e81c8e04473b8880e3878694e5e);
SAMPLER(samplerTexture2D_b01d9e81c8e04473b8880e3878694e5e);

// Graph Functions

void Unity_SceneDepth_Eye_float(float4 UV, out float Out)
{
    Out = LinearEyeDepth(SHADERGRAPH_SAMPLE_SCENE_DEPTH(UV.xy), _ZBufferParams);
}

void Unity_Subtract_float(float A, float B, out float Out)
{
    Out = A - B;
}

void Unity_Divide_float(float A, float B, out float Out)
{
    Out = A / B;
}

void Unity_Saturate_float(float In, out float Out)
{
    Out = saturate(In);
}

// Graph Vertex
struct VertexDescription
{
    float3 Position;
    float3 Normal;
    float3 Tangent;
};

VertexDescription VertexDescriptionFunction(VertexDescriptionInputs IN)
{
    VertexDescription description = (VertexDescription)0;
    description.Position = IN.ObjectSpacePosition;
    description.Normal = IN.ObjectSpaceNormal;
    description.Tangent = IN.ObjectSpaceTangent;
    return description;
}

// Graph Pixel
struct SurfaceDescription
{
    float Alpha;
    float AlphaClipThreshold;
};

SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
{
    SurfaceDescription surface = (SurfaceDescription)0;
    float _SceneDepth_4b924296a5444c71987b488d105a7c45_Out_1;
    Unity_SceneDepth_Eye_float(float4(IN.ScreenPosition.xy / IN.ScreenPosition.w, 0, 0), _SceneDepth_4b924296a5444c71987b488d105a7c45_Out_1);
    float4 _ScreenPosition_088c919562d7448b8f64e5e7c85bef8f_Out_0 = IN.ScreenPosition;
    float _Split_715c1865a284495abb42c53b6db4bb77_R_1 = _ScreenPosition_088c919562d7448b8f64e5e7c85bef8f_Out_0[0];
    float _Split_715c1865a284495abb42c53b6db4bb77_G_2 = _ScreenPosition_088c919562d7448b8f64e5e7c85bef8f_Out_0[1];
    float _Split_715c1865a284495abb42c53b6db4bb77_B_3 = _ScreenPosition_088c919562d7448b8f64e5e7c85bef8f_Out_0[2];
    float _Split_715c1865a284495abb42c53b6db4bb77_A_4 = _ScreenPosition_088c919562d7448b8f64e5e7c85bef8f_Out_0[3];
    float _Subtract_94c5a0cdecd3465c8ebce61a76126ae9_Out_2;
    Unity_Subtract_float(_SceneDepth_4b924296a5444c71987b488d105a7c45_Out_1, _Split_715c1865a284495abb42c53b6db4bb77_A_4, _Subtract_94c5a0cdecd3465c8ebce61a76126ae9_Out_2);
    float _Property_c2fb2ab4753f4a67b952b67243833c4a_Out_0 = Vector1_4f1d2b1f25a248f3923ab327466f2485;
    float _Divide_6b505606b9094bd6890b7e45cb0eb021_Out_2;
    Unity_Divide_float(_Subtract_94c5a0cdecd3465c8ebce61a76126ae9_Out_2, _Property_c2fb2ab4753f4a67b952b67243833c4a_Out_0, _Divide_6b505606b9094bd6890b7e45cb0eb021_Out_2);
    float _Saturate_39416b919e4d4793892c252123310234_Out_1;
    Unity_Saturate_float(_Divide_6b505606b9094bd6890b7e45cb0eb021_Out_2, _Saturate_39416b919e4d4793892c252123310234_Out_1);
    surface.Alpha = _Saturate_39416b919e4d4793892c252123310234_Out_1;
    surface.AlphaClipThreshold = 0.1;
    return surface;
}

// --------------------------------------------------
// Build Graph Inputs

VertexDescriptionInputs BuildVertexDescriptionInputs(Attributes input)
{
    VertexDescriptionInputs output;
    ZERO_INITIALIZE(VertexDescriptionInputs, output);

    output.ObjectSpaceNormal = input.normalOS;
    output.ObjectSpaceTangent = input.tangentOS.xyz;
    output.ObjectSpacePosition = input.positionOS;

    return output;
}
    SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
{
    SurfaceDescriptionInputs output;
    ZERO_INITIALIZE(SurfaceDescriptionInputs, output);





    output.WorldSpacePosition = input.positionWS;
    output.ScreenPosition = ComputeScreenPos(TransformWorldToHClip(input.positionWS), _ProjectionParams.x);
#if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
#define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN output.FaceSign =                    IS_FRONT_VFACE(input.cullFace, true, false);
#else
#define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
#endif
#undef BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN

    return output;
}

    // --------------------------------------------------
    // Main

    #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"
#include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/Varyings.hlsl"
#include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShadowCasterPass.hlsl"

    ENDHLSL
}
Pass
{
    Name "DepthOnly"
    Tags
    {
        "LightMode" = "DepthOnly"
    }

        // Render State
        Cull Back
    Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha
    ZTest LEqual
    ZWrite On
    ColorMask 0

        // Debug
        // <None>

        // --------------------------------------------------
        // Pass

        HLSLPROGRAM

        // Pragmas
        #pragma target 4.5
    #pragma exclude_renderers gles gles3 glcore
    #pragma multi_compile_instancing
    #pragma multi_compile _ DOTS_INSTANCING_ON
    #pragma vertex vert
    #pragma fragment frag

        // DotsInstancingOptions: <None>
        // HybridV1InjectedBuiltinProperties: <None>

        // Keywords
        // PassKeywords: <None>
        // GraphKeywords: <None>

        // Defines
        #define _SURFACE_TYPE_TRANSPARENT 1
        #define _AlphaClip 1
        #define _NORMALMAP 1
        #define _NORMAL_DROPOFF_TS 1
        #define ATTRIBUTES_NEED_NORMAL
        #define ATTRIBUTES_NEED_TANGENT
        #define VARYINGS_NEED_POSITION_WS
        #define FEATURES_GRAPH_VERTEX
        /* WARNING: $splice Could not find named fragment 'PassInstancing' */
        #define SHADERPASS SHADERPASS_DEPTHONLY
    #define REQUIRE_DEPTH_TEXTURE
        /* WARNING: $splice Could not find named fragment 'DotsInstancingVars' */

        // Includes
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
    #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
    #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"

        // --------------------------------------------------
        // Structs and Packing

        struct Attributes
    {
        float3 positionOS : POSITION;
        float3 normalOS : NORMAL;
        float4 tangentOS : TANGENT;
        #if UNITY_ANY_INSTANCING_ENABLED
        uint instanceID : INSTANCEID_SEMANTIC;
        #endif
    };
    struct Varyings
    {
        float4 positionCS : SV_POSITION;
        float3 positionWS;
        #if UNITY_ANY_INSTANCING_ENABLED
        uint instanceID : CUSTOM_INSTANCE_ID;
        #endif
        #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
        uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
        #endif
        #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
        uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
        #endif
        #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
        FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
        #endif
    };
    struct SurfaceDescriptionInputs
    {
        float3 WorldSpacePosition;
        float4 ScreenPosition;
    };
    struct VertexDescriptionInputs
    {
        float3 ObjectSpaceNormal;
        float3 ObjectSpaceTangent;
        float3 ObjectSpacePosition;
    };
    struct PackedVaryings
    {
        float4 positionCS : SV_POSITION;
        float3 interp0 : TEXCOORD0;
        #if UNITY_ANY_INSTANCING_ENABLED
        uint instanceID : CUSTOM_INSTANCE_ID;
        #endif
        #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
        uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
        #endif
        #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
        uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
        #endif
        #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
        FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
        #endif
    };

        PackedVaryings PackVaryings(Varyings input)
    {
        PackedVaryings output;
        output.positionCS = input.positionCS;
        output.interp0.xyz = input.positionWS;
        #if UNITY_ANY_INSTANCING_ENABLED
        output.instanceID = input.instanceID;
        #endif
        #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
        output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
        #endif
        #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
        output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
        #endif
        #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
        output.cullFace = input.cullFace;
        #endif
        return output;
    }
    Varyings UnpackVaryings(PackedVaryings input)
    {
        Varyings output;
        output.positionCS = input.positionCS;
        output.positionWS = input.interp0.xyz;
        #if UNITY_ANY_INSTANCING_ENABLED
        output.instanceID = input.instanceID;
        #endif
        #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
        output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
        #endif
        #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
        output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
        #endif
        #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
        output.cullFace = input.cullFace;
        #endif
        return output;
    }

    // --------------------------------------------------
    // Graph

    // Graph Properties
    CBUFFER_START(UnityPerMaterial)
float4 Texture2D_818cca92a18946a3b27ffbaf1b40a062_TexelSize;
float4 Texture2D_3f2131083fc44d9b989206ab02babd86_TexelSize;
float4 Texture2D_b01d9e81c8e04473b8880e3878694e5e_TexelSize;
float Vector1_accfafba726746c885b8e27280080a86;
float Vector1_c5686f27abfe4d5081a5b5eee32afda8;
float Vector1_9ec3ae0c6d7a4e1f8b784c14876e5174;
float4 Color_dc69a74bd5704ea796654354ea94b8d9;
float Vector1_4f1d2b1f25a248f3923ab327466f2485;
CBUFFER_END

// Object and Global properties
SAMPLER(SamplerState_Linear_Repeat);
TEXTURE2D(Texture2D_818cca92a18946a3b27ffbaf1b40a062);
SAMPLER(samplerTexture2D_818cca92a18946a3b27ffbaf1b40a062);
TEXTURE2D(Texture2D_3f2131083fc44d9b989206ab02babd86);
SAMPLER(samplerTexture2D_3f2131083fc44d9b989206ab02babd86);
TEXTURE2D(Texture2D_b01d9e81c8e04473b8880e3878694e5e);
SAMPLER(samplerTexture2D_b01d9e81c8e04473b8880e3878694e5e);

// Graph Functions

void Unity_SceneDepth_Eye_float(float4 UV, out float Out)
{
    Out = LinearEyeDepth(SHADERGRAPH_SAMPLE_SCENE_DEPTH(UV.xy), _ZBufferParams);
}

void Unity_Subtract_float(float A, float B, out float Out)
{
    Out = A - B;
}

void Unity_Divide_float(float A, float B, out float Out)
{
    Out = A / B;
}

void Unity_Saturate_float(float In, out float Out)
{
    Out = saturate(In);
}

// Graph Vertex
struct VertexDescription
{
    float3 Position;
    float3 Normal;
    float3 Tangent;
};

VertexDescription VertexDescriptionFunction(VertexDescriptionInputs IN)
{
    VertexDescription description = (VertexDescription)0;
    description.Position = IN.ObjectSpacePosition;
    description.Normal = IN.ObjectSpaceNormal;
    description.Tangent = IN.ObjectSpaceTangent;
    return description;
}

// Graph Pixel
struct SurfaceDescription
{
    float Alpha;
    float AlphaClipThreshold;
};

SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
{
    SurfaceDescription surface = (SurfaceDescription)0;
    float _SceneDepth_4b924296a5444c71987b488d105a7c45_Out_1;
    Unity_SceneDepth_Eye_float(float4(IN.ScreenPosition.xy / IN.ScreenPosition.w, 0, 0), _SceneDepth_4b924296a5444c71987b488d105a7c45_Out_1);
    float4 _ScreenPosition_088c919562d7448b8f64e5e7c85bef8f_Out_0 = IN.ScreenPosition;
    float _Split_715c1865a284495abb42c53b6db4bb77_R_1 = _ScreenPosition_088c919562d7448b8f64e5e7c85bef8f_Out_0[0];
    float _Split_715c1865a284495abb42c53b6db4bb77_G_2 = _ScreenPosition_088c919562d7448b8f64e5e7c85bef8f_Out_0[1];
    float _Split_715c1865a284495abb42c53b6db4bb77_B_3 = _ScreenPosition_088c919562d7448b8f64e5e7c85bef8f_Out_0[2];
    float _Split_715c1865a284495abb42c53b6db4bb77_A_4 = _ScreenPosition_088c919562d7448b8f64e5e7c85bef8f_Out_0[3];
    float _Subtract_94c5a0cdecd3465c8ebce61a76126ae9_Out_2;
    Unity_Subtract_float(_SceneDepth_4b924296a5444c71987b488d105a7c45_Out_1, _Split_715c1865a284495abb42c53b6db4bb77_A_4, _Subtract_94c5a0cdecd3465c8ebce61a76126ae9_Out_2);
    float _Property_c2fb2ab4753f4a67b952b67243833c4a_Out_0 = Vector1_4f1d2b1f25a248f3923ab327466f2485;
    float _Divide_6b505606b9094bd6890b7e45cb0eb021_Out_2;
    Unity_Divide_float(_Subtract_94c5a0cdecd3465c8ebce61a76126ae9_Out_2, _Property_c2fb2ab4753f4a67b952b67243833c4a_Out_0, _Divide_6b505606b9094bd6890b7e45cb0eb021_Out_2);
    float _Saturate_39416b919e4d4793892c252123310234_Out_1;
    Unity_Saturate_float(_Divide_6b505606b9094bd6890b7e45cb0eb021_Out_2, _Saturate_39416b919e4d4793892c252123310234_Out_1);
    surface.Alpha = _Saturate_39416b919e4d4793892c252123310234_Out_1;
    surface.AlphaClipThreshold = 0.1;
    return surface;
}

// --------------------------------------------------
// Build Graph Inputs

VertexDescriptionInputs BuildVertexDescriptionInputs(Attributes input)
{
    VertexDescriptionInputs output;
    ZERO_INITIALIZE(VertexDescriptionInputs, output);

    output.ObjectSpaceNormal = input.normalOS;
    output.ObjectSpaceTangent = input.tangentOS.xyz;
    output.ObjectSpacePosition = input.positionOS;

    return output;
}
    SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
{
    SurfaceDescriptionInputs output;
    ZERO_INITIALIZE(SurfaceDescriptionInputs, output);





    output.WorldSpacePosition = input.positionWS;
    output.ScreenPosition = ComputeScreenPos(TransformWorldToHClip(input.positionWS), _ProjectionParams.x);
#if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
#define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN output.FaceSign =                    IS_FRONT_VFACE(input.cullFace, true, false);
#else
#define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
#endif
#undef BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN

    return output;
}

    // --------------------------------------------------
    // Main

    #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"
#include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/Varyings.hlsl"
#include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/DepthOnlyPass.hlsl"

    ENDHLSL
}
Pass
{
    Name "DepthNormals"
    Tags
    {
        "LightMode" = "DepthNormals"
    }

        // Render State
        Cull Back
    Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha
    ZTest LEqual
    ZWrite On

        // Debug
        // <None>

        // --------------------------------------------------
        // Pass

        HLSLPROGRAM

        // Pragmas
        #pragma target 4.5
    #pragma exclude_renderers gles gles3 glcore
    #pragma multi_compile_instancing
    #pragma multi_compile _ DOTS_INSTANCING_ON
    #pragma vertex vert
    #pragma fragment frag

        // DotsInstancingOptions: <None>
        // HybridV1InjectedBuiltinProperties: <None>

        // Keywords
        // PassKeywords: <None>
        // GraphKeywords: <None>

        // Defines
        #define _SURFACE_TYPE_TRANSPARENT 1
        #define _AlphaClip 1
        #define _NORMALMAP 1
        #define _NORMAL_DROPOFF_TS 1
        #define ATTRIBUTES_NEED_NORMAL
        #define ATTRIBUTES_NEED_TANGENT
        #define ATTRIBUTES_NEED_TEXCOORD1
        #define VARYINGS_NEED_POSITION_WS
        #define VARYINGS_NEED_NORMAL_WS
        #define VARYINGS_NEED_TANGENT_WS
        #define FEATURES_GRAPH_VERTEX
        /* WARNING: $splice Could not find named fragment 'PassInstancing' */
        #define SHADERPASS SHADERPASS_DEPTHNORMALSONLY
    #define REQUIRE_DEPTH_TEXTURE
        /* WARNING: $splice Could not find named fragment 'DotsInstancingVars' */

        // Includes
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
    #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
    #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"

        // --------------------------------------------------
        // Structs and Packing

        struct Attributes
    {
        float3 positionOS : POSITION;
        float3 normalOS : NORMAL;
        float4 tangentOS : TANGENT;
        float4 uv1 : TEXCOORD1;
        #if UNITY_ANY_INSTANCING_ENABLED
        uint instanceID : INSTANCEID_SEMANTIC;
        #endif
    };
    struct Varyings
    {
        float4 positionCS : SV_POSITION;
        float3 positionWS;
        float3 normalWS;
        float4 tangentWS;
        #if UNITY_ANY_INSTANCING_ENABLED
        uint instanceID : CUSTOM_INSTANCE_ID;
        #endif
        #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
        uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
        #endif
        #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
        uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
        #endif
        #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
        FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
        #endif
    };
    struct SurfaceDescriptionInputs
    {
        float3 TangentSpaceNormal;
        float3 WorldSpacePosition;
        float3 AbsoluteWorldSpacePosition;
        float4 ScreenPosition;
    };
    struct VertexDescriptionInputs
    {
        float3 ObjectSpaceNormal;
        float3 ObjectSpaceTangent;
        float3 ObjectSpacePosition;
    };
    struct PackedVaryings
    {
        float4 positionCS : SV_POSITION;
        float3 interp0 : TEXCOORD0;
        float3 interp1 : TEXCOORD1;
        float4 interp2 : TEXCOORD2;
        #if UNITY_ANY_INSTANCING_ENABLED
        uint instanceID : CUSTOM_INSTANCE_ID;
        #endif
        #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
        uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
        #endif
        #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
        uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
        #endif
        #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
        FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
        #endif
    };

        PackedVaryings PackVaryings(Varyings input)
    {
        PackedVaryings output;
        output.positionCS = input.positionCS;
        output.interp0.xyz = input.positionWS;
        output.interp1.xyz = input.normalWS;
        output.interp2.xyzw = input.tangentWS;
        #if UNITY_ANY_INSTANCING_ENABLED
        output.instanceID = input.instanceID;
        #endif
        #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
        output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
        #endif
        #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
        output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
        #endif
        #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
        output.cullFace = input.cullFace;
        #endif
        return output;
    }
    Varyings UnpackVaryings(PackedVaryings input)
    {
        Varyings output;
        output.positionCS = input.positionCS;
        output.positionWS = input.interp0.xyz;
        output.normalWS = input.interp1.xyz;
        output.tangentWS = input.interp2.xyzw;
        #if UNITY_ANY_INSTANCING_ENABLED
        output.instanceID = input.instanceID;
        #endif
        #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
        output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
        #endif
        #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
        output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
        #endif
        #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
        output.cullFace = input.cullFace;
        #endif
        return output;
    }

    // --------------------------------------------------
    // Graph

    // Graph Properties
    CBUFFER_START(UnityPerMaterial)
float4 Texture2D_818cca92a18946a3b27ffbaf1b40a062_TexelSize;
float4 Texture2D_3f2131083fc44d9b989206ab02babd86_TexelSize;
float4 Texture2D_b01d9e81c8e04473b8880e3878694e5e_TexelSize;
float Vector1_accfafba726746c885b8e27280080a86;
float Vector1_c5686f27abfe4d5081a5b5eee32afda8;
float Vector1_9ec3ae0c6d7a4e1f8b784c14876e5174;
float4 Color_dc69a74bd5704ea796654354ea94b8d9;
float Vector1_4f1d2b1f25a248f3923ab327466f2485;
CBUFFER_END

// Object and Global properties
SAMPLER(SamplerState_Linear_Repeat);
TEXTURE2D(Texture2D_818cca92a18946a3b27ffbaf1b40a062);
SAMPLER(samplerTexture2D_818cca92a18946a3b27ffbaf1b40a062);
TEXTURE2D(Texture2D_3f2131083fc44d9b989206ab02babd86);
SAMPLER(samplerTexture2D_3f2131083fc44d9b989206ab02babd86);
TEXTURE2D(Texture2D_b01d9e81c8e04473b8880e3878694e5e);
SAMPLER(samplerTexture2D_b01d9e81c8e04473b8880e3878694e5e);

// Graph Functions

void Unity_Multiply_float(float2 A, float2 B, out float2 Out)
{
    Out = A * B;
}

void Unity_Rotate_Radians_float(float2 UV, float2 Center, float Rotation, out float2 Out)
{
    //rotation matrix
    UV -= Center;
    float s = sin(Rotation);
    float c = cos(Rotation);

    //center rotation matrix
    float2x2 rMatrix = float2x2(c, -s, s, c);
    rMatrix *= 0.5;
    rMatrix += 0.5;
    rMatrix = rMatrix * 2 - 1;

    //multiply the UVs by the rotation matrix
    UV.xy = mul(UV.xy, rMatrix);
    UV += Center;

    Out = UV;
}

void Unity_NormalStrength_float(float3 In, float Strength, out float3 Out)
{
    Out = float3(In.rg * Strength, lerp(1, In.b, saturate(Strength)));
}

void Unity_SceneDepth_Eye_float(float4 UV, out float Out)
{
    Out = LinearEyeDepth(SHADERGRAPH_SAMPLE_SCENE_DEPTH(UV.xy), _ZBufferParams);
}

void Unity_Subtract_float(float A, float B, out float Out)
{
    Out = A - B;
}

void Unity_Divide_float(float A, float B, out float Out)
{
    Out = A / B;
}

void Unity_Saturate_float(float In, out float Out)
{
    Out = saturate(In);
}

// Graph Vertex
struct VertexDescription
{
    float3 Position;
    float3 Normal;
    float3 Tangent;
};

VertexDescription VertexDescriptionFunction(VertexDescriptionInputs IN)
{
    VertexDescription description = (VertexDescription)0;
    description.Position = IN.ObjectSpacePosition;
    description.Normal = IN.ObjectSpaceNormal;
    description.Tangent = IN.ObjectSpaceTangent;
    return description;
}

// Graph Pixel
struct SurfaceDescription
{
    float3 NormalTS;
    float Alpha;
    float AlphaClipThreshold;
};

SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
{
    SurfaceDescription surface = (SurfaceDescription)0;
    UnityTexture2D _Property_a6c0ed483ac044e08dbc27cbaab9842b_Out_0 = UnityBuildTexture2DStructNoScale(Texture2D_b01d9e81c8e04473b8880e3878694e5e);
    float _Split_a0d1d32f695a4ceabaf3057615b92541_R_1 = IN.AbsoluteWorldSpacePosition[0];
    float _Split_a0d1d32f695a4ceabaf3057615b92541_G_2 = IN.AbsoluteWorldSpacePosition[1];
    float _Split_a0d1d32f695a4ceabaf3057615b92541_B_3 = IN.AbsoluteWorldSpacePosition[2];
    float _Split_a0d1d32f695a4ceabaf3057615b92541_A_4 = 0;
    float2 _Vector2_f84baf3fcaab45edb30625c920fb91bd_Out_0 = float2(_Split_a0d1d32f695a4ceabaf3057615b92541_R_1, _Split_a0d1d32f695a4ceabaf3057615b92541_B_3);
    float _Property_505300b000564f4fbe41a0d1fb1dd9bb_Out_0 = Vector1_c5686f27abfe4d5081a5b5eee32afda8;
    float2 _Multiply_adf8c2949a394a748dcbaf9bbecc58b5_Out_2;
    Unity_Multiply_float(_Vector2_f84baf3fcaab45edb30625c920fb91bd_Out_0, (_Property_505300b000564f4fbe41a0d1fb1dd9bb_Out_0.xx), _Multiply_adf8c2949a394a748dcbaf9bbecc58b5_Out_2);
    float2 _Vector2_6df102a229904a54a01b4e43c9924cce_Out_0 = float2(0.5, 0.5);
    float _Property_25c7c338c2a340f99397839c68ce01bf_Out_0 = Vector1_9ec3ae0c6d7a4e1f8b784c14876e5174;
    float2 _Rotate_e74144c0506c44c79764e30df1dcc15c_Out_3;
    Unity_Rotate_Radians_float(_Multiply_adf8c2949a394a748dcbaf9bbecc58b5_Out_2, _Vector2_6df102a229904a54a01b4e43c9924cce_Out_0, _Property_25c7c338c2a340f99397839c68ce01bf_Out_0, _Rotate_e74144c0506c44c79764e30df1dcc15c_Out_3);
    float4 _SampleTexture2D_b451781d16ed4f80b11c1edbdc0145a1_RGBA_0 = SAMPLE_TEXTURE2D(_Property_a6c0ed483ac044e08dbc27cbaab9842b_Out_0.tex, _Property_a6c0ed483ac044e08dbc27cbaab9842b_Out_0.samplerstate, _Rotate_e74144c0506c44c79764e30df1dcc15c_Out_3);
    _SampleTexture2D_b451781d16ed4f80b11c1edbdc0145a1_RGBA_0.rgb = UnpackNormal(_SampleTexture2D_b451781d16ed4f80b11c1edbdc0145a1_RGBA_0);
    float _SampleTexture2D_b451781d16ed4f80b11c1edbdc0145a1_R_4 = _SampleTexture2D_b451781d16ed4f80b11c1edbdc0145a1_RGBA_0.r;
    float _SampleTexture2D_b451781d16ed4f80b11c1edbdc0145a1_G_5 = _SampleTexture2D_b451781d16ed4f80b11c1edbdc0145a1_RGBA_0.g;
    float _SampleTexture2D_b451781d16ed4f80b11c1edbdc0145a1_B_6 = _SampleTexture2D_b451781d16ed4f80b11c1edbdc0145a1_RGBA_0.b;
    float _SampleTexture2D_b451781d16ed4f80b11c1edbdc0145a1_A_7 = _SampleTexture2D_b451781d16ed4f80b11c1edbdc0145a1_RGBA_0.a;
    float _Property_99d8f29d83c44a4ba32c6eabbbc92ab2_Out_0 = Vector1_accfafba726746c885b8e27280080a86;
    float3 _NormalStrength_1323c37b2b684b1193977b46a393a553_Out_2;
    Unity_NormalStrength_float((_SampleTexture2D_b451781d16ed4f80b11c1edbdc0145a1_RGBA_0.xyz), _Property_99d8f29d83c44a4ba32c6eabbbc92ab2_Out_0, _NormalStrength_1323c37b2b684b1193977b46a393a553_Out_2);
    float _SceneDepth_4b924296a5444c71987b488d105a7c45_Out_1;
    Unity_SceneDepth_Eye_float(float4(IN.ScreenPosition.xy / IN.ScreenPosition.w, 0, 0), _SceneDepth_4b924296a5444c71987b488d105a7c45_Out_1);
    float4 _ScreenPosition_088c919562d7448b8f64e5e7c85bef8f_Out_0 = IN.ScreenPosition;
    float _Split_715c1865a284495abb42c53b6db4bb77_R_1 = _ScreenPosition_088c919562d7448b8f64e5e7c85bef8f_Out_0[0];
    float _Split_715c1865a284495abb42c53b6db4bb77_G_2 = _ScreenPosition_088c919562d7448b8f64e5e7c85bef8f_Out_0[1];
    float _Split_715c1865a284495abb42c53b6db4bb77_B_3 = _ScreenPosition_088c919562d7448b8f64e5e7c85bef8f_Out_0[2];
    float _Split_715c1865a284495abb42c53b6db4bb77_A_4 = _ScreenPosition_088c919562d7448b8f64e5e7c85bef8f_Out_0[3];
    float _Subtract_94c5a0cdecd3465c8ebce61a76126ae9_Out_2;
    Unity_Subtract_float(_SceneDepth_4b924296a5444c71987b488d105a7c45_Out_1, _Split_715c1865a284495abb42c53b6db4bb77_A_4, _Subtract_94c5a0cdecd3465c8ebce61a76126ae9_Out_2);
    float _Property_c2fb2ab4753f4a67b952b67243833c4a_Out_0 = Vector1_4f1d2b1f25a248f3923ab327466f2485;
    float _Divide_6b505606b9094bd6890b7e45cb0eb021_Out_2;
    Unity_Divide_float(_Subtract_94c5a0cdecd3465c8ebce61a76126ae9_Out_2, _Property_c2fb2ab4753f4a67b952b67243833c4a_Out_0, _Divide_6b505606b9094bd6890b7e45cb0eb021_Out_2);
    float _Saturate_39416b919e4d4793892c252123310234_Out_1;
    Unity_Saturate_float(_Divide_6b505606b9094bd6890b7e45cb0eb021_Out_2, _Saturate_39416b919e4d4793892c252123310234_Out_1);
    surface.NormalTS = _NormalStrength_1323c37b2b684b1193977b46a393a553_Out_2;
    surface.Alpha = _Saturate_39416b919e4d4793892c252123310234_Out_1;
    surface.AlphaClipThreshold = 0.1;
    return surface;
}

// --------------------------------------------------
// Build Graph Inputs

VertexDescriptionInputs BuildVertexDescriptionInputs(Attributes input)
{
    VertexDescriptionInputs output;
    ZERO_INITIALIZE(VertexDescriptionInputs, output);

    output.ObjectSpaceNormal = input.normalOS;
    output.ObjectSpaceTangent = input.tangentOS.xyz;
    output.ObjectSpacePosition = input.positionOS;

    return output;
}
    SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
{
    SurfaceDescriptionInputs output;
    ZERO_INITIALIZE(SurfaceDescriptionInputs, output);



    output.TangentSpaceNormal = float3(0.0f, 0.0f, 1.0f);


    output.WorldSpacePosition = input.positionWS;
    output.AbsoluteWorldSpacePosition = GetAbsolutePositionWS(input.positionWS);
    output.ScreenPosition = ComputeScreenPos(TransformWorldToHClip(input.positionWS), _ProjectionParams.x);
#if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
#define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN output.FaceSign =                    IS_FRONT_VFACE(input.cullFace, true, false);
#else
#define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
#endif
#undef BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN

    return output;
}

    // --------------------------------------------------
    // Main

    #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"
#include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/Varyings.hlsl"
#include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/DepthNormalsOnlyPass.hlsl"

    ENDHLSL
}
Pass
{
    Name "Meta"
    Tags
    {
        "LightMode" = "Meta"
    }

        // Render State
        Cull Off

        // Debug
        // <None>

        // --------------------------------------------------
        // Pass

        HLSLPROGRAM

        // Pragmas
        #pragma target 4.5
    #pragma exclude_renderers gles gles3 glcore
    #pragma vertex vert
    #pragma fragment frag

        // DotsInstancingOptions: <None>
        // HybridV1InjectedBuiltinProperties: <None>

        // Keywords
        #pragma shader_feature _ _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
        // GraphKeywords: <None>

        // Defines
        #define _SURFACE_TYPE_TRANSPARENT 1
        #define _AlphaClip 1
        #define _NORMALMAP 1
        #define _NORMAL_DROPOFF_TS 1
        #define ATTRIBUTES_NEED_NORMAL
        #define ATTRIBUTES_NEED_TANGENT
        #define ATTRIBUTES_NEED_TEXCOORD1
        #define ATTRIBUTES_NEED_TEXCOORD2
        #define VARYINGS_NEED_POSITION_WS
        #define FEATURES_GRAPH_VERTEX
        /* WARNING: $splice Could not find named fragment 'PassInstancing' */
        #define SHADERPASS SHADERPASS_META
    #define REQUIRE_DEPTH_TEXTURE
        /* WARNING: $splice Could not find named fragment 'DotsInstancingVars' */

        // Includes
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
    #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
    #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/MetaInput.hlsl"

        // --------------------------------------------------
        // Structs and Packing

        struct Attributes
    {
        float3 positionOS : POSITION;
        float3 normalOS : NORMAL;
        float4 tangentOS : TANGENT;
        float4 uv1 : TEXCOORD1;
        float4 uv2 : TEXCOORD2;
        #if UNITY_ANY_INSTANCING_ENABLED
        uint instanceID : INSTANCEID_SEMANTIC;
        #endif
    };
    struct Varyings
    {
        float4 positionCS : SV_POSITION;
        float3 positionWS;
        #if UNITY_ANY_INSTANCING_ENABLED
        uint instanceID : CUSTOM_INSTANCE_ID;
        #endif
        #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
        uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
        #endif
        #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
        uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
        #endif
        #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
        FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
        #endif
    };
    struct SurfaceDescriptionInputs
    {
        float3 WorldSpacePosition;
        float3 AbsoluteWorldSpacePosition;
        float4 ScreenPosition;
    };
    struct VertexDescriptionInputs
    {
        float3 ObjectSpaceNormal;
        float3 ObjectSpaceTangent;
        float3 ObjectSpacePosition;
    };
    struct PackedVaryings
    {
        float4 positionCS : SV_POSITION;
        float3 interp0 : TEXCOORD0;
        #if UNITY_ANY_INSTANCING_ENABLED
        uint instanceID : CUSTOM_INSTANCE_ID;
        #endif
        #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
        uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
        #endif
        #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
        uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
        #endif
        #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
        FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
        #endif
    };

        PackedVaryings PackVaryings(Varyings input)
    {
        PackedVaryings output;
        output.positionCS = input.positionCS;
        output.interp0.xyz = input.positionWS;
        #if UNITY_ANY_INSTANCING_ENABLED
        output.instanceID = input.instanceID;
        #endif
        #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
        output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
        #endif
        #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
        output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
        #endif
        #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
        output.cullFace = input.cullFace;
        #endif
        return output;
    }
    Varyings UnpackVaryings(PackedVaryings input)
    {
        Varyings output;
        output.positionCS = input.positionCS;
        output.positionWS = input.interp0.xyz;
        #if UNITY_ANY_INSTANCING_ENABLED
        output.instanceID = input.instanceID;
        #endif
        #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
        output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
        #endif
        #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
        output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
        #endif
        #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
        output.cullFace = input.cullFace;
        #endif
        return output;
    }

    // --------------------------------------------------
    // Graph

    // Graph Properties
    CBUFFER_START(UnityPerMaterial)
float4 Texture2D_818cca92a18946a3b27ffbaf1b40a062_TexelSize;
float4 Texture2D_3f2131083fc44d9b989206ab02babd86_TexelSize;
float4 Texture2D_b01d9e81c8e04473b8880e3878694e5e_TexelSize;
float Vector1_accfafba726746c885b8e27280080a86;
float Vector1_c5686f27abfe4d5081a5b5eee32afda8;
float Vector1_9ec3ae0c6d7a4e1f8b784c14876e5174;
float4 Color_dc69a74bd5704ea796654354ea94b8d9;
float Vector1_4f1d2b1f25a248f3923ab327466f2485;
CBUFFER_END

// Object and Global properties
SAMPLER(SamplerState_Linear_Repeat);
TEXTURE2D(Texture2D_818cca92a18946a3b27ffbaf1b40a062);
SAMPLER(samplerTexture2D_818cca92a18946a3b27ffbaf1b40a062);
TEXTURE2D(Texture2D_3f2131083fc44d9b989206ab02babd86);
SAMPLER(samplerTexture2D_3f2131083fc44d9b989206ab02babd86);
TEXTURE2D(Texture2D_b01d9e81c8e04473b8880e3878694e5e);
SAMPLER(samplerTexture2D_b01d9e81c8e04473b8880e3878694e5e);

// Graph Functions

void Unity_Multiply_float(float2 A, float2 B, out float2 Out)
{
    Out = A * B;
}

void Unity_Rotate_Radians_float(float2 UV, float2 Center, float Rotation, out float2 Out)
{
    //rotation matrix
    UV -= Center;
    float s = sin(Rotation);
    float c = cos(Rotation);

    //center rotation matrix
    float2x2 rMatrix = float2x2(c, -s, s, c);
    rMatrix *= 0.5;
    rMatrix += 0.5;
    rMatrix = rMatrix * 2 - 1;

    //multiply the UVs by the rotation matrix
    UV.xy = mul(UV.xy, rMatrix);
    UV += Center;

    Out = UV;
}

void Unity_Multiply_float(float4 A, float4 B, out float4 Out)
{
    Out = A * B;
}

void Unity_SceneDepth_Eye_float(float4 UV, out float Out)
{
    Out = LinearEyeDepth(SHADERGRAPH_SAMPLE_SCENE_DEPTH(UV.xy), _ZBufferParams);
}

void Unity_Subtract_float(float A, float B, out float Out)
{
    Out = A - B;
}

void Unity_Divide_float(float A, float B, out float Out)
{
    Out = A / B;
}

void Unity_Saturate_float(float In, out float Out)
{
    Out = saturate(In);
}

// Graph Vertex
struct VertexDescription
{
    float3 Position;
    float3 Normal;
    float3 Tangent;
};

VertexDescription VertexDescriptionFunction(VertexDescriptionInputs IN)
{
    VertexDescription description = (VertexDescription)0;
    description.Position = IN.ObjectSpacePosition;
    description.Normal = IN.ObjectSpaceNormal;
    description.Tangent = IN.ObjectSpaceTangent;
    return description;
}

// Graph Pixel
struct SurfaceDescription
{
    float3 BaseColor;
    float3 Emission;
    float Alpha;
    float AlphaClipThreshold;
};

SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
{
    SurfaceDescription surface = (SurfaceDescription)0;
    UnityTexture2D _Property_c13bab846ff3488187905dd48c26755a_Out_0 = UnityBuildTexture2DStructNoScale(Texture2D_818cca92a18946a3b27ffbaf1b40a062);
    float _Split_a0d1d32f695a4ceabaf3057615b92541_R_1 = IN.AbsoluteWorldSpacePosition[0];
    float _Split_a0d1d32f695a4ceabaf3057615b92541_G_2 = IN.AbsoluteWorldSpacePosition[1];
    float _Split_a0d1d32f695a4ceabaf3057615b92541_B_3 = IN.AbsoluteWorldSpacePosition[2];
    float _Split_a0d1d32f695a4ceabaf3057615b92541_A_4 = 0;
    float2 _Vector2_f84baf3fcaab45edb30625c920fb91bd_Out_0 = float2(_Split_a0d1d32f695a4ceabaf3057615b92541_R_1, _Split_a0d1d32f695a4ceabaf3057615b92541_B_3);
    float _Property_505300b000564f4fbe41a0d1fb1dd9bb_Out_0 = Vector1_c5686f27abfe4d5081a5b5eee32afda8;
    float2 _Multiply_adf8c2949a394a748dcbaf9bbecc58b5_Out_2;
    Unity_Multiply_float(_Vector2_f84baf3fcaab45edb30625c920fb91bd_Out_0, (_Property_505300b000564f4fbe41a0d1fb1dd9bb_Out_0.xx), _Multiply_adf8c2949a394a748dcbaf9bbecc58b5_Out_2);
    float2 _Vector2_6df102a229904a54a01b4e43c9924cce_Out_0 = float2(0.5, 0.5);
    float _Property_25c7c338c2a340f99397839c68ce01bf_Out_0 = Vector1_9ec3ae0c6d7a4e1f8b784c14876e5174;
    float2 _Rotate_e74144c0506c44c79764e30df1dcc15c_Out_3;
    Unity_Rotate_Radians_float(_Multiply_adf8c2949a394a748dcbaf9bbecc58b5_Out_2, _Vector2_6df102a229904a54a01b4e43c9924cce_Out_0, _Property_25c7c338c2a340f99397839c68ce01bf_Out_0, _Rotate_e74144c0506c44c79764e30df1dcc15c_Out_3);
    float4 _SampleTexture2D_6958146740004e6383c135b2c28476d4_RGBA_0 = SAMPLE_TEXTURE2D(_Property_c13bab846ff3488187905dd48c26755a_Out_0.tex, _Property_c13bab846ff3488187905dd48c26755a_Out_0.samplerstate, _Rotate_e74144c0506c44c79764e30df1dcc15c_Out_3);
    float _SampleTexture2D_6958146740004e6383c135b2c28476d4_R_4 = _SampleTexture2D_6958146740004e6383c135b2c28476d4_RGBA_0.r;
    float _SampleTexture2D_6958146740004e6383c135b2c28476d4_G_5 = _SampleTexture2D_6958146740004e6383c135b2c28476d4_RGBA_0.g;
    float _SampleTexture2D_6958146740004e6383c135b2c28476d4_B_6 = _SampleTexture2D_6958146740004e6383c135b2c28476d4_RGBA_0.b;
    float _SampleTexture2D_6958146740004e6383c135b2c28476d4_A_7 = _SampleTexture2D_6958146740004e6383c135b2c28476d4_RGBA_0.a;
    float4 _Property_13e483cd51b8490b9a44512c4f249ccb_Out_0 = Color_dc69a74bd5704ea796654354ea94b8d9;
    float4 _Multiply_cb02d0721341452692ce3355b85e3b79_Out_2;
    Unity_Multiply_float(_SampleTexture2D_6958146740004e6383c135b2c28476d4_RGBA_0, _Property_13e483cd51b8490b9a44512c4f249ccb_Out_0, _Multiply_cb02d0721341452692ce3355b85e3b79_Out_2);
    float _SceneDepth_4b924296a5444c71987b488d105a7c45_Out_1;
    Unity_SceneDepth_Eye_float(float4(IN.ScreenPosition.xy / IN.ScreenPosition.w, 0, 0), _SceneDepth_4b924296a5444c71987b488d105a7c45_Out_1);
    float4 _ScreenPosition_088c919562d7448b8f64e5e7c85bef8f_Out_0 = IN.ScreenPosition;
    float _Split_715c1865a284495abb42c53b6db4bb77_R_1 = _ScreenPosition_088c919562d7448b8f64e5e7c85bef8f_Out_0[0];
    float _Split_715c1865a284495abb42c53b6db4bb77_G_2 = _ScreenPosition_088c919562d7448b8f64e5e7c85bef8f_Out_0[1];
    float _Split_715c1865a284495abb42c53b6db4bb77_B_3 = _ScreenPosition_088c919562d7448b8f64e5e7c85bef8f_Out_0[2];
    float _Split_715c1865a284495abb42c53b6db4bb77_A_4 = _ScreenPosition_088c919562d7448b8f64e5e7c85bef8f_Out_0[3];
    float _Subtract_94c5a0cdecd3465c8ebce61a76126ae9_Out_2;
    Unity_Subtract_float(_SceneDepth_4b924296a5444c71987b488d105a7c45_Out_1, _Split_715c1865a284495abb42c53b6db4bb77_A_4, _Subtract_94c5a0cdecd3465c8ebce61a76126ae9_Out_2);
    float _Property_c2fb2ab4753f4a67b952b67243833c4a_Out_0 = Vector1_4f1d2b1f25a248f3923ab327466f2485;
    float _Divide_6b505606b9094bd6890b7e45cb0eb021_Out_2;
    Unity_Divide_float(_Subtract_94c5a0cdecd3465c8ebce61a76126ae9_Out_2, _Property_c2fb2ab4753f4a67b952b67243833c4a_Out_0, _Divide_6b505606b9094bd6890b7e45cb0eb021_Out_2);
    float _Saturate_39416b919e4d4793892c252123310234_Out_1;
    Unity_Saturate_float(_Divide_6b505606b9094bd6890b7e45cb0eb021_Out_2, _Saturate_39416b919e4d4793892c252123310234_Out_1);
    surface.BaseColor = (_Multiply_cb02d0721341452692ce3355b85e3b79_Out_2.xyz);
    surface.Emission = float3(0, 0, 0);
    surface.Alpha = _Saturate_39416b919e4d4793892c252123310234_Out_1;
    surface.AlphaClipThreshold = 0.1;
    return surface;
}

// --------------------------------------------------
// Build Graph Inputs

VertexDescriptionInputs BuildVertexDescriptionInputs(Attributes input)
{
    VertexDescriptionInputs output;
    ZERO_INITIALIZE(VertexDescriptionInputs, output);

    output.ObjectSpaceNormal = input.normalOS;
    output.ObjectSpaceTangent = input.tangentOS.xyz;
    output.ObjectSpacePosition = input.positionOS;

    return output;
}
    SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
{
    SurfaceDescriptionInputs output;
    ZERO_INITIALIZE(SurfaceDescriptionInputs, output);





    output.WorldSpacePosition = input.positionWS;
    output.AbsoluteWorldSpacePosition = GetAbsolutePositionWS(input.positionWS);
    output.ScreenPosition = ComputeScreenPos(TransformWorldToHClip(input.positionWS), _ProjectionParams.x);
#if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
#define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN output.FaceSign =                    IS_FRONT_VFACE(input.cullFace, true, false);
#else
#define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
#endif
#undef BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN

    return output;
}

    // --------------------------------------------------
    // Main

    #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"
#include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/Varyings.hlsl"
#include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/LightingMetaPass.hlsl"

    ENDHLSL
}
Pass
{
        // Name: <None>
        Tags
        {
            "LightMode" = "Universal2D"
        }

        // Render State
        Cull Back
    Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha
    ZTest LEqual
    ZWrite Off

        // Debug
        // <None>

        // --------------------------------------------------
        // Pass

        HLSLPROGRAM

        // Pragmas
        #pragma target 4.5
    #pragma exclude_renderers gles gles3 glcore
    #pragma vertex vert
    #pragma fragment frag

        // DotsInstancingOptions: <None>
        // HybridV1InjectedBuiltinProperties: <None>

        // Keywords
        // PassKeywords: <None>
        // GraphKeywords: <None>

        // Defines
        #define _SURFACE_TYPE_TRANSPARENT 1
        #define _AlphaClip 1
        #define _NORMALMAP 1
        #define _NORMAL_DROPOFF_TS 1
        #define ATTRIBUTES_NEED_NORMAL
        #define ATTRIBUTES_NEED_TANGENT
        #define VARYINGS_NEED_POSITION_WS
        #define FEATURES_GRAPH_VERTEX
        /* WARNING: $splice Could not find named fragment 'PassInstancing' */
        #define SHADERPASS SHADERPASS_2D
    #define REQUIRE_DEPTH_TEXTURE
        /* WARNING: $splice Could not find named fragment 'DotsInstancingVars' */

        // Includes
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
    #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
    #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"

        // --------------------------------------------------
        // Structs and Packing

        struct Attributes
    {
        float3 positionOS : POSITION;
        float3 normalOS : NORMAL;
        float4 tangentOS : TANGENT;
        #if UNITY_ANY_INSTANCING_ENABLED
        uint instanceID : INSTANCEID_SEMANTIC;
        #endif
    };
    struct Varyings
    {
        float4 positionCS : SV_POSITION;
        float3 positionWS;
        #if UNITY_ANY_INSTANCING_ENABLED
        uint instanceID : CUSTOM_INSTANCE_ID;
        #endif
        #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
        uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
        #endif
        #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
        uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
        #endif
        #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
        FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
        #endif
    };
    struct SurfaceDescriptionInputs
    {
        float3 WorldSpacePosition;
        float3 AbsoluteWorldSpacePosition;
        float4 ScreenPosition;
    };
    struct VertexDescriptionInputs
    {
        float3 ObjectSpaceNormal;
        float3 ObjectSpaceTangent;
        float3 ObjectSpacePosition;
    };
    struct PackedVaryings
    {
        float4 positionCS : SV_POSITION;
        float3 interp0 : TEXCOORD0;
        #if UNITY_ANY_INSTANCING_ENABLED
        uint instanceID : CUSTOM_INSTANCE_ID;
        #endif
        #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
        uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
        #endif
        #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
        uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
        #endif
        #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
        FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
        #endif
    };

        PackedVaryings PackVaryings(Varyings input)
    {
        PackedVaryings output;
        output.positionCS = input.positionCS;
        output.interp0.xyz = input.positionWS;
        #if UNITY_ANY_INSTANCING_ENABLED
        output.instanceID = input.instanceID;
        #endif
        #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
        output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
        #endif
        #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
        output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
        #endif
        #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
        output.cullFace = input.cullFace;
        #endif
        return output;
    }
    Varyings UnpackVaryings(PackedVaryings input)
    {
        Varyings output;
        output.positionCS = input.positionCS;
        output.positionWS = input.interp0.xyz;
        #if UNITY_ANY_INSTANCING_ENABLED
        output.instanceID = input.instanceID;
        #endif
        #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
        output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
        #endif
        #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
        output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
        #endif
        #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
        output.cullFace = input.cullFace;
        #endif
        return output;
    }

    // --------------------------------------------------
    // Graph

    // Graph Properties
    CBUFFER_START(UnityPerMaterial)
float4 Texture2D_818cca92a18946a3b27ffbaf1b40a062_TexelSize;
float4 Texture2D_3f2131083fc44d9b989206ab02babd86_TexelSize;
float4 Texture2D_b01d9e81c8e04473b8880e3878694e5e_TexelSize;
float Vector1_accfafba726746c885b8e27280080a86;
float Vector1_c5686f27abfe4d5081a5b5eee32afda8;
float Vector1_9ec3ae0c6d7a4e1f8b784c14876e5174;
float4 Color_dc69a74bd5704ea796654354ea94b8d9;
float Vector1_4f1d2b1f25a248f3923ab327466f2485;
CBUFFER_END

// Object and Global properties
SAMPLER(SamplerState_Linear_Repeat);
TEXTURE2D(Texture2D_818cca92a18946a3b27ffbaf1b40a062);
SAMPLER(samplerTexture2D_818cca92a18946a3b27ffbaf1b40a062);
TEXTURE2D(Texture2D_3f2131083fc44d9b989206ab02babd86);
SAMPLER(samplerTexture2D_3f2131083fc44d9b989206ab02babd86);
TEXTURE2D(Texture2D_b01d9e81c8e04473b8880e3878694e5e);
SAMPLER(samplerTexture2D_b01d9e81c8e04473b8880e3878694e5e);

// Graph Functions

void Unity_Multiply_float(float2 A, float2 B, out float2 Out)
{
    Out = A * B;
}

void Unity_Rotate_Radians_float(float2 UV, float2 Center, float Rotation, out float2 Out)
{
    //rotation matrix
    UV -= Center;
    float s = sin(Rotation);
    float c = cos(Rotation);

    //center rotation matrix
    float2x2 rMatrix = float2x2(c, -s, s, c);
    rMatrix *= 0.5;
    rMatrix += 0.5;
    rMatrix = rMatrix * 2 - 1;

    //multiply the UVs by the rotation matrix
    UV.xy = mul(UV.xy, rMatrix);
    UV += Center;

    Out = UV;
}

void Unity_Multiply_float(float4 A, float4 B, out float4 Out)
{
    Out = A * B;
}

void Unity_SceneDepth_Eye_float(float4 UV, out float Out)
{
    Out = LinearEyeDepth(SHADERGRAPH_SAMPLE_SCENE_DEPTH(UV.xy), _ZBufferParams);
}

void Unity_Subtract_float(float A, float B, out float Out)
{
    Out = A - B;
}

void Unity_Divide_float(float A, float B, out float Out)
{
    Out = A / B;
}

void Unity_Saturate_float(float In, out float Out)
{
    Out = saturate(In);
}

// Graph Vertex
struct VertexDescription
{
    float3 Position;
    float3 Normal;
    float3 Tangent;
};

VertexDescription VertexDescriptionFunction(VertexDescriptionInputs IN)
{
    VertexDescription description = (VertexDescription)0;
    description.Position = IN.ObjectSpacePosition;
    description.Normal = IN.ObjectSpaceNormal;
    description.Tangent = IN.ObjectSpaceTangent;
    return description;
}

// Graph Pixel
struct SurfaceDescription
{
    float3 BaseColor;
    float Alpha;
    float AlphaClipThreshold;
};

SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
{
    SurfaceDescription surface = (SurfaceDescription)0;
    UnityTexture2D _Property_c13bab846ff3488187905dd48c26755a_Out_0 = UnityBuildTexture2DStructNoScale(Texture2D_818cca92a18946a3b27ffbaf1b40a062);
    float _Split_a0d1d32f695a4ceabaf3057615b92541_R_1 = IN.AbsoluteWorldSpacePosition[0];
    float _Split_a0d1d32f695a4ceabaf3057615b92541_G_2 = IN.AbsoluteWorldSpacePosition[1];
    float _Split_a0d1d32f695a4ceabaf3057615b92541_B_3 = IN.AbsoluteWorldSpacePosition[2];
    float _Split_a0d1d32f695a4ceabaf3057615b92541_A_4 = 0;
    float2 _Vector2_f84baf3fcaab45edb30625c920fb91bd_Out_0 = float2(_Split_a0d1d32f695a4ceabaf3057615b92541_R_1, _Split_a0d1d32f695a4ceabaf3057615b92541_B_3);
    float _Property_505300b000564f4fbe41a0d1fb1dd9bb_Out_0 = Vector1_c5686f27abfe4d5081a5b5eee32afda8;
    float2 _Multiply_adf8c2949a394a748dcbaf9bbecc58b5_Out_2;
    Unity_Multiply_float(_Vector2_f84baf3fcaab45edb30625c920fb91bd_Out_0, (_Property_505300b000564f4fbe41a0d1fb1dd9bb_Out_0.xx), _Multiply_adf8c2949a394a748dcbaf9bbecc58b5_Out_2);
    float2 _Vector2_6df102a229904a54a01b4e43c9924cce_Out_0 = float2(0.5, 0.5);
    float _Property_25c7c338c2a340f99397839c68ce01bf_Out_0 = Vector1_9ec3ae0c6d7a4e1f8b784c14876e5174;
    float2 _Rotate_e74144c0506c44c79764e30df1dcc15c_Out_3;
    Unity_Rotate_Radians_float(_Multiply_adf8c2949a394a748dcbaf9bbecc58b5_Out_2, _Vector2_6df102a229904a54a01b4e43c9924cce_Out_0, _Property_25c7c338c2a340f99397839c68ce01bf_Out_0, _Rotate_e74144c0506c44c79764e30df1dcc15c_Out_3);
    float4 _SampleTexture2D_6958146740004e6383c135b2c28476d4_RGBA_0 = SAMPLE_TEXTURE2D(_Property_c13bab846ff3488187905dd48c26755a_Out_0.tex, _Property_c13bab846ff3488187905dd48c26755a_Out_0.samplerstate, _Rotate_e74144c0506c44c79764e30df1dcc15c_Out_3);
    float _SampleTexture2D_6958146740004e6383c135b2c28476d4_R_4 = _SampleTexture2D_6958146740004e6383c135b2c28476d4_RGBA_0.r;
    float _SampleTexture2D_6958146740004e6383c135b2c28476d4_G_5 = _SampleTexture2D_6958146740004e6383c135b2c28476d4_RGBA_0.g;
    float _SampleTexture2D_6958146740004e6383c135b2c28476d4_B_6 = _SampleTexture2D_6958146740004e6383c135b2c28476d4_RGBA_0.b;
    float _SampleTexture2D_6958146740004e6383c135b2c28476d4_A_7 = _SampleTexture2D_6958146740004e6383c135b2c28476d4_RGBA_0.a;
    float4 _Property_13e483cd51b8490b9a44512c4f249ccb_Out_0 = Color_dc69a74bd5704ea796654354ea94b8d9;
    float4 _Multiply_cb02d0721341452692ce3355b85e3b79_Out_2;
    Unity_Multiply_float(_SampleTexture2D_6958146740004e6383c135b2c28476d4_RGBA_0, _Property_13e483cd51b8490b9a44512c4f249ccb_Out_0, _Multiply_cb02d0721341452692ce3355b85e3b79_Out_2);
    float _SceneDepth_4b924296a5444c71987b488d105a7c45_Out_1;
    Unity_SceneDepth_Eye_float(float4(IN.ScreenPosition.xy / IN.ScreenPosition.w, 0, 0), _SceneDepth_4b924296a5444c71987b488d105a7c45_Out_1);
    float4 _ScreenPosition_088c919562d7448b8f64e5e7c85bef8f_Out_0 = IN.ScreenPosition;
    float _Split_715c1865a284495abb42c53b6db4bb77_R_1 = _ScreenPosition_088c919562d7448b8f64e5e7c85bef8f_Out_0[0];
    float _Split_715c1865a284495abb42c53b6db4bb77_G_2 = _ScreenPosition_088c919562d7448b8f64e5e7c85bef8f_Out_0[1];
    float _Split_715c1865a284495abb42c53b6db4bb77_B_3 = _ScreenPosition_088c919562d7448b8f64e5e7c85bef8f_Out_0[2];
    float _Split_715c1865a284495abb42c53b6db4bb77_A_4 = _ScreenPosition_088c919562d7448b8f64e5e7c85bef8f_Out_0[3];
    float _Subtract_94c5a0cdecd3465c8ebce61a76126ae9_Out_2;
    Unity_Subtract_float(_SceneDepth_4b924296a5444c71987b488d105a7c45_Out_1, _Split_715c1865a284495abb42c53b6db4bb77_A_4, _Subtract_94c5a0cdecd3465c8ebce61a76126ae9_Out_2);
    float _Property_c2fb2ab4753f4a67b952b67243833c4a_Out_0 = Vector1_4f1d2b1f25a248f3923ab327466f2485;
    float _Divide_6b505606b9094bd6890b7e45cb0eb021_Out_2;
    Unity_Divide_float(_Subtract_94c5a0cdecd3465c8ebce61a76126ae9_Out_2, _Property_c2fb2ab4753f4a67b952b67243833c4a_Out_0, _Divide_6b505606b9094bd6890b7e45cb0eb021_Out_2);
    float _Saturate_39416b919e4d4793892c252123310234_Out_1;
    Unity_Saturate_float(_Divide_6b505606b9094bd6890b7e45cb0eb021_Out_2, _Saturate_39416b919e4d4793892c252123310234_Out_1);
    surface.BaseColor = (_Multiply_cb02d0721341452692ce3355b85e3b79_Out_2.xyz);
    surface.Alpha = _Saturate_39416b919e4d4793892c252123310234_Out_1;
    surface.AlphaClipThreshold = 0.1;
    return surface;
}

// --------------------------------------------------
// Build Graph Inputs

VertexDescriptionInputs BuildVertexDescriptionInputs(Attributes input)
{
    VertexDescriptionInputs output;
    ZERO_INITIALIZE(VertexDescriptionInputs, output);

    output.ObjectSpaceNormal = input.normalOS;
    output.ObjectSpaceTangent = input.tangentOS.xyz;
    output.ObjectSpacePosition = input.positionOS;

    return output;
}
    SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
{
    SurfaceDescriptionInputs output;
    ZERO_INITIALIZE(SurfaceDescriptionInputs, output);





    output.WorldSpacePosition = input.positionWS;
    output.AbsoluteWorldSpacePosition = GetAbsolutePositionWS(input.positionWS);
    output.ScreenPosition = ComputeScreenPos(TransformWorldToHClip(input.positionWS), _ProjectionParams.x);
#if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
#define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN output.FaceSign =                    IS_FRONT_VFACE(input.cullFace, true, false);
#else
#define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
#endif
#undef BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN

    return output;
}

    // --------------------------------------------------
    // Main

    #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"
#include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/Varyings.hlsl"
#include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/PBR2DPass.hlsl"

    ENDHLSL
}
Pass
{
    Name "MotionVectors"
    Tags
    {
        "LightMode" = "MotionVectors"
    }

        // Render State
        Cull Back
    Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha
    ZTest LEqual
    ZWrite Off

        // Debug
        // <None>

        // --------------------------------------------------
        // Pass

        HLSLPROGRAM

        // Pragmas
        #pragma target 4.5
    #pragma exclude_renderers gles gles3 glcore
    #pragma multi_compile_instancing
    #pragma multi_compile _ DOTS_INSTANCING_ON
    #pragma vertex vert
    #pragma fragment frag

        // DotsInstancingOptions: <None>
        // HybridV1InjectedBuiltinProperties: <None>

        // Keywords
        // PassKeywords: <None>
        // GraphKeywords: <None>

        // Defines
        #define _SURFACE_TYPE_TRANSPARENT 1
        #define _AlphaClip 1
        #define _NORMALMAP 1
        #define _NORMAL_DROPOFF_TS 1
        #define ATTRIBUTES_NEED_NORMAL
        #define ATTRIBUTES_NEED_TANGENT
        #define ATTRIBUTES_NEED_TEXCOORD4
        #define VARYINGS_NEED_CURRENT_POSITION_CS
        #define VARYINGS_NEED_PREVIOUS_POSITION_CS
        #define FEATURES_GRAPH_VERTEX
        /* WARNING: $splice Could not find named fragment 'PassInstancing' */
        #define SHADERPASS SHADERPASS_MOTIONVECTORS
        /* WARNING: $splice Could not find named fragment 'DotsInstancingVars' */

        // Includes
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
    #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
    #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"

        // --------------------------------------------------
        // Structs and Packing

        struct Attributes
    {
        float3 positionOS : POSITION;
        float3 normalOS : NORMAL;
        float4 tangentOS : TANGENT;
        float4 uv4 : TEXCOORD4;
        #if UNITY_ANY_INSTANCING_ENABLED
        uint instanceID : INSTANCEID_SEMANTIC;
        #endif
    };
    struct Varyings
    {
        float4 positionCS : SV_POSITION;
        float4 curPositionCS;
        float4 prevPositionCS;
        #if UNITY_ANY_INSTANCING_ENABLED
        uint instanceID : CUSTOM_INSTANCE_ID;
        #endif
        #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
        uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
        #endif
        #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
        uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
        #endif
        #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
        FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
        #endif
    };
    struct SurfaceDescriptionInputs
    {
    };
    struct VertexDescriptionInputs
    {
        float3 ObjectSpaceNormal;
        float3 ObjectSpaceTangent;
        float3 ObjectSpacePosition;
    };
    struct PackedVaryings
    {
        float4 positionCS : SV_POSITION;
        float4 interp0 : TEXCOORD0;
        float4 interp1 : TEXCOORD1;
        #if UNITY_ANY_INSTANCING_ENABLED
        uint instanceID : CUSTOM_INSTANCE_ID;
        #endif
        #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
        uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
        #endif
        #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
        uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
        #endif
        #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
        FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
        #endif
    };

        PackedVaryings PackVaryings(Varyings input)
    {
        PackedVaryings output;
        output.positionCS = input.positionCS;
        output.interp0.xyzw = input.curPositionCS;
        output.interp1.xyzw = input.prevPositionCS;
        #if UNITY_ANY_INSTANCING_ENABLED
        output.instanceID = input.instanceID;
        #endif
        #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
        output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
        #endif
        #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
        output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
        #endif
        #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
        output.cullFace = input.cullFace;
        #endif
        return output;
    }
    Varyings UnpackVaryings(PackedVaryings input)
    {
        Varyings output;
        output.positionCS = input.positionCS;
        output.curPositionCS = input.interp0.xyzw;
        output.prevPositionCS = input.interp1.xyzw;
        #if UNITY_ANY_INSTANCING_ENABLED
        output.instanceID = input.instanceID;
        #endif
        #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
        output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
        #endif
        #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
        output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
        #endif
        #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
        output.cullFace = input.cullFace;
        #endif
        return output;
    }

    // --------------------------------------------------
    // Graph

    // Graph Properties
    CBUFFER_START(UnityPerMaterial)
float4 Texture2D_818cca92a18946a3b27ffbaf1b40a062_TexelSize;
float4 Texture2D_3f2131083fc44d9b989206ab02babd86_TexelSize;
float4 Texture2D_b01d9e81c8e04473b8880e3878694e5e_TexelSize;
float Vector1_accfafba726746c885b8e27280080a86;
float Vector1_c5686f27abfe4d5081a5b5eee32afda8;
float Vector1_9ec3ae0c6d7a4e1f8b784c14876e5174;
float4 Color_dc69a74bd5704ea796654354ea94b8d9;
float Vector1_4f1d2b1f25a248f3923ab327466f2485;
CBUFFER_END

// Object and Global properties
SAMPLER(SamplerState_Linear_Repeat);
TEXTURE2D(Texture2D_818cca92a18946a3b27ffbaf1b40a062);
SAMPLER(samplerTexture2D_818cca92a18946a3b27ffbaf1b40a062);
TEXTURE2D(Texture2D_3f2131083fc44d9b989206ab02babd86);
SAMPLER(samplerTexture2D_3f2131083fc44d9b989206ab02babd86);
TEXTURE2D(Texture2D_b01d9e81c8e04473b8880e3878694e5e);
SAMPLER(samplerTexture2D_b01d9e81c8e04473b8880e3878694e5e);

// Graph Functions
// GraphFunctions: <None>

// Graph Vertex
struct VertexDescription
{
    float3 Position;
    float3 Normal;
    float3 Tangent;
};

VertexDescription VertexDescriptionFunction(VertexDescriptionInputs IN)
{
    VertexDescription description = (VertexDescription)0;
    description.Position = IN.ObjectSpacePosition;
    description.Normal = IN.ObjectSpaceNormal;
    description.Tangent = IN.ObjectSpaceTangent;
    return description;
}

// Graph Pixel
struct SurfaceDescription
{
};

SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
{
    SurfaceDescription surface = (SurfaceDescription)0;
    return surface;
}

// --------------------------------------------------
// Build Graph Inputs

VertexDescriptionInputs BuildVertexDescriptionInputs(Attributes input)
{
    VertexDescriptionInputs output;
    ZERO_INITIALIZE(VertexDescriptionInputs, output);

    output.ObjectSpaceNormal = input.normalOS;
    output.ObjectSpaceTangent = input.tangentOS.xyz;
    output.ObjectSpacePosition = input.positionOS;

    return output;
}
    SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
{
    SurfaceDescriptionInputs output;
    ZERO_INITIALIZE(SurfaceDescriptionInputs, output);





#if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
#define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN output.FaceSign =                    IS_FRONT_VFACE(input.cullFace, true, false);
#else
#define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
#endif
#undef BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN

    return output;
}

    // --------------------------------------------------
    // Main

    #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"
#include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/Varyings.hlsl"
#include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/OculusMotionVectors.hlsl"

    ENDHLSL
}
    }
        SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
            "RenderType" = "Transparent"
            "UniversalMaterialType" = "Lit"
            "Queue" = "Transparent"
        }
        Pass
        {
            Name "Universal Forward"
            Tags
            {
                "LightMode" = "UniversalForward"
            }

        // Render State
        Cull Back
    Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha
    ZTest LEqual
    ZWrite On

        // Debug
        // <None>

        // --------------------------------------------------
        // Pass

        HLSLPROGRAM

        // Pragmas
        #pragma target 2.0
    #pragma only_renderers gles gles3 glcore d3d11
    #pragma multi_compile_instancing
    #pragma multi_compile_fog
    #pragma vertex vert
    #pragma fragment frag

        // DotsInstancingOptions: <None>
        // HybridV1InjectedBuiltinProperties: <None>

        // Keywords
        #pragma multi_compile _ _SCREEN_SPACE_OCCLUSION
    #pragma multi_compile _ LIGHTMAP_ON
    #pragma multi_compile _ DIRLIGHTMAP_COMBINED
    #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
    #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
    #pragma multi_compile _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS _ADDITIONAL_OFF
    #pragma multi_compile _ _ADDITIONAL_LIGHT_SHADOWS
    #pragma multi_compile _ _SHADOWS_SOFT
    #pragma multi_compile _ LIGHTMAP_SHADOW_MIXING
    #pragma multi_compile _ SHADOWS_SHADOWMASK
        // GraphKeywords: <None>

        // Defines
        #define _SURFACE_TYPE_TRANSPARENT 1
        #define _AlphaClip 1
        #define _NORMALMAP 1
        #define _NORMAL_DROPOFF_TS 1
        #define ATTRIBUTES_NEED_NORMAL
        #define ATTRIBUTES_NEED_TANGENT
        #define ATTRIBUTES_NEED_TEXCOORD1
        #define VARYINGS_NEED_POSITION_WS
        #define VARYINGS_NEED_NORMAL_WS
        #define VARYINGS_NEED_TANGENT_WS
        #define VARYINGS_NEED_VIEWDIRECTION_WS
        #define VARYINGS_NEED_FOG_AND_VERTEX_LIGHT
        #define FEATURES_GRAPH_VERTEX
        /* WARNING: $splice Could not find named fragment 'PassInstancing' */
        #define SHADERPASS SHADERPASS_FORWARD
    #define REQUIRE_DEPTH_TEXTURE
        /* WARNING: $splice Could not find named fragment 'DotsInstancingVars' */

        // Includes
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
    #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
    #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"

        // --------------------------------------------------
        // Structs and Packing

        struct Attributes
    {
        float3 positionOS : POSITION;
        float3 normalOS : NORMAL;
        float4 tangentOS : TANGENT;
        float4 uv1 : TEXCOORD1;
        #if UNITY_ANY_INSTANCING_ENABLED
        uint instanceID : INSTANCEID_SEMANTIC;
        #endif
    };
    struct Varyings
    {
        float4 positionCS : SV_POSITION;
        float3 positionWS;
        float3 normalWS;
        float4 tangentWS;
        float3 viewDirectionWS;
        #if defined(LIGHTMAP_ON)
        float2 lightmapUV;
        #endif
        #if !defined(LIGHTMAP_ON)
        float3 sh;
        #endif
        float4 fogFactorAndVertexLight;
        float4 shadowCoord;
        #if UNITY_ANY_INSTANCING_ENABLED
        uint instanceID : CUSTOM_INSTANCE_ID;
        #endif
        #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
        uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
        #endif
        #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
        uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
        #endif
        #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
        FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
        #endif
    };
    struct SurfaceDescriptionInputs
    {
        float3 TangentSpaceNormal;
        float3 WorldSpacePosition;
        float3 AbsoluteWorldSpacePosition;
        float4 ScreenPosition;
    };
    struct VertexDescriptionInputs
    {
        float3 ObjectSpaceNormal;
        float3 ObjectSpaceTangent;
        float3 ObjectSpacePosition;
    };
    struct PackedVaryings
    {
        float4 positionCS : SV_POSITION;
        float3 interp0 : TEXCOORD0;
        float3 interp1 : TEXCOORD1;
        float4 interp2 : TEXCOORD2;
        float3 interp3 : TEXCOORD3;
        #if defined(LIGHTMAP_ON)
        float2 interp4 : TEXCOORD4;
        #endif
        #if !defined(LIGHTMAP_ON)
        float3 interp5 : TEXCOORD5;
        #endif
        float4 interp6 : TEXCOORD6;
        float4 interp7 : TEXCOORD7;
        #if UNITY_ANY_INSTANCING_ENABLED
        uint instanceID : CUSTOM_INSTANCE_ID;
        #endif
        #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
        uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
        #endif
        #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
        uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
        #endif
        #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
        FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
        #endif
    };

        PackedVaryings PackVaryings(Varyings input)
    {
        PackedVaryings output;
        output.positionCS = input.positionCS;
        output.interp0.xyz = input.positionWS;
        output.interp1.xyz = input.normalWS;
        output.interp2.xyzw = input.tangentWS;
        output.interp3.xyz = input.viewDirectionWS;
        #if defined(LIGHTMAP_ON)
        output.interp4.xy = input.lightmapUV;
        #endif
        #if !defined(LIGHTMAP_ON)
        output.interp5.xyz = input.sh;
        #endif
        output.interp6.xyzw = input.fogFactorAndVertexLight;
        output.interp7.xyzw = input.shadowCoord;
        #if UNITY_ANY_INSTANCING_ENABLED
        output.instanceID = input.instanceID;
        #endif
        #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
        output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
        #endif
        #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
        output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
        #endif
        #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
        output.cullFace = input.cullFace;
        #endif
        return output;
    }
    Varyings UnpackVaryings(PackedVaryings input)
    {
        Varyings output;
        output.positionCS = input.positionCS;
        output.positionWS = input.interp0.xyz;
        output.normalWS = input.interp1.xyz;
        output.tangentWS = input.interp2.xyzw;
        output.viewDirectionWS = input.interp3.xyz;
        #if defined(LIGHTMAP_ON)
        output.lightmapUV = input.interp4.xy;
        #endif
        #if !defined(LIGHTMAP_ON)
        output.sh = input.interp5.xyz;
        #endif
        output.fogFactorAndVertexLight = input.interp6.xyzw;
        output.shadowCoord = input.interp7.xyzw;
        #if UNITY_ANY_INSTANCING_ENABLED
        output.instanceID = input.instanceID;
        #endif
        #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
        output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
        #endif
        #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
        output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
        #endif
        #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
        output.cullFace = input.cullFace;
        #endif
        return output;
    }

    // --------------------------------------------------
    // Graph

    // Graph Properties
    CBUFFER_START(UnityPerMaterial)
float4 Texture2D_818cca92a18946a3b27ffbaf1b40a062_TexelSize;
float4 Texture2D_3f2131083fc44d9b989206ab02babd86_TexelSize;
float4 Texture2D_b01d9e81c8e04473b8880e3878694e5e_TexelSize;
float Vector1_accfafba726746c885b8e27280080a86;
float Vector1_c5686f27abfe4d5081a5b5eee32afda8;
float Vector1_9ec3ae0c6d7a4e1f8b784c14876e5174;
float4 Color_dc69a74bd5704ea796654354ea94b8d9;
float Vector1_4f1d2b1f25a248f3923ab327466f2485;
CBUFFER_END

// Object and Global properties
SAMPLER(SamplerState_Linear_Repeat);
TEXTURE2D(Texture2D_818cca92a18946a3b27ffbaf1b40a062);
SAMPLER(samplerTexture2D_818cca92a18946a3b27ffbaf1b40a062);
TEXTURE2D(Texture2D_3f2131083fc44d9b989206ab02babd86);
SAMPLER(samplerTexture2D_3f2131083fc44d9b989206ab02babd86);
TEXTURE2D(Texture2D_b01d9e81c8e04473b8880e3878694e5e);
SAMPLER(samplerTexture2D_b01d9e81c8e04473b8880e3878694e5e);

// Graph Functions

void Unity_Multiply_float(float2 A, float2 B, out float2 Out)
{
    Out = A * B;
}

void Unity_Rotate_Radians_float(float2 UV, float2 Center, float Rotation, out float2 Out)
{
    //rotation matrix
    UV -= Center;
    float s = sin(Rotation);
    float c = cos(Rotation);

    //center rotation matrix
    float2x2 rMatrix = float2x2(c, -s, s, c);
    rMatrix *= 0.5;
    rMatrix += 0.5;
    rMatrix = rMatrix * 2 - 1;

    //multiply the UVs by the rotation matrix
    UV.xy = mul(UV.xy, rMatrix);
    UV += Center;

    Out = UV;
}

void Unity_Multiply_float(float4 A, float4 B, out float4 Out)
{
    Out = A * B;
}

void Unity_NormalStrength_float(float3 In, float Strength, out float3 Out)
{
    Out = float3(In.rg * Strength, lerp(1, In.b, saturate(Strength)));
}

void Unity_SceneDepth_Eye_float(float4 UV, out float Out)
{
    Out = LinearEyeDepth(SHADERGRAPH_SAMPLE_SCENE_DEPTH(UV.xy), _ZBufferParams);
}

void Unity_Subtract_float(float A, float B, out float Out)
{
    Out = A - B;
}

void Unity_Divide_float(float A, float B, out float Out)
{
    Out = A / B;
}

void Unity_Saturate_float(float In, out float Out)
{
    Out = saturate(In);
}

// Graph Vertex
struct VertexDescription
{
    float3 Position;
    float3 Normal;
    float3 Tangent;
};

VertexDescription VertexDescriptionFunction(VertexDescriptionInputs IN)
{
    VertexDescription description = (VertexDescription)0;
    description.Position = IN.ObjectSpacePosition;
    description.Normal = IN.ObjectSpaceNormal;
    description.Tangent = IN.ObjectSpaceTangent;
    return description;
}

// Graph Pixel
struct SurfaceDescription
{
    float3 BaseColor;
    float3 NormalTS;
    float3 Emission;
    float Metallic;
    float Smoothness;
    float Occlusion;
    float Alpha;
    float AlphaClipThreshold;
};

SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
{
    SurfaceDescription surface = (SurfaceDescription)0;
    UnityTexture2D _Property_c13bab846ff3488187905dd48c26755a_Out_0 = UnityBuildTexture2DStructNoScale(Texture2D_818cca92a18946a3b27ffbaf1b40a062);
    float _Split_a0d1d32f695a4ceabaf3057615b92541_R_1 = IN.AbsoluteWorldSpacePosition[0];
    float _Split_a0d1d32f695a4ceabaf3057615b92541_G_2 = IN.AbsoluteWorldSpacePosition[1];
    float _Split_a0d1d32f695a4ceabaf3057615b92541_B_3 = IN.AbsoluteWorldSpacePosition[2];
    float _Split_a0d1d32f695a4ceabaf3057615b92541_A_4 = 0;
    float2 _Vector2_f84baf3fcaab45edb30625c920fb91bd_Out_0 = float2(_Split_a0d1d32f695a4ceabaf3057615b92541_R_1, _Split_a0d1d32f695a4ceabaf3057615b92541_B_3);
    float _Property_505300b000564f4fbe41a0d1fb1dd9bb_Out_0 = Vector1_c5686f27abfe4d5081a5b5eee32afda8;
    float2 _Multiply_adf8c2949a394a748dcbaf9bbecc58b5_Out_2;
    Unity_Multiply_float(_Vector2_f84baf3fcaab45edb30625c920fb91bd_Out_0, (_Property_505300b000564f4fbe41a0d1fb1dd9bb_Out_0.xx), _Multiply_adf8c2949a394a748dcbaf9bbecc58b5_Out_2);
    float2 _Vector2_6df102a229904a54a01b4e43c9924cce_Out_0 = float2(0.5, 0.5);
    float _Property_25c7c338c2a340f99397839c68ce01bf_Out_0 = Vector1_9ec3ae0c6d7a4e1f8b784c14876e5174;
    float2 _Rotate_e74144c0506c44c79764e30df1dcc15c_Out_3;
    Unity_Rotate_Radians_float(_Multiply_adf8c2949a394a748dcbaf9bbecc58b5_Out_2, _Vector2_6df102a229904a54a01b4e43c9924cce_Out_0, _Property_25c7c338c2a340f99397839c68ce01bf_Out_0, _Rotate_e74144c0506c44c79764e30df1dcc15c_Out_3);
    float4 _SampleTexture2D_6958146740004e6383c135b2c28476d4_RGBA_0 = SAMPLE_TEXTURE2D(_Property_c13bab846ff3488187905dd48c26755a_Out_0.tex, _Property_c13bab846ff3488187905dd48c26755a_Out_0.samplerstate, _Rotate_e74144c0506c44c79764e30df1dcc15c_Out_3);
    float _SampleTexture2D_6958146740004e6383c135b2c28476d4_R_4 = _SampleTexture2D_6958146740004e6383c135b2c28476d4_RGBA_0.r;
    float _SampleTexture2D_6958146740004e6383c135b2c28476d4_G_5 = _SampleTexture2D_6958146740004e6383c135b2c28476d4_RGBA_0.g;
    float _SampleTexture2D_6958146740004e6383c135b2c28476d4_B_6 = _SampleTexture2D_6958146740004e6383c135b2c28476d4_RGBA_0.b;
    float _SampleTexture2D_6958146740004e6383c135b2c28476d4_A_7 = _SampleTexture2D_6958146740004e6383c135b2c28476d4_RGBA_0.a;
    float4 _Property_13e483cd51b8490b9a44512c4f249ccb_Out_0 = Color_dc69a74bd5704ea796654354ea94b8d9;
    float4 _Multiply_cb02d0721341452692ce3355b85e3b79_Out_2;
    Unity_Multiply_float(_SampleTexture2D_6958146740004e6383c135b2c28476d4_RGBA_0, _Property_13e483cd51b8490b9a44512c4f249ccb_Out_0, _Multiply_cb02d0721341452692ce3355b85e3b79_Out_2);
    UnityTexture2D _Property_a6c0ed483ac044e08dbc27cbaab9842b_Out_0 = UnityBuildTexture2DStructNoScale(Texture2D_b01d9e81c8e04473b8880e3878694e5e);
    float4 _SampleTexture2D_b451781d16ed4f80b11c1edbdc0145a1_RGBA_0 = SAMPLE_TEXTURE2D(_Property_a6c0ed483ac044e08dbc27cbaab9842b_Out_0.tex, _Property_a6c0ed483ac044e08dbc27cbaab9842b_Out_0.samplerstate, _Rotate_e74144c0506c44c79764e30df1dcc15c_Out_3);
    _SampleTexture2D_b451781d16ed4f80b11c1edbdc0145a1_RGBA_0.rgb = UnpackNormal(_SampleTexture2D_b451781d16ed4f80b11c1edbdc0145a1_RGBA_0);
    float _SampleTexture2D_b451781d16ed4f80b11c1edbdc0145a1_R_4 = _SampleTexture2D_b451781d16ed4f80b11c1edbdc0145a1_RGBA_0.r;
    float _SampleTexture2D_b451781d16ed4f80b11c1edbdc0145a1_G_5 = _SampleTexture2D_b451781d16ed4f80b11c1edbdc0145a1_RGBA_0.g;
    float _SampleTexture2D_b451781d16ed4f80b11c1edbdc0145a1_B_6 = _SampleTexture2D_b451781d16ed4f80b11c1edbdc0145a1_RGBA_0.b;
    float _SampleTexture2D_b451781d16ed4f80b11c1edbdc0145a1_A_7 = _SampleTexture2D_b451781d16ed4f80b11c1edbdc0145a1_RGBA_0.a;
    float _Property_99d8f29d83c44a4ba32c6eabbbc92ab2_Out_0 = Vector1_accfafba726746c885b8e27280080a86;
    float3 _NormalStrength_1323c37b2b684b1193977b46a393a553_Out_2;
    Unity_NormalStrength_float((_SampleTexture2D_b451781d16ed4f80b11c1edbdc0145a1_RGBA_0.xyz), _Property_99d8f29d83c44a4ba32c6eabbbc92ab2_Out_0, _NormalStrength_1323c37b2b684b1193977b46a393a553_Out_2);
    UnityTexture2D _Property_8d4bdc48c17846329687d49c0a59d87d_Out_0 = UnityBuildTexture2DStructNoScale(Texture2D_3f2131083fc44d9b989206ab02babd86);
    float4 _SampleTexture2D_056131d47da244f48c8ea7a72bb86401_RGBA_0 = SAMPLE_TEXTURE2D(_Property_8d4bdc48c17846329687d49c0a59d87d_Out_0.tex, _Property_8d4bdc48c17846329687d49c0a59d87d_Out_0.samplerstate, _Rotate_e74144c0506c44c79764e30df1dcc15c_Out_3);
    float _SampleTexture2D_056131d47da244f48c8ea7a72bb86401_R_4 = _SampleTexture2D_056131d47da244f48c8ea7a72bb86401_RGBA_0.r;
    float _SampleTexture2D_056131d47da244f48c8ea7a72bb86401_G_5 = _SampleTexture2D_056131d47da244f48c8ea7a72bb86401_RGBA_0.g;
    float _SampleTexture2D_056131d47da244f48c8ea7a72bb86401_B_6 = _SampleTexture2D_056131d47da244f48c8ea7a72bb86401_RGBA_0.b;
    float _SampleTexture2D_056131d47da244f48c8ea7a72bb86401_A_7 = _SampleTexture2D_056131d47da244f48c8ea7a72bb86401_RGBA_0.a;
    float _SceneDepth_4b924296a5444c71987b488d105a7c45_Out_1;
    Unity_SceneDepth_Eye_float(float4(IN.ScreenPosition.xy / IN.ScreenPosition.w, 0, 0), _SceneDepth_4b924296a5444c71987b488d105a7c45_Out_1);
    float4 _ScreenPosition_088c919562d7448b8f64e5e7c85bef8f_Out_0 = IN.ScreenPosition;
    float _Split_715c1865a284495abb42c53b6db4bb77_R_1 = _ScreenPosition_088c919562d7448b8f64e5e7c85bef8f_Out_0[0];
    float _Split_715c1865a284495abb42c53b6db4bb77_G_2 = _ScreenPosition_088c919562d7448b8f64e5e7c85bef8f_Out_0[1];
    float _Split_715c1865a284495abb42c53b6db4bb77_B_3 = _ScreenPosition_088c919562d7448b8f64e5e7c85bef8f_Out_0[2];
    float _Split_715c1865a284495abb42c53b6db4bb77_A_4 = _ScreenPosition_088c919562d7448b8f64e5e7c85bef8f_Out_0[3];
    float _Subtract_94c5a0cdecd3465c8ebce61a76126ae9_Out_2;
    Unity_Subtract_float(_SceneDepth_4b924296a5444c71987b488d105a7c45_Out_1, _Split_715c1865a284495abb42c53b6db4bb77_A_4, _Subtract_94c5a0cdecd3465c8ebce61a76126ae9_Out_2);
    float _Property_c2fb2ab4753f4a67b952b67243833c4a_Out_0 = Vector1_4f1d2b1f25a248f3923ab327466f2485;
    float _Divide_6b505606b9094bd6890b7e45cb0eb021_Out_2;
    Unity_Divide_float(_Subtract_94c5a0cdecd3465c8ebce61a76126ae9_Out_2, _Property_c2fb2ab4753f4a67b952b67243833c4a_Out_0, _Divide_6b505606b9094bd6890b7e45cb0eb021_Out_2);
    float _Saturate_39416b919e4d4793892c252123310234_Out_1;
    Unity_Saturate_float(_Divide_6b505606b9094bd6890b7e45cb0eb021_Out_2, _Saturate_39416b919e4d4793892c252123310234_Out_1);
    surface.BaseColor = (_Multiply_cb02d0721341452692ce3355b85e3b79_Out_2.xyz);
    surface.NormalTS = _NormalStrength_1323c37b2b684b1193977b46a393a553_Out_2;
    surface.Emission = float3(0, 0, 0);
    surface.Metallic = _SampleTexture2D_056131d47da244f48c8ea7a72bb86401_B_6;
    surface.Smoothness = _SampleTexture2D_056131d47da244f48c8ea7a72bb86401_G_5;
    surface.Occlusion = _SampleTexture2D_056131d47da244f48c8ea7a72bb86401_R_4;
    surface.Alpha = _Saturate_39416b919e4d4793892c252123310234_Out_1;
    surface.AlphaClipThreshold = 0.1;
    return surface;
}

// --------------------------------------------------
// Build Graph Inputs

VertexDescriptionInputs BuildVertexDescriptionInputs(Attributes input)
{
    VertexDescriptionInputs output;
    ZERO_INITIALIZE(VertexDescriptionInputs, output);

    output.ObjectSpaceNormal = input.normalOS;
    output.ObjectSpaceTangent = input.tangentOS.xyz;
    output.ObjectSpacePosition = input.positionOS;

    return output;
}
    SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
{
    SurfaceDescriptionInputs output;
    ZERO_INITIALIZE(SurfaceDescriptionInputs, output);



    output.TangentSpaceNormal = float3(0.0f, 0.0f, 1.0f);


    output.WorldSpacePosition = input.positionWS;
    output.AbsoluteWorldSpacePosition = GetAbsolutePositionWS(input.positionWS);
    output.ScreenPosition = ComputeScreenPos(TransformWorldToHClip(input.positionWS), _ProjectionParams.x);
#if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
#define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN output.FaceSign =                    IS_FRONT_VFACE(input.cullFace, true, false);
#else
#define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
#endif
#undef BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN

    return output;
}

    // --------------------------------------------------
    // Main

    #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"
#include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/Varyings.hlsl"
#include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/PBRForwardPass.hlsl"

    ENDHLSL
}
Pass
{
    Name "ShadowCaster"
    Tags
    {
        "LightMode" = "ShadowCaster"
    }

        // Render State
        Cull Back
    Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha
    ZTest LEqual
    ZWrite On
    ColorMask 0

        // Debug
        // <None>

        // --------------------------------------------------
        // Pass

        HLSLPROGRAM

        // Pragmas
        #pragma target 2.0
    #pragma only_renderers gles gles3 glcore d3d11
    #pragma multi_compile_instancing
    #pragma vertex vert
    #pragma fragment frag

        // DotsInstancingOptions: <None>
        // HybridV1InjectedBuiltinProperties: <None>

        // Keywords
        // PassKeywords: <None>
        // GraphKeywords: <None>

        // Defines
        #define _SURFACE_TYPE_TRANSPARENT 1
        #define _AlphaClip 1
        #define _NORMALMAP 1
        #define _NORMAL_DROPOFF_TS 1
        #define ATTRIBUTES_NEED_NORMAL
        #define ATTRIBUTES_NEED_TANGENT
        #define VARYINGS_NEED_POSITION_WS
        #define FEATURES_GRAPH_VERTEX
        /* WARNING: $splice Could not find named fragment 'PassInstancing' */
        #define SHADERPASS SHADERPASS_SHADOWCASTER
    #define REQUIRE_DEPTH_TEXTURE
        /* WARNING: $splice Could not find named fragment 'DotsInstancingVars' */

        // Includes
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
    #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
    #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"

        // --------------------------------------------------
        // Structs and Packing

        struct Attributes
    {
        float3 positionOS : POSITION;
        float3 normalOS : NORMAL;
        float4 tangentOS : TANGENT;
        #if UNITY_ANY_INSTANCING_ENABLED
        uint instanceID : INSTANCEID_SEMANTIC;
        #endif
    };
    struct Varyings
    {
        float4 positionCS : SV_POSITION;
        float3 positionWS;
        #if UNITY_ANY_INSTANCING_ENABLED
        uint instanceID : CUSTOM_INSTANCE_ID;
        #endif
        #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
        uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
        #endif
        #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
        uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
        #endif
        #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
        FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
        #endif
    };
    struct SurfaceDescriptionInputs
    {
        float3 WorldSpacePosition;
        float4 ScreenPosition;
    };
    struct VertexDescriptionInputs
    {
        float3 ObjectSpaceNormal;
        float3 ObjectSpaceTangent;
        float3 ObjectSpacePosition;
    };
    struct PackedVaryings
    {
        float4 positionCS : SV_POSITION;
        float3 interp0 : TEXCOORD0;
        #if UNITY_ANY_INSTANCING_ENABLED
        uint instanceID : CUSTOM_INSTANCE_ID;
        #endif
        #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
        uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
        #endif
        #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
        uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
        #endif
        #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
        FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
        #endif
    };

        PackedVaryings PackVaryings(Varyings input)
    {
        PackedVaryings output;
        output.positionCS = input.positionCS;
        output.interp0.xyz = input.positionWS;
        #if UNITY_ANY_INSTANCING_ENABLED
        output.instanceID = input.instanceID;
        #endif
        #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
        output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
        #endif
        #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
        output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
        #endif
        #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
        output.cullFace = input.cullFace;
        #endif
        return output;
    }
    Varyings UnpackVaryings(PackedVaryings input)
    {
        Varyings output;
        output.positionCS = input.positionCS;
        output.positionWS = input.interp0.xyz;
        #if UNITY_ANY_INSTANCING_ENABLED
        output.instanceID = input.instanceID;
        #endif
        #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
        output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
        #endif
        #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
        output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
        #endif
        #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
        output.cullFace = input.cullFace;
        #endif
        return output;
    }

    // --------------------------------------------------
    // Graph

    // Graph Properties
    CBUFFER_START(UnityPerMaterial)
float4 Texture2D_818cca92a18946a3b27ffbaf1b40a062_TexelSize;
float4 Texture2D_3f2131083fc44d9b989206ab02babd86_TexelSize;
float4 Texture2D_b01d9e81c8e04473b8880e3878694e5e_TexelSize;
float Vector1_accfafba726746c885b8e27280080a86;
float Vector1_c5686f27abfe4d5081a5b5eee32afda8;
float Vector1_9ec3ae0c6d7a4e1f8b784c14876e5174;
float4 Color_dc69a74bd5704ea796654354ea94b8d9;
float Vector1_4f1d2b1f25a248f3923ab327466f2485;
CBUFFER_END

// Object and Global properties
SAMPLER(SamplerState_Linear_Repeat);
TEXTURE2D(Texture2D_818cca92a18946a3b27ffbaf1b40a062);
SAMPLER(samplerTexture2D_818cca92a18946a3b27ffbaf1b40a062);
TEXTURE2D(Texture2D_3f2131083fc44d9b989206ab02babd86);
SAMPLER(samplerTexture2D_3f2131083fc44d9b989206ab02babd86);
TEXTURE2D(Texture2D_b01d9e81c8e04473b8880e3878694e5e);
SAMPLER(samplerTexture2D_b01d9e81c8e04473b8880e3878694e5e);

// Graph Functions

void Unity_SceneDepth_Eye_float(float4 UV, out float Out)
{
    Out = LinearEyeDepth(SHADERGRAPH_SAMPLE_SCENE_DEPTH(UV.xy), _ZBufferParams);
}

void Unity_Subtract_float(float A, float B, out float Out)
{
    Out = A - B;
}

void Unity_Divide_float(float A, float B, out float Out)
{
    Out = A / B;
}

void Unity_Saturate_float(float In, out float Out)
{
    Out = saturate(In);
}

// Graph Vertex
struct VertexDescription
{
    float3 Position;
    float3 Normal;
    float3 Tangent;
};

VertexDescription VertexDescriptionFunction(VertexDescriptionInputs IN)
{
    VertexDescription description = (VertexDescription)0;
    description.Position = IN.ObjectSpacePosition;
    description.Normal = IN.ObjectSpaceNormal;
    description.Tangent = IN.ObjectSpaceTangent;
    return description;
}

// Graph Pixel
struct SurfaceDescription
{
    float Alpha;
    float AlphaClipThreshold;
};

SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
{
    SurfaceDescription surface = (SurfaceDescription)0;
    float _SceneDepth_4b924296a5444c71987b488d105a7c45_Out_1;
    Unity_SceneDepth_Eye_float(float4(IN.ScreenPosition.xy / IN.ScreenPosition.w, 0, 0), _SceneDepth_4b924296a5444c71987b488d105a7c45_Out_1);
    float4 _ScreenPosition_088c919562d7448b8f64e5e7c85bef8f_Out_0 = IN.ScreenPosition;
    float _Split_715c1865a284495abb42c53b6db4bb77_R_1 = _ScreenPosition_088c919562d7448b8f64e5e7c85bef8f_Out_0[0];
    float _Split_715c1865a284495abb42c53b6db4bb77_G_2 = _ScreenPosition_088c919562d7448b8f64e5e7c85bef8f_Out_0[1];
    float _Split_715c1865a284495abb42c53b6db4bb77_B_3 = _ScreenPosition_088c919562d7448b8f64e5e7c85bef8f_Out_0[2];
    float _Split_715c1865a284495abb42c53b6db4bb77_A_4 = _ScreenPosition_088c919562d7448b8f64e5e7c85bef8f_Out_0[3];
    float _Subtract_94c5a0cdecd3465c8ebce61a76126ae9_Out_2;
    Unity_Subtract_float(_SceneDepth_4b924296a5444c71987b488d105a7c45_Out_1, _Split_715c1865a284495abb42c53b6db4bb77_A_4, _Subtract_94c5a0cdecd3465c8ebce61a76126ae9_Out_2);
    float _Property_c2fb2ab4753f4a67b952b67243833c4a_Out_0 = Vector1_4f1d2b1f25a248f3923ab327466f2485;
    float _Divide_6b505606b9094bd6890b7e45cb0eb021_Out_2;
    Unity_Divide_float(_Subtract_94c5a0cdecd3465c8ebce61a76126ae9_Out_2, _Property_c2fb2ab4753f4a67b952b67243833c4a_Out_0, _Divide_6b505606b9094bd6890b7e45cb0eb021_Out_2);
    float _Saturate_39416b919e4d4793892c252123310234_Out_1;
    Unity_Saturate_float(_Divide_6b505606b9094bd6890b7e45cb0eb021_Out_2, _Saturate_39416b919e4d4793892c252123310234_Out_1);
    surface.Alpha = _Saturate_39416b919e4d4793892c252123310234_Out_1;
    surface.AlphaClipThreshold = 0.1;
    return surface;
}

// --------------------------------------------------
// Build Graph Inputs

VertexDescriptionInputs BuildVertexDescriptionInputs(Attributes input)
{
    VertexDescriptionInputs output;
    ZERO_INITIALIZE(VertexDescriptionInputs, output);

    output.ObjectSpaceNormal = input.normalOS;
    output.ObjectSpaceTangent = input.tangentOS.xyz;
    output.ObjectSpacePosition = input.positionOS;

    return output;
}
    SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
{
    SurfaceDescriptionInputs output;
    ZERO_INITIALIZE(SurfaceDescriptionInputs, output);





    output.WorldSpacePosition = input.positionWS;
    output.ScreenPosition = ComputeScreenPos(TransformWorldToHClip(input.positionWS), _ProjectionParams.x);
#if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
#define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN output.FaceSign =                    IS_FRONT_VFACE(input.cullFace, true, false);
#else
#define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
#endif
#undef BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN

    return output;
}

    // --------------------------------------------------
    // Main

    #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"
#include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/Varyings.hlsl"
#include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShadowCasterPass.hlsl"

    ENDHLSL
}
Pass
{
    Name "DepthOnly"
    Tags
    {
        "LightMode" = "DepthOnly"
    }

        // Render State
        Cull Back
    Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha
    ZTest LEqual
    ZWrite On
    ColorMask 0

        // Debug
        // <None>

        // --------------------------------------------------
        // Pass

        HLSLPROGRAM

        // Pragmas
        #pragma target 2.0
    #pragma only_renderers gles gles3 glcore d3d11
    #pragma multi_compile_instancing
    #pragma vertex vert
    #pragma fragment frag

        // DotsInstancingOptions: <None>
        // HybridV1InjectedBuiltinProperties: <None>

        // Keywords
        // PassKeywords: <None>
        // GraphKeywords: <None>

        // Defines
        #define _SURFACE_TYPE_TRANSPARENT 1
        #define _AlphaClip 1
        #define _NORMALMAP 1
        #define _NORMAL_DROPOFF_TS 1
        #define ATTRIBUTES_NEED_NORMAL
        #define ATTRIBUTES_NEED_TANGENT
        #define VARYINGS_NEED_POSITION_WS
        #define FEATURES_GRAPH_VERTEX
        /* WARNING: $splice Could not find named fragment 'PassInstancing' */
        #define SHADERPASS SHADERPASS_DEPTHONLY
    #define REQUIRE_DEPTH_TEXTURE
        /* WARNING: $splice Could not find named fragment 'DotsInstancingVars' */

        // Includes
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
    #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
    #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"

        // --------------------------------------------------
        // Structs and Packing

        struct Attributes
    {
        float3 positionOS : POSITION;
        float3 normalOS : NORMAL;
        float4 tangentOS : TANGENT;
        #if UNITY_ANY_INSTANCING_ENABLED
        uint instanceID : INSTANCEID_SEMANTIC;
        #endif
    };
    struct Varyings
    {
        float4 positionCS : SV_POSITION;
        float3 positionWS;
        #if UNITY_ANY_INSTANCING_ENABLED
        uint instanceID : CUSTOM_INSTANCE_ID;
        #endif
        #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
        uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
        #endif
        #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
        uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
        #endif
        #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
        FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
        #endif
    };
    struct SurfaceDescriptionInputs
    {
        float3 WorldSpacePosition;
        float4 ScreenPosition;
    };
    struct VertexDescriptionInputs
    {
        float3 ObjectSpaceNormal;
        float3 ObjectSpaceTangent;
        float3 ObjectSpacePosition;
    };
    struct PackedVaryings
    {
        float4 positionCS : SV_POSITION;
        float3 interp0 : TEXCOORD0;
        #if UNITY_ANY_INSTANCING_ENABLED
        uint instanceID : CUSTOM_INSTANCE_ID;
        #endif
        #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
        uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
        #endif
        #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
        uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
        #endif
        #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
        FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
        #endif
    };

        PackedVaryings PackVaryings(Varyings input)
    {
        PackedVaryings output;
        output.positionCS = input.positionCS;
        output.interp0.xyz = input.positionWS;
        #if UNITY_ANY_INSTANCING_ENABLED
        output.instanceID = input.instanceID;
        #endif
        #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
        output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
        #endif
        #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
        output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
        #endif
        #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
        output.cullFace = input.cullFace;
        #endif
        return output;
    }
    Varyings UnpackVaryings(PackedVaryings input)
    {
        Varyings output;
        output.positionCS = input.positionCS;
        output.positionWS = input.interp0.xyz;
        #if UNITY_ANY_INSTANCING_ENABLED
        output.instanceID = input.instanceID;
        #endif
        #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
        output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
        #endif
        #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
        output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
        #endif
        #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
        output.cullFace = input.cullFace;
        #endif
        return output;
    }

    // --------------------------------------------------
    // Graph

    // Graph Properties
    CBUFFER_START(UnityPerMaterial)
float4 Texture2D_818cca92a18946a3b27ffbaf1b40a062_TexelSize;
float4 Texture2D_3f2131083fc44d9b989206ab02babd86_TexelSize;
float4 Texture2D_b01d9e81c8e04473b8880e3878694e5e_TexelSize;
float Vector1_accfafba726746c885b8e27280080a86;
float Vector1_c5686f27abfe4d5081a5b5eee32afda8;
float Vector1_9ec3ae0c6d7a4e1f8b784c14876e5174;
float4 Color_dc69a74bd5704ea796654354ea94b8d9;
float Vector1_4f1d2b1f25a248f3923ab327466f2485;
CBUFFER_END

// Object and Global properties
SAMPLER(SamplerState_Linear_Repeat);
TEXTURE2D(Texture2D_818cca92a18946a3b27ffbaf1b40a062);
SAMPLER(samplerTexture2D_818cca92a18946a3b27ffbaf1b40a062);
TEXTURE2D(Texture2D_3f2131083fc44d9b989206ab02babd86);
SAMPLER(samplerTexture2D_3f2131083fc44d9b989206ab02babd86);
TEXTURE2D(Texture2D_b01d9e81c8e04473b8880e3878694e5e);
SAMPLER(samplerTexture2D_b01d9e81c8e04473b8880e3878694e5e);

// Graph Functions

void Unity_SceneDepth_Eye_float(float4 UV, out float Out)
{
    Out = LinearEyeDepth(SHADERGRAPH_SAMPLE_SCENE_DEPTH(UV.xy), _ZBufferParams);
}

void Unity_Subtract_float(float A, float B, out float Out)
{
    Out = A - B;
}

void Unity_Divide_float(float A, float B, out float Out)
{
    Out = A / B;
}

void Unity_Saturate_float(float In, out float Out)
{
    Out = saturate(In);
}

// Graph Vertex
struct VertexDescription
{
    float3 Position;
    float3 Normal;
    float3 Tangent;
};

VertexDescription VertexDescriptionFunction(VertexDescriptionInputs IN)
{
    VertexDescription description = (VertexDescription)0;
    description.Position = IN.ObjectSpacePosition;
    description.Normal = IN.ObjectSpaceNormal;
    description.Tangent = IN.ObjectSpaceTangent;
    return description;
}

// Graph Pixel
struct SurfaceDescription
{
    float Alpha;
    float AlphaClipThreshold;
};

SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
{
    SurfaceDescription surface = (SurfaceDescription)0;
    float _SceneDepth_4b924296a5444c71987b488d105a7c45_Out_1;
    Unity_SceneDepth_Eye_float(float4(IN.ScreenPosition.xy / IN.ScreenPosition.w, 0, 0), _SceneDepth_4b924296a5444c71987b488d105a7c45_Out_1);
    float4 _ScreenPosition_088c919562d7448b8f64e5e7c85bef8f_Out_0 = IN.ScreenPosition;
    float _Split_715c1865a284495abb42c53b6db4bb77_R_1 = _ScreenPosition_088c919562d7448b8f64e5e7c85bef8f_Out_0[0];
    float _Split_715c1865a284495abb42c53b6db4bb77_G_2 = _ScreenPosition_088c919562d7448b8f64e5e7c85bef8f_Out_0[1];
    float _Split_715c1865a284495abb42c53b6db4bb77_B_3 = _ScreenPosition_088c919562d7448b8f64e5e7c85bef8f_Out_0[2];
    float _Split_715c1865a284495abb42c53b6db4bb77_A_4 = _ScreenPosition_088c919562d7448b8f64e5e7c85bef8f_Out_0[3];
    float _Subtract_94c5a0cdecd3465c8ebce61a76126ae9_Out_2;
    Unity_Subtract_float(_SceneDepth_4b924296a5444c71987b488d105a7c45_Out_1, _Split_715c1865a284495abb42c53b6db4bb77_A_4, _Subtract_94c5a0cdecd3465c8ebce61a76126ae9_Out_2);
    float _Property_c2fb2ab4753f4a67b952b67243833c4a_Out_0 = Vector1_4f1d2b1f25a248f3923ab327466f2485;
    float _Divide_6b505606b9094bd6890b7e45cb0eb021_Out_2;
    Unity_Divide_float(_Subtract_94c5a0cdecd3465c8ebce61a76126ae9_Out_2, _Property_c2fb2ab4753f4a67b952b67243833c4a_Out_0, _Divide_6b505606b9094bd6890b7e45cb0eb021_Out_2);
    float _Saturate_39416b919e4d4793892c252123310234_Out_1;
    Unity_Saturate_float(_Divide_6b505606b9094bd6890b7e45cb0eb021_Out_2, _Saturate_39416b919e4d4793892c252123310234_Out_1);
    surface.Alpha = _Saturate_39416b919e4d4793892c252123310234_Out_1;
    surface.AlphaClipThreshold = 0.1;
    return surface;
}

// --------------------------------------------------
// Build Graph Inputs

VertexDescriptionInputs BuildVertexDescriptionInputs(Attributes input)
{
    VertexDescriptionInputs output;
    ZERO_INITIALIZE(VertexDescriptionInputs, output);

    output.ObjectSpaceNormal = input.normalOS;
    output.ObjectSpaceTangent = input.tangentOS.xyz;
    output.ObjectSpacePosition = input.positionOS;

    return output;
}
    SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
{
    SurfaceDescriptionInputs output;
    ZERO_INITIALIZE(SurfaceDescriptionInputs, output);





    output.WorldSpacePosition = input.positionWS;
    output.ScreenPosition = ComputeScreenPos(TransformWorldToHClip(input.positionWS), _ProjectionParams.x);
#if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
#define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN output.FaceSign =                    IS_FRONT_VFACE(input.cullFace, true, false);
#else
#define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
#endif
#undef BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN

    return output;
}

    // --------------------------------------------------
    // Main

    #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"
#include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/Varyings.hlsl"
#include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/DepthOnlyPass.hlsl"

    ENDHLSL
}
Pass
{
    Name "DepthNormals"
    Tags
    {
        "LightMode" = "DepthNormals"
    }

        // Render State
        Cull Back
    Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha
    ZTest LEqual
    ZWrite On

        // Debug
        // <None>

        // --------------------------------------------------
        // Pass

        HLSLPROGRAM

        // Pragmas
        #pragma target 2.0
    #pragma only_renderers gles gles3 glcore d3d11
    #pragma multi_compile_instancing
    #pragma vertex vert
    #pragma fragment frag

        // DotsInstancingOptions: <None>
        // HybridV1InjectedBuiltinProperties: <None>

        // Keywords
        // PassKeywords: <None>
        // GraphKeywords: <None>

        // Defines
        #define _SURFACE_TYPE_TRANSPARENT 1
        #define _AlphaClip 1
        #define _NORMALMAP 1
        #define _NORMAL_DROPOFF_TS 1
        #define ATTRIBUTES_NEED_NORMAL
        #define ATTRIBUTES_NEED_TANGENT
        #define ATTRIBUTES_NEED_TEXCOORD1
        #define VARYINGS_NEED_POSITION_WS
        #define VARYINGS_NEED_NORMAL_WS
        #define VARYINGS_NEED_TANGENT_WS
        #define FEATURES_GRAPH_VERTEX
        /* WARNING: $splice Could not find named fragment 'PassInstancing' */
        #define SHADERPASS SHADERPASS_DEPTHNORMALSONLY
    #define REQUIRE_DEPTH_TEXTURE
        /* WARNING: $splice Could not find named fragment 'DotsInstancingVars' */

        // Includes
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
    #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
    #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"

        // --------------------------------------------------
        // Structs and Packing

        struct Attributes
    {
        float3 positionOS : POSITION;
        float3 normalOS : NORMAL;
        float4 tangentOS : TANGENT;
        float4 uv1 : TEXCOORD1;
        #if UNITY_ANY_INSTANCING_ENABLED
        uint instanceID : INSTANCEID_SEMANTIC;
        #endif
    };
    struct Varyings
    {
        float4 positionCS : SV_POSITION;
        float3 positionWS;
        float3 normalWS;
        float4 tangentWS;
        #if UNITY_ANY_INSTANCING_ENABLED
        uint instanceID : CUSTOM_INSTANCE_ID;
        #endif
        #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
        uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
        #endif
        #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
        uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
        #endif
        #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
        FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
        #endif
    };
    struct SurfaceDescriptionInputs
    {
        float3 TangentSpaceNormal;
        float3 WorldSpacePosition;
        float3 AbsoluteWorldSpacePosition;
        float4 ScreenPosition;
    };
    struct VertexDescriptionInputs
    {
        float3 ObjectSpaceNormal;
        float3 ObjectSpaceTangent;
        float3 ObjectSpacePosition;
    };
    struct PackedVaryings
    {
        float4 positionCS : SV_POSITION;
        float3 interp0 : TEXCOORD0;
        float3 interp1 : TEXCOORD1;
        float4 interp2 : TEXCOORD2;
        #if UNITY_ANY_INSTANCING_ENABLED
        uint instanceID : CUSTOM_INSTANCE_ID;
        #endif
        #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
        uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
        #endif
        #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
        uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
        #endif
        #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
        FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
        #endif
    };

        PackedVaryings PackVaryings(Varyings input)
    {
        PackedVaryings output;
        output.positionCS = input.positionCS;
        output.interp0.xyz = input.positionWS;
        output.interp1.xyz = input.normalWS;
        output.interp2.xyzw = input.tangentWS;
        #if UNITY_ANY_INSTANCING_ENABLED
        output.instanceID = input.instanceID;
        #endif
        #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
        output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
        #endif
        #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
        output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
        #endif
        #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
        output.cullFace = input.cullFace;
        #endif
        return output;
    }
    Varyings UnpackVaryings(PackedVaryings input)
    {
        Varyings output;
        output.positionCS = input.positionCS;
        output.positionWS = input.interp0.xyz;
        output.normalWS = input.interp1.xyz;
        output.tangentWS = input.interp2.xyzw;
        #if UNITY_ANY_INSTANCING_ENABLED
        output.instanceID = input.instanceID;
        #endif
        #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
        output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
        #endif
        #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
        output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
        #endif
        #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
        output.cullFace = input.cullFace;
        #endif
        return output;
    }

    // --------------------------------------------------
    // Graph

    // Graph Properties
    CBUFFER_START(UnityPerMaterial)
float4 Texture2D_818cca92a18946a3b27ffbaf1b40a062_TexelSize;
float4 Texture2D_3f2131083fc44d9b989206ab02babd86_TexelSize;
float4 Texture2D_b01d9e81c8e04473b8880e3878694e5e_TexelSize;
float Vector1_accfafba726746c885b8e27280080a86;
float Vector1_c5686f27abfe4d5081a5b5eee32afda8;
float Vector1_9ec3ae0c6d7a4e1f8b784c14876e5174;
float4 Color_dc69a74bd5704ea796654354ea94b8d9;
float Vector1_4f1d2b1f25a248f3923ab327466f2485;
CBUFFER_END

// Object and Global properties
SAMPLER(SamplerState_Linear_Repeat);
TEXTURE2D(Texture2D_818cca92a18946a3b27ffbaf1b40a062);
SAMPLER(samplerTexture2D_818cca92a18946a3b27ffbaf1b40a062);
TEXTURE2D(Texture2D_3f2131083fc44d9b989206ab02babd86);
SAMPLER(samplerTexture2D_3f2131083fc44d9b989206ab02babd86);
TEXTURE2D(Texture2D_b01d9e81c8e04473b8880e3878694e5e);
SAMPLER(samplerTexture2D_b01d9e81c8e04473b8880e3878694e5e);

// Graph Functions

void Unity_Multiply_float(float2 A, float2 B, out float2 Out)
{
    Out = A * B;
}

void Unity_Rotate_Radians_float(float2 UV, float2 Center, float Rotation, out float2 Out)
{
    //rotation matrix
    UV -= Center;
    float s = sin(Rotation);
    float c = cos(Rotation);

    //center rotation matrix
    float2x2 rMatrix = float2x2(c, -s, s, c);
    rMatrix *= 0.5;
    rMatrix += 0.5;
    rMatrix = rMatrix * 2 - 1;

    //multiply the UVs by the rotation matrix
    UV.xy = mul(UV.xy, rMatrix);
    UV += Center;

    Out = UV;
}

void Unity_NormalStrength_float(float3 In, float Strength, out float3 Out)
{
    Out = float3(In.rg * Strength, lerp(1, In.b, saturate(Strength)));
}

void Unity_SceneDepth_Eye_float(float4 UV, out float Out)
{
    Out = LinearEyeDepth(SHADERGRAPH_SAMPLE_SCENE_DEPTH(UV.xy), _ZBufferParams);
}

void Unity_Subtract_float(float A, float B, out float Out)
{
    Out = A - B;
}

void Unity_Divide_float(float A, float B, out float Out)
{
    Out = A / B;
}

void Unity_Saturate_float(float In, out float Out)
{
    Out = saturate(In);
}

// Graph Vertex
struct VertexDescription
{
    float3 Position;
    float3 Normal;
    float3 Tangent;
};

VertexDescription VertexDescriptionFunction(VertexDescriptionInputs IN)
{
    VertexDescription description = (VertexDescription)0;
    description.Position = IN.ObjectSpacePosition;
    description.Normal = IN.ObjectSpaceNormal;
    description.Tangent = IN.ObjectSpaceTangent;
    return description;
}

// Graph Pixel
struct SurfaceDescription
{
    float3 NormalTS;
    float Alpha;
    float AlphaClipThreshold;
};

SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
{
    SurfaceDescription surface = (SurfaceDescription)0;
    UnityTexture2D _Property_a6c0ed483ac044e08dbc27cbaab9842b_Out_0 = UnityBuildTexture2DStructNoScale(Texture2D_b01d9e81c8e04473b8880e3878694e5e);
    float _Split_a0d1d32f695a4ceabaf3057615b92541_R_1 = IN.AbsoluteWorldSpacePosition[0];
    float _Split_a0d1d32f695a4ceabaf3057615b92541_G_2 = IN.AbsoluteWorldSpacePosition[1];
    float _Split_a0d1d32f695a4ceabaf3057615b92541_B_3 = IN.AbsoluteWorldSpacePosition[2];
    float _Split_a0d1d32f695a4ceabaf3057615b92541_A_4 = 0;
    float2 _Vector2_f84baf3fcaab45edb30625c920fb91bd_Out_0 = float2(_Split_a0d1d32f695a4ceabaf3057615b92541_R_1, _Split_a0d1d32f695a4ceabaf3057615b92541_B_3);
    float _Property_505300b000564f4fbe41a0d1fb1dd9bb_Out_0 = Vector1_c5686f27abfe4d5081a5b5eee32afda8;
    float2 _Multiply_adf8c2949a394a748dcbaf9bbecc58b5_Out_2;
    Unity_Multiply_float(_Vector2_f84baf3fcaab45edb30625c920fb91bd_Out_0, (_Property_505300b000564f4fbe41a0d1fb1dd9bb_Out_0.xx), _Multiply_adf8c2949a394a748dcbaf9bbecc58b5_Out_2);
    float2 _Vector2_6df102a229904a54a01b4e43c9924cce_Out_0 = float2(0.5, 0.5);
    float _Property_25c7c338c2a340f99397839c68ce01bf_Out_0 = Vector1_9ec3ae0c6d7a4e1f8b784c14876e5174;
    float2 _Rotate_e74144c0506c44c79764e30df1dcc15c_Out_3;
    Unity_Rotate_Radians_float(_Multiply_adf8c2949a394a748dcbaf9bbecc58b5_Out_2, _Vector2_6df102a229904a54a01b4e43c9924cce_Out_0, _Property_25c7c338c2a340f99397839c68ce01bf_Out_0, _Rotate_e74144c0506c44c79764e30df1dcc15c_Out_3);
    float4 _SampleTexture2D_b451781d16ed4f80b11c1edbdc0145a1_RGBA_0 = SAMPLE_TEXTURE2D(_Property_a6c0ed483ac044e08dbc27cbaab9842b_Out_0.tex, _Property_a6c0ed483ac044e08dbc27cbaab9842b_Out_0.samplerstate, _Rotate_e74144c0506c44c79764e30df1dcc15c_Out_3);
    _SampleTexture2D_b451781d16ed4f80b11c1edbdc0145a1_RGBA_0.rgb = UnpackNormal(_SampleTexture2D_b451781d16ed4f80b11c1edbdc0145a1_RGBA_0);
    float _SampleTexture2D_b451781d16ed4f80b11c1edbdc0145a1_R_4 = _SampleTexture2D_b451781d16ed4f80b11c1edbdc0145a1_RGBA_0.r;
    float _SampleTexture2D_b451781d16ed4f80b11c1edbdc0145a1_G_5 = _SampleTexture2D_b451781d16ed4f80b11c1edbdc0145a1_RGBA_0.g;
    float _SampleTexture2D_b451781d16ed4f80b11c1edbdc0145a1_B_6 = _SampleTexture2D_b451781d16ed4f80b11c1edbdc0145a1_RGBA_0.b;
    float _SampleTexture2D_b451781d16ed4f80b11c1edbdc0145a1_A_7 = _SampleTexture2D_b451781d16ed4f80b11c1edbdc0145a1_RGBA_0.a;
    float _Property_99d8f29d83c44a4ba32c6eabbbc92ab2_Out_0 = Vector1_accfafba726746c885b8e27280080a86;
    float3 _NormalStrength_1323c37b2b684b1193977b46a393a553_Out_2;
    Unity_NormalStrength_float((_SampleTexture2D_b451781d16ed4f80b11c1edbdc0145a1_RGBA_0.xyz), _Property_99d8f29d83c44a4ba32c6eabbbc92ab2_Out_0, _NormalStrength_1323c37b2b684b1193977b46a393a553_Out_2);
    float _SceneDepth_4b924296a5444c71987b488d105a7c45_Out_1;
    Unity_SceneDepth_Eye_float(float4(IN.ScreenPosition.xy / IN.ScreenPosition.w, 0, 0), _SceneDepth_4b924296a5444c71987b488d105a7c45_Out_1);
    float4 _ScreenPosition_088c919562d7448b8f64e5e7c85bef8f_Out_0 = IN.ScreenPosition;
    float _Split_715c1865a284495abb42c53b6db4bb77_R_1 = _ScreenPosition_088c919562d7448b8f64e5e7c85bef8f_Out_0[0];
    float _Split_715c1865a284495abb42c53b6db4bb77_G_2 = _ScreenPosition_088c919562d7448b8f64e5e7c85bef8f_Out_0[1];
    float _Split_715c1865a284495abb42c53b6db4bb77_B_3 = _ScreenPosition_088c919562d7448b8f64e5e7c85bef8f_Out_0[2];
    float _Split_715c1865a284495abb42c53b6db4bb77_A_4 = _ScreenPosition_088c919562d7448b8f64e5e7c85bef8f_Out_0[3];
    float _Subtract_94c5a0cdecd3465c8ebce61a76126ae9_Out_2;
    Unity_Subtract_float(_SceneDepth_4b924296a5444c71987b488d105a7c45_Out_1, _Split_715c1865a284495abb42c53b6db4bb77_A_4, _Subtract_94c5a0cdecd3465c8ebce61a76126ae9_Out_2);
    float _Property_c2fb2ab4753f4a67b952b67243833c4a_Out_0 = Vector1_4f1d2b1f25a248f3923ab327466f2485;
    float _Divide_6b505606b9094bd6890b7e45cb0eb021_Out_2;
    Unity_Divide_float(_Subtract_94c5a0cdecd3465c8ebce61a76126ae9_Out_2, _Property_c2fb2ab4753f4a67b952b67243833c4a_Out_0, _Divide_6b505606b9094bd6890b7e45cb0eb021_Out_2);
    float _Saturate_39416b919e4d4793892c252123310234_Out_1;
    Unity_Saturate_float(_Divide_6b505606b9094bd6890b7e45cb0eb021_Out_2, _Saturate_39416b919e4d4793892c252123310234_Out_1);
    surface.NormalTS = _NormalStrength_1323c37b2b684b1193977b46a393a553_Out_2;
    surface.Alpha = _Saturate_39416b919e4d4793892c252123310234_Out_1;
    surface.AlphaClipThreshold = 0.1;
    return surface;
}

// --------------------------------------------------
// Build Graph Inputs

VertexDescriptionInputs BuildVertexDescriptionInputs(Attributes input)
{
    VertexDescriptionInputs output;
    ZERO_INITIALIZE(VertexDescriptionInputs, output);

    output.ObjectSpaceNormal = input.normalOS;
    output.ObjectSpaceTangent = input.tangentOS.xyz;
    output.ObjectSpacePosition = input.positionOS;

    return output;
}
    SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
{
    SurfaceDescriptionInputs output;
    ZERO_INITIALIZE(SurfaceDescriptionInputs, output);



    output.TangentSpaceNormal = float3(0.0f, 0.0f, 1.0f);


    output.WorldSpacePosition = input.positionWS;
    output.AbsoluteWorldSpacePosition = GetAbsolutePositionWS(input.positionWS);
    output.ScreenPosition = ComputeScreenPos(TransformWorldToHClip(input.positionWS), _ProjectionParams.x);
#if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
#define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN output.FaceSign =                    IS_FRONT_VFACE(input.cullFace, true, false);
#else
#define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
#endif
#undef BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN

    return output;
}

    // --------------------------------------------------
    // Main

    #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"
#include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/Varyings.hlsl"
#include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/DepthNormalsOnlyPass.hlsl"

    ENDHLSL
}
Pass
{
    Name "Meta"
    Tags
    {
        "LightMode" = "Meta"
    }

        // Render State
        Cull Off

        // Debug
        // <None>

        // --------------------------------------------------
        // Pass

        HLSLPROGRAM

        // Pragmas
        #pragma target 2.0
    #pragma only_renderers gles gles3 glcore d3d11
    #pragma vertex vert
    #pragma fragment frag

        // DotsInstancingOptions: <None>
        // HybridV1InjectedBuiltinProperties: <None>

        // Keywords
        #pragma shader_feature _ _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
        // GraphKeywords: <None>

        // Defines
        #define _SURFACE_TYPE_TRANSPARENT 1
        #define _AlphaClip 1
        #define _NORMALMAP 1
        #define _NORMAL_DROPOFF_TS 1
        #define ATTRIBUTES_NEED_NORMAL
        #define ATTRIBUTES_NEED_TANGENT
        #define ATTRIBUTES_NEED_TEXCOORD1
        #define ATTRIBUTES_NEED_TEXCOORD2
        #define VARYINGS_NEED_POSITION_WS
        #define FEATURES_GRAPH_VERTEX
        /* WARNING: $splice Could not find named fragment 'PassInstancing' */
        #define SHADERPASS SHADERPASS_META
    #define REQUIRE_DEPTH_TEXTURE
        /* WARNING: $splice Could not find named fragment 'DotsInstancingVars' */

        // Includes
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
    #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
    #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/MetaInput.hlsl"

        // --------------------------------------------------
        // Structs and Packing

        struct Attributes
    {
        float3 positionOS : POSITION;
        float3 normalOS : NORMAL;
        float4 tangentOS : TANGENT;
        float4 uv1 : TEXCOORD1;
        float4 uv2 : TEXCOORD2;
        #if UNITY_ANY_INSTANCING_ENABLED
        uint instanceID : INSTANCEID_SEMANTIC;
        #endif
    };
    struct Varyings
    {
        float4 positionCS : SV_POSITION;
        float3 positionWS;
        #if UNITY_ANY_INSTANCING_ENABLED
        uint instanceID : CUSTOM_INSTANCE_ID;
        #endif
        #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
        uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
        #endif
        #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
        uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
        #endif
        #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
        FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
        #endif
    };
    struct SurfaceDescriptionInputs
    {
        float3 WorldSpacePosition;
        float3 AbsoluteWorldSpacePosition;
        float4 ScreenPosition;
    };
    struct VertexDescriptionInputs
    {
        float3 ObjectSpaceNormal;
        float3 ObjectSpaceTangent;
        float3 ObjectSpacePosition;
    };
    struct PackedVaryings
    {
        float4 positionCS : SV_POSITION;
        float3 interp0 : TEXCOORD0;
        #if UNITY_ANY_INSTANCING_ENABLED
        uint instanceID : CUSTOM_INSTANCE_ID;
        #endif
        #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
        uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
        #endif
        #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
        uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
        #endif
        #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
        FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
        #endif
    };

        PackedVaryings PackVaryings(Varyings input)
    {
        PackedVaryings output;
        output.positionCS = input.positionCS;
        output.interp0.xyz = input.positionWS;
        #if UNITY_ANY_INSTANCING_ENABLED
        output.instanceID = input.instanceID;
        #endif
        #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
        output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
        #endif
        #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
        output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
        #endif
        #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
        output.cullFace = input.cullFace;
        #endif
        return output;
    }
    Varyings UnpackVaryings(PackedVaryings input)
    {
        Varyings output;
        output.positionCS = input.positionCS;
        output.positionWS = input.interp0.xyz;
        #if UNITY_ANY_INSTANCING_ENABLED
        output.instanceID = input.instanceID;
        #endif
        #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
        output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
        #endif
        #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
        output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
        #endif
        #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
        output.cullFace = input.cullFace;
        #endif
        return output;
    }

    // --------------------------------------------------
    // Graph

    // Graph Properties
    CBUFFER_START(UnityPerMaterial)
float4 Texture2D_818cca92a18946a3b27ffbaf1b40a062_TexelSize;
float4 Texture2D_3f2131083fc44d9b989206ab02babd86_TexelSize;
float4 Texture2D_b01d9e81c8e04473b8880e3878694e5e_TexelSize;
float Vector1_accfafba726746c885b8e27280080a86;
float Vector1_c5686f27abfe4d5081a5b5eee32afda8;
float Vector1_9ec3ae0c6d7a4e1f8b784c14876e5174;
float4 Color_dc69a74bd5704ea796654354ea94b8d9;
float Vector1_4f1d2b1f25a248f3923ab327466f2485;
CBUFFER_END

// Object and Global properties
SAMPLER(SamplerState_Linear_Repeat);
TEXTURE2D(Texture2D_818cca92a18946a3b27ffbaf1b40a062);
SAMPLER(samplerTexture2D_818cca92a18946a3b27ffbaf1b40a062);
TEXTURE2D(Texture2D_3f2131083fc44d9b989206ab02babd86);
SAMPLER(samplerTexture2D_3f2131083fc44d9b989206ab02babd86);
TEXTURE2D(Texture2D_b01d9e81c8e04473b8880e3878694e5e);
SAMPLER(samplerTexture2D_b01d9e81c8e04473b8880e3878694e5e);

// Graph Functions

void Unity_Multiply_float(float2 A, float2 B, out float2 Out)
{
    Out = A * B;
}

void Unity_Rotate_Radians_float(float2 UV, float2 Center, float Rotation, out float2 Out)
{
    //rotation matrix
    UV -= Center;
    float s = sin(Rotation);
    float c = cos(Rotation);

    //center rotation matrix
    float2x2 rMatrix = float2x2(c, -s, s, c);
    rMatrix *= 0.5;
    rMatrix += 0.5;
    rMatrix = rMatrix * 2 - 1;

    //multiply the UVs by the rotation matrix
    UV.xy = mul(UV.xy, rMatrix);
    UV += Center;

    Out = UV;
}

void Unity_Multiply_float(float4 A, float4 B, out float4 Out)
{
    Out = A * B;
}

void Unity_SceneDepth_Eye_float(float4 UV, out float Out)
{
    Out = LinearEyeDepth(SHADERGRAPH_SAMPLE_SCENE_DEPTH(UV.xy), _ZBufferParams);
}

void Unity_Subtract_float(float A, float B, out float Out)
{
    Out = A - B;
}

void Unity_Divide_float(float A, float B, out float Out)
{
    Out = A / B;
}

void Unity_Saturate_float(float In, out float Out)
{
    Out = saturate(In);
}

// Graph Vertex
struct VertexDescription
{
    float3 Position;
    float3 Normal;
    float3 Tangent;
};

VertexDescription VertexDescriptionFunction(VertexDescriptionInputs IN)
{
    VertexDescription description = (VertexDescription)0;
    description.Position = IN.ObjectSpacePosition;
    description.Normal = IN.ObjectSpaceNormal;
    description.Tangent = IN.ObjectSpaceTangent;
    return description;
}

// Graph Pixel
struct SurfaceDescription
{
    float3 BaseColor;
    float3 Emission;
    float Alpha;
    float AlphaClipThreshold;
};

SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
{
    SurfaceDescription surface = (SurfaceDescription)0;
    UnityTexture2D _Property_c13bab846ff3488187905dd48c26755a_Out_0 = UnityBuildTexture2DStructNoScale(Texture2D_818cca92a18946a3b27ffbaf1b40a062);
    float _Split_a0d1d32f695a4ceabaf3057615b92541_R_1 = IN.AbsoluteWorldSpacePosition[0];
    float _Split_a0d1d32f695a4ceabaf3057615b92541_G_2 = IN.AbsoluteWorldSpacePosition[1];
    float _Split_a0d1d32f695a4ceabaf3057615b92541_B_3 = IN.AbsoluteWorldSpacePosition[2];
    float _Split_a0d1d32f695a4ceabaf3057615b92541_A_4 = 0;
    float2 _Vector2_f84baf3fcaab45edb30625c920fb91bd_Out_0 = float2(_Split_a0d1d32f695a4ceabaf3057615b92541_R_1, _Split_a0d1d32f695a4ceabaf3057615b92541_B_3);
    float _Property_505300b000564f4fbe41a0d1fb1dd9bb_Out_0 = Vector1_c5686f27abfe4d5081a5b5eee32afda8;
    float2 _Multiply_adf8c2949a394a748dcbaf9bbecc58b5_Out_2;
    Unity_Multiply_float(_Vector2_f84baf3fcaab45edb30625c920fb91bd_Out_0, (_Property_505300b000564f4fbe41a0d1fb1dd9bb_Out_0.xx), _Multiply_adf8c2949a394a748dcbaf9bbecc58b5_Out_2);
    float2 _Vector2_6df102a229904a54a01b4e43c9924cce_Out_0 = float2(0.5, 0.5);
    float _Property_25c7c338c2a340f99397839c68ce01bf_Out_0 = Vector1_9ec3ae0c6d7a4e1f8b784c14876e5174;
    float2 _Rotate_e74144c0506c44c79764e30df1dcc15c_Out_3;
    Unity_Rotate_Radians_float(_Multiply_adf8c2949a394a748dcbaf9bbecc58b5_Out_2, _Vector2_6df102a229904a54a01b4e43c9924cce_Out_0, _Property_25c7c338c2a340f99397839c68ce01bf_Out_0, _Rotate_e74144c0506c44c79764e30df1dcc15c_Out_3);
    float4 _SampleTexture2D_6958146740004e6383c135b2c28476d4_RGBA_0 = SAMPLE_TEXTURE2D(_Property_c13bab846ff3488187905dd48c26755a_Out_0.tex, _Property_c13bab846ff3488187905dd48c26755a_Out_0.samplerstate, _Rotate_e74144c0506c44c79764e30df1dcc15c_Out_3);
    float _SampleTexture2D_6958146740004e6383c135b2c28476d4_R_4 = _SampleTexture2D_6958146740004e6383c135b2c28476d4_RGBA_0.r;
    float _SampleTexture2D_6958146740004e6383c135b2c28476d4_G_5 = _SampleTexture2D_6958146740004e6383c135b2c28476d4_RGBA_0.g;
    float _SampleTexture2D_6958146740004e6383c135b2c28476d4_B_6 = _SampleTexture2D_6958146740004e6383c135b2c28476d4_RGBA_0.b;
    float _SampleTexture2D_6958146740004e6383c135b2c28476d4_A_7 = _SampleTexture2D_6958146740004e6383c135b2c28476d4_RGBA_0.a;
    float4 _Property_13e483cd51b8490b9a44512c4f249ccb_Out_0 = Color_dc69a74bd5704ea796654354ea94b8d9;
    float4 _Multiply_cb02d0721341452692ce3355b85e3b79_Out_2;
    Unity_Multiply_float(_SampleTexture2D_6958146740004e6383c135b2c28476d4_RGBA_0, _Property_13e483cd51b8490b9a44512c4f249ccb_Out_0, _Multiply_cb02d0721341452692ce3355b85e3b79_Out_2);
    float _SceneDepth_4b924296a5444c71987b488d105a7c45_Out_1;
    Unity_SceneDepth_Eye_float(float4(IN.ScreenPosition.xy / IN.ScreenPosition.w, 0, 0), _SceneDepth_4b924296a5444c71987b488d105a7c45_Out_1);
    float4 _ScreenPosition_088c919562d7448b8f64e5e7c85bef8f_Out_0 = IN.ScreenPosition;
    float _Split_715c1865a284495abb42c53b6db4bb77_R_1 = _ScreenPosition_088c919562d7448b8f64e5e7c85bef8f_Out_0[0];
    float _Split_715c1865a284495abb42c53b6db4bb77_G_2 = _ScreenPosition_088c919562d7448b8f64e5e7c85bef8f_Out_0[1];
    float _Split_715c1865a284495abb42c53b6db4bb77_B_3 = _ScreenPosition_088c919562d7448b8f64e5e7c85bef8f_Out_0[2];
    float _Split_715c1865a284495abb42c53b6db4bb77_A_4 = _ScreenPosition_088c919562d7448b8f64e5e7c85bef8f_Out_0[3];
    float _Subtract_94c5a0cdecd3465c8ebce61a76126ae9_Out_2;
    Unity_Subtract_float(_SceneDepth_4b924296a5444c71987b488d105a7c45_Out_1, _Split_715c1865a284495abb42c53b6db4bb77_A_4, _Subtract_94c5a0cdecd3465c8ebce61a76126ae9_Out_2);
    float _Property_c2fb2ab4753f4a67b952b67243833c4a_Out_0 = Vector1_4f1d2b1f25a248f3923ab327466f2485;
    float _Divide_6b505606b9094bd6890b7e45cb0eb021_Out_2;
    Unity_Divide_float(_Subtract_94c5a0cdecd3465c8ebce61a76126ae9_Out_2, _Property_c2fb2ab4753f4a67b952b67243833c4a_Out_0, _Divide_6b505606b9094bd6890b7e45cb0eb021_Out_2);
    float _Saturate_39416b919e4d4793892c252123310234_Out_1;
    Unity_Saturate_float(_Divide_6b505606b9094bd6890b7e45cb0eb021_Out_2, _Saturate_39416b919e4d4793892c252123310234_Out_1);
    surface.BaseColor = (_Multiply_cb02d0721341452692ce3355b85e3b79_Out_2.xyz);
    surface.Emission = float3(0, 0, 0);
    surface.Alpha = _Saturate_39416b919e4d4793892c252123310234_Out_1;
    surface.AlphaClipThreshold = 0.1;
    return surface;
}

// --------------------------------------------------
// Build Graph Inputs

VertexDescriptionInputs BuildVertexDescriptionInputs(Attributes input)
{
    VertexDescriptionInputs output;
    ZERO_INITIALIZE(VertexDescriptionInputs, output);

    output.ObjectSpaceNormal = input.normalOS;
    output.ObjectSpaceTangent = input.tangentOS.xyz;
    output.ObjectSpacePosition = input.positionOS;

    return output;
}
    SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
{
    SurfaceDescriptionInputs output;
    ZERO_INITIALIZE(SurfaceDescriptionInputs, output);





    output.WorldSpacePosition = input.positionWS;
    output.AbsoluteWorldSpacePosition = GetAbsolutePositionWS(input.positionWS);
    output.ScreenPosition = ComputeScreenPos(TransformWorldToHClip(input.positionWS), _ProjectionParams.x);
#if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
#define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN output.FaceSign =                    IS_FRONT_VFACE(input.cullFace, true, false);
#else
#define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
#endif
#undef BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN

    return output;
}

    // --------------------------------------------------
    // Main

    #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"
#include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/Varyings.hlsl"
#include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/LightingMetaPass.hlsl"

    ENDHLSL
}
Pass
{
        // Name: <None>
        Tags
        {
            "LightMode" = "Universal2D"
        }

        // Render State
        Cull Back
    Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha
    ZTest LEqual
    ZWrite Off

        // Debug
        // <None>

        // --------------------------------------------------
        // Pass

        HLSLPROGRAM

        // Pragmas
        #pragma target 2.0
    #pragma only_renderers gles gles3 glcore d3d11
    #pragma multi_compile_instancing
    #pragma vertex vert
    #pragma fragment frag

        // DotsInstancingOptions: <None>
        // HybridV1InjectedBuiltinProperties: <None>

        // Keywords
        // PassKeywords: <None>
        // GraphKeywords: <None>

        // Defines
        #define _SURFACE_TYPE_TRANSPARENT 1
        #define _AlphaClip 1
        #define _NORMALMAP 1
        #define _NORMAL_DROPOFF_TS 1
        #define ATTRIBUTES_NEED_NORMAL
        #define ATTRIBUTES_NEED_TANGENT
        #define VARYINGS_NEED_POSITION_WS
        #define FEATURES_GRAPH_VERTEX
        /* WARNING: $splice Could not find named fragment 'PassInstancing' */
        #define SHADERPASS SHADERPASS_2D
    #define REQUIRE_DEPTH_TEXTURE
        /* WARNING: $splice Could not find named fragment 'DotsInstancingVars' */

        // Includes
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
    #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
    #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"

        // --------------------------------------------------
        // Structs and Packing

        struct Attributes
    {
        float3 positionOS : POSITION;
        float3 normalOS : NORMAL;
        float4 tangentOS : TANGENT;
        #if UNITY_ANY_INSTANCING_ENABLED
        uint instanceID : INSTANCEID_SEMANTIC;
        #endif
    };
    struct Varyings
    {
        float4 positionCS : SV_POSITION;
        float3 positionWS;
        #if UNITY_ANY_INSTANCING_ENABLED
        uint instanceID : CUSTOM_INSTANCE_ID;
        #endif
        #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
        uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
        #endif
        #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
        uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
        #endif
        #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
        FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
        #endif
    };
    struct SurfaceDescriptionInputs
    {
        float3 WorldSpacePosition;
        float3 AbsoluteWorldSpacePosition;
        float4 ScreenPosition;
    };
    struct VertexDescriptionInputs
    {
        float3 ObjectSpaceNormal;
        float3 ObjectSpaceTangent;
        float3 ObjectSpacePosition;
    };
    struct PackedVaryings
    {
        float4 positionCS : SV_POSITION;
        float3 interp0 : TEXCOORD0;
        #if UNITY_ANY_INSTANCING_ENABLED
        uint instanceID : CUSTOM_INSTANCE_ID;
        #endif
        #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
        uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
        #endif
        #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
        uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
        #endif
        #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
        FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
        #endif
    };

        PackedVaryings PackVaryings(Varyings input)
    {
        PackedVaryings output;
        output.positionCS = input.positionCS;
        output.interp0.xyz = input.positionWS;
        #if UNITY_ANY_INSTANCING_ENABLED
        output.instanceID = input.instanceID;
        #endif
        #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
        output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
        #endif
        #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
        output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
        #endif
        #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
        output.cullFace = input.cullFace;
        #endif
        return output;
    }
    Varyings UnpackVaryings(PackedVaryings input)
    {
        Varyings output;
        output.positionCS = input.positionCS;
        output.positionWS = input.interp0.xyz;
        #if UNITY_ANY_INSTANCING_ENABLED
        output.instanceID = input.instanceID;
        #endif
        #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
        output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
        #endif
        #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
        output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
        #endif
        #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
        output.cullFace = input.cullFace;
        #endif
        return output;
    }

    // --------------------------------------------------
    // Graph

    // Graph Properties
    CBUFFER_START(UnityPerMaterial)
float4 Texture2D_818cca92a18946a3b27ffbaf1b40a062_TexelSize;
float4 Texture2D_3f2131083fc44d9b989206ab02babd86_TexelSize;
float4 Texture2D_b01d9e81c8e04473b8880e3878694e5e_TexelSize;
float Vector1_accfafba726746c885b8e27280080a86;
float Vector1_c5686f27abfe4d5081a5b5eee32afda8;
float Vector1_9ec3ae0c6d7a4e1f8b784c14876e5174;
float4 Color_dc69a74bd5704ea796654354ea94b8d9;
float Vector1_4f1d2b1f25a248f3923ab327466f2485;
CBUFFER_END

// Object and Global properties
SAMPLER(SamplerState_Linear_Repeat);
TEXTURE2D(Texture2D_818cca92a18946a3b27ffbaf1b40a062);
SAMPLER(samplerTexture2D_818cca92a18946a3b27ffbaf1b40a062);
TEXTURE2D(Texture2D_3f2131083fc44d9b989206ab02babd86);
SAMPLER(samplerTexture2D_3f2131083fc44d9b989206ab02babd86);
TEXTURE2D(Texture2D_b01d9e81c8e04473b8880e3878694e5e);
SAMPLER(samplerTexture2D_b01d9e81c8e04473b8880e3878694e5e);

// Graph Functions

void Unity_Multiply_float(float2 A, float2 B, out float2 Out)
{
    Out = A * B;
}

void Unity_Rotate_Radians_float(float2 UV, float2 Center, float Rotation, out float2 Out)
{
    //rotation matrix
    UV -= Center;
    float s = sin(Rotation);
    float c = cos(Rotation);

    //center rotation matrix
    float2x2 rMatrix = float2x2(c, -s, s, c);
    rMatrix *= 0.5;
    rMatrix += 0.5;
    rMatrix = rMatrix * 2 - 1;

    //multiply the UVs by the rotation matrix
    UV.xy = mul(UV.xy, rMatrix);
    UV += Center;

    Out = UV;
}

void Unity_Multiply_float(float4 A, float4 B, out float4 Out)
{
    Out = A * B;
}

void Unity_SceneDepth_Eye_float(float4 UV, out float Out)
{
    Out = LinearEyeDepth(SHADERGRAPH_SAMPLE_SCENE_DEPTH(UV.xy), _ZBufferParams);
}

void Unity_Subtract_float(float A, float B, out float Out)
{
    Out = A - B;
}

void Unity_Divide_float(float A, float B, out float Out)
{
    Out = A / B;
}

void Unity_Saturate_float(float In, out float Out)
{
    Out = saturate(In);
}

// Graph Vertex
struct VertexDescription
{
    float3 Position;
    float3 Normal;
    float3 Tangent;
};

VertexDescription VertexDescriptionFunction(VertexDescriptionInputs IN)
{
    VertexDescription description = (VertexDescription)0;
    description.Position = IN.ObjectSpacePosition;
    description.Normal = IN.ObjectSpaceNormal;
    description.Tangent = IN.ObjectSpaceTangent;
    return description;
}

// Graph Pixel
struct SurfaceDescription
{
    float3 BaseColor;
    float Alpha;
    float AlphaClipThreshold;
};

SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
{
    SurfaceDescription surface = (SurfaceDescription)0;
    UnityTexture2D _Property_c13bab846ff3488187905dd48c26755a_Out_0 = UnityBuildTexture2DStructNoScale(Texture2D_818cca92a18946a3b27ffbaf1b40a062);
    float _Split_a0d1d32f695a4ceabaf3057615b92541_R_1 = IN.AbsoluteWorldSpacePosition[0];
    float _Split_a0d1d32f695a4ceabaf3057615b92541_G_2 = IN.AbsoluteWorldSpacePosition[1];
    float _Split_a0d1d32f695a4ceabaf3057615b92541_B_3 = IN.AbsoluteWorldSpacePosition[2];
    float _Split_a0d1d32f695a4ceabaf3057615b92541_A_4 = 0;
    float2 _Vector2_f84baf3fcaab45edb30625c920fb91bd_Out_0 = float2(_Split_a0d1d32f695a4ceabaf3057615b92541_R_1, _Split_a0d1d32f695a4ceabaf3057615b92541_B_3);
    float _Property_505300b000564f4fbe41a0d1fb1dd9bb_Out_0 = Vector1_c5686f27abfe4d5081a5b5eee32afda8;
    float2 _Multiply_adf8c2949a394a748dcbaf9bbecc58b5_Out_2;
    Unity_Multiply_float(_Vector2_f84baf3fcaab45edb30625c920fb91bd_Out_0, (_Property_505300b000564f4fbe41a0d1fb1dd9bb_Out_0.xx), _Multiply_adf8c2949a394a748dcbaf9bbecc58b5_Out_2);
    float2 _Vector2_6df102a229904a54a01b4e43c9924cce_Out_0 = float2(0.5, 0.5);
    float _Property_25c7c338c2a340f99397839c68ce01bf_Out_0 = Vector1_9ec3ae0c6d7a4e1f8b784c14876e5174;
    float2 _Rotate_e74144c0506c44c79764e30df1dcc15c_Out_3;
    Unity_Rotate_Radians_float(_Multiply_adf8c2949a394a748dcbaf9bbecc58b5_Out_2, _Vector2_6df102a229904a54a01b4e43c9924cce_Out_0, _Property_25c7c338c2a340f99397839c68ce01bf_Out_0, _Rotate_e74144c0506c44c79764e30df1dcc15c_Out_3);
    float4 _SampleTexture2D_6958146740004e6383c135b2c28476d4_RGBA_0 = SAMPLE_TEXTURE2D(_Property_c13bab846ff3488187905dd48c26755a_Out_0.tex, _Property_c13bab846ff3488187905dd48c26755a_Out_0.samplerstate, _Rotate_e74144c0506c44c79764e30df1dcc15c_Out_3);
    float _SampleTexture2D_6958146740004e6383c135b2c28476d4_R_4 = _SampleTexture2D_6958146740004e6383c135b2c28476d4_RGBA_0.r;
    float _SampleTexture2D_6958146740004e6383c135b2c28476d4_G_5 = _SampleTexture2D_6958146740004e6383c135b2c28476d4_RGBA_0.g;
    float _SampleTexture2D_6958146740004e6383c135b2c28476d4_B_6 = _SampleTexture2D_6958146740004e6383c135b2c28476d4_RGBA_0.b;
    float _SampleTexture2D_6958146740004e6383c135b2c28476d4_A_7 = _SampleTexture2D_6958146740004e6383c135b2c28476d4_RGBA_0.a;
    float4 _Property_13e483cd51b8490b9a44512c4f249ccb_Out_0 = Color_dc69a74bd5704ea796654354ea94b8d9;
    float4 _Multiply_cb02d0721341452692ce3355b85e3b79_Out_2;
    Unity_Multiply_float(_SampleTexture2D_6958146740004e6383c135b2c28476d4_RGBA_0, _Property_13e483cd51b8490b9a44512c4f249ccb_Out_0, _Multiply_cb02d0721341452692ce3355b85e3b79_Out_2);
    float _SceneDepth_4b924296a5444c71987b488d105a7c45_Out_1;
    Unity_SceneDepth_Eye_float(float4(IN.ScreenPosition.xy / IN.ScreenPosition.w, 0, 0), _SceneDepth_4b924296a5444c71987b488d105a7c45_Out_1);
    float4 _ScreenPosition_088c919562d7448b8f64e5e7c85bef8f_Out_0 = IN.ScreenPosition;
    float _Split_715c1865a284495abb42c53b6db4bb77_R_1 = _ScreenPosition_088c919562d7448b8f64e5e7c85bef8f_Out_0[0];
    float _Split_715c1865a284495abb42c53b6db4bb77_G_2 = _ScreenPosition_088c919562d7448b8f64e5e7c85bef8f_Out_0[1];
    float _Split_715c1865a284495abb42c53b6db4bb77_B_3 = _ScreenPosition_088c919562d7448b8f64e5e7c85bef8f_Out_0[2];
    float _Split_715c1865a284495abb42c53b6db4bb77_A_4 = _ScreenPosition_088c919562d7448b8f64e5e7c85bef8f_Out_0[3];
    float _Subtract_94c5a0cdecd3465c8ebce61a76126ae9_Out_2;
    Unity_Subtract_float(_SceneDepth_4b924296a5444c71987b488d105a7c45_Out_1, _Split_715c1865a284495abb42c53b6db4bb77_A_4, _Subtract_94c5a0cdecd3465c8ebce61a76126ae9_Out_2);
    float _Property_c2fb2ab4753f4a67b952b67243833c4a_Out_0 = Vector1_4f1d2b1f25a248f3923ab327466f2485;
    float _Divide_6b505606b9094bd6890b7e45cb0eb021_Out_2;
    Unity_Divide_float(_Subtract_94c5a0cdecd3465c8ebce61a76126ae9_Out_2, _Property_c2fb2ab4753f4a67b952b67243833c4a_Out_0, _Divide_6b505606b9094bd6890b7e45cb0eb021_Out_2);
    float _Saturate_39416b919e4d4793892c252123310234_Out_1;
    Unity_Saturate_float(_Divide_6b505606b9094bd6890b7e45cb0eb021_Out_2, _Saturate_39416b919e4d4793892c252123310234_Out_1);
    surface.BaseColor = (_Multiply_cb02d0721341452692ce3355b85e3b79_Out_2.xyz);
    surface.Alpha = _Saturate_39416b919e4d4793892c252123310234_Out_1;
    surface.AlphaClipThreshold = 0.1;
    return surface;
}

// --------------------------------------------------
// Build Graph Inputs

VertexDescriptionInputs BuildVertexDescriptionInputs(Attributes input)
{
    VertexDescriptionInputs output;
    ZERO_INITIALIZE(VertexDescriptionInputs, output);

    output.ObjectSpaceNormal = input.normalOS;
    output.ObjectSpaceTangent = input.tangentOS.xyz;
    output.ObjectSpacePosition = input.positionOS;

    return output;
}
    SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
{
    SurfaceDescriptionInputs output;
    ZERO_INITIALIZE(SurfaceDescriptionInputs, output);





    output.WorldSpacePosition = input.positionWS;
    output.AbsoluteWorldSpacePosition = GetAbsolutePositionWS(input.positionWS);
    output.ScreenPosition = ComputeScreenPos(TransformWorldToHClip(input.positionWS), _ProjectionParams.x);
#if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
#define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN output.FaceSign =                    IS_FRONT_VFACE(input.cullFace, true, false);
#else
#define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
#endif
#undef BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN

    return output;
}

    // --------------------------------------------------
    // Main

    #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"
#include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/Varyings.hlsl"
#include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/PBR2DPass.hlsl"

    ENDHLSL
}
    }
        CustomEditor "ShaderGraph.PBRMasterGUI"
        FallBack "Hidden/Shader Graph/FallbackError"
}