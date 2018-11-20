using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Threading;
using Minu.Backend;
using Minu.Metro.Native;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Windows.Media;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Highlighting;
using org.mariuszgromada.math.mxparser;
using Expression = org.mariuszgromada.math.mxparser.Expression;

namespace Minu {
    /// <summary>
    /// Interaction logic for Home.xaml
    /// </summary>
    public partial class Home {

        enum OutputMode {
            Bin, Oct, Dec, Hex, Sci
        };

        public Home() {
            InitializeComponent();
            DwmDropShadow.DropShadowToWindow(this);

            UpdateTitleText("Minu");
            UpdateStatusText("Ready...");

            // Set width/height/state from last session
            if (!double.IsNaN(Settings.ApplicationSizeHeight))
                Height = Settings.ApplicationSizeHeight;
            if (!double.IsNaN(Settings.ApplicationSizeWidth))
                Width = Settings.ApplicationSizeWidth;
            WindowState = Settings.ApplicationSizeMaximize ? WindowState.Maximized : WindowState.Normal;

            Window_StateChanged(null, null);
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
            return (int)(editor.TextArea.TextView.ActualWidth / charWidth);
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

            bool outputOverflowed = (outputColumn.ActualWidth - output.ActualWidth - output.Margin.Left - output.Margin.Right) <= 10;
            if (outputOverflowed || inputOverflowed)
                splitter.Visibility = Visibility.Visible;
            else
                splitter.Visibility = Visibility.Collapsed;
        }

        private void textChangedEventHandler(object sender, EventArgs args) {
            recalculate();
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e) {
            characterPerLine = -1;
            characterPerLine = getCharacterPerLine(input);
            recalculate();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e) {
            IHighlightingDefinition customHighlighting;
            using (Stream s = typeof(MainWindow).Assembly.GetManifestResourceStream("Minu.minu.xshd")) {
                if (s == null)
                    throw new InvalidOperationException("Could not find embedded resource");
                using (System.Xml.XmlReader reader = new System.Xml.XmlTextReader(s)) {
                    customHighlighting = ICSharpCode.AvalonEdit.Highlighting.Xshd.
                        HighlightingLoader.Load(reader, HighlightingManager.Instance);
                }
            }
            // and register it in the HighlightingManager
            HighlightingManager.Instance.RegisterHighlighting("minu", new string[] { ".minu" }, customHighlighting);

            input.SyntaxHighlighting =
                HighlightingManager.Instance.GetDefinition("minu");
        }

        /*
         * BELOW IS UI TEMPLATE SECTION
        */

        /// <summary>
        /// Set the title text
        /// </summary>
        /// <param name="title">Current Title</param>
        public void UpdateTitleText(string title) {
            this.Title = title.Trim();
            lblTitle.Text = title.Trim();
        }

        /// <summary>
		/// Set the status text at the buttom
        /// </summary>
        /// <param name="status">Current Status Text</param>
        public void UpdateStatusText(string status) {
            this.Status.Text = status;

            _statusUpdateTimer.Stop();
            _statusUpdateTimer.Interval = new TimeSpan(0, 0, 0, 4);
            _statusUpdateTimer.Tick += statusUpdateCleaner_Clear;
            _statusUpdateTimer.Start();
        }
        private void statusUpdateCleaner_Clear(object sender, EventArgs e) {
            this.Status.Text = "Ready...";
        }
        private DispatcherTimer _statusUpdateTimer = new DispatcherTimer();

        private void headerThumb_DragDelta(object sender, DragDeltaEventArgs e) {
            Left = Left + e.HorizontalChange;
            Top = Top + e.VerticalChange;
        }

        private void ResizeDrop_DragDelta(object sender, DragDeltaEventArgs e) {
            double yadjust = this.Height + e.VerticalChange;
            double xadjust = this.Width + e.HorizontalChange;

            if (xadjust > this.MinWidth)
                this.Width = xadjust;
            if (yadjust > this.MinHeight)
                this.Height = yadjust;
        }

        private void Window_StateChanged(object sender, EventArgs e) {
            if (this.WindowState == WindowState.Normal) {
                borderFrame.BorderThickness = new Thickness(1, 1, 1, 23);
                Settings.ApplicationSizeMaximize = false;
                Settings.ApplicationSizeHeight = this.Height;
                Settings.ApplicationSizeWidth = this.Width;
                Settings.UpdateSettings();

                btnActionRestore.Visibility = Visibility.Collapsed;
                btnActionMaxamize.Visibility = ResizeDropVector.Visibility = ResizeDrop.Visibility = System.Windows.Visibility.Visible;
            }
            else if (this.WindowState == WindowState.Maximized) {
                borderFrame.BorderThickness = new Thickness(0, 0, 0, 23);
                Settings.ApplicationSizeMaximize = true;
                Settings.UpdateSettings();

                btnActionRestore.Visibility = Visibility.Visible;
                btnActionMaxamize.Visibility = ResizeDropVector.Visibility = ResizeDrop.Visibility = System.Windows.Visibility.Collapsed;
            }
            /*
             * ResizeDropVector
             * ResizeDrop
             * ResizeRight
             * ResizeBottom
             */
        }
        private void headerThumb_MouseDoubleClick(object sender, MouseButtonEventArgs e) {
            if (this.WindowState == WindowState.Normal)
                this.WindowState = WindowState.Maximized;
            else if (this.WindowState == WindowState.Maximized)
                this.WindowState = WindowState.Normal;
        }
        private void headerThumb_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            base.OnMouseLeftButtonDown(e);
            DragMove();
        }

        private void btnActionSupport_Click(object sender, RoutedEventArgs e) {
            // Load support page?
        }
        private void btnActionMinimize_Click(object sender, System.Windows.RoutedEventArgs e) {
            WindowState = WindowState.Minimized;
        }
        private void btnActionRestore_Click(object sender, System.Windows.RoutedEventArgs e) {
            WindowState = WindowState.Normal;
        }
        private void btnActionMaxamize_Click(object sender, System.Windows.RoutedEventArgs e) {
            WindowState = WindowState.Maximized;
        }
        private void btnActionClose_Click(object sender, System.Windows.RoutedEventArgs e) {
            Application.Current.Shutdown();
        }
        public int OpacityIndex = 0;
        public void ShowMask() {
            OpacityIndex++;
            OpacityMask.Visibility = System.Windows.Visibility.Visible;
        }
        public void HideMask() {
            OpacityIndex--;

            if (OpacityIndex == 0)
                OpacityMask.Visibility = System.Windows.Visibility.Collapsed;
        }
        private void menuCloseApplication_Click(object sender, RoutedEventArgs e) { Application.Current.Shutdown(); }
    }
}
