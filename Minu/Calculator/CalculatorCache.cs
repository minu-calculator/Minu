using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using org.mariuszgromada.math.mxparser;
using org.mariuszgromada.math.mxparser.parsertokens;

namespace Minu.Calculator
{
    class CalculatorCacheSystem {
        public class Cache {
            public readonly string[] DependedTokens;
            public readonly string Name;
            public readonly double Result;
            public bool IsValid = true;

            public Cache(List<Token> dependedTokens, string name, double result)
            {
                DependedTokens = (from token in dependedTokens
                    where token.tokenTypeId == Argument.TYPE_ID
                    select token.tokenStr).ToArray();
                Name = name;
                Result = result;
            }
        }

        private int _maximumCacheSize;
        private Dictionary<string, Cache> caches = new Dictionary<string, Cache>();
        private Dictionary<int, string> definedToken = new Dictionary<int, string>();

        public CalculatorCacheSystem(int maximumCacheSize = 200)
        {
            _maximumCacheSize = maximumCacheSize;
        }

        public void SetCache(string expression, List<Token> dependedTokens, string name, double result)
        {
            if (name == null) return;
            caches[expression] = new Cache(dependedTokens, name, result);
        }

        public bool TryGetResult(string expression, out Cache result)
        {
            return caches.TryGetValue(expression, out result) && result.IsValid;
        }

        public void InvalidateCache(string tokenName)
        {
            if (tokenName == "") return;
            foreach (var cache in caches)
            {
                foreach (var token in cache.Value.DependedTokens)
                {
                    if (token == tokenName)
                    {
                        cache.Value.IsValid = false;
                        break;
                    }
                }
            }
        }

        public void RegisterDefinedToken(int lineNumber, string tokenName)
        {
            if (definedToken.ContainsKey(lineNumber) &&
                definedToken[lineNumber] != tokenName)
                InvalidateCache(definedToken[lineNumber]); // Invalidate the old token.
            definedToken[lineNumber] = tokenName;
        }
    }
}
