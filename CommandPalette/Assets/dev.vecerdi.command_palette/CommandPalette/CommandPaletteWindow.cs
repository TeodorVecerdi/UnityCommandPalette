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

        private static Object s_mainUnityEditorWindow;
        private static Rect s_mainUnityEditorWindowRect;
        private static Vector2 s_unityEditorWindowPosition;
        private static Vector2Int s_unityEditorWindowSize;

        private static Texture2D s_backgroundTexture;
        private static Texture2D s_blurredTexture;
        private static Texture2D s_windowIconTexture;
        private static CommandPaletteSettings s_settings;

        private static StyleSheet s_stylesheet;

        [MenuItem("Tools/Command Palette &]")]
        private static void ShowWindow() {
            CommandPaletteWindow[] windows = Resources.FindObjectsOfTypeAll<CommandPaletteWindow>();
            foreach (CommandPaletteWindow commandPaletteWindow in windows) {
                commandPaletteWindow.Close();
            }

            CommandPaletteWindow window = CreateInstance<CommandPaletteWindow>();

            if (s_settings == null) {
                s_settings = CommandPaletteSettings.GetOrCreateSettings();
            }

            InitializeBackground();

            if (s_windowIconTexture == null) {
                s_windowIconTexture = Resources.Load<Texture2D>("CommandPalette/Textures/window_icon");
            }

            window.minSize = new Vector2(k_BaseWidth, 0);
            window.position = new Rect(s_unityEditorWindowPosition.x + 0.5f * s_unityEditorWindowSize.x - 0.5f * k_BaseWidth, s_unityEditorWindowPosition.y + k_YOffset, k_BaseWidth, k_MinHeight);
            window.titleContent = new GUIContent("Command Palette", s_windowIconTexture);
            window.m_LastRefreshTime = DateTime.Now;
            window.ShowPopup();
        }

        private static void InitializeBackground() {
            if (s_settings == null) {
                s_settings = CommandPaletteSettings.GetOrCreateSettings();
            }

            if (s_mainUnityEditorWindow == null) {
                s_mainUnityEditorWindow = UnityExtensions.GetEditorMainWindow();
                s_mainUnityEditorWindowRect = UnityExtensions.GetEditorMainWindowPos(s_mainUnityEditorWindow);
                s_unityEditorWindowPosition = s_mainUnityEditorWindowRect.position;
                s_unityEditorWindowSize = new Vector2Int((int)s_mainUnityEditorWindowRect.width, (int)s_mainUnityEditorWindowRect.height);
            }

            if (s_backgroundTexture == null || s_blurredTexture == null) {
                s_backgroundTexture = new Texture2D(s_unityEditorWindowSize.x, s_unityEditorWindowSize.y, TextureFormat.RGBA32, false, true);
                s_blurredTexture = new Texture2D(s_backgroundTexture.width, s_backgroundTexture.height, s_backgroundTexture.graphicsFormat, TextureCreationFlags.None);
            }

            Color[] screen = UnityEditorInternal.InternalEditorUtility.ReadScreenPixel(s_unityEditorWindowPosition, s_unityEditorWindowSize.x, s_unityEditorWindowSize.y);
            s_backgroundTexture.SetPixels(screen);
            s_backgroundTexture.Apply(false, false);

            Blur.BlurTexture(s_backgroundTexture, s_blurredTexture, s_settings.BlurDownSample, s_settings.BlurSize, s_settings.BlurPasses, s_settings.BlurTintColor, s_settings.BlurTintAmount);
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
            if (s_stylesheet == null) {
                LoadStylesheets();
            }

            foreach (IPlugin plugin in PluginManager.GetPlugins()) {
                plugin.Window = this;
            }

            m_Root = new VisualElement().WithName("CommandPalette").WithStylesheet(s_stylesheet);
            m_Root.styleSheets.Add(s_stylesheet);
            m_Root.Add(new IMGUIContainer(DrawTexture).WithName("Background"));
            m_Root.RegisterCallback<KeyUpEvent>(evt => {
                if (evt.shiftKey && evt.keyCode == KeyCode.Escape) {
                    Close();
                } else if (m_View != null) {
                    m_View.OnEvent(new Event {
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
            position = new Rect(s_unityEditorWindowPosition.x + 0.5f * s_unityEditorWindowSize.x - 0.5f * k_BaseWidth, s_unityEditorWindowPosition.y + k_YOffset, k_BaseWidth, Mathf.Max(height, k_MinHeight));
        }

        private void DrawTexture() {
            if (s_blurredTexture == null) {
                ReloadBackgroundTexture();
            }

            Rect currentPosition = position;
            int offsetX = -(int)(currentPosition.x - s_unityEditorWindowPosition.x);
            int offsetY = -(int)(currentPosition.y - s_unityEditorWindowPosition.y);
            GUI.DrawTextureWithTexCoords(new Rect(offsetX, offsetY, s_unityEditorWindowSize.x, s_unityEditorWindowSize.y), s_blurredTexture, new Rect(0, 0, 1, 1));

            if (s_settings == null) {
                s_settings = CommandPaletteSettings.GetOrCreateSettings();
            }

            if (!s_settings.RefreshBlur) {
                return;
            }

            if ((DateTime.Now - m_LastRefreshTime).TotalSeconds >= s_settings.RefreshBlurFrequency) {
                m_LastRefreshTime = DateTime.Now;
                ReloadBackgroundTexture();
            }
        }

        private void ReloadBackgroundTexture() {
            if (s_settings == null) {
                s_settings = CommandPaletteSettings.GetOrCreateSettings();
            }

            if (s_mainUnityEditorWindow == null) {
                s_mainUnityEditorWindow = UnityExtensions.GetEditorMainWindow();
                s_mainUnityEditorWindowRect = UnityExtensions.GetEditorMainWindowPos(s_mainUnityEditorWindow);
                s_unityEditorWindowPosition = s_mainUnityEditorWindowRect.position;
                s_unityEditorWindowSize = new Vector2Int((int)s_mainUnityEditorWindowRect.width, (int)s_mainUnityEditorWindowRect.height);
            }

            if (s_backgroundTexture == null) {
                s_backgroundTexture = new Texture2D(s_unityEditorWindowSize.x, s_unityEditorWindowSize.y, TextureFormat.RGBA32, false, true);
            }

            if (s_blurredTexture == null) {
                s_blurredTexture = new Texture2D(s_backgroundTexture.width, s_backgroundTexture.height, s_backgroundTexture.graphicsFormat, TextureCreationFlags.None);
            }

            Rect oldPosition = position;
            position = Rect.zero;
            Color[] screen = UnityEditorInternal.InternalEditorUtility.ReadScreenPixel(s_unityEditorWindowPosition, s_unityEditorWindowSize.x, s_unityEditorWindowSize.y);
            position = oldPosition;
            s_backgroundTexture.SetPixels(screen);
            s_backgroundTexture.Apply(false, false);

            Blur.BlurTexture(s_backgroundTexture, s_blurredTexture, s_settings.BlurDownSample, s_settings.BlurSize, s_settings.BlurPasses, s_settings.BlurTintColor, s_settings.BlurTintAmount);
        }

        private static void LoadStylesheets() {
            s_stylesheet = Resources.Load<StyleSheet>("CommandPalette/Stylesheets/CommandPalette");
        }
    }
}