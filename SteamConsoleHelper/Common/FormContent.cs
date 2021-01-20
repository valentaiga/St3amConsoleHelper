using System.Net.Http;
using System.Text;

namespace SteamConsoleHelper.Common
{
    public class FormContent : StringContent
    {
        public FormContent(string value) :
            base(value, Encoding.UTF8, "application/x-www-form-urlencoded")
        { }
    }
}