using UnityEditor;
using UnityEngine;
using static LighadEngine.PBSShaderGUI.PBSShaderGUIBase;
/*
 * UVAnim������ Shader������            Shader��������                   ˵��
 *          �꣺_UVANIM_ON                define                        ������UV���� 
 *              _CUSTOM_DATA_ON           define                        Dissolve��DissolveLevel�� uv1.x �� i.uv1.y
 *      ��  ����_SpeedU                   float                         U�ٶ�
 *              _SpeedV                   float                         V�ٶ�
 * UVAnim�رգ�
 *          �꣺_UVANIM_ON                define
 *              _CUSTOM_DATA_ON           define
 */
/*
 * Dissolve������Shader������            Shader��������                   ˵��
 *          �꣺_DISSOLVE_SIMPLE          define                       ֱ���ܽ�   ��_DISSOLVE_SIMPLE��_DISSOLVE_EDGEֻ�ܴ���һ����
 *              _DISSOLVE_EDGE            define                       ����ɫ�ܽ�
 *              _CUSTOM_DATA_ON           define                       Dissolve��DissolveLevel�� uv1.x �� i.uv1.z
 *      ��  ����_DissolveSpeedU           float                        U�ٶ�
 *              _DissolveSpeedV           float                        V�ٶ�
 *              _DissolveLevel            Range(0,1.01)                Dissolve��ͼ͸������
 *              _EffectColor              Color                        �ܽ���ɫ
 *              _EdgeIntensity            float                        ��ɫǿ��
 *              _EdgeWidth                Range(0,1)                   ��ɫǿ��ռ�ñ߽���
 * Dissolve�رգ�
 *          �꣺_DISSOLVE_SIMPLE          define
 *              _DISSOLVE_EDGE            define
 */
namespace LighadEngine.PBSShaderGUI
{
    public class DissolveAndUVAnimEffect : EffectGUIBase
    {
        [InitializeOnLoadMethod]
        static void RegisterType()
        {
            EffectTypes.RegisterBatcherType(typeof(DissolveAndUVAnimEffect));
        }

        struct PropertyNames
        {
            public static readonly string UVAnim = "_UVAnim";
            public static readonly string Dissolve = "_Dissolve";
            public static readonly string DissolveLevel = "_DissolveLevel";
            public static readonly string DissolveSpeedU = "_DissolveSpeedU";
            public static readonly string DissolveSpeedV = "_DissolveSpeedV";
            public static readonly string EdgeIntensity = "_EdgeIntensity";
            public static readonly string EdgeWidth = "_EdgeWidth";
        }
        public enum DissolveMode
        {
            //None,
            Dissolve_Simple = 1,
            Dissolve_Edge,
        }

        private SharedEffect m_Shared;
        //��������ֵ
        SharedEffectMode m_SharedMode = new SharedEffectMode();

        bool m_enableCustomDataUV = false;
        bool m_enableDissolve;
        bool m_enableDissolveEdge;
        bool m_enableCustomDataLevel;
        public DissolveAndUVAnimEffect(SharedEffect shared) : base(null)
        {
            m_Shared = shared;
            m_PopertieDic.Add(PropertyNames.UVAnim, new Popertie("Enable UV Anim", "", PropertyNames.UVAnim, "_UVANIM_ON", PopertieType.Toggle));

            m_PopertieDic.Add(PropertyNames.Dissolve, new Popertie("Dissolve", "", PropertyNames.Dissolve));
            m_PopertieDic.Add(PropertyNames.DissolveSpeedU, new Popertie("DissolveSpeedU", "", PropertyNames.DissolveSpeedU));
            m_PopertieDic.Add(PropertyNames.DissolveSpeedV, new Popertie("DissolveSpeedV", "", PropertyNames.DissolveSpeedV));
            m_PopertieDic.Add(PropertyNames.DissolveLevel, new Popertie("Dissolve Level", "", PropertyNames.DissolveLevel));
            m_PopertieDic.Add(PropertyNames.EdgeIntensity, new Popertie("Intensity", "", PropertyNames.EdgeIntensity));
            m_PopertieDic.Add(PropertyNames.EdgeWidth, new Popertie("Width", "", PropertyNames.EdgeWidth));
        }

