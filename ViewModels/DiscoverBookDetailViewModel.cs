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
    private readonly AuthService _auth;

    public DiscoverBookDetailViewModel(IOnlineCatalogService catalog, IReadingListService readingList, ReviewsService reviews, AuthService auth)
    {
        _catalog = catalog;
        _readingList = readingList;
        _reviews = reviews;
        _auth = auth;
    }

    [ObservableProperty]
    private DiscoverBook? book;

    [ObservableProperty]
    private bool isBusy;

    [ObservableProperty]
    private bool isSubmitting;

    [ObservableProperty]
    private bool isSaved;

    [ObservableProperty]
    private string? existingReviewId;

    [ObservableProperty]
    private bool hasExistingReview;

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
                review.IsOwnReview = _auth.UserId is not null && review.UserId == _auth.UserId;
                Reviews.Add(review);
            }

            var mine = _auth.UserId is null ? null : reviews.FirstOrDefault(r => r.UserId == _auth.UserId);
            ExistingReviewId = mine?.Id.ToString();
            HasExistingReview = mine is not null;
            if (mine is not null)
            {
                NewRating = mine.Rating;
                NewReviewText = mine.ReviewText ?? string.Empty;
            }
        }
        catch (Exception ex)
        {
            ReviewStatus = $"Could not load reviews: {ex.Message}";
        }
    }

    [RelayCommand]
    private void EditReview(Review? review)
    {
        if (review is null)
        {
            return;
        }

        NewRating = review.Rating;
        NewReviewText = review.ReviewText ?? string.Empty;
        ExistingReviewId = review.Id.ToString();
        HasExistingReview = true;
        ReviewStatus = "Editing your review.";
    }

    [RelayCommand]
    private async Task LikeReviewAsync(Review? review)
    {
        if (review is null || review.IsLiking)
        {
            return;
        }

        review.IsLiking = true;
        try
        {
            if (review.IsLikedByCurrentUser)
            {
                await _reviews.UnlikeReviewAsync(review.Id.ToString());
                review.IsLikedByCurrentUser = false;
                if (review.LikeCount > 0)
                {
                    review.LikeCount--;
                }
            }
            else
            {
                await _reviews.LikeReviewAsync(review.Id.ToString());
                review.IsLikedByCurrentUser = true;
                review.LikeCount++;
            }
        }
        catch (Exception ex)
        {
            ReviewStatus = $"Could not update like: {ex.Message}";
        }
        finally
        {
            review.IsLiking = false;
        }
    }

    [RelayCommand]
    private async Task ReportReviewAsync(Review? review)
    {
        if (review is null)
        {
            return;
        }

        if (!_auth.IsAuthenticated)
        {
            ReviewStatus = "Sign in to report a review.";
            return;
        }

        var reason = await Shell.Current.DisplayActionSheet("Report review", "Cancel", null,
            "Spam", "Harassment", "Inappropriate content", "Misinformation", "Other");
        if (string.IsNullOrWhiteSpace(reason) || reason == "Cancel")
        {
            return;
        }

        try
        {
            IsSubmitting = true;
            await _reviews.ReportReviewAsync(review.Id.ToString(), reason);
            ReviewStatus = "Report submitted.";
        }
        catch (Exception ex)
        {
            ReviewStatus = $"Could not report review: {ex.Message}";
        }
        finally
        {
            IsSubmitting = false;
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
        if (Book is null || IsSubmitting)
        {
            return;
        }

        if (NewRating < 1 || NewRating > 5)
        {
            ReviewStatus = "Choose a rating from 1 to 5.";
            return;
        }

        try
        {
            IsSubmitting = true;
            Review updated;
            if (ExistingReviewId is not null)
            {
                updated = await _reviews.UpdateReviewAsync(ExistingReviewId, NewRating, NewReviewText);
            }
            else
            {
                updated = await _reviews.CreateReviewAsync(Book.Id, NewRating, NewReviewText);
            }
            ExistingReviewId = updated.Id.ToString();
            HasExistingReview = true;
            NewReviewText = string.Empty;
            NewRating = 5;
            ReviewStatus = "Review submitted.";
            await LoadReviewsAsync();
        }
        catch (Exception ex)
        {
            ReviewStatus = ex.Message;
        }
        finally
        {
            IsSubmitting = false;
        }
    }

    [RelayCommand]
    private void SetRating(string? rating)
    {
        if (int.TryParse(rating, out var value) && value >= 1 && value <= 5)
        {
            NewRating = value;
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
