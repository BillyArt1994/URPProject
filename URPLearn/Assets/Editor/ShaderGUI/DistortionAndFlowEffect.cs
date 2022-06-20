using UnityEditor;
using UnityEngine;
using static LighadEngine.PBSShaderGUI.PBSShaderGUIBase;
/*
 * Distortion开启：Shader属性名          Shader属性类型                   说明
 *          宏：_DISTORTION_UV            define                        UV模式  (_DISTORTION_UV和_DISTORTION_GRAB只能存在一个)
 *              _DISTORTION_GRAB          define                        GRAB模式
 *      参  数：_DistortionTex             2D                           失真贴图
 *              _DistortionStrength       float                         失真强度
 *              _DistortionSpeed          float                         移动速度
 *              _DistortionBlend          Range(0,1)                    混合权重
 *              
 *      声  明：GRAB模式下  Blend[SrcAlpha][OneMinusSrcAlpha]
 * Distortion关闭：
 *          宏：_DISTORTION_UV            define
 *              _DISTORTION_GRAB          define
 */
/*
 * Flow开启：Shader属性名                Shader属性类型                   说明
 *          宏：_FLOW_ADD                 define                         叠加
 *              _FLOW_ALPHA               define                         根据透明度混合
 *              _FLOW_MUL                 define                         相乘
 *      参  数：_EffectTex                 2D                            flow贴图
 *              _EffectColor              float                          颜色
 *              _SpeedU                   float                          U移动
 *              _SpeedV                   float                          V移动
 *              
 * Flow关闭：
 *          宏：_FLOW_ADD                 define
 *              _FLOW_ALPHA               define
 *              _FLOW_MUL                 define
 */


namespace LighadEngine.PBSShaderGUI
{
    public class DistortionAndFlowEffect : EffectGUIBase
    {
        [InitializeOnLoadMethod]
        static void RegisterType()
        {
            EffectTypes.RegisterBatcherType(typeof(DistortionAndFlowEffect));
        }
        private enum DistortionMode
        {
            //None,
            UV = 1,
            GRAB = 2,
        }
        public enum FlowBlendMode
        {
            //None,
            Add = 1,
            Alpha,
            Mul,   // Old school alpha-blending mode, fresnel does not affect amount of transparency
        }
        public struct PropertyNames
        {
            public static readonly string Distortion = "_Distortion";
            public static readonly string DistortionTex = "_DistortionTex";
            public static readonly string DistortionStrength = "_DistortionStrength";
            public static readonly string DistortionSpeed = "_DistortionSpeed";
            public static readonly string DistortionBlend = "_DistortionBlend";

            public static readonly string Flow = "_Flow";
        }
        private SharedEffect m_Shared;
        //共享属性值
        SharedEffectMode m_SharedMode = new SharedEffectMode();

        bool m_enableDistortion = false;
        bool m_enableFlow;
        DistortionMode _distortionMode = DistortionMode.UV;
        FlowBlendMode _flowBlendMode = FlowBlendMode.Add;
        GUIContent m_ColorLabel = new GUIContent("Color");
        public DistortionAndFlowEffect(SharedEffect shared) : base(null)
        {
            m_Shared = shared;
            m_PopertieDic.Add(PropertyNames.Distortion, new Popertie("Distortion", "", PropertyNames.Distortion));
            m_PopertieDic.Add(PropertyNames.DistortionTex, new Popertie("DistortionTex", "", PropertyNames.DistortionTex));
            m_PopertieDic.Add(PropertyNames.DistortionStrength, new Popertie("DistortionStrength", "", PropertyNames.DistortionStrength));
            m_PopertieDic.Add(PropertyNames.DistortionSpeed, new Popertie("DistortionSpeed", "", PropertyNames.DistortionSpeed));
            m_PopertieDic.Add(PropertyNames.DistortionBlend, new Popertie("Blend", "", PropertyNames.DistortionBlend));

            m_PopertieDic.Add(PropertyNames.Flow, new Popertie("Flow", "", PropertyNames.Flow));
        }

