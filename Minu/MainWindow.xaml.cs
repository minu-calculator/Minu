using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Rendering;
using org.mariuszgromada.math.mxparser;
using Expression = org.mariuszgromada.math.mxparser.Expression;

namespace Minu {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {

        Calculator calculator = new Calculator();
        double baseLineHeight;

        public MainWindow() {
            InitializeComponent();

            // Debug
            input.Text = "sqrare_root(x)=sqrt(x)\nva = sqrare_root(2*6.21*3.77*10^5) \npr = .01*2*va\nvnew = pr/2\nta = va/6.21\ng = 9.8\nvland = tan(vnew^2+2*9.8*1.6*10^4)\nF = 2*vland/.5\ntl = (vland - vnew)/g \nF/.15*100000\ntpfff = (−.5*vland+ sqrare_root((0.5*vland)^2−4*. 5*g*−35))/2*.5*g\ntp = sin(35/(.5*g)) \n.5*vland*tp \nta+tl+tp+30+.5\nva\nvnew";
        }

        private double measureLineHeight() {
            string backup = input.Text;
            input.Text = "\n";
            double ret = input.TextArea.TextView.DocumentHeight -
                input.TextArea.TextView.GetVisualTopByDocumentLine(input.LineCount);
            input.Text = backup;
            return ret;
        }

        private void output_MouseMove(object sender, EventArgs e) {
            if (output.LineCount <= 0) return;
            output.Text = output.Text.Replace("\u2000", "");

            Point p = Mouse.GetPosition(output);
            int lineNum = (int)Math.Ceiling(p.Y / baseLineHeight);
            if (lineNum > output.LineCount) return;

            var line = output.TextArea.Document.GetLineByNumber(lineNum);
            var visualLine = output.TextArea.TextView.GetOrConstructVisualLine(line);
            if (visualLine == null) return; // ignore invisible lines

            var visualXStart = visualLine.GetVisualPosition(0, VisualYPosition.Baseline).X;
            var visualXEnd = visualLine.GetVisualPosition(line.Length, VisualYPosition.Baseline).X;
            if (p.X < visualXStart || p.X > visualXEnd) return;

            if (line.ToString().Contains("\u2000")) return;
            output.TextArea.Document.Insert(line.Offset, "\u2000\u2000");
        }

        private void recalculate() {
            if (baseLineHeight == 0) return;

            var resultList = calculator.Calculate(input.Text);

            // Calculate visual line count for every input line after wrapping
            var visualLineNums = Utils.GetWrappedLineCount(input, baseLineHeight);

            var outputText = "";
            for (int i = 0; i < resultList.Count; ++i) {
                // Line up to input
                int bound = visualLineNums[i];
                // One less '\n' on the first line
                if (i == 0) bound--;
                outputText += (bound > 0?(new string('\n', bound)):"") + resultList[i] + "\u2002\u2001";
            }
            output.Text = outputText;
            output.InvalidateMeasure();

            output.InvalidateVisual();
            // Show the splitter if necessary
            bool outputOverflowed = (outputColumn.ActualWidth - output.ActualWidth - output.Margin.Left - output.Margin.Right) <= 10;
            if (outputOverflowed || calculator.isInputOverflowed)
                splitter.Visibility = Visibility.Visible;
            else
                splitter.Visibility = Visibility.Collapsed;
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e) {
            base.OnMouseLeftButtonDown(e);
            DragMove();
        }

        private void textChangedEventHandler(object sender, EventArgs args) {
            recalculate();
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e) {
            recalculate();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e) {
            Utils.LoadHighlightRule("minu.xshd", "minu");
            Utils.LoadHighlightRule("output.xshd", "output");
            input.SyntaxHighlighting = HighlightingManager.Instance.GetDefinition("minu");
            output.SyntaxHighlighting = HighlightingManager.Instance.GetDefinition("output");

            output.TextArea.TextView.Options.Alignment = TextAlignment.Right;
            baseLineHeight = measureLineHeight();
            recalculate();
        }
    }
}
