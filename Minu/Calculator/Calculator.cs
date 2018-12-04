using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
using org.mariuszgromada.math.mxparser;
using org.mariuszgromada.math.mxparser.parsertokens;

namespace Minu.Calculator {
    class Calculator {
        public bool isInputOverflowed { get; private set; }

        private static Regex functionRegex = new Regex(@"\(.*?\)\s*=");
        private CalculatorCacheSystem cacheSystem = new CalculatorCacheSystem();
        private Dictionary<string, double> lastCalculatedVariableValues = new Dictionary<string, double>();

        public bool GetVariableValue(string token, out double value)
        {
            return lastCalculatedVariableValues.TryGetValue(token, out value);
        }

        public List<string> Calculate(string rawInput)
        {
            IOutputFormatter outputFormatter = new DecFormatter();

            isInputOverflowed = false;
            lastCalculatedVariableValues.Clear();

            string[] inputs = rawInput.Replace("\r", "").Split('\n');

            var resultList = new List<string>();
            var arguments = new List<Argument>();
            var functions = new List<Function>();
            double ans = 0.0;

            for (int lineNumber = 0; lineNumber < inputs.Length; ++lineNumber)
            {
                string inputLine = inputs[lineNumber];
                string lineResult = "";
                string definedToken = "";

                if (inputLine.StartsWith("#"))
                {
                    // comments + settings
                    string trimed = inputLine.Substring(1).TrimStart().ToLower();
                    if (trimed.StartsWith("bin")) outputFormatter = new BinFormatter();
                    else if (trimed.StartsWith("oct")) outputFormatter = new OctFormatter();
                    else if (trimed.StartsWith("dec")) outputFormatter = new DecFormatter();
                    else if (trimed.StartsWith("hex")) outputFormatter = new HexFormatter();
                    else if (trimed.StartsWith("sci")) outputFormatter = new SciFormatter();
                }
                else if (functionRegex.IsMatch(inputLine))
                {
                    // functions
                    Function func = new Function(inputLine);
                    func.addFunctions(functions.ToArray());
                    if (func.getFunctionName() != null) {
                        bool overrided = functions.RemoveAll(f => f.getFunctionName() == func.getFunctionName()) > 0;
                        functions.Add(func);
                        definedToken = func.getFunctionName();
                        cacheSystem.InvalidateCache(func.getFunctionName());
                    }
                }
                else if (inputLine.Contains("="))
                {
                    // variables
                    Argument arg;
                    double res;
                    if (cacheSystem.TryGetResult(inputLine, out var cache)) // try to get from cache first
                    {
                        arg = new Argument(cache.Name, cache.Result);
                        //lineResult = "[#] ";
                    }
                    else
                    {
                        arg = new Argument(inputLine, new Argument("ans", ans));
                        arg.addArguments(arguments.ToArray());
                        arg.addFunctions(functions.ToArray());

                        var argName = arg.getArgumentName();
                        cacheSystem.InvalidateCache(argName);

                        if (argName != null) // arg.argumentExpression.initialTokens
                            if (typeof(Argument).GetField("argumentExpression",
                                    BindingFlags.Instance | BindingFlags.NonPublic)
                                ?.GetValue(arg) is Expression expression)
                                if (typeof(Expression).GetField("initialTokens",
                                        BindingFlags.Instance | BindingFlags.NonPublic)
                                    ?.GetValue(expression) is List<Token> tks)
                                    cacheSystem.SetCache(inputLine, tks, argName, arg.getArgumentValue());
                        
                        //lineResult = "[*] ";
                    }

                    bool overrided = arguments.RemoveAll(a => a.getArgumentName() == arg.getArgumentName()) > 0;
                    arguments.Add(arg);
                    res = arg.getArgumentValue();
                    definedToken = arg.getArgumentName();
                    if(definedToken!=null)
                        lastCalculatedVariableValues[definedToken] = res;
                    lineResult += outputFormatter.Format(res);
                }
                else if (inputLine != "")
                {
                    // evaluate
                    double result;
                    if (!cacheSystem.TryGetResult(inputLine, out var cache)) {
                        var expression = new Expression(inputLine, new Argument("ans", ans));
                        expression.addArguments(arguments.ToArray());
                        expression.addFunctions(functions.ToArray());
                        result = expression.calculate();
                        if(!double.IsNaN(result))
                            cacheSystem.SetCache(inputLine, expression.getCopyOfInitialTokens(), null, result);
                        //lineResult = "[*] ";
                    }
                    else
                    {
                        result = cache.Result;
                        //lineResult = "[#] ";
                    }

                    if (!double.IsNaN(result))
                    {
                        ans = result;
                        lineResult += outputFormatter.Format(result);
                    }

                }

                cacheSystem.InvalidateCache("ans");
                cacheSystem.RegisterDefinedToken(lineNumber, definedToken);
                resultList.Add(lineResult);
            }

            return resultList;
        }
    }
}
