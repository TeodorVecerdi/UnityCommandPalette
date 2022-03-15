namespace CommandPalette.Core {
    public readonly struct ResultDisplaySettings {
        public readonly string Title;
        public readonly string ShortName;
        public readonly string Description;
        public readonly string Icon;
        public readonly string SuffixIcon;

        public ResultDisplaySettings(string title, string shortName, string description, string icon = null, string suffixIcon = null) {
            Title = title;
            ShortName = shortName;
            Description = description;

            Icon = icon;
            SuffixIcon = suffixIcon;
        }
    }
}