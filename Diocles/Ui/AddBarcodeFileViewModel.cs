using CommunityToolkit.Mvvm.ComponentModel;
using Inanna.Models;
using Inanna.Services;
using Inanna.Ui;

namespace Diocles.Ui;

public sealed partial class AddBarcodeFileViewModel : ViewModelBase
{
    public AddBarcodeFileViewModel(IInannaViewModelFactory factory)
    {
        LinearBarcodeGenerator = factory.CreateLinearBarcodeGenerator();
        _fileName = "barcode";
    }

    public LinearBarcodeGeneratorViewModel LinearBarcodeGenerator { get; }

    [ObservableProperty]
    private string _fileName;
}
