using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class PBRShaderGUIBaseTest : ShaderGUI
{
    

    // 折叠栏
    private bool m_GroupFlag = true;

    public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
    {
        m_GroupFlag = EditorGUILayout.BeginFoldoutHeaderGroup(m_GroupFlag, "GUI折叠");
        if (m_GroupFlag)
        {
            //MaterialProperty _MainTex = FindProperty("_MainTex", properties);
            //GUIContent mainTex = new GUIContent("主贴图");
            //materialEditor.TextureProperty(_MainTex, "主贴图");
            //MaterialProperty _Value = FindProperty("_Value", properties);
            //MaterialProperty _Test = FindProperty("_Test", properties);
            //materialEditor.ShaderProperty(_Value, "B_Value");
            //materialEditor.ShaderProperty(_Test, "test");
             base.OnGUI(materialEditor, properties);
        }

        
    }

    public void DrawEffectProperty()
    {

    }



}
