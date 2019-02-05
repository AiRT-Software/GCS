// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Airt/TriangleRender"
{
	Properties{
		[Toggle] _Discard("Discard", Float) = 1
	}
		SubShader
	{
		Tags {"RenderType" = "Transparent"}

		//Vertex & Fragment Shaders need to be inside the PASS block
		//The Pass block causes the geometry of a GameObject to be rendered once.
		Pass
		{
			Name "TRIANGLERENDERPASS"

			Cull Off ZWrite On ZTest LEqual

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"
			uniform float _Discard;

			struct vertInput {
				// The position of the vertex in local space (model space) 
				// Using "pos : POSITION" indicates that we want Unity3D to initialised pos with the vertex positions
				float4 pos : POSITION;
				float4 color : COLOR;
			};

			struct vertOutput {
				 // The position of the vertex after being transformed into projection space
				 // "SV_POSITION" typically contains also a Z and W components, used to store the depth (ZTest) and one value for the homogeneous space, respectively
				float4 pos : SV_POSITION;
				fixed4 color : COLOR0;
				float3 WorldPos: COLOR1;
			};

			vertOutput vert(vertInput input){
				vertOutput o;
				//Multiply the vertex position with UNITY_MATRIX_MVP (modelviewprojection matrix)
				//to get the screen coordinates
				o.pos = UnityObjectToClipPos(input.pos);
				//o.WorldPos = mul(unity_ObjectToWorld, input.pos);
				o.WorldPos = mul(unity_ObjectToWorld, input.pos);
				o.color = input.color;	
				return o;
			}

			half4 frag(vertOutput output) : COLOR {
				//TODO: discard fragments based on the angle to the Camera and the distance to the Camera


				
				return output.color;
			}


			ENDCG
		}
	}
	
}