        public override void DrawEffectsOptions(MaterialEditor materialEditor, int indentLevel)
        {
            Material material = materialEditor.target as Material;

            m_PopertieDic.TryGetValue(PropertyNames.Distortion, out var distortion);
            bool bDistortion = distortion.floatValue > 0;
            EditorGUI.BeginChangeCheck();
            {
                m_enableDistortion = EditorGUILayout.Toggle("Enable Distortion", bDistortion);
            }
            EditorGUI.EndChangeCheck();
            {
                if (distortion.floatValue == 0) distortion.floatValue++;

                distortion.floatValue = m_enableDistortion ? distortion.floatValue : 0;

            }
            if (m_enableDistortion)
            {
                EditorGUI.indentLevel = indentLevel + 1;
                DoDistortion(materialEditor);
                EditorGUI.indentLevel = indentLevel;
            }
            else
            {
                SetKeyword(material, "_DISTORTION_UV", false);
                SetKeyword(material, "_DISTORTION_GRAB", false);
                material.SetShaderPassEnabled("Always", false);
            }

            EditorGUILayout.Space(5);
            m_PopertieDic.TryGetValue(PropertyNames.Flow, out var flow);
            bool bFlow = flow.floatValue > 0;
            EditorGUI.BeginChangeCheck();
            {
                m_enableFlow = EditorGUILayout.Toggle("Enable Flow", bFlow);
            }
            EditorGUI.EndChangeCheck();
            {
                if (flow.floatValue == 0) flow.floatValue++;

                flow.floatValue = m_enableFlow ? flow.floatValue : 0;

            }
            if (m_enableFlow)
            {
                EditorGUI.indentLevel = indentLevel + 1;
                DoFlowArea(materialEditor);
                EditorGUI.indentLevel = indentLevel;
            }
            else
            {
                SetKeyword(material, "_FLOW_ADD", false);
                SetKeyword(material, "_FLOW_ALPHA", false);
                SetKeyword(material, "_FLOW_MUL", false);
            }



            m_Shared.SetMode(m_SharedMode);
        }
        void DoDistortion(MaterialEditor materialEditor)
        {
            Material material = materialEditor.target as Material;
            m_PopertieDic.TryGetValue(PropertyNames.Distortion, out var distortion);
            m_PopertieDic.TryGetValue(PropertyNames.DistortionTex, out var distortionTex);

            _distortionMode = (DistortionMode)distortion.floatValue;
            if (_distortionMode == DistortionMode.UV)
            {
                distortionTex.m_Poperty.textureValue = (Texture2D)EditorGUILayout.ObjectField("Distortion (R)", distortionTex.m_Poperty.textureValue, typeof(Texture2D), false, GUILayout.ExpandWidth(true));
                distortionTex.m_Poperty.textureScaleAndOffset = EditorGUILayout.Vector4Field("XY:Tiling  ZW:Offset", distortionTex.m_Poperty.textureScaleAndOffset);

                SetKeyword(material, "_DISTORTION_UV", true);
                SetKeyword(material, "_DISTORTION_GRAB", false);
                material.SetShaderPassEnabled("Always", false);
            }
            else if (_distortionMode == DistortionMode.GRAB)
            {
                m_SharedMode.Tex = null;
                SetKeyword(material, "_DISTORTION_UV", false);
                SetKeyword(material, "_DISTORTION_GRAB", true);
                material.SetShaderPassEnabled("Always", true);
                PBSShaderGUIUtils.ShaderProperty(PropertyNames.DistortionBlend, m_PopertieDic, materialEditor);
            }
            PBSShaderGUIUtils.ShaderProperty(PropertyNames.DistortionStrength, m_PopertieDic, materialEditor);
            PBSShaderGUIUtils.ShaderProperty(PropertyNames.DistortionSpeed, m_PopertieDic, materialEditor);

            EditorGUI.BeginChangeCheck();
            {
                _distortionMode = (DistortionMode)EditorGUILayout.EnumPopup("Distortion Mode", _distortionMode);
            }
            if (EditorGUI.EndChangeCheck())
            {
                materialEditor.RegisterPropertyChangeUndo("Distortion Mode");
                if (_distortionMode != (DistortionMode)distortion.floatValue)
                {
                    distortion.floatValue = (int)_distortionMode;
                    m_SharedMode.SpeedU = 0;
                    m_SharedMode.SpeedV = 0;
                    if (_distortionMode == DistortionMode.GRAB)
                    {
                        material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                        material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                    }
                }
            }
        }
        void DoFlowArea(MaterialEditor materialEditor)
        {
            Material material = materialEditor.target as Material;
            m_PopertieDic.TryGetValue(PropertyNames.Flow, out var flow);
            _flowBlendMode = (FlowBlendMode)flow.floatValue;
            string flowText = "Flow (RGBA)";
            if (_flowBlendMode == FlowBlendMode.Add)
            {
                flowText = "Flow (RGB)";
            }
            m_SharedMode.Tex = (Texture2D)EditorGUILayout.ObjectField(flowText, m_SharedMode.Tex, typeof(Texture2D), false, GUILayout.ExpandWidth(true));
            m_SharedMode.TexScaleOffest = EditorGUILayout.Vector4Field("XY:Tiling  ZW:Offset", m_SharedMode.TexScaleOffest);

            m_SharedMode.Color = EditorGUILayout.ColorField(m_ColorLabel, m_SharedMode.Color, true, true, true);
            m_SharedMode.SpeedU = EditorGUILayout.FloatField("SpeedU", m_SharedMode.SpeedU);
            m_SharedMode.SpeedV = EditorGUILayout.FloatField("SpeedV", m_SharedMode.SpeedV);

            EditorGUI.BeginChangeCheck();
            {
                _flowBlendMode = (FlowBlendMode)EditorGUILayout.EnumPopup("Flow Blend Mode", _flowBlendMode);
            }
            if (EditorGUI.EndChangeCheck())
            {
                materialEditor.RegisterPropertyChangeUndo("Flow Blend Mode");
                flow.floatValue = (int)_flowBlendMode;
            }

            if (_flowBlendMode == FlowBlendMode.Add)
            {
                SetKeyword(material, "_FLOW_ADD", true);
                SetKeyword(material, "_FLOW_ALPHA", false);
                SetKeyword(material, "_FLOW_MUL", false);
            }
            else if (_flowBlendMode == FlowBlendMode.Alpha)
            {
                SetKeyword(material, "_FLOW_ADD", false);
                SetKeyword(material, "_FLOW_ALPHA", true);
                SetKeyword(material, "_FLOW_MUL", false);
            }
            else if (_flowBlendMode == FlowBlendMode.Mul)
            {
                SetKeyword(material, "_FLOW_ADD", false);
                SetKeyword(material, "_FLOW_ALPHA", false);
                SetKeyword(material, "_FLOW_MUL", true);
            }
        }
        public override void DisableMaterialKeywords(Material material)
        {
            base.DisableMaterialKeywords(material);

        }
    }
}
