using System.ComponentModel.DataAnnotations;

namespace TodoApiProject.Models.RequestModels.Todo
{
    public class CreateTodoRequest
    {
        [Required]
        [MaxLength(100)]
        public string Title { get; set; } = null!;

        [MaxLength(500)]
        public string? Description { get; set; }
    }
}
