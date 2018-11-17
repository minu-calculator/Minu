using org.mariuszgromada.math.mxparser;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Minu {
    class Calculator {

        public int characterPerLine { get; set; }
        public bool isInputOverflowed { get; private set; }

        private static Regex functionRegex = new Regex(@"\(.*?\)\s*=");

        public string Calculate(string rawInput) {
            if (characterPerLine < 0) return "";

            IOutputFormatter outputFormatter = new DecFormatter();

            isInputOverflowed = false;

            string outputText = "";
            string[] inputs = rawInput.Replace("\r", "").Split('\n');

            var arguments = new List<Argument>();
            var functions = new List<Function>();
            double ans = 0.0;
            int count = 0;

            foreach (string input in inputs) {

                count++;

                // Add \n to align with the input
                var inputActualLine = Math.Max(1, (int)Math.Ceiling((double)input.Length / characterPerLine));
                if (inputActualLine > 1)
                    isInputOverflowed = true;
                //if (count > 1)
                    outputText += new string('\n', inputActualLine - 1);

                if (input.StartsWith("#")) { // comments + settings
                    string trimed = input.Substring(1).TrimStart().ToLower();
                    if (trimed.StartsWith("bin")) outputFormatter = new BinFormatter();
                    else if (trimed.StartsWith("oct")) outputFormatter = new OctFormatter();
                    else if (trimed.StartsWith("dec")) outputFormatter = new DecFormatter();
                    else if (trimed.StartsWith("hex")) outputFormatter = new HexFormatter();
                    else if (trimed.StartsWith("sci")) outputFormatter = new SciFormatter();
                }
                else if (functionRegex.IsMatch(input)) { // functions
                    bool overrided = false;
                    Function func = new Function(input);
                    func.addFunctions(functions.ToArray());
                    if (func.getFunctionName() != null) {
                        if (functions.RemoveAll(f => f.getFunctionName() == func.getFunctionName()) > 0) // override occurred
                            overrided = true;
                        functions.Add(func);

                        //show function assignment
                        //outputText += (overrided ? "(*) " : "") + func.getFunctionName();
                    }
                }
                else if (input.Contains("=")) { // variables
                    bool overrided = false;
                    Argument arg = new Argument(input, new Argument("ans", ans));
                    arg.addArguments(arguments.ToArray());
                    arg.addFunctions(functions.ToArray());
                    if (arguments.RemoveAll(a => a.getArgumentName() == arg.getArgumentName()) > 0) // override occurred
                        overrided = true;
                    arguments.Add(arg);
                    outputText +=
                        //show variable assignment
                        //(overrided ? "(*) " : "") + arg.getArgumentName() + " = " +
                        outputFormatter.Format(arg.getArgumentValue());
                }
                else if (input != "") { // evaluate
                    var expression = new Expression(input, new Argument("ans", ans));
                    expression.addArguments(arguments.ToArray());
                    expression.addFunctions(functions.ToArray());
                    var result = expression.calculate();
                    if (!double.IsNaN(result)) {
                        ans = result;
                        outputText += outputFormatter.Format(result);
                    }
                }
                outputText += '\n';
            }
            return outputText.TrimEnd('\n');
        }
    }
}
