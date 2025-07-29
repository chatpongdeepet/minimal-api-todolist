using System.ComponentModel.DataAnnotations;
using minimal_api_todolist.Models;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.Use(async (context, next) =>
{
    Console.WriteLine(
        $"Request: {context.Request.Method} {context.Request.Path}"
    );
    await next(context);
    Console.WriteLine(
        $"Response: {context.Response.StatusCode}"
    );
});

List<ToDoItem> toDoItems = new List<ToDoItem>();

app.MapGet("/todoitems", () =>
{
    return Results.Ok(toDoItems);
});

app.MapPost("/todoitems", (ToDoItem item) =>
{
    var validationResults = new List<ValidationResult>();
    var validationContext = new ValidationContext(item);
    bool isValid = Validator.TryValidateObject(item, validationContext, validationResults, true);
    if (!isValid)
    {
        return Results.BadRequest(validationResults);
    }
    toDoItems.Add(item);
    return Results.Created();
}).AddEndpointFilter<CreateTodoFilter>();

app.MapPut("/todotiems", (ToDoItem item) =>
{
    var index = toDoItems.FindIndex(x => x.Id == item.Id);
    if (index == -1)
    {
        return Results.NotFound();
    }
    toDoItems[index] = item;
    return Results.NoContent();
});

app.MapPatch("/updateTodoItemDueDate/{id}", (int id, DateTime newDueDate) =>
{
    var index = toDoItems.FindIndex(x => x.Id == id);
    if (index == -1)
    {
        return Results.NotFound();
    }
    toDoItems[index].DueDate = newDueDate;
    return Results.NoContent();
});

app.MapGet("/todoitems/{id}", (int id) =>
{
    var index = toDoItems.FindIndex(x => x.Id == id);
    if (index == -1)
    {
        return Results.NotFound();
    }
    return Results.Ok(toDoItems[index]);
});

app.MapDelete("/todoitems/{id:int:range(1,1000)}", (int id) =>
{
    var index = toDoItems.FindIndex(x => x.Id == id);
    if (index == -1)
    {
        return Results.NotFound();
    }
    toDoItems.RemoveAt(index);
    return Results.NoContent();
});

app.Run();

public class CreateTodoFilter : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var todoitem = context.GetArgument<ToDoItem>(0);
        if (todoitem.Assignee == "Joe Bloggs")
        {
            return Results.Problem("Joe Bloggs cannot be assigned a todoitem");
        }
        // Call the next filter/middleware in the pipeline
        return await next(context);
    }
}