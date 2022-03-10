using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.Experimental;
using UnityEngine;

public static class EditorGUIIconGenerator {
    public const string compileFlag = "EDITOR_ICONS_GENERATED";
    private const string generatedEnumFileName = "EditorGUIIcons";
    private const string defaultGeneratedEnumRelativeFilePath = "Assets/Plugins/EditorGUIIcons/Editor/" + generatedEnumFileName + ".cs";

    #region Template Strings

    private const string resetContent = @"
using System.Collections.Generic;
using UnityEngine;

public static class EditorGUIIcons
{
    public const string BUILD_VERSION = ""0"";
    public const int VALUE_COUNT = 1;
    public const string VALUE_COUNT_STRING = ""1"";

    public enum Enum
    {
        None = 0
    }

    internal enum SortEnum
    {
        None = 0
    }

    public static GUIContent GetIconContent(string iconName)
    {
        return GUIContent.none;
    }

    public static GUIContent GetIconContent(string iconName, string tooltip)
    {
        return GUIContent.none;
    }

    public static GUIContent GetIconContent(Enum icon)
    {
        return GUIContent.none;
    }

    public static GUIContent GetIconContent(Enum icon, string tooltip)
    {
        return GUIContent.none;
    }

    public static string GetIconName(Enum icon)
    {
        return null;
    }
    
    [RuntimeInitializeOnLoadMethod]
    public static void RemoveScriptingDefine()
    {
        EditorGUIIconGenerator.RemoveScriptingDefine();
    }
}
";

    private const string generatorString = @"
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public static class EditorGUIIcons
{
    public const string BUILD_VERSION = ""###VERSION###"";
    public const int VALUE_COUNT = ###COUNT###;
    public const string VALUE_COUNT_STRING = ""###COUNT###"";

#region Enumerations

    public enum Enum
    {
        None = 0,
###ENUM###
    }

    internal enum SortEnum
    {
        None = 0,
###SORTENUM###
    }

#endregion

#region Constants

###LIST###

#endregion

#region Initialization

#if UNITY_EDITOR
    public static readonly Dictionary<Enum, string> _iconLookup;
    public static readonly Dictionary<string, Enum> _reverseIconLookup;

    static EditorGUIIcons()
    {
        _iconLookup = new Dictionary<Enum, string>();
        _reverseIconLookup = new Dictionary<string, Enum>();

        _iconLookup.Add(Enum.None, ""None"");
        _reverseIconLookup.Add(""None"", Enum.None);

###LOOKUP###
    }
#endif

#endregion

#region Methods

    public static GUIContent GetIconContent(Enum icon)
    {
#if UNITY_EDITOR
        return UnityEditor.EditorGUIUtility.IconContent(_iconLookup[icon]);
#else
        return GUIContent.none;
#endif
    }

    public static GUIContent GetIconContent(Enum icon, string tooltip)
    {
#if UNITY_EDITOR
        return UnityEditor.EditorGUIUtility.IconContent(_iconLookup[icon], tooltip);
#else
        return GUIContent.none;
#endif
    }

    public static GUIContent GetIconContent(string iconName)
    {
#if UNITY_EDITOR
        return UnityEditor.EditorGUIUtility.IconContent(iconName);
#else
        return GUIContent.none;
#endif
    }

    public static GUIContent GetIconContent(string iconName, string tooltip)
    {
#if UNITY_EDITOR
        return UnityEditor.EditorGUIUtility.IconContent(iconName, tooltip);
#else
        return GUIContent.none;
#endif
    }

    public static string GetIconName(Enum icon)
    {
#if UNITY_EDITOR
        return _iconLookup[icon];
#else
        return null;
#endif
    }

    public static Enum GetIconEnum(string name)
    {
#if UNITY_EDITOR
        return _reverseIconLookup[name];
#else
        return default;
#endif
    }

#endregion

#region UI

###EDITOR###

#endregion
}
";

    private const string editorString = @"
    #if UNITY_EDITOR
    public class EditorGUIIconViewer : EditorWindow
    {
#region UI Constants

        private const string _headerText = ""EditorGUI Icons"";

        private const string _subheaderText = ""Build Version: "" +
                                              BUILD_VERSION +
                                              "" | Icon Count: "" +
                                              VALUE_COUNT_STRING;

