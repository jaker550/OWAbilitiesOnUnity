Shader "Custom/Silhouette"
{
    Properties
    {
        _Color ("Silhouette Color", Color) = (1,0,0,1) // Default to red
    }
    
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Overlay" }
        
        Pass
        {
            ZWrite On
            ZTest Always
            ColorMask RGB
            Cull Off
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            
            struct appdata
            {
                float4 vertex : POSITION;
            };
            
            struct v2f
            {
                float4 pos : SV_POSITION;
            };

            fixed4 _Color; // Color property

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                return _Color; // Return the color defined in the Inspector
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}
