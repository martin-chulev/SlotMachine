using SlotMachine.Core.Models;

namespace SlotMachine.Core.Config
{
    public class GameSettings
    {
        /// <summary>
        /// Possible symbols in the slot machine.
        /// </summary>
        public Symbol[] Symbols { get; set; } = Array.Empty<Symbol>();

        /// <summary>
        /// Number of rows in the slot machine.
        /// </summary>
        public int Rows { get; set; }

        /// <summary>
        /// Number of columns (symbols per row) in the slot machine.
        /// </summary>
        public int SymbolsPerRow { get; set; }

        /// <summary>
        /// <para>Amount of times to retry when user enters faulty input.</para>
        /// <para><see langword="null"/> = unlimited.</para>
        /// </summary>
        public int? InputRetryAmount { get; set; }

        /// <summary>
        /// <para>Seed for random symbol generator.</para>
        /// <para><see langword="null"/> = random.</para>
        /// </summary>
        public int? RandomSeed { get; set; }
    }
}
