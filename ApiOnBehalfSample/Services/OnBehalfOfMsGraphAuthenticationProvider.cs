using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Threading.Tasks;
using ApiOnBehalfSample.Extensions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Graph;
using Microsoft.IdentityModel.Clients.ActiveDirectory;

namespace ApiOnBehalfSample.Services
{
    public class OnBehalfOfMsGraphAuthenticationProvider : IAuthenticationProvider
    {
        private readonly IDistributedCache _distributedCache;
        private readonly ILoggerFactory _loggerFactory;
        private readonly IDataProtectionProvider _dataProtectionProvider;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly Options.AuthenticationOptions _authSettings;

        public OnBehalfOfMsGraphAuthenticationProvider(
            IDistributedCache distributedCache,
            ILoggerFactory loggerFactory,
            IDataProtectionProvider dataProtectionProvider,
            IOptions<Options.AuthenticationOptions> authenticationOptions,
            IHttpContextAccessor httpContextAccessor)
        {
            _distributedCache = distributedCache;
            _loggerFactory = loggerFactory;
            _dataProtectionProvider = dataProtectionProvider;
            _httpContextAccessor = httpContextAccessor;
            _authSettings = authenticationOptions.Value;
        }

        public async Task AuthenticateRequestAsync(HttpRequestMessage request)
        {
            var httpContext = _httpContextAccessor.HttpContext;

            //Get the access token used to call this API
            string token = await httpContext.GetTokenAsync("access_token");

            //We are passing an *assertion* to Azure AD about the current user
            //Here we specify that assertion's type, that is a JWT Bearer token
            string assertionType = "urn:ietf:params:oauth:grant-type:jwt-bearer";

            //User name is needed here only for ADAL, it is not passed to AAD
            //ADAL uses it to find a token in the cache if available
            var user = httpContext.User;
            string userName = user.FindFirstValue(ClaimTypes.Upn) ?? user.FindFirstValue(ClaimTypes.Email);

            var userAssertion = new UserAssertion(token, assertionType, userName);

            //Construct the token cache
            var cache = new DistributedTokenCache(user, _distributedCache, _loggerFactory, _dataProtectionProvider);

            var authContext = new AuthenticationContext(_authSettings.Authority, cache);
            var clientCredential = new ClientCredential(_authSettings.ClientId, _authSettings.ClientSecret);
            //Acquire access token
            var result = await authContext.AcquireTokenAsync("https://graph.microsoft.com", clientCredential, userAssertion);
            //Set the authentication header
            request.Headers.Authorization = new AuthenticationHeaderValue(result.AccessTokenType, result.AccessToken);
        }
    }
}