using System;
using CommandPalette.Plugins;
using CommandPalette.Settings;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using CommandPalette.Utils;
using CommandPalette.Views;
using UnityEngine.Experimental.Rendering;
using Object = UnityEngine.Object;

namespace CommandPalette {
    public class CommandPaletteWindow : EditorWindow {
        private const float k_BaseWidth = 680.0f;
        private const float k_YOffset = 200.0f;
        private const float k_MinHeight = 128.0f;

        private static Object s_MainUnityEditorWindow;
        private static Rect s_MainUnityEditorWindowRect;
        private static Vector2 s_UnityEditorWindowPosition;
        private static Vector2Int s_UnityEditorWindowSize;

        private static Texture2D s_BackgroundTexture;
        private static Texture2D s_BlurredTexture;
        private static Texture2D s_WindowIconTexture;
        private static CommandPaletteSettings s_Settings;

        private static StyleSheet s_Stylesheet;

        [MenuItem("Tools/Command Palette &]")]
        private static void ShowWindow() {
            CommandPaletteWindow[] windows = Resources.FindObjectsOfTypeAll<CommandPaletteWindow>();
            foreach (CommandPaletteWindow commandPaletteWindow in windows) {
                commandPaletteWindow.Close();
            }

            CommandPaletteWindow window = CreateInstance<CommandPaletteWindow>();

            if (s_Settings == null) {
                s_Settings = CommandPaletteSettings.GetOrCreateSettings();
            }

            InitializeBackground();

            if (s_WindowIconTexture == null) {
                s_WindowIconTexture = ResourceLoader.Load<Texture2D>("Textures/window_icon.png");
            }

            window.minSize = new Vector2(k_BaseWidth, 0);
            window.position = new Rect(s_UnityEditorWindowPosition.x + 0.5f * s_UnityEditorWindowSize.x - 0.5f * k_BaseWidth, s_UnityEditorWindowPosition.y + k_YOffset, k_BaseWidth, k_MinHeight);
            window.titleContent = new GUIContent("Command Palette", s_WindowIconTexture);
            window.m_LastRefreshTime = DateTime.Now;
            window.ShowPopup();
        }

        private static void InitializeBackground() {
            if (s_Settings == null) {
                s_Settings = CommandPaletteSettings.GetOrCreateSettings();
            }

            if (s_MainUnityEditorWindow == null) {
                s_MainUnityEditorWindow = UnityExtensions.GetEditorMainWindow();
                s_MainUnityEditorWindowRect = UnityExtensions.GetEditorMainWindowPos(s_MainUnityEditorWindow);
                s_UnityEditorWindowPosition = s_MainUnityEditorWindowRect.position;
                s_UnityEditorWindowSize = new Vector2Int((int)s_MainUnityEditorWindowRect.width, (int)s_MainUnityEditorWindowRect.height);
            }

            if (s_BackgroundTexture == null || s_BlurredTexture == null) {
                s_BackgroundTexture = new Texture2D(s_UnityEditorWindowSize.x, s_UnityEditorWindowSize.y, TextureFormat.RGBA32, false, true);
                s_BlurredTexture = new Texture2D(s_BackgroundTexture.width, s_BackgroundTexture.height, s_BackgroundTexture.graphicsFormat, TextureCreationFlags.None);
            }

            Color[] screen = UnityEditorInternal.InternalEditorUtility.ReadScreenPixel(s_UnityEditorWindowPosition, s_UnityEditorWindowSize.x, s_UnityEditorWindowSize.y);
            s_BackgroundTexture.SetPixels(screen);
            s_BackgroundTexture.Apply(false, false);

            Blur.BlurTexture(s_BackgroundTexture, s_BlurredTexture, s_Settings.BlurDownSample, s_Settings.BlurSize, s_Settings.BlurPasses, s_Settings.BlurTintColor, s_Settings.BlurTintAmount, s_Settings.Vibrancy);
        }

        private View m_View;
        private VisualElement m_ViewElement;
        private VisualElement m_Root;
        private DateTime m_LastRefreshTime;

        private void OnGUI() {
            if (m_View == null) return;

            Event current = new Event();
            while (Event.PopEvent(current)) {
                m_View.OnEvent(current);
            }
        }

