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
            editor.MouseEnter += TextEditorMouseEnter;
        }
        
        public void Deactivate()
        {
            editor.MouseHover -= TextEditorMouseHover;
            editor.MouseHoverStopped -= TextEditorMouseHoverStopped;
            editor.MouseEnter -= TextEditorMouseEnter;
        }

        void TextEditorMouseEnter(object sender, MouseEventArgs e) {
            Mouse.OverrideCursor = Cursors.IBeam;
        }
        void TextEditorMouseHover(object sender, MouseEventArgs e) {
            bool IsValidWordPart(char c) => char.IsLetterOrDigit(c) || c == '_';

            var pos = editor.GetPositionFromPoint(e.GetPosition(editor));
            if (pos == null) return;
            
            var line = editor.Document.GetText(editor.Document.GetLineByNumber(pos.Value.Line));
            int start = pos.Value.VisualColumn, end = pos.Value.VisualColumn;

            if (start < 0 || end >= line.Length || !IsValidWordPart(line[start])) return;

            while (start - 1 >= 0 && IsValidWordPart(line[start - 1])) start--;
            while (end + 1 < line.Length && IsValidWordPart(line[end + 1])) end++;
            var token = line.Substring(start, end - start + 1);

            if (calculator.Variables.TryGetValue(token, out var value))
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
