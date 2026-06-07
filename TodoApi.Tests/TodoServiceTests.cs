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
using TodoApiProject.Models.RequestModels.Todo;
using TodoApiProject.Services;

namespace TodoApi.Tests
{
    public class TodoServiceTests
    {
        ApplicationDbContext dbContext;
        TodoService service;
        public TodoServiceTests()
        {
            dbContext = TestDbContextFactory.Create();
            service = new TodoService(dbContext);
        }

        // Create Todo
        [Fact]
        public async Task CreateTodoAsync_Should_Create_Todo_Successfully()
        {
            // Arrange
            var userId = Guid.NewGuid();

            var request = new CreateTodoRequest
            {
                Title = "Learn JWT",
                Description = "Practice authentication"
            };

            // Act
            var result = await service.CreateTodoAsync(request, userId);

            // Assert
            result.Status.Should().Be("Success");
            result.Message.Should().Be("Todo created successfully");
            result.Data.Should().NotBeNull();
            result.Data!.Title.Should().Be("Learn JWT");
            result.Data.Description.Should().Be("Practice authentication");

            dbContext.TodoItems.Count().Should().Be(1);
        }

        [Fact]
        public async Task CreateTodoAsync_Should_Set_Empty_Description_When_Null()
        {
            // Arrange
            var request = new CreateTodoRequest
            {
                Title = "Learn EF Core",
                Description = null
            };

            // Act
            var result = await service.CreateTodoAsync(
                request,
                Guid.NewGuid());

            // Assert
            result.Status.Should().Be("Success");
            result.Data.Should().NotBeNull();
            result.Data!.Description.Should().Be(string.Empty);
        }

        [Fact]
        public async Task CreateTodoAsync_Should_Save_Todo_In_Database()
        {
            // Arrange
            var userId = Guid.NewGuid();

            var request = new CreateTodoRequest
            {
                Title = "Learn PostgreSQL",
                Description = "Database Practice"
            };

            // Act
            await service.CreateTodoAsync(request, userId);

            // Assert
            var todo = await dbContext.TodoItems.FirstAsync();
            todo.Title.Should().Be("Learn PostgreSQL");
            todo.Description.Should().Be("Database Practice");
            todo.UserId.Should().Be(userId);
            todo.IsCompleted.Should().BeFalse();
        }

        [Fact]
        public async Task CreateTodoAsync_Should_Assign_Correct_UserId()
        {
            // Arrange
            var userId = Guid.NewGuid();

            var request = new CreateTodoRequest
            {
                Title = "My Todo"
            };

            // Act
            await service.CreateTodoAsync(request, userId);

            // Assert
            var todo = await dbContext.TodoItems.FirstAsync();
            todo.UserId.Should().Be(userId);
        }

        [Fact]
        public async Task CreateTodoAsync_Should_Set_CreatedOn()
        {
            // Arrange
            var request = new CreateTodoRequest
            {
                Title = "Test Todo"
            };

            // Act
            await service.CreateTodoAsync(
                request,
                Guid.NewGuid());

            // Assert
            var todo = await dbContext.TodoItems.FirstAsync();

            todo.CreatedOn.Should().BeCloseTo(
                DateTime.UtcNow,
                TimeSpan.FromSeconds(5));
        }


        // Get All Todos
        [Fact]
        public async Task GetAllTodos_Should_Return_NotFound_When_No_Todos_Exist()
        {
            // Act
            var result = await service.GetAllTodosAsync(
                new TodoListFilterRequest(),
                Guid.NewGuid());

            // Assert
            result.Status.Should().Be("NotFound");
            result.Message.Should().Be("No todos found");
            result.Data.Should().BeNull();
        }

        [Fact]
        public async Task GetAllTodos_Should_Return_All_Todos()
        {
            // Arrange
            var userId = Guid.NewGuid();

            await SeedTodos(userId);

            // Act
            var result = await service.GetAllTodosAsync(
                new TodoListFilterRequest(),
                userId);

            // Assert
            result.Status.Should().Be("Success");
            result.Data.Should().NotBeNull();
            result.Data!.TodoItems.Count.Should().Be(2);
            result.Data.TotalCount.Should().Be(2);
        }

