namespace TodoApiProject.Models.ResponseModels.Todo
{
    public class TodoListResponse
    {
        public int TotalCount { get; set; }
        public List<TodoItemResponse> TodoItems { get; set; } = [];
    }
}
