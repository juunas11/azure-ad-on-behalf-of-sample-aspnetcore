using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Graph;
using System.Threading.Tasks;

namespace ApiOnBehalfSample.Services
{
    public class GraphApiService : IGraphApiService
    {
        private readonly IDistributedCache _distributedCache;
        private readonly ILoggerFactory _loggerFactory;
        private readonly IDataProtectionProvider _dataProtectionProvider;
        private readonly IAuthenticationProvider _msGraphAuthenticationProvider;
        private readonly Options.AuthenticationOptions _authSettings;

        public GraphApiService(
            IOptions<Options.AuthenticationOptions> authOptions,
            IDistributedCache distributedCache,
            ILoggerFactory loggerFactory,
            IDataProtectionProvider dataProtectionProvider,
            IAuthenticationProvider authenticationProvider)
        {
            _distributedCache = distributedCache;
            _loggerFactory = loggerFactory;
            _dataProtectionProvider = dataProtectionProvider;
            _msGraphAuthenticationProvider = authenticationProvider;
            _authSettings = authOptions.Value;
        }

        public async Task<User> GetUserProfileAsync()
        {
            var client = new GraphServiceClient(_msGraphAuthenticationProvider);
            return await client.Me.Request().GetAsync();
        }
    }
}
