using TodoApiProject.Models.RequestModels;
using TodoApiProject.Models.RequestModels.Todo;
using TodoApiProject.Models.ResponseModels;
using TodoApiProject.Models.ResponseModels.Todo;

namespace TodoApiProject.Services.Interfaces
{
    public interface ITodoService
    {
        Task<CommonResponse<CreateTodoResponse>> CreateTodoAsync(CreateTodoRequest request, Guid userId);
        Task<CommonResponse<TodoListResponse>> GetAllTodosAsync(TodoListFilterRequest request, Guid userId);
        Task<CommonResponse<TodoItemResponse>> GetTodoByIdAsync(Guid id, Guid userId);
        Task<CommonResponse<string>> UpdateTodoAsync(UpdateTodoRequest request, Guid id, Guid userId);
        Task<CommonResponse<string>> DeleteTodoAsync(Guid id, Guid userId);
        Task<CommonResponse<TodoItemResponse>> CompleteTodoAsync(Guid id, bool isCompleted, Guid userId);
    }
}
