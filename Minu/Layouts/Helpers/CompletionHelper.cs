using System;
using System.Collections.Generic;
using System.Windows.Input;
using System.Windows.Media;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Editing;

namespace Minu.Layouts.Helpers
{
    class CompletionHelper
    {
        class CompletionData : ICompletionData {
            public CompletionData(string text, double priority)
            {
                Text = text;
                Priority = priority;
            }

            public ImageSource Image => null;

            public string Text { get; }

            // Use this property if you want to show a fancy UIElement in the list.
            public object Content => Text;

            public object Description => "Description for " + Text;
            public double Priority { get; }

            public void Complete(TextArea textArea, ISegment completionSegment,
                EventArgs insertionRequestEventArgs) {
                textArea.Document.Replace(completionSegment, Text);
            }
        }
        
        readonly TextEditor _editor;
        CompletionWindow _completionWindow;

        public CompletionHelper(TextEditor editor)
        {
            _editor = editor;
            _editor.TextArea.TextEntering += TextEntering;
            _editor.TextArea.TextEntered += TextEntered;
        }

        public void Deactivate()
        {
            _editor.TextArea.TextEntering -= TextEntering;
            _editor.TextArea.TextEntered -= TextEntered;
        }

        void TextEntered(object sender, TextCompositionEventArgs e)
        {
            _completionWindow = new CompletionWindow(_editor.TextArea);
            IList<ICompletionData> data = _completionWindow.CompletionList.CompletionData;
            data.Add(new CompletionData("Item1", 2));
            data.Add(new CompletionData("Item2", 1));
            data.Add(new CompletionData("Item3", 3));
            _completionWindow.Show();
            _completionWindow.Closed += delegate { _completionWindow = null; };
        }

        void TextEntering(object sender, TextCompositionEventArgs e)
        {
            if (e.Text.Length > 0 && _completionWindow != null)
            {
                if (!char.IsLetterOrDigit(e.Text[0]))
                {
                    _completionWindow.CompletionList.RequestInsertion(e);
                }
            }
        }
    }
}
