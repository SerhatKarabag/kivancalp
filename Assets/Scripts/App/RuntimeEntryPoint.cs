using UnityEngine;

namespace Kivancalp.App
{
    internal static class RuntimeEntryPoint
    {
        private static bool _isCreated;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void CreateBootstrapObject()
        {
            if (_isCreated)
            {
                return;
            }

            var bootstrapObject = new GameObject("CardMatchBootstrap", typeof(GameBootstrapper));
            Object.DontDestroyOnLoad(bootstrapObject);
            _isCreated = true;
        }
    }
}
