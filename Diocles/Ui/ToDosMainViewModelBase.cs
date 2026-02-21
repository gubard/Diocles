using System.Runtime.CompilerServices;
using Avalonia.Collections;
using CommunityToolkit.Mvvm.ComponentModel;
using Diocles.Models;
using Diocles.Services;
using Gaia.Services;
using Inanna.Models;
using Inanna.Services;
using Weber.Services;

namespace Diocles.Ui;

public abstract class ToDosMainViewModelBase : ToDosViewModelBase, IRefresh
{
    public abstract ConfiguredValueTaskAwaitable RefreshAsync(CancellationToken ct);

    protected ToDosMainViewModelBase(
        IDialogService dialogService,
        IAppResourceService appResourceService,
        IStringFormater stringFormater,
        IDioclesViewModelFactory factory,
        IToDoUiService toDoUiService,
        IToDoUiCache toDoUiCache,
        IAvaloniaReadOnlyList<ToDoNotify> items,
        IFileStorageUiService fileStorageUiService
    )
        : base(
            dialogService,
            appResourceService,
            stringFormater,
            factory,
            toDoUiService,
            toDoUiCache,
            items,
            fileStorageUiService
        ) { }
}

public abstract partial class ToDosViewModelBase : ViewModelBase, IToDosViewModel
{
    public ToDoListViewModel List { get; }

    protected readonly IDialogService DialogService;
    protected readonly IAppResourceService AppResourceService;
    protected readonly IStringFormater StringFormater;
    protected readonly IDioclesViewModelFactory Factory;
    protected readonly IToDoUiService ToDoUiService;
    protected readonly IToDoUiCache ToDoUiCache;
    protected readonly IFileStorageUiService FileStorageUiService;

    protected ToDosViewModelBase(
        IDialogService dialogService,
        IAppResourceService appResourceService,
        IStringFormater stringFormater,
        IDioclesViewModelFactory factory,
        IToDoUiService toDoUiService,
        IToDoUiCache toDoUiCache,
        IAvaloniaReadOnlyList<ToDoNotify> items,
        IFileStorageUiService fileStorageUiService
    )
    {
        DialogService = dialogService;
        AppResourceService = appResourceService;
        StringFormater = stringFormater;
        Factory = factory;
        ToDoUiService = toDoUiService;
        ToDoUiCache = toDoUiCache;
        FileStorageUiService = fileStorageUiService;
        List = factory.CreateToDoList(items);
    }

    [ObservableProperty]
    private bool _isMulti;
}
