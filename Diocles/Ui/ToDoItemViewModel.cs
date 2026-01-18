using System.ComponentModel;
using System.Runtime.CompilerServices;
using Avalonia.Collections;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.Input;
using Diocles.Helpers;
using Diocles.Models;
using Diocles.Services;
using Gaia.Services;
using Hestia.Contract.Models;
using IconPacks.Avalonia.MaterialDesign;
using Inanna.Helpers;
using Inanna.Models;
using Inanna.Services;
using Inanna.Ui;

namespace Diocles.Ui;

public partial class ToDoItemViewModel : ToDosMainViewModelBase, IHeader, ISaveUi, IInitUi
{
    public ToDoItemViewModel(
        ToDoNotify item,
        IToDoUiService toDoUiService,
        IToDoUiCache toDoUiCache,
        IStringFormater stringFormater,
        IDialogService dialogService,
        IAppResourceService appResourceService,
        IDioclesViewModelFactory factory,
        IObjectStorage objectStorage
    )
        : base(
            dialogService,
            appResourceService,
            stringFormater,
            factory,
            toDoUiService,
            toDoUiCache,
            item.Children
        )
    {
        Item = item;

        _header = factory.CreateToDosHeader(
            item.Name,
            new AvaloniaList<InannaCommand>()
            {
                new(
                    ShowEditCommand,
                    item,
                    appResourceService.GetResource<string>("Lang.Edit"),
                    PackIconMaterialDesignKind.Edit
                ),
            },
            DiocleHelper.CreateMultiCommands(item.Children)
        );

        _objectStorage = objectStorage;
    }

    public object Header => _header;
    public ToDoNotify Item { get; }

    public ConfiguredValueTaskAwaitable SaveUiAsync(CancellationToken ct)
    {
        _header.PropertyChanged -= HeaderChanged;
        Item.PropertyChanged -= ItemChanged;

        return _objectStorage.SaveAsync(
            $"{typeof(ToDoItemViewModel).FullName}.{Item.Id}",
            new ToDosSetting { GroupBy = List.GroupBy, OrderBy = List.OrderBy },
            ct
        );
    }

    public ConfiguredValueTaskAwaitable InitUiAsync(CancellationToken ct)
    {
        return InitCore(ct).ConfigureAwait(false);
    }

    protected override HestiaGetRequest CreateRefreshRequest()
    {
        return new() { ChildrenIds = [Item.Id], ParentIds = [Item.Id] };
    }

    public override void RefreshUi()
    {
        base.RefreshUi();
        _header.RefreshUi();
    }

    private readonly IObjectStorage _objectStorage;
    private readonly ToDosHeaderViewModel _header;

    private void ItemChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(Item.Name))
        {
            _header.Title = Item.Name;
        }
    }

    private void HeaderChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ToDosHeaderViewModel.IsMulti))
        {
            IsMulti = _header.IsMulti;
        }
    }

    private async ValueTask InitCore(CancellationToken ct)
    {
        _header.PropertyChanged += HeaderChanged;
        Item.PropertyChanged += ItemChanged;
        await RefreshAsync(ct);

        var setting = await _objectStorage.LoadAsync<ToDosSetting>(
            $"{typeof(ToDoItemViewModel).FullName}.{Item.Id}",
            ct
        );

        if (setting is null)
        {
            return;
        }

        Dispatcher.UIThread.Post(() =>
        {
            List.GroupBy = setting.GroupBy;
            List.OrderBy = setting.OrderBy;
        });
    }

    [RelayCommand]
    private async Task ShowCreateViewAsync(CancellationToken ct)
    {
        var credential = Factory.CreateToDoParameters(ValidationMode.ValidateAll, false);

        await WrapCommandAsync(
            () =>
            {
                var header = StringFormater
                    .Format(
                        AppResourceService.GetResource<string>("Lang.CreatingNewItem"),
                        AppResourceService.GetResource<string>("Lang.ToDo")
                    )
                    .DispatchToDialogHeader();

                var button = new DialogButton(
                    AppResourceService.GetResource<string>("Lang.Create"),
                    CreateCommand,
                    credential,
                    DialogButtonType.Primary
                );

                var dialog = new DialogViewModel(header, credential, button, UiHelper.CancelButton);

                return DialogService.ShowMessageBoxAsync(dialog, ct);
            },
            ct
        );
    }

    [RelayCommand]
    private async Task CreateAsync(ToDoParametersViewModel parameters, CancellationToken ct)
    {
        await WrapCommandAsync(() => CreateCore(parameters, ct).ConfigureAwait(false), ct);
    }

    private async ValueTask<IValidationErrors> CreateCore(
        ToDoParametersViewModel parameters,
        CancellationToken ct
    )
    {
        DialogService.DispatchCloseMessageBox();
        parameters.StartExecute();

        if (parameters.HasErrors)
        {
            return new EmptyValidationErrors();
        }

        var create = parameters.CreateShortToDo();
        create.ParentId = Item.Id;

        return await ToDoUiService.PostAsync(Guid.NewGuid(), new() { Creates = [create] }, ct);
    }
}
