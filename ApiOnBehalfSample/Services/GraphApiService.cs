using Microsoft.Graph;
using System.Threading.Tasks;

namespace ApiOnBehalfSample.Services
{
    public class GraphApiService : IGraphApiService
    {
        private readonly IAuthenticationProvider _msGraphAuthenticationProvider;

        public GraphApiService(IAuthenticationProvider authenticationProvider)
        {
            _msGraphAuthenticationProvider = authenticationProvider;
        }

        public async Task<User> GetUserProfileAsync()
        {
            var client = new GraphServiceClient(_msGraphAuthenticationProvider);
            return await client.Me.Request().GetAsync();
        }
    }
}
