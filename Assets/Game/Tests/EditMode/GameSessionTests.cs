using Kivancalp.Core.Logging;
using Kivancalp.Gameplay.Application;
using Kivancalp.Gameplay.Configuration;
using Kivancalp.Gameplay.Contracts;
using Kivancalp.Gameplay.Domain;
using NUnit.Framework;

namespace Kivancalp.Tests.EditMode
{
    public sealed class GameSessionTests
    {
        [Test]
        public void TryFlipCard_ShouldAllowContinuousInput()
        {
            var session = CreateSession();
            session.StartNewGame(new LayoutId(0));

            bool firstFlip = session.TryFlipCard(0);
            bool secondFlip = session.TryFlipCard(2);
            bool thirdFlipWithoutWaiting = session.TryFlipCard(1);

            Assert.That(firstFlip, Is.True);
            Assert.That(secondFlip, Is.True);
            Assert.That(thirdFlipWithoutWaiting, Is.True);
        }

        [Test]
        public void Tick_ShouldResolveMatchAndIncreaseScore()
        {
            var session = CreateSession();
            session.StartNewGame(new LayoutId(0));

            session.TryFlipCard(0);
            session.TryFlipCard(1);
            session.Tick(0.11f);

            CardSnapshot first = session.GetCardSnapshot(0);
            CardSnapshot second = session.GetCardSnapshot(1);
            GameStats stats = session.GetStats();

            Assert.That(first.State, Is.EqualTo(CardState.Matched));
            Assert.That(second.State, Is.EqualTo(CardState.Matched));
            Assert.That(stats.Score, Is.GreaterThan(0));
            Assert.That(stats.Matches, Is.EqualTo(1));
        }

        [Test]
        public void Tick_ShouldHideMismatchedCardsAfterRevealDelay()
        {
            var session = CreateSession();
            session.StartNewGame(new LayoutId(0));

            session.TryFlipCard(0);
            session.TryFlipCard(2);

            session.Tick(0.11f);
            Assert.That(session.GetCardSnapshot(0).State, Is.EqualTo(CardState.FaceUp));
            Assert.That(session.GetCardSnapshot(2).State, Is.EqualTo(CardState.FaceUp));

            session.Tick(0.46f);
            Assert.That(session.GetCardSnapshot(0).State, Is.EqualTo(CardState.FaceDown));
            Assert.That(session.GetCardSnapshot(2).State, Is.EqualTo(CardState.FaceDown));
        }

        private static GameSession CreateSession()
        {
            BoardLayoutConfig[] layouts =
            {
                new BoardLayoutConfig(new LayoutId(0), "2x2", 2, 2, 16f, 24f),
                new BoardLayoutConfig(new LayoutId(1), "2x3", 2, 3, 16f, 24f),
            };

            var config = new GameConfig(layouts, new LayoutId(0), new ScoringConfig(100, 25, 10), 0.16f, 0.10f, 0.45f, 10f, 1234);
            return new GameSession(config, new IdentityRandomProvider(), new InMemoryPersistence(), new SilentAudio(), new SilentLogger());
        }

        private sealed class IdentityRandomProvider : IRandomProvider
        {
            public int NextInt(int minInclusive, int maxExclusive)
            {
                return minInclusive;
            }

            public void Shuffle(int[] values, int count)
            {
            }
        }

        private sealed class InMemoryPersistence : IGamePersistence
        {
            private PersistedGameState _state;

            public bool TryLoad(out PersistedGameState persistedState)
            {
                persistedState = _state;
                return _state != null;
            }

            public void Save(PersistedGameState persistedState)
            {
                _state = persistedState;
            }

            public void Delete()
            {
                _state = null;
            }
        }

        private sealed class SilentAudio : IGameAudio
        {
            public void Play(SoundEffectType effectType)
            {
            }
        }

        private sealed class SilentLogger : IGameLogger
        {
            public void Info(string message)
            {
            }

            public void Warning(string message)
            {
            }

            public void Error(string message, System.Exception exception = null)
            {
            }
        }
    }
}
