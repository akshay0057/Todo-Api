using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TodoApiProject.Models.RequestModels.Auth;
using TodoApiProject.Models.ResponseModels;
using TodoApiProject.Services.Interfaces;

namespace TodoApiProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [AllowAnonymous]
        [HttpPost("signup")]
        public async Task<IActionResult> Signup([FromBody] SignupRequest request)
        {
            var user = await _authService.Signup(request);

            return Ok(user);
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequest request)
        {
            var response = await _authService.Login(request);

            return response.Status switch
            {
                "Unauthorized" => Unauthorized(response),
                "NotFound" => NotFound(response),
                "Failure" => BadRequest(response),
                _ => Ok(response)
            };
        }

        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            // var userId = HttpContext.Items["UserId"]; // When we will custom middleware will be implemented
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userId is not string userIdString || string.IsNullOrEmpty(userIdString))
            {
                return Unauthorized();
            }

            if (!Guid.TryParse(userIdString, out Guid userIdGuid))
            {
                return BadRequest(CommonResponse<object>.Failure("Invalid User ID format."));
            }

            var response = await _authService.GetProfile(userIdGuid);

            return Ok(response);
        }
    }
}
