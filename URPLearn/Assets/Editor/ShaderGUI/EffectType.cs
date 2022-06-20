using System;
using System.Collections.Generic;


namespace LighadEngine.PBSShaderGUI
{
    public static class EffectTypes
    {
        private static List<Type> s_Types = new List<Type>();

        public static void RegisterBatcherType(Type type)
        {
            s_Types.Add(type);
        }

        public static void UnregisterBatcherType(Type type)
        {
            s_Types.Remove(type);
        }

        public static Type[] GetTypes()
        {
            return s_Types.ToArray();
        }
    }



}