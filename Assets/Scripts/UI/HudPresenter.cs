using System;
using System.Text;
using Kivancalp.Gameplay.Interfaces;
using Kivancalp.Gameplay.Models;
using Kivancalp.UI.Views;

namespace Kivancalp.UI.Presentation
{
    public sealed class HudPresenter : IDisposable
    {
        private readonly IGameSession _session;
        private readonly GameUiRef _ui;
        private readonly UiThemeConfig _theme;
        private readonly StringBuilder _sb = new StringBuilder(32);

        public HudPresenter(IGameSession session, GameUiRef ui, UiThemeConfig theme)
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

            _sb.Clear().Append(_theme.hudLabels.layoutPrefix).Append(_session.CurrentLayout.DisplayName);
            _ui.LayoutText.text = _sb.ToString();
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
            _sb.Clear().Append(_theme.hudLabels.layoutPrefix).Append(boardChanged.Layout.DisplayName);
            _ui.LayoutText.text = _sb.ToString();
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
            _ui.ScoreText.text = FormatStat(_theme.hudLabels.scorePrefix, stats.Score);
            _ui.TurnsText.text = FormatStat(_theme.hudLabels.turnsPrefix, stats.Turns);

            _sb.Clear().Append(_theme.hudLabels.matchesPrefix).Append(stats.Matches).Append('/').Append(stats.TotalPairs);
            _ui.MatchesText.text = _sb.ToString();

            _ui.ComboText.text = FormatStat(_theme.hudLabels.comboPrefix, stats.Combo);
        }

        private string FormatStat(string prefix, int value)
        {
            _sb.Clear().Append(prefix).Append(value);
            return _sb.ToString();
        }
    }
}
