using Microsoft.EntityFrameworkCore;
using nineth1ngs.Data;
using nineth1ngs.Services;
using System.Windows;

namespace nineth1ngs;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
	protected override async void OnStartup(StartupEventArgs e)
	{
		base.OnStartup(e);

		try
		{
			await using var database = new Th1ngDbContext();
			await database.Database.MigrateAsync();

			var settingsService = new WindowSettingsService();
			var mainWindow = new MainWindow(new Th1ngStore(), settingsService, settingsService.Load());
			MainWindow = mainWindow;
			mainWindow.Show();
		}
		catch (Exception exception)
		{
			MessageBox.Show(
				$"nineth1ngs could not start its local database.\n\n{exception.Message}",
				"nineth1ngs",
				MessageBoxButton.OK,
				MessageBoxImage.Error);
			Shutdown(1);
		}
	}
}

