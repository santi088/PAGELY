using PAGELY.Views;

namespace PAGELY;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();
        Routing.RegisterRoute("bookdetail", typeof(BookDetailPage));
        Routing.RegisterRoute("discoverbookdetail", typeof(DiscoverBookDetailPage));
        Routing.RegisterRoute("reader", typeof(ReaderPage));
        Routing.RegisterRoute("stats", typeof(StatsPage));
        Routing.RegisterRoute("history", typeof(HistoryPage));
        Routing.RegisterRoute("settings", typeof(SettingsPage));
        Routing.RegisterRoute("saved", typeof(SavedPage));
        Routing.RegisterRoute("readinglist", typeof(ReadingListPage));
        Routing.RegisterRoute("myreviews", typeof(MyReviewsPage));
        Routing.RegisterRoute("verifyemail", typeof(VerifyEmailPage));

#if DEBUG
        Routing.RegisterRoute("gallery", typeof(DesignGalleryPage));
#endif
    }
}
