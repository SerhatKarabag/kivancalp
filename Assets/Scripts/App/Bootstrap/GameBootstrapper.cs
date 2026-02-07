using System;
using Kivancalp.Core;
using Kivancalp.Core.DI;
using Kivancalp.Gameplay.Application;
using Kivancalp.Gameplay.Configuration;
using Kivancalp.Gameplay.Interfaces;
using Kivancalp.Infrastructure.Audio;
using Kivancalp.Infrastructure.Config;
using Kivancalp.Infrastructure.Logging;
using Kivancalp.Infrastructure.Persistence;
using Kivancalp.Infrastructure.Randomness;
using Kivancalp.UI.Presentation;
using Kivancalp.UI.Views;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Kivancalp.App
{
    public sealed class GameBootstrapper : MonoBehaviour
    {
        private const float SaveFlushDebounceSeconds = 0.20f;
        private const string GameUiRootResourcePath = "Prefabs/GameUiRoot";
        private const string CardViewResourcePath = "Prefabs/CardView";

        private IDiContainer _rootContainer;
        private IDiContainer _sceneScope;
        private TickDriver _tickDriver;
        private IGameSessionLifecycle _sessionLifecycle;
        private bool _isQuitting;
        private float _lastFlushRealtime = float.NegativeInfinity;

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
            BuildComposition();
        }

        private void OnDestroy()
        {
            if (!_isQuitting)
            {
                FlushSessionSave(force: true);
            }

            _sceneScope?.Dispose();
            _rootContainer?.Dispose();
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
            {
                FlushSessionSave();
            }
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            if (!hasFocus)
            {
                FlushSessionSave();
            }
        }

        private void OnApplicationQuit()
        {
            _isQuitting = true;
            FlushSessionSave(force: true);
        }

        private void BuildComposition()
        {
            _rootContainer = new DiContainer();

            UiThemeConfig uiThemeConfig = UiThemeConfig.LoadOrCreateRuntimeDefault();

            EnsureEventSystem();

            GameUiRef uiRefPrefab = Resources.Load<GameUiRef>(GameUiRootResourcePath);

            GameUiRef uiRef = Instantiate(uiRefPrefab, transform);

            AudioSource audioSource = uiRef.gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.loop = false;
            audioSource.spatialBlend = 0f;

            CardView cardViewPrefab = Resources.Load<CardView>(CardViewResourcePath);


            _rootContainer.RegisterInstance(uiThemeConfig);
            _rootContainer.RegisterInstance(uiRef);
            _rootContainer.RegisterInstance(audioSource);
            _rootContainer.RegisterInstance(cardViewPrefab);

            RegisterServices(_rootContainer);

            _sceneScope = _rootContainer.CreateScope();

            _sessionLifecycle = (IGameSessionLifecycle)_sceneScope.Resolve<IGameSession>();
            CardFlipAnimationSystem flipAnimationSystem = _sceneScope.Resolve<CardFlipAnimationSystem>();
            BoardPresenter boardPresenter = _sceneScope.Resolve<BoardPresenter>();
            HudPresenter hudPresenter = _sceneScope.Resolve<HudPresenter>();

            boardPresenter.Initialize();
            hudPresenter.Initialize();
            _sessionLifecycle.Start();

            _tickDriver = gameObject.AddComponent<TickDriver>();
            _tickDriver.Initialize(new ITickable[]
            {
                _sessionLifecycle,
                flipAnimationSystem,
                boardPresenter,
            });
        }

        private void FlushSessionSave(bool force = false)
        {
            if (_sessionLifecycle == null)
            {
                return;
            }

            if (!force)
            {
                float now = Time.realtimeSinceStartup;

                if ((now - _lastFlushRealtime) < SaveFlushDebounceSeconds)
                {
                    return;
                }

                _lastFlushRealtime = now;
            }
            else
            {
                _lastFlushRealtime = Time.realtimeSinceStartup;
            }

            _sessionLifecycle.ForceSave();
        }

        private static void EnsureEventSystem()
        {
            EventSystem current = EventSystem.current;

            if (current != null)
            {
                EnsureInputModule(current.gameObject);
                return;
            }

            var eventSystemObject = new GameObject("EventSystem", typeof(EventSystem));
            EnsureInputModule(eventSystemObject);
            DontDestroyOnLoad(eventSystemObject);
        }

        private static void EnsureInputModule(GameObject eventSystemObject)
        {
#if ENABLE_INPUT_SYSTEM
            StandaloneInputModule standaloneInputModule = eventSystemObject.GetComponent<StandaloneInputModule>();

            if (standaloneInputModule != null)
            {
                standaloneInputModule.enabled = false;
                Destroy(standaloneInputModule);
            }

            System.Type inputSystemUiModuleType = System.Type.GetType("UnityEngine.InputSystem.UI.InputSystemUIInputModule, Unity.InputSystem");

            if (inputSystemUiModuleType != null)
            {
                if (eventSystemObject.GetComponent(inputSystemUiModuleType) == null)
                {
                    eventSystemObject.AddComponent(inputSystemUiModuleType);
                }

                return;
            }

            if (eventSystemObject.GetComponent<StandaloneInputModule>() == null)
            {
                eventSystemObject.AddComponent<StandaloneInputModule>();
            }

            Debug.LogWarning("InputSystemUIInputModule type could not be resolved. Falling back to StandaloneInputModule.");
#else
            if (eventSystemObject.GetComponent<StandaloneInputModule>() == null)
            {
                eventSystemObject.AddComponent<StandaloneInputModule>();
            }
#endif
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
                    resolver.Resolve<GameUiRef>().BoardContainer,
                    resolver.Resolve<CardView>(),
                    resolver.Resolve<UiThemeConfig>().card,
                    resolver.Resolve<GameConfig>().GetMaxCardCount()));
            container.RegisterScoped<BoardPresenter, BoardPresenter>();
            container.RegisterScoped<HudPresenter, HudPresenter>();
        }
    }
}
