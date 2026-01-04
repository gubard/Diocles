using System.Text.Json;
using Diocles.Models;
using Diocles.Ui;
using Gaia.Helpers;
using Gaia.Models;
using Gaia.Services;
using Hestia.Contract.Models;
using Hestia.Contract.Services;
using Inanna.Models;
using Inanna.Services;
using Jab;
using Nestor.Db.Services;

namespace Diocles.Services;

[ServiceProviderModule]
[Transient(typeof(RootToDosViewModel))]
[Transient(typeof(ToDoParametersFillerService))]
[Singleton(typeof(IToDoCache), typeof(ToDoCache))]
[Transient(typeof(IToDoValidator), typeof(ToDoValidator))]
[Transient(typeof(IDioclesViewModelFactory), typeof(DioclesViewModelFactory))]
public interface IDioclesServiceProvider;
