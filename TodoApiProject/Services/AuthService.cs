using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using TodoApiProject.Data;
using TodoApiProject.Data.Entities;
using TodoApiProject.Models.RequestModels.Auth;
using TodoApiProject.Models.ResponseModels;
using TodoApiProject.Models.ResponseModels.Auth;
using TodoApiProject.Services.Interfaces;

namespace TodoApiProject.Services
{
    public class AuthService : IAuthService
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        public AuthService(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public async Task<CommonResponse<SignupResponse>> Signup(SignupRequest request)
        {
            var userExists = await _context.Users.AnyAsync(x => x.Email == request.Email);

            if (userExists)
                return CommonResponse<SignupResponse>.Failure("User already exists");

            var user = new UserEntity
            {
                Name = request.Name,
                Email = request.Email,
                Password = BCrypt.Net.BCrypt.HashPassword(request.Password)
            };

            _context.Users.Add(user);

            await _context.SaveChangesAsync();

            return CommonResponse<SignupResponse>.Success("User created successfully",
                new SignupResponse()
                {
                    Id = user.Id,
                    Name = user.Name,
                    Email = user.Email
                }
            );
        }

        public async Task<CommonResponse<LoginResponse>> Login(LoginRequest request)
        {
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Email == request.Email);

            if (user == null)
                return CommonResponse<LoginResponse>.NotFound("User not found");

            bool isPasswordValid = BCrypt.Net.BCrypt.Verify(request.Password, user.Password);

            if (!isPasswordValid)
                return CommonResponse<LoginResponse>.Unauthorized("Email or password is incorrect");

            var token = GenerateJwtToken(user);

            return CommonResponse<LoginResponse>.Success("User logged in successfully",
                new LoginResponse()
                {
                    Id = user.Id,
                    Name = user.Name,
                    Email = user.Email,
                    Token = token
                }
            );
        }

        public async Task<CommonResponse<ProfileResponse>> GetProfile(Guid userId)
        {
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Id == userId);

            if (user == null)
                return CommonResponse<ProfileResponse>.NotFound("User not found");

            return CommonResponse<ProfileResponse>.Success("User found successfully",
                new ProfileResponse
                {
                    Id = user.Id,
                    Name = user.Name,
                    Email = user.Email
                }
            );
        }

        #region Private Methods
        private string GenerateJwtToken(UserEntity user)
        {
            // JwtToken = Header + Payload + Signature

            // Header
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"] ?? string.Empty));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // Payload
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.Name)
            };


            var token = new JwtSecurityToken(
                    issuer: _configuration["Jwt:Issuer"],
                    audience: _configuration["Jwt:Audience"],
                    claims: claims,
                    expires: DateTime.UtcNow.AddHours(1),
                    signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
        #endregion
    }
}
