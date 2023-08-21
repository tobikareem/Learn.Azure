namespace Todo.Model;

public interface ITodoGeneral
{
    public string TaskDescription { get; set; }
    public bool IsCompleted { get; set; }
}