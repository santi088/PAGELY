using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PAGELY.Models;
using PAGELY.Services;

namespace PAGELY.ViewModels;

public partial class MyReviewsViewModel : ObservableObject
{
    private readonly ReviewsService _reviews;

    public MyReviewsViewModel(ReviewsService reviews)
    {
        _reviews = reviews;
    }

    public ObservableCollection<Review> Reviews { get; } = new();

    [ObservableProperty]
    private bool isEmpty;

    [ObservableProperty]
    private string statusMessage = string.Empty;

    [RelayCommand]
    public async Task LoadAsync()
    {
        try
        {
            var reviews = await _reviews.GetMyReviewsAsync();
            Reviews.Clear();
            foreach (var review in reviews)
            {
                Reviews.Add(review);
            }

            IsEmpty = Reviews.Count == 0;
            StatusMessage = IsEmpty ? "You have not written any reviews yet." : string.Empty;
        }
        catch (Exception ex)
        {
            StatusMessage = ex.Message;
        }
    }
}
