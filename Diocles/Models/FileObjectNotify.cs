using System.ComponentModel;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using Gaia.Services;

namespace Diocles.Models;

public sealed partial class FileObjectNotify
    : ObservableObject,
        IStaticFactory<Guid, FileObjectNotify>
{
    public FileObjectNotify(Guid id)
    {
        Id = id;
        _description = string.Empty;
        _name = string.Empty;
        _dir = string.Empty;
        _data = [];
    }

    public Guid Id { get; }
    public IImage? Image => _image;
    public FileObjectNotifyType Type => ParseType();

    public static FileObjectNotify Create(Guid input)
    {
        return new(input);
    }

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        switch (e.PropertyName)
        {
            case nameof(Name):
                OnPropertyChanged(nameof(Type));

                break;
            case nameof(Type):
            {
                UpdateImage();

                break;
            }
            case nameof(Data):
            {
                UpdateImage();

                break;
            }
        }
    }

    private Bitmap? _image;

    [ObservableProperty]
    private string _name;

    [ObservableProperty]
    private string _description;

    [ObservableProperty]
    private byte[] _data;

    [ObservableProperty]
    private string _dir;

    [ObservableProperty]
    private FileObjectNotifyStatus _status;

    private void UpdateImage()
    {
        _image?.Dispose();

        if (Data.Length == 0)
        {
            _image = null;

            return;
        }

        if (Type != FileObjectNotifyType.Image)
        {
            _image = null;

            return;
        }

        using var stream = new MemoryStream(Data);
        stream.Position = 0;
        _image = new(stream);
        OnPropertyChanged(nameof(Image));
    }

    private FileObjectNotifyType ParseType()
    {
        var extension = Path.GetExtension(Name);

        return extension.ToUpperInvariant() switch
        {
            ".JPEG"
            or ".JPG"
            or ".PNG"
            or ".WEBI"
            or ".APNG"
            or ".AVIF"
            or ".GIF"
            or ".JFIF"
            or ".PJP"
            or ".SVG"
            or ".BMP"
            or ".ICO"
            or ".TIFF"
            or ".WEBP"
            or ".PJPEG" => FileObjectNotifyType.Image,
            ".PDF" => FileObjectNotifyType.Pdf,
            _ => FileObjectNotifyType.Unknown,
        };
    }
}

public enum FileObjectNotifyType
{
    Unknown,
    Image,
    Pdf,
}

public enum FileObjectNotifyStatus
{
    Updated,
    Deleted,
    Added,
}
