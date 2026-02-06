using System;
using Kivancalp.Gameplay.Application;
using Kivancalp.Gameplay.Contracts;
using Kivancalp.Gameplay.Domain;
using Kivancalp.UI.Views;

namespace Kivancalp.UI.Presentation
{
    public sealed class HudPresenter : IDisposable
    {
        private readonly IGameSession _session;
        private readonly GameUiContext _ui;
        private readonly UiThemeConfig _theme;

        public HudPresenter(IGameSession session, GameUiContext ui, UiThemeConfig theme)
        {
            _session = session ?? throw new ArgumentNullException(nameof(session));
            _ui = ui ?? throw new ArgumentNullException(nameof(ui));
            _theme = theme ?? throw new ArgumentNullException(nameof(theme));
            _theme.EnsureInitialized();
        }

        public void Initialize()
        {
            _session.StatsChanged += OnStatsChanged;
            _session.BoardChanged += OnBoardChanged;
            _session.GameCompleted += OnGameCompleted;

            _ui.NewGameButton.onClick.AddListener(OnNewGameClicked);
            _ui.PreviousLayoutButton.onClick.AddListener(OnPreviousLayoutClicked);
            _ui.NextLayoutButton.onClick.AddListener(OnNextLayoutClicked);

            UpdateStats(_session.GetStats());
            _ui.LayoutText.text = _theme.hudLabels.layoutPrefix + _session.CurrentLayout.DisplayName;
            _ui.StatusText.text = string.Empty;
        }

        public void Dispose()
        {
            _session.StatsChanged -= OnStatsChanged;
            _session.BoardChanged -= OnBoardChanged;
            _session.GameCompleted -= OnGameCompleted;

            _ui.NewGameButton.onClick.RemoveListener(OnNewGameClicked);
            _ui.PreviousLayoutButton.onClick.RemoveListener(OnPreviousLayoutClicked);
            _ui.NextLayoutButton.onClick.RemoveListener(OnNextLayoutClicked);
        }

        private void OnBoardChanged(BoardChangedEvent boardChanged)
        {
            _ui.LayoutText.text = _theme.hudLabels.layoutPrefix + boardChanged.Layout.DisplayName;
            _ui.StatusText.text = string.Empty;
        }

        private void OnStatsChanged(GameStatsChangedEvent statsChanged)
        {
            UpdateStats(statsChanged.Stats);
        }

        private void OnGameCompleted(GameCompletedEvent gameCompleted)
        {
            _ui.StatusText.text = _theme.hudLabels.completedStatus;
            UpdateStats(gameCompleted.Stats);
        }

        private void OnNewGameClicked()
        {
            _session.StartNewGame(_session.CurrentLayout.Id);
        }

        private void OnPreviousLayoutClicked()
        {
            _session.SwitchLayoutByOffset(-1);
        }

        private void OnNextLayoutClicked()
        {
            _session.SwitchLayoutByOffset(1);
        }

        private void UpdateStats(GameStats stats)
        {
            _ui.ScoreText.text = _theme.hudLabels.scorePrefix + stats.Score;
            _ui.TurnsText.text = _theme.hudLabels.turnsPrefix + stats.Turns;
            _ui.MatchesText.text = _theme.hudLabels.matchesPrefix + stats.Matches + "/" + stats.TotalPairs;
            _ui.ComboText.text = _theme.hudLabels.comboPrefix + stats.Combo;
        }
    }
}
