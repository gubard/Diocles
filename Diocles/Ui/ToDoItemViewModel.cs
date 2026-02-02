using System.ComponentModel;
using System.Runtime.CompilerServices;
using Avalonia.Collections;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.Input;
using Diocles.Helpers;
using Diocles.Models;
using Diocles.Services;
using Gaia.Services;
using IconPacks.Avalonia.MaterialDesign;
using Inanna.Helpers;
using Inanna.Models;
using Inanna.Services;
using Inanna.Ui;
using Neotoma.Contract.Models;

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
        IObjectStorage objectStorage,
        IFileStorageUiService fileStorageUiService,
        IFileStorageUiCache fileStorageUiCache
    )
        : base(
            dialogService,
            appResourceService,
            stringFormater,
            factory,
            toDoUiService,
            toDoUiCache,
            item.Children,
            fileStorageUiService
        )
    {
        Item = item;
        Files = fileStorageUiCache.GetFiles($"{Item.Id}/ToDo");

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
    public AvaloniaList<FileObjectNotify> Files { get; }

    public ConfiguredValueTaskAwaitable SaveUiAsync(CancellationToken ct)
    {
        return SaveUiCore(ct).ConfigureAwait(false);
    }

    public ConfiguredValueTaskAwaitable InitUiAsync(CancellationToken ct)
    {
        return InitCore(ct).ConfigureAwait(false);
    }

    public override ConfiguredValueTaskAwaitable RefreshAsync(CancellationToken ct)
    {
        return WrapCommandAsync(
            async () =>
            {
                IValidationErrors errors = await ToDoUiService.GetAsync(
                    new() { ChildrenIds = [Item.Id], ParentIds = [Item.Id] },
                    ct
                );

                if (errors.ValidationErrors.Count > 0)
                {
                    return errors;
                }

                var dir = $"{Item.Id}/ToDo";

                return await FileStorageUiService.GetAsync(new() { GetFiles = [dir] }, ct);
            },
            ct
        );
    }

    public override void RefreshUi()
    {
        base.RefreshUi();
        _header.RefreshUi();
    }

    private readonly IObjectStorage _objectStorage;
    private readonly ToDosHeaderViewModel _header;

    private async ValueTask SaveUiCore(CancellationToken ct)
    {
        _header.PropertyChanged -= HeaderPropertyChanged;
        Item.PropertyChanged -= ItemPropertyChanged;

        await _objectStorage.SaveAsync(
            $"{typeof(ToDoItemViewModel).FullName}.{Item.Id}",
            new ToDosSetting { GroupBy = List.GroupBy, OrderBy = List.OrderBy },
            ct
        );

        await List.SaveUiAsync(ct);
    }

    private void ItemPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(Item.Name))
        {
            _header.Title = Item.Name;
        }
    }

    private void HeaderPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ToDosHeaderViewModel.IsMulti))
        {
            IsMulti = _header.IsMulti;
        }
    }

    private async ValueTask InitCore(CancellationToken ct)
    {
        _header.PropertyChanged += HeaderPropertyChanged;
        Item.PropertyChanged += ItemPropertyChanged;

        var setting = await _objectStorage.LoadAsync<ToDosSetting>(
            $"{typeof(ToDoItemViewModel).FullName}.{Item.Id}",
            ct
        );

        Dispatcher.UIThread.Post(() =>
        {
            List.GroupBy = setting.GroupBy;
            List.OrderBy = setting.OrderBy;
        });

        await List.InitUiAsync(ct);
        await RefreshAsync(ct);
    }

    [RelayCommand]
    private async Task ShowCreateViewAsync(CancellationToken ct)
    {
        var settings = await _objectStorage.LoadAsync<ToDoParametersSettings>(
            $"{typeof(ToDoParametersSettings).FullName}.{Item.Id}",
            ct
        );

        var credential = Factory.CreateToDoParameters(settings, ValidationMode.ValidateAll, false);

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
        parameters.StartExecute();

        if (parameters.HasErrors)
        {
            return new EmptyValidationErrors();
        }

        var settings = parameters.CreateSettings();
        await DialogService.CloseMessageBoxAsync(ct);

        await _objectStorage.SaveAsync(
            $"{typeof(ToDoParametersSettings).FullName}.{Item.Id}",
            settings,
            ct
        );

        var create = parameters.CreateShortToDo();
        create.ParentId = Item.Id;

        return await ToDoUiService.PostAsync(Guid.NewGuid(), new() { Creates = [create] }, ct);
    }
}
