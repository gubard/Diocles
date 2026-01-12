using System.Runtime.CompilerServices;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.Input;
using Diocles.Models;
using Diocles.Services;
using Gaia.Services;
using Hestia.Contract.Models;
using Inanna.Helpers;
using Inanna.Models;
using Inanna.Services;

namespace Diocles.Ui;

public partial class RootToDosViewModel : ToDosViewModelBase, IHeader, ISaveUi, IInitUi
{
    public RootToDosViewModel(
        IUiToDoService uiToDoService,
        IToDoMemoryCache toDoMemoryCache,
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
            toDoMemoryCache.Roots
        )
    {
        _objectStorage = objectStorage;
        Header = factory.CreateRootToDosHeader([]);
        Multi = Header;
    }

    object IHeader.Header => Header;
    public RootToDosHeaderViewModel Header { get; }

    public ConfiguredValueTaskAwaitable SaveAsync(CancellationToken ct)
    {
        return _objectStorage.SaveAsync(
            $"{typeof(RootToDosViewModel).FullName}",
            new ToDosSetting { GroupBy = List.GroupBy, OrderBy = List.OrderBy },
            ct
        );
    }

    public ConfiguredValueTaskAwaitable InitUiAsync(CancellationToken ct)
    {
        return InitCore(ct).ConfigureAwait(false);
    }

    public override void RefreshUi()
    {
        base.RefreshUi();
        Header.RefreshUi();
    }

    protected override HestiaGetRequest CreateRefreshRequest()
    {
        return new() { IsRoots = true };
    }

    private readonly IObjectStorage _objectStorage;

    private async ValueTask InitCore(CancellationToken ct)
    {
        await RefreshAsync(ct);

        var setting = await _objectStorage.LoadAsync<ToDosSetting>(
            $"{typeof(RootToDosViewModel).FullName}",
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
                DialogService.ShowMessageBoxAsync(
                    new(
                        Dispatcher.UIThread.Invoke(() =>
                            StringFormater
                                .Format(
                                    AppResourceService.GetResource<string>("Lang.CreatingNewItem"),
                                    AppResourceService.GetResource<string>("Lang.ToDo")
                                )
                                .ToDialogHeader()
                        ),
                        credential,
                        new DialogButton(
                            AppResourceService.GetResource<string>("Lang.Create"),
                            CreateCommand,
                            credential,
                            DialogButtonType.Primary
                        ),
                        UiHelper.CancelButton
                    ),
                    ct
                ),
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

        var request = new HestiaPostRequest { Creates = [parameters.CreateShortToDo()] };
        var response = await UiToDoService.PostAsync(Guid.NewGuid(), request, ct);
        Dispatcher.UIThread.Post(() => DialogService.CloseMessageBox());

        return response;
    }
}
