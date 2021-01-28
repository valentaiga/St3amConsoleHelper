namespace SteamConsoleHelper.Abstractions.Login
{
    public struct LoginResult
    {
        public bool IsError => Result != SteamAuth.LoginResult.LoginOkay;

        public bool IsTwoFactorNeeded => Result == SteamAuth.LoginResult.NeedCaptcha ||
                                         Result == SteamAuth.LoginResult.Need2FA ||
                                         Result == SteamAuth.LoginResult.NeedEmail;

        public string ErrorText { get; set; }

        public SteamAuth.LoginResult Result { get; set; }

        public LoginResult(SteamAuth.LoginResult result)
            : this(result, null)
        { }

        public LoginResult(SteamAuth.LoginResult result, string errorText)
            => (Result, ErrorText) = (result, errorText);
    }
}