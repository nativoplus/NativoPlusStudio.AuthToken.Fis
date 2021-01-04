using Microsoft.Extensions.DependencyInjection;
using NativoPlusStudio.AuthToken.Core.Enums;
using NativoPlusStudio.AuthToken.Core.Extensions;
using NativoPlusStudio.AuthToken.Core.Interfaces;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace NativoPlusStudio.AuthToken.FIS.Extensions
{
    public static class FisHelperExtensions
    {
        public static IHttpClientBuilder AddRefreshFisTokenPolicyWithJitteredBackoff(this IHttpClientBuilder builder, string protectedResourceName, int initialDelayInSeconds, int retryCount)
        {
            builder.AddPolicyHandler((provider, request) => provider
               .GetRequiredService<IAuthTokenGenerator>()
               .CreateTokenRefreshPolicy(
                   request,
                   protectedResourceName,
                   async (generator, message, protectedResource) =>
                   {
                       await UpdateHttpRequestMessage(generator, message, protectedResource);
                   },
                   BackoffAlgorithmTypeEnums.Jitter,
                   initialDelayInSeconds,
                   retryCount
               )
               .WrapAsync(
                HttpClientPolicyHelperExtensions
                .AsyncFallbackPolicy(
                    (message) => message.StatusCode == HttpStatusCode.InternalServerError,
                    async (message) =>
                    {
                        string stringContent = await message.Content.ReadAsStringAsync(); // Could wrap this line in an additional policy as desired.
                        var innerTextInNode = stringContent
                            .GetXmlDocument()
                            .GetInnerTextInNode(AllMagicStrings.ErrorMessageNodeInFisResponse);

                        if (!string.IsNullOrEmpty(innerTextInNode))
                        {
                            if (message.StatusCode == HttpStatusCode.InternalServerError && (innerTextInNode?.Contains("Session Invalid") ?? false))
                            {
                                message.StatusCode = HttpStatusCode.Unauthorized;
                            }
                        }
                        return message;
                    }
                )));

            return builder;
        }

        private static async Task UpdateHttpRequestMessage(IAuthTokenGenerator generator, HttpRequestMessage message, string protectedResource)
        {
            var token = await generator.GetTokenAsync(protectedResource);
            if (!string.IsNullOrEmpty(token.Token))
            {
                var cont = await message.Content.ReadAsStringAsync();

                var xmlDoc = cont.GetXmlDocument();
                var node = xmlDoc.GetFirstNode(AllMagicStrings.TokenNodeInFisRequest);

                if (node != null)
                {
                    node.InnerText = token?.Token;
                    message.Content = new StringContent(xmlDoc.InnerXml, Encoding.UTF8, "application/xml");
                }
            }
        }
    }
}
