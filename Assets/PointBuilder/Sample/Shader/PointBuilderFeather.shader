Shader "PointBuilder/Feather"
{
    Properties
    {
        [HDR]_Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
		_Scale("Scale", Float) = 1
		_NormalOffset("NormalOffset", Float) = 0
		_Adjust("Adjust", Float) = 1
		[Toggle]_UseWind ("UseWind", int) = 0 // TODO to ifdef Toggle(FUGA_ENABLE)
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

		float rand(float2 co)
		{
			return frac(sin(dot(co.xy ,float2(12.9898,78.233))) * 43758.5453);
		}

		ENDCG
		
        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard addshadow fullforwardshadows vertex:vert
        #pragma multi_compile_instancing
        #pragma instancing_options procedural:setup
        #pragma target 3.0
		

        struct Input
        {
            float2 uv_MainTex;
        };

        struct PointData
        {
			float3 Position;
			float3 Normals;
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
		float _Adjust;
		int _UseWind;

		#ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
			StructuredBuffer<PointData> _PointDataBuffer;
		#endif

        void setup()
        {
        }
		
		float3 CalcSelfPosition(float3 pos, float4x4 mat, float4 q)
		{
			float3 t = mat._14_24_34;
			float3 s = float3(
				length(mat._11_21_31),
				length(mat._12_22_32),
				length(mat._13_23_33)
			);
			
			return rotate_vector(pos*s, q) + t;
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

			float4x4 object2world = (float4x4)0; 
			float3 scl = _Scale;
			object2world._11_22_33_44 = float4(scl, 1.0);

			float4 q1 = q_look_at(normal, float3(0, 1, 0));
			float4 q2 = euler_to_quaternion(_Euler);
			float4 q = qmul(q1, q2);
			float4x4 rotMatrix = quaternion_to_matrix(q);
			object2world = mul(rotMatrix, object2world);
			object2world._14_24_34 += position + normal * _NormalOffset;

			vert = mul(object2world, vert);
			
			float dist = distance(vert, position);
			float3 downUnit = float3(0, -1, 0);

			// 法線と下方向ベクトルから回転軸(横)ベクトルを計算
			float3 rotAxis = cross(normal, downUnit);

			// ■長さランダム
			float rnd = rand(float2(unity_InstanceID,unity_InstanceID));
			vert.xyz += dist * normal * rnd;
			
			// ■風
			// https://spphire9.wordpress.com/2012/11/20/%E9%A0%82%E7%82%B9%E3%82%B7%E3%82%A7%E3%83%BC%E3%83%80%E3%81%A7%E8%8D%89%E3%82%92%E6%8F%BA%E3%82%89%E3%81%99/
			if(_UseWind == 1)
			{
				float3 windDir = normalize(float3(0.2, 0.0, -0.1));
				float3 windForce = float3(0.2, 0.0, -0.1);
				vert.xyz += dist * windForce * sin(dot(vert.xz, windDir.xz) + _Time.z) * vert.y;
			}

			// ■距離に応じて角度をつける
			float3 diffv = vert - position;
			float rot = pow(dist,0.5) * _Adjust;
			v.vertex.xyz = rotate_quaternion_axis(rotAxis, rot, position, vert.xyz);
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
