using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PAGELY.Models;
using PAGELY.Services;

namespace PAGELY.ViewModels;

public partial class ReadingListViewModel : ObservableObject
{
    private readonly IReadingListService _readingList;

    public ReadingListViewModel(IReadingListService readingList)
    {
        _readingList = readingList;
        _readingList.Changed += (_, _) => _ = LoadAsync();
    }

    public ObservableCollection<DiscoverBook> Items { get; } = [];

    [ObservableProperty]
    private bool isEmpty;

    [RelayCommand]
    public async Task LoadAsync()
    {
        var items = await _readingList.GetAsync();
        Items.Clear();
        foreach (var item in items)
        {
            Items.Add(item);
        }

        IsEmpty = Items.Count == 0;
    }

    [RelayCommand]
    private async Task RemoveAsync(DiscoverBook? book)
    {
        if (book is null)
        {
            return;
        }

        await _readingList.RemoveAsync(book.Id);
        await LoadAsync();
    }

    [RelayCommand]
    private async Task OpenAsync(DiscoverBook? book)
    {
        if (book is null)
        {
            return;
        }

        await Shell.Current.GoToAsync($"discoverbookdetail?BookId={book.Id}");
    }
}
