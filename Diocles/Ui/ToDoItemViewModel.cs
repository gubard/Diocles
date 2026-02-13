using System.ComponentModel;
using System.Runtime.CompilerServices;
using Avalonia.Collections;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.Input;
using Diocles.Helpers;
using Diocles.Models;
using Diocles.Services;
using Gaia.Helpers;
using Gaia.Services;
using Hestia.Contract.Models;
using IconPacks.Avalonia.MaterialDesign;
using Inanna.Helpers;
using Inanna.Models;
using Inanna.Services;
using Inanna.Ui;
using Weber.Models;
using Weber.Services;

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
        IFileStorageUiCache fileStorageUiCache,
        IWeberViewModelFactory weberFactory
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

        _header = factory.CreateToDoItemHeader(
            item,
            new AvaloniaList<InannaCommand>
            {
                new(
                    ShowEditCommand,
                    item,
                    appResourceService.GetResource<string>("Lang.Edit"),
                    PackIconMaterialDesignKind.Edit
                ),
                new(
                    DioclesCommands.ShowDeleteToDoCommand,
                    item,
                    appResourceService.GetResource<string>("Lang.Delete"),
                    PackIconMaterialDesignKind.Delete,
                    ButtonType.Danger
                ),
            }
        );

        _objectStorage = objectStorage;
        _weberFactory = weberFactory;
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
        var dir = $"{Item.Id}/ToDo";

        return WrapCommandAsync(
            async () =>
            {
                var errors = await TaskHelper.WhenAllAsync(
                    [
                        ToDoUiService
                            .GetAsync(new() { ChildrenIds = [Item.Id], ParentIds = [Item.Id] }, ct)
                            .ToValidationErrors(),
                        FileStorageUiService
                            .GetAsync(new() { GetFiles = [dir] }, ct)
                            .ToValidationErrors(),
                    ],
                    ct
                );

                Dispatcher.UIThread.Post(() => List.Refresh());

                return errors.Combine();
            },
            ct
        );
    }

    private readonly IObjectStorage _objectStorage;
    private readonly ToDoItemHeaderViewModel _header;
    private readonly IWeberViewModelFactory _weberFactory;

    [RelayCommand]
    private async Task ShowImageAsync(FileObjectNotify item, CancellationToken ct)
    {
        await WrapCommandAsync(
            () =>
                DialogService.ShowMessageBoxAsync(
                    new(
                        Dispatcher.UIThread.Invoke(() =>
                            StringFormater
                                .Format(
                                    AppResourceService.GetResource<string>("Lang.ImageItem"),
                                    Item.Name
                                )
                                .DispatchToDialogHeader()
                        ),
                        _weberFactory.CreateFiles(Files, item),
                        UiHelper.OkButton
                    ),
                    ct
                ),
            ct
        );
    }

    private async ValueTask SaveUiCore(CancellationToken ct)
    {
        _header.PropertyChanged -= HeaderPropertyChanged;

        await _objectStorage.SaveAsync(
            new ToDosSetting { GroupBy = List.GroupBy, OrderBy = List.OrderBy },
            Item.Id,
            ct
        );

        await List.SaveUiAsync(ct);
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

        await WrapCommandAsync(
            async () =>
            {
                var setting = await _objectStorage.LoadAsync<ToDosSetting>(Item.Id, ct);

                Dispatcher.UIThread.Post(() =>
                {
                    List.GroupBy = setting.GroupBy;
                    List.OrderBy = setting.OrderBy;
                });

                await List.InitUiAsync(ct);
            },
            ct
        );

        await RefreshAsync(ct);
    }

    [RelayCommand]
    private async Task ShowCreateViewAsync(CancellationToken ct)
    {
        var settings = await _objectStorage.LoadAsync<ToDoParametersSettings>(Item.Id, ct);
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
            return new DefaultValidationErrors();
        }

        var id = Guid.NewGuid();
        var settings = parameters.CreateSettings();
        var create = parameters.CreateShortToDo(id, Item.Id);
        var files = parameters.CreateNeotomaPostRequest($"{id}/ToDo");
        await DialogService.CloseMessageBoxAsync(ct);
        await _objectStorage.SaveAsync(settings, Item.Id, ct);
        var request = new HestiaPostRequest { Creates = [create] };

        var errors = await TaskHelper.WhenAllAsync(
            [
                ToDoUiService.PostAsync(Guid.NewGuid(), request, ct).ToValidationErrors(),
                FileStorageUiService.PostAsync(Guid.NewGuid(), files, ct).ToValidationErrors(),
            ],
            ct
        );

        return errors.Combine();
    }
}
