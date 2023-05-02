using SlotMachine.Core.Config;
using SlotMachine.Core.Models;

namespace SlotMachine.Core
{
    public class SimplifiedSlotMachine
    {
        private readonly Action<string?> _textOutput;
        private readonly Func<string?> _textInput;
        private readonly Random _random = new();

        public SimplifiedSlotMachine(Action<string?> textOutput, Func<string?> textInput)
        {
            _textOutput = textOutput;
            _textInput = textInput;
        }

        public decimal Balance { get; set; }

        public decimal RequestDeposit()
        {
            return RequestMoneyAmount(Texts.ENTER_DEPOSIT_AMOUNT, Texts.INVALID_AMOUNT);
        }

        public decimal RequestStake()
        {
            decimal stake = RequestMoneyAmount(Texts.ENTER_STAKE_AMOUNT, Texts.INVALID_AMOUNT);

            while (stake > Balance)
            {
                _textOutput?.Invoke(Texts.INSUFFICIENT_FUNDS);
                stake = RequestMoneyAmount(Texts.ENTER_STAKE_AMOUNT, Texts.INVALID_AMOUNT);
            }

            return stake;
        }

        private decimal RequestMoneyAmount(string requestText, string errorText)
        {
            decimal amount;

            _textOutput?.Invoke(requestText);

            while (!decimal.TryParse(_textInput?.Invoke(), out amount) || amount <= 0m)
            {
                _textOutput?.Invoke(errorText);
                _textOutput?.Invoke(requestText);
            }

            return amount;
        }

        public void Start()
        {
            Balance = 0m;

            var deposit = RequestDeposit();
            Balance += deposit;

            StartSpinning();
        }

        public void StartSpinning()
        {
            while (Balance > 0)
            {
                var stakeAmount = RequestStake();
                Balance -= stakeAmount;

                var result = Spin(stakeAmount);
                Balance += result;

                _textOutput?.Invoke($"{Texts.WON_AMOUNT} {result}");
                _textOutput?.Invoke($"{Texts.CURRENT_BALANCE} {Balance}");
                _textOutput?.Invoke(string.Empty);
            }

            _textOutput?.Invoke(Texts.GAME_OVER);
        }

        private decimal Spin(decimal stakeAmount)
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

                if (row.Where(symbol => !symbol.IsWildcard).DistinctBy(symbol => symbol.Value).Count() == 1)
                {
                    totalCoefficient += row.Sum(symbol => symbol.Coefficient);
                }
            }

            _textOutput?.Invoke(string.Empty);

            return totalCoefficient * stakeAmount;
        }

        private Symbol GetRandomSymbol()
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
    }
}
