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
    }
}
