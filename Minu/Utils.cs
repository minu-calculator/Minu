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
        static public int GetCharacterPerLine(TextEditor editor) {
            var charWidth = new FormattedText(
                ".",
                System.Globalization.CultureInfo.CurrentCulture,
                System.Windows.FlowDirection.LeftToRight,
                new Typeface(editor.FontFamily, editor.FontStyle, editor.FontWeight, editor.FontStretch),
                editor.FontSize,
                Brushes.Black,
                new NumberSubstitution(),
                TextFormattingMode.Ideal).Width;
            return (int)(editor.TextArea.TextView.ActualWidth / charWidth);
        }
    }
}
