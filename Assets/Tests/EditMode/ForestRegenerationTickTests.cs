using NUnit.Framework;
using Oerfi.Core;
using Oerfi.Terrain;
using UnityEngine;

namespace Oerfi.Tests.EditMode
{
    /// <summary>
    /// Forest regeneration tick tests: the ITickable wiring (predecessor
    /// bug — its adapter was never registered with the calendar) and the
    /// constructor guard.
    /// </summary>
    public class ForestRegenerationTickTests
    {
        [Test]
        public void OnDayTick_ViaCalendar_AdvancesRegeneration()
        {
            TerrainResourceLayer layer =
                ScriptableObject.CreateInstance<TerrainResourceLayer>();
            try
            {
                layer.Initialize(2, 2, 50f);
                var serialized = new UnityEditor.SerializedObject(layer);
                serialized.FindProperty("_forestRegenerationRatePerDay").floatValue = 0.01f;
                serialized.ApplyModifiedPropertiesWithoutUndo();

                var calendar = new CalendarModel();
                calendar.Register(new ForestRegenerationTick(layer));

                float before = layer.GetForestDensity(0, 0);
                calendar.AdvanceDay();
                calendar.AdvanceDay();
                calendar.AdvanceDay();

                Assert.That(layer.GetForestDensity(0, 0),
                    Is.EqualTo(before + 3 * 0.01f).Within(0.0001f),
                    "three calendar days advance regeneration by three rates");
            }
            finally
            {
                Object.DestroyImmediate(layer);
            }
        }

        [Test]
        public void Constructor_NullLayer_Throws()
        {
            Assert.Throws<System.ArgumentNullException>(
                () => new ForestRegenerationTick(null));
        }
    }
}
