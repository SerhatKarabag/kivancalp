using System;
using Kivancalp.Core.Lifecycle;
using Kivancalp.Gameplay.Application;
using Kivancalp.Gameplay.Contracts;
using Kivancalp.Gameplay.Domain;
using Kivancalp.UI.Views;
using UnityEngine;

namespace Kivancalp.UI.Presentation
{
    public sealed class BoardPresenter : ITickable, IDisposable
    {
        private readonly IGameSession _session;
        private readonly GameUiContext _ui;
        private readonly CardViewPool _pool;
        private readonly CardFlipAnimationSystem _flipAnimationSystem;
        private readonly CardView[] _activeViews;

        private int _activeCardCount;
        private bool _layoutDirty;
        private Vector2 _lastBoardSize;

        public BoardPresenter(IGameSession session, GameUiContext ui, CardViewPool pool, CardFlipAnimationSystem flipAnimationSystem)
        {
            _session = session ?? throw new ArgumentNullException(nameof(session));
            _ui = ui ?? throw new ArgumentNullException(nameof(ui));
            _pool = pool ?? throw new ArgumentNullException(nameof(pool));
            _flipAnimationSystem = flipAnimationSystem ?? throw new ArgumentNullException(nameof(flipAnimationSystem));
            _activeViews = new CardView[_session.Config.GetMaxCardCount()];
        }

        public void Initialize()
        {
            _session.BoardChanged += OnBoardChanged;
            _session.CardStateChanged += OnCardStateChanged;
            _layoutDirty = true;
        }

        public void Tick(float deltaTime)
        {
            Vector2 boardSize = _ui.BoardContainer.rect.size;

            if (boardSize.x <= 1f || boardSize.y <= 1f)
            {
                return;
            }

            if (_layoutDirty || !Approximately(boardSize, _lastBoardSize))
            {
                BoardLayoutCalculator.Apply(_ui.BoardContainer, _session.CurrentLayout, _activeViews, _activeCardCount);
                _lastBoardSize = boardSize;
                _layoutDirty = false;
            }
        }

        public void Dispose()
        {
            _session.BoardChanged -= OnBoardChanged;
            _session.CardStateChanged -= OnCardStateChanged;
            ReturnAllCardsToPool();
        }

        private void OnBoardChanged(BoardChangedEvent boardChanged)
        {
            ReturnAllCardsToPool();
            _activeCardCount = boardChanged.CardCount;

            for (int cardIndex = 0; cardIndex < _activeCardCount; cardIndex += 1)
            {
                CardSnapshot snapshot = _session.GetCardSnapshot(cardIndex);
                CardView view = _pool.Rent();
                view.Bind(snapshot.CardIndex, snapshot.FaceId, OnCardClicked);
                view.SetFaceUpImmediate(snapshot.State != CardState.FaceDown);

                if (snapshot.State == CardState.Matched)
                {
                    view.SetMatched(true);
                }

                _activeViews[cardIndex] = view;
            }

            _layoutDirty = true;
        }

        private void OnCardStateChanged(CardStateChangedEvent cardChanged)
        {
            if (cardChanged.CardIndex < 0 || cardChanged.CardIndex >= _activeCardCount)
            {
                return;
            }

            CardView view = _activeViews[cardChanged.CardIndex];

            if (view == null)
            {
                return;
            }

            if (cardChanged.Reason == CardStateChangeReason.PlayerFlip)
            {
                _flipAnimationSystem.Play(view, true, _session.Config.FlipDurationSeconds);
                return;
            }

            if (cardChanged.Reason == CardStateChangeReason.AutoHide)
            {
                _flipAnimationSystem.Play(view, false, _session.Config.FlipDurationSeconds);
                return;
            }

            if (cardChanged.Reason == CardStateChangeReason.Matched)
            {
                view.SetFaceUpImmediate(true);
                view.SetMatched(true);
            }
        }

        private void OnCardClicked(int cardIndex)
        {
            _session.TryFlipCard(cardIndex);
        }

        private void ReturnAllCardsToPool()
        {
            for (int cardIndex = 0; cardIndex < _activeCardCount; cardIndex += 1)
            {
                CardView view = _activeViews[cardIndex];

                if (view != null)
                {
                    _pool.Return(view);
                    _activeViews[cardIndex] = null;
                }
            }

            _activeCardCount = 0;
        }

        private static bool Approximately(Vector2 a, Vector2 b)
        {
            return Mathf.Abs(a.x - b.x) <= 0.5f && Mathf.Abs(a.y - b.y) <= 0.5f;
        }
    }
}
