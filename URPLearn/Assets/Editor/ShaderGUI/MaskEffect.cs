using UnityEditor;
using UnityEngine;

/*
 * ������      Shader������      Shader��������                ˵��
 *          �꣺_MASK_ON            define                     ����
 *      ��  ����_EffectTex            2D                       �ڵ�ͼ
 * �رգ�
 *          �꣺_MASK_ON            define
 */
namespace LighadEngine.PBSShaderGUI
{
    public class MaskEffect : EffectGUIBase
    {
        [InitializeOnLoadMethod]
        static void RegisterType()
        {
            EffectTypes.RegisterBatcherType(typeof(MaskEffect));
        }

        private SharedEffect m_Shared;
        //��������ֵ
        SharedEffectMode m_SharedMode = new SharedEffectMode();
        public MaskEffect(SharedEffect shared) : base("_MASK_ON")
        {
            m_Shared = shared;
        }

        public override void DrawEffectsOptions(MaterialEditor materialEditor, int indentLevel)
        {
            m_SharedMode.Tex = (Texture2D)EditorGUILayout.ObjectField("Mask(R)", m_SharedMode.Tex, typeof(Texture2D), false, GUILayout.ExpandWidth(true));
            m_SharedMode.TexScaleOffest = EditorGUILayout.Vector4Field("XY:Tiling  ZW:Offset", m_SharedMode.TexScaleOffest);

            m_Shared.SetMode(m_SharedMode);
        }
    }
}
