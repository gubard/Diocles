using Diocles.Ui;
using Hestia.Contract.Services;
using Jab;

namespace Diocles.Services;

[ServiceProviderModule]
[Transient(typeof(RootToDosViewModel))]
[Transient(typeof(ToDoParametersFillerService))]
[Singleton(typeof(IToDoMemoryCache), typeof(ToDoMemoryCache))]
[Transient(typeof(IToDoValidator), typeof(ToDoValidator))]
[Transient(typeof(IDioclesViewModelFactory), typeof(DioclesViewModelFactory))]
public interface IDioclesServiceProvider;