        private void CreateGUI() {
            if (s_Stylesheet == null) {
                LoadStylesheets();
            }

            foreach (IPlugin plugin in PluginManager.GetPlugins()) {
                plugin.Window = this;
            }

            m_Root = new VisualElement().WithName("CommandPalette").WithStylesheet(s_Stylesheet);
            m_Root.styleSheets.Add(s_Stylesheet);
            m_Root.Add(new IMGUIContainer(DrawTexture).WithName("Background"));
            m_Root.RegisterCallback<KeyUpEvent>(evt => {
                if (evt.shiftKey && evt.keyCode == KeyCode.Escape) {
                    Close();
                } else {
                    m_View?.OnEvent(new Event {
                        capsLock = evt.modifiers.HasFlag(EventModifiers.CapsLock),
                        character = evt.character,
                        alt = evt.altKey,
                        command = evt.commandKey,
                        control = evt.ctrlKey,
                        shift = evt.shiftKey,
                        modifiers = evt.modifiers,
                        keyCode = evt.keyCode,
                    });
                }
            });

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
            position = new Rect(s_UnityEditorWindowPosition.x + 0.5f * s_UnityEditorWindowSize.x - 0.5f * k_BaseWidth, s_UnityEditorWindowPosition.y + k_YOffset, k_BaseWidth, Mathf.Max(height, k_MinHeight));
        }

        private void DrawTexture() {
            if (s_BlurredTexture == null) {
                ReloadBackgroundTexture();
            }

            Rect currentPosition = position;
            int offsetX = -(int)(currentPosition.x - s_UnityEditorWindowPosition.x);
            int offsetY = -(int)(currentPosition.y - s_UnityEditorWindowPosition.y);
            GUI.DrawTextureWithTexCoords(new Rect(offsetX, offsetY, s_UnityEditorWindowSize.x, s_UnityEditorWindowSize.y), s_BlurredTexture, new Rect(0, 0, 1, 1));

            if (s_Settings == null) {
                s_Settings = CommandPaletteSettings.GetOrCreateSettings();
            }

            if (!s_Settings.RefreshBlur) {
                return;
            }

            if ((DateTime.Now - m_LastRefreshTime).TotalSeconds >= s_Settings.RefreshBlurFrequency) {
                m_LastRefreshTime = DateTime.Now;
                ReloadBackgroundTexture();
            }
        }

        private void ReloadBackgroundTexture() {
            if (s_Settings == null) {
                s_Settings = CommandPaletteSettings.GetOrCreateSettings();
            }

            if (s_MainUnityEditorWindow == null) {
                s_MainUnityEditorWindow = UnityExtensions.GetEditorMainWindow();
                s_MainUnityEditorWindowRect = UnityExtensions.GetEditorMainWindowPos(s_MainUnityEditorWindow);
                s_UnityEditorWindowPosition = s_MainUnityEditorWindowRect.position;
                s_UnityEditorWindowSize = new Vector2Int((int)s_MainUnityEditorWindowRect.width, (int)s_MainUnityEditorWindowRect.height);
            }

            if (s_BackgroundTexture == null) {
                s_BackgroundTexture = new Texture2D(s_UnityEditorWindowSize.x, s_UnityEditorWindowSize.y, TextureFormat.RGBA32, false, true);
            }

            if (s_BlurredTexture == null) {
                s_BlurredTexture = new Texture2D(s_BackgroundTexture.width, s_BackgroundTexture.height, s_BackgroundTexture.graphicsFormat, TextureCreationFlags.None);
            }

            Rect oldPosition = position;
            position = Rect.zero;
            Color[] screen = UnityEditorInternal.InternalEditorUtility.ReadScreenPixel(s_UnityEditorWindowPosition, s_UnityEditorWindowSize.x, s_UnityEditorWindowSize.y);
            position = oldPosition;
            s_BackgroundTexture.SetPixels(screen);
            s_BackgroundTexture.Apply(false, false);

            Blur.BlurTexture(s_BackgroundTexture, s_BlurredTexture, s_Settings.BlurDownSample, s_Settings.BlurSize, s_Settings.BlurPasses, s_Settings.BlurTintColor, s_Settings.BlurTintAmount, s_Settings.Vibrancy);
        }

        private static void LoadStylesheets() {
            s_Stylesheet = ResourceLoader.Load<StyleSheet>("StyleSheets/CommandPalette.uss");
        }
    }
}