        private const string _settingsText = ""Display Settings"";
        private const string _searchText = ""Search (RegEx)"";
        private const string _selectedIconText = ""Selected Icon Name"";
        private const string _buttonTextPadding = ""  "";

        private static readonly Color lineColor = new(.22f, .22f, .22f, 1f);

        private const int _defaultIconSize = 32;
        private const int _iconPadding = 1;
        private const float _paddingMultiplier = 3f;
        private const int _reservedSpace = 15;
        private const float _verticalSpacer = 8f;
        private const float _horizontalSpacer = 4f;
        private const float _selectionSize = 1f;
        private const int _buttonIconSize = 16;
        private const float _iconSizeLabelSize = 60f;
        private const float _iconBackgroundLabelSize = 110f;
        private const float _iconSelectedBackgroundLabelSize = 120f;
        private const float _searchLabelSize = 120f;
        private const float _regexLabelSize = 18f;
        private const float _selectedIconLabelSize = 130f;

#endregion

#region Icons

        private const string _regenerateButtonIconName = ""sceneviewtools on"";
        private const string _resetButtonIconName = ""grid.erasertool"";
        private const string _searchIconName = ""searchoverlay"";
        private const string _settingsIconName = ""settings"";
        private const string _regexValidIconName = ""p4_checkoutremote"";
        private const string _regexInvalidIconName = ""p4_deletedlocal"";
        private const string _regexMissingIconName = ""testignored"";

#endregion

#region State

        private static Enum[] _enums;
        private static string[] _iconNames;
        private static GUIContent[] _icons;

#endregion

#region UI State

        private string _searchFilter = """";
        private Color _backgroundColor = new(0.1647059f, 0.1647059f, 0.1647059f, 1f);
        private Color _selectedBackgroundColor = new(0f, 1f, 0f, .7f);
        private int _iconSize = _defaultIconSize;
        private bool _showSettings;

        private RegexState _regexState;
        private Vector2 _scrollPosition;

        private Color _lastBackgroundColor;
        private int _lastIconSize;
        private string _lastSelection = """";
        private GUIContent _lastSelectionContent;

#endregion

#region Layout

#region Fields

        private static readonly GUILayoutOption[] _expandWidth = {GUILayout.ExpandWidth(true)};
        private GUILayoutOption[] _iconLayout = {GUILayout.Width(_defaultIconSize)};
        private GUILayoutOption[] _iconSizeLabelLayout = {GUILayout.Width(_iconSizeLabelSize)};
        private GUILayoutOption[] _regexIconLayout = {GUILayout.Width(_regexLabelSize)};
        private GUILayoutOption[] _searchLabelLayout = {GUILayout.Width(_searchLabelSize)};

        private GUILayoutOption[] _iconBackgroundLabelLayout =
        {
            GUILayout.Width(_iconBackgroundLabelSize)
        };

        private GUILayoutOption[] _iconSelectedBackgroundLabelLayout =
        {
            GUILayout.Width(_iconSelectedBackgroundLabelSize)
        };

        private GUILayoutOption[] _selectedIconLabelLayout =
        {
            GUILayout.Width(_selectedIconLabelSize)
        };

#endregion

#region Properties

        private GUILayoutOption[] iconSizeLabelLayout
        {
            get
            {
                if (_iconSizeLabelLayout == null)
                {
                    _iconSizeLabelLayout = new[] {GUILayout.Width(_iconSizeLabelSize)};
                }

                return _iconSizeLabelLayout;
            }
        }

        private GUILayoutOption[] iconBackgroundLabelLayout
        {
            get
            {
                if (_iconBackgroundLabelLayout == null)
                {
                    _iconBackgroundLabelLayout = new[] {GUILayout.Width(_iconBackgroundLabelSize)};
                }

                return _iconBackgroundLabelLayout;
            }
        }

        private GUILayoutOption[] iconSelectedBackgroundLabelLayout
        {
            get
            {
                if (_iconSelectedBackgroundLabelLayout == null)
                {
                    _iconSelectedBackgroundLabelLayout = new[]
                    {
                        GUILayout.Width(_iconSelectedBackgroundLabelSize)
                    };
                }

                return _iconSelectedBackgroundLabelLayout;
            }
        }

        private GUILayoutOption[] searchLabelLayout
        {
            get
            {
                if (_searchLabelLayout == null)
                {
                    _searchLabelLayout = new[] {GUILayout.Width(_searchLabelSize)};
                }

                return _searchLabelLayout;
            }
        }

