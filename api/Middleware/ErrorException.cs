using System;

namespace Api.Middleware
{
    public class ErrorException : Exception
    {
        public ErrorException(string message) : base(message) {}
    }
}