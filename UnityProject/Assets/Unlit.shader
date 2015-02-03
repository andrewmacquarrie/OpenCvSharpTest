 Shader "Projector/Unlit"
 {
   // http://answers.unity3d.com/questions/40357/projector-projector-shader-and-cg.html
     Properties
     {
         _ShadowTex ("Cookie", 2D) = "gray" { TexGen ObjectLinear }
          _FalloffTex ("FallOff", 2D) = "white" { TexGen ObjectLinear   }
     }
      
     Subshader
     {
         Tags { "RenderType"="Transparent-1" }
         Pass
         {
             //ZWrite Off
              Fog { Color (1, 1, 1) }
              AlphaTest Less 1
              ColorMask RGB
              //Blend DstColor Zero
              Offset -1, -1
                          
             CGPROGRAM
             #pragma vertex vert
             #pragma fragment frag
             #pragma fragmentoption ARB_fog_exp2
             #pragma fragmentoption ARB_precision_hint_fastest
             #include "UnityCG.cginc"
              
             struct v2f
             {
                 float4 pos : SV_POSITION;
                 float4 uv_Main : TEXCOORD0;
                 float4 uv_Clip : TEXCOORD1;
             };
             
              
             sampler2D _ShadowTex;
             sampler2D _FalloffTex;
             float4x4 _Projector;
             float4x4 _ProjectorClip;
              
             v2f vert(appdata_tan v)
             {
                 v2f o;
                 o.pos = mul (UNITY_MATRIX_MVP, v.vertex);
                 o.uv_Main = mul (_Projector, v.vertex);
                 o.uv_Clip = mul (_ProjectorClip, v.vertex);
                 return o;
             }
              
             half4 frag (v2f i) : COLOR
             {
                 half4 tex = tex2Dproj(_ShadowTex, i.uv_Main);
                 half4 falloff = tex2D(_FalloffTex, i.uv_Clip.xy);
                 tex = lerp(float4(1,1,1,1),tex,falloff.a);
                 return tex;
             }
             ENDCG
      
         }
     }
 }