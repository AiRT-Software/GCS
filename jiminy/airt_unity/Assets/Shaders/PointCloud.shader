﻿// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/PointCloud" {
//based on http://forum.unity3d.com/threads/176317-Point-Sprite-automatic-texture-coords
    Properties {
        _PointSize("PointSize", Float) = 3

    }
    SubShader {
        Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}
        LOD 200
        Pass {
			Cull Off ZWrite On Blend SrcAlpha OneMinusSrcAlpha
             
            CGPROGRAM
                  
            #pragma exclude_renderers flash
            #pragma vertex vert
            #pragma fragment frag 
			       
            struct appdata {
                float4 pos : POSITION;
                fixed4 color : COLOR;
            };
                     
            struct v2f {
                float4 pos : SV_POSITION;
                float size : PSIZE;
                fixed4 color : COLOR;
                };
            float _PointSize;
     
            v2f vert(appdata v){
                v2f o;
                o.pos = UnityObjectToClipPos(v.pos);
                o.size = _PointSize; 
                o.color = v.color;
				o.color.a = 0.3f;
                return o;
            }
     
            half4 frag(v2f i) : COLOR0
            {
				
				i.color.a = 0.3f;
                return i.color; 
            }

            ENDCG
        }
    } 
}