        public override void DrawEffectsOptions(MaterialEditor materialEditor, int indentLevel)
        {
            m_PopertieDic.TryGetValue(PropertyNames.UVAnim, out var uvAnim);
            PBSShaderGUIUtils.ShaderProperty(PropertyNames.UVAnim, m_PopertieDic, materialEditor);
            if (uvAnim.floatValue >= 1)
            {
                EditorGUI.indentLevel = indentLevel + 1;
                DoUVAnim(materialEditor);
                EditorGUI.indentLevel = indentLevel;
            }
            else
            {
                Material material = materialEditor.target as Material;
                //UVAnim
                material.SetFloat("_CustomData_UV", 0);

                UpdateCustomData(material, false);
            }

            EditorGUILayout.Space(5);
            m_PopertieDic.TryGetValue(PropertyNames.Dissolve, out var dissolve);
            bool bDissolve = dissolve.floatValue > 0;
            EditorGUI.BeginChangeCheck();
            {
                m_enableDissolve = EditorGUILayout.Toggle("Enable Dissolve", bDissolve);
            }
            EditorGUI.EndChangeCheck();
            {
                if (dissolve.floatValue == 0) dissolve.floatValue++;

                dissolve.floatValue = m_enableDissolve ? dissolve.floatValue : 0;
            }

            if (m_enableDissolve)
            {
                EditorGUI.indentLevel = indentLevel + 1;
                DoDissolve(materialEditor);
                EditorGUI.indentLevel = indentLevel;
            }
            else
            {
                Material material = materialEditor.target as Material;
                dissolve.floatValue = 0;

                SetKeyword(material, "_DISSOLVE_SIMPLE", false);
                SetKeyword(material, "_DISSOLVE_EDGE", false);

                material.SetFloat("_CustomData_Level", 0);
                UpdateCustomData(material, false);
            }
        }
        void DoUVAnim(MaterialEditor materialEditor)
        {
            Material material = materialEditor.target as Material;
            bool bCustomData_UV = material.GetFloat("_CustomData_UV") > 0;
            if (!bCustomData_UV)
            {
                m_SharedMode.SpeedU = EditorGUILayout.FloatField("SpeedU", m_SharedMode.SpeedU);
                m_SharedMode.SpeedV = EditorGUILayout.FloatField("SpeedV", m_SharedMode.SpeedV);
            }
            else
            {
                m_SharedMode.SpeedU = 0;
                m_SharedMode.SpeedV = 0;
            }
            //m_MaterialEditor.ShaderProperty(customData_UV, "UV From Custom Data(0.zw)");
            EditorGUI.BeginChangeCheck();
            {
                m_enableCustomDataUV = EditorGUILayout.Toggle("UV From Custom Data(0.zw)", bCustomData_UV);
            }
            EditorGUI.EndChangeCheck();
            {
                material.SetFloat("_CustomData_UV", m_enableCustomDataUV ? 1 : 0);
                UpdateCustomData(material, m_enableCustomDataUV);
            }

            m_Shared.SetMode(m_SharedMode);
        }

        void DoDissolve(MaterialEditor materialEditor)
        {
            m_PopertieDic.TryGetValue(PropertyNames.Dissolve, out var dissolve);
            var material = materialEditor.target as Material;

            m_SharedMode.Tex = (Texture2D)EditorGUILayout.ObjectField("Dissolve (R)", m_SharedMode.Tex, typeof(Texture2D), false, GUILayout.ExpandWidth(true));
            m_SharedMode.TexScaleOffest = EditorGUILayout.Vector4Field("XY:Tiling  ZW:Offset", m_SharedMode.TexScaleOffest);
            PBSShaderGUIUtils.ShaderProperty(PropertyNames.DissolveSpeedU, m_PopertieDic, materialEditor);
            PBSShaderGUIUtils.ShaderProperty(PropertyNames.DissolveSpeedV, m_PopertieDic, materialEditor);

            bool bCustomData_Level = material.GetFloat("_CustomData_Level") > 0;
            if (!bCustomData_Level)
            {
                PBSShaderGUIUtils.ShaderProperty(PropertyNames.DissolveLevel, m_PopertieDic, materialEditor, 0);
            }
            else
            {
                material.SetFloat("_DissolveLevel", 0);
            }

            bool bUVAnim = material.GetFloat("_UVAnim") > 0;
            string dec = "Dissolve From Custom Data(0.z)";
            if (bUVAnim)
            {
                dec = "Dissolve From Custom Data(1.x)";
            }
            EditorGUI.BeginChangeCheck();
            {
                m_enableCustomDataLevel = EditorGUILayout.Toggle(dec, bCustomData_Level);
            }
            EditorGUI.EndChangeCheck();
            {
                material.SetFloat("_CustomData_Level", m_enableCustomDataLevel ? 1 : 0);
                UpdateCustomData(material, m_enableCustomDataLevel);
            }

            //���������ѡ�� Ϊ�˷��������ۿ� ��ѡ��ĳ���Toggle��ʽ
            bool bDissolveEdge = dissolve.floatValue == (int)DissolveMode.Dissolve_Edge;
            EditorGUI.BeginChangeCheck();
            {
                m_enableDissolveEdge = EditorGUILayout.Toggle("Edge", bDissolveEdge);
            }
            EditorGUI.EndChangeCheck();
            {
                dissolve.floatValue = m_enableDissolveEdge ? (int)DissolveMode.Dissolve_Edge : (int)DissolveMode.Dissolve_Simple;
                SetKeyword(material, "_DISSOLVE_SIMPLE", !m_enableDissolveEdge);
                SetKeyword(material, "_DISSOLVE_EDGE", m_enableDissolveEdge);
            }
            if (m_enableDissolveEdge)
            {
                m_SharedMode.Color = EditorGUILayout.ColorField("Color", m_SharedMode.Color);
                PBSShaderGUIUtils.ShaderProperty(PropertyNames.EdgeIntensity, m_PopertieDic, materialEditor);
                PBSShaderGUIUtils.ShaderProperty(PropertyNames.EdgeWidth, m_PopertieDic, materialEditor);
            }

            m_Shared.SetMode(m_SharedMode);
        }
        public override void DisableMaterialKeywords(Material material)
        {
            base.DisableMaterialKeywords(material);
        }
        void UpdateCustomData(Material material, bool isEnable)
        {
            bool bCustomData_UV = material.GetFloat("_CustomData_UV") > 0;
            bool bCustomData_Level = material.GetFloat("_CustomData_Level") > 0;
            if (isEnable)
            {
                if (bCustomData_UV || bCustomData_Level)
                {
                    SetKeyword(material, "_CUSTOM_DATA_ON", true);
                    material.SetFloat("_CUSTOM_DATA_ON", 1);
                }
            }
            else
            {
                if (!bCustomData_UV && !bCustomData_Level)
                {
                    SetKeyword(material, "_CUSTOM_DATA_ON", false);
                    material.SetFloat("_CUSTOM_DATA_ON", 0);
                }
            }
        }
    }
}