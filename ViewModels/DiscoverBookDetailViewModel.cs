using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PAGELY.Models;
using PAGELY.Services;

namespace PAGELY.ViewModels;

public partial class DiscoverBookDetailViewModel : ObservableObject
{
    private readonly IOnlineCatalogService _catalog;
    private readonly IReadingListService _readingList;

    public DiscoverBookDetailViewModel(IOnlineCatalogService catalog, IReadingListService readingList)
    {
        _catalog = catalog;
        _readingList = readingList;
    }

    [ObservableProperty]
    private DiscoverBook? book;

    [ObservableProperty]
    private bool isBusy;

    [ObservableProperty]
    private bool isSaved;

    public async Task LoadAsync(string id)
    {
        try
        {
            IsBusy = true;
            Book = await _catalog.GetBookAsync(id);
            if (Book is not null)
            {
                IsSaved = await _readingList.ContainsAsync(Book.Id);
            }
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task ToggleSaveAsync()
    {
        if (Book is null)
        {
            return;
        }

        if (IsSaved)
        {
            await _readingList.RemoveAsync(Book.Id);
            IsSaved = false;
        }
        else
        {
            await _readingList.AddAsync(Book);
            IsSaved = true;
        }
    }
}
