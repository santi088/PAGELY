using PAGELY.ViewModels;

namespace PAGELY.Views;

public partial class ProfilePage : ContentPage
{
    private readonly ProfileViewModel _viewModel;

    public ProfilePage(ProfileViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;

        var selector = new ProfileTabTemplateSelector
        {
            ProfileTemplate = (DataTemplate)Resources["ProfileTemplate"],
            ReadingListTemplate = (DataTemplate)Resources["ReadingListTemplate"],
            HistoryTemplate = (DataTemplate)Resources["HistoryTemplate"],
            SettingsTemplate = (DataTemplate)Resources["SettingsTemplate"]
        };
        TabsCarousel.ItemTemplate = selector;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        try
        {
            await _viewModel.InitializeAsync();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Profile", ex.Message, "OK");
        }
    }

    private void OnTabTapped(object? sender, TappedEventArgs e)
    {
        if (e.Parameter is string value && int.TryParse(value, out var index))
        {
            _viewModel.SelectedTab = index;
        }
    }

    private sealed class ProfileTabTemplateSelector : DataTemplateSelector
    {
        public DataTemplate? ProfileTemplate { get; set; }
        public DataTemplate? ReadingListTemplate { get; set; }
        public DataTemplate? HistoryTemplate { get; set; }
        public DataTemplate? SettingsTemplate { get; set; }

        protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
        {
            return item switch
            {
                ReadingListViewModel => ReadingListTemplate ?? new DataTemplate(),
                HistoryViewModel => HistoryTemplate ?? new DataTemplate(),
                SettingsViewModel => SettingsTemplate ?? new DataTemplate(),
                _ => ProfileTemplate ?? new DataTemplate()
            };
        }
    }
}
