/*******************************************************************
** �ļ���:	EffectManager
** ��  Ȩ:
** ������:	������
** ��  ��:	2020/12/01
** ��  ��:	1.0
** ��  ��:	
** Ӧ  ��:  

**************************** �޸ļ�¼ ******************************
** �޸���:
** ��  ��:
** ��  ��:
********************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace LighadEngine.PBSShaderGUI
{
    public class EffectManager
    {
        static EffectManager m_Instance;
        public static EffectManager Instance
        {
            get
            {
                if (m_Instance == null)
                {
                    m_Instance = new EffectManager();
                }
                return m_Instance;
            }
        }

        private Type[] m_EffectTypes;
        private string[] m_EffectNames;
        private Type m_CurrentType;

        //��Чͨ�ò���
        EffectGUIBase m_SharedEffect;
        private
        //��Ч
        protected Dictionary<Type, EffectGUIBase> m_EffectGUIList = new Dictionary<Type, EffectGUIBase>();

        public EffectManager()
        {
            m_EffectTypes = EffectTypes.GetTypes();
            m_EffectNames = m_EffectTypes.Select(t => t.Name).ToArray();

            SharedEffect shared = new SharedEffect();
            m_SharedEffect = shared;
            m_EffectGUIList.Add(typeof(EmissiveEffect), new EmissiveEffect(shared));
            m_EffectGUIList.Add(typeof(MaskEffect), new MaskEffect(shared));
            m_EffectGUIList.Add(typeof(FresnelEffect), new FresnelEffect(shared));
            m_EffectGUIList.Add(typeof(DissolveAndUVAnimEffect), new DissolveAndUVAnimEffect(shared));
            m_EffectGUIList.Add(typeof(DistortionAndFlowEffect), new DistortionAndFlowEffect(shared));
        }
        #region ���Բ���
        public void GetProperties(MaterialProperty[] properties)
        {
            m_SharedEffect.GetProperties(properties);
            if (m_EffectTypes.Length > 0)
            {
                int effectIndex = Math.Max(Array.IndexOf(m_EffectTypes, m_CurrentType), 0);
                m_CurrentType = m_EffectTypes[effectIndex];

                //ֻ���ǰѡ�е���Ч
                if (m_EffectGUIList.TryGetValue(m_CurrentType, out var effect))
                {
                    effect.GetProperties(properties);
                }
            }
            else
            {
                EditorGUILayout.LabelField("Cannot find Effects.");
            }
        }
        public void DrawEffectsOptions(MaterialEditor materialEditor)
        {
            EditorGUI.indentLevel = 1;
            GUILayout.Label("��ܰ��ʾ���������ᱣ�棬��Ҫ����ά��", EditorStyles.boldLabel);
            m_SharedEffect.DrawEffectsOptions(materialEditor, EditorGUI.indentLevel);
            if (m_EffectTypes.Length > 0)
            {
                int effectIndex = Math.Max(Array.IndexOf(m_EffectTypes, m_CurrentType), 0);
                effectIndex = EditorGUILayout.Popup("Effect", effectIndex, m_EffectNames);
                m_CurrentType = m_EffectTypes[effectIndex];

                //ֻ����ǰѡ�е���Ч
                if (m_EffectGUIList.TryGetValue(m_CurrentType, out var effect))
                {
                    EditorGUI.indentLevel = 2;
                    effect.DrawEffectsOptions(materialEditor, EditorGUI.indentLevel);
                }
            }
            EditorGUI.indentLevel = 0;
        }

        public void SetMaterialKeywords(Material material)
        {
            m_SharedEffect.SetMaterialKeywords(material);
            if (m_EffectTypes.Length > 0)
            {
                int effectIndex = Math.Max(Array.IndexOf(m_EffectTypes, m_CurrentType), 0);
                m_CurrentType = m_EffectTypes[effectIndex];

                foreach (var e in m_EffectGUIList)
                {
                    if (e.Key == m_CurrentType)
                    {
                        e.Value.SetMaterialKeywords(material);
                    }
                    else
                    {
                        e.Value.DisableMaterialKeywords(material);
                    }
                }
            }
        }
        public void DisableMaterialKeywords(Material material)
        {
            if (m_EffectTypes.Length > 0)
            {
                int effectIndex = Math.Max(Array.IndexOf(m_EffectTypes, m_CurrentType), 0);
                effectIndex = EditorGUILayout.Popup("Effect", effectIndex, m_EffectNames);
                m_CurrentType = m_EffectTypes[effectIndex];

                foreach (var e in m_EffectGUIList)
                {
                    e.Value.DisableMaterialKeywords(material);
                }
            }
        }
        #endregion
    }
}
