using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEditor;


namespace ShaderGUIBase
{
    public class ShaderGUIBase : ShaderGUI
    {
        public const string FoldoutSign = "_Foldout";

        public bool FoldoutEditor { get { return _foldoutEditor; } }

        //折叠页中的属性是否可以被编辑
        private bool _foldoutEditor = true;

        //折叠页状态, true展开, false折叠
        private bool _foldoutOpen = true;

        public override void OnGUI(MaterialEditor materialEditor , MaterialProperty[] Properties)
        {
            foreach (var prop in Properties)
            {
                if (!IsFoldout(prop))
                {
                    EditorGUI.BeginDisabledGroup(!_foldoutEditor);
                }

                if (_foldoutOpen|| IsFoldout(prop))
                {
                    materialEditor.ShaderProperty(prop, prop.displayName);
                }
                if (!IsFoldout(prop))
                    EditorGUI.EndDisabledGroup();
            }


            if (_foldoutOpen)
            {
                EditorGUI.BeginDisabledGroup(!_foldoutEditor);
                //双面全局光照UI
                materialEditor.DoubleSidedGIField();
                //绘制调节队列的控件
                materialEditor.RenderQueueField();
                EditorGUI.EndDisabledGroup();
            }


        }

        bool IsFoldout(MaterialProperty property)
        {
            string pattern = FoldoutSign + @"\s*$";
            return Regex.IsMatch(property.displayName, pattern);
        }
    }

}
