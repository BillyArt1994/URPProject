using UnityEditor;
using UnityEngine;
using static LighadEngine.PBSShaderGUI.PBSShaderGUIBase;
/*
 * UVAnim开启： Shader属性名            Shader属性类型                   说明
 *          宏：_UVANIM_ON                define                        主纹理UV动画 
 *              _CUSTOM_DATA_ON           define                        Dissolve的DissolveLevel用 uv1.x 或 i.uv1.y
 *      参  数：_SpeedU                   float                         U速度
 *              _SpeedV                   float                         V速度
 * UVAnim关闭：
 *          宏：_UVANIM_ON                define
 *              _CUSTOM_DATA_ON           define
 */
/*
 * Dissolve开启：Shader属性名            Shader属性类型                   说明
 *          宏：_DISSOLVE_SIMPLE          define                       直接溶解   （_DISSOLVE_SIMPLE和_DISSOLVE_EDGE只能存在一个）
 *              _DISSOLVE_EDGE            define                       用颜色溶解
 *              _CUSTOM_DATA_ON           define                       Dissolve的DissolveLevel用 uv1.x 或 i.uv1.z
 *      参  数：_DissolveSpeedU           float                        U速度
 *              _DissolveSpeedV           float                        V速度
 *              _DissolveLevel            Range(0,1.01)                Dissolve贴图透明测试
 *              _EffectColor              Color                        溶解颜色
 *              _EdgeIntensity            float                        颜色强度
 *              _EdgeWidth                Range(0,1)                   颜色强度占用边界宽度
 * Dissolve关闭：
 *          宏：_DISSOLVE_SIMPLE          define
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
        //共享属性值
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

            //这里就两个选项 为了方便美术观看 把选项改成了Toggle形式
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