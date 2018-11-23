using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Xml;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using MahApps.Metro.Controls;

namespace Minu {
    class UIHelper {
        public TextEditor Input { get; }
        public TextEditor Output { get; }
        public Calculator.Calculator Calculator { get; }
        public MouseSelectionHelper SelectionHelper { get; set; }
        public double BaseLineHeight { get; set; }

        private ColumnDefinition outputColumn { get; }
        private GridSplitter splitter { get; }

        public UIHelper(Calculator.Calculator calculator, TextEditor input, TextEditor output,
            ColumnDefinition outputColumn, GridSplitter splitter, double baseLineHeight = 28)
        {
            Calculator = calculator;
            Input = input;
            Output = output;
            BaseLineHeight = baseLineHeight;
            this.outputColumn = outputColumn;
            this.splitter = splitter;

            LoadHighlightRule("Resources.HighlightRules.minu.xshd", "minu");
            LoadHighlightRule("Resources.HighlightRules.output.xshd", "output");

            input.SyntaxHighlighting = HighlightingManager.Instance.GetDefinition("minu");
            output.SyntaxHighlighting = HighlightingManager.Instance.GetDefinition("output");

            input.TextArea.TextView.Options.LineHeight = BaseLineHeight;
            output.TextArea.TextView.Options.LineHeight = BaseLineHeight;
            output.TextArea.TextView.Options.Alignment = TextAlignment.Right;

            SelectionHelper = new MouseSelectionHelper(output);
            SelectionHelper.OnClickEvent += (s, arg) => {
                Clipboard.SetText(arg.LineContent);
            };
            input.SizeChanged += (s, e) => ReCalculate();
            input.TextChanged += (s, e) => ReCalculate();

            input.PreviewMouseWheel += previewMouseWheel;
            output.PreviewMouseWheel += previewMouseWheel;
            output.MouseMove += (s, e) => SelectionHelper.MouseMove(e);
            output.MouseLeave += (s, e) => SelectionHelper.MouseMove(e);

            output.TextArea.SelectionChanged += (s, e) => {
                output.TextArea.ClearSelection();
                output.TextArea.Caret.Hide();
            };
            output.TextArea.MouseSelectionMode = ICSharpCode.AvalonEdit.Editing.MouseSelectionMode.None;

            // Debug
            input.Text = "square_root(x)=sqrt(x)\n" +
                         "va = square_root(2*6.21*3.77*10^5) \n" +
                         "pr = .01*2*va\n" +
                         "vnew = pr/2\n" +
                         "ta = va/6.21\n" +
                         "g = 9.8\n" +
                         "vland = tan(vnew^2+2*9.8*1.6*10^4)\n" +
                         "F = 2*vland/.5\n" +
                         "tl = (vland - vnew)/g \n" +
                         "F/.15*100000\n" +
                         "tpfff = (−.5*vland+ square_root((0.5*vland)^2−4*. 5*g*−35))/2*.5*g\n" +
                         "tp = sin(35/(.5*g)) \n" +
                         ".5*vland*tp \n" +
                         "ta+tl+tp+30+.5\n" +
                         "va\n" +
                         "vnew";
        }

        public void LoadHighlightRule(string resourceName, string ruleName) {
            IHighlightingDefinition customHighlighting;
            using (Stream s = typeof(MainWindow).Assembly.GetManifestResourceStream("Minu." + resourceName)) {
                if (s == null)
                    throw new InvalidOperationException("Could not find embedded resource");
                using (XmlReader reader = new XmlTextReader(s)) {
                    customHighlighting = HighlightingLoader.Load(reader, HighlightingManager.Instance);
                }
            }
            HighlightingManager.Instance.RegisterHighlighting(ruleName, new[] { "." + ruleName }, customHighlighting);
        }

        public void ReCalculate() {
            if (!Input.TextArea.TextView.VisualLinesValid)
                Input.TextArea.TextView.EnsureVisualLines();

            var resultList = Calculator.Calculate(Input.Text);

            var outputText = "";
            for (int i = 0; i < resultList.Count; ++i) {
                // Line up to input
                int bound = (int)(Input.TextArea.TextView.VisualLines[i].Height / BaseLineHeight);
                // One less '\n' on the first line
                if (i == 0) bound--;
                outputText += (bound > 0 ? (new string('\n', bound)) : "") + resultList[i] + "\u2001";
            }
            Output.Text = outputText;
            SelectionHelper?.InvalidateCache();

            // Show the splitter if necessary
            bool outputOverflowed = (outputColumn.ActualWidth - Output.ActualWidth - Output.Margin.Left - Output.Margin.Right) <= 10;
            if (outputOverflowed || Calculator.isInputOverflowed)
                splitter.Visibility = Visibility.Visible;
            else
                splitter.Visibility = Visibility.Collapsed;
        }

        private void previewMouseWheel(object sender, MouseWheelEventArgs e) {
            if (!e.Handled) {
                e.Handled = true;
                var eventArg = new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta);
                eventArg.RoutedEvent = UIElement.MouseWheelEvent;
                eventArg.Source = sender;
                var parent = ((Control)sender).Parent as UIElement;
                parent.RaiseEvent(eventArg);
            }
        }
    }
}
