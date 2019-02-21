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

		float rand(float2 co)
		{
			return frac(sin(dot(co.xy ,float2(12.9898,78.233))) * 43758.5453);
		}
		
		float4x4 eulerAnglesToRotationMatrix(float3 angles)
		{
			float ch = cos(angles.y); float sh = sin(angles.y); // heading
			float ca = cos(angles.z); float sa = sin(angles.z); // attitude
			float cb = cos(angles.x); float sb = sin(angles.x); // bank

			// Ry-Rx-Rz (Yaw Pitch Roll)
			return float4x4(
				ch * ca + sh * sb * sa, -ch * sa + sh * sb * ca, sh * cb, 0,
				cb * sa, cb * ca, -sb, 0,
				-sh * ca + ch * sb * sa, sh * sa + ch * sb * ca, ch * cb, 0,
				0, 0, 0, 1
			);
		}
		
		float3 calcWorldPosition(float4x4 localToWorldMat, float3 wPosition, float3 wEuler)
		{
			float3 p = localToWorldMat._14_24_34;
			float3 s = float3(
				length(localToWorldMat._11_21_31),
				length(localToWorldMat._12_22_32),
				length(localToWorldMat._13_23_33)
			);
			float4x4 mat = 0;
			mat._11_22_33_44 = float4(s, 1.0);
			float4x4 rotMat = eulerAnglesToRotationMatrix(wEuler);
			mat = mul(rotMat, mat);
			mat._14_24_34 += p;
			return mul(mat, float4(wPosition, 1)).xyz;
		}

		float3 rotate_quaternion_axis(float3 n, float rot, float3 center, float3 pos)
		{
			float4 q = 0;
			q.x = cos(rot/2);
			q.y = n.x * sin(rot/2);
			q.z = n.y * sin(rot/2);
			q.w = n.z * sin(rot/2);

			float3x3 r = float3x3(
				q.x*q.x + q.y*q.y - q.z*q.z - q.w*q.w, 2*(q.y*q.z-q.x*q.w), 2*(q.y*q.w + q.x*q.z),
				2*(q.y*q.z+q.x*q.w), q.x*q.x - q.y*q.y + q.z*q.z - q.w*q.w, 2*(q.z*q.w - q.x*q.y),
				2*(q.y*q.w-q.x*q.z), 2*(q.z*q.w+q.x*q.y), q.x*q.x - q.y*q.y - q.z*q.z + q.w*q.w
			);

			return mul(r, pos-center) + center;
		}

		ENDCG
		
        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard addshadow fullforwardshadows vertex:vert
        #pragma multi_compile_instancing
        #pragma instancing_options procedural:setup
        #pragma target 3.0
		
        struct PointData
        {
			float3 Position;
			float3 Normals;
			float3 EulerAngles;
        };
		
        struct Input
        {
            float2 uv_MainTex;
        };
        sampler2D _MainTex;
        half _Glossiness;
        half _Metallic;
        fixed4 _Color;
		
		// world mat
		float4x4 _LocalToWorldMat;
		// world euler
		float4 _ParentEuler;
		// local euler
		float3 _Euler;
		// local scale
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

		void vert(inout appdata_full v)
		{
        #ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
		
			PointData data = _PointDataBuffer[unity_InstanceID];
			float3 parentNormal = data.Normals;
			float3 parentPosition = data.Position;
			float3 eulerAngle = data.EulerAngles;
			
			// parents 
			parentPosition = calcWorldPosition(_LocalToWorldMat, parentPosition, _ParentEuler);
			parentNormal = mul(eulerAnglesToRotationMatrix(_ParentEuler), float4(parentNormal, 1)).xyz;

			// conv matrix
			float4x4 mat = 0; 
			// scale
			float3 scl = _Scale;
			mat._11_22_33_44 = float4(scl, 1.0);
			// rotate
			float4x4 rotMatrix = eulerAnglesToRotationMatrix(eulerAngle + _ParentEuler + _Euler);
			mat = mul(rotMatrix, mat);
			// position
			mat._14_24_34 += parentPosition + parentNormal * _NormalOffset;
			
			v.vertex.xyz = mul(mat, v.vertex).xyz;

			//v.normal = mul(rotMatrix, float4(v.normal, 1));
			
			float4 vert = v.vertex;
			float dist = distance(vert, parentPosition);
			float3 downUnit = float3(0, -1, 0);

			// 法線と下方向ベクトルから回転軸(横)ベクトルを計算
			float3 rotAxis = cross(parentNormal, downUnit);

			// ■長さランダム
			float rnd = rand(float2(unity_InstanceID,unity_InstanceID));
			vert.xyz += dist * parentNormal * rnd;
			
			// ■風
			// https://spphire9.wordpress.com/2012/11/20/%E9%A0%82%E7%82%B9%E3%82%B7%E3%82%A7%E3%83%BC%E3%83%80%E3%81%A7%E8%8D%89%E3%82%92%E6%8F%BA%E3%82%89%E3%81%99/
			if(_UseWind == 1)
			{
				float3 windDir = normalize(float3(0.2, 0.0, -0.1));
				float3 windForce = float3(0.2, 0.0, -0.1);
				vert.xyz += dist * windForce * sin(dot(vert.xz, windDir.xz) + _Time.z) * vert.y;
			}

			// ■距離に応じて角度をつける
			float3 diffv = vert - parentPosition;
			float rot = pow(dist,0.5) * _Adjust;
			v.vertex.xyz = rotate_quaternion_axis(rotAxis, rot, parentPosition, vert.xyz);
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
