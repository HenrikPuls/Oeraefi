using System;
using System.Collections.Generic;
using System.Linq;

namespace Oerfi.Economy
{
    /// <summary>
    /// Per-farm demand/production for one economy tick. Pure data container —
    /// the tuple lists are given in arbitrary order; consumption order is
    /// determined by the agent's priority list.
    /// </summary>
    public class EconomyTickInput
    {
        public IReadOnlyList<(Good good, float amount)> Demand { get; set; } =
            Array.Empty<(Good, float)>();

        public IReadOnlyList<(Good good, float amount)> Production { get; set; } =
            Array.Empty<(Good, float)>();

        public static EconomyTickInput Empty() => new EconomyTickInput();
    }

    /// <summary>
    /// One farm as an economy agent (concept doc 4.1): storage, spoilage,
    /// priority-ordered consumption, production. Pure C# state — driven by
    /// the caller's day loop (calendar wiring is the runtime milestone's job).
    /// Tick mechanics are delegated to an <see cref="IEconomyDetailLevel"/>
    /// strategy (two-tier simulation: detailed near-player, abstracted far).
    /// Serialization/embedding concerns (Unity save data) are handled by the
    /// save milestone (M7).
    /// </summary>
    [Serializable]
    public class FarmEconomyAgent
    {
        private readonly string _farmName;

        // Runtime state: keyed by Good asset reference. Deliberately not
        // Unity-serialized (dictionary); persistence is the save milestone's job.
        private readonly Dictionary<Good, float> _stock = new Dictionary<Good, float>();

        private List<Good> _priorities = new List<Good>();

        public FarmEconomyAgent(string farmName)
        {
            _farmName = farmName;
        }

        public string FarmName => _farmName;

        public IReadOnlyDictionary<Good, float> Stock => _stock;

        public float GetStock(Good good)
        {
            return _stock.TryGetValue(good, out float amount) ? amount : 0f;
        }

        public void SetStock(Good good, float amount)
        {
            _stock[good] = Math.Max(0f, amount);
        }

        public void AddStock(Good good, float amount)
        {
            SetStock(good, GetStock(good) + amount);
        }

        /// <summary>Total storage value in Kúgildi (Σ quantity × good value).</summary>
        public float TotalKugildiValue()
        {
            float sum = 0f;
            foreach (KeyValuePair<Good, float> entry in _stock)
                sum += entry.Key.KugildiValue * entry.Value;
            return sum;
        }

        /// <summary>
        /// Sets the consumption priority order. Goods not listed are consumed
        /// after all listed ones, in input order.
        /// </summary>
        public void SetPriorities(IEnumerable<Good> priorities)
        {
            _priorities = new List<Good>(priorities);
        }

        public IReadOnlyList<Good> GetPriorities()
        {
            return _priorities;
        }

        /// <summary>
        /// Runs one economy tick via the injected detail-level strategy:
        /// spoilage → priority-ordered consumption → production.
        /// </summary>
        public void Tick(IEconomyDetailLevel detailLevel, EconomyTickInput input)
        {
            if (detailLevel == null) throw new ArgumentNullException(nameof(detailLevel));
            detailLevel.ExecuteTick(this, input, ApplySpoilage, ConsumeByPriority, Produce);
        }

        private void ApplySpoilage()
        {
            List<Good> keys = new List<Good>(_stock.Keys);
            foreach (Good good in keys)
            {
                if (!good.Perishable) continue;
                _stock[good] = _stock[good] * (1f - Math.Clamp(good.SpoilageRatePerDay, 0f, 1f));
            }
        }

        private void ConsumeByPriority(IReadOnlyList<(Good good, float amount)> demand)
        {
            // Stable sort by priority position: listed goods consume first
            // (in priority order), unlisted ones after. An empty priority or a
            // partially served one stops all lower priorities (essential-first
            // rationing, kept exactly testable).
            List<(Good good, float amount)> ordered =
                demand.OrderBy(PriorityPosition).ToList();

            foreach ((Good good, float amount) in ordered)
            {
                float stock = GetStock(good);
                if (stock <= 0f) break;
                float consumed = Math.Min(stock, amount);
                SetStock(good, stock - consumed);
                if (consumed < amount) break;
            }
        }

        private int PriorityPosition((Good good, float amount) entry)
        {
            int index = _priorities.IndexOf(entry.good);
            return index >= 0 ? index : int.MaxValue;
        }

        private void Produce(IReadOnlyList<(Good good, float amount)> production)
        {
            foreach ((Good good, float amount) in production)
                AddStock(good, amount);
        }
    }
}
