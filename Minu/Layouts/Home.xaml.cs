using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using ICSharpCode.AvalonEdit.Highlighting;
using MahApps.Metro.Controls;

namespace Minu {
    /// <summary>
    /// Interaction logic for Home.xaml
    /// </summary>
    public partial class Home
    {
        private UIHelper helper;

        public Home() {
            InitializeComponent();
        }

        private void settings_Clicked(object sender, RoutedEventArgs e) {
            SettingsWindow settingsWindow = new SettingsWindow();
            settingsWindow.Owner = this;
            settingsWindow.ShowDialog();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e) {
            ReloadUI();
        }

        public void ReloadUI() {
            helper?.Deactivate();
            helper = new UIHelper(new Calculator.Calculator(), input, output, outputColumn, splitter, this);
        }
    }
}
