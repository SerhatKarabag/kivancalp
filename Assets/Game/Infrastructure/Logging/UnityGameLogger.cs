using System;
using Kivancalp.Core.Logging;
using UnityEngine;

namespace Kivancalp.Infrastructure.Logging
{
    public sealed class UnityGameLogger : IGameLogger
    {
        public void Info(string message)
        {
            Debug.Log(message);
        }

        public void Warning(string message)
        {
            Debug.LogWarning(message);
        }

        public void Error(string message, Exception exception = null)
        {
            if (exception == null)
            {
                Debug.LogError(message);
                return;
            }

            Debug.LogError(message + "\n" + exception);
        }
    }
}
