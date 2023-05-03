using SlotMachine.Core.Config;
using SlotMachine.Core.Models;

namespace SlotMachine.Core
{
    public class SimplifiedSlotMachine
    {
        private readonly Action<string?> _textOutput;
        private readonly Func<string?> _textInput;
        private readonly Random _random;

        private readonly Texts _texts;
        private readonly GameSettings _gameSettings;

        /// <summary>
        /// Current available amount in the slot machine.
        /// </summary>
        public decimal Balance { get; set; }

        public SimplifiedSlotMachine(Action<string?> textOutput, Func<string?> textInput, Texts texts, GameSettings gameSettings)
        {
            _textOutput = textOutput;
            _textInput = textInput;
            _texts = texts;
            _gameSettings = gameSettings;
            _random = _gameSettings.RandomSeed != null ? new Random(_gameSettings.RandomSeed.Value) : new Random();
        }

        #region Input
        /// <summary>
        /// Prompts the player to input a deposit amount.
        /// </summary>
        /// <returns>The deposited amount.</returns>
        public decimal RequestDeposit()
        {
            return RequestMoneyAmount(_texts.EnterDepositAmount, _texts.InvalidAmount);
        }

        /// <summary>
        /// Prompts the player to input a stake amount.
        /// </summary>
        /// <returns>The staked amount.</returns>
        public decimal RequestStake()
        {
            decimal stake = RequestMoneyAmount(_texts.EnterStakeAmount, _texts.InvalidAmount);

            int retryCount = 1;
            while (stake > Balance)
            {
                _textOutput?.Invoke(_texts.InsufficientFunds);

                if (_gameSettings.InputRetryAmount != null && retryCount > _gameSettings.InputRetryAmount)
                {
                    return 0m;
                }

                stake = RequestMoneyAmount(_texts.EnterStakeAmount, _texts.InvalidAmount);
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
        private decimal RequestMoneyAmount(string? requestText, string? errorText)
        {
            decimal amount;

            _textOutput?.Invoke(requestText);

            int retryCount = 0;
            while (!decimal.TryParse(_textInput?.Invoke(), out amount) || amount <= 0m)
            {
                _textOutput?.Invoke(errorText);
                _textOutput?.Invoke(requestText);
                
                ++retryCount;

                if (_gameSettings.InputRetryAmount != null && retryCount > _gameSettings.InputRetryAmount)
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

                _textOutput?.Invoke($"{_texts.WonAmount} {result:F2}");
                _textOutput?.Invoke($"{_texts.CurrentBalance} {Balance:F2}");
                _textOutput?.Invoke(string.Empty);
            }

            _textOutput?.Invoke(_texts.GameOver);
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

            for (int rowIndex = 0; rowIndex < _gameSettings.Rows; rowIndex++)
            {
                var row = new Symbol[_gameSettings.SymbolsPerRow];
                for (int colIndex = 0; colIndex < _gameSettings.SymbolsPerRow; colIndex++)
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
            for (int i = 0; i < _gameSettings.Symbols.Length; i++)
            {
                if (randomNumber < iterator + _gameSettings.Symbols[i].ProbabilityPercent)
                {
                    return _gameSettings.Symbols[i];
                }
                else
                {
                    iterator += _gameSettings.Symbols[i].ProbabilityPercent;
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
