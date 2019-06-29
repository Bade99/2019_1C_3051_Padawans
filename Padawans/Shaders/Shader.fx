// ---------------------------------------------------------
// Sombras en el image space con la tecnica de Shadows Map
// ---------------------------------------------------------

/**************************************************************************************/
/* Variables comunes */
/**************************************************************************************/

//Matrices de transformacion
float4x4 matWorld; //Matriz de transformacion World
float4x4 matWorldView; //Matriz World * View
float4x4 matWorldViewProj; //Matriz World * View * Projection
float4x4 matInverseTransposeWorld; //Matriz Transpose(Invert(World))

//Textura para DiffuseMap
texture texDiffuseMap;
sampler2D diffuseMap = sampler_state
{
    Texture = (texDiffuseMap);
    ADDRESSU = WRAP;
    ADDRESSV = WRAP;
    MINFILTER = LINEAR;
    MAGFILTER = LINEAR;
    MIPFILTER = LINEAR;
};

#define SMAP_SIZE 1024
#define EPSILON 0.05f

float4x4 g_mViewLightProj;
float4x4 g_mProjLight;
float3 g_vLightPos; // posicion de la luz (en World Space) = pto que representa patch emisor Bj
float3 g_vLightDir; // Direcion de la luz (en World Space) = normal al patch Bj

texture g_txShadow; // textura para el shadow map
sampler2D g_samShadow =
sampler_state
{
    Texture = <g_txShadow>;
    MinFilter = Point;
    MagFilter = Point;
    MipFilter = Point;
    AddressU = Clamp;
    AddressV = Clamp;
};



//Output del Vertex Shader
struct VS_OUTPUT
{
    float4 Position : POSITION0;
    float2 Texcoord : TEXCOORD0;
    float3 Norm : TEXCOORD1; // Normales
    float3 Pos : TEXCOORD2; // Posicion real 3d
};


//+gameOverShader+++++++++++++

float gameOverTime = 0;
//Input del Vertex Shader
struct VS_INPUT_gameOver
{
	float4 Position : POSITION0;
	float4 Color : COLOR0;
	float2 Texcoord : TEXCOORD0;
};

struct VS_OUTPUT_gameOver
{
	float4 Position :        POSITION0;
	float2 Texcoord :        TEXCOORD0;
	float4 Color :			COLOR0;
	float3 Pos :			TEXCOORD1;
};

//Vertex Shader
VS_OUTPUT_gameOver vs_gameOver(VS_INPUT_gameOver Input)
{
	VS_OUTPUT_gameOver Output;
    Output.Position = mul(Input.Position, matWorldViewProj);
    Output.Pos = Input.Position;
    Output.Color = Input.Color;
    Output.Texcoord = Input.Texcoord;
	return(Output);
}

//Pixel Shader
float4 ps_gameOver(float2 Texcoord: TEXCOORD0, float3 Pos : TEXCOORD1) : COLOR0
{
	float4 coordenada = float4(tex2D(diffuseMap, Texcoord));
	coordenada.r -= 4*gameOverTime + cos(Pos.xyz) - 2;
	coordenada.g -= 4*gameOverTime + cos(Pos.xyz) - 2;
	coordenada.b -= 4*gameOverTime + cos(Pos.xyz) - 2;
	return coordenada;
}

technique RenderGameOver
{
	pass Pass_0
	{
		VertexShader = compile vs_2_0 vs_gameOver();
		PixelShader = compile ps_2_0 ps_gameOver();
	}
}

//++++++++++++++++++++++++

//-----------------------------------------------------------------------------
// Vertex Shader que implementa un shadow map
//-----------------------------------------------------------------------------
void VertShadow(float4 Pos : POSITION,
	float3 Normal : NORMAL,
	out float4 oPos : POSITION,
	out float2 Depth : TEXCOORD0)
{
	// transformacion estandard
    oPos = mul(Pos, matWorld); // uso el del mesh
    oPos = mul(oPos, g_mViewLightProj); // pero visto desde la pos. de la luz

	// devuelvo: profundidad = z/w
    Depth.xy = oPos.zw;
}

