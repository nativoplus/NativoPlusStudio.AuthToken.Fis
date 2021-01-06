using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using NativoPlusStudio.AuthToken.FIS.Extensions;

namespace NativoPlusStudio.AuthToken.FisTests
{
    public abstract class BaseConfiguration
    {
        public IServiceProvider serviceProvider;

        public BaseConfiguration()
        {
            IConfiguration configuration = new ConfigurationBuilder()
             .SetBasePath(Directory.GetCurrentDirectory())
             .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
             .Build();

            var services = new ServiceCollection();

            services.AddFisAuthTokenProvider((options, builder) =>
                    {
                        options.AddFisOptions(
                            protectedResource: "FIS",
                            userName: "",
                            password: "",
                            checkSystems: "chexsystems?wsdl",
                            url: "https://penleyincqa.penleyinc.com/fissoap1/services/",
                            includeEncryptedTokenInResponse: true
                        );
                    
                });

            services.AddHttpClient<FisHttpClient>((provider, client) => {
                client.DefaultRequestHeaders.Add("SOAPAction", "");

                client.BaseAddress = new Uri("https://penleyincqa.penleyinc.com/fissoap1/services/");
            })
            .AddRefreshFisTokenPolicyWithJitteredBackoff("FIS", 1, 2);

            serviceProvider = services.BuildServiceProvider();
        }
    }
}
