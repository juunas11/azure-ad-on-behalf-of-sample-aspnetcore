using ApiOnBehalfSample.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace ApiOnBehalfSample.Controllers
{
    [Route("api/user")]
    public class UserController : ControllerBase
    {
        private readonly IGraphApiService _graphApiService;

        public UserController(IGraphApiService graphApiService)
        {
            _graphApiService = graphApiService;
        }

        [HttpGet("profile")]
        public async Task<IActionResult> GetUserProfile()
        {
            Microsoft.Graph.User user = await _graphApiService.GetUserProfileAsync();
            return Ok(user);
        }
    }
}
