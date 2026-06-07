using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TodoApi.Tests.Helper;
using TodoApiProject.Data;
using TodoApiProject.Data.Entities;
using TodoApiProject.Models.RequestModels.Auth;
using TodoApiProject.Services;

namespace TodoApi.Tests
{
    public class AuthServiceTests
    {
        ApplicationDbContext dbContext;
        AuthService service;
        IConfiguration configuration;
        public AuthServiceTests()
        {
            dbContext = TestDbContextFactory.Create();
            configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "Jwt:Key", "ThisIsMyVerySecretKeyForJwtToken12345" },
                { "Jwt:Issuer", "MyAPI" },
                { "Jwt:Audience", "MyUsers" }
            })
            .Build();
            service = new AuthService(dbContext, configuration);
        }

        // Signup
        [Fact]
        public async Task Signup_Should_Create_User()
        {
            // Arrange
            var request = new SignupRequest
            {
                Name = "Akshay",
                Email = "akshay@gmail.com",
                Password = "123456"
            };

            // Act
            var result = await service.Signup(request);

            // Assert
            result.Should().NotBeNull();
            result.Status.Should().Be("Success");
            result.Message.Should().Be("User created successfully");
            dbContext.Users.Count().Should().Be(1);

            var user = await dbContext.Users.FirstAsync();

            user.Name.Should().Be("Akshay");
            user.Email.Should().Be("akshay@gmail.com");
            user.Password.Should().NotBe("123456");
        }

        [Fact]
        public async Task Signup_Should_Return_Failure_When_Email_Already_Exists()
        {
            // Arrange
            dbContext.Users.Add(new UserEntity
            {
                Name = "Existing User",
                Email = "akshay@gmail.com",
                Password = "hash"
            });

            await dbContext.SaveChangesAsync();

            var request = new SignupRequest
            {
                Name = "Akshay",
                Email = "akshay@gmail.com",
                Password = "123456"
            };

            // Act
            var result = await service.Signup(request);

            // Assert
            result.Status.Should().Be("Failure");
            result.Message.Should().Be("User already exists");
            dbContext.Users.Count().Should().Be(1);
        }

        // Login
        [Fact]
        public async Task Login_Should_Return_Success_When_Credentials_Are_Valid()
        {
            // Arrange
            dbContext.Users.Add(new UserEntity
            {
                Name = "Akshay",
                Email = "akshay@gmail.com",
                Password = BCrypt.Net.BCrypt.HashPassword("123456")
            });

            await dbContext.SaveChangesAsync();

            var request = new LoginRequest
            {
                Email = "akshay@gmail.com",
                Password = "123456"
            };

            // Act
            var result = await service.Login(request);

            // Assert
            result.Should().NotBeNull();
            result.Status.Should().Be("Success");
            result.Message.Should().Be("User logged in successfully");
            result.Data.Should().NotBeNull();
        }

        [Fact]
        public async Task Login_Should_Return_Unauthorized_When_User_Does_Not_Exist()
        {
            // Arrange
            var request = new LoginRequest
            {
                Email = "test@gmail.com",
                Password = "123456"
            };

            // Act
            var result = await service.Login(request);

            // Assert
            result.Status.Should().Be("NotFound");
            result.Message.Should().Be("User not found");
            result.Data.Should().BeNull();
        }

        [Fact]
        public async Task Login_Should_Return_Unauthorized_When_Password_Is_Wrong()
        {
            // Arrange
            dbContext.Users.Add(new UserEntity
            {
                Name = "Akshay",
                Email = "akshay@gmail.com",
                Password = BCrypt.Net.BCrypt.HashPassword("123456")
            });

            await dbContext.SaveChangesAsync();

            var request = new LoginRequest
            {
                Email = "akshay@gmail.com",
                Password = "999999"
            };

            // Act
            var result = await service.Login(request);

            // Assert
            result.Status.Should().Be("Unauthorized");
            result.Message.Should().Be("Email or password is incorrect");
            result.Data.Should().BeNull();
        }

        [Fact]
        public async Task Login_Should_Return_Jwt_Token()
        {
            // Arrange
            dbContext.Users.Add(new UserEntity
            {
                Name = "Akshay",
                Email = "akshay@gmail.com",
                Password = BCrypt.Net.BCrypt.HashPassword("123456")
            });

            await dbContext.SaveChangesAsync();

            var request = new LoginRequest
            {
                Email = "akshay@gmail.com",
                Password = "123456"
            };

            // Act
            var result = await service.Login(request);

            // Assert
            result.Status.Should().Be("Success");
            result.Data?.Token.Should().NotBeNullOrWhiteSpace();
            result.Data?.Email.Should().Be("akshay@gmail.com");
        }

        // Profile
        [Fact]
        public async Task GetProfile_Should_Return_User_Profile_When_User_Exists()
        {
            // Arrange
            var userId = Guid.NewGuid();

            dbContext.Users.Add(new UserEntity
            {
                Id = userId,
                Name = "Akshay",
                Email = "akshay@gmail.com",
                Password = BCrypt.Net.BCrypt.HashPassword("123456")
            });

            await dbContext.SaveChangesAsync();

            // Act
            var result = await service.GetProfile(userId);

            // Assert
            result.Should().NotBeNull();
            result.Status.Should().Be("Success");
            result.Message.Should().Be("User found successfully");
            result.Data.Should().NotBeNull();
            result.Data!.Id.Should().Be(userId);
            result.Data.Name.Should().Be("Akshay");
            result.Data.Email.Should().Be("akshay@gmail.com");
        }

        [Fact]
        public async Task GetProfile_Should_Return_Failure_When_User_Does_Not_Exist()
        {
            // Arrange
            var userId = Guid.NewGuid();

            // Act
            var result = await service.GetProfile(userId);

            // Assert
            result.Should().NotBeNull();
            result.Status.Should().Be("NotFound");
            result.Message.Should().Be("User not found");
            result.Data.Should().BeNull();
        }

        [Fact]
        public async Task GetProfile_Should_Return_Correct_User_When_Multiple_Users_Exist()
        {
            // Arrange
            var targetUserId = Guid.NewGuid();

            dbContext.Users.AddRange(
                new UserEntity
                {
                    Id = Guid.NewGuid(),
                    Name = "User1",
                    Email = "user1@gmail.com",
                    Password = "hash"
                },
                new UserEntity
                {
                    Id = targetUserId,
                    Name = "Akshay",
                    Email = "akshay@gmail.com",
                    Password = "hash"
                });

            await dbContext.SaveChangesAsync();

            // Act
            var result = await service.GetProfile(targetUserId);

            // Assert
            result.Status.Should().Be("Success");
            result.Data.Should().NotBeNull();
            result.Data!.Id.Should().Be(targetUserId);
            result.Data.Name.Should().Be("Akshay");
        }
    }
}
