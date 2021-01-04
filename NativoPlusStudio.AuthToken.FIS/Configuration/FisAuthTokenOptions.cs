using NativoPlusStudio.AuthToken.Core.DTOs;

namespace NativoPlusStudio.AuthToken.FIS.Configuration
{
    public class FisAuthTokenOptions : BaseOptions
    {
        public string BaseUrl { get; set; }
        public string CheckSystems { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string ProtectedResource { get; set; }

        public void AddFisOptions(string checkSystems,
         string userName,
         string password,
         string protectedResource,
         string url,
         bool includeEncryptedTokenInResponse = false)
        {
            ProtectedResource = protectedResource;
            BaseUrl = url;
            UserName = userName;
            Password = password;
            CheckSystems = checkSystems;
            IncludeEncryptedTokenInResponse = includeEncryptedTokenInResponse;
        }
    }
}
