using System.Runtime.CompilerServices;
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

public partial class ToDosViewModel : ToDosMainViewModelBase, IHeader, ISaveUi, IInitUi
{
    public ToDosViewModel(
        ToDoNotify item,
        IUiToDoService uiToDoService,
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
            uiToDoService,
            item.Children
        )
    {
        Header = factory.CreateToDosHeader(
            item,
            [new(ShowEditCommand, item, PackIconMaterialDesignKind.Edit)],
            [
                new(
                    DioclesCommands.DeleteToDosCommand,
                    item.Children,
                    PackIconMaterialDesignKind.Delete,
                    ButtonType.Danger
                ),
            ]
        );

        _objectStorage = objectStorage;

        Header.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(Header.IsMulti))
            {
                IsMulti = Header.IsMulti;
            }
        };
    }

    public ToDosHeaderViewModel Header { get; }
    object IHeader.Header => Header;

    public ConfiguredValueTaskAwaitable SaveAsync(CancellationToken ct)
    {
        return _objectStorage.SaveAsync(
            $"{typeof(ToDosViewModel).FullName}.{Header.Item.Id}",
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
        return new() { ChildrenIds = [Header.Item.Id], ParentIds = [Header.Item.Id] };
    }

    public override void RefreshUi()
    {
        base.RefreshUi();
        Header.RefreshUi();
    }

    private readonly IObjectStorage _objectStorage;

    private async ValueTask InitCore(CancellationToken ct)
    {
        await RefreshAsync(ct);

        var setting = await _objectStorage.LoadAsync<ToDosSetting>(
            $"{typeof(ToDosViewModel).FullName}.{Header.Item.Id}",
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
                var header = Dispatcher.UIThread.Invoke(() =>
                    StringFormater
                        .Format(
                            AppResourceService.GetResource<string>("Lang.CreatingNewItem"),
                            AppResourceService.GetResource<string>("Lang.ToDo")
                        )
                        .ToDialogHeader()
                );

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
        Dispatcher.UIThread.Post(() => DialogService.CloseMessageBox());
        parameters.StartExecute();

        if (parameters.HasErrors)
        {
            return new EmptyValidationErrors();
        }

        var create = parameters.CreateShortToDo();
        create.ParentId = Header.Item.Id;

        return await UiToDoService.PostAsync(Guid.NewGuid(), new() { Creates = [create] }, ct);
    }
}
