using UnityEditor;
using UnityEngine;
using static LighadEngine.PBSShaderGUI.PBSShaderGUIBase;
/*
 * ������      Shader������      Shader��������                ˵��
 *          �꣺_EmissiveEffect       define                   ����  
 *      ��  ����_EffectTex             2D                    Noise��ͼ
 * �رգ�
 *          �꣺_EmissiveEffec        define
 */
namespace LighadEngine.PBSShaderGUI
{
    public class EmissiveEffect : EffectGUIBase
    {
        [InitializeOnLoadMethod]
        static void RegisterType()
        {
            EffectTypes.RegisterBatcherType(typeof(EmissiveEffect));
        }

        struct PropertyNames
        {
            public static readonly string SpeedXYFresnelEmission = "_SpeedXYFresnelEmission";
        }

        private SharedEffect m_Shared;
        //��������ֵ
        SharedEffectMode m_SharedMode = new SharedEffectMode();
        public EmissiveEffect(SharedEffect shared) : base("_EmissiveEffect")
        {
            m_Shared = shared;
            m_PopertieDic.Add(PropertyNames.SpeedXYFresnelEmission, new Popertie("Speed XY + Fresnel + Emission", "Speed:xy, ���������:z����ǿ:w", PropertyNames.SpeedXYFresnelEmission));
        }

        public override void DrawEffectsOptions(MaterialEditor materialEditor, int indentLevel)
        {
            m_SharedMode.Tex = (Texture2D)EditorGUILayout.ObjectField("Noise", m_SharedMode.Tex, typeof(Texture2D), false, GUILayout.ExpandWidth(true));
            m_SharedMode.TexScaleOffest = EditorGUILayout.Vector4Field("XY:Tiling  ZW:Offset", m_SharedMode.TexScaleOffest);
            m_Shared.SetMode(m_SharedMode);

            PBSShaderGUIUtils.ShaderProperty(PropertyNames.SpeedXYFresnelEmission, m_PopertieDic, materialEditor);
        }
    }
}