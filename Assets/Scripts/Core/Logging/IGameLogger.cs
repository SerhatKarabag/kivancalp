using System;

namespace Kivancalp.Core.Logging
{
    public interface IGameLogger
    {
        void Info(string message);

        void Warning(string message);

        void Error(string message, Exception exception = null);
    }
}
