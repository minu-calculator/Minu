﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Converters;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Editing;
using org.mariuszgromada.math.mxparser.parsertokens;
using Expression = org.mariuszgromada.math.mxparser.Expression;

namespace Minu.Layouts.Helpers
{
    class CompletionHelper {

        class CompletionData : ICompletionData {
            public CompletionData(string text, string description, double priority)
            {
                Text = text;
                Priority = priority;
                Description = description;
            }
            
            public ImageSource Image => null;

            public string Text { get; }
            public object Content => Text;
            public object Description { get; }
            public double Priority { get; }

            public void Complete(TextArea textArea, ISegment completionSegment,
                EventArgs insertionRequestEventArgs)
            {

                var alreadyInputted = GetTokenBefore(textArea.Document, completionSegment.EndOffset, out var isConstant);
                var constantBonus = isConstant ? 1 : 0; // Remove "[" if it starts with so.
                textArea.Document.Replace(completionSegment.EndOffset - alreadyInputted.Length - constantBonus,
                    alreadyInputted.Length + constantBonus, Text);
            }
        }
        
        readonly TextEditor _editor;
        CompletionWindow _completionWindow;
        private readonly Calculator.Calculator _calculator;
        private bool _toggled = false;
        // name: description
        private static readonly Dictionary<string, string> _constantsDictionary;
        private static readonly Dictionary<string, string> _keywordsDictionary;
        
        static CompletionHelper()
        {
            _constantsDictionary = new Dictionary<string, string>();
            _keywordsDictionary = new Dictionary<string, string>();
            var constantFilter = new[]
            {
                ConstantValue.TYPE_ID, Unit.TYPE_ID, RandomVariable.TYPE_ID
            };
            var keywordFilterExclude = new[]
            {
                ConstantValue.TYPE_ID, Unit.TYPE_ID, RandomVariable.TYPE_ID,
                Operator.TYPE_ID, BooleanOperator.TYPE_ID, BinaryRelation.TYPE_ID,
                BitwiseOperator.TYPE_ID,ParserSymbol.TYPE_ID, ParserSymbol.NUMBER_TYPE_ID
            };
            foreach (var item in new Expression().getKeyWords())
            {
                if (constantFilter.Contains(item.wordTypeId))
                    _constantsDictionary[item.wordString] =
                        item.description + " (" + new Expression(item.wordString).calculate() + ")";

                if (!keywordFilterExclude.Contains(item.wordTypeId))
                    _keywordsDictionary[item.wordString] = item.syntax + " " + item.description;
            }
        }

        public CompletionHelper(TextEditor editor, Calculator.Calculator calculator)
        {
            _editor = editor;
            _editor.TextArea.TextEntered += TextEntered;
            _editor.TextArea.KeyDown += KeyDown;
            _calculator = calculator;
        }

        public void Deactivate()
        {
            _editor.TextArea.TextEntered -= TextEntered;
            _editor.TextArea.KeyDown -= KeyDown;
        }

        static bool IsValidWordPart(char c) => char.IsLetterOrDigit(c) || c == '_';
        static string GetTokenBefore(TextDocument document, int offset, out bool isConstant) {
            var docLine = document.GetLineByOffset(offset);
            var line = document.GetText(docLine);
            var end = offset - docLine.Offset;
            var start = end;
            while (start - 1 >= 0 && IsValidWordPart(line[start - 1])) start--;
            isConstant = start - 1 >= 0 && line[start - 1] == '[';
            return line.Substring(start, Math.Max(0, Math.Min(end - start + 1, line.Length - start)));
        }

        private void KeyDown(object sender, KeyEventArgs e)
        {
            if ((Keyboard.GetKeyStates(Key.LeftCtrl) & KeyStates.Down) != 0 &&
                (Keyboard.GetKeyStates(Key.Enter) & KeyStates.Down) != 0)
            {
                // Ctrl + Enter
                _toggled = !_toggled;
                UpdateCompletionWindow();
            }
        }


        void UpdateCompletionWindow()
        {
            if (!_toggled)
            {
                _completionWindow?.Close(); // Close the completion window if it still opens
                return;
            }

            if (_completionWindow != null) return;

            // initialize the completion window
            GetTokenBefore(_editor.Document, _editor.CaretOffset, out var isConstant);

            _completionWindow = new CompletionWindow(_editor.TextArea)
            {
                CloseWhenCaretAtBeginning = !isConstant
            };

            _completionWindow.Closed += delegate
            {
                _completionWindow = null;
                _toggled = false;
            };

            var data = _completionWindow.CompletionList.CompletionData;
            if (!isConstant)
            {
                foreach (var variable in _calculator.Variables)
                    data.Add(new CompletionData(variable.Key, "(user-defined) " + variable.Value, 1));

                foreach (var func in _calculator.Functions)
                    data.Add(new CompletionData(func.Key, "(user-defined) " + func.Value, 1));

                foreach (var keyword in _keywordsDictionary)
                    data.Add(new CompletionData(keyword.Key, keyword.Value, 2));
            }
            else
            {
                foreach (var constant in _constantsDictionary)
                    data.Add(new CompletionData(constant.Key, constant.Value, 1));
            }

            _completionWindow.Show();
        }

        void TextEntered(object sender, TextCompositionEventArgs e)
        {
            if (e.Text == "[") _toggled = true;
            if (!IsValidWordPart(e.Text[0])) _completionWindow?.Close();
            UpdateCompletionWindow();
        }
        
    }
}
