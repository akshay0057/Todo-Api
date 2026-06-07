namespace TodoApiProject.Models.RequestModels.Todo
{
    public class TodoListFilterRequest : PaginationRequest
    {
        public bool? IsCompleted { get; set; }
    }
}