        [Fact]
        public async Task GetAllTodos_Should_Filter_Completed_Todos()
        {
            // Arrange
            var userId = Guid.NewGuid();

            await SeedTodos(userId);

            // Act
            var result = await service.GetAllTodosAsync(
                new TodoListFilterRequest
                {
                    IsCompleted = true
                },
                userId);

            // Assert
            result.Status.Should().Be("Success");
            result.Data!.TodoItems.Count.Should().Be(1);
            result.Data.TodoItems.First().IsCompleted.Should().BeTrue();
        }

        [Fact]
        public async Task GetAllTodos_Should_Filter_Pending_Todos()
        {
            // Arrange
            var userId = Guid.NewGuid();

            await SeedTodos(userId);

            // Act
            var result = await service.GetAllTodosAsync(
                new TodoListFilterRequest
                {
                    IsCompleted = false
                },
                userId);

            // Assert
            result.Data!.TodoItems.Count.Should().Be(1);
            result.Data.TodoItems.First().IsCompleted.Should().BeFalse();
        }

        [Fact]
        public async Task GetAllTodos_Should_Search_By_Title()
        {
            // Arrange
            var userId = Guid.NewGuid();

            await SeedTodos(userId);

            // Act
            var result = await service.GetAllTodosAsync(
                new TodoListFilterRequest
                {
                    SearchText = "JWT"
                },
                userId);

            // Assert
            result.Data!.TodoItems.Count.Should().Be(1);
            result.Data.TodoItems.First()
                .Title.Should().Contain("JWT");
        }

        [Fact]
        public async Task GetAllTodos_Should_Search_By_Description()
        {
            // Arrange
            var userId = Guid.NewGuid();

            await SeedTodos(userId);

            // Act
            var result = await service.GetAllTodosAsync(
                new TodoListFilterRequest
                {
                    SearchText = "Database"
                },
                userId);

            // Assert
            result.Data!.TodoItems.Count.Should().Be(1);
            result.Data.TodoItems.First()
                .Description.Should().Contain("Database");
        }

        [Fact]
        public async Task GetAllTodos_Should_Search_Case_Insensitive()
        {
            // Arrange
            var userId = Guid.NewGuid();

            await SeedTodos(userId);

            // Act
            var result = await service.GetAllTodosAsync(
                new TodoListFilterRequest
                {
                    SearchText = "jwt"
                },
                userId);

            // Assert
            result.Data!.TodoItems.Count.Should().Be(1);
        }

        [Fact]
        public async Task GetAllTodos_Should_Apply_Pagination()
        {
            // Arrange
            var userId = Guid.NewGuid();

            for (int i = 1; i <= 20; i++)
            {
                dbContext.TodoItems.Add(
                    new TodoItem
                    {
                        Title = $"Todo {i}",
                        UserId = userId,
                        CreatedOn = DateTime.UtcNow
                    });
            }

            await dbContext.SaveChangesAsync();

            // Act
            var result = await service.GetAllTodosAsync(
                new TodoListFilterRequest
                {
                    PageNumber = 2,
                    PageSize = 5
                },
                userId);

            // Assert
            result.Data!.TodoItems.Count.Should().Be(5);
            result.Data.TotalCount.Should().Be(20);
        }

        [Fact]
        public async Task GetAllTodos_Should_Return_Only_Current_User_Todos()
        {
            // Arrange
            var user1 = Guid.NewGuid();

            var user2 = Guid.NewGuid();

            dbContext.TodoItems.AddRange(
                new TodoItem
                {
                    Title = "User1 Todo",
                    UserId = user1
                },
                new TodoItem
                {
                    Title = "User2 Todo",
                    UserId = user2
                });

            await dbContext.SaveChangesAsync();

            // Act
            var result = await service.GetAllTodosAsync(
                new TodoListFilterRequest(),
                user1);

            // Assert
            result.Data!.TodoItems.Count.Should().Be(1);
            result.Data.TodoItems.First().Title.Should().Be("User1 Todo");
        }

