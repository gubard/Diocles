using System.Security.Cryptography;
using Gaia.Services;
using Hestia.Contract.Services;
using Jab;
using Weber.Services;

namespace Diocles.Services;

[ServiceProviderModule]
[Import(typeof(IWeberServiceProvider))]
[Transient(typeof(ToDoParametersFillerService))]
[Singleton(typeof(IToDoMemoryCache), typeof(ToDoMemoryCache))]
[Transient(typeof(IToDoValidator), typeof(ToDoValidator))]
[Transient(typeof(IDioclesViewModelFactory), typeof(DioclesViewModelFactory))]
[Transient(typeof(ITransformer<byte[], string>), typeof(BytesToBase64))]
[Transient(typeof(IHashService<byte[], string>), typeof(BytesToStringHashService))]
[Singleton(typeof(DioclesCommands))]
[Singleton(typeof(IHashService<byte[], byte[]>), typeof(Sha512HashService))]
[Transient(typeof(SHA512), Factory = nameof(GetSha512))]
public interface IDioclesServiceProvider
{
    public static SHA512 GetSha512()
    {
        return SHA512.Create();
    }
}
