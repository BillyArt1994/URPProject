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

        //�۵�ҳ�е������Ƿ���Ա��༭
        private bool _foldoutEditor = true;

        //�۵�ҳ״̬, trueչ��, false�۵�
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
                //˫��ȫ�ֹ���UI
                materialEditor.DoubleSidedGIField();
                //���Ƶ��ڶ��еĿؼ�
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