//-----------------------------------------------------------------------------
// Pixel Shader para el shadow map, dibuja la "profundidad"
//-----------------------------------------------------------------------------
void PixShadow(float2 Depth : TEXCOORD0, out float4 Color : COLOR)
{
	// parche para ver el shadow map
	//float k = Depth.x/Depth.y;
	//Color = (1-k);
    Color = Depth.x / Depth.y;
}

technique RenderShadow
{
    pass p0
    {
        VertexShader = compile vs_3_0 VertShadow();
        PixelShader = compile ps_3_0 PixShadow();
    }
}

//-----------------------------------------------------------------------------
// Vertex Shader para dibujar la escena pp dicha con sombras
//-----------------------------------------------------------------------------
void VertScene(float4 iPos : POSITION,
	float2 iTex : TEXCOORD0,
	float3 iNormal : NORMAL,
	out float4 oPos : POSITION,
	out float2 Tex : TEXCOORD0,
	out float4 vPos : TEXCOORD1,
	out float3 vNormal : TEXCOORD2,
	out float4 vPosLight : TEXCOORD3
)
{
	// transformo al screen space
    oPos = mul(iPos, matWorldViewProj);

	// propago coordenadas de textura
    Tex = iTex;

	// propago la normal
    vNormal = mul(iNormal, (float3x3) matWorldView);

	// propago la posicion del vertice en World space
    vPos = mul(iPos, matWorld);
	// propago la posicion del vertice en el espacio de proyeccion de la luz
    vPosLight = mul(vPos, g_mViewLightProj);
}

//-----------------------------------------------------------------------------
// Pixel Shader para dibujar la escena
//-----------------------------------------------------------------------------
float4 PixScene(float2 Tex : TEXCOORD0,
	float4 vPos : TEXCOORD1,
	float3 vNormal : TEXCOORD2,
	float4 vPosLight : TEXCOORD3
) : COLOR
{
    float3 vLight = normalize(float3(vPos - g_vLightPos));
    float cono = dot(vLight, g_vLightDir);
    float4 K = 0.0;
    if (cono > 0.7)
    {
		// coordenada de textura CT
        float2 CT = 0.5 * vPosLight.xy / vPosLight.w + float2(0.5, 0.5);
        CT.y = 1.0f - CT.y;

		// sin ningun aa. conviene con smap size >= 512
        float I = (tex2D(g_samShadow, CT) + EPSILON < vPosLight.z / vPosLight.w) ? 0.0f : 1.0f;

		// interpolacion standard bi-lineal del shadow map
		// CT va de 0 a 1, lo multiplico x el tamaño de la textura
		// la parte fraccionaria indica cuanto tengo que tomar del vecino
		// conviene cuando el smap size = 256
		// leo 4 valores
		/*float2 vecino = frac( CT*SMAP_SIZE);
		float prof = vPosLight.z / vPosLight.w;
		float s0 = (tex2D( g_samShadow, float2(CT)) + EPSILON < prof)? 0.0f: 1.0f;
		float s1 = (tex2D( g_samShadow, float2(CT) + float2(1.0/SMAP_SIZE,0))
							+ EPSILON < prof)? 0.0f: 1.0f;
		float s2 = (tex2D( g_samShadow, float2(CT) + float2(0,1.0/SMAP_SIZE))
							+ EPSILON < prof)? 0.0f: 1.0f;
		float s3 = (tex2D( g_samShadow, float2(CT) + float2(1.0/SMAP_SIZE,1.0/SMAP_SIZE))
							+ EPSILON < prof)? 0.0f: 1.0f;
		float I = lerp( lerp( s0, s1, vecino.x ),lerp( s2, s3, vecino.x ),vecino.y);
		*/

		/*
		// anti-aliasing del shadow map (tmb genera sombras mas suaves, y artifacts)
		float I = 0;
		float r = 2;
		for(int i=-r;i<=r;++i)
			for(int j=-r;j<=r;++j)
				I += (tex2D( g_samShadow, CT + float2((float)i/SMAP_SIZE, (float)j/SMAP_SIZE) ) + EPSILON < vPosLight.z / vPosLight.w)? 0.0f: 1.0f;
		I /= (2*r+1)*(2*r+1);
		*/

        if (cono < 0.8)
            I *= 1 - (0.8 - cono) * 10;

        K = I;
    }

    float4 color_base = tex2D(diffuseMap, Tex);
    color_base.rgb *= 0.5 + 0.5 * K;
    return color_base;
}

