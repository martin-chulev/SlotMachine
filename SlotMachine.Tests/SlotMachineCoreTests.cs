using Newtonsoft.Json;
using SlotMachine.Core;
using SlotMachine.Core.Config;
using SlotMachine.Core.Models;
using Xunit;

namespace SlotMachine.Tests
{
    public class SlotMachineCoreTests
    {
        private readonly Texts _texts;
        private readonly GameSettings _gameSettings;

        public SlotMachineCoreTests()
        {
            _gameSettings = JsonConvert.DeserializeObject<GameSettings>(File.ReadAllText("Config/GameSettings.json")) ?? new();
            _texts = JsonConvert.DeserializeObject<Texts>(File.ReadAllText("Config/Texts.json")) ?? new();
            _gameSettings.InputRetryAmount = 0;
        }

        #region Deposit
        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        [InlineData(123456789)]
        public void ShouldAcceptValidDeposit(decimal depositAmount)
        {
            var slotMachine = new SimplifiedSlotMachine((text) => { }, () => depositAmount.ToString(), _texts, _gameSettings);

            var result = slotMachine.RequestDeposit();

            Assert.Equal(depositAmount, result);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(-10)]
        [InlineData(-123456789)]
        public void ShouldShowErrorOnInvalidDepositAmount(decimal depositAmount)
        {
            var outputList = new List<string?>();
            var slotMachine = new SimplifiedSlotMachine((text) => outputList.Add(text), () => depositAmount.ToString(), _texts, _gameSettings);

            var result = slotMachine.RequestDeposit();

            Assert.Contains(_texts.InvalidAmount, outputList);
        }

        [Theory]
        [InlineData(0, 1)]
        [InlineData(-1, 1)]
        [InlineData(-10, 1)]
        [InlineData(-123456789, 1)]
        public void ShouldRetryOnInvalidDepositAmount(decimal depositAmount1, decimal depositAmount2)
        {
            _gameSettings.InputRetryAmount = 1;
            var outputList = new List<string?>();

            var currentDepositAmount = depositAmount1;
            var slotMachine = new SimplifiedSlotMachine(
                (text) => outputList.Add(text), 
                () => 
                { 
                    var result = currentDepositAmount.ToString(); 
                    currentDepositAmount = depositAmount2;
                    return result;
                }, 
                _texts, 
                _gameSettings
            );

            var result = slotMachine.RequestDeposit();

            Assert.Contains(_texts.InvalidAmount, outputList);
            Assert.Equal(depositAmount2, result);
        }

        [Theory]
        [InlineData("--123")]
        [InlineData("a123")]
        [InlineData("abc")]
        [InlineData("")]
        public void ShouldShowErrorOnInvalidDepositInput(string depositInput)
        {
            var outputList = new List<string?>();
            var slotMachine = new SimplifiedSlotMachine((text) => outputList.Add(text), () => depositInput, _texts, _gameSettings);

            var result = slotMachine.RequestDeposit();

            Assert.Contains(_texts.InvalidAmount, outputList);
        }
        #endregion

        #region Stake
        [Theory]
        [InlineData(1000000000, 1)]
        [InlineData(1000000000, 10)]
        [InlineData(1000000000, 123456789)]
        public void ShouldAcceptValidStake(decimal balance, decimal stakeAmount)
        {
            var slotMachine = new SimplifiedSlotMachine((text) => { }, () => stakeAmount.ToString(), _texts, _gameSettings);
            slotMachine.Balance = balance;

            var result = slotMachine.RequestStake();

            Assert.Equal(stakeAmount, result);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(-10)]
        [InlineData(-123456789)]
        public void ShouldShowErrorOnInvalidStakeAmount(decimal stakeAmount)
        {
            var outputList = new List<string?>();
            var slotMachine = new SimplifiedSlotMachine((text) => outputList.Add(text), () => stakeAmount.ToString(), _texts, _gameSettings);

            var result = slotMachine.RequestStake();

            Assert.Contains(_texts.InvalidAmount, outputList);
        }

