using SlotMachine.Core.Config;
using SlotMachine.Core.Models;

namespace SlotMachine.Core
{
    public class SimplifiedSlotMachine
    {
        private readonly Action<string?> _textOutput;
        private readonly Func<string?> _textInput;
        private readonly Random _random;

        /// <summary>
        /// Current available amount in the slot machine.
        /// </summary>
        public decimal Balance { get; set; }

        public SimplifiedSlotMachine(Action<string?> textOutput, Func<string?> textInput)
        {
            _textOutput = textOutput;
            _textInput = textInput;
            _random = GameSettings.RandomSeed != null ? new Random(GameSettings.RandomSeed.Value) : new Random();
        }

        #region Input
        /// <summary>
        /// Prompts the player to input a deposit amount.
        /// </summary>
        /// <returns>The deposited amount.</returns>
        public decimal RequestDeposit()
        {
            return RequestMoneyAmount(Texts.ENTER_DEPOSIT_AMOUNT, Texts.INVALID_AMOUNT);
        }

        /// <summary>
        /// Prompts the player to input a stake amount.
        /// </summary>
        /// <returns>The staked amount.</returns>
        public decimal RequestStake()
        {
            decimal stake = RequestMoneyAmount(Texts.ENTER_STAKE_AMOUNT, Texts.INVALID_AMOUNT);

            int retryCount = 1;
            while (stake > Balance)
            {
                _textOutput?.Invoke(Texts.INSUFFICIENT_FUNDS);

                if (GameSettings.InputRetryAmount != null && retryCount > GameSettings.InputRetryAmount)
                {
                    return 0m;
                }

                stake = RequestMoneyAmount(Texts.ENTER_STAKE_AMOUNT, Texts.INVALID_AMOUNT);
                ++retryCount;
            }

            return stake;
        }

        /// <summary>
        /// Requests input until a valid amount is entered or retry count was exceeded.
        /// </summary>
        /// <param name="requestText">Text which prompts the user what to enter.</param>
        /// <param name="errorText">Text to display in case of invalid input.</param>
        /// <returns>The parsed valid amount or 0 in case the retry count was exceeded.</returns>
        private decimal RequestMoneyAmount(string requestText, string errorText)
        {
            decimal amount;

            _textOutput?.Invoke(requestText);

            int retryCount = 0;
            while (!decimal.TryParse(_textInput?.Invoke(), out amount) || amount <= 0m)
            {
                _textOutput?.Invoke(errorText);
                _textOutput?.Invoke(requestText);
                
                ++retryCount;

                if (GameSettings.InputRetryAmount != null && retryCount > GameSettings.InputRetryAmount)
                {
                    return 0m;
                }
            }

            return amount;
        }
        #endregion

        #region Machine flow / game logic
        /// <summary>
        /// Starts the slot machine.
        /// </summary>
        public void Start()
        {
            Balance = 0m;

            var deposit = RequestDeposit();
            Balance += deposit;

            StartSpinning();
        }

        /// <summary>
        /// Starts spinning the slot machine until the balance runs out.
        /// </summary>
        public void StartSpinning()
        {
            while (Balance > 0)
            {
                var stakeAmount = RequestStake();
                Balance -= stakeAmount;

                var result = Spin(stakeAmount);
                Balance += result;

                _textOutput?.Invoke($"{Texts.WON_AMOUNT} {result:F2}");
                _textOutput?.Invoke($"{Texts.CURRENT_BALANCE} {Balance:F2}");
                _textOutput?.Invoke(string.Empty);
            }

            _textOutput?.Invoke(Texts.GAME_OVER);
        }

        /// <summary>
        /// Spins the slot machine and outputs the result.
        /// </summary>
        /// <param name="stakeAmount">Amount to stake on the spin.</param>
        /// <returns>The won amount.</returns>
        public decimal Spin(decimal stakeAmount)
        {
            decimal totalCoefficient = 0m;

            _textOutput?.Invoke(string.Empty);

            for (int rowIndex = 0; rowIndex < GameSettings.Rows; rowIndex++)
            {
                var row = new Symbol[GameSettings.SymbolsPerRow];
                for (int colIndex = 0; colIndex < GameSettings.SymbolsPerRow; colIndex++)
                {
                    row[colIndex] = GetRandomSymbol();
                }

                _textOutput?.Invoke(string.Concat(row.Select(symbol => symbol.Value)));

                if (RowIsWinning(row))
                {
                    totalCoefficient += row.Sum(symbol => symbol.Coefficient);
                }
            }

            _textOutput?.Invoke(string.Empty);

            return totalCoefficient * stakeAmount;
        }

        /// <summary>
        /// Randomly selects a symbol from the array of possible symbols based on their probability to appear.
        /// </summary>
        /// <returns>The generated symbol.</returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public Symbol GetRandomSymbol()
        {
            var randomNumber = _random.Next(0, 100);

            double iterator = 0d;
            for (int i = 0; i < GameSettings.Symbols.Length; i++)
            {
                if (randomNumber < iterator + GameSettings.Symbols[i].ProbabilityPercent)
                {
                    return GameSettings.Symbols[i];
                }
                else
                {
                    iterator += GameSettings.Symbols[i].ProbabilityPercent;
                }
            }

            throw new ArgumentOutOfRangeException($"Error while generating random symbol. Number {randomNumber} could not be matched.");
        }

        /// <summary>
        /// Checks whether the specified row is a winning sequence.
        /// </summary>
        /// <param name="row">The sequence of symbols to check.</param>
        /// <returns><see langword="true"/> if the sequence is winning.</returns>
        public bool RowIsWinning(Symbol[] row)
        {
            return row.Where(symbol => !symbol.IsWildcard).DistinctBy(symbol => symbol.Value).Count() == 1;
        }
        #endregion
    }
}
