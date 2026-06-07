using FluentAssertions;
using System.Net;
using System.Net.Http.Json;
using TodoApi.IntegrationTests.Helper;
using TodoApiProject.Models.RequestModels.Auth;
using TodoApiProject.Models.ResponseModels;
using TodoApiProject.Models.ResponseModels.Auth;

namespace TodoApi.IntegrationTests
{
    public class AuthControllerTests : IntegrationTestBase
    {
        public AuthControllerTests(CustomWebApplicationFactory factory)
        : base(factory)
        {
        }

        [Fact]
        public async Task Signup_Should_Return_Success()
        {
            // Arrange
            var request = new SignupRequest
            {
                Name = "Akshay",
                Email = "akshay@gmail.com",
                Password = "123456"
            };

            // Act
            var response = await Client.PostAsJsonAsync("/api/auth/signup", request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task Signup_Should_Return_BadRequest()
        {
            // Arrange
            var request = new SignupRequest
            {
                Name = "",
                Email = "",
                Password = ""
            };

            // Act
            var response = await Client.PostAsJsonAsync("/api/auth/signup", request);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Login_Should_Return_Jwt_Token()
        {
            // Arrange
            await Client.PostAsJsonAsync(
                "/api/auth/signup",
                new SignupRequest
                {
                    Name = "Akshay",
                    Email = "akshay@gmail.com",
                    Password = "123456"
                });

            // Act
            var response =
                await Client.PostAsJsonAsync(
                    "/api/auth/login",
                    new LoginRequest
                    {
                        Email = "akshay@gmail.com",
                        Password = "123456"
                    });

            // Assert
            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var result = await response.Content
                .ReadFromJsonAsync<CommonResponse<LoginResponse>>();

            result.Should().NotBeNull();
            result!.Status.Should().Be("Success");
            result.Message.Should().Be("User logged in successfully");

            result.Data.Should().NotBeNull();
            result.Data!.Token.Should().NotBeNullOrWhiteSpace();
        }

        [Fact]
        public async Task Login_Should_Return_Unauthorized()
        {
            // Arrange
            await Client.PostAsJsonAsync(
                "/api/auth/signup",
                new SignupRequest
                {
                    Name = "Akshay",
                    Email = "akshay@gmail.com",
                    Password = "123456"
                });

            // Act
            var response =
                await Client.PostAsJsonAsync(
                    "/api/auth/login",
                    new LoginRequest
                    {
                        Email = "akshay@gmail.com",
                        Password = "999999"
                    });

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }
    }
}
