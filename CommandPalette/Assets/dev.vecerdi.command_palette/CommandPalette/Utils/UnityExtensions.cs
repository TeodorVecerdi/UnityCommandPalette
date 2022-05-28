using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CommandPalette.Utils {
    public static class UnityExtensions {
        public static Type[] GetAllDerivedTypes(this AppDomain aAppDomain, Type aType) {
            List<Type> result = new List<Type>();
            Assembly[] assemblies = aAppDomain.GetAssemblies();
            foreach (Assembly assembly in assemblies) {
                Type[] types = assembly.GetTypes();
                foreach (Type type in types)
                    if (type.IsSubclassOf(aType))
                        result.Add(type);
            }

            return result.ToArray();
        }
        
        private static Type s_containerWinType;
        private static FieldInfo s_showModeField;
        private static PropertyInfo s_positionProperty;

        public static Object GetEditorMainWindow() {
            if (s_containerWinType == null) {
                s_containerWinType = AppDomain.CurrentDomain.GetAllDerivedTypes(typeof(ScriptableObject)).FirstOrDefault(t => t.Name == "ContainerWindow");
                if (s_containerWinType == null)
                    throw new MissingMemberException("Can't find internal type ContainerWindow. Maybe something has changed inside Unity");
                s_showModeField = s_containerWinType.GetField("m_ShowMode", BindingFlags.NonPublic | BindingFlags.Instance);
                s_positionProperty = s_containerWinType.GetProperty("position", BindingFlags.Public | BindingFlags.Instance);
                if (s_showModeField == null || s_positionProperty == null)
                    throw new MissingFieldException("Can't find internal fields 'm_ShowMode' or 'position'. Maybe something has changed inside Unity");
            }
            Object[] windows = Resources.FindObjectsOfTypeAll(s_containerWinType);
            foreach (Object win in windows) {
                int showmode = (int)s_showModeField.GetValue(win);
                if (showmode == 4) {
                    return win;
                }
            }

            throw new NotSupportedException("Can't find internal main window. Maybe something has changed inside Unity");
        }

        public static Rect GetEditorMainWindowPos(Object window) {
            if (window == null) {
                throw new ArgumentNullException(nameof(window));
            }

            return (Rect)s_positionProperty.GetValue(window, null);
        }
    }
}