technique RenderScene
{
    pass p0
    {
        VertexShader = compile vs_3_0 VertScene();
        PixelShader = compile ps_3_0 PixScene();
    }
}

//-----------------------------------------------------------------------------
// DYNAMIC ILLUMINATION (DI)
//-----------------------------------------------------------------------------

float3 fvLightPosition;
float3 fvEyePosition;
float k_la; // luz ambiente global
float k_ld; // luz difusa
float k_ls; // luz specular
float fSpecularPower; // exponente de la luz specular

//Input del Vertex Shader
struct VS_INPUT_DI
{
    float4 Position : POSITION0;
    float3 Normal : NORMAL0;
    float4 Color : COLOR;
    float2 Texcoord : TEXCOORD0;
};

//Output del Vertex Shader
struct VS_OUTPUT_DI
{
    float4 Position : POSITION0;
    float2 Texcoord : TEXCOORD0;
    float3 Norm : TEXCOORD1; // Normales
    float3 Pos : TEXCOORD2; // Posicion real 3d
};

//Vertex Shader
VS_OUTPUT_DI vs_DI(VS_INPUT_DI Input)
{
    VS_OUTPUT_DI Output;

   //Proyectar posicion
    Output.Position = mul(Input.Position, matWorldViewProj);
   
   //Propagamos las coordenadas de textura
    Output.Texcoord = Input.Texcoord;

   // Calculo la posicion real (en world space)
    float4 pos_real = mul(Input.Position, matWorld);
   // Y la propago usando las coordenadas de texturas 2 (*)
    Output.Pos = float3(pos_real.x, pos_real.y, pos_real.z);
   
   // Transformo la normal y la normalizo (si la escala no es uniforme usar la inversa Traspta)
   //Output.Norm = normalize(mul(Input.Normal,matInverseTransposeWorld));
    Output.Norm = normalize(mul(Input.Normal, matWorld));
    return (Output);  
}

//Pixel Shader
float4 ps_DI(float3 Texcoord : TEXCOORD0, float3 N : TEXCOORD1,
	float3 Pos : TEXCOORD2) : COLOR0
{
    float ld = 0; // luz difusa
    float le = 0; // luz specular

    N = normalize(N);

	// for(int =0;i<cant_ligths;++i)

	// 1- calculo la luz diffusa
    float3 LD = normalize(fvLightPosition - float3(Pos.x, Pos.y, Pos.z));
    ld += saturate(dot(N, LD)) * k_ld;

	// 2- calcula la reflexion specular
    float3 D = normalize(float3(Pos.x, Pos.y, Pos.z) - fvEyePosition);
    float ks = saturate(dot(reflect(LD, N), D));
    ks = pow(ks, fSpecularPower);
    le += ks * k_ls;

	//Obtener el texel de textura
    float4 fvBaseColor = tex2D(diffuseMap, Texcoord);

	// suma luz diffusa, ambiente y especular
    float4 RGBColor = 0;
    RGBColor.rgb = saturate(fvBaseColor * (saturate(k_la + ld)) + le);

	// saturate deja los valores entre [0,1]

    return RGBColor;
}

technique DynamicIllumination
{
    pass Pass_0
    {
        VertexShader = compile vs_3_0 vs_DI();
        PixelShader = compile ps_3_0 ps_DI();
    }
}