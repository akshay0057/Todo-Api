using TodoApiProject.Models.RequestModels.Auth;
using TodoApiProject.Models.ResponseModels;
using TodoApiProject.Models.ResponseModels.Auth;

namespace TodoApiProject.Services.Interfaces
{
    public interface IAuthService
    {
        Task<CommonResponse<SignupResponse>> Signup(SignupRequest request);
        Task<CommonResponse<LoginResponse>> Login(LoginRequest request);
        Task<CommonResponse<ProfileResponse>> GetProfile(Guid userId);
    }
}