        [Fact]
        public async Task GetAllTodos_Should_Order_By_CreatedOn_Descending()
        {
            // Arrange
            var userId = Guid.NewGuid();

            dbContext.TodoItems.AddRange(
                new TodoItem
                {
                    Title = "Old Todo",
                    UserId = userId,
                    CreatedOn = DateTime.UtcNow.AddDays(-5)
                },
                new TodoItem
                {
                    Title = "New Todo",
                    UserId = userId,
                    CreatedOn = DateTime.UtcNow
                });

            await dbContext.SaveChangesAsync();

            // Act
            var result = await service.GetAllTodosAsync(
                new TodoListFilterRequest(),
                userId);

            // Assert
            result.Data!.TodoItems.First().Title.Should().Be("New Todo");
        }


        // Get Todo By Id
        [Fact]
        public async Task GetTodoByIdAsync_Should_Return_Todo_When_Todo_Exists()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var todoId = Guid.NewGuid();

            dbContext.TodoItems.Add(new TodoItem
            {
                Id = todoId,
                Title = "Learn JWT",
                Description = "Authentication",
                IsCompleted = false,
                CreatedOn = DateTime.UtcNow,
                UserId = userId
            });

            await dbContext.SaveChangesAsync();

            // Act
            var result = await service.GetTodoByIdAsync(todoId, userId);

            // Assert
            result.Status.Should().Be("Success");
            result.Message.Should().Be("Todo found successfully");
            result.Data.Should().NotBeNull();
            result.Data!.Id.Should().Be(todoId);
            result.Data.Title.Should().Be("Learn JWT");
            result.Data.Description.Should().Be("Authentication");
            result.Data.IsCompleted.Should().BeFalse();
        }

        [Fact]
        public async Task GetTodoByIdAsync_Should_Return_NotFound_When_Todo_Does_Not_Exist()
        {
            // Arrange
            var todoId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            // Act
            var result = await service.GetTodoByIdAsync(todoId, userId);

            // Assert
            result.Status.Should().Be("NotFound");
            result.Message.Should().Be("Todo not found");
            result.Data.Should().BeNull();
        }

        [Fact]
        public async Task GetTodoByIdAsync_Should_Return_NotFound_When_Todo_Belongs_To_Another_User()
        {
            // Arrange
            var ownerUserId = Guid.NewGuid();
            var anotherUserId = Guid.NewGuid();
            var todoId = Guid.NewGuid();

            dbContext.TodoItems.Add(new TodoItem
            {
                Id = todoId,
                Title = "Private Todo",
                UserId = ownerUserId,
                CreatedOn = DateTime.UtcNow
            });

            await dbContext.SaveChangesAsync();

            // Act
            var result = await service.GetTodoByIdAsync(
                todoId,
                anotherUserId);

            // Assert
            result.Status.Should().Be("NotFound");
            result.Message.Should().Be("Todo not found");
            result.Data.Should().BeNull();
        }


        // Update Todo
        [Fact]
        public async Task UpdateTodoAsync_Should_Update_Todo_Successfully()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var todoId = Guid.NewGuid();

            dbContext.TodoItems.Add(new TodoItem
            {
                Id = todoId,
                Title = "Old Title",
                Description = "Old Description",
                IsCompleted = false,
                UserId = userId,
                CreatedOn = DateTime.UtcNow
            });

            await dbContext.SaveChangesAsync();

            var request = new UpdateTodoRequest
            {
                Title = "New Title",
                Description = "New Description",
                IsCompleted = true
            };

            // Act
            var result = await service.UpdateTodoAsync(request, todoId, userId);

            // Assert
            result.Status.Should().Be("Success");
            result.Message.Should().Be("Todo updated successfully");

            var updatedTodo = await dbContext.TodoItems.FirstAsync();

