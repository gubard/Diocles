using Diocles.Ui;
using Jab;

namespace Diocles.Services;

[ServiceProviderModule]
[Transient(typeof(RootToDosViewModel))]
[Transient(typeof(ToDoSubItemsViewModel))]
public interface IDioclesServiceProvider;