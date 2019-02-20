Shader "PointBuilder/Simple"
{
    Properties
    {
        [HDR]_Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
		_Scale("Scale", Float) = 1
		_NormalOffset("NormalOffset", Float) = 0
    }

    SubShader
    {
		Tags
		{
			"RenderType" = "Opaque"
		}
        Cull Off
        ZWrite On
		
		CGINCLUDE
		#include "Quaternion.cginc"
		
		ENDCG
		
        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard addshadow fullforwardshadows vertex:vert
        #pragma multi_compile_instancing
        #pragma instancing_options procedural:setup

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0
		
        struct PointData
        {
			float3 Position;
			float3 Normals;
        };

        struct Input
        {
            float2 uv_MainTex;
        };
        sampler2D _MainTex;
        half _Glossiness;
        half _Metallic;
        fixed4 _Color;
		
		float4x4 _LocalToWorldMat;
		float3 _Euler;
		float4 _ParentQuaternion;
		uniform float _Scale;
		uniform float _NormalOffset;

		#ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
			StructuredBuffer<PointData> _PointDataBuffer;
		#endif

        void setup()
        {
        }
		
		void vert(inout appdata_full v)
		{
        #ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
			float4 vert = v.vertex;
		
			PointData data = _PointDataBuffer[unity_InstanceID];
			float3 normal = data.Normals;
			float3 position = data.Position;
			position = mul(_LocalToWorldMat, float4(position, 1)).xyz;
			normal = rotate_vector(normal, _ParentQuaternion);
			// normal = float3(0,0,1); // camera forward

			float4x4 object2world = (float4x4)0; 
			float3 scl = _Scale;
			object2world._11_22_33_44 = float4(scl, 1.0);

			float4 q1 = q_look_at(normal, float3(0, 1, 0));
			float4 q2 = euler_to_quaternion(_Euler);
			float4 q = qmul(q1, q2);
			float4x4 rotMatrix = quaternion_to_matrix(q);
			object2world = mul(rotMatrix, object2world);
			object2world._14_24_34 += position + normal * _NormalOffset;

			v.vertex.xyz = mul(object2world, vert);
        #endif

		}

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            // Albedo comes from a texture tinted by color
            fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
            o.Albedo = c.rgb;
            // Metallic and smoothness come from slider variables
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha = c.a;
        }
        ENDCG

    }
    FallBack "Diffuse"
}
