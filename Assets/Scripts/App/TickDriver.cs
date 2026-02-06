using Kivancalp.Core.Lifecycle;
using UnityEngine;

namespace Kivancalp.App
{
    public sealed class TickDriver : MonoBehaviour
    {
        private ITickable[] _tickables;

        public void Initialize(ITickable[] tickables)
        {
            _tickables = tickables;
        }

        private void Update()
        {
            if (_tickables == null)
            {
                return;
            }

            float deltaTime = Time.unscaledDeltaTime;

            for (int index = 0; index < _tickables.Length; index += 1)
            {
                _tickables[index].Tick(deltaTime);
            }
        }
    }
}
