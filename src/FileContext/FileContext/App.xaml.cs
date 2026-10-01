using System.Windows;
using FileContext.Data;
using FileContext.Services;

namespace FileContext;

public partial class App : Application
{
    protected override async void OnStartup(StartupEventArgs e)
    {
        Database.Initialize();

        if (!EverythingService.IsEsAvailable())
        {
            MessageBox.Show(
                "FileContext için gerekli Everything CLI (es.exe) bulunamadı.",
                "Eksik Bağımlılık",
                MessageBoxButton.OK,
                MessageBoxImage.Error);

            Shutdown();
            return;
        }

        bool everythingReady =
            await EverythingService.EnsureEverythingRunningAsync();

        if (!everythingReady)
        {
            MessageBox.Show(
                "Everything bulunamadı veya başlatılamadı.\n\n" +
                "FileContext çalışmak için Everything'e ihtiyaç duyuyor.",
                "Everything Gerekli",
                MessageBoxButton.OK,
                MessageBoxImage.Error);

            Shutdown();
            return;
        }

        base.OnStartup(e);
    }
}