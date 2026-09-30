using System.Windows;
using FileContext.Data;

namespace FileContext;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        Database.Initialize();

        base.OnStartup(e);
    }
}