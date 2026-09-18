using UnityEngine;

namespace Oerfi.Economy
{
    /// <summary>Functional group of a good (concept doc 9/10/14).</summary>
    public enum GoodCategory
    {
        Food,
        RawMaterial,
        CraftGood,
        ImportGood,
        ExportGood
    }

    /// <summary>
    /// A tradeable/stockable good definition (concept doc 14.6). Balancing
    /// values live on the asset instances (ADR-004) — the player-facing
    /// display names stay German by design (game content, not code).
    /// </summary>
    [CreateAssetMenu(fileName = "Good", menuName = "Oerfi/Good")]
    public class Good : ScriptableObject
    {
        [SerializeField] private string _id;
        [SerializeField] private string _displayName;
        [SerializeField] private GoodCategory _category = GoodCategory.Food;
        [SerializeField] private float _kugildiValue;
        [SerializeField] private bool _perishable;
        [SerializeField] private float _spoilageRatePerDay;

        /// <summary>Stable identifier; falls back to the asset name when unset.</summary>
        public string Id => string.IsNullOrEmpty(_id) ? name : _id;

        public string DisplayName => string.IsNullOrEmpty(_displayName) ? name : _displayName;

        public GoodCategory Category => _category;

        /// <summary>Baseline value in Kúgildi (1 Kúgildi = 1 milk cow, concept doc 14.1).</summary>
        public float KugildiValue => _kugildiValue;

        public bool Perishable => _perishable;

        /// <summary>Fraction of the stock that spoils per day (0 for non-perishables).</summary>
        public float SpoilageRatePerDay => _perishable ? _spoilageRatePerDay : 0f;

#if UNITY_EDITOR
        private void OnValidate()
        {
            // Keep the id synced with the asset name while an explicit id is unset.
            if (string.IsNullOrEmpty(_id)) _id = name.ToLowerInvariant().Replace(' ', '-');
        }
#endif

        /// <summary>
        /// Creates a Good instance at runtime/editor time without an asset file
        /// (tests, procedurally constructed scenarios). The instance is NOT
        /// persisted — do not save references to it or ship it in builds.
        /// </summary>
        public static Good CreateRuntimeOnly(
            string id,
            string displayName,
            GoodCategory category,
            float kugildiValue,
            bool perishable = false,
            float spoilageRatePerDay = 0f)
        {
            Good good = CreateInstance<Good>();
            good._id = id;
            good._displayName = displayName;
            good._category = category;
            good._kugildiValue = kugildiValue;
            good._perishable = perishable;
            good._spoilageRatePerDay = spoilageRatePerDay;
            good.name = id;
            return good;
        }
    }
}
