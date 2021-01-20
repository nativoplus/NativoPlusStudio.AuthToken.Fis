using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using NativoPlusStudio.AuthToken.FIS.Extensions;
using FisLib;

namespace NativoPlusStudio.AuthToken.FisTests
{
    public abstract class BaseConfiguration
    {
        public IServiceProvider serviceProvider;
        public IConfiguration configuration;
        public BaseConfiguration()
        {
            configuration = new ConfigurationBuilder()
             .SetBasePath(Directory.GetCurrentDirectory())
             .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
             .Build();

            var services = new ServiceCollection();

            //services.AddFisAuthTokenProvider((options, builder) =>
            //        {
            //            options.AddFisOptions(
            //                protectedResource: configuration["FisOptions:ProtectedResourceName"],
            //                userName: configuration["FisOptions:Username"],
            //                password: configuration["FisOptions:Password"],
            //                checkSystems: configuration["FisOptions:CheckSystemsEndpoint"],
            //                url: configuration["FisOptions:Url"],
            //                includeEncryptedTokenInResponse: true
            //            );

            //        });

            //services.AddHttpClient<FisHttpClient>((provider, client) => {
            //    client.DefaultRequestHeaders.Add("SOAPAction", "");

            //    client.BaseAddress = new Uri(configuration["FisOptions:Url"]);
            //})
            //.AddRefreshFisTokenPolicyWithJitteredBackoff(configuration["FisOptions:ProtectedResourceName"], 1, 2);

            serviceProvider = services.BuildServiceProvider();
        }
    }
}
