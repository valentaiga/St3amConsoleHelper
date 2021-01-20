using System;

namespace SteamConsoleHelper.Exceptions
{
    public class InternalException : Exception
    {
        public InternalException(InternalError exceptionType) : base(exceptionType.ToString())
        {
        }

        public InternalException(Exception exception, InternalError exceptionType) : base(
            $"{exceptionType}: {exception.Message}")
        {
        }

        public InternalException(string exceptionMessage, InternalError exceptionType) : base(
            $"{exceptionType}: {exceptionMessage}")
        {
        }
    }
}