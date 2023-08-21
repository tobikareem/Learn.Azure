namespace Todo.Model
{
    public class Todo : TodoCreate
    {
        public string Id { get; set; } = Guid.NewGuid().ToString("n");
        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
        public bool IsCompleted { get; set; }
    }
}