        [Theory]
        [InlineData("--123")]
        [InlineData("a123")]
        [InlineData("abc")]
        [InlineData("")]
        public void ShouldShowErrorOnInvalidStakeInput(string stakeInput)
        {
            var outputList = new List<string?>();
            var slotMachine = new SimplifiedSlotMachine((text) => outputList.Add(text), () => stakeInput, _texts, _gameSettings);

            var result = slotMachine.RequestStake();

            Assert.Contains(_texts.InvalidAmount, outputList);
        }

        [Theory]
        [InlineData(0, 1)]
        [InlineData(9, 10)]
        [InlineData(100000000, 123456789)]
        public void ShouldShowErrorOnInsufficientStakeFunds(decimal balance, decimal stakeAmount)
        {
            var outputList = new List<string?>();
            var slotMachine = new SimplifiedSlotMachine((text) => outputList.Add(text), () => stakeAmount.ToString(), _texts, _gameSettings);
            slotMachine.Balance = balance;

            var result = slotMachine.RequestStake();

            Assert.Contains(_texts.InsufficientFunds, outputList);
        }

        [Theory]
        [InlineData(0, 1)]
        [InlineData(-1, 1)]
        [InlineData(-10, 1)]
        [InlineData(-123456789, 1)]
        public void ShouldRetryOnInvalidStakeAmount(decimal stakeAmount1, decimal stakeAmount2)
        {
            _gameSettings.InputRetryAmount = 1;
            var outputList = new List<string?>();

            var currentStakeAmount = stakeAmount1;
            var slotMachine = new SimplifiedSlotMachine(
                (text) => outputList.Add(text),
                () =>
                {
                    var result = currentStakeAmount.ToString();
                    currentStakeAmount = stakeAmount2;
                    return result;
                }, 
                _texts, 
                _gameSettings
            );
            slotMachine.Balance = decimal.MaxValue;

            var result = slotMachine.RequestStake();

            Assert.Contains(_texts.InvalidAmount, outputList);
            Assert.Equal(stakeAmount2, result);
        }

        [Theory]
        [InlineData(1, 2, 1)]
        [InlineData(9, 10, 5)]
        [InlineData(100000000, 123456789, 1)]
        public void ShouldRetryOnInsufficientStakeFunds(decimal balance, decimal stakeAmount1, decimal stakeAmount2)
        {
            _gameSettings.InputRetryAmount = 1;
            var outputList = new List<string?>();

            var currentStakeAmount = stakeAmount1;
            var slotMachine = new SimplifiedSlotMachine(
                (text) => outputList.Add(text),
                () =>
                {
                    var result = currentStakeAmount.ToString();
                    currentStakeAmount = stakeAmount2;
                    return result;
                }, 
                _texts, 
                _gameSettings
            );
            slotMachine.Balance = balance;

            var result = slotMachine.RequestStake();

            Assert.Contains(_texts.InsufficientFunds, outputList);
            Assert.Equal(stakeAmount2, result);
        }
        #endregion

        #region Game logic
        [Fact]
        public void ShouldNotSpinWithZeroBalance()
        {
            var outputList = new List<string?>();
            var slotMachine = new SimplifiedSlotMachine((text) => outputList.Add(text), () => "100", _texts, _gameSettings);
            slotMachine.Balance = 0;
            slotMachine.StartSpinning();

            Assert.Single(outputList);
            Assert.Contains(_texts.GameOver, outputList);
        }

        [Fact]
        public void ShouldStopSpinningWithZeroBalance()
        {
            _gameSettings.RandomSeed = 20;
            var outputList = new List<string?>();
            var slotMachine = new SimplifiedSlotMachine((text) => outputList.Add(text), () => "100", _texts, _gameSettings);
            slotMachine.Balance = 100;
            slotMachine.StartSpinning();

            Assert.Equal(1, outputList.Count(output => output == _texts.EnterStakeAmount));
            Assert.Equal(1, outputList.Count(output => output == _texts.GameOver));
        }

