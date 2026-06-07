namespace TodoApiProject.Data.Entities
{
    public class TodoItem
    {
        public Guid Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public bool IsCompleted { get; set; } = false;

        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedOn { get; set; }

        public Guid UserId { get; set; }

        public UserEntity User { get; set; } = null!;
    }
}
