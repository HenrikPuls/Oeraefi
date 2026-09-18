using System;
using System.Collections.Generic;
using System.Linq;

namespace Oerfi.Economy
{
    /// <summary>
    /// Strategy for one economy tick (concept doc 4.1 two-tier simulation).
    /// Implementations receive delegate hooks that encapsulate the agent's
    /// mutations, so both tiers share identical mechanics and only differ in
    /// how many ticks they aggregate.
    /// </summary>
    public interface IEconomyDetailLevel
    {
        void ExecuteTick(
            FarmEconomyAgent agent,
            EconomyTickInput input,
            Action applySpoilage,
            Action<IReadOnlyList<(Good good, float amount)>> consumeByPriority,
            Action<IReadOnlyList<(Good good, float amount)>> produce);
    }

    /// <summary>Near-player tier: every day ticks verbatim (spoilage → demand → production).</summary>
    public class DetailedFarmSimulation : IEconomyDetailLevel
    {
        public void ExecuteTick(
            FarmEconomyAgent agent,
            EconomyTickInput input,
            Action applySpoilage,
            Action<IReadOnlyList<(Good good, float amount)>> consumeByPriority,
            Action<IReadOnlyList<(Good good, float amount)>> produce)
        {
            applySpoilage();
            consumeByPriority(input.Demand);
            produce(input.Production);
        }
    }

    /// <summary>
    /// Far-farm tier: one tick represents <see cref="_batchFactorDays"/> days.
    /// Demand and production are scaled by the factor; spoilage is applied
    /// once per batch (deliberately coarse but process-identical).
    /// </summary>
    public class AbstractedFarmSimulation : IEconomyDetailLevel
    {
        public const int MIN_BATCH_FACTOR_DAYS = 1;
        public const int DEFAULT_BATCH_FACTOR_DAYS = 7;

        private readonly int _batchFactorDays;

        public AbstractedFarmSimulation(int batchFactorDays = DEFAULT_BATCH_FACTOR_DAYS)
        {
            _batchFactorDays = Math.Max(MIN_BATCH_FACTOR_DAYS, batchFactorDays);
        }

        public void ExecuteTick(
            FarmEconomyAgent agent,
            EconomyTickInput input,
            Action applySpoilage,
            Action<IReadOnlyList<(Good good, float amount)>> consumeByPriority,
            Action<IReadOnlyList<(Good good, float amount)>> produce)
        {
            applySpoilage();
            consumeByPriority(Scale(input.Demand));
            produce(Scale(input.Production));
        }

        private (Good good, float amount)[] Scale(IReadOnlyList<(Good good, float amount)> entries)
        {
            var scaled = new (Good good, float amount)[entries.Count];
            for (int i = 0; i < entries.Count; i++)
                scaled[i] = (entries[i].good, entries[i].amount * _batchFactorDays);
            return scaled;
        }
    }

    /// <summary>
    /// Chooses the simulation tier by distance to the player's farm.
    /// Farms at or beyond the threshold run abstracted (shared instances —
    /// the strategy objects are stateless, so one of each suffices).
    /// </summary>
    public class EconomyLevelSelector
    {
        public const float DEFAULT_ABSTRACTION_DISTANCE_METERS = 5000f;

        private readonly float _abstractionDistanceMeters;
        private readonly DetailedFarmSimulation _detailed = new DetailedFarmSimulation();
        private readonly AbstractedFarmSimulation _abstracted = new AbstractedFarmSimulation();

        public EconomyLevelSelector(float abstractionDistanceMeters = DEFAULT_ABSTRACTION_DISTANCE_METERS)
        {
            _abstractionDistanceMeters = abstractionDistanceMeters;
        }

        public IEconomyDetailLevel Select(float distanceToPlayerMeters)
        {
            return distanceToPlayerMeters >= _abstractionDistanceMeters
                ? _abstracted
                : _detailed;
        }
    }
}
