using UnityEngine;

namespace Kivancalp.App
{
    internal static class RuntimeEntryPoint
    {
        private static bool _isCreated;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStaticState()
        {
            _isCreated = false;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void CreateBootstrapObject()
        {
            if (_isCreated)
            {
                return;
            }

            if (Object.FindFirstObjectByType<GameBootstrapper>() != null)
            {
                _isCreated = true;
                return;
            }

            var bootstrapObject = new GameObject("CardMatchBootstrap", typeof(GameBootstrapper));
            Object.DontDestroyOnLoad(bootstrapObject);
            _isCreated = true;
        }
    }
}
