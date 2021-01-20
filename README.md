# NativoPlusStudio.AuthToken.Fis

NativoPlusStudio.AuthToken.Fis is part of the NativoPlusStudio.AuthToken set of libraries that can be used to retrieve the auth token to be able to interface with the FIS SOAP APIs.

### Usage

To use this nuget package you can use the AddFisAuthTokenProvider extension method to register the FisAuthTokenProvider service in a Console app or api. Here is an example:

```csharp
using FisLib;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NativoPlusStudio.AuthToken.Core.Interfaces;
using NativoPlusStudio.AuthToken.FIS.Extensions;
using System;
using System.IO;
using Newtonsoft.Json;

namespace ExampleApp
{
    class Program
    {
        public static IServiceProvider serviceProvider;
        static void Main(string[] args)
        {
            IConfiguration configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile($"{AppContext.BaseDirectory}/appsettings.json", optional: false, reloadOnChange: true)
                            .Build();

            var services = new ServiceCollection();

            //use AddFisAuthTokenProvider to add the FisAuthTokenProvider service into the Services pipeline
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

            serviceProvider = services.BuildServiceProvider();

            var token = serviceProvider.GetService<IAuthTokenGenerator>().GetTokenAsync(protectedResource: configuration["FisOptions:ProtectedResourceName"]).GetAwaiter().GetResult();

            Console.WriteLine(JsonConvert.SerializeObject(token));
        }
    }
}

```

This nuget package also includes extension methods to extends IHttpClientBuilder to add a Retry Authorization algorithm that fetches the token if the consequent requests to the Fis endpoints return a 401 Unauthorized.

Example of these extension methods:

```csharp
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

            //use AddFisAuthTokenProvider to add the FisAuthTokenProvider service into the Services pipeline
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

            //your client to connect to the Fis endpoints
            services.AddHttpClient<FisHttpClient>((provider, client) =>
            {
                client.BaseAddress = new Uri(configuration["FisOptions:Url"]);
            })
            //add the Retry Authorization algorithm
            .AddRefreshFisTokenPolicyWithJitteredBackoff(protectedResourceName: configuration["FisOptions:ProtectedResourceName"], initialDelayInSeconds: 2, retryCount: 2)
            ;

            serviceProvider = services.BuildServiceProvider();

            var token = serviceProvider.GetService<IAuthTokenGenerator>().GetTokenAsync(protectedResource: configuration["FisOptions:ProtectedResourceName"]).GetAwaiter().GetResult();

            client = serviceProvider.GetRequiredService<FisHttpClient>();

            var response = client.ChexSystems(token.Token).GetAwaiter().GetResult();

            Console.WriteLine(response.Content.ReadAsStringAsync().GetAwaiter().GetResult());
        }
    }
}
```

The above code can be found in a console app in the project called ExampleApp.

Visit the following repositories for examples on how to use other auth token nuget packages

https://github.com/nativoplus/NativoPlusStudio.AuthToken.SymmetricEncryption
https://github.com/nativoplus/NativoPlusStudio.AuthToken.SqlServerCaching
https://github.com/nativoplus/NativoPlusStudio.AuthToken.Core
https://github.com/nativoplus/NativoPlusStudio.AuthToken.Ficoso
