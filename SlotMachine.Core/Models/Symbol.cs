using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlotMachine.Core.Models
{
    internal class Symbol
    {
        public Symbol(string name, char displayValue, decimal coefficient, double probabilityPercent, bool isWildcard = false)
        {
            Name = name;
            Value = displayValue;
            Coefficient = coefficient;
            ProbabilityPercent = probabilityPercent;
            IsWildcard = isWildcard;
        }

        public string Name { get; }
        public char Value { get; }
        public decimal Coefficient { get; }
        public double ProbabilityPercent { get; }
        public bool IsWildcard { get; }
    }
}
