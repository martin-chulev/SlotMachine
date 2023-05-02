using SlotMachine.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlotMachine.Core.Config
{
    internal static class GameSettings
    {
        public static readonly Symbol[] Symbols = new Symbol[]
        {
            new Symbol("Apple", 'A', 0.4m, 45),
            new Symbol("Banana", 'B', 0.6m, 35),
            new Symbol("Pineapple", 'P', 0.8m, 15),
            new Symbol("Wildcard", '*', 0m, 5, isWildcard: true)
        };

        public static int Rows = 4;
        public static int SymbolsPerRow = 3;
    }
}
