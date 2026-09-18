namespace Oerfi.Household
{
    using Oerfi.Characters;

    /// <summary>Labour status of a person on the farm (concept doc 4.5).</summary>
    public enum LabourStatus
    {
        /// <summary>Unfree; full supply obligation.</summary>
        Thrall = 0,
        /// <summary>Freed; stays as wage worker (Vinnufólk) instead of full supply.</summary>
        Vinnufolk = 1,
        /// <summary>Freed and left the farm (concept state; produced by the
        /// later narrative/decision layer, not by this service).</summary>
        Departed = 2
    }

    /// <summary>
    /// Manumission as an economic valve (concept doc 4.5): when a thrall's
    /// upkeep exceeds their work value, freeing them is the rational move —
    /// not merely a moral act. Also the mechanical consequence of the
    /// Freedman origin's legally-grounded ThingOratory zero (concept doc
    /// 11.6): manumission releases the skill to normal learning.
    /// </summary>
    public static class ManumissionService
    {
        /// <summary>ThingOratory value granted on manumission (the zero was
        /// legal, not a lack of experience).</summary>
        public const float NORMAL_THING_ORATORY = 1f;

        /// <summary>Thrall upkeep per day (Kúgildi-equivalent).</summary>
        public const float UPKEEP_PER_DAY = 0.01f;
        /// <summary>Work value scales with skill by this divisor.</summary>
        public const float WORK_SKILL_DIVISOR = 10f;
        /// <summary>Upkeep scales with skill by this divisor (twice as slow
        /// as work value: skilled thralls are never rational to free).</summary>
        public const float UPKEEP_SKILL_DIVISOR = 20f;

        /// <summary>
        /// True when daily upkeep exceeds the skill-scaled daily work value.
        /// </summary>
        public static bool IsManumissionRational(float workValuePerDay, float skillLevel)
        {
            float workOutput = workValuePerDay * (1f + skillLevel / WORK_SKILL_DIVISOR);
            float upkeep = UPKEEP_PER_DAY * (1f + skillLevel / UPKEEP_SKILL_DIVISOR);
            return upkeep > workOutput;
        }

        /// <summary>
        /// Frees a thrall: ThingOratory jumps to its normal value (legal
        /// capacity restored), learning-by-doing resumes from there.
        /// </summary>
        public static LabourStatus Manumit(Character character)
        {
            character.Skills.Set(SkillId.ThingOratory, NORMAL_THING_ORATORY);
            return LabourStatus.Vinnufolk;
        }
    }
}
