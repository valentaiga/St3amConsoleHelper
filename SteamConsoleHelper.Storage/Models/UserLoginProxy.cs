using SteamAuth;

namespace SteamConsoleHelper.Storage.Models
{
    internal class UserLoginProxy
    {
        public ulong SteamId { get; set; }

        public SessionData Session { get; set; }

        public bool LoggedIn { get; set; }

        public UserLoginProxy()
        {
        }

        public UserLoginProxy(UserLogin userLogin)
        {
            SteamId = userLogin.SteamID;
            Session = userLogin.Session;
            LoggedIn = userLogin.LoggedIn;
        }

        public UserLogin CastToOrigin()
        {
            return new UserLogin(null, null)
            {
                SteamID = SteamId, 
                Session = Session, 
                LoggedIn = LoggedIn
            };
        }
    }
}
