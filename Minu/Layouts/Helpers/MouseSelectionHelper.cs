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
        public class OnClickEventArg :EventArgs {
            public string LineContent { get; }
            public int LineNumber { get; }

            public OnClickEventArg(string content, int number)
            {
                LineContent = content;
                LineNumber = number;
            }
        }
        public delegate void OnClickEventHandler(object sender, OnClickEventArg e);
        public event OnClickEventHandler OnClickEvent;

        Dictionary<int, VisualLineInfo> visualLineInfoCache = new Dictionary<int, VisualLineInfo>();
        TextEditor output;
        int lastHighlightedLine = -1;
        bool lastClicked = false;

        public MouseSelectionHelper(TextEditor output) {
            this.output = output;
        }

        public void MouseMove(EventArgs e) {
            if (output.LineCount <= 0) return;

            EnsureCache();

            // Estimate the line number according to cursor position
            Point p = Mouse.GetPosition(output);
            int lineNum = (int)Math.Ceiling(p.Y / output.TextArea.TextView.Options.LineHeight);

            if (lineNum <= 0 || lineNum > output.LineCount) {
                if (lastHighlightedLine != -1) {
                    var offset = visualLineInfoCache[lastHighlightedLine].Offset;
                    output.TextArea.Document.Remove(offset, 1 + (lastClicked ? 1 : 0));
                    lastHighlightedLine = -1;
                    lastClicked = false;
                }
                return;
            }

            // Calculated if the line is actually selected.
            var visualLine = visualLineInfoCache[lineNum];
            bool validSelection = visualLine.Length != 1 && visualLine.XStart <= p.X && p.X <= visualLine.XEnd;

            // If (1) anything is highlighted and (2) the current line is not to be highlighted,
            // remove the old highlight mark
            if (lastHighlightedLine != -1 && (lastHighlightedLine != lineNum || !validSelection)) {
                var offset = visualLineInfoCache[lastHighlightedLine].Offset;
                output.TextArea.Document.Remove(offset, 1 + (lastClicked ? 1 : 0));
                lastHighlightedLine = -1;
                lastClicked = false;
            }

            Mouse.OverrideCursor = validSelection ? Cursors.Hand : Cursors.Arrow;

            if (!validSelection) return; // if nothing is selected, do nothing

            if (lineNum != lastHighlightedLine) // need to highlight
                output.TextArea.Document.Insert(visualLine.Offset, "\u2000");
            lastHighlightedLine = lineNum;

            bool nowClicked = ((MouseEventArgs)e).LeftButton == MouseButtonState.Pressed;

            if (lastClicked && !nowClicked) // unclick
                output.TextArea.Document.Remove(visualLine.Offset, 1);
            else if (!lastClicked && nowClicked) { //click
                OnClickEvent?.Invoke(this, new OnClickEventArg(
                    output.TextArea.Document.GetText(visualLine.Offset + 1, visualLine.Length - 1),
                    lineNum));
                output.TextArea.Document.Insert(visualLine.Offset, "\u200B");
            }

            lastClicked = nowClicked;
        }

        public void InvalidateCache() {
            visualLineInfoCache.Clear();
            lastHighlightedLine = -1;
        }

        public void EnsureCache() {

            // Rebuild cache if hasn't yet
            if (visualLineInfoCache.Count == 0) {
                for (int i = 1; i <= output.TextArea.Document.LineCount; ++i)
                    visualLineInfoCache.Add(i, new VisualLineInfo(output, i));
            }

        }

    }
}
