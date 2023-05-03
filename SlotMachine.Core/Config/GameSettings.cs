using SlotMachine.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlotMachine.Core.Config
{
    public static class GameSettings
    {
        /// <summary>
        /// Possible symbols in the slot machine.
        /// </summary>
        public static readonly Symbol[] Symbols = new Symbol[]
        {
            new Symbol("Apple", 'A', 0.4m, 45),
            new Symbol("Banana", 'B', 0.6m, 35),
            new Symbol("Pineapple", 'P', 0.8m, 15),
            new Symbol("Wildcard", '*', 0m, 5, isWildcard: true)
        };

        /// <summary>
        /// Number of rows in the slot machine.
        /// </summary>
        public static int Rows = 4;

        /// <summary>
        /// Number of columns (symbols per row) in the slot machine.
        /// </summary>
        public static int SymbolsPerRow = 3;

        /// <summary>
        /// <para>Amount of times to retry when user enters faulty input.</para>
        /// <para><see langword="null"/> = unlimited.</para>
        /// </summary>
        public static int? InputRetryAmount = null;

        /// <summary>
        /// <para>Seed for random symbol generator.</para>
        /// <para><see langword="null"/> = random.</para>
        /// </summary>
        public static int? RandomSeed = null;
    }
}
