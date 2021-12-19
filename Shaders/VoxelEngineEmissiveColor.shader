﻿Shader "VoxelEngine/VoxelEngineEmissiveColor"
{
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        #pragma surface surf Lambert vertex:vert
        #pragma target 3.0

        struct Input
        {
            float4 vertColor: Color;
        };

		void vert(inout appdata_full v, out Input o){
			UNITY_INITIALIZE_OUTPUT(Input, o);
			o.vertColor = v.color;
		}

		void surf (Input IN, inout SurfaceOutput o) {
			o.Emission = IN.vertColor.rgb * IN.vertColor.a;
		}
        ENDCG
    }
    FallBack "Diffuse"
}
