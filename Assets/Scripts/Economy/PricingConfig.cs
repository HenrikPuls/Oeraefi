using UnityEngine;

namespace Oerfi.Economy
{
    /// <summary>
    /// Balancing data for the Kúgildi pricing engine (ADR-004; concept doc
    /// 14.4/14.6). Values are game-design decisions — only the user changes
    /// them, in the Inspector.
    /// </summary>
    [CreateAssetMenu(fileName = "PricingConfig", menuName = "Oerfi/Economy/Pricing Config")]
    public class PricingConfig : ScriptableObject
    {
        [SerializeField] private float _wageRate = 0.01f;
        [SerializeField] private float _ironMultiplier = 3f;
        [SerializeField] private float _woodMultiplier = 2f;

        /// <summary>Kúgildi value of one labor-hour at the multiplier-neutral baseline.</summary>
        public float WageRate => _wageRate;

        /// <summary>Multiplier for iron-containing goods (import scarcity, concept doc 14.6).</summary>
        public float IronMultiplier => _ironMultiplier;

        /// <summary>Multiplier for wooden goods (wood scarcity, concept doc 14.6).</summary>
        public float WoodMultiplier => _woodMultiplier;
    }
}
