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
            toDoUiCache.Roots
        )
    {
        _objectStorage = objectStorage;

        _header = factory.CreateToDosHeader(
            appResourceService.GetResource<string>("Lang.ToDos"),
            new AvaloniaList<InannaCommand>(),
            DiocleHelper.CreateMultiCommands(toDoUiCache.Roots)
        );
    }

    public object Header => _header;

    public ConfiguredValueTaskAwaitable SaveUiAsync(CancellationToken ct)
    {
        return SaveUiCore(ct).ConfigureAwait(false);
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

    private void HeaderPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ToDosHeaderViewModel.IsMulti))
        {
            IsMulti = _header.IsMulti;
        }
    }

    public async ValueTask SaveUiCore(CancellationToken ct)
    {
        _header.PropertyChanged -= HeaderPropertyChanged;

        await _objectStorage.SaveAsync(
            $"{typeof(RootToDosViewModel).FullName}",
            new ToDosSetting { GroupBy = List.GroupBy, OrderBy = List.OrderBy },
            ct
        );

        await List.SaveUiAsync(ct);
    }

    private async ValueTask InitCore(CancellationToken ct)
    {
        _header.PropertyChanged += HeaderPropertyChanged;
        await List.InitUiAsync(ct);
        await RefreshAsync(ct);

        var setting = await _objectStorage.LoadAsync<ToDosSetting>(
            $"{typeof(RootToDosViewModel).FullName}",
            ct
        );

        Dispatcher.UIThread.Post(() =>
        {
            List.GroupBy = setting.GroupBy;
            List.OrderBy = setting.OrderBy;
        });
    }

    [RelayCommand]
    private async Task ShowCreateViewAsync(CancellationToken ct)
    {
        var settings = await _objectStorage.LoadAsync<ToDoParametersSettings>(
            $"{typeof(ToDoParametersSettings).FullName}",
            ct
        );

        var parameters = Factory.CreateToDoParameters(settings, ValidationMode.ValidateAll, false);

        await WrapCommandAsync(
            () =>
                DialogService.ShowMessageBoxAsync(
                    new(
                        StringFormater
                            .Format(
                                AppResourceService.GetResource<string>("Lang.CreatingNewItem"),
                                AppResourceService.GetResource<string>("Lang.ToDo")
                            )
                            .DispatchToDialogHeader(),
                        parameters,
                        new DialogButton(
                            AppResourceService.GetResource<string>("Lang.Create"),
                            CreateCommand,
                            parameters,
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

        var settings = parameters.CreateSettings();
        var create = parameters.CreateShortToDo();
        await DialogService.CloseMessageBoxAsync(ct);
        await _objectStorage.SaveAsync($"{typeof(ToDoParametersSettings).FullName}", settings, ct);
        var request = new HestiaPostRequest { Creates = [create] };
        var response = await ToDoUiService.PostAsync(Guid.NewGuid(), request, ct);

        return response;
    }
}
