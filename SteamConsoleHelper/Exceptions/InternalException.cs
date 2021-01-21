using System;

namespace SteamConsoleHelper.Exceptions
{
    public class InternalException : Exception
    {
        public InternalError Error { get; }

        public InternalException(InternalError exceptionType) 
            : base(exceptionType.ToString())
        {
            Error = exceptionType;
        }

        public InternalException(InternalError exceptionType, Exception exception) 
            : this(exceptionType, exception.ToString())
        {
        }

        public InternalException(InternalError exceptionType, string exceptionMessage) 
            : base($"{exceptionType}: {exceptionMessage}")
        {
            Error = exceptionType;
        }
    }
}