using System.Runtime.CompilerServices;
using Avalonia.Collections;
using Avalonia.Media;
using Avalonia.Threading;
using Diocles.Helpers;
using Diocles.Models;
using Gaia.Helpers;
using Gaia.Services;
using Hestia.Contract.Models;
using Hestia.Contract.Services;
using IconPacks.Avalonia.MaterialDesign;
using Inanna.Helpers;
using Inanna.Models;
using Inanna.Services;

namespace Diocles.Services;

public interface IToDoMemoryCache : IMemoryCache<HestiaPostRequest, HestiaGetResponse>
{
    IAvaloniaReadOnlyList<ToDoNotify> Roots { get; }
    IAvaloniaReadOnlyList<ToDoNotify> Favorites { get; }
    IAvaloniaReadOnlyList<ToDoNotify> Bookmarks { get; }
    IAvaloniaReadOnlyList<ToDoNotify> Search { get; }
    ToDoNotify? CurrentActive { get; }
    void ResetItems();
}

public interface IToDoUiCache : IUiCache<HestiaPostRequest, HestiaGetResponse, IToDoMemoryCache>
{
    IAvaloniaReadOnlyList<ToDoNotify> Roots { get; }
    IAvaloniaReadOnlyList<ToDoNotify> Favorites { get; }
    IAvaloniaReadOnlyList<ToDoNotify> Bookmarks { get; }
    IAvaloniaReadOnlyList<ToDoNotify> Search { get; }
    ToDoNotify? CurrentActive { get; }
    void ResetItems();
}

public sealed class ToDoUiCache
    : UiCache<HestiaPostRequest, HestiaGetResponse, IToDoDbCache, IToDoMemoryCache>,
        IToDoUiCache
{
    public ToDoUiCache(IToDoDbCache dbCache, IToDoMemoryCache memoryCache)
        : base(dbCache, memoryCache)
    {
        RootCommands = DiocleHelper.CreateMultiCommands(Roots);
    }

    public IAvaloniaReadOnlyList<ToDoNotify> Roots => MemoryCache.Roots;
    public IAvaloniaReadOnlyList<InannaCommand> RootCommands { get; }
    public IAvaloniaReadOnlyList<ToDoNotify> Favorites => MemoryCache.Favorites;
    public IAvaloniaReadOnlyList<ToDoNotify> Bookmarks => MemoryCache.Bookmarks;
    public IAvaloniaReadOnlyList<ToDoNotify> Search => MemoryCache.Search;
    public ToDoNotify? CurrentActive => MemoryCache.CurrentActive;

    public void ResetItems()
    {
        MemoryCache.ResetItems();
    }
}

