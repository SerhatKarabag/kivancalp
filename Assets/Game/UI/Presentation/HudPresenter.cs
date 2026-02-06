using System;
using Kivancalp.Gameplay.Application;
using Kivancalp.Gameplay.Contracts;
using Kivancalp.Gameplay.Domain;
using Kivancalp.UI.Views;
using UnityEngine;

namespace Kivancalp.UI.Presentation
{
    public sealed class HudPresenter : IDisposable
    {
        private const string ScorePrefix = "Score: ";
        private const string TurnsPrefix = "Turns: ";
        private const string MatchesPrefix = "Matches: ";
        private const string ComboPrefix = "Combo: ";
        private const string LayoutPrefix = "Layout: ";

        private readonly IGameSession _session;
        private readonly GameUiContext _ui;

        public HudPresenter(IGameSession session, GameUiContext ui)
        {
            _session = session ?? throw new ArgumentNullException(nameof(session));
            _ui = ui ?? throw new ArgumentNullException(nameof(ui));
        }

        public void Initialize()
        {
            _session.StatsChanged += OnStatsChanged;
            _session.BoardChanged += OnBoardChanged;
            _session.GameCompleted += OnGameCompleted;

            _ui.NewGameButton.onClick.AddListener(OnNewGameClicked);
            _ui.PreviousLayoutButton.onClick.AddListener(OnPreviousLayoutClicked);
            _ui.NextLayoutButton.onClick.AddListener(OnNextLayoutClicked);
            _ui.SaveButton.onClick.AddListener(OnSaveClicked);
            _ui.LoadButton.onClick.AddListener(OnLoadClicked);

            UpdateStats(_session.GetStats());
            _ui.LayoutText.text = LayoutPrefix + _session.CurrentLayout.DisplayName;
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
            _ui.SaveButton.onClick.RemoveListener(OnSaveClicked);
            _ui.LoadButton.onClick.RemoveListener(OnLoadClicked);
        }

        private void OnBoardChanged(BoardChangedEvent boardChanged)
        {
            _ui.LayoutText.text = LayoutPrefix + boardChanged.Layout.DisplayName;
            _ui.StatusText.text = string.Empty;
        }

        private void OnStatsChanged(GameStatsChangedEvent statsChanged)
        {
            UpdateStats(statsChanged.Stats);
        }

        private void OnGameCompleted(GameCompletedEvent gameCompleted)
        {
            _ui.StatusText.text = "Completed";
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

        private void OnSaveClicked()
        {
            _session.ForceSave();
            _ui.StatusText.text = "Saved";
        }

        private void OnLoadClicked()
        {
            _session.ReloadFromSave();
            _ui.StatusText.text = "Loaded";
        }

        private void UpdateStats(GameStats stats)
        {
            _ui.ScoreText.text = ScorePrefix + stats.Score;
            _ui.TurnsText.text = TurnsPrefix + stats.Turns;
            _ui.MatchesText.text = MatchesPrefix + stats.Matches + "/" + stats.TotalPairs;
            _ui.ComboText.text = ComboPrefix + stats.Combo;
        }
    }
}
