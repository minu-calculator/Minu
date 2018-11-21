using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Minu {
    class VisualLineInfo {
        public VisualLineInfo(TextEditor editor, int lineNumber) {
            var line = editor.TextArea.Document.GetLineByNumber(lineNumber);
            Length = line.Length;
            Offset = line.Offset;

            var visualLine = editor.TextArea.TextView.GetOrConstructVisualLine(line);
            if (visualLine == null) {
                XStart = 0;
                XEnd = 0;
            }
            else {
                XStart = visualLine.GetVisualPosition(0, VisualYPosition.Baseline).X;
                XEnd = visualLine.GetVisualPosition(line.Length, VisualYPosition.Baseline).X;
            }
        }
        public readonly double XStart, XEnd;
        public readonly int Length, Offset;
    }
}
