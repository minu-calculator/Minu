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

        private void Window_Loaded(object sender, RoutedEventArgs e) {
            helper = new UIHelper(new Calculator.Calculator(), input, output, outputColumn, splitter, 28);
        }
    }
}
