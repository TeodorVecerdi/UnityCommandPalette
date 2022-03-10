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

        public static Rect GetEditorMainWindowPos() {
            Type containerWinType = AppDomain.CurrentDomain.GetAllDerivedTypes(typeof(ScriptableObject)).FirstOrDefault(t => t.Name == "ContainerWindow");
            if (containerWinType == null)
                throw new MissingMemberException("Can't find internal type ContainerWindow. Maybe something has changed inside Unity");
            FieldInfo showModeField = containerWinType.GetField("m_ShowMode", BindingFlags.NonPublic | BindingFlags.Instance);
            PropertyInfo positionProperty = containerWinType.GetProperty("position", BindingFlags.Public | BindingFlags.Instance);
            if (showModeField == null || positionProperty == null)
                throw new MissingFieldException("Can't find internal fields 'm_ShowMode' or 'position'. Maybe something has changed inside Unity");
            Object[] windows = Resources.FindObjectsOfTypeAll(containerWinType);
            foreach (Object win in windows) {
                int showmode = (int)showModeField.GetValue(win);
                if (showmode == 4) {
                    Rect pos = (Rect)positionProperty.GetValue(win, null);
                    return pos;
                }
            }

            throw new NotSupportedException("Can't find internal main window. Maybe something has changed inside Unity");
        }
    }
}