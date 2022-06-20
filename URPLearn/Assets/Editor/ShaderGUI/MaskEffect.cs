using UnityEditor;
using UnityEngine;

/*
 * 开启：      Shader属性名      Shader属性类型                说明
 *          宏：_MASK_ON            define                     开关
 *      参  数：_EffectTex            2D                       遮挡图
 * 关闭：
 *          宏：_MASK_ON            define
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
        //共享属性值
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
