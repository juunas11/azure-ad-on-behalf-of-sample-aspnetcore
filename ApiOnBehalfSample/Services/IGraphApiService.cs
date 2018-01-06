using System.Threading.Tasks;
using Microsoft.Graph;

namespace ApiOnBehalfSample.Services
{
    public interface IGraphApiService
    {
        Task<User> GetUserProfileAsync();
    }
}