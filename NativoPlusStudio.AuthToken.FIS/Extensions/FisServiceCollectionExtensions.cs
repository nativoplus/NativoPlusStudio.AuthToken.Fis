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
        public static IServiceCollection AddFisAuthTokenProvider(this IServiceCollection services,
            Action<FisAuthTokenOptions, AuthTokenServicesBuilder> actions
            )
        {
            var fisOptions = new FisAuthTokenOptions();
            var servicesBuilder = new AuthTokenServicesBuilder() { Services = services };

            actions.Invoke(fisOptions, servicesBuilder);

            services.AddTokenProviderHelper(fisOptions.ProtectedResource, () =>
            {
                services.Configure<FisAuthTokenOptions>(f =>
                {
                    f.BaseUrl = fisOptions.BaseUrl;
                    f.UserName = fisOptions.UserName;
                    f.Password = fisOptions.Password;
                    f.ProtectedResource = fisOptions.ProtectedResource;
                    f.IncludeEncryptedTokenInResponse = fisOptions.IncludeEncryptedTokenInResponse;
                    f.CheckSystems = fisOptions.CheckSystems;
                });

                services
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

                services.AddTransient(implementationFactory => servicesBuilder.EncryptionService);
                services.AddTransient(implementationFactory => servicesBuilder.TokenCacheService);
            });

            return services;
        }
    }
}