        private GUILayoutOption[] regexIconLayout
        {
            get
            {
                if (_regexIconLayout == null)
                {
                    _regexIconLayout = new[] {GUILayout.Width(_regexLabelSize)};
                }

                return _regexIconLayout;
            }
        }

        private GUILayoutOption[] selectedIconLabelLayout
        {
            get
            {
                if (_selectedIconLabelLayout == null)
                {
                    _selectedIconLabelLayout = new[] {GUILayout.Width(_selectedIconLabelSize)};
                }

                return _selectedIconLabelLayout;
            }
        }

#endregion

#endregion

#region Style

#region Fields

        private GUIStyle _scrollViewStyle;
        private GUIStyle _iconStyle;
        private GUIStyle _regexIconStyle;

#endregion

#region Properties

        private GUIStyle scrollViewStyle
        {
            get
            {
                if (_scrollViewStyle == null)
                {
                    _scrollViewStyle = new GUIStyle(GUI.skin.scrollView)
                    {
                    };

                    _scrollViewStyle.normal.background = MakeTex(1, 1, _backgroundColor);
                }

                return _scrollViewStyle;
            }
        }
        
        private GUIStyle iconStyle
        {
            get
            {
                if (_iconStyle == null)
                {
                    _iconStyle = new GUIStyle
                    {
                        fixedWidth = _iconSize,
                        padding = new RectOffset(
                            _iconPadding,
                            _iconPadding,
                            _iconPadding,
                            _iconPadding
                        ),
                        border = new RectOffset(0, 0, 0, 0)
                    };
                }

                return _iconStyle;
            }
        }

        private GUIStyle regexIconStyle
        {
            get
            {
                if (_regexIconStyle == null)
                {
                    _regexIconStyle = new GUIStyle
                    {
                        fixedWidth = _regexLabelSize,
                        padding = new RectOffset(
                            _iconPadding,
                            _iconPadding,
                            _iconPadding,
                            _iconPadding
                        ),
                        border = new RectOffset(0, 0, 0, 0)
                    };
                }

                return _regexIconStyle;
            }
        }

#endregion

#endregion

#region Content

#region Fields

        private GUIContent _regenerateButtonContent;
        private GUIContent _resetButtonContent;
        private GUIContent _settingsLabel;
        private GUIContent _searchLabel;
        private GUIContent _regexInvalidLabel;
        private GUIContent _regexMissingLabel;
        private GUIContent _regexValidLabel;
        private GUIContent _selectedIconLabel;

#endregion

#region Properties

        private GUIContent regenerateButtonContent
        {
            get
            {
                if (_regenerateButtonContent == null)
                {
                    var regenerateButtonIcon = GetIconEnum(_regenerateButtonIconName);
                    _regenerateButtonContent = GetIconContent(regenerateButtonIcon);

                    _regenerateButtonContent.text = _buttonTextPadding +
                                                    EditorGUIIconGenerator.regenerateButtonText;
                }

                return _regenerateButtonContent;
            }
        }

        private GUIContent resetButtonContent
        {
            get
            {
                if (_resetButtonContent == null)
                {
                    var resetButtonIcon = GetIconEnum(_resetButtonIconName);
                    _resetButtonContent = GetIconContent(resetButtonIcon);

                    _resetButtonContent.text =
                        _buttonTextPadding + EditorGUIIconGenerator.resetButtonText;
                }

                return _resetButtonContent;
            }
        }

        public GUIContent settingsLabel
        {
            get
            {
                if (_settingsLabel == null)
                {
                    var settingsButtonIcon = GetIconEnum(_settingsIconName);
                    _settingsLabel = GetIconContent(settingsButtonIcon);

                    _settingsLabel.text = _buttonTextPadding + _settingsText;
                }

                return _settingsLabel;
            }
        }

        public GUIContent searchLabel
        {
            get
            {
                if (_searchLabel == null)
                {
                    var searchButtonIcon = GetIconEnum(_searchIconName);
                    _searchLabel = GetIconContent(searchButtonIcon);

                    _searchLabel.text = _buttonTextPadding + _searchText;
                }

                return _searchLabel;
            }
        }

