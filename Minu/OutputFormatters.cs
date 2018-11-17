using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Minu
{
    interface IOutputFormatter {
        string Format(double input);
    }

    class BinFormatter : IOutputFormatter {
        public string Format(double input) {
            return Convert.ToString((long)input, 2);
        }
    }
    class OctFormatter : IOutputFormatter {
        public string Format(double input) {
            return Convert.ToString((long)input, 8);
        }
    }
    class DecFormatter : IOutputFormatter {
        public string Format(double input) {
            return input.ToString("G");
        }
    }
    class HexFormatter : IOutputFormatter {
        public string Format(double input) {
            return Convert.ToString((long)input, 16);
        }
    }
    class SciFormatter : IOutputFormatter {
        public string Format(double input) {
            return input.ToString("E");
        }
    }
}
