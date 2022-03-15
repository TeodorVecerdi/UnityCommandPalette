using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using CommandPalette.Utils;
using CommandPalette.Views;

namespace CommandPalette {
    public class CommandPaletteWindow : EditorWindow {
        private static Rect mainWindowRect;
        private static Vector2 windowPosition;
        private static Vector2Int windowSize;
        private static Texture2D backgroundTexture;
        private static Texture blurredTexture;

        [MenuItem("Tools/Destroy All Command Palettes")]
        private static void DestroyAllCommandPalettes() {
            CommandPaletteWindow[] windows = Resources.FindObjectsOfTypeAll<CommandPaletteWindow>();
            foreach (CommandPaletteWindow window in windows) {
                try {
                    window.Close();
                } catch {
                    DestroyImmediate(window);
                }
            }
        }

        [MenuItem("Tools/Command Palette &]")]
        private static void ShowWindow() {
            CommandPaletteWindow[] windows = Resources.FindObjectsOfTypeAll<CommandPaletteWindow>();
            foreach (CommandPaletteWindow commandPaletteWindow in windows) {
                commandPaletteWindow.Close();
            }

            CommandPaletteWindow window = CreateInstance<CommandPaletteWindow>();
            InitializeBackground();

            window.minSize = new Vector2(k_BaseWidth, 50);
            window.position = new Rect(windowPosition.x + 0.5f * windowSize.x - 0.5f * k_BaseWidth, windowPosition.y + k_YOffset, k_BaseWidth, 50);
            window.ShowPopup();
        }

        private static void InitializeBackground() {
            mainWindowRect = UnityExtensions.GetEditorMainWindowPos();
            windowPosition = mainWindowRect.position;
            windowSize = new Vector2Int((int)mainWindowRect.width, (int)mainWindowRect.height);

            if (backgroundTexture == null) {
                backgroundTexture = new Texture2D(windowSize.x, windowSize.y, TextureFormat.RGBA32, false, true);
            }

            Color[] screen = UnityEditorInternal.InternalEditorUtility.ReadScreenPixel(windowPosition, windowSize.x, windowSize.y);
            backgroundTexture.SetPixels(screen);
            backgroundTexture.Apply(false, false);

            if (blurredTexture != null) {
                DestroyImmediate(blurredTexture);
            }

            Blur blur = new Blur();
            blurredTexture = blur.BlurTexture(backgroundTexture, downSample, blurSize, blurPasses, blurTintColor, blurTintAmount);
        }

        private const float k_BaseWidth = 680.0f;
        private const float k_YOffset = 200.0f;

        private static readonly int downSample = 1;
        private static readonly float blurSize = 1.3f;
        private static readonly int blurPasses = 32;
        private static readonly Color blurTintColor = Color.black;
        private static readonly float blurTintAmount = 0.1f;

        private static StyleSheet s_stylesheet;

        private View m_View;
        private VisualElement m_ViewElement;
        private VisualElement m_Root;

        private void OnGUI() {
            if (m_View == null) return;

            Event current = new Event();
            while (Event.PopEvent(current)) {
                m_View.OnEvent(current);
            }
        }

        private void CreateGUI() {
            if (s_stylesheet == null) {
                LoadStylesheets();
            }

            m_Root = new VisualElement().WithName("CommandPalette").WithStylesheet(s_stylesheet);
            m_Root.styleSheets.Add(s_stylesheet);
            m_Root.Add(new IMGUIContainer(DrawTexture).WithName("Background"));

            SwitchToView<MainView>();

            rootVisualElement.Add(m_Root);
        }

        public void SwitchToView<T>(Action<T> initializeView = null) where T : View, new() {
            if (m_ViewElement != null) {
                m_Root.Remove(m_ViewElement);
            }

            m_View = new T { Window = this };
            initializeView?.Invoke(m_View as T);

            m_ViewElement = m_View.Build();
            m_Root.Add(m_ViewElement);
        }

        public void SetHeight(float height) {
            position = new Rect(windowPosition.x + 0.5f * windowSize.x - 0.5f * k_BaseWidth, windowPosition.y + k_YOffset, k_BaseWidth, height);
        }

        private void DrawTexture() {
            if (blurredTexture == null) {
                ReloadBackgroundTexture();
            }

            Rect currentPosition = position;
            int offsetX = -(int)(currentPosition.x - windowPosition.x);
            int offsetY = -(int)(currentPosition.y - windowPosition.y);
            GUI.DrawTextureWithTexCoords(new Rect(offsetX, offsetY, windowSize.x, windowSize.y), blurredTexture, new Rect(0, 0, 1, 1));
        }

        private void ReloadBackgroundTexture() {
            Rect oldPosition = position;
            position = Rect.zero;
            InitializeBackground();
            position = oldPosition;
        }

        private static void LoadStylesheets() {
            s_stylesheet = Resources.Load<StyleSheet>("CommandPalette/Stylesheets/CommandPalette");
        }
    }
}