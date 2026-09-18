using System;
using Oerfi.Core;

namespace Oerfi.Terrain
{
    /// <summary>
    /// Calendar adapter: advances forest regeneration once per simulated
    /// day. Plain class implementing <see cref="ITickable"/> — register it
    /// with the <see cref="CalendarModel"/>; runtime scene wiring is M5.
    ///
    /// Migration note: the predecessor implemented this as a MonoBehaviour
    /// that never registered itself with the calendar singleton — the
    /// adapter existed but was never driven. With the pure-C# calendar the
    /// MonoBehaviour wrapper is unnecessary.
    /// </summary>
    public sealed class ForestRegenerationTick : ITickable
    {
        private readonly TerrainResourceLayer _resourceLayer;

        public ForestRegenerationTick(TerrainResourceLayer resourceLayer)
        {
            _resourceLayer = resourceLayer
                ?? throw new ArgumentNullException(nameof(resourceLayer));
            ResourceLayer = resourceLayer;
        }

        public TerrainResourceLayer ResourceLayer { get; }

        public void OnDayTick(int dayIndex) => _resourceLayer.TickForestRegeneration();

        /// <summary>
        /// Forest regenerates season-independently for now; seasonal growth
        /// rates are an open concept question (no-op by design).
        /// </summary>
        public void OnSeasonChange(Season newSeason)
        {
        }
    }
}
