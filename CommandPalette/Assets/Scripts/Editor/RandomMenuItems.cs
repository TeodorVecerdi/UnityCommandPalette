using UnityEditor;
using UnityEngine;

namespace Editor {
    public static class RandomMenuItems {
        [MenuItem("Tools/Find All Windows", false, 101)]
        private static void FindAllWindows() {
            EditorWindow[] windows = Resources.FindObjectsOfTypeAll<EditorWindow>();
            foreach (EditorWindow window in windows) {
                Debug.Log($"{window.titleContent} ({window.GetType()})");
            }
        }
    }
}