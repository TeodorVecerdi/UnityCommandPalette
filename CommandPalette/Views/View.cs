using UnityEngine;
using UnityEngine.UIElements;

namespace CommandPalette.Views {
    public abstract class View {
        public CommandPaletteWindow Window { get; set; } = null!;

        public abstract VisualElement Build();

        public virtual void OnEvent(Event evt) { }
    }
}