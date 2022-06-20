/*******************************************************************
** 文件名:	PBSShaderGUIBase
** 版  权:
** 创建人:	甘文棋
** 日  期:	2020/11/11
** 版  本:	1.0
** 描  述:	自定义ShaderGUI基类
** 应  用:  

**************************** 修改记录 ******************************
** 修改人:
** 日  期:
** 描  述:
********************************************************************/
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
namespace LighadEngine.PBSShaderGUI
{
    public abstract class PBSShaderGUIBase : ShaderGUI
    {
        #region Structs
        public enum PopertieType
        {
            None,
            Normal,
            Toggle,
            Enum,
        }
        public class Popertie
        {
            public string propertyName;
            public string defineName;
            public PopertieType type;
            public GUIContent label;

            public MaterialProperty m_Poperty;

            //名字，tips，属性名，宏定义名，什么属性（Toggle表示有宏）
            public Popertie(string title, string tips, string propertyName, string defineName = "", PopertieType type = PopertieType.Normal)
            {
                this.propertyName = propertyName;
                this.defineName = defineName;
                this.type = type;
                label = new GUIContent(title, tips);
            }
            public float floatValue
            {
                get => m_Poperty != null ? m_Poperty.floatValue : 0;
                set
                {
                    if (m_Poperty != null)
                    {
                        m_Poperty.floatValue = value;
                    }
                }
            }

            public void GetPropertie(MaterialProperty[] properties)
            {
                m_Poperty = FindProperty(this.propertyName, properties, false);
            }

        }
        #endregion

        #region Fields
        struct StylesFoldout
        {
            // Foldouts
            public static readonly GUIContent LODOptions = new GUIContent("LOD Options");

            public static readonly GUIContent SurfaceInputs = new GUIContent("Surface Inputs");

            public static readonly GUIContent EffectsOptions = new GUIContent("Effects Options");

            public static readonly GUIContent AnimOptions = new GUIContent("Anim Options");

            public static readonly GUIContent VegAnimOptions = new GUIContent("植被摆动");

            public static readonly GUIContent SupportOptions = new GUIContent("Support Options");

            public static readonly GUIContent AdvancedOptions = new GUIContent("Advanced Options");

            public static readonly GUIContent DissolveOptions = new GUIContent("Dissolve Options");

            public static readonly GUIContent FlowMapOptions = new GUIContent("FlowMap Options");

            public static readonly GUIContent TipEffectOptions = new GUIContent("TipEffect Options");
        }

        const string kEditorPrefKey = "BasePBSGUI:";
        // Foldouts
        bool m_LODOptionFoldout;
        bool m_SurfaceInputsFoldout;
        bool m_EffectsOptionsFoldout;
        bool m_AnimOptionFoldout;
        bool m_VegAnimOptionsFoldout;
        bool m_DissolveOptionFoldout;
        bool m_FlowMapOptionFoldout;
        bool m_TipEffectOptionFoldout;
        bool m_SupportOptionsFoldout;
        bool m_AdvancedOptionsFoldout;

        #endregion
        struct PropertyNames
        {
            public static readonly string EnableEffects = "EnableEffects";
        }
        //所有属性
        protected Dictionary<string, Popertie> m_PopertieDic = new Dictionary<string, Popertie>();
        public PBSShaderGUIBase()
        {
            m_PopertieDic.Add(PropertyNames.EnableEffects, new Popertie("Enable Effects", "总特效开关", PropertyNames.EnableEffects));
        }
        public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
        {
            //base.OnGUI(materialEditor, properties);
            m_LODOptionFoldout = GetFoldoutState(StylesFoldout.LODOptions.text);
            m_SurfaceInputsFoldout = GetFoldoutState(StylesFoldout.SurfaceInputs.text);
            m_EffectsOptionsFoldout = GetFoldoutState(StylesFoldout.EffectsOptions.text);
            m_AnimOptionFoldout = GetFoldoutState(StylesFoldout.AnimOptions.text);
            m_VegAnimOptionsFoldout = GetFoldoutState(StylesFoldout.VegAnimOptions.text);
            m_DissolveOptionFoldout = GetFoldoutState(StylesFoldout.DissolveOptions.text);
            m_FlowMapOptionFoldout = GetFoldoutState(StylesFoldout.FlowMapOptions.text);
            m_TipEffectOptionFoldout = GetFoldoutState(StylesFoldout.TipEffectOptions.text);
            m_SupportOptionsFoldout = GetFoldoutState(StylesFoldout.SupportOptions.text);
            m_AdvancedOptionsFoldout = GetFoldoutState(StylesFoldout.AdvancedOptions.text);

            CheckBasicMaterialKeywords(materialEditor.target as Material);

            GetProperties(properties);
            EditorGUI.BeginChangeCheck();
            DrawProperties(materialEditor);
            if (EditorGUI.EndChangeCheck())
            {
                SetBaseMaterialKeywords(materialEditor.target as Material);
            }
        }

