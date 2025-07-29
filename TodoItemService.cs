using System;
using minimal_api_todolist.Models;

namespace minimal_api_todolist;

public class TodoItemService
{
    List<ToDoItem> todoItems = new List<ToDoItem>();
    public ToDoItem GetById(int id)
    {
        return todoItems.FirstOrDefault(x => x.Id == id);
    }
    public List<ToDoItem> GetToDoItems(bool pastDue, int priority)
    {
        var todoItemsQuery = todoItems.AsQueryable();

        if (pastDue)
        {
            todoItemsQuery = todoItemsQuery.Where(
                x => x.DueDate <= DateTime.Now
            );
        }
        if (priority > 0)
        {
            todoItemsQuery = todoItemsQuery.Where();
        }

        return todoItemsQuery.ToList();
    }
}
