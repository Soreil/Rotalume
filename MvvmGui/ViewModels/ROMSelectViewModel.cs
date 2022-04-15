using MvvmGui.Models;

namespace MvvmGui.ViewModels;

internal class ROMSelectViewModel : ViewModelBase
{
    private readonly ROMSelect romSelect;

    public ROMSelectViewModel(ROMSelect romSelect) => this.romSelect = romSelect;

    public string ROMLocation => romSelect.RomLocation;
}
