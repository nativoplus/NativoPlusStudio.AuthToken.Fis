using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NativoPlusStudio.AuthToken.Core.Interfaces;
using NativoPlusStudio.AuthToken.FIS;

namespace NativoPlusStudio.AuthToken.FisTests
{
    [TestClass]
    public class FisTests:BaseConfiguration
    {
        private IAuthTokenGenerator tokenGenerator;
        private FisHttpClient fcsClient;
        public FisTests()
        {
            tokenGenerator = serviceProvider.GetRequiredService<IAuthTokenGenerator>();
            //fcsClient = serviceProvider.GetRequiredService<FisHttpClient>();
        }

        //[TestMethod]
        //public void Test1()
        //{
        //    var token = tokenGenerator.GetTokenAsync("FIS").GetAwaiter().GetResult();
        //    Assert.IsTrue(true);
        //}

        //[TestMethod]
        //public void Test2()
        //{
        //    var resp = fcsClient
        //        //uncomment and add your own
        //        .GetFillingAsync("")
        //        .GetAwaiter().GetResult();

        //    var cont = resp.Content.ReadAsStringAsync().GetAwaiter().GetResult();

        //    Assert.IsTrue(true);
        //}

        [TestMethod]
        public void Test()
        {
            var provider = new FisAuthTokenProvider(new System.Net.Http.HttpClient(), Options.Create(new FIS.Configuration.FisAuthTokenOptions()));
            Assert.IsTrue(true);
        }
    }
}
