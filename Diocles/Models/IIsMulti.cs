namespace Diocles.Models;

public interface IIsMulti
{
    bool IsMulti { get; }
}

public interface IIsMultiObject
{
    IIsMulti Multi { get; }
}
