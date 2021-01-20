using FisLib;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NativoPlusStudio.AuthToken.Core.Interfaces;
using NativoPlusStudio.AuthToken.FIS.Extensions;
using System;
using System.IO;

namespace ExampleApp
{
    class Program
    {
        public static IServiceProvider serviceProvider;
        public static FisHttpClient client;
        static void Main(string[] args)
        {
            IConfiguration configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile($"{AppContext.BaseDirectory}/appsettings.json", optional: false, reloadOnChange: true)
                            .Build();

            var services = new ServiceCollection();

            services.AddFisAuthTokenProvider((options, builder) =>
            {
                options.AddFisOptions(
                            protectedResource: configuration["FisOptions:ProtectedResourceName"],
                            userName: configuration["FisOptions:Username"],
                            password: configuration["FisOptions:Password"],
                            checkSystems: configuration["FisOptions:CheckSystemsEndpoint"],
                            url: configuration["FisOptions:Url"],
                            includeEncryptedTokenInResponse: true
                        );
            });

            services.AddHttpClient<FisHttpClient>((provider, client) =>
            {
                client.BaseAddress = new Uri(configuration["FisOptions:Url"]);
            })
            .AddRefreshFisTokenPolicyWithJitteredBackoff(protectedResourceName: configuration["FisOptions:ProtectedResourceName"], initialDelayInSeconds: 1, retryCount: 2)
            ;

            serviceProvider = services.BuildServiceProvider();

            var token = serviceProvider.GetService<IAuthTokenGenerator>().GetTokenAsync(protectedResource: configuration["FisOptions:ProtectedResourceName"]).GetAwaiter().GetResult();

            client = serviceProvider.GetRequiredService<FisHttpClient>();

            var response = client.ChexSystems(token.Token).GetAwaiter().GetResult();

            Console.WriteLine(response.Content.ReadAsStringAsync().GetAwaiter().GetResult());

        }
    }
}
