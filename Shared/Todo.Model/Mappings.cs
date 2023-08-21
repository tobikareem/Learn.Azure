namespace Todo.Model;

public static class Mappings
{
    public static TodoTableEntity ToTableEntity(this Todo todo)
    {
        return new TodoTableEntity
        {
            TaskDescription = todo.TaskDescription,
            CreatedOn = todo.CreatedOn,
            IsCompleted = todo.IsCompleted,
            RowKey = todo.Id,
            PartitionKey = "TODO"
        };
    }

    public static Todo ToTodo(this TodoTableEntity todo)
    {
        return new Todo
        {
            Id = todo.RowKey,
            CreatedOn = todo.CreatedOn,
            IsCompleted = todo.IsCompleted,
            TaskDescription = todo.TaskDescription
        };
    }
}