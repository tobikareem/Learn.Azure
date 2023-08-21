using Microsoft.WindowsAzure.Storage.Table;

namespace Todo.Model;

public class TodoTableEntity : TableEntity
{
    public string TaskDescription { get; set; }
    public bool IsCompleted { get; set; }
    public DateTime CreatedOn { get; set; }
}