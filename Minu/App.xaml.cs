using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Minu.Properties;

namespace Minu {
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application {
        protected override void OnStartup(StartupEventArgs e) {
            StartupUri = new Uri(Settings.Default.mac_style ? "/Layouts/MainWindow.xaml" : "/Layouts/Home.xaml",
                UriKind.Relative);
        }
    }
}
