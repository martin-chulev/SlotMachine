using SlotMachine.Core;
using SlotMachine.Core.Config;
using SlotMachine.Core.Models;
using Xunit;

namespace SlotMachine.Tests
{
    public class SlotMachineCoreTests
    {
        public SlotMachineCoreTests()
        {
            GameSettings.InputRetryAmount = 0;
        }

        #region Deposit
        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        [InlineData(123456789)]
        public void ShouldAcceptValidDeposit(decimal depositAmount)
        {
            var slotMachine = new SimplifiedSlotMachine((text) => { }, () => depositAmount.ToString());

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
            var slotMachine = new SimplifiedSlotMachine((text) => outputList.Add(text), () => depositAmount.ToString());

            var result = slotMachine.RequestDeposit();

            Assert.Contains(Texts.INVALID_AMOUNT, outputList);
        }

        [Theory]
        [InlineData(0, 1)]
        [InlineData(-1, 1)]
        [InlineData(-10, 1)]
        [InlineData(-123456789, 1)]
        public void ShouldRetryOnInvalidDepositAmount(decimal depositAmount1, decimal depositAmount2)
        {
            GameSettings.InputRetryAmount = 1;
            var outputList = new List<string?>();

            var currentDepositAmount = depositAmount1;
            var slotMachine = new SimplifiedSlotMachine(
                (text) => outputList.Add(text), 
                () => 
                { 
                    var result = currentDepositAmount.ToString(); 
                    currentDepositAmount = depositAmount2;
                    return result;
                }
            );

            var result = slotMachine.RequestDeposit();

            Assert.Contains(Texts.INVALID_AMOUNT, outputList);
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
            var slotMachine = new SimplifiedSlotMachine((text) => outputList.Add(text), () => depositInput);

            var result = slotMachine.RequestDeposit();

            Assert.Contains(Texts.INVALID_AMOUNT, outputList);
        }
        #endregion

        #region Stake
        [Theory]
        [InlineData(1000000000, 1)]
        [InlineData(1000000000, 10)]
        [InlineData(1000000000, 123456789)]
        public void ShouldAcceptValidStake(decimal balance, decimal stakeAmount)
        {
            var slotMachine = new SimplifiedSlotMachine((text) => { }, () => stakeAmount.ToString());
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
            var slotMachine = new SimplifiedSlotMachine((text) => outputList.Add(text), () => stakeAmount.ToString());

            var result = slotMachine.RequestStake();

            Assert.Contains(Texts.INVALID_AMOUNT, outputList);
        }

        [Theory]
        [InlineData("--123")]
        [InlineData("a123")]
        [InlineData("abc")]
        [InlineData("")]
        public void ShouldShowErrorOnInvalidStakeInput(string stakeInput)
        {
            var outputList = new List<string?>();
            var slotMachine = new SimplifiedSlotMachine((text) => outputList.Add(text), () => stakeInput);

            var result = slotMachine.RequestStake();

            Assert.Contains(Texts.INVALID_AMOUNT, outputList);
        }

        [Theory]
        [InlineData(0, 1)]
        [InlineData(9, 10)]
        [InlineData(100000000, 123456789)]
        public void ShouldShowErrorOnInsufficientStakeFunds(decimal balance, decimal stakeAmount)
        {
            var outputList = new List<string?>();
            var slotMachine = new SimplifiedSlotMachine((text) => outputList.Add(text), () => stakeAmount.ToString());
            slotMachine.Balance = balance;

            var result = slotMachine.RequestStake();

            Assert.Contains(Texts.INSUFFICIENT_FUNDS, outputList);
        }

        [Theory]
        [InlineData(0, 1)]
        [InlineData(-1, 1)]
        [InlineData(-10, 1)]
        [InlineData(-123456789, 1)]
        public void ShouldRetryOnInvalidStakeAmount(decimal stakeAmount1, decimal stakeAmount2)
        {
            GameSettings.InputRetryAmount = 1;
            var outputList = new List<string?>();

            var currentStakeAmount = stakeAmount1;
            var slotMachine = new SimplifiedSlotMachine(
                (text) => outputList.Add(text),
                () =>
                {
                    var result = currentStakeAmount.ToString();
                    currentStakeAmount = stakeAmount2;
                    return result;
                }
            );
            slotMachine.Balance = decimal.MaxValue;

            var result = slotMachine.RequestStake();

            Assert.Contains(Texts.INVALID_AMOUNT, outputList);
            Assert.Equal(stakeAmount2, result);
        }

        [Theory]
        [InlineData(1, 2, 1)]
        [InlineData(9, 10, 5)]
        [InlineData(100000000, 123456789, 1)]
        public void ShouldRetryOnInsufficientStakeFunds(decimal balance, decimal stakeAmount1, decimal stakeAmount2)
        {
            GameSettings.InputRetryAmount = 1;
            var outputList = new List<string?>();

            var currentStakeAmount = stakeAmount1;
            var slotMachine = new SimplifiedSlotMachine(
                (text) => outputList.Add(text),
                () =>
                {
                    var result = currentStakeAmount.ToString();
                    currentStakeAmount = stakeAmount2;
                    return result;
                }
            );
            slotMachine.Balance = balance;

            var result = slotMachine.RequestStake();

            Assert.Contains(Texts.INSUFFICIENT_FUNDS, outputList);
            Assert.Equal(stakeAmount2, result);
        }
        #endregion