        #region Properties 画属性
        public virtual void GetProperties(MaterialProperty[] properties)
        {
            // Find properties
            foreach (var propertie in m_PopertieDic)
            {
                propertie.Value.GetPropertie(properties);
            }
            EffectManager.Instance.GetProperties(properties);
        }

        public virtual void DrawLODOptions(MaterialEditor materialEditor) { }

        //常规参数输入
        public abstract void DrawSurfaceInputs(MaterialEditor materialEditor);//1

        //可选特效接入
        public virtual void DrawEffectsOptions(MaterialEditor materialEditor)//2
        {
            Popertie enableEffects;

            m_PopertieDic.TryGetValue(PropertyNames.EnableEffects, out enableEffects);

            if (!PBSShaderGUIUtils.DrawDefineSwitch(enableEffects))
            {
                //特效没打开
                return;
            }
            EffectManager.Instance.DrawEffectsOptions(materialEditor);
        }
        //动画参数输入
        public virtual void DrawAnimOptions(MaterialEditor materialEditor) { }

        //植被摆动输入
        public virtual void DrawVegAnim(MaterialEditor materialEditor) { }

        //溶解
        public virtual void DrawDissolveOptions(MaterialEditor materialEditor) { }

        //FlowMap
        public virtual void DrawFlowMapOptions(MaterialEditor materialEditor) { }

        //TipEffect
        public virtual void DrawTipEffectOptions(MaterialEditor materialEditor) { }

        //常规一些支持输入
        public abstract void DrawSupportOptions(MaterialEditor materialEditor);//3

        //可选自带支持输入
        public virtual void DrawAdvancedOptions(MaterialEditor materialEditor)//4
        {
            materialEditor.RenderQueueField();
            materialEditor.EnableInstancingField();
            materialEditor.DoubleSidedGIField();
        }

        void DrawProperties(MaterialEditor materialEditor)
        {
            // LOD Options
            var LODOptions = EditorGUILayout.BeginFoldoutHeaderGroup(m_LODOptionFoldout, StylesFoldout.LODOptions);
            if (LODOptions)
            {
                DrawLODOptions(materialEditor);
                EditorGUILayout.Space();
            }
            SetFoldoutState(StylesFoldout.LODOptions.text, m_LODOptionFoldout, LODOptions);
            EditorGUILayout.EndFoldoutHeaderGroup();

            // Surface Input
            var surfaceInputs = EditorGUILayout.BeginFoldoutHeaderGroup(m_SurfaceInputsFoldout, StylesFoldout.SurfaceInputs);
            if (surfaceInputs)
            {
                DrawSurfaceInputs(materialEditor);
                EditorGUILayout.Space();
            }
            SetFoldoutState(StylesFoldout.SurfaceInputs.text, m_SurfaceInputsFoldout, surfaceInputs);
            EditorGUILayout.EndFoldoutHeaderGroup();

            // Effects Options
            var effectsOptions = EditorGUILayout.BeginFoldoutHeaderGroup(m_EffectsOptionsFoldout, StylesFoldout.EffectsOptions);
            if (effectsOptions)
            {
                DrawEffectsOptions(materialEditor);
                EditorGUILayout.Space();
            }
            SetFoldoutState(StylesFoldout.EffectsOptions.text, m_EffectsOptionsFoldout, effectsOptions);
            EditorGUILayout.EndFoldoutHeaderGroup();

            // Anim Options
            var animOptions = EditorGUILayout.BeginFoldoutHeaderGroup(m_AnimOptionFoldout, StylesFoldout.AnimOptions);
            if (animOptions)
            {
                DrawAnimOptions(materialEditor);
                EditorGUILayout.Space();
            }
            SetFoldoutState(StylesFoldout.AnimOptions.text, m_AnimOptionFoldout, animOptions);
            EditorGUILayout.EndFoldoutHeaderGroup();

            // Veg Anim Options
            var vegAnimOptions = EditorGUILayout.BeginFoldoutHeaderGroup(m_VegAnimOptionsFoldout, StylesFoldout.VegAnimOptions);
            if (vegAnimOptions)
            {
                DrawVegAnim(materialEditor);
                EditorGUILayout.Space();
            }
            SetFoldoutState(StylesFoldout.VegAnimOptions.text, m_VegAnimOptionsFoldout, vegAnimOptions);
            EditorGUILayout.EndFoldoutHeaderGroup();

            // Dissolve Options
            var dissolveOptions = EditorGUILayout.BeginFoldoutHeaderGroup(m_DissolveOptionFoldout, StylesFoldout.DissolveOptions);
            if (dissolveOptions)
            {
                DrawDissolveOptions(materialEditor);
                EditorGUILayout.Space();
            }
            SetFoldoutState(StylesFoldout.DissolveOptions.text, m_DissolveOptionFoldout, dissolveOptions);
            EditorGUILayout.EndFoldoutHeaderGroup();

            // FlowMap Options
            var flowMapOptions = EditorGUILayout.BeginFoldoutHeaderGroup(m_FlowMapOptionFoldout, StylesFoldout.FlowMapOptions);
            if (flowMapOptions)
            {
                DrawFlowMapOptions(materialEditor);
                EditorGUILayout.Space();
            }
            SetFoldoutState(StylesFoldout.FlowMapOptions.text, m_FlowMapOptionFoldout, flowMapOptions);
            EditorGUILayout.EndFoldoutHeaderGroup();

            // TipEffect Options
            var tipEffectOptions = EditorGUILayout.BeginFoldoutHeaderGroup(m_TipEffectOptionFoldout, StylesFoldout.TipEffectOptions);
            if (tipEffectOptions)
            {
                DrawTipEffectOptions(materialEditor);
                EditorGUILayout.Space();
            }
            SetFoldoutState(StylesFoldout.TipEffectOptions.text, m_TipEffectOptionFoldout, tipEffectOptions);
            EditorGUILayout.EndFoldoutHeaderGroup();

            // Support Options
            var supportOptions = EditorGUILayout.BeginFoldoutHeaderGroup(m_SupportOptionsFoldout, StylesFoldout.SupportOptions);
            if (supportOptions)
            {
                DrawSupportOptions(materialEditor);
                EditorGUILayout.Space();
            }
            SetFoldoutState(StylesFoldout.SupportOptions.text, m_SupportOptionsFoldout, supportOptions);
            EditorGUILayout.EndFoldoutHeaderGroup();

            // Advanced Options
            var advancedOptions = EditorGUILayout.BeginFoldoutHeaderGroup(m_AdvancedOptionsFoldout, StylesFoldout.AdvancedOptions);
            if (advancedOptions)
            {
                DrawAdvancedOptions(materialEditor);
                EditorGUILayout.Space();
            }
            SetFoldoutState(StylesFoldout.AdvancedOptions.text, m_AdvancedOptionsFoldout, advancedOptions);
            EditorGUILayout.EndFoldoutHeaderGroup();
        }
        #endregion

