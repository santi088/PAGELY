using PAGELY.Services;

namespace PAGELY;

public partial class App : Application
{
	public App()
	{
		InitializeComponent();
		ThemeService.Apply(ReaderSettings.Theme);

		MainPage = new AppShell();
	}
}
