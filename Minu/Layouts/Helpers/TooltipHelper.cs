using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using ICSharpCode.AvalonEdit;
using ToolTip = System.Windows.Controls.ToolTip;

namespace Minu.Layouts.Helpers {
    class TooltipHelper {
        private ToolTip toolTip = new ToolTip();
        private TextEditor editor;
        private Calculator.Calculator calculator;
        private Window window;

        public TooltipHelper(TextEditor editor, Calculator.Calculator calculator, Window window)
        {
            this.editor = editor;
            this.calculator = calculator;
            this.window = window;
            editor.MouseHover += TextEditorMouseHover;
            editor.MouseHoverStopped += TextEditorMouseHoverStopped;
            editor.MouseEnter += (s,e) => Mouse.OverrideCursor = Cursors.IBeam;
        }

        void TextEditorMouseHover(object sender, MouseEventArgs e) {
            bool isValidWordPart(char c)
            {
                return char.IsLetterOrDigit(c) || c == '_';
            }

            var pos = editor.GetPositionFromPoint(e.GetPosition(editor));
            if (pos == null) return;
            
            var line = editor.Document.GetText(editor.Document.GetLineByNumber(pos.Value.Line));
            int start = pos.Value.VisualColumn, end = pos.Value.VisualColumn;

            if (start < 0 || end >= line.Length || !isValidWordPart(line[start])) return;

            while (start - 1 >= 0 && isValidWordPart(line[start - 1])) start--;
            while (end + 1 < line.Length && isValidWordPart(line[end + 1])) end++;
            var token = line.Substring(start, end - start + 1);

            if (calculator.GetVariableValue(token, out var value))
            {
                toolTip.PlacementTarget = window; // required for property inheritance
                toolTip.Content = value;
                toolTip.IsOpen = true;
                e.Handled = true;
            }
        }

        void TextEditorMouseHoverStopped(object sender, MouseEventArgs e) {
            toolTip.IsOpen = false;
        }

    }
}
