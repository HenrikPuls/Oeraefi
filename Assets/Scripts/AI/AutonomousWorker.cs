using System;

namespace Oerfi.AI
{
    /// <summary>
    /// One autonomous worker (thrall or free household member, concept doc
    /// 4.5): assigned a <see cref="TaskRole"/>, produces a base yield scaled
    /// by the role skill, learns by doing, and couples overload to the
    /// household's hidden unrest. Plain C# state — the caller's day loop
    /// invokes <see cref="Tick(int, Oerfi.Economy.Good, bool)"/> with the
    /// calendar month index; scene/runtime wiring is M5.
    ///
    /// Migration notes (predecessor fixes): the duplicated Tick/TickMitOutput
    /// pair is unified into one method that takes the output good (the
    /// predecessor's internal good lookup always returned null, so its Tick
    /// never fed the farm stock); the dead ThrallSupplyNeed field is dropped
    /// (supply needs are static functions with the season as parameter); the
    /// four late roles are explicitly mapped instead of silently falling to
    /// defaults.
    /// </summary>
    public class AutonomousWorker
    {
        // --- Unrest coupling (predecessor values, adopted as named constants) ---
        /// <summary>Unrest gained per tick while overloaded.</summary>
        public const float UNREST_OVERLOAD_DELTA_PER_TICK = 0.5f;
        /// <summary>Passive unrest recovery per non-overloaded tick.</summary>
        public const float UNREST_RECOVERY_PER_TICK = 0.1f;
        /// <summary>Toughness below this plus heavy work means overload.</summary>
        public const int OVERLOAD_TOUGHNESS_THRESHOLD = 7;
        /// <summary>Raw experience gained per active day (learning by doing).</summary>
        public const float EXPERIENCE_PER_DAY = 1f;

        // --- Base daily yields per role (balancing start values; predecessor
        //     values; SO-ification candidate, M13 two-tier-params precedent). ---
        public const float YIELD_FISHING = 2f;
        public const float YIELD_FOWLING = 1.5f;
        public const float YIELD_FARMING = 1.2f;
        public const float YIELD_PEAT_CUTTING = 2f;
        public const float YIELD_WOOL_AND_WEAVING = 0.5f;
        public const float YIELD_QUARRYING = 1f;
        public const float YIELD_WOOD_CONSTRUCTION = 0.8f;
        public const float YIELD_CONSTRUCTION = 1f;
        public const float YIELD_DEFAULT = 1f;
        public const float YIELD_IDLE = 0f;

        private readonly Oerfi.Characters.Character _character;
        private readonly Oerfi.Economy.FarmEconomyAgent _targetAgent;
        private readonly Oerfi.Household.UnrestSystem _unrest;

        private TaskRole _role;
        private int _overloadTicks;

        public AutonomousWorker(
            Oerfi.Characters.Character character,
            TaskRole role,
            Oerfi.Economy.FarmEconomyAgent targetAgent,
            Oerfi.Household.UnrestSystem unrest = null)
        {
            _character = character ?? throw new ArgumentNullException(nameof(character));
            _targetAgent = targetAgent;
            _unrest = unrest;
            _role = role;
        }

        public Oerfi.Characters.Character Character => _character;

        public TaskRole Role => _role;

        /// <summary>
        /// Assigns a new role. Accrued overload ticks carry over (the
        /// worker's condition persists across reassignment).
        /// </summary>
        public void SetRole(TaskRole newRole) => _role = newRole;

        /// <summary>Inactive workers skip the tick entirely (no yield, no
        /// experience, no unrest recovery — e.g. winter quarters).</summary>
        public bool IsActive { get; set; } = true;

        /// <summary>Consecutive overload ticks; decays on recovery ticks.</summary>
        public int OverloadTicks => _overloadTicks;

        /// <summary>
        /// The skill trained by the role. Documented predecessor quirks kept:
        /// PeatCutting trains Farming, Cooking trains Leatherworking (no
        /// cooking skill exists — domestic work is lumped with handicraft).
        /// Idle trains nothing.
        /// </summary>
        public static Oerfi.Characters.SkillId MapRoleToSkill(TaskRole role)
        {
            switch (role)
            {
                case TaskRole.WoolProcessing:
                case TaskRole.Weaving:
                    return Oerfi.Characters.SkillId.Weaving;
                case TaskRole.Fishing:
                    return Oerfi.Characters.SkillId.Fishing;
                case TaskRole.PeatCutting:
                case TaskRole.Farming:
                    return Oerfi.Characters.SkillId.Farming;
                case TaskRole.Fowling:
                    return Oerfi.Characters.SkillId.Fowling;
                case TaskRole.SealHunting:
                    return Oerfi.Characters.SkillId.SealHunting;
                case TaskRole.Quarrying:
                    return Oerfi.Characters.SkillId.Stonemasonry;
                case TaskRole.WoodConstruction:
                case TaskRole.Construction:
                case TaskRole.Carpenter:
                    return Oerfi.Characters.SkillId.Woodworking;
                case TaskRole.Smithing:
                    return Oerfi.Characters.SkillId.Smithing;
                case TaskRole.Coopering:
                    return Oerfi.Characters.SkillId.Coopering;
                case TaskRole.Leatherworking:
                case TaskRole.Cooking:
                    return Oerfi.Characters.SkillId.Leatherworking;
                case TaskRole.Trading:
                    return Oerfi.Characters.SkillId.Trading;
                case TaskRole.Stonemason:
                    return Oerfi.Characters.SkillId.Stonemasonry;
                case TaskRole.Idle:
                    // Idle trains nothing; callers must not pass it here.
                    throw new ArgumentOutOfRangeException(nameof(role), role,
                        "Idle trains no skill.");
                default:
                    throw new ArgumentOutOfRangeException(nameof(role), role, null);
            }
        }

