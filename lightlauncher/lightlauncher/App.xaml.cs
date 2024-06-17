using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace lightlauncher
{
    public partial class App : Application
    {
        private readonly IHost _host;
        /*

                public App()
                {
                    _host = Host.CreateDefaultBuilder().ConfigureServices((context, service) =>
                    {
                        service.AddSingleton<MainWindow>((services) => new MainWindow());
                    }
                    ).Build();
                }
                protected override void OnStartup(StartupEventArgs e)
                {
                    MainWindow = _host.Services.GetRequiredService<MainWindow>();
                    MainWindow.Show();
                    base.OnStartup(e);
                }
        */
    }
}