public sealed class ToDoMemoryCache
    : MemoryCache<ToDoNotify, HestiaPostRequest, HestiaGetResponse>,
        IToDoMemoryCache
{
    public ToDoNotify? CurrentActive { get; private set; }
    public IAvaloniaReadOnlyList<ToDoNotify> Roots => _roots;
    public IAvaloniaReadOnlyList<ToDoNotify> Favorites => _favorites;
    public IAvaloniaReadOnlyList<ToDoNotify> Bookmarks => _bookmarks;
    public IAvaloniaReadOnlyList<ToDoNotify> Search => _search;

    public void ResetItems()
    {
        foreach (var (_, item) in Items)
        {
            item.IsSelected = false;
            item.IsChangingOrder = false;
            item.IsHideOnTree = false;
        }
    }

    public override ConfiguredValueTaskAwaitable UpdateAsync(
        HestiaGetResponse source,
        CancellationToken ct
    )
    {
        Update(source);

        return TaskHelper.ConfiguredCompletedTask;
    }

    public override ConfiguredValueTaskAwaitable UpdateAsync(
        HestiaPostRequest source,
        CancellationToken ct
    )
    {
        Update(source);

        return TaskHelper.ConfiguredCompletedTask;
    }

    private readonly AvaloniaList<ToDoNotify> _roots = [];
    private readonly AvaloniaList<ToDoNotify> _favorites = [];
    private readonly AvaloniaList<ToDoNotify> _bookmarks = [];
    private readonly AvaloniaList<ToDoNotify> _search = [];

    private void Update(HestiaGetResponse source)
    {
        var shortUpdatedIds = new HashSet<Guid>();

        if (source.CurrentActive.HasResponse)
        {
            Dispatcher.UIThread.Invoke(() =>
                CurrentActive = source.CurrentActive.Item is not null
                    ? UpdateShortToDo(source.CurrentActive.Item, shortUpdatedIds)
                    : null
            );
        }

        Dispatcher.UIThread.Post(() =>
        {
            var fullUpdatedIds = new HashSet<Guid>();

            foreach (var (id, items) in source.Children)
            {
                var notify = GetItem(id);

                notify.UpdateChildren(
                    items
                        .OrderBy(x => x.Item.OrderIndex)
                        .Select(item => UpdateFullToDo(item, fullUpdatedIds, shortUpdatedIds))
                        .ToArray()
                );
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
                var notify = GetItem(id);

                notify.UpdateParents(
                    items.Select(item => UpdateShortToDo(item, shortUpdatedIds)).ToArray()
                );
            }

            foreach (var item in source.Items)
            {
                UpdateFullToDo(item, fullUpdatedIds, shortUpdatedIds);
            }

            if (source.Selectors is not null)
            {
                _roots.UpdateOrder(
                    source
                        .Selectors.OrderBy(x => x.Item.OrderIndex)
                        .Select(x => UpdateToDoSelector(x, shortUpdatedIds))
                        .ToArray()
                );
            }

            if (source.Favorites is not null)
            {
                _favorites.UpdateOrder(
                    source
                        .Favorites.Select(x => UpdateFullToDo(x, fullUpdatedIds, shortUpdatedIds))
                        .ToArray()
                );
            }

            if (source.Bookmarks is not null)
            {
                _bookmarks.UpdateOrder(
                    source
                        .Bookmarks.OrderBy(x => x.Name)
                        .Select(x => UpdateShortToDo(x, shortUpdatedIds))
                        .ToArray()
                );
            }

            if (source.Roots is not null)
            {
                _roots.UpdateOrder(
                    source
                        .Roots.OrderBy(x => x.Item.OrderIndex)
                        .Select(x => UpdateFullToDo(x, fullUpdatedIds, shortUpdatedIds))
                        .ToArray()
                );
            }

            _search.UpdateOrder(
                source
                    .Search.OrderBy(x => x.Item.Name)
                    .ThenBy(x => x.Item.OrderIndex)
                    .Select(x => UpdateFullToDo(x, fullUpdatedIds, shortUpdatedIds))
                    .ToArray()
            );

            foreach (var item in source.Today)
            {
                UpdateFullToDo(item, fullUpdatedIds, shortUpdatedIds);
            }
        });
    }

    private void SetupDueToDo(ToDoNotify item)
    {
        var now = DateTime.Now.ToDateOnly();

        if (item.DueDate == now)
        {
            item.IsCanDo = ToDoIsCanDo.CanComplete;
            item.Status = ToDoStatus.ReadyForComplete;
        }
        else if (item.DueDate > now)
        {
            item.IsCanDo = ToDoIsCanDo.None;
            item.Status = ToDoStatus.Planned;
        }
        else
        {
            item.IsCanDo = ToDoIsCanDo.CanComplete;
            item.Status = ToDoStatus.Miss;
        }
    }

    private void SetupCreatedToDo(ShortToDo todo, HashSet<Guid> shortUpdatedIds)
    {
        var item = UpdateShortToDo(todo, shortUpdatedIds);

        item.OrderIndex = item.Parent is null
            ? (uint)_roots.Count + 1
            : (uint)item.Parent.Children.Count + 1;

        switch (item.Type)
        {
            case ToDoType.Value:
                item.IsCanDo = ToDoIsCanDo.CanComplete;
                item.Status = ToDoStatus.ReadyForComplete;

                break;
            case ToDoType.Step:
                item.IsCanDo = ToDoIsCanDo.CanComplete;
                item.Status = ToDoStatus.ReadyForComplete;

                break;
            case ToDoType.Circle:
                item.IsCanDo = ToDoIsCanDo.CanComplete;
                item.Status = ToDoStatus.ReadyForComplete;

                break;
            case ToDoType.Group:
                item.IsCanDo = ToDoIsCanDo.None;
                item.Status = ToDoStatus.Completed;

                break;
            case ToDoType.FixedDate:
                SetupDueToDo(item);

                break;
            case ToDoType.Periodicity:
                SetupDueToDo(item);

                break;
            case ToDoType.PeriodicityOffset:
                SetupDueToDo(item);

                break;
            case ToDoType.Reference:
                if (item.Reference is null)
                {
                    break;
                }

                item.IsCanDo = item.Reference.IsCanDo;
                item.Status = item.Reference.Status;

                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void Update(HestiaPostRequest source)
    {
        Dispatcher.UIThread.Post(() =>
        {
            var shortUpdatedIds = new HashSet<Guid>();

            foreach (var create in source.Creates)
            {
                SetupCreatedToDo(create, shortUpdatedIds);
            }

            foreach (var edit in source.Edits)
            {
                var items = edit.Ids.Select(GetItem).ToArray();

                if (edit.IsEditName)
                {
                    foreach (var item in items)
                    {
                        item.Name = edit.Name;
                    }
                }

                if (edit.IsEditDescription)
                {
                    foreach (var item in items)
                    {
                        item.Description = edit.Description;
                    }
                }

                if (edit.IsEditType)
                {
                    foreach (var item in items)
                    {
                        item.Type = edit.Type;
                    }
                }

                if (edit.IsEditIsBookmark)
                {
                    foreach (var item in items)
                    {
                        item.IsBookmark = edit.IsBookmark;
                    }
                }

                if (edit.IsEditIsFavorite)
                {
                    foreach (var item in items)
                    {
                        item.IsFavorite = edit.IsFavorite;
                    }
                }

                if (edit.IsEditDueDate)
                {
                    foreach (var item in items)
                    {
                        item.DueDate = edit.DueDate;
                    }
                }

                if (edit.IsEditTypeOfPeriodicity)
                {
                    foreach (var item in items)
                    {
                        item.TypeOfPeriodicity = edit.TypeOfPeriodicity;
                    }
                }

                if (edit.IsEditAnnuallyDays)
                {
                    var days = edit.AnnuallyDays.ToArray();

                    foreach (var item in items)
                    {
                        item.UpdateAnnualDays(days);
                    }
                }

                if (edit.IsEditMonthlyDays)
                {
                    var days = edit.MonthlyDays.Select(x => (int)x).ToArray();

                    foreach (var item in items)
                    {
                        item.UpdateMonthlyDays(days);
                    }
                }

                if (edit.IsEditWeeklyDays)
                {
                    var days = edit.WeeklyDays.ToArray();

                    foreach (var item in items)
                    {
                        item.UpdateWeeklyDays(days);
                    }
                }

                if (edit.IsEditDaysOffset)
                {
                    foreach (var item in items)
                    {
                        item.DaysOffset = edit.DaysOffset;
                    }
                }

                if (edit.IsEditMonthsOffset)
                {
                    foreach (var item in items)
                    {
                        item.MonthsOffset = edit.MonthsOffset;
                    }
                }

                if (edit.IsEditWeeksOffset)
                {
                    foreach (var item in items)
                    {
                        item.WeeksOffset = edit.WeeksOffset;
                    }
                }

                if (edit.IsEditYearsOffset)
                {
                    foreach (var item in items)
                    {
                        item.YearsOffset = edit.YearsOffset;
                    }
                }

                if (edit.IsEditChildrenCompletionType)
                {
                    foreach (var item in items)
                    {
                        item.ChildrenCompletionType = edit.ChildrenCompletionType;
                    }
                }

                if (edit.IsEditLink)
                {
                    foreach (var item in items)
                    {
                        item.Link = edit.Link;
                    }
                }

                if (edit.IsEditIsRequiredCompleteInDueDate)
                {
                    foreach (var item in items)
                    {
                        item.IsRequiredCompleteInDueDate = edit.IsRequiredCompleteInDueDate;
                    }
                }

                if (edit.IsEditDescriptionType)
                {
                    foreach (var item in items)
                    {
                        item.DescriptionType = edit.DescriptionType;
                    }
                }

                if (edit.IsEditIcon)
                {
                    foreach (var item in items)
                    {
                        item.Icon = Enum.TryParse<PackIconMaterialDesignKind>(
                            edit.Icon,
                            out var icon
                        )
                            ? icon
                            : PackIconMaterialDesignKind.None;
                    }
                }

                if (edit.IsEditColor)
                {
                    foreach (var item in items)
                    {
                        item.Color = Color.Parse(edit.Color);
                    }
                }

                if (edit.IsEditRemindDaysBefore)
                {
                    foreach (var item in items)
                    {
                        item.RemindDaysBefore = edit.RemindDaysBefore;
                    }
                }

                if (edit.IsEditReferenceId)
                {
                    var reference = edit.ReferenceId.HasValue
                        ? GetItem(edit.ReferenceId.Value)
                        : null;

                    foreach (var item in items)
                    {
                        item.Reference = reference;
                    }
                }

                if (edit.IsEditParentId)
                {
                    foreach (var item in items)
                    {
                        ChangeParent(item, edit.ParentId);
                    }
                }
            }

            foreach (var changeOrder in source.ChangeOrders)
            {
                var item = GetItem(changeOrder.StartId);

                var siblings = item.Parent is not null
                    ? (AvaloniaList<ToDoNotify>)item.Children
                    : _roots;

                var index = siblings.IndexOf(item);

                if (index == -1)
                {
                    continue;
                }

                var insertItems = changeOrder.InsertIds.Select(GetItem);

                foreach (var insertItem in insertItems)
                {
                    siblings.Remove(insertItem);
                    siblings.Insert(index, insertItem);
                }
            }

            foreach (var deleteId in source.DeleteIds)
            {
                var deleteItem = GetItem(deleteId);
                Items.Remove(deleteId);

                if (deleteItem.Parent is not null)
                {
                    deleteItem.Parent.RemoveChild(deleteItem);
                }
                else
                {
                    _roots.Remove(deleteItem);
                }
            }

            foreach (var id in source.SwitchCompleteIds)
            {
                var item = GetItem(id);

                switch (item.IsCanDo)
                {
                    case ToDoIsCanDo.None:
                        break;
                    case ToDoIsCanDo.CanComplete:
                        item.Status = ToDoStatus.Completed;
                        break;
                    case ToDoIsCanDo.CanIncomplete:
                        item.Status = ToDoStatus.ReadyForComplete;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(
                            nameof(item.IsCanDo),
                            item.IsCanDo,
                            null
                        );
                }
            }
        });
    }

    private ToDoNotify UpdateToDoSelector(ToDoSelector toDo, HashSet<Guid> shortUpdatedIds)
    {
        var item = UpdateShortToDo(toDo.Item, shortUpdatedIds);

        item.UpdateChildren(
            toDo.Children.OrderBy(x => x.Item.OrderIndex)
                .Select(x => UpdateToDoSelector(x, shortUpdatedIds))
                .ToArray()
        );

        return item;
    }

    private ToDoNotify UpdateShortToDo(ShortToDo toDo, HashSet<Guid> updatedIds)
    {
        if (updatedIds.Contains(toDo.Id))
        {
            return GetItem(toDo.Id);
        }

        var item = GetItem(toDo.Id);
        item.UpdateAnnualDays(toDo.AnnuallyDays.ToArray());
        item.UpdateMonthlyDays(toDo.MonthlyDays.ToArray());
        item.UpdateWeeklyDays(toDo.WeeklyDays.ToArray());
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
        item.IsRequiredCompleteInDueDate = toDo.IsRequiredCompleteInDueDate;
        item.DescriptionType = toDo.DescriptionType;
        item.Color = Color.TryParse(toDo.Color, out var color) ? color : Colors.Transparent;
        item.RemindDaysBefore = toDo.RemindDaysBefore;
        item.Reference = toDo.ReferenceId.HasValue ? GetItem(toDo.ReferenceId.Value) : null;

        item.Icon = Enum.TryParse<PackIconMaterialDesignKind>(toDo.Icon, out var icon)
            ? icon
            : PackIconMaterialDesignKind.None;

        if (item.Parent?.Id != toDo.ParentId)
        {
            ChangeParent(item, toDo.ParentId);
        }

        updatedIds.Add(item.Id);

        return item;
    }

    private ToDoNotify UpdateFullToDo(
        FullToDo toDo,
        HashSet<Guid> fullUpdatedIds,
        HashSet<Guid> shortUpdatedIds
    )
    {
        if (fullUpdatedIds.Contains(toDo.Item.Id))
        {
            return GetItem(toDo.Item.Id);
        }

        var item = UpdateShortToDo(toDo.Item, shortUpdatedIds);

        item.Active = toDo.Active is not null
            ? UpdateShortToDo(toDo.Active, shortUpdatedIds)
            : null;

        item.IsCanDo = toDo.IsCanDo;
        item.Status = toDo.Status;
        fullUpdatedIds.Add(item.Id);

        return item;
    }

    private void ChangeParent(ToDoNotify item, Guid? newParentId)
    {
        if (newParentId == item.Parent?.Id)
        {
            return;
        }

        if (item.Parent is not null)
        {
            item.Parent.RemoveChild(item);
        }
        else
        {
            _roots.Remove(item);
        }

        item.Parent = newParentId.HasValue ? GetItem(newParentId.Value) : null;

        if (item.Parent is not null)
        {
            item.Parent.AddChild(item);
        }
        else
        {
            _roots.Add(item);
        }
    }
}
