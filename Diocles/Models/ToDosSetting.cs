namespace Diocles.Models;

public class ToDosSetting
{
    public ToDoGroupBy GroupBy { get; set; } = ToDoGroupBy.Status;
    public ToDoOrderBy OrderBy { get; set; }
}
