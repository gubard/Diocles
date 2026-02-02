using System.Runtime.CompilerServices;
using Avalonia.Collections;
using Avalonia.Threading;
using Diocles.Models;
using Gaia.Helpers;
using Gaia.Services;
using Inanna.Helpers;
using Inanna.Services;
using Neotoma.Contract.Models;
using Neotoma.Contract.Services;

namespace Diocles.Services;

public interface IFileStorageMemoryCache : IMemoryCache<NeotomaPostRequest, NeotomaGetResponse>
{
    AvaloniaList<FileObjectNotify> GetFiles(string dir);
}

public interface IFileStorageUiCache
    : IUiCache<NeotomaPostRequest, NeotomaGetResponse, IFileStorageMemoryCache>
{
    AvaloniaList<FileObjectNotify> GetFiles(string dir);
}

public sealed class FileStorageUiCache
    : UiCache<NeotomaPostRequest, NeotomaGetResponse, IFileStorageDbCache, IFileStorageMemoryCache>,
        IFileStorageUiCache
{
    public FileStorageUiCache(IFileStorageDbCache dbCache, IFileStorageMemoryCache memoryCache)
        : base(dbCache, memoryCache) { }

    public AvaloniaList<FileObjectNotify> GetFiles(string dir)
    {
        return MemoryCache.GetFiles(dir);
    }
}

public sealed class FileStorageMemoryCache
    : MemoryCache<FileObjectNotify, NeotomaPostRequest, NeotomaGetResponse>,
        IFileStorageMemoryCache
{
    private readonly Dictionary<string, AvaloniaList<FileObjectNotify>> _files = new();

    public override ConfiguredValueTaskAwaitable UpdateAsync(
        NeotomaPostRequest source,
        CancellationToken ct
    )
    {
        Dispatcher.UIThread.Post(() =>
        {
            foreach (var id in source.Deletes)
            {
                var item = GetItem(id);
                Items.Remove(id);
                _files[item.Dir].Remove(item);
            }
        });

        return TaskHelper.ConfiguredCompletedTask;
    }

    public override ConfiguredValueTaskAwaitable UpdateAsync(
        NeotomaGetResponse source,
        CancellationToken ct
    )
    {
        Dispatcher.UIThread.Post(() =>
        {
            foreach (var item in source.GetFiles)
            {
                var files = GetFiles(item.Key);

                files.UpdateOrder(
                    item.Value.OrderBy(x => x.Name).Select(x => UpdateItem(x, item.Key)).ToArray()
                );
            }
        });

        return TaskHelper.ConfiguredCompletedTask;
    }

    public AvaloniaList<FileObjectNotify> GetFiles(string dir)
    {
        if (_files.TryGetValue(dir, out var files))
        {
            return files;
        }

        files = new();
        _files[dir] = files;

        return files;
    }

    private FileObjectNotify UpdateItem(FileData data, string dir)
    {
        var item = GetItem(data.Id);
        item.Name = data.Name;
        item.Description = data.Description;
        item.Data = data.Data;
        item.Status = FileObjectNotifyStatus.Updated;
        item.Dir = dir;

        return item;
    }
}
