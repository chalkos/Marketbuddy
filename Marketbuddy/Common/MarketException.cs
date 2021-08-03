using System;

namespace Marketbuddy.Common
{
    internal class MarketException : InvalidOperationException
    {
        public MarketException(string message) : base(message)
        {
        }

        public MarketException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}