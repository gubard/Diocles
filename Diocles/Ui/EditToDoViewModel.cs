using Avalonia.Controls;
using CommunityToolkit.Mvvm.Input;
using Diocles.Models;
using Diocles.Services;
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
        Parameters = factory.Create((item, ValidationMode.ValidateOnlyEdited, false));
        _header = factory.CreateEditToDoHeader(item);
    }

    public ToDoParametersViewModel Parameters { get; }
    public object Header => _header;

    [RelayCommand]
    private async Task SaveAsync(CancellationToken ct)
    {
        await WrapCommand(async () =>
        {
            var response = await _uiToDoService.PostAsync(
                new() { Edits = [Parameters.CreateEditToDos()] },
                ct
            );

            _notificationService.ShowNotification(
                new TextBlock
                {
                    Text = _appResourceService.GetResource<string>("Lang.Saved"),
                    Classes = { "h2", "align-center" },
                },
                NotificationType.None
            );

            return response;
        });
    }
}
