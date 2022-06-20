Shader "Mya/TestShader"
{
	Properties
	{
		[Foldout(1,1,0,0)]
		_FoldoutWaveBlend("Æß²ÊÁ÷¹â_Foldout", Float) = 1
		_MainTex1("Texture1", 2D) = "white" {}
		_Color1("Color1" , Color) = (1,1,1,1)
		_Float1("Float1" , Float) = 0
		_Range1("Range1" , Range(0,1)) = 0
		_VetorVal1("VectorVal1" , Vector) = (0,0,0,0)

		[Foldout(1,1,0,0)]
		_FoldoutBlend("¹â", Float) = 1

		_MainTex2("Texture2", 2D) = "white" {}
		_Color2("Color2" , Color) = (1,1,1,1)
		_Float2("Float2" , Float) = 0
		_Range2("Range2" , Range(0,1)) = 0
		_VetorVal2("VectorVal2" , Vector) = (0,0,0,0)


	}
		SubShader
		{
			Tags { "RenderType" = "Opaque" }
			LOD 100
			Fog {Mode Off}
			Pass
			{

			}
		}
			CustomEditor "ShaderGUIBase.ShaderGUIBase"
}