        [Fact]
        public void LosingSpinShouldReturnZero()
        {
            _gameSettings.RandomSeed = 20;
            var slotMachine = new SimplifiedSlotMachine((text) => { }, () => string.Empty, _texts, _gameSettings);
            var result = slotMachine.Spin(100);

            Assert.Equal(0, result);
        }

        [Fact]
        public void WinningSpinShouldReturnCorrectProfit()
        {
            _gameSettings.RandomSeed = 40;
            var slotMachine = new SimplifiedSlotMachine((text) => { }, () => string.Empty, _texts, _gameSettings);
            var result = slotMachine.Spin(40);

            Assert.Equal(72, result);
        }

        [Fact]
        public void WinningSpinWithWildcardShouldReturnCorrectProfit()
        {
            _gameSettings.RandomSeed = 30;
            var slotMachine = new SimplifiedSlotMachine((text) => { }, () => string.Empty, _texts, _gameSettings);
            var result = slotMachine.Spin(30);

            Assert.Equal(36, result);
        }

        [Theory]
        [InlineData(10)]
        [InlineData(20)]
        [InlineData(30)]
        public void ShouldGenerateRandomSymbol(int seed)
        {
            _gameSettings.RandomSeed = seed;
            var slotMachine = new SimplifiedSlotMachine((text) => { }, () => string.Empty, _texts, _gameSettings);
            
            Symbol symbol1 = slotMachine.GetRandomSymbol();
            Symbol symbol2 = slotMachine.GetRandomSymbol();
            Symbol symbol3 = slotMachine.GetRandomSymbol();

            Assert.Contains(symbol1, _gameSettings.Symbols);
            Assert.Contains(symbol2, _gameSettings.Symbols);
            Assert.Contains(symbol3, _gameSettings.Symbols);
            Assert.NotEqual(symbol1, symbol2);
        }

        [Fact]
        public void MatchingRowShouldBeWinning()
        {
            var slotMachine = new SimplifiedSlotMachine((text) => { }, () => string.Empty, _texts, _gameSettings);

            var row = new Symbol[]
            {
                new Symbol("Apple", "A", 0.4m, 45),
                new Symbol("Apple", "A", 0.4m, 45),
                new Symbol("Apple", "A", 0.4m, 45)
            };

            Assert.True(slotMachine.RowIsWinning(row));
        }

        [Fact]
        public void MatchingRowWithWildcardShouldBeWinning()
        {
            var slotMachine = new SimplifiedSlotMachine((text) => { }, () => string.Empty, _texts, _gameSettings);

            var row = new Symbol[]
            {
                new Symbol("Apple", "A", 0.4m, 45),
                new Symbol("Apple", "A", 0.4m, 45),
                new Symbol("Wildcard", "*", 0m, 5, isWildcard: true)
            };

            Assert.True(slotMachine.RowIsWinning(row));
        }

        [Fact]
        public void NonMatchingRowShouldBeLosing()
        {
            var slotMachine = new SimplifiedSlotMachine((text) => { }, () => string.Empty, _texts, _gameSettings);

            var row = new Symbol[]
            {
                new Symbol("Apple", "A", 0.4m, 45),
                new Symbol("Banana", "B", 0.6m, 35),
                new Symbol("Apple", "A", 0.4m, 45)
            };

            Assert.False(slotMachine.RowIsWinning(row));
        }

        [Fact]
        public void NonMatchingRowWithWildcardShouldBeLosing()
        {
            var slotMachine = new SimplifiedSlotMachine((text) => { }, () => string.Empty, _texts, _gameSettings);

            var row = new Symbol[]
            {
                new Symbol("Apple", "A", 0.4m, 45),
                new Symbol("Banana", "B", 0.6m, 35),
                new Symbol("Wildcard", "*", 0m, 5, isWildcard: true)
            };

            Assert.False(slotMachine.RowIsWinning(row));
        }
        #endregion
    }
}