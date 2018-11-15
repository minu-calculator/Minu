using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using org.mariuszgromada.math.mxparser;
using Expression = org.mariuszgromada.math.mxparser.Expression;

namespace Minu
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void textChangedEventHandler(object sender, TextChangedEventArgs args)
        {
            output.Text = "";

            string rawInput = (new TextRange(input.Document.ContentStart, input.Document.ContentEnd)).Text;
            string[] inputs = rawInput.Replace("\r", "").Split('\n');

            foreach (string input in inputs)
            {
                if (input != "")
                    output.Text += new Expression(input).calculate();
                output.Text += "\n";
            }
        }

    }
}