        #region Keywords
        /// <summary>
        /// Set Material keywords when changes are made during OnGUI call.
        /// </summary>
        /// <param name="material">Material target of current MaterialEditor.</param>
        public abstract void SetMaterialKeywords(Material material);
        public abstract void DisableMaterialKeywords(Material material);
        public virtual void SetMaterialEnumKeywords(Material material) { }
        public virtual void CheckBasicMaterialKeywords(Material material) { }

        void SetBaseMaterialKeywords(Material material)
        {
            // Reset
            bool enableEffect = true;
            bool enableEnum = false;
            material.shaderKeywords = null;
            foreach (var propertie in m_PopertieDic)
            {
                Popertie p = propertie.Value;
                if (p.type == PopertieType.Toggle)
                {
                    if (p.floatValue == 1)
                    {
                        material.EnableKeyword(p.defineName);
                    }
                    else
                    {
                        material.DisableKeyword(p.defineName);
                    }
                }
                if (propertie.Key == PropertyNames.EnableEffects && p.floatValue == 0)
                {
                    //没启动特效
                    enableEffect = false;
                }
                if (p.type == PopertieType.Enum)
                {
                    enableEnum = true;
                }
            }

            if (enableEffect)
            {
                EffectManager.Instance.SetMaterialKeywords(material);
            }
            else
            {
                EffectManager.Instance.DisableMaterialKeywords(material);
            }
            // Custom Keywords
            if (enableEffect)
            {
                SetMaterialKeywords(material);
            }
            else
            {
                DisableMaterialKeywords(material);
            }

            if (enableEnum)
            {
                SetMaterialEnumKeywords(material);
            }
        }
        #endregion
        #region EditorPrefs
        bool GetFoldoutState(string name)
        {
            // Get value from EditorPrefs
            return EditorPrefs.GetBool($"{kEditorPrefKey}.{name}");
        }

        void SetFoldoutState(string name, bool field, bool value)
        {
            if (field == value)
                return;

            // Set value to EditorPrefs and field
            EditorPrefs.SetBool($"{kEditorPrefKey}.{name}", value);
            field = value;
        }
        #endregion

    }
    #region Utils
    public static class PBSShaderGUIUtils
    {
        public static bool DrawDefineSwitch(PBSShaderGUIBase.Popertie poperite)
        {
            if (poperite == null || poperite.m_Poperty == null)
                return false;

            //开关
            EditorGUI.BeginChangeCheck();
            var state = EditorGUILayout.Toggle(poperite.label, poperite.floatValue == 1);
            if (EditorGUI.EndChangeCheck())
            {
                poperite.floatValue = state ? 1 : 0;
            }
            return poperite.floatValue == 1 ? true : false;
        }
        public static void ShaderProperty(string propertyName, Dictionary<string, PBSShaderGUIBase.Popertie> m_PopertieDic, MaterialEditor materialEditor, int labelIndent = 0)
        {
            PBSShaderGUIBase.Popertie popertie;
            if (m_PopertieDic.TryGetValue(propertyName, out popertie))
            {
                if (popertie.m_Poperty == null)
                    return;

                materialEditor.ShaderProperty(popertie.m_Poperty, popertie.label, labelIndent);
            }
        }
    }
    #endregion
}
