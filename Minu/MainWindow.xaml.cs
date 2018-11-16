using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
using ICSharpCode.AvalonEdit;
using org.mariuszgromada.math.mxparser;
using Expression = org.mariuszgromada.math.mxparser.Expression;

namespace Minu {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {

        enum OutputMode {
            Bin, Oct, Dec, Hex, Sci
        };

        public MainWindow() {
            InitializeComponent();
        }

        private int characterPerLine = -1;
        private static Regex functionRegex = new Regex(@"\(.*?\)\s*=");

        private int getCharacterPerLine(TextEditor editor) {
            var charWidth = new FormattedText(
                ".",
                System.Globalization.CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                new Typeface(editor.FontFamily, editor.FontStyle, editor.FontWeight, editor.FontStretch),
                editor.FontSize,
                Brushes.Black,
                new NumberSubstitution(),
                TextFormattingMode.Ideal).Width;
            return (int) (editor.TextArea.TextView.ActualWidth / charWidth);
        }

        private string formattedOutput(double val, OutputMode mode) {
            if (mode == OutputMode.Bin) return Convert.ToString((long)val, 2);
            if (mode == OutputMode.Oct) return Convert.ToString((long)val, 8);
            if (mode == OutputMode.Hex) return Convert.ToString((long)val, 16);
            if (mode == OutputMode.Dec) return val.ToString("G");
            if (mode == OutputMode.Sci) return val.ToString("E");
            return val.ToString();
        }

        private void recalculate() {
            if (characterPerLine == -1) return;

            OutputMode outputMode = OutputMode.Dec;

            bool inputOverflowed = false;

            string outputText = "";
            string rawInput = input.Text;
            string[] inputs = rawInput.Replace("\r", "").Split('\n');

            var arguments = new List<Argument>();
            var functions = new List<Function>();
            double ans = 0.0;

            foreach (string input in inputs) {
                if (input.StartsWith("#")) { // comments + settings
                    string trimed = input.Substring(1).TrimStart().ToLower();
                    if (trimed.StartsWith("bin")) outputMode = OutputMode.Bin;
                    else if (trimed.StartsWith("oct")) outputMode = OutputMode.Oct;
                    else if (trimed.StartsWith("dec")) outputMode = OutputMode.Dec;
                    else if (trimed.StartsWith("hex")) outputMode = OutputMode.Hex;
                    else if (trimed.StartsWith("sci")) outputMode = OutputMode.Sci;
                }
                else if (functionRegex.IsMatch(input)) { // functions
                    bool overrided = false;
                    Function func = new Function(input);
                    func.addFunctions(functions.ToArray());
                    if (functions.RemoveAll(f => f.getFunctionName() == func.getFunctionName()) > 0) // override occurred
                        overrided = true;
                    functions.Add(func);
                    outputText += (overrided ? "(*) " : "") + func.getFunctionName();
                }
                else if (input.Contains("=")) { // variables
                    bool overrided = false;
                    Argument arg = new Argument(input, new Argument("ans", ans));
                    arg.addArguments(arguments.ToArray());
                    arg.addFunctions(functions.ToArray());
                    if (arguments.RemoveAll(a => a.getArgumentName() == arg.getArgumentName()) > 0) // override occurred
                        overrided = true;
                    arguments.Add(arg);
                    outputText += (overrided ? "(*) " : "") + arg.getArgumentName() + " = " +
                        formattedOutput(arg.getArgumentValue(), outputMode);
                }
                else if (input != "") { // evaluate
                    var expression = new Expression(input, new Argument("ans", ans));
                    expression.addArguments(arguments.ToArray());
                    expression.addFunctions(functions.ToArray());
                    var result = expression.calculate();
                    if (!double.IsNaN(result)) {
                        ans = result;
                        outputText += formattedOutput(result, outputMode);
                    }
                }
                var inputActualLine = Math.Max(1, (int)Math.Ceiling((double)input.Length / characterPerLine));
                if (inputActualLine > 1)
                    inputOverflowed = true;
                outputText += new string('\n', inputActualLine);
            }
            output.Text = outputText.TrimEnd('\n');
            showSplitterIfNecessary(inputOverflowed);
        }

        private void showSplitterIfNecessary(bool inputOverflowed) {
            double outDifference = outputColumn.ActualWidth - output.ActualWidth - output.Margin.Left - output.Margin.Right;
            bool outOverflowed = outDifference <= 10;
            
            if (outOverflowed || inputOverflowed)
                splitter.Visibility = Visibility.Visible;
            else splitter.Visibility = Visibility.Collapsed;
        }
        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e) {
            base.OnMouseLeftButtonDown(e);
            DragMove();
        }

        private void textChangedEventHandler(object sender, EventArgs args) {
            recalculate();
        }

        private void titleLoaded(object sender, RoutedEventArgs args) {
            MouseDown += delegate { DragMove(); };
        }

        private void btnClose(object sender, RoutedEventArgs e) {
            Close();
        }

        private void btnRest(object sender, RoutedEventArgs e) {
            if (WindowState == WindowState.Maximized)
                WindowState = WindowState.Normal;
            else WindowState = WindowState.Maximized;
        }

        private void btnMini(object sender, RoutedEventArgs e) {
            WindowState = WindowState.Minimized;
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e) {
            characterPerLine = -1;
            characterPerLine = getCharacterPerLine(input);
            recalculate();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e) {

        }
    }
}