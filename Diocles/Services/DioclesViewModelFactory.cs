using Avalonia;
using Avalonia.Collections;
using Diocles.Models;
using Diocles.Ui;
using Gaia.Services;
using Hestia.Contract.Services;
using Inanna.Models;
using Inanna.Services;
using Weber.Services;
using IServiceProvider = Gaia.Services.IServiceProvider;

namespace Diocles.Services;

public interface IDioclesViewModelFactory
{
    AddBarcodeFileViewModel CreateAddBarcodeFile();
    ToDoItemViewModel CreateToDos(ToDoNotify item);
    ChangeParentToDoViewModel CreateChangeParentToDo();
    RootToDosViewModel CreateRootToDos();
    ToDoTreeViewModel CreateToDoTree();
    ToDoListViewModel CreateToDoList(IAvaloniaReadOnlyList<ToDoNotify> input);
    SearchToDoViewModel CreSearchToDo();
    ToDoItemHeaderViewModel CreateToDoItemHeader(ToDoNotify item);

    ToDoParametersViewModel CreateToDoParameters(
        ToDoParametersSettings settings,
        ValidationMode validationMode,
        bool isShowEdit
    );

    ToDosHeaderViewModel CreateToDosHeader(
        string title,
        IAvaloniaReadOnlyList<InannaCommand> commands,
        IAvaloniaReadOnlyList<InannaCommand> multiCommands
    );

    ToDoParametersViewModel CreateToDoParameters(
        ToDoNotify item,
        ValidationMode validationMode,
        bool isShowEdit
    );
}

public sealed class DioclesViewModelFactory : IDioclesViewModelFactory
{
    public DioclesViewModelFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public ToDosHeaderViewModel CreateToDosHeader(
        string title,
        IAvaloniaReadOnlyList<InannaCommand> commands,
        IAvaloniaReadOnlyList<InannaCommand> multiCommands
    )
    {
        return new(
            title,
            commands,
            multiCommands,
            _serviceProvider.GetService<IInannaViewModelFactory>(),
            _serviceProvider.GetService<ISafeExecuteWrapper>(),
            _serviceProvider.GetService<DioclesCommands>()
        );
    }

    public SearchToDoViewModel CreSearchToDo()
    {
        return new(
            this,
            _serviceProvider.GetService<IToDoUiService>(),
            _serviceProvider.GetService<IToDoUiCache>(),
            _serviceProvider.GetService<IDialogService>(),
            _serviceProvider.GetService<IAppResourceService>(),
            _serviceProvider.GetService<IStringFormater>(),
            _serviceProvider.GetService<IFileStorageUiService>(),
            _serviceProvider.GetService<IObjectStorage>(),
            _serviceProvider.GetService<DioclesCommands>(),
            _serviceProvider.GetService<ISafeExecuteWrapper>()
        );
    }

    public ToDoItemHeaderViewModel CreateToDoItemHeader(ToDoNotify item)
    {
        return new(
            item,
            _serviceProvider.GetService<IInannaViewModelFactory>(),
            _serviceProvider.GetService<ISafeExecuteWrapper>(),
            _serviceProvider.GetService<DioclesCommands>()
        );
    }

    public AddBarcodeFileViewModel CreateAddBarcodeFile()
    {
        return new(
            _serviceProvider.GetService<IInannaViewModelFactory>(),
            _serviceProvider.GetService<ISafeExecuteWrapper>()
        );
    }

    public ToDoItemViewModel CreateToDos(ToDoNotify item)
    {
        return new(
            item,
            _serviceProvider.GetService<IToDoUiService>(),
            _serviceProvider.GetService<IStringFormater>(),
            _serviceProvider.GetService<IDialogService>(),
            _serviceProvider.GetService<IAppResourceService>(),
            this,
            _serviceProvider.GetService<IObjectStorage>(),
            _serviceProvider.GetService<IFileStorageUiService>(),
            _serviceProvider.GetService<IFileStorageUiCache>(),
            _serviceProvider.GetService<IWeberViewModelFactory>(),
            _serviceProvider.GetService<InannaCommands>(),
            _serviceProvider.GetService<DioclesCommands>(),
            _serviceProvider.GetService<ISafeExecuteWrapper>()
        );
    }

    public ChangeParentToDoViewModel CreateChangeParentToDo()
    {
        return new(this, _serviceProvider.GetService<ISafeExecuteWrapper>());
    }

    public RootToDosViewModel CreateRootToDos()
    {
        return new(
            _serviceProvider.GetService<IToDoUiService>(),
            _serviceProvider.GetService<IToDoUiCache>(),
            _serviceProvider.GetService<IStringFormater>(),
            _serviceProvider.GetService<IDialogService>(),
            _serviceProvider.GetService<IAppResourceService>(),
            this,
            _serviceProvider.GetService<IObjectStorage>(),
            _serviceProvider.GetService<IFileStorageUiService>(),
            _serviceProvider.GetService<DioclesCommands>(),
            _serviceProvider.GetService<ISafeExecuteWrapper>()
        );
    }

    public ToDoTreeViewModel CreateToDoTree()
    {
        return new(
            _serviceProvider.GetService<IToDoUiCache>(),
            _serviceProvider.GetService<IToDoUiService>(),
            _serviceProvider.GetService<ISafeExecuteWrapper>()
        );
    }

    public ToDoParametersViewModel CreateToDoParameters(
        ToDoParametersSettings settings,
        ValidationMode validationMode,
        bool isShowEdit
    )
    {
        return new(
            settings,
            validationMode,
            isShowEdit,
            _serviceProvider.GetService<IToDoValidator>(),
            this,
            _serviceProvider.GetService<IFileStorageUiService>(),
            _serviceProvider.GetService<Application>(),
            _serviceProvider.GetService<IAppResourceService>(),
            _serviceProvider.GetService<IStringFormater>(),
            _serviceProvider.GetService<IDialogService>(),
            _serviceProvider.GetService<IToDoUiService>(),
            _serviceProvider.GetService<ISafeExecuteWrapper>(),
            _serviceProvider.GetService<ICommandFactory>(),
            _serviceProvider.GetService<IHashService<byte[], string>>()
        );
    }

    public ToDoParametersViewModel CreateToDoParameters(
        ToDoNotify item,
        ValidationMode validationMode,
        bool isShowEdit
    )
    {
        return new(
            item,
            validationMode,
            isShowEdit,
            _serviceProvider.GetService<IToDoValidator>(),
            this,
            _serviceProvider.GetService<IFileStorageUiService>(),
            _serviceProvider.GetService<IFileStorageUiCache>(),
            _serviceProvider.GetService<Application>(),
            _serviceProvider.GetService<IAppResourceService>(),
            _serviceProvider.GetService<IStringFormater>(),
            _serviceProvider.GetService<IDialogService>(),
            _serviceProvider.GetService<IToDoUiService>(),
            _serviceProvider.GetService<ISafeExecuteWrapper>(),
            _serviceProvider.GetService<ICommandFactory>(),
            _serviceProvider.GetService<IHashService<byte[], string>>()
        );
    }

    public ToDoListViewModel CreateToDoList(IAvaloniaReadOnlyList<ToDoNotify> input)
    {
        return new(
            input,
            _serviceProvider.GetService<IToDoUiCache>(),
            _serviceProvider.GetService<InannaCommands>(),
            _serviceProvider.GetService<DioclesCommands>(),
            _serviceProvider.GetService<ISafeExecuteWrapper>()
        );
    }

    private readonly IServiceProvider _serviceProvider;
}
