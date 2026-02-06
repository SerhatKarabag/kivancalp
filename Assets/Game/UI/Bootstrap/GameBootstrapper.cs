using Kivancalp.Core.DI;
using Kivancalp.Core.Lifecycle;
using Kivancalp.Core.Logging;
using Kivancalp.Gameplay.Application;
using Kivancalp.Gameplay.Configuration;
using Kivancalp.Gameplay.Contracts;
using Kivancalp.Infrastructure.Audio;
using Kivancalp.Infrastructure.Config;
using Kivancalp.Infrastructure.Logging;
using Kivancalp.Infrastructure.Persistence;
using Kivancalp.Infrastructure.Randomness;
using Kivancalp.UI.Presentation;
using Kivancalp.UI.Views;
using UnityEngine;

namespace Kivancalp.UI.Bootstrap
{
    public sealed class GameBootstrapper : MonoBehaviour
    {
        private IDiContainer _rootContainer;
        private IDiContainer _sceneScope;
        private TickDriver _tickDriver;

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
            BuildComposition();
        }

        private void OnDestroy()
        {
            _sceneScope?.Dispose();
            _rootContainer?.Dispose();
        }

        private void BuildComposition()
        {
            _rootContainer = new DiContainer();

            GameUiContext uiContext = GameUiFactory.Create(transform);
            AudioSource audioSource = uiContext.RootObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.loop = false;
            audioSource.spatialBlend = 0f;

            _rootContainer.RegisterInstance(uiContext);
            _rootContainer.RegisterInstance(audioSource);

            RegisterServices(_rootContainer);

            _sceneScope = _rootContainer.CreateScope();

            IGameSession session = _sceneScope.Resolve<IGameSession>();
            CardFlipAnimationSystem flipAnimationSystem = _sceneScope.Resolve<CardFlipAnimationSystem>();
            BoardPresenter boardPresenter = _sceneScope.Resolve<BoardPresenter>();
            HudPresenter hudPresenter = _sceneScope.Resolve<HudPresenter>();

            var presenter = new GamePresenter(session, boardPresenter, hudPresenter);
            presenter.Initialize();

            _tickDriver = gameObject.AddComponent<TickDriver>();
            _tickDriver.Initialize(new ITickable[]
            {
                session,
                flipAnimationSystem,
                boardPresenter,
            });
        }

        private static void RegisterServices(IDiContainer container)
        {
            container.RegisterSingleton<IGameLogger, UnityGameLogger>();
            container.RegisterSingleton<IGameConfigProvider, ResourcesGameConfigProvider>();
            container.RegisterSingleton<GameConfig>(resolver => resolver.Resolve<IGameConfigProvider>().Load());
            container.RegisterSingleton<IRandomProvider>(resolver => new DeterministicRandomProvider(resolver.Resolve<GameConfig>().RandomSeed));
            container.RegisterSingleton<IGamePersistence, JsonGamePersistence>();

            container.RegisterScoped<IGameAudio, GameAudioFeedbackService>();
            container.RegisterScoped<IGameSession, GameSession>();
            container.RegisterScoped<CardFlipAnimationSystem>(resolver => new CardFlipAnimationSystem(resolver.Resolve<GameConfig>().GetMaxCardCount()));
            container.RegisterScoped<CardViewPool>(resolver =>
                new CardViewPool(
                    resolver.Resolve<GameUiContext>().BoardContainer,
                    resolver.Resolve<GameUiContext>().UiFont,
                    resolver.Resolve<GameConfig>().GetMaxCardCount()));
            container.RegisterScoped<BoardPresenter, BoardPresenter>();
            container.RegisterScoped<HudPresenter, HudPresenter>();
        }
    }
}