        public GUIContent regexValidLabel
        {
            get
            {
                if (_regexValidLabel == null)
                {
                    _regexValidLabel = new GUIContent(GetIconContent(_regexValidIconName));
                }

                return _regexValidLabel;
            }
        }

        public GUIContent regexInvalidLabel
        {
            get
            {
                if (_regexInvalidLabel == null)
                {
                    _regexInvalidLabel = new GUIContent(GetIconContent(_regexInvalidIconName));
                }

                return _regexInvalidLabel;
            }
        }

        public GUIContent regexMissingLabel
        {
            get
            {
                if (_regexMissingLabel == null)
                {
                    _regexMissingLabel = new GUIContent(GetIconContent(_regexMissingIconName));
                }

                return _regexMissingLabel;
            }
        }

        public GUIContent selectedIconLabel
        {
            get
            {
                if (_selectedIconLabel == null)
                {
                    _selectedIconLabel = new GUIContent(_selectedIconText);
                }

                return _selectedIconLabel;
            }
        }

        public GUIContent lastSelectionContent
        {
            get
            {
                if (_lastSelectionContent == null)
                {
                    _lastSelectionContent = new GUIContent(string.Empty);
                }

                return _lastSelectionContent;
            }
        }

#endregion

#endregion

#region Editor Window API

        private void OnEnable()
        {
            if ((Application.unityVersion != BUILD_VERSION) || (VALUE_COUNT == 0))
            {
                EditorGUIIconGenerator.RegenerateIconUtilities(true);
                return;
            }

            _lastIconSize = 0;
            _lastBackgroundColor = new Color();

            minSize = new Vector2(640, 640);

            var sortEnumValues = System.Enum.GetValues(typeof(SortEnum));
            var sortEnumEnumerable = sortEnumValues.Cast<SortEnum>();
            var sortEnumArray = sortEnumEnumerable.ToArray();
            var sortEnums = sortEnumArray.ToDictionary(e => e.ToString());

            _enums = System.Enum.GetValues(typeof(Enum)).Cast<Enum>().ToArray();

            Array.Sort(
                _enums,
                (a, b) => sortEnums[a.ToString()].CompareTo(sortEnums[b.ToString()])
            );
            _icons = new GUIContent[_enums.Length];
            _iconNames = new string[_enums.Length];

            for (var i = 0; i < _enums.Length; i++)
            {
                var e = _enums[i];
                var iconName = GetIconName(e);

                _iconNames[i] = iconName;
                var content = GetIconContent(e, iconName);
                content.tooltip = iconName;
                _icons[i] = content;
            }
        }

        private void OnGUI()
        {
            Action drawButtons = () =>
            {
                if (GUILayout.Button(regenerateButtonContent))
                {
                    EditorGUIIconGenerator.RegenerateIconUtilities(true);
                }

                if (GUILayout.Button(resetButtonContent))
                {
                    EditorGUIIconGenerator.ResetGeneratedIconUtilities();
                }

                EditorGUILayout.Space(_horizontalSpacer * 4, false);
            };


            DrawHeader(_headerText, _subheaderText, drawButtons);

            DrawSettings();

            EditorGUILayout.Space(_verticalSpacer, false);

            using (new EditorGUI.IndentLevelScope())
            {
                DrawSearch();

                DrawResult();

                EditorGUILayout.Space(_verticalSpacer, false);
                DrawIcons();
            }

            EditorGUILayout.Space(_verticalSpacer, false);

            if (GUILayout.Button(""Close""))
            {
                CloseWindow();
            }
        }

        private void CloseWindow()
        {
            Close();
            GUIUtility.ExitGUI();
        }

#endregion

#region UI Menu Items

        [MenuItem(""Tools/EditorGUI Icons/Explore"", false, 22)]
        internal static void ExploreEditorGUIIcons()
        {
            GetWindow<EditorGUIIconViewer>(false, ""EditorGUI Icons"", true);
        }

#endregion

#region UI Parts

        private void DrawHeader(string text, string subheader, Action additional)
        {
            EditorGUILayout.LabelField(text, EditorStyles.whiteLargeLabel);

            using (new GUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField(subheader, EditorStyles.whiteMiniLabel);

                if (additional != null)
                {
                    additional();
                }
            }

            HorizontalLineSeparator(lineColor, 4);
        }

