namespace SteamConsoleHelper.Abstractions.Login
{
    public struct LoginResult
    {
        public bool IsError => Result != SteamAuth.LoginResult.LoginOkay;

        public string ErrorText { get; set; }

        public SteamAuth.LoginResult Result { get; set; }

        public LoginResult(SteamAuth.LoginResult result)
            : this(result, null)
        { }

        public LoginResult(SteamAuth.LoginResult result, string errorText)
            => (Result, ErrorText) = (result, errorText);
    }
}