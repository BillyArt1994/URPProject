using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static LighadEngine.PBSShaderGUI.PBSShaderGUIBase;

namespace LighadEngine.PBSShaderGUI
{
    public class SharedEffectMode
    {
        public Texture2D Tex;
        public Vector4 TexScaleOffest = new Vector4(1, 1, 0, 0);
        public Color32 Color = new Color32(255, 255, 255, 255);
        public float SpeedU;
        public float SpeedV;
        public static void CopyScrToDis(SharedEffectMode scr, SharedEffectMode dis)
        {
            scr.Tex = dis.Tex;
        }
    }
    //特效共享的属性都写到这里
    public class SharedEffect : EffectGUIBase
    {
        public struct PropertyNames
        {
            public static readonly string _EffectTex = "_EffectTex";
            public static readonly string _EffectColor = "_EffectColor";
            public static readonly string _SpeedU = "_SpeedU";
            public static readonly string _SpeedV = "_SpeedV";
        }

        SharedEffectMode m_Mode = new SharedEffectMode();
        public SharedEffect() : base(null)
        {
            m_PopertieDic.Add(PropertyNames._EffectTex, new Popertie("EffectTex", "特效图", PropertyNames._EffectTex));
            m_PopertieDic.Add(PropertyNames._EffectColor, new Popertie("EffectColor", "特效颜色", PropertyNames._EffectColor));
            m_PopertieDic.Add(PropertyNames._SpeedU, new Popertie("SpeedU", "U速度", PropertyNames._SpeedU));
            m_PopertieDic.Add(PropertyNames._SpeedV, new Popertie("SpeedV", "V速度", PropertyNames._SpeedV));
        }
        public void SetMode(SharedEffectMode mode)
        {
            m_Mode = mode;
            foreach (var propertie in m_PopertieDic)
            {
                MaterialProperty poperty = propertie.Value.m_Poperty;
                if (propertie.Key == PropertyNames._EffectTex)
                {
                    poperty.textureValue = m_Mode.Tex;
                    poperty.textureScaleAndOffset = m_Mode.TexScaleOffest;
                }
                else if (propertie.Key == PropertyNames._SpeedU)
                {
                    poperty.floatValue = m_Mode.SpeedU;
                }
                else if (propertie.Key == PropertyNames._SpeedV)
                {
                    poperty.floatValue = m_Mode.SpeedV;
                }
                else if (propertie.Key == PropertyNames._EffectColor)
                {
                    poperty.colorValue = m_Mode.Color;
                }
            }
        }
        public SharedEffectMode GetModel()
        {
            return m_Mode;
        }
        public Popertie GetPopertie(string key)
        {
            Popertie p = null;
            if (m_PopertieDic.TryGetValue(key, out p))
            {
                return p;
            }
            return null;
        }
        public override void DrawEffectsOptions(MaterialEditor materialEditor, int indentLevel)
        {
        }
    }
}