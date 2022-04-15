using MvvmGui.ViewModels;

namespace MvvmGui.Stores;

internal class NavigationStore
{
    private ViewModelBase? curentViewModel;

    public ViewModelBase? CurrentViewModel
    {
        get => curentViewModel;
        set
        {
            curentViewModel = value;
            OnCurrentViewModelChanged();
        }
    }

    private void OnCurrentViewModelChanged() => CurrentViewModelChanged?.Invoke();

    public event Action? CurrentViewModelChanged;
}
