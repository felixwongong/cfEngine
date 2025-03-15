using System;

namespace cfEngine.Util
{
    public class SanityCheckException : Exception
    {
        public SanityCheckException() : base("A sanity check failed, indicating an unexpected internal state.")
        {
        }

        public SanityCheckException(string message) : base(message)
        {
        }

        public SanityCheckException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}