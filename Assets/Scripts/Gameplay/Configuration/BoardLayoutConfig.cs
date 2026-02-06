namespace Kivancalp.Gameplay.Configuration
{
    public readonly struct BoardLayoutConfig
    {
        public BoardLayoutConfig(LayoutId id, string displayName, int rows, int columns, float spacing, float padding)
        {
            Id = id;
            DisplayName = displayName;
            Rows = rows;
            Columns = columns;
            Spacing = spacing;
            Padding = padding;
        }

        public LayoutId Id { get; }

        public string DisplayName { get; }

        public int Rows { get; }

        public int Columns { get; }

        public float Spacing { get; }

        public float Padding { get; }

        public int CardCount => Rows * Columns;

        public int PairCount => CardCount / 2;

        public bool IsValid => Rows > 0 && Columns > 0 && (CardCount % 2 == 0);
    }
}
