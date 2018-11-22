using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Editing;

namespace Minu {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        
        private UIHelper helper;

        public MainWindow() {
            InitializeComponent();
        }

        private void settings_Clicked(object sender, RoutedEventArgs e) {
            SettingsWindow settingsWindow = new SettingsWindow();
            settingsWindow.ShowDialog();
        }
        
        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e) {
            base.OnMouseLeftButtonDown(e);
            DragMove();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            helper = new UIHelper(new Calculator(), input, output, outputColumn, splitter, 28);
        }
    }
}
