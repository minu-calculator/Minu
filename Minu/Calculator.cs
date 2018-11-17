using org.mariuszgromada.math.mxparser;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Minu {
    class Calculator {

        public int characterPerLine { get; set; }
        public bool isInputOverflowed { get; private set; }

        private static Regex functionRegex = new Regex(@"\(.*?\)\s*=");

        public List<string> Calculate(string rawInput) {
            if (characterPerLine < 0) return new List<string>();

            IOutputFormatter outputFormatter = new DecFormatter();

            isInputOverflowed = false;
            
            string[] inputs = rawInput.Replace("\r", "").Split('\n');
            
            var resultList = new List<string>();
            var arguments = new List<Argument>();
            var functions = new List<Function>();
            double ans = 0.0;

            foreach (string inputLine in inputs) {

                if (inputLine.StartsWith("#")) { // comments + settings
                    string trimed = inputLine.Substring(1).TrimStart().ToLower();
                    if (trimed.StartsWith("bin")) outputFormatter = new BinFormatter();
                    else if (trimed.StartsWith("oct")) outputFormatter = new OctFormatter();
                    else if (trimed.StartsWith("dec")) outputFormatter = new DecFormatter();
                    else if (trimed.StartsWith("hex")) outputFormatter = new HexFormatter();
                    else if (trimed.StartsWith("sci")) outputFormatter = new SciFormatter();
                }
                else if (functionRegex.IsMatch(inputLine)) { // functions
                    bool overrided = false;
                    Function func = new Function(inputLine);
                    func.addFunctions(functions.ToArray());
                    if (func.getFunctionName() != null) {
                        if (functions.RemoveAll(f => f.getFunctionName() == func.getFunctionName()) > 0) // override occurred
                            overrided = true;
                        functions.Add(func);
                        resultList.Add("");
                        //outputText += (overrided ? "(*) " : "") + func.getFunctionName();
                    }
                }
                else if (inputLine.Contains("=")) { // variables
                    bool overrided = false;
                    Argument arg = new Argument(inputLine, new Argument("ans", ans));
                    arg.addArguments(arguments.ToArray());
                    arg.addFunctions(functions.ToArray());
                    if (arguments.RemoveAll(a => a.getArgumentName() == arg.getArgumentName()) > 0) // override occurred
                        overrided = true;
                    arguments.Add(arg);
                    //outputText += (overrided ? "(*) " : "") + arg.getArgumentName() + " = " +
                    //outputFormatter.Format(arg.getArgumentValue());
                    resultList.Add(arg.getArgumentValue().ToString());
                }
                else if (inputLine != "") { // evaluate
                    var expression = new Expression(inputLine, new Argument("ans", ans));
                    expression.addArguments(arguments.ToArray());
                    expression.addFunctions(functions.ToArray());
                    var result = expression.calculate();
                    if (!double.IsNaN(result)) {
                        ans = result;
                        resultList.Add(outputFormatter.Format(result));
                    }
                }
            }
            return resultList;
        }
    }
}
