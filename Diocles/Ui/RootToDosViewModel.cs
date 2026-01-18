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
using Inanna.Helpers;
using Inanna.Models;
using Inanna.Services;

namespace Diocles.Ui;

public partial class RootToDosViewModel : ToDosMainViewModelBase, IHeader, ISaveUi, IInitUi
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

        _header = factory.CreateToDosHeader(
            appResourceService.GetResource<string>("Lang.ToDos"),
            new AvaloniaList<InannaCommand>(),
            DiocleHelper.CreateMultiCommands(toDoMemoryCache.Roots)
        );
    }

    public object Header => _header;

    public ConfiguredValueTaskAwaitable SaveUiAsync(CancellationToken ct)
    {
        _header.PropertyChanged -= HeaderChanged;

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

    protected override HestiaGetRequest CreateRefreshRequest()
    {
        return new() { IsRoots = true, IsSelectors = true };
    }

    private readonly IObjectStorage _objectStorage;
    private readonly ToDosHeaderViewModel _header;

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
        DialogService.DispatchCloseMessageBox();
        parameters.StartExecute();

        if (parameters.HasErrors)
        {
            return new EmptyValidationErrors();
        }

        var request = new HestiaPostRequest { Creates = [parameters.CreateShortToDo()] };
        var response = await UiToDoService.PostAsync(Guid.NewGuid(), request, ct);

        return response;
    }
}
