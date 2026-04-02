using System.Runtime.CompilerServices;
using Neotoma.Contract.Models;
using Weber.Models;
using Weber.Services;

namespace Diocles.Helpers;

public static class FileStorageUiServiceExtension
{
    public static ConfiguredValueTaskAwaitable<NeotomaGetResponse> RefreshFileStorageAsync(
        this IFileStorageUiService service,
        string filesDir,
        IEnumerable<FileObjectNotify> files,
        CancellationToken ct
    )
    {
        return service.RefreshFileStorageCore(filesDir, files, ct).ConfigureAwait(false);
    }

    private static async ValueTask<NeotomaGetResponse> RefreshFileStorageCore(
        this IFileStorageUiService service,
        string filesDir,
        IEnumerable<FileObjectNotify> files,
        CancellationToken ct
    )
    {
        var response = await service.GetAsync(new() { GetInfo = [filesDir] }, ct);

        if (response.ValidationErrors.Count != 0)
        {
            return response;
        }

        var ids = files
            .Where(x => x.Status == FileObjectNotifyStatus.WrongHash)
            .Select(x => x.Id)
            .ToArray();

        if (ids.Length == 0)
        {
            return response;
        }

        response = await service.GetAsync(new() { GetData = ids }, ct);

        return response;
    }
}
