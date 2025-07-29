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

List<ToDoItem> toDoItems = new List<ToDoItem>();

app.MapGet("/todoitems", () =>
{
    return Results.Ok(toDoItems);
});

app.MapPost("/todoitems", (ToDoItem item) =>
{
    toDoItems.Add(item);
    return Results.Created();
});

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
