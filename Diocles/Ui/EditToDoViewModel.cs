using Avalonia.Controls;
using CommunityToolkit.Mvvm.Input;
using Diocles.Models;
using Diocles.Services;
using Hestia.Contract.Models;
using Inanna.Models;
using Inanna.Services;

namespace Diocles.Ui;

public partial class EditToDoViewModel : ViewModelBase, IHeader
{
    private readonly EditToDoHeaderViewModel _header;
    private readonly IUiToDoService _uiToDoService;
    private readonly INotificationService _notificationService;
    private readonly IAppResourceService _appResourceService;

    public EditToDoViewModel(
        ToDoNotify item,
        IDioclesViewModelFactory factory,
        IUiToDoService uiToDoService,
        INotificationService notificationService,
        IAppResourceService appResourceService
    )
    {
        _uiToDoService = uiToDoService;
        _notificationService = notificationService;
        _appResourceService = appResourceService;
        Parameters = factory.CreateToDoParameters(item, ValidationMode.ValidateOnlyEdited, false);
        _header = factory.CreateEditToDoHeader(item);
    }

    public ToDoParametersViewModel Parameters { get; }
    public object Header => _header;

    [RelayCommand]
    private async Task SaveAsync(CancellationToken ct)
    {
        await WrapCommandAsync(() => SaveCore(ct).ConfigureAwait(false), ct);
    }

    private async ValueTask<HestiaPostResponse> SaveCore(CancellationToken ct)
    {
        var edit = Parameters.CreateEditToDos();
        edit.Ids = [_header.Item.Id];
        var response = await _uiToDoService.PostAsync(Guid.NewGuid(), new() { Edits = [edit] }, ct);

        _notificationService.ShowNotification(
            new TextBlock
            {
                Text = _appResourceService.GetResource<string>("Lang.Saved"),
                Classes = { "h2", "align-center" },
            },
            NotificationType.None
        );

        return response;
    }
}
