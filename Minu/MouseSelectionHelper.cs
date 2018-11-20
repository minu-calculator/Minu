using ICSharpCode.AvalonEdit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Minu {
    class MouseSelectionHelper {

        Dictionary<int, VisualLineInfo> visualLineInfoCache = new Dictionary<int, VisualLineInfo>();
        TextEditor output;
        int lastHighlightedLine = -1;

        public MouseSelectionHelper(TextEditor output) {
            this.output = output;
        }

        public void MouseMove() {

            if (output.LineCount <= 0) return;

            // Estimate the line number according to cursor position
            Point p = Mouse.GetPosition(output);
            int lineNum = (int)Math.Ceiling(p.Y / output.TextArea.TextView.Options.LineHeight);

            if (lineNum <= 0 || lineNum > output.LineCount) {
                if (lastHighlightedLine != -1) {
                    output.TextArea.Document.Remove(visualLineInfoCache[lastHighlightedLine].Offset, 2);
                    lastHighlightedLine = -1;
                }
                return;
            }

            if (output.TextArea.Document.GetLineByNumber(lineNum).Length == 0) return;

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

        public void InvalidateCache() {
            visualLineInfoCache.Clear();
            lastHighlightedLine = -1;
        }

    }
}
