using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using TodoApi.IntegrationTests.Helper;
using TodoApiProject.Models.RequestModels.Auth;
using TodoApiProject.Models.RequestModels.Todo;
using TodoApiProject.Models.ResponseModels;
using TodoApiProject.Models.ResponseModels.Auth;

namespace TodoApi.IntegrationTests
{
    public class TodoControllerTests : IntegrationTestBase
    {
        public TodoControllerTests(CustomWebApplicationFactory factory)
        : base(factory)
        {
        }

        [Fact]
        public async Task GetTodos_Should_Return_401()
        {
            var response =
                await Client.GetAsync(
                    "/api/todo/all");

            response.StatusCode
                .Should()
                .Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task CreateTodo_Should_Return_200()
        {
            var token = await GetToken();

            Client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue(
                    "Bearer",
                    token);

            var response =
                await Client.PostAsJsonAsync(
                    "/api/todo/create",
                    new CreateTodoRequest
                    {
                        Title = "Learn JWT"
                    });

            response.StatusCode
                .Should()
                .Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task CreateTodo_Should_Return_400()
        {
            var token = await GetToken();

            Client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue(
                    "Bearer",
                    token);

            var response =
                await Client.PostAsJsonAsync(
                    "/api/todo/create",
                    new CreateTodoRequest
                    {
                        Title = ""
                    });

            response.StatusCode
                .Should()
                .Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task GetAllTodos_Should_Return_Data()
        {
            var token = await GetToken();

            Client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue(
                    "Bearer",
                    token);

            await Client.PostAsJsonAsync(
                "/api/todo/create",
                new CreateTodoRequest
                {
                    Title = "Todo 1"
                });

            var response =
                await Client.GetAsync(
                    "/api/todo/all");

            response.StatusCode
                .Should()
                .Be(HttpStatusCode.OK);
        }


        #region Private Methods
        private async Task<string> GetToken()
        {
            await Client.PostAsJsonAsync(
                "/api/auth/signup",
                new SignupRequest
                {
                    Name = "Akshay",
                    Email = "akshay@gmail.com",
                    Password = "123456"
                });

            var loginResponse =
                await Client.PostAsJsonAsync(
                    "/api/auth/login",
                    new LoginRequest
                    {
                        Email = "akshay@gmail.com",
                        Password = "123456"
                    });

            var content =
                await loginResponse.Content
                    .ReadFromJsonAsync<
                        CommonResponse<LoginResponse>>();

            return content!.Data!.Token;
        }
        #endregion
    }
}
