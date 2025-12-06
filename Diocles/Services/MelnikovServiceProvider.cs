using Diocles.Ui;
using Jab;

namespace Diocles.Services;

[ServiceProviderModule]
[Transient(typeof(RootToDosViewModel))]
public interface IDioclesServiceProvider;