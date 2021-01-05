using HandlebarsDotNet;
using Microsoft.Extensions.Options;
using NativoPlusStudio.AuthToken.Core;
using NativoPlusStudio.AuthToken.Core.Extensions;
using NativoPlusStudio.AuthToken.Core.Interfaces;
using NativoPlusStudio.AuthToken.DTOs;
using NativoPlusStudio.AuthToken.FIS.Configuration;
using NativoPlusStudio.Encryption.Interfaces;
using Serilog;
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml;

namespace NativoPlusStudio.AuthToken.FIS
{
    public class FisAuthTokenProvider : BaseTokenProvider<FisAuthTokenOptions>, IAuthTokenProvider
    {
        private readonly HttpClient _client;
        public FisAuthTokenProvider(
            HttpClient client,
            IOptions<FisAuthTokenOptions> fisOptions,
            IEncryption symmetricEncryption = null,
            IAuthTokenCacheService tokenCacheService = null,
            ILogger logger = null
        )
            : base(symmetricEncryption, tokenCacheService, logger, fisOptions)
        {
            _client = client;
        }

        public async Task<ITokenResponse> GetTokenAsync()
        {
            _logger.Information("FisAuthTokenProvider GetTokenAsync start");
            try
            {
                if (_tokenCacheService != null)
                {
                    var cachedToken = _tokenCacheService.GetCachedAuthToken(_options.ProtectedResource);

                    if (!cachedToken?.IsExpired ?? false)
                    {
                        return GetTokenFromCache(cachedToken);
                    }
                }

                _logger.Information("#GetToken");
                var (Response, Status, Code, Message) = await (await _client
                    .PostAsync(GetAuthCommand(), _options.CheckSystems, "application/xml"))
                    .TransformHttpResponseToString();

                TokenResponse tokenResponse = null;

                if (Status)
                {
                    var authToken = Response.GetXmlDocument()
                        .GetInnerTextInNode(AllMagicStrings.TokenNodeInFisAuthRequest);
                    
                    if (string.IsNullOrWhiteSpace(authToken))
                    {
                        LogAuthTokenEmptyAnError();
                        tokenResponse = new TokenResponse();
                    }
                    else
                    {
                        tokenResponse = new TokenResponse
                        {
                            Token = authToken,
                            EncryptedToken = _options.IncludeEncryptedTokenInResponse && _symmetricEncryption != null
                        ? _symmetricEncryption.Encrypt(authToken)
                        : null,
                            ExpiryDateUtc = DateTime.UtcNow.AddMinutes(15),
                            TokenType = "FIS"
                        };

                        if (_tokenCacheService != null)
                        {
                            TokenCacheUpsert(_options.ProtectedResource, tokenResponse);
                        }
                    }
                }
                else
                {
                    LogAuthTokenEmptyAnError();
                }

                _logger.Information($"Encrypted Token: {tokenResponse.EncryptedToken}. Included EncryptedToken InResponse: {_options.IncludeEncryptedTokenInResponse}");
                return tokenResponse;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "#GetToken: Caught exception {Exception}");
                return new TokenResponse()
                {
                };
            }
        }

        private void LogAuthTokenEmptyAnError()
        {
            _logger.Error("#FisAuthTokenService AuthToken was empty");
        }

        private string GetAuthCommand()
        {
            _logger.Information($"#GetAuthCommand");

            var template = Handlebars.Compile(AllMagicStrings.AuthCommand);

            var data = new
            {
                UserName = _options.UserName,
                Password = _options.Password
            };

            var result = template(data);
            return result;
        }

        private void TokenCacheUpsert(string protectedResource, ITokenResponse tokenResponse)
        {
            _logger.Information("FisAuthTokenProvider TokenCacheUpsert start");

            string tokenTobeStored;
            if (tokenResponse.EncryptedToken != null)
            {
                tokenTobeStored = tokenResponse.EncryptedToken;
            }
            else
            {
                tokenTobeStored = _symmetricEncryption != null
                    ? _symmetricEncryption.Encrypt(tokenResponse.Token)
                    : tokenResponse.Token;
            }

            var (upsertResult, errorMessage) = _tokenCacheService.UpsertAuthTokenCache(
                    protectedResource.ToString(),
                    tokenTobeStored,
                    tokenResponse.TokenType,
                    tokenResponse.ExpiryDateUtc
                );

            if (!string.IsNullOrEmpty(errorMessage))
            {
                _logger.Error($"#GetToken {errorMessage}");
            }
        }

        private ITokenResponse GetTokenFromCache(IAuthTokenDetails cachedToken)
        {
            _logger.Information("FisAuthTokenProvider GetTokenFromCache start");

            var decryptedToken = _symmetricEncryption != null ? _symmetricEncryption.Decrypt(cachedToken.Token) : cachedToken.Token;
            return new TokenResponse()
            {
                Token = decryptedToken,
                TokenType = cachedToken.TokenType,
                EncryptedToken = decryptedToken != cachedToken.Token ? cachedToken.Token : null,
                ExpiryDateUtc = cachedToken.ExpirationDate

            };
        }
    }
}
