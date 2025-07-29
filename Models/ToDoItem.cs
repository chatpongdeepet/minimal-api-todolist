using System;

namespace minimal_api_todolist.Models;

public class ToDoItem
{
    public int Id { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime DueDate { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public string Assignee { get; set; }
    public int Priority { get; set; }
    public bool IsComplete { get; set; }

}
