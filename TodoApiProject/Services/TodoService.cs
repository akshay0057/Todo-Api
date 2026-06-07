using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TodoApiProject.Data;
using TodoApiProject.Data.Entities;
using TodoApiProject.Models.RequestModels;
using TodoApiProject.Models.RequestModels.Todo;
using TodoApiProject.Models.ResponseModels;
using TodoApiProject.Models.ResponseModels.Todo;
using TodoApiProject.Services.Interfaces;

namespace TodoApiProject.Services
{
    public class TodoService : ITodoService
    {
        private readonly ApplicationDbContext _context;

        public TodoService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<CommonResponse<CreateTodoResponse>> CreateTodoAsync(CreateTodoRequest request, Guid userId)
        {
            var todo = new TodoItem
            {
                Title = request.Title,
                Description = request.Description ?? string.Empty,
                IsCompleted = false,
                CreatedOn = DateTime.UtcNow,
                UserId = userId
            };

            _context.TodoItems.Add(todo);

            await _context.SaveChangesAsync();

            return CommonResponse<CreateTodoResponse>.Success("Todo created successfully",
                new CreateTodoResponse
                {
                    Id = todo.Id,
                    Title = todo.Title,
                    Description = todo.Description ?? string.Empty,
                    IsCompleted = todo.IsCompleted
                });
        }

        public async Task<CommonResponse<TodoListResponse>> GetAllTodosAsync(TodoListFilterRequest request, Guid userId)
        {
            var todos = await _context.TodoItems
                .Where(x => x.UserId == userId)
                .OrderByDescending(x => x.CreatedOn)
                .ToListAsync();

            if (todos.Count == 0)
            {
                return CommonResponse<TodoListResponse>.NotFound("No todos found");
            }

            if(request.IsCompleted.HasValue)
            {
                if(request.IsCompleted.Value == true)
                    todos = todos.Where(x => x.IsCompleted).ToList();
                else
                    todos = todos.Where(x => !x.IsCompleted).ToList();
            }

            if (!string.IsNullOrEmpty(request.SearchText))
            {
                todos = todos.Where(x =>
                x.Title.ToLower().Contains(request.SearchText.ToLower().Trim()) ||
                (x.Description ?? string.Empty).ToLower().Contains(request.SearchText.ToLower().Trim())).ToList();
            }

            var todoItems = todos
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(x => new TodoItemResponse
                {
                    Id = x.Id,
                    Title = x.Title,
                    Description = x.Description ?? string.Empty,
                    IsCompleted = x.IsCompleted,
                    CreatedOn = x.CreatedOn
                }).ToList();

            return CommonResponse<TodoListResponse>.Success("Todos fetched successfully",
                new TodoListResponse
                {
                    TodoItems = todoItems,
                    TotalCount = todos.Count
                });
        }

        public async Task<CommonResponse<TodoItemResponse>> GetTodoByIdAsync(Guid id, Guid userId)
        {
            var todo = await _context.TodoItems.FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId);

            if (todo == null)
                return CommonResponse<TodoItemResponse>.NotFound("Todo not found");

            return CommonResponse<TodoItemResponse>.Success("Todo found successfully",
                new TodoItemResponse
                {
                    Id = todo.Id,
                    Title = todo.Title,
                    Description = todo.Description ?? string.Empty,
                    IsCompleted = todo.IsCompleted,
                    CreatedOn = todo.CreatedOn
                });
        }

        public async Task<CommonResponse<string>> UpdateTodoAsync(UpdateTodoRequest request, Guid id, Guid userId)
        {
            var todo = await _context.TodoItems.FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId);

            if (todo == null)
                return CommonResponse<string>.NotFound("Todo not found");

            todo.Title = request.Title;
            todo.Description = request.Description ?? string.Empty;
            todo.IsCompleted = request.IsCompleted;
            todo.UpdatedOn = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return CommonResponse<string>.Success("Todo updated successfully", string.Empty);
        }

        public async Task<CommonResponse<string>> DeleteTodoAsync(Guid id, Guid userId)
        {
            var todo = await _context.TodoItems.FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId);

            if (todo == null)
                return CommonResponse<string>.NotFound("Todo not found");

            _context.TodoItems.Remove(todo);

            await _context.SaveChangesAsync();

            return CommonResponse<string>.Success("Todo deleted successfully", string.Empty);
        }

        public async Task<CommonResponse<TodoItemResponse>> CompleteTodoAsync(Guid id, bool isCompleted, Guid userId)
        {
            var todo = await _context.TodoItems.FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId);

            if (todo == null)
                return CommonResponse<TodoItemResponse>.NotFound("Todo not found");

            todo.IsCompleted = isCompleted;
            todo.UpdatedOn = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return CommonResponse<TodoItemResponse>.Success("IsCompleted  status updated successfully",
                new TodoItemResponse
                {
                    Id = todo.Id,
                    Title = todo.Title,
                    Description = todo.Description ?? string.Empty,
                    IsCompleted = todo.IsCompleted,
                    CreatedOn = todo.CreatedOn,
                    LastUpdatedOn = todo.UpdatedOn
                });
        }
    }
}
