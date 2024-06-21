namespace CommandPalette.Core {
    public readonly struct ResultDisplaySettings {
        public readonly string Title;
        public readonly string? ShortName;
        public readonly string? Description;
        public readonly IconResource Icon;
        public readonly IconResource SuffixIcon;

        public ResultDisplaySettings(string title, string? shortName = null, string? description = null, IconResource icon = default, IconResource suffixIcon = default) {
            Title = title;
            ShortName = shortName;
            Description = description;

            Icon = icon;
            SuffixIcon = suffixIcon;
        }
    }
}