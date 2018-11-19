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
        Dictionary<int, VisualLineInfo> visualLineInfoCache = new Dictionary<int, VisualLineInfo>();
        int lastHighlightedLine = -1;

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

            // Estimate the line number according to cursor position
            Point p = Mouse.GetPosition(output);
            int lineNum = (int)Math.Ceiling(p.Y / baseLineHeight);

            if (lineNum <= 0 || lineNum > output.LineCount) {
                if (lastHighlightedLine != -1) {
                    output.TextArea.Document.Remove(visualLineInfoCache[lastHighlightedLine].Offset, 2);
                    lastHighlightedLine = -1;
                }
                return;
            }

            // Rebuild cache if hasn't yet
            if (visualLineInfoCache.Count == 0) {
                for (int i = 1; i <= output.TextArea.Document.LineCount; ++i)
                    visualLineInfoCache.Add(i, new VisualLineInfo(output, i));
            }

            // Calculated if the line is actually selected.
            var visualLine = visualLineInfoCache[lineNum];
            bool validSelection = visualLine.Length != 0 && visualLine.XStart <= p.X && p.X <= visualLine.XEnd;

            // If (1) anything is highlighted and (2) the current line is not to be highlighted,
            // remove the old highlight mark
            if (lastHighlightedLine != -1 && (lastHighlightedLine != lineNum || !validSelection)) {
                output.TextArea.Document.Remove(visualLineInfoCache[lastHighlightedLine].Offset, 2);
                lastHighlightedLine = -1;
            }

            if (lineNum == lastHighlightedLine) return; // if already selected, do nothing
            if (!validSelection) return; // if nothing is selected, do nothing
            
            output.TextArea.Document.Insert(visualLine.Offset, "\u2000\u2000");

            lastHighlightedLine = lineNum;
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

            visualLineInfoCache.Clear();
            lastHighlightedLine = -1;

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
