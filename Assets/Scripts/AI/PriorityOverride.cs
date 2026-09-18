using System;

namespace Oerfi.AI
{
    /// <summary>
    /// Player intervention seam (concept doc 4.5/10): the player changes
    /// priorities, not individual actions — reassigning a role or parking a
    /// worker is the entire control surface. Yield already banked and skill
    /// progress carry over; nothing is rolled back.
    /// </summary>
    public static class PriorityOverride
    {
        /// <summary>Assigns a new role to the worker.</summary>
        public static void Reassign(AutonomousWorker worker, TaskRole newRole)
        {
            if (worker == null) throw new ArgumentNullException(nameof(worker));
            worker.SetRole(newRole);
        }

        /// <summary>Activates or deactivates the worker (deactivated = full no-op).</summary>
        public static void SetActive(AutonomousWorker worker, bool active)
        {
            if (worker == null) throw new ArgumentNullException(nameof(worker));
            worker.IsActive = active;
        }
    }
}