        /// <summary>The skill trained by this worker's current role.</summary>
        public Oerfi.Characters.SkillId SkillForRole => MapRoleToSkill(_role);

        /// <summary>Base yield in supply-equivalent units per day, before skill scaling.</summary>
        public static float BaseYield(TaskRole role)
        {
            switch (role)
            {
                case TaskRole.Fishing:
                    return YIELD_FISHING;
                case TaskRole.Fowling:
                    return YIELD_FOWLING;
                case TaskRole.Farming:
                    return YIELD_FARMING;
                case TaskRole.PeatCutting:
                    return YIELD_PEAT_CUTTING;
                case TaskRole.WoolProcessing:
                case TaskRole.Weaving:
                    return YIELD_WOOL_AND_WEAVING;
                case TaskRole.Quarrying:
                    return YIELD_QUARRYING;
                case TaskRole.WoodConstruction:
                case TaskRole.Carpenter:
                    return YIELD_WOOD_CONSTRUCTION;
                case TaskRole.Construction:
                    return YIELD_CONSTRUCTION;
                case TaskRole.Idle:
                    return YIELD_IDLE;
                default:
                    // Smithing, Coopering, Leatherworking, Cooking, Trading, Stonemason.
                    return YIELD_DEFAULT;
            }
        }

        /// <summary>
        /// Pure yield computation: 0 outside the role's seasonal window,
        /// otherwise base yield scaled by skill: base × (1 + skill/10).
        /// </summary>
        public static float DailyYield(TaskRole role, float skillValue, int currentMonthIndex)
        {
            if (TaskRoleDefinitions.TryGetWindow(role, out Oerfi.AI.SeasonalWindow window)
                && !window.IsAvailable(currentMonthIndex))
                return 0f;

            return BaseYield(role) * (1f + skillValue / Oerfi.Characters.SkillSystem.MAX_SKILL);
        }

        /// <summary>Instance shortcut for the worker's current role and skill.</summary>
        public float ComputeDailyYield(int currentMonthIndex)
        {
            if (_role == TaskRole.Idle) return YIELD_IDLE;
            return DailyYield(_role, _character.Skills.Get(SkillForRole), currentMonthIndex);
        }

        /// <summary>
        /// One day tick. Feeds the computed yield into the farm stock when an
        /// output good and a target agent are supplied, grants experience
        /// (any active non-Idle role, also out of season — the worker still
        /// practices; predecessor behaviour, kept), and resolves overload:
        /// heavy work below the toughness threshold raises unrest, otherwise
        /// unrest recovers passively.
        /// </summary>
        /// <param name="currentMonthIndex">0-based calendar month (0 = Harpa).</param>
        /// <param name="outputGood">
        /// The good to add the yield as; null skips the stock feed (caller
        /// owns the role→good mapping until the M5/M6 data tables).
        /// </param>
        /// <param name="heavyWork">
        /// Caller-supplied heavy-labour flag (per-role classification is
        /// M5/M6 wiring); overload only applies to non-Idle roles.
        /// </param>
        public void Tick(int currentMonthIndex, Oerfi.Economy.Good outputGood, bool heavyWork = false)
        {
            if (!IsActive) return;

            if (_role != TaskRole.Idle)
            {
                float yield = ComputeDailyYield(currentMonthIndex);
                if (yield > 0f && outputGood != null && _targetAgent != null)
                    _targetAgent.AddStock(outputGood, yield);

                _character.Skills.GainExperience(SkillForRole, EXPERIENCE_PER_DAY);
            }

            bool overloaded = heavyWork
                && _role != TaskRole.Idle
                && _character.Attributes.Get(Oerfi.Characters.AttributeId.Toughness)
                    < OVERLOAD_TOUGHNESS_THRESHOLD;

            if (overloaded)
            {
                _overloadTicks++;
                _unrest?.ApplyDelta(UNREST_OVERLOAD_DELTA_PER_TICK);
            }
            else if (_unrest != null && _unrest.RawValue > 0f)
            {
                _unrest.ApplyDelta(-UNREST_RECOVERY_PER_TICK);
                _overloadTicks = Math.Max(0, _overloadTicks - 1);
            }
        }
    }
}
