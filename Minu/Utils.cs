using ICSharpCode.AvalonEdit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Minu
{
    class Utils
    {
        // Returns an array that contains the number of lines visually presented
        // in each input line after wrapping
        static public int[] GetWrappedLineCount(TextEditor editor, double baseLineHeight) {
            var visualLineNum = new int[editor.LineCount];
            for (int i = 0; i < editor.LineCount; i++) {
                double bottom = 0;
                double top = editor.TextArea.TextView.GetVisualTopByDocumentLine(i + 1);
                if (i + 2 <= editor.LineCount)
                    bottom = editor.TextArea.TextView.GetVisualTopByDocumentLine(i + 2);
                else
                    bottom = editor.TextArea.TextView.DocumentHeight;
                visualLineNum[i] = (int)Math.Round((bottom - top) / baseLineHeight);
            }
            return visualLineNum;
        }
    }
}
