using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlotMachine.Core.Models
{
    public class Symbol
    {
        public Symbol(string name, string value, decimal coefficient, double probabilityPercent, bool isWildcard = false)
        {
            Name = name;
            Value = value;
            Coefficient = coefficient;
            ProbabilityPercent = probabilityPercent;
            IsWildcard = isWildcard;
        }

        public string Name { get; }
        public string Value { get; }
        public decimal Coefficient { get; }
        public double ProbabilityPercent { get; }
        public bool IsWildcard { get; }
    }
}