        private void DrawSettings()
        {
            _showSettings = EditorGUILayout.Foldout(_showSettings, settingsLabel);

            if (_showSettings)
            {
                using (new EditorGUILayout.HorizontalScope())
                using (new EditorGUI.IndentLevelScope())
                {
                    EditorGUILayout.LabelField(""Icon Size"", iconSizeLabelLayout);
                    _iconSize = EditorGUILayout.IntSlider(_iconSize, 8, 256);

                    EditorGUILayout.Space(6f, false);

                    EditorGUILayout.LabelField(""Icon Background"", iconBackgroundLabelLayout);
                    _backgroundColor = EditorGUILayout.ColorField(_backgroundColor);

                    EditorGUILayout.Space(6f, false);

                    EditorGUILayout.LabelField(
                        ""Selected Outline"",
                        iconSelectedBackgroundLabelLayout
                    );
                    _selectedBackgroundColor = EditorGUILayout.ColorField(_selectedBackgroundColor);
                }
            }

            if (_lastIconSize != _iconSize)
            {
                _iconStyle = new GUIStyle(iconStyle) {fixedWidth = _iconSize, fixedHeight = _iconSize};
                _iconLayout = new[] {GUILayout.Width(_iconSize), GUILayout.Height(_iconSize)};
                _lastIconSize = _iconSize;
            }

            if (_lastBackgroundColor != _backgroundColor)
            {
                
                PaintTex(scrollViewStyle.normal.background, _backgroundColor);
                _lastBackgroundColor = _backgroundColor;
            }
        }

        private void DrawSearch()
        {
            CheckRegexEnumState();

            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField(searchLabel, searchLabelLayout);
                _searchFilter = EditorGUILayout.TextField(_searchFilter);

                var regexStatusLabel = _regexState == RegexState.Valid
                    ? regexValidLabel
                    : _regexState == RegexState.Invalid
                        ? regexInvalidLabel
                        : regexMissingLabel;

                EditorGUILayout.LabelField(regexStatusLabel, regexIconStyle, regexIconLayout);

                EditorGUILayout.Space(_horizontalSpacer * 10, false);
            }

            CheckRegexEnumState();
        }

        private void DrawResult()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField(selectedIconLabel, selectedIconLabelLayout);

                EditorGUILayout.LabelField(lastSelectionContent);
            }
        }

        private void DrawIcons()
        {
            if ((_enums == null) || (_enums.Length == 0))
            {
                return;
            }

            var indentOffset = EditorGUI.indentLevel * 15f;

            var width = position.width - _reservedSpace - indentOffset;
            var iconWidth = _iconSize + (_paddingMultiplier * _iconPadding);
            var columnCount = (int) (width / iconWidth);

            var iconIndex = 0;
            var columnIndex = 0;
            
            using (var scroll = new EditorGUILayout.ScrollViewScope(
                _scrollPosition, scrollViewStyle))
            {
                scroll.handleScrollWheel = true;
                _scrollPosition = scroll.scrollPosition;

                var scope = new EditorGUILayout.HorizontalScope();

                try
                {
                    while (iconIndex < _enums.Length)
                    {
                        var iconName = _iconNames[iconIndex];

                        var regexMatch = true;

                        if (_regexState == RegexState.Valid)
                        {
                            regexMatch = Regex.IsMatch(iconName, _searchFilter);
                        }

                        if (!string.IsNullOrWhiteSpace(_searchFilter) && !regexMatch)
                        {
                            iconIndex += 1;
                            continue;
                        }

                        if (columnIndex >= columnCount)
                        {
                            columnIndex = 0;
                            scope.Dispose();
                            scope = new EditorGUILayout.HorizontalScope();
                        }

                        var icon = _icons[iconIndex];
                        var style = iconStyle;

                        //var style = _lastSelection == icon.tooltip ? selectedIconStyle : iconStyle;

                        EditorGUILayout.LabelField(icon, style, _iconLayout);

                        var lastRectSize = GUILayoutUtility.GetLastRect();
                        lastRectSize.x += EditorGUI.indentLevel * 15f;

                        if (icon.tooltip == _lastSelection)
                        {
                            DrawUIBox(lastRectSize, _selectedBackgroundColor, _selectionSize);
                        }

                        if (GUI.Button(lastRectSize, string.Empty, GUIStyle.none))
                        {
                            _lastSelection = icon.tooltip;
                            EditorGUIUtility.systemCopyBuffer = _lastSelection;

                            var lastSelectionEnum = GetIconEnum(_lastSelection);
                            _lastSelectionContent =
                                new GUIContent(GetIconContent(lastSelectionEnum))
                                {
                                    text = _buttonTextPadding + _lastSelection
                                };
                        }

                        columnIndex += 1;
                        iconIndex += 1;
                    }
                }
                finally
                {
                    scope.Dispose();
                }
            }
        }