            updatedTodo.Title.Should().Be("New Title");
            updatedTodo.Description.Should().Be("New Description");
            updatedTodo.IsCompleted.Should().BeTrue();
        }

        [Fact]
        public async Task UpdateTodoAsync_Should_Return_NotFound_When_Todo_Does_Not_Exist()
        {
            // Arrange
            var request = new UpdateTodoRequest
            {
                Title = "Updated Title",
                Description = "Updated Description",
                IsCompleted = true
            };

            // Act
            var result = await service.UpdateTodoAsync(
                request,
                Guid.NewGuid(),
                Guid.NewGuid());

            // Assert
            result.Status.Should().Be("NotFound");
            result.Message.Should().Be("Todo not found");
            result.Data.Should().BeNull();
        }

        [Fact]
        public async Task UpdateTodoAsync_Should_Return_NotFound_When_Todo_Belongs_To_Another_User()
        {
            // Arrange
            var ownerUserId = Guid.NewGuid();
            var anotherUserId = Guid.NewGuid();
            var todoId = Guid.NewGuid();

            dbContext.TodoItems.Add(new TodoItem
            {
                Id = todoId,
                Title = "Private Todo",
                UserId = ownerUserId,
                CreatedOn = DateTime.UtcNow
            });

            await dbContext.SaveChangesAsync();

            var request = new UpdateTodoRequest
            {
                Title = "Hacked Title",
                Description = "Hacked Description",
                IsCompleted = true
            };

            // Act
            var result = await service.UpdateTodoAsync(
                request,
                todoId,
                anotherUserId);

            // Assert
            result.Status.Should().Be("NotFound");

            var todo = await dbContext.TodoItems.FirstAsync();

            todo.Title.Should().Be("Private Todo");
        }

        [Fact]
        public async Task UpdateTodoAsync_Should_Set_Empty_String_When_Description_Is_Null()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var todoId = Guid.NewGuid();

            dbContext.TodoItems.Add(new TodoItem
            {
                Id = todoId,
                Title = "Old Title",
                Description = "Old Description",
                UserId = userId
            });

            await dbContext.SaveChangesAsync();

            var request = new UpdateTodoRequest
            {
                Title = "Updated Title",
                Description = null,
                IsCompleted = false
            };

            // Act
            await service.UpdateTodoAsync(request, todoId, userId);

            // Assert
            var todo = await dbContext.TodoItems.FirstAsync();

            todo.Description.Should().Be(string.Empty);
        }

        [Fact]
        public async Task UpdateTodoAsync_Should_Set_UpdatedOn()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var todoId = Guid.NewGuid();

            dbContext.TodoItems.Add(new TodoItem
            {
                Id = todoId,
                Title = "Test Todo",
                UserId = userId
            });

            await dbContext.SaveChangesAsync();

            var request = new UpdateTodoRequest
            {
                Title = "Updated Todo",
                Description = "Updated",
                IsCompleted = true
            };

            // Act
            await service.UpdateTodoAsync(request, todoId, userId);

            // Assert
            var todo = await dbContext.TodoItems.FirstAsync();
            todo.UpdatedOn.Should().BeCloseTo(
                DateTime.UtcNow,
                TimeSpan.FromSeconds(5));
        }


        // Delete Todo
        [Fact]
        public async Task DeleteTodoAsync_Should_Delete_Todo_Successfully()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var todoId = Guid.NewGuid();

            dbContext.TodoItems.Add(new TodoItem
            {
                Id = todoId,
                Title = "Learn JWT",
                UserId = userId,
                CreatedOn = DateTime.UtcNow
            });

            await dbContext.SaveChangesAsync();

            // Act
            var result = await service.DeleteTodoAsync(todoId, userId);

            // Assert
            result.Status.Should().Be("Success");
            result.Message.Should().Be("Todo deleted successfully");

            dbContext.TodoItems.Count().Should().Be(0);
        }

        [Fact]
        public async Task DeleteTodoAsync_Should_Return_NotFound_When_Todo_Does_Not_Exist()
        {
            // Arrange
            var todoId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            // Act
            var result = await service.DeleteTodoAsync(todoId, userId);

            // Assert
            result.Status.Should().Be("NotFound");
            result.Message.Should().Be("Todo not found");
            result.Data.Should().BeNull();
        }

        [Fact]
        public async Task DeleteTodoAsync_Should_Return_NotFound_When_Todo_Belongs_To_Another_User()
        {
            // Arrange
            var ownerUserId = Guid.NewGuid();
            var anotherUserId = Guid.NewGuid();

            var todoId = Guid.NewGuid();

            dbContext.TodoItems.Add(new TodoItem
            {
                Id = todoId,
                Title = "Private Todo",
                UserId = ownerUserId,
                CreatedOn = DateTime.UtcNow
            });

            await dbContext.SaveChangesAsync();

            // Act
            var result = await service.DeleteTodoAsync(
                todoId,
                anotherUserId);

            // Assert
            result.Status.Should().Be("NotFound");
            result.Message.Should().Be("Todo not found");

            dbContext.TodoItems.Count().Should().Be(1);
        }


        // Status IsCompleted
        [Fact]
        public async Task CompleteTodoAsync_Should_Mark_Todo_As_Completed()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var todoId = Guid.NewGuid();

            dbContext.TodoItems.Add(new TodoItem
            {
                Id = todoId,
                Title = "Learn JWT",
                IsCompleted = false,
                UserId = userId,
                CreatedOn = DateTime.UtcNow
            });

            await dbContext.SaveChangesAsync();

            // Act
            var result = await service.CompleteTodoAsync(
                todoId,
                true,
                userId);

            // Assert
            result.Status.Should().Be("Success");
            result.Message.Should().Be("IsCompleted  status updated successfully");
            result.Data.Should().NotBeNull();
            result.Data!.IsCompleted.Should().BeTrue();

            var todo = await dbContext.TodoItems.FirstAsync();
            todo.IsCompleted.Should().BeTrue();
        }

        [Fact]
        public async Task CompleteTodoAsync_Should_Mark_Todo_As_Pending()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var todoId = Guid.NewGuid();

            dbContext.TodoItems.Add(new TodoItem
            {
                Id = todoId,
                Title = "Learn EF Core",
                IsCompleted = true,
                UserId = userId,
                CreatedOn = DateTime.UtcNow
            });

            await dbContext.SaveChangesAsync();

            // Act
            var result = await service.CompleteTodoAsync(
                todoId,
                false,
                userId);

            // Assert
            result.Status.Should().Be("Success");
            result.Data!.IsCompleted.Should().BeFalse();

            var todo = await dbContext.TodoItems.FirstAsync();
            todo.IsCompleted.Should().BeFalse();
        }

        [Fact]
        public async Task CompleteTodoAsync_Should_Return_NotFound_When_Todo_Does_Not_Exist()
        {
            // Arrange
            var todoId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            // Act
            var result = await service.CompleteTodoAsync(
                todoId,
                true,
                userId);

            // Assert
            result.Status.Should().Be("NotFound");
            result.Message.Should().Be("Todo not found");
            result.Data.Should().BeNull();
        }

        [Fact]
        public async Task CompleteTodoAsync_Should_Return_NotFound_When_Todo_Belongs_To_Another_User()
        {
            // Arrange
            var ownerUserId = Guid.NewGuid();
            var anotherUserId = Guid.NewGuid();
            var todoId = Guid.NewGuid();

            dbContext.TodoItems.Add(new TodoItem
            {
                Id = todoId,
                Title = "Private Todo",
                IsCompleted = false,
                UserId = ownerUserId
            });

            await dbContext.SaveChangesAsync();

            // Act
            var result = await service.CompleteTodoAsync(
                todoId,
                true,
                anotherUserId);

            // Assert
            result.Status.Should().Be("NotFound");
            result.Message.Should().Be("Todo not found");

            var todo = await dbContext.TodoItems.FirstAsync();
            todo.IsCompleted.Should().BeFalse();
        }

        [Fact]
        public async Task CompleteTodoAsync_Should_Set_UpdatedOn()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var todoId = Guid.NewGuid();

            dbContext.TodoItems.Add(new TodoItem
            {
                Id = todoId,
                Title = "Learn PostgreSQL",
                UserId = userId,
                IsCompleted = false
            });

            await dbContext.SaveChangesAsync();

            // Act
            var result = await service.CompleteTodoAsync(
                todoId,
                true,
                userId);

            // Assert
            result.Status.Should().Be("Success");

            var todo = await dbContext.TodoItems.FirstAsync();
            todo.UpdatedOn.Should().BeCloseTo(
                DateTime.UtcNow,
                TimeSpan.FromSeconds(5));
        }

        #region Private Methods
        private async Task SeedTodos(Guid userId)
        {
            dbContext.TodoItems.AddRange(
                new TodoItem
                {
                    Title = "Learn JWT",
                    Description = "Authentication",
                    IsCompleted = false,
                    UserId = userId,
                    CreatedOn = DateTime.UtcNow.AddDays(-2)
                },
                new TodoItem
                {
                    Title = "Learn EF Core",
                    Description = "Database",
                    IsCompleted = true,
                    UserId = userId,
                    CreatedOn = DateTime.UtcNow.AddDays(-1)
                });

            await dbContext.SaveChangesAsync();
        }
        #endregion
    }
}
