using Avalonia.Controls;
using Inanna.Models;
using Inanna.Services;

namespace Diocles.Ui;

public class RootToDosViewModel : ViewModelBase, IHeader
{
    private readonly IAppResourceService _appResourceService;

    public RootToDosViewModel(IAppResourceService appResourceService)
    {
        _appResourceService = appResourceService;
    }

    public object Header
    {
        get => new TextBlock
        {
            Text = _appResourceService.GetResource<string>("Lang.ToDos"),
            Classes =
            {
                "h2",
            },
        };
    }
}