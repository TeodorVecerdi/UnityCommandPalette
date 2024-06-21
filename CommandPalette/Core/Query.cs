namespace CommandPalette.Core {
    public readonly struct Query {
        public readonly string Text;

        public Query(string text) {
            Text = text;
        }
    }
}