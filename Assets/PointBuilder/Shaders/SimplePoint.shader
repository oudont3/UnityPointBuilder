Shader "PointBuilder/Editor/SimplePoint"
{
    Properties {
        _Color ("Color", Color) = (1, 1, 1, 1)
        _Size ("Point Size", Range(1,10)) = 1
    }
    SubShader {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 200
        ZWrite OFF
        ZTest LEqual
        Cull Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass {
            CGPROGRAM

            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            
            struct v2f {
                float4 pos : SV_POSITION;
                float3 worldPos : TEXCOORD0;
				float4 col : COLOR;
				float size : PSIZE;
            };
            
            float4 _Color;
			half _Size;

            v2f vert(appdata_base v)
            {
                v2f o;
				float3 n = UnityObjectToWorldNormal(v.normal);
                o.pos = UnityObjectToClipPos (v.vertex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
				o.col = _Color;
				o.size = _Size;
     
                return o;
            }
            
            half4 frag (v2f i) : COLOR
            {
            	//float4 red  = float4(255.0/255,70.0/255,150.0/255,1);
            	//float4 blue = float4(90.0/255,90.0/255,250.0/255,1);
				//   return lerp(red, blue, i.worldPos.y*0.2);
				return i.col;
            }
            ENDCG
        }
    } 
    FallBack Off
}
