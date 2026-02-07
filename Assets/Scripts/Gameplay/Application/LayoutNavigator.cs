using Kivancalp.Gameplay.Configuration;

namespace Kivancalp.Gameplay.Application
{
    internal sealed class LayoutNavigator
    {
        private readonly GameConfig _config;

        private int _currentLayoutIndex;
        private BoardLayoutConfig _currentLayout;

        public LayoutNavigator(GameConfig config)
        {
            _config = config;

            _currentLayoutIndex = config.FindLayoutIndex(config.DefaultLayoutId);

            if (_currentLayoutIndex < 0)
            {
                _currentLayoutIndex = 0;
            }

            _currentLayout = config.GetLayoutByIndex(_currentLayoutIndex);
        }

        public BoardLayoutConfig CurrentLayout => _currentLayout;

        public int CurrentLayoutIndex => _currentLayoutIndex;

        public int ResolveLayoutIndex(LayoutId layoutId)
        {
            int layoutIndex = _config.FindLayoutIndex(layoutId);

            if (layoutIndex < 0)
            {
                layoutIndex = _config.FindLayoutIndex(_config.DefaultLayoutId);

                if (layoutIndex < 0)
                {
                    layoutIndex = 0;
                }
            }

            return layoutIndex;
        }

        public void SetLayout(int layoutIndex)
        {
            _currentLayoutIndex = layoutIndex;
            _currentLayout = _config.GetLayoutByIndex(layoutIndex);
        }

        public int CalculateOffsetIndex(int offset)
        {
            int layoutCount = _config.LayoutCount;

            if (layoutCount <= 0)
            {
                return _currentLayoutIndex;
            }

            return ((_currentLayoutIndex + offset) % layoutCount + layoutCount) % layoutCount;
        }
    }
}