        #region Game logic
        [Fact]
        public void ShouldNotSpinWithZeroBalance()
        {
            var outputList = new List<string?>();
            var slotMachine = new SimplifiedSlotMachine((text) => outputList.Add(text), () => "100");
            slotMachine.Balance = 0;
            slotMachine.StartSpinning();

            Assert.Single(outputList);
            Assert.Contains(Texts.GAME_OVER, outputList);
        }

        [Fact]
        public void ShouldStopSpinningWithZeroBalance()
        {
            GameSettings.RandomSeed = 20;
            var outputList = new List<string?>();
            var slotMachine = new SimplifiedSlotMachine((text) => outputList.Add(text), () => "100");
            slotMachine.Balance = 100;
            slotMachine.StartSpinning();

            Assert.Equal(1, outputList.Count(output => output == Texts.ENTER_STAKE_AMOUNT));
            Assert.Equal(1, outputList.Count(output => output == Texts.GAME_OVER));
        }

        [Fact]
        public void LosingSpinShouldReturnZero()
        {
            GameSettings.RandomSeed = 20;
            var slotMachine = new SimplifiedSlotMachine((text) => { }, () => string.Empty);
            var result = slotMachine.Spin(100);

            Assert.Equal(0, result);
        }

        [Fact]
        public void WinningSpinShouldReturnCorrectProfit()
        {
            GameSettings.RandomSeed = 40;
            var slotMachine = new SimplifiedSlotMachine((text) => { }, () => string.Empty);
            var result = slotMachine.Spin(40);

            Assert.Equal(72, result);
        }

        [Fact]
        public void WinningSpinWithWildcardShouldReturnCorrectProfit()
        {
            GameSettings.RandomSeed = 30;
            var slotMachine = new SimplifiedSlotMachine((text) => { }, () => string.Empty);
            var result = slotMachine.Spin(30);

            Assert.Equal(36, result);
        }

        [Theory]
        [InlineData(10)]
        [InlineData(20)]
        [InlineData(30)]
        public void ShouldGenerateRandomSymbol(int seed)
        {
            GameSettings.RandomSeed = seed;
            var slotMachine = new SimplifiedSlotMachine((text) => { }, () => string.Empty);
            
            Symbol symbol1 = slotMachine.GetRandomSymbol();
            Symbol symbol2 = slotMachine.GetRandomSymbol();
            Symbol symbol3 = slotMachine.GetRandomSymbol();

            Assert.Contains(symbol1, GameSettings.Symbols);
            Assert.Contains(symbol2, GameSettings.Symbols);
            Assert.Contains(symbol3, GameSettings.Symbols);
            Assert.NotEqual(symbol1, symbol2);
        }

        [Fact]
        public void MatchingRowShouldBeWinning()
        {
            var slotMachine = new SimplifiedSlotMachine((text) => { }, () => string.Empty);

            var row = new Symbol[]
            {
                new Symbol("Apple", 'A', 0.4m, 45),
                new Symbol("Apple", 'A', 0.4m, 45),
                new Symbol("Apple", 'A', 0.4m, 45)
            };

            Assert.True(slotMachine.RowIsWinning(row));
        }

        [Fact]
        public void MatchingRowWithWildcardShouldBeWinning()
        {
            var slotMachine = new SimplifiedSlotMachine((text) => { }, () => string.Empty);

            var row = new Symbol[]
            {
                new Symbol("Apple", 'A', 0.4m, 45),
                new Symbol("Apple", 'A', 0.4m, 45),
                new Symbol("Wildcard", '*', 0m, 5, isWildcard: true)
            };

            Assert.True(slotMachine.RowIsWinning(row));
        }

        [Fact]
        public void NonMatchingRowShouldBeLosing()
        {
            var slotMachine = new SimplifiedSlotMachine((text) => { }, () => string.Empty);

            var row = new Symbol[]
            {
                new Symbol("Apple", 'A', 0.4m, 45),
                new Symbol("Banana", 'B', 0.6m, 35),
                new Symbol("Apple", 'A', 0.4m, 45)
            };

            Assert.False(slotMachine.RowIsWinning(row));
        }

        [Fact]
        public void NonMatchingRowWithWildcardShouldBeLosing()
        {
            var slotMachine = new SimplifiedSlotMachine((text) => { }, () => string.Empty);

            var row = new Symbol[]
            {
                new Symbol("Apple", 'A', 0.4m, 45),
                new Symbol("Banana", 'B', 0.6m, 35),
                new Symbol("Wildcard", '*', 0m, 5, isWildcard: true)
            };

            Assert.False(slotMachine.RowIsWinning(row));
        }
        #endregion
    }
}