#endregion

#region UI Helper Methods

        private Texture2D MakeTex(int width, int height, Color col)
        {
            var result = new Texture2D(width, height);

            return PaintTex(result, col);
        }

        private Texture2D PaintTex(Texture2D texture, Color col)
        {
            if (texture == null)
            {
                texture = new Texture2D(1, 1);
            }

            var pix = texture.GetPixels();

            for (var i = 0; i < pix.Length; i++)
            {
                pix[i] = col;
            }

            texture.SetPixels(pix);
            texture.Apply();

            return texture;
        }

        private void CheckRegexEnumState()
        {
            if (!string.IsNullOrWhiteSpace(_searchFilter))
            {
                try
                {
                    Regex.Match("""", _searchFilter);
                    _regexState = RegexState.Valid;
                }
                catch (Exception ex)
                {
                    _regexState = RegexState.Invalid;
                }
            }
            else
            {
                _regexState = RegexState.Missing;
            }
        }


        public static void HorizontalLineSeparator(Color color, int lineWidth = 1)
        {
            DrawSolidRect(GUILayoutUtility.GetRect(lineWidth, lineWidth, _expandWidth), color);
        }

        public static void DrawUIBox(Rect rect, Color borderColor, float size = 1.5f)
        {
            var left = new Rect(rect.xMin - size, rect.yMin - size, size, rect.height + (2 * size));
            var right = new Rect(rect.xMax, rect.yMin - size, size, rect.height + (2 * size));
            var top = new Rect(rect.xMin - size, rect.yMin - size, rect.width + (2 * size), size);
            var bottom = new Rect(rect.xMin - size, rect.yMax, rect.width + (2 * size), size);

            EditorGUI.DrawRect(left,   borderColor);
            EditorGUI.DrawRect(right,  borderColor);
            EditorGUI.DrawRect(top,    borderColor);
            EditorGUI.DrawRect(bottom, borderColor);
        }

        public static void DrawSolidRect(Rect rect, Color color, bool usePlaymodeTint = true)
        {
            if (Event.current.type != EventType.Repaint)
            {
                return;
            }

            if (usePlaymodeTint)
            {
                EditorGUI.DrawRect(rect, color);
            }
            else
            {
                var oldColor = GUI.color;
                GUI.color = color;
                GUI.DrawTexture(rect, EditorGUIUtility.whiteTexture);
                GUI.color = oldColor;
            }
        }

        private enum RegexState
        {
            Valid,
            Invalid,
            Missing
        }

#endregion
    }
#endif  
";

    #endregion

    private const string TOKEN_VERSION = "###VERSION###";
    private const string TOKEN_COUNT = "###COUNT###";
    private const string TOKEN_LOOKUP = "###LOOKUP###";
    private const string TOKEN_ENUM = "###ENUM###";
    private const string TOKEN_SORTENUM = "###SORTENUM###";
    private const string TOKEN_LIST = "###LIST###";
    private const string TOKEN_EDITOR = "###EDITOR###";

    private const string REPLACEMENT_LOOKUP = "        _iconLookup.Add(Enum.$2, $2);";
    private const string REPLACEMENT_LOOKUP2 = "        _reverseIconLookup.Add($2, Enum.$2);";
    private const string REPLACEMENT_ENUM = "        $2 = $3,";
    private const string REPLACEMENT_SORTENUM = "        $2 = $4,";
    private const string REPLACEMENT_LIST = "    public const string $2 = \"$1\";";

    public const string regenerateButtonText = "Regenerate";
    public const string resetButtonText = "Reset To Default";

    private static StringBuilder _lookupBuilder;
    private static StringBuilder _enumBuilder;
    private static StringBuilder _sortEnumBuilder;
    private static StringBuilder _listBuilder;

    private static bool NeedToGenerateEditorIcons() {
#if !EDITOR_ICONS_GENERATED
        return true;
#else
        return false;
#endif
    }

    [InitializeOnLoadMethod]
    public static void CheckForInitializationNeed() {
        if (NeedToGenerateEditorIcons()) {
            EditorApplication.delayCall += () => RegenerateIconUtilities(true);
        }
    }

    private static void Initialize() {
        if (_lookupBuilder == null) {
            _lookupBuilder = new StringBuilder();
        }

        if (_enumBuilder == null) {
            _enumBuilder = new StringBuilder();
        }

        if (_sortEnumBuilder == null) {
            _sortEnumBuilder = new StringBuilder();
        }

        if (_listBuilder == null) {
            _listBuilder = new StringBuilder();
        }

        _lookupBuilder.Clear();
        _enumBuilder.Clear();
        _sortEnumBuilder.Clear();
        _listBuilder.Clear();
    }

    [MenuItem("Tools/EditorGUI Icons/" + resetButtonText, false, 15000)]
    public static void ResetGeneratedIconUtilities() {
        WriteGeneratedIconFile(resetContent);
    }

    [MenuItem("Tools/EditorGUI Icons/" + regenerateButtonText, false, 15001)]
    public static void RegenerateIconUtilities() {
        RegenerateIconUtilities(true);
    }

    public static void RegenerateIconUtilities(bool force = false) {
        Initialize();

        var bundle = GetEditorAssetBundle();
        var iconsPath = GetIconsPath();

        var processedIcons = new Dictionary<string, IconMetadata>();

        var versionString = GetEditorVersion();

        foreach (var iconPath in EnumerateIcons(bundle, iconsPath)) {
            var iconMetadata = new IconMetadata(iconPath);

            if (!iconMetadata.shouldInclude) {
                continue;
            }

            if (processedIcons.ContainsKey(iconMetadata.cleanIconName)) {
                continue;
            }

            processedIcons.Add(iconMetadata.cleanIconName, iconMetadata);
        }

        if (!(force || NeedToGenerateEditorIcons())) {
            return;
        }

        var sortedValues = processedIcons.Values.ToList();
        sortedValues.Sort(
            (a, b) => string.Compare(a.iconPath, b.iconPath, StringComparison.Ordinal)
        );

        for (var index = 0; index < sortedValues.Count; index++) {
            var iconMetadata = sortedValues[index];
            iconMetadata.sortIndex = index + 1;

            var lookupValue = ReplaceTokens(REPLACEMENT_LOOKUP, iconMetadata);
            var lookupValue2 = ReplaceTokens(REPLACEMENT_LOOKUP2, iconMetadata);
            var enumValue = ReplaceTokens(REPLACEMENT_ENUM, iconMetadata);
            var sortEnumValue = ReplaceTokens(REPLACEMENT_SORTENUM, iconMetadata);
            var listValue = ReplaceTokens(REPLACEMENT_LIST, iconMetadata);

            _lookupBuilder.AppendLine(lookupValue);
            _lookupBuilder.AppendLine(lookupValue2);
            _enumBuilder.AppendLine(enumValue);
            _sortEnumBuilder.AppendLine(sortEnumValue);
            _listBuilder.AppendLine(listValue);
        }

        var newContent = generatorString.Replace(TOKEN_VERSION, versionString)
                                        .Replace(TOKEN_COUNT, processedIcons.Count.ToString())
                                        .Replace(TOKEN_LOOKUP, _lookupBuilder.ToString())
                                        .Replace(TOKEN_ENUM, _enumBuilder.ToString())
                                        .Replace(TOKEN_SORTENUM, _sortEnumBuilder.ToString())
                                        .Replace(TOKEN_LIST, _listBuilder.ToString())
                                        .Replace(TOKEN_EDITOR, editorString)
            ;

        WriteGeneratedIconFile(newContent);
    }

    private static void WriteGeneratedIconFile(string newContent) {
        var assetSearchString = "t:MonoScript " + generatedEnumFileName;

        var foundGuids = AssetDatabase.FindAssets(assetSearchString);

        var realGeneratedEnumRelativePath = defaultGeneratedEnumRelativeFilePath;

        if (foundGuids != null && foundGuids.Length > 0) {
            realGeneratedEnumRelativePath = AssetDatabase.GUIDToAssetPath(foundGuids[0]);
        }

        var realGeneratedEnumAbsolutePath = new FileInfo(realGeneratedEnumRelativePath).FullName;

        using (var fs = File.Open(realGeneratedEnumAbsolutePath, FileMode.OpenOrCreate))
        using (var sr = new StreamReader(fs)) {
            var currentContent = sr.ReadToEnd();

            if (currentContent == newContent) {
                return;
            }
        }

        using (var fs = File.Open(realGeneratedEnumAbsolutePath, FileMode.Truncate))
        using (var sr = new StreamWriter(fs)) {
            sr.Write(newContent);
        }

        AssetDatabase.Refresh();

        AddScriptingDefine();
    }

    private static void AddScriptingDefine() {
        PlayerSettings.GetScriptingDefineSymbolsForGroup(
            EditorUserBuildSettings.selectedBuildTargetGroup, out var scriptingDefines
        );

        var scriptingDefinesLookup = new HashSet<string>(scriptingDefines);

        scriptingDefinesLookup.Add(compileFlag);

        PlayerSettings.SetScriptingDefineSymbolsForGroup(
            EditorUserBuildSettings.selectedBuildTargetGroup,
            scriptingDefinesLookup.ToArray()
        );
    }

    public static void RemoveScriptingDefine() {
        PlayerSettings.GetScriptingDefineSymbolsForGroup(
            EditorUserBuildSettings.selectedBuildTargetGroup, out var scriptingDefines
        );

        var scriptingDefinesLookup = new HashSet<string>(scriptingDefines);

        scriptingDefinesLookup.Remove(compileFlag);

        PlayerSettings.SetScriptingDefineSymbolsForGroup(
            EditorUserBuildSettings.selectedBuildTargetGroup,
            scriptingDefinesLookup.ToArray()
        );
    }

    private static string ReplaceTokens(string baseString, IconMetadata iconMetadata) {
        return baseString.Replace("$1", iconMetadata.iconName)
                         .Replace("$2", iconMetadata.cleanIconName)
                         .Replace("$3", iconMetadata.nameHash.ToString())
                         .Replace("$4", iconMetadata.sortIndex.ToString());
    }

    private static IEnumerable<string> EnumerateIcons(
        AssetBundle editorAssetBundle,
        string iconsPath) {
        foreach (var assetName in editorAssetBundle.GetAllAssetNames()) {
            if (assetName.StartsWith(iconsPath, StringComparison.OrdinalIgnoreCase) == false) {
                continue;
            }

            if ((assetName.EndsWith(".png", StringComparison.OrdinalIgnoreCase) == false) &&
                (assetName.EndsWith(".asset", StringComparison.OrdinalIgnoreCase) == false)) {
                continue;
            }

            yield return assetName;
        }
    }

    private static string GetEditorVersion() {
        return Application.unityVersion;
    }

    private static AssetBundle GetEditorAssetBundle() {
        var editorGUIUtility = typeof(EditorGUIUtility);
        var getEditorAssetBundle = editorGUIUtility.GetMethod(
            "GetEditorAssetBundle",
            BindingFlags.NonPublic | BindingFlags.Static
        );

        return (AssetBundle)getEditorAssetBundle.Invoke(null, new object[] { });
    }

    private static string GetIconsPath() {
#if UNITY_2018_3_OR_NEWER
        return EditorResources.iconsPath;
#else
        var assembly = typeof(EditorGUIUtility).Assembly;
        var editorResourcesUtility =
assembly.GetType("UnityEditorInternal.EditorResourcesUtility");

        var iconsPathProperty = editorResourcesUtility.GetProperty(
            "iconsPath",
            BindingFlags.Static | BindingFlags.Public);

        return (string)iconsPathProperty.GetValue(null, new object[] { });
#endif
    }

    private class IconMetadata {
        public readonly string cleanIconName;
        public readonly string iconName;
        public readonly string iconPath;
        public readonly int nameHash;
        public readonly bool shouldInclude;
        public int sortIndex;

        public IconMetadata(string iconPath) {
            this.iconPath = iconPath;
            iconName = Path.GetFileNameWithoutExtension(iconPath);
            cleanIconName = CleanIconName(iconName);

            shouldInclude = !iconName.Contains("@");

            nameHash = cleanIconName.GetHashCode();

            if (nameHash < 0) {
                nameHash *= -1;
            }
        }

        private static string CleanIconName(string iconName) {
            return Regex.Replace(iconName, @"[\., \\\/-]+", "_");
        }
    }
}