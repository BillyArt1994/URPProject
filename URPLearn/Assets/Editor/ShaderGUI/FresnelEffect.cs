using UnityEditor;
using UnityEngine;
using static LighadEngine.PBSShaderGUI.PBSShaderGUIBase;
/*
 * ������      Shader������      Shader��������                ˵��
 *          �꣺_FRESNEL_EDGE         define                 ����fresnel  ������ͷ���ͬʱֻ��һ��������
 *              _FRESNEL_REVERSE      define                 ����fresnel
 *      ��  ����_FresnelPower          float                 ���������ƽ��
 *              _FresnelIntensity      float                 ǿ��
 *              _FadeFact            Randge(0,1)             ͸����
 * �رգ�
 *          �꣺_FRESNEL_EDGE         define
 *              _FRESNEL_REVERSE      define
 */
namespace LighadEngine.PBSShaderGUI
{
    public class FresnelEffect : EffectGUIBase
    {
        [InitializeOnLoadMethod]
        static void RegisterType()
        {
            EffectTypes.RegisterBatcherType(typeof(FresnelEffect));
        }
        public enum FresnelMode
        {
            //None,
            Fresnel_Edge = 1,
            FresnelMode_Reverse,
        }
        public struct PropertyNames
        {
            public static readonly string Fresnel = "_Fresnel";
            public static readonly string FresnelPower = "_FresnelPower";
            public static readonly string FresnelIntensity = "_FresnelIntensity";
            public static readonly string FadeFact = "_FadeFact";
        }
        private SharedEffect m_Shared;
        //��������ֵ
        SharedEffectMode m_SharedMode = new SharedEffectMode();

        bool m_enableFresnelReverse = false;

        GUIContent m_ColorLabel = new GUIContent("Color");
        public FresnelEffect(SharedEffect shared) : base(null)
        {
            m_Shared = shared;
            m_PopertieDic.Add(PropertyNames.Fresnel, new Popertie("Fresnel", "", PropertyNames.Fresnel));
            m_PopertieDic.Add(PropertyNames.FresnelPower, new Popertie("FresnelPower", "", PropertyNames.FresnelPower));
            m_PopertieDic.Add(PropertyNames.FresnelIntensity, new Popertie("FresnelIntensity", "", PropertyNames.FresnelIntensity));
            m_PopertieDic.Add(PropertyNames.FadeFact, new Popertie("FadeFact", "", PropertyNames.FadeFact));
        }

        public override void DrawEffectsOptions(MaterialEditor materialEditor, int indentLevel)
        {
            m_PopertieDic.TryGetValue(PropertyNames.Fresnel, out var fresnel);
            if (fresnel.floatValue == 0) fresnel.floatValue++;

            m_SharedMode.Color = EditorGUILayout.ColorField(m_ColorLabel, m_SharedMode.Color, true, true, true);
            PBSShaderGUIUtils.ShaderProperty(PropertyNames.FresnelPower, m_PopertieDic, materialEditor);
            PBSShaderGUIUtils.ShaderProperty(PropertyNames.FresnelIntensity, m_PopertieDic, materialEditor);
            PBSShaderGUIUtils.ShaderProperty(PropertyNames.FadeFact, m_PopertieDic, materialEditor);

            //���������ѡ�� Ϊ�˷��������ۿ� ��ѡ��ĳ���Toggle��ʽ
            bool bFresnelReverse = fresnel.floatValue == (int)FresnelMode.FresnelMode_Reverse;
            EditorGUI.BeginChangeCheck();
            {
                m_enableFresnelReverse = EditorGUILayout.Toggle("Reverse", bFresnelReverse);
            }
            EditorGUI.EndChangeCheck();
            {
                fresnel.floatValue = m_enableFresnelReverse ? (int)FresnelMode.FresnelMode_Reverse : (int)FresnelMode.Fresnel_Edge;

                Material material = materialEditor.target as Material;
                SetKeyword(material, "_FRESNEL_EDGE", !m_enableFresnelReverse);
                SetKeyword(material, "_FRESNEL_REVERSE", m_enableFresnelReverse);
            }

            m_Shared.SetMode(m_SharedMode);
        }
        public override void DisableMaterialKeywords(Material material)
        {
            base.DisableMaterialKeywords(material);
            SetKeyword(material, "_FRESNEL_EDGE", false);
            SetKeyword(material, "_FRESNEL_REVERSE", false);
        }
    }
}
