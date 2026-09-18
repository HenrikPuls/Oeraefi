using System;

namespace Oerfi.Characters
{
    /// <summary>
    /// Injectable randomness. Convention: BOTH bounds are inclusive
    /// (Next(1, 6) returns 1..6) — deliberately unlike System.Random.
    /// Documented here because the off-by-one direction is a migration trap.
    /// </summary>
    public interface IRandomSource
    {
        int Next(int minInclusive, int maxInclusive);
    }

    /// <summary>Production randomness wrapping System.Random (seedable).</summary>
    public class RandomSource : IRandomSource
    {
        private readonly Random _random;

        public RandomSource() : this(Environment.TickCount) { }

        public RandomSource(int seed) => _random = new Random(seed);

        public int Next(int minInclusive, int maxInclusive)
            => _random.Next(minInclusive, maxInclusive + 1);
    }

    /// <summary>
    /// Scripted randomness for tests: values repeat cyclically, clamped into
    /// the requested range so a scripted value can never leak outside a die
    /// bound by accident.
    /// </summary>
    public class FixedRandomSource : IRandomSource
    {
        private readonly int[] _values;
        private int _index;

        public FixedRandomSource(params int[] values)
        {
            if (values == null || values.Length == 0)
                throw new ArgumentException("FixedRandomSource requires at least one value.");
            _values = values;
        }

        public int Next(int minInclusive, int maxInclusive)
        {
            int value = _values[_index % _values.Length];
            _index++;
            if (value < minInclusive) value = minInclusive;
            if (value > maxInclusive) value = maxInclusive;
            return value;
        }
    }

    /// <summary>
    /// 2d10 check resolution (concept doc 11.4): roll = sum of two d10
    /// (bell curve around 11), target = attribute + skill; roll &lt;= target
    /// succeeds (low rolls are good, equal counts as success). Deliberately
    /// no difficulty modifiers or criticals — the concept doc defines none
    /// (revisit at M14 when combat lands).
    /// </summary>
    public class CheckResolver
    {
        public enum Result
        {
            Success,
            Failure
        }

        private readonly IRandomSource _random;

        public CheckResolver(IRandomSource random)
        {
            _random = random ?? throw new ArgumentNullException(nameof(random));
        }

        public (Result result, int roll, int targetValue) Resolve(int attributeValue, int skillLevel)
        {
            int targetValue = attributeValue + skillLevel;
            int roll = Roll2d10();
            return (roll <= targetValue ? Result.Success : Result.Failure, roll, targetValue);
        }

        // Two independent d10: inclusive bounds 1..10 each.
        private int Roll2d10() => _random.Next(1, 10) + _random.Next(1, 10);
    }
}
