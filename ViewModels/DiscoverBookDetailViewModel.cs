using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PAGELY.Models;
using PAGELY.Services;

namespace PAGELY.ViewModels;

public partial class DiscoverBookDetailViewModel : ObservableObject
{
    private readonly IOnlineCatalogService _catalog;
    private readonly IReadingListService _readingList;
    private readonly ReviewsService _reviews;

    public DiscoverBookDetailViewModel(IOnlineCatalogService catalog, IReadingListService readingList, ReviewsService reviews)
    {
        _catalog = catalog;
        _readingList = readingList;
        _reviews = reviews;
    }

    [ObservableProperty]
    private DiscoverBook? book;

    [ObservableProperty]
    private bool isBusy;

    [ObservableProperty]
    private bool isSaved;

    public System.Collections.ObjectModel.ObservableCollection<Models.Review> Reviews { get; } = new();

    [ObservableProperty]
    private int newRating = 5;

    [ObservableProperty]
    private string newReviewText = string.Empty;

    [ObservableProperty]
    private string reviewStatus = string.Empty;

    public async Task LoadAsync(string id)
    {
        try
        {
            IsBusy = true;
            Book = await _catalog.GetBookAsync(id);
            if (Book is not null)
            {
                IsSaved = await _readingList.ContainsAsync(Book.Id);
                await LoadReviewsAsync();
            }
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task LoadReviewsAsync()
    {
        if (Book is null)
        {
            return;
        }

        try
        {
            var reviews = await _reviews.GetReviewsAsync(Book.Id);
            Reviews.Clear();
            foreach (var review in reviews)
            {
                Reviews.Add(review);
            }
        }
        catch (Exception ex)
        {
            ReviewStatus = $"Could not load reviews: {ex.Message}";
        }
    }

    [RelayCommand]
    private void CancelReview()
    {
        NewRating = 5;
        NewReviewText = string.Empty;
        ReviewStatus = string.Empty;
    }

    [RelayCommand]
    private async Task SubmitReviewAsync()
    {
        if (Book is null)
        {
            return;
        }

        try
        {
            IsBusy = true;
            await _reviews.CreateReviewAsync(Book.Id, NewRating, NewReviewText);
            NewReviewText = string.Empty;
            ReviewStatus = "Review submitted.";
            await LoadReviewsAsync();
        }
        catch (Exception ex)
        {
            ReviewStatus = ex.Message;
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
