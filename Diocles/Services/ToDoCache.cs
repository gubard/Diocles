using Avalonia.Collections;
using CommunityToolkit.Mvvm.ComponentModel;
using Diocles.Models;
using Gaia.Services;
using Hestia.Contract.Models;
using Inanna.Helpers;

namespace Diocles.Services;

public interface IToDoCache : ICache<HestiaGetResponse>, ICache<HestiaPostRequest>
{
    IEnumerable<ToDoNotify> Roots { get; }
    IEnumerable<ToDoNotify> Favorites { get; }
    IEnumerable<ToDoNotify> Bookmarks { get; }
    ToDoNotify? CurrentActive { get; }
}

public partial class ToDoCache : ObservableObject, IToDoCache
{
    private readonly Dictionary<Guid, ToDoNotify> _items = new();
    private readonly AvaloniaList<ToDoNotify> _roots = [];
    private readonly AvaloniaList<ToDoNotify> _favorites = [];
    private readonly AvaloniaList<ToDoNotify> _bookmarks = [];

    [ObservableProperty]
    public partial ToDoNotify? CurrentActive { get; set; }

    public IEnumerable<ToDoNotify> Roots => _roots;
    public IEnumerable<ToDoNotify> Favorites => _favorites;
    public IEnumerable<ToDoNotify> Bookmarks => _bookmarks;

    public void Update(HestiaGetResponse source)
    {
        var fullUpdatedIds = new HashSet<Guid>();
        var shortUpdatedIds = new HashSet<Guid>();

        if (source.CurrentActive.IsResponse)
        {
            CurrentActive = source.CurrentActive.Item is not null
                ? UpdateShortToDo(source.CurrentActive.Item, shortUpdatedIds)
                : null;
        }

        foreach (var (id, items) in source.Children)
        {
            var notify = GeItem(id);
            notify.Children.UpdateOrder(items.OrderBy(x => x.Item.OrderIndex)
               .Select(item => UpdateFullToDo(item, fullUpdatedIds,
                    shortUpdatedIds)).ToArray());
        }

        foreach (var (_, items) in source.Leafs)
        {
            foreach (var item in items)
            {
                UpdateFullToDo(item, fullUpdatedIds, shortUpdatedIds);
            }
        }

        foreach (var (id, items) in source.Parents)
        {
            var notify = GeItem(id);
            notify.Parents.UpdateOrder(items.Select(item =>
                UpdateShortToDo(item, shortUpdatedIds)).ToArray());
        }

        foreach (var item in source.Items)
        {
            UpdateFullToDo(item, fullUpdatedIds, shortUpdatedIds);
        }

        if (source.Selectors is not null)
        {
            _roots.UpdateOrder(source.Selectors
               .OrderBy(x => x.Item.OrderIndex).Select(x =>
                    UpdateToDoSelector(x, shortUpdatedIds)).ToArray());
        }

        if (source.Favorites is not null)
        {
            _favorites.UpdateOrder(source.Favorites.Select(x =>
                UpdateFullToDo(x, fullUpdatedIds, shortUpdatedIds)).ToArray());
        }

        if (source.Bookmarks is not null)
        {
            _bookmarks.UpdateOrder(source.Bookmarks.Select(x =>
                UpdateShortToDo(x, shortUpdatedIds)).ToArray());
        }

        if (source.Roots is not null)
        {
            _roots.UpdateOrder(source.Roots.OrderBy(x => x.Item.OrderIndex)
               .Select(x =>
                    UpdateFullToDo(x, fullUpdatedIds, shortUpdatedIds))
               .ToArray());
        }

        foreach (var item in source.Search)
        {
            UpdateFullToDo(item, fullUpdatedIds, shortUpdatedIds);
        }

        foreach (var item in source.Today)
        {
            UpdateFullToDo(item, fullUpdatedIds, shortUpdatedIds);
        }
    }

    private ToDoNotify UpdateToDoSelector(ToDoSelector toDo,
        HashSet<Guid> shortUpdatedIds)
    {
        var item = UpdateShortToDo(toDo.Item, shortUpdatedIds);
        item.Children.UpdateOrder(toDo.Children.OrderBy(x => x.Item.OrderIndex)
           .Select(x => UpdateShortToDo(x.Item, shortUpdatedIds)).ToArray());
        return item;
    }

    private ToDoNotify UpdateShortToDo(ShortToDo toDo, HashSet<Guid> updatedIds)
    {
        if (updatedIds.Contains(toDo.Id))
        {
            return GeItem(toDo.Id);
        }

        var item = GeItem(toDo.Id);
        item.AnnuallyDays = toDo.AnnuallyDays;
        item.MonthlyDays = toDo.MonthlyDays;
        item.WeeklyDays = toDo.WeeklyDays;
        item.Name = toDo.Name;
        item.OrderIndex = toDo.OrderIndex;
        item.Description = toDo.Description;
        item.Type = toDo.Type;
        item.IsBookmark = toDo.IsBookmark;
        item.IsFavorite = toDo.IsFavorite;
        item.DueDate = toDo.DueDate;
        item.TypeOfPeriodicity = toDo.TypeOfPeriodicity;
        item.DaysOffset = toDo.DaysOffset;
        item.MonthsOffset = toDo.MonthsOffset;
        item.WeeksOffset = toDo.WeeksOffset;
        item.YearsOffset = toDo.YearsOffset;
        item.ChildrenCompletionType = toDo.ChildrenCompletionType;
        item.Link = toDo.Link;
        item.IsRequiredCompleteInDueDate =
            toDo.IsRequiredCompleteInDueDate;
        item.DescriptionType = toDo.DescriptionType;
        item.Icon = toDo.Icon;
        item.Color = toDo.Color;
        item.RemindDaysBefore = toDo.RemindDaysBefore;
        item.ReferenceId = toDo.ReferenceId.HasValue
            ? GeItem(toDo.ReferenceId.Value)
            : null;
        item.ParentId = toDo.ParentId.HasValue
            ? GeItem(toDo.ParentId.Value)
            : null;
        updatedIds.Add(item.Id);

        return item;
    }

    private ToDoNotify UpdateFullToDo(FullToDo toDo,
        HashSet<Guid> fullUpdatedIds, HashSet<Guid> shortUpdatedIds)
    {
        if (fullUpdatedIds.Contains(toDo.Item.Id))
        {
            return GeItem(toDo.Item.Id);
        }

        var item = UpdateShortToDo(toDo.Item, shortUpdatedIds);
        item.Active = toDo.Active is not null
            ? UpdateShortToDo(toDo.Active, shortUpdatedIds)
            : null;
        item.IsCan = toDo.IsCan;
        item.Status = toDo.Status;
        fullUpdatedIds.Add(item.Id);

        return item;
    }

    private ToDoNotify GeItem(Guid id)
    {
        if (_items.TryGetValue(id, out var value))
        {
            return value;
        }

        var result = new ToDoNotify(id);

        if (_items.TryAdd(id, result))
        {
            return result;
        }

        return _items[id];
    }

    public void Update(HestiaPostRequest source)
    {
        throw new NotImplementedException();
    }
}