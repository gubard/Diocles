using Avalonia;
using Avalonia.Collections;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Diocles.Models;
using Diocles.Services;
using Gaia.Helpers;
using Gaia.Services;
using Inanna.Helpers;
using Inanna.Models;
using Inanna.Services;

namespace Diocles.Ui;

public partial class FilesViewModel : ViewModelBase
{
    public FilesViewModel(
        AvaloniaList<FileObjectNotify> files,
        FileObjectNotify selectedFile,
        IFileStorageUiService fileStorageUiService,
        Application app,
        IAppResourceService appResourceService,
        IStringFormater stringFormater
    )
    {
        Files = files;
        _selectedFile = selectedFile;
        _fileStorageUiService = fileStorageUiService;
        _app = app;
        _appResourceService = appResourceService;
        _stringFormater = stringFormater;
    }

    public AvaloniaList<FileObjectNotify> Files { get; }

    private readonly IFileStorageUiService _fileStorageUiService;
    private readonly Application _app;
    private readonly IAppResourceService _appResourceService;
    private readonly IStringFormater _stringFormater;

    [ObservableProperty]
    private FileObjectNotify _selectedFile;

    [RelayCommand]
    private void NextFile()
    {
        var index = Files.IndexOf(SelectedFile);

        if (index == Files.Count - 1)
        {
            return;
        }

        if (index == -1)
        {
            return;
        }

        WrapCommand(() => Dispatcher.UIThread.Post(() => SelectedFile = Files[index + 1]));
    }

    [RelayCommand]
    private void PreviousFile()
    {
        var index = Files.IndexOf(SelectedFile);

        if (index == 0)
        {
            return;
        }

        if (index == -1)
        {
            return;
        }

        WrapCommand(() => Dispatcher.UIThread.Post(() => SelectedFile = Files[index - 1]));
    }

    [RelayCommand]
    private async Task DeleteFileAsync(CancellationToken ct)
    {
        await WrapCommandAsync(
            () =>
                _fileStorageUiService.PostAsync(
                    Guid.NewGuid(),
                    new() { Deletes = [SelectedFile.Id] },
                    ct
                ),
            ct
        );
    }

    [RelayCommand]
    private async Task SaveAsync(CancellationToken ct)
    {
        await WrapCommandAsync(
            async () =>
            {
                var file = await _app.GetTopLevel()
                    .ThrowIfNull()
                    .StorageProvider.SaveFilePickerAsync(
                        new()
                        {
                            Title = _stringFormater.Format(
                                _appResourceService.GetResource<string>("Lang.SaveItem"),
                                SelectedFile.Name
                            ),
                            SuggestedFileName = SelectedFile.Name,
                        }
                    );

                if (file is null)
                {
                    return;
                }

                await using var stream = await file.OpenWriteAsync();
                await stream.WriteAsync(SelectedFile.Data, ct);
            },
            ct
        );
    }
}
