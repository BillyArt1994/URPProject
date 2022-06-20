/*******************************************************************
** 文件名:	EffectGUIBase
** 版  权:
** 创建人:	甘文棋
** 日  期:	2020/11/11
** 版  本:	1.0
** 描  述:	自定义特效ShaderGUI基类
** 应  用:  

**************************** 修改记录 ******************************
** 修改人:
** 日  期:
** 描  述:
********************************************************************/
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static LighadEngine.PBSShaderGUI.PBSShaderGUIBase;

namespace LighadEngine.PBSShaderGUI
{
    public abstract class EffectGUIBase
    {
        protected Dictionary<string, Popertie> m_PopertieDic = new Dictionary<string, Popertie>();
        public abstract void DrawEffectsOptions(MaterialEditor materialEditor, int indentLevel);

        public string m_DefineName;
        public EffectGUIBase(string defineName)
        {
            m_DefineName = defineName;
        }
        public virtual void GetProperties(MaterialProperty[] properties)
        {
            // Find properties
            foreach (var propertie in m_PopertieDic)
            {
                propertie.Value.GetPropertie(properties);
            }
        }
        public virtual void SetMaterialKeywords(Material material)
        {
            if (m_DefineName != null)
            {
                material.EnableKeyword(m_DefineName);
            }
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
            }
        }
        public virtual void DisableMaterialKeywords(Material material)
        {
            if (m_DefineName != null)
            {
                material.DisableKeyword(m_DefineName);
            }
            foreach (var propertie in m_PopertieDic)
            {
                Popertie p = propertie.Value;
                if (p.type == PopertieType.Toggle)
                {
                    material.DisableKeyword(p.defineName);
                }
            }
        }
        public void SetKeyword(Material m, string keyword, bool state)
        {
            if (state)
                m.EnableKeyword(keyword);
            else
                m.DisableKeyword(keyword);
        }
    }
}
