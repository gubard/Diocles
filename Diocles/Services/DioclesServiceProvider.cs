using Diocles.Ui;
using Hestia.Contract.Services;
using Jab;
using Weber.Services;

namespace Diocles.Services;

[ServiceProviderModule]
[Import(typeof(IWeberServiceProvider))]
[Transient(typeof(RootToDosViewModel))]
[Transient(typeof(ToDoParametersFillerService))]
[Singleton(typeof(IToDoMemoryCache), typeof(ToDoMemoryCache))]
[Transient(typeof(IToDoValidator), typeof(ToDoValidator))]
[Transient(typeof(IDioclesViewModelFactory), typeof(DioclesViewModelFactory))]
[Transient(typeof(SearchToDoViewModel))]
public interface IDioclesServiceProvider;
