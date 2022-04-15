using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MvvmGui.ViewModels;

internal class MakeROMSelectViewModel : ViewModelBase
{
    private string romLocation = "";
    public string ROMLocation
    {
        get => romLocation;
        set
        {
            romLocation = value;
            OnPropertyChanged(nameof(ROMLocation));
        }
    }
}
