using Microsoft.Extensions.DependencyInjection;
using NativoPlusStudio.AuthToken.Core;
using NativoPlusStudio.AuthToken.Core.Extensions;
using NativoPlusStudio.AuthToken.Core.Interfaces;
using NativoPlusStudio.AuthToken.FIS.Configuration;
using Polly;
using System;

namespace NativoPlusStudio.AuthToken.FIS.Extensions
{
    public static class FisServiceCollectionExtensions
    {
        public static void AddFisAuthTokenProvider(this IAuthTokenProviderBuilder builder,
            Action<FisAuthTokenOptions, AuthTokenServicesBuilder> actions
            )
        {
            var fisOptions = new FisAuthTokenOptions();
            var servicesBuilder = new AuthTokenServicesBuilder() { Services = builder.Services };

            actions.Invoke(fisOptions, servicesBuilder);

            builder.AddTokenProviderHelper(fisOptions.ProtectedResource, () =>
            {
                builder.Services.Configure<FisAuthTokenOptions>(f =>
                {
                    f.BaseUrl = fisOptions.BaseUrl;
                    f.UserName = fisOptions.UserName;
                    f.Password = fisOptions.Password;
                    f.ProtectedResource = fisOptions.ProtectedResource;
                    f.IncludeEncryptedTokenInResponse = fisOptions.IncludeEncryptedTokenInResponse;
                    f.CheckSystems = fisOptions.CheckSystems;
                });

                builder.Services
                .AddHttpClient<IAuthTokenProvider, FisAuthTokenProvider>(client =>
                {
                    client.DefaultRequestHeaders.Add("SOAPAction", "");
                    client.BaseAddress = new Uri(fisOptions.BaseUrl);
                })
                .AddTransientHttpErrorPolicy(builder => builder.WaitAndRetryAsync(new[]
                {
                    TimeSpan.FromSeconds(1),
                    TimeSpan.FromSeconds(5),
                    TimeSpan.FromSeconds(10)
                }))
                .AddTransientHttpErrorPolicy(builder => builder.CircuitBreakerAsync(
                    handledEventsAllowedBeforeBreaking: 3,
                    durationOfBreak: TimeSpan.FromSeconds(30)
                ));

                builder.Services.AddTransient(implementationFactory => servicesBuilder.EncryptionService);
                builder.Services.AddTransient(implementationFactory => servicesBuilder.TokenCacheService);
            });
        }
    }
}
