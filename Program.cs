using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;
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

/* app.Use(async (context, next) =>
{
    Console.WriteLine(
        $"Request: {context.Request.Method} {context.Request.Path}"
    );
    await next(context);
    Console.WriteLine(
        $"Response: {context.Response.StatusCode}"
    );
}); */
var _blockedIPs = new List<string>
{
"192.168.1.1",
"203.0.113.0",
// "::1"
};

app.UseMiddleware<LoggingMiddleware>();
app.UseMiddleware<IPBlockingMiddleware>(_blockedIPs);

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

public class LoggingMiddleware
{
    private readonly RequestDelegate _next;

    public LoggingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {

        Console.WriteLine(
            $"Request: {context.Request.Method} {context.Request.Path}"
        );
        await _next(context);
        Console.WriteLine(
            $"Response: {context.Response.StatusCode}"
        );
    }
}

public class IPBlockingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly HashSet<string> _blockedIPs;
    public IPBlockingMiddleware(RequestDelegate next, IEnumerable<string> blockedIPs)
    {
        _next = next;
        _blockedIPs = new HashSet<string>(blockedIPs);
    }
    public async Task InvokeAsync(HttpContext context)
    {
        var requestIP = context.Connection.RemoteIpAddress?.ToString();
        if (requestIP != null && _blockedIPs.Contains(requestIP))
        {
            context.Response.StatusCode = 403;
            Console.WriteLine($"IP {requestIP} is blocked");
            await context.Response.WriteAsync("Your IP is blocked.");
            return;
        }
        Console.WriteLine($"IP {requestIP ?? "Unknown"} is allowed.");
        await _next(context);
    }
}

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;

    public ExceptionHandlingMiddleware(
        RequestDelegate next
    )
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception caught: {ex.Message}");
            context.Response.StatusCode = 500;
            await context.Response.WriteAsync("An unexpected error occurred");
        }
    }
}