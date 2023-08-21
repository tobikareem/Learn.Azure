using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;
using Todo.Model;

namespace FunctionAppTest1
{
    public static class TodoApi
    {
        private static readonly List<Todo.Model.Todo> TodoItems = new();

        [FunctionName("CreateTodo")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "todo")] HttpRequest req,
            [Table("todos", Connection = "AzureWebJobsStorage")] IAsyncCollector<TodoTableEntity> todoTable,
            ILogger log)
        {
            log.LogInformation("Creating a new todo list item.");

            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var data = JsonConvert.DeserializeObject<TodoCreate>(requestBody);

            var todo = new Todo.Model.Todo
            {
                TaskDescription = data?.TaskDescription ?? string.Empty
            };

            await todoTable.AddAsync(todo.ToTableEntity());

            return new OkObjectResult(todo);
        }

        [FunctionName("GetTodos")]
        public static async Task<IActionResult> GetTodos(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "todo")] HttpRequest req,
            [Table("todos", Connection = "AzureWebJobsStorage")] CloudTable todoTable,
            ILogger log)
        {
            var query = new TableQuery<TodoTableEntity>();
            var segment = await todoTable.ExecuteQuerySegmentedAsync(query, null);

            var items = segment.Select(Mappings.ToTodo);

            log.LogInformation("Getting the count of Todo: {items}", items.Count());
            return new OkObjectResult(items);
        }


        [FunctionName("GetTodoById")]
        public static IActionResult GetTodoById(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "todo/{id}")] HttpRequest req,
            [Table("todos", "TODO", "{id}", Connection = "AzureWebJobsStorage")] TodoTableEntity todoTable,
            ILogger log, string id)
        {
            log.LogInformation("The item exists in the list of Todo: {items}", todoTable.ToTodo() == default(Todo.Model.Todo));
            return todoTable == null ? new NotFoundObjectResult(TodoItems) : new OkObjectResult(todoTable.ToTodo());
        }

        [FunctionName("UpdateTodo")]
        public static async Task<IActionResult> UpdateTodoById(
            [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "todo/{id}")] HttpRequest req,
            [Table("todos", Connection = "AzureWebJobsStorage")] CloudTable todoTable,
            ILogger log, string id)
        {

            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var data = JsonConvert.DeserializeObject<TodoUpdate>(requestBody);

            var findOperation = TableOperation.Retrieve<TodoTableEntity>("TODO", id);
            var findResult = await todoTable.ExecuteAsync(findOperation);

            if (findResult.Result == null)
            {
                return new NotFoundObjectResult(data);
            }

            var existingRow = (TodoTableEntity)findResult.Result;
            existingRow.IsCompleted = data?.IsCompleted ?? false;
            existingRow.TaskDescription = string.IsNullOrWhiteSpace(data?.TaskDescription) ? existingRow.TaskDescription : data.TaskDescription;

            var replaceOperation = TableOperation.Replace(existingRow);

            await todoTable.ExecuteAsync(replaceOperation);

            log.LogInformation("The item exists is updated: {items}", existingRow.IsCompleted);
            return new OkObjectResult(existingRow.ToTodo());
        }

        [FunctionName("DeleteTodos")]
        public static async Task<IActionResult> DeleteTodos(
            [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "todo/{id}")] HttpRequest req,
            [Table("todos", Connection = "AzureWebJobsStorage")] CloudTable todoTable,
            ILogger log, string id)
        {
            var deleteOperation = TableOperation.Delete(new TableEntity()
            {
                PartitionKey = "TODO",
                RowKey = id,
                ETag = "*"
            });

            try
            {
                var deleteResult = await todoTable.ExecuteAsync(deleteOperation);

                log.LogInformation("Data was deleted. {isDeleted}", deleteResult);
            }
            catch (StorageException e) when (e.RequestInformation.HttpStatusCode == 404)
            {
                return new NotFoundResult();
            }

            return new OkResult();
        }